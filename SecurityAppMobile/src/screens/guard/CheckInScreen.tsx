import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Image,
  Alert,
  ActivityIndicator,
  TextInput,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as Location from 'expo-location';
import * as ImagePicker from 'expo-image-picker';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Button from '../../components/common/Button';
import Card from '../../components/common/Card';
import { SafeAreaView } from 'react-native-safe-area-context';
import { CheckInScreenProps } from '../../types/navigation';
import { attendanceService, CheckInRequest } from '../../services/attendanceService';
import { authService, StoredUserData } from '../../services/authService';
import { deploymentService } from '../../services/deploymentService';
import { siteService } from '../../services/siteService';
import { getDistanceMeters, formatDistanceMeters } from '../../utils/geoUtils';

const CheckInScreen: React.FC<CheckInScreenProps> = ({ navigation }) => {
  const [location, setLocation] = useState<Location.LocationObject | null>(null);
  const [photo, setPhoto] = useState<string | null>(null);
  const [notes, setNotes] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [gettingLocation, setGettingLocation] = useState<boolean>(true);
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [userData, setUserData] = useState<StoredUserData | null>(null);

  useEffect(() => {
    initializeCheckIn();
  }, []);

  const initializeCheckIn = async (): Promise<void> => {
    // Get user data
    const storedUser = await authService.getStoredUser();
    if (storedUser) {
      setUserData(storedUser);
    } else {
      Alert.alert('Error', 'User data not found. Please login again.');
      navigation.goBack();
      return;
    }

    // Get location
    getLocation();
  };

  const getLocation = async (): Promise<void> => {
    setGettingLocation(true);
    try {
      let { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permission Denied', 'Location permission is required for check-in.');
        setGettingLocation(false);
        return;
      }
      let loc = await Location.getCurrentPositionAsync({});
      setLocation(loc);
    } catch (error) {
      Alert.alert('Error', 'Failed to get location');
    }
    setGettingLocation(false);
  };

  const takePhoto = async (): Promise<void> => {
    try {
      const { status } = await ImagePicker.requestCameraPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permission Denied', 'Camera permission is required for check-in.');
        return;
      }
      
      let result = await ImagePicker.launchCameraAsync({
        mediaTypes: ['images'],
        allowsEditing: true,
        aspect: [1, 1],
        quality: 0.8,
      });

      if (!result.canceled) {
        setPhoto(result.assets[0].uri);
      }
    } catch (error) {
      Alert.alert('Error', 'Failed to take photo');
    }
  };

  const handleCheckIn = async (): Promise<void> => {
    if (!location) {
      Alert.alert('Error', 'Location is required for check-in');
      return;
    }
    if (!photo) {
      Alert.alert('Error', 'Photo is required for check-in');
      return;
    }
    if (!userData) {
      Alert.alert('Error', 'User data not found');
      return;
    }

    setSubmitting(true);

    try {
      // Convert photo to base64
      const photoBase64 = await attendanceService.convertImageToBase64(photo);
      const photoFileName = `checkin_${Date.now()}.jpg`;

      // Get current deployment for site and shift (use guardId from login, not user id)
      const guardIdForApi = userData.guardId || userData.id;
      if (!guardIdForApi) {
        Alert.alert('Error', 'Guard profile not linked. Please contact your supervisor.');
        setSubmitting(false);
        return;
      }

      const deploymentsResult = await deploymentService.getDeployments({
        guardId: guardIdForApi,
        pageSize: 10,
        skipCache: true,
      });

      let siteId = '';
      let shiftId = '';

      if (deploymentsResult.success && deploymentsResult.data != null) {
        const raw = deploymentsResult.data as any;
        const deployments: any[] = Array.isArray(raw)
          ? raw
          : Array.isArray(raw?.items)
            ? raw.items
            : Array.isArray(raw?.data)
              ? raw.data
              : Array.isArray(raw?.Items)
                ? raw.Items
                : Array.isArray(raw?.Data)
                  ? raw.Data
                  : [];
        if (deployments.length > 0) {
          const first = deployments[0];
          siteId = (first.siteId ?? first.SiteId ?? '') as string;
          shiftId = (first.shiftId ?? first.ShiftId ?? '') as string;
        }
      }

      if (!siteId || !shiftId) {
        Alert.alert('Error', 'No active assignment found for this guard. Please contact your supervisor.');
        setSubmitting(false);
        return;
      }

      // Geofence: only allow check-in within allocated site
      const siteResult = await siteService.getSiteById(siteId);
      if (siteResult.success && siteResult.data?.latitude != null && siteResult.data?.longitude != null) {
        const radiusMeters = siteResult.data.geofenceRadiusMeters ?? 100;
        const distanceMeters = getDistanceMeters(
          siteResult.data.latitude,
          siteResult.data.longitude,
          location.coords.latitude,
          location.coords.longitude
        );
        if (distanceMeters > radiusMeters) {
          const distanceText = formatDistanceMeters(distanceMeters);
          Alert.alert(
            'Location Not Allowed',
            `Check-in is only allowed at the allocated site. You are ${distanceText} away from the site (allowed: ${radiusMeters} m). Please reach the site to check in.`
          );
          setSubmitting(false);
          return;
        }
      } else if (siteResult.success && (siteResult.data?.latitude == null || siteResult.data?.longitude == null)) {
        Alert.alert('Error', 'Site location is not configured. Please contact your supervisor.');
        setSubmitting(false);
        return;
      }

      // Create check-in data (guardId = SecurityGuard.Id; checkInTime = mobile current time)
      const checkInData: CheckInRequest = {
        guardId: guardIdForApi,
        siteId,
        shiftId,
        latitude: location.coords.latitude,
        longitude: location.coords.longitude,
        checkInTime: new Date().toISOString(),
        photoBase64,
        photoFileName,
        notes: notes.trim(),
        agencyId: userData.agencyId,
      };

      console.log('Submitting check-in data:', checkInData);

      const response = await attendanceService.checkIn(checkInData);

      if (response.success) {
        Alert.alert(
          'Success',
          response.message || 'You have successfully checked in!',
          [{ text: 'OK', onPress: () => navigation.goBack() }]
        );
      } else {
        Alert.alert(
          'Check-in Failed',
          response.error?.message || 'Please try again.'
        );
      }
    } catch (error) {
      console.error('Check-in error:', error);
      Alert.alert('Error', 'Failed to process check-in. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  const getCurrentTimeString = (): string => {
    return new Date().toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit',
      hour12: true 
    });
  };

  const getCurrentDateString = (): string => {
    return new Date().toLocaleDateString('en-US', { 
      weekday: 'long',
      day: 'numeric',
      month: 'long',
      year: 'numeric'
    });
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <LinearGradient
        colors={[COLORS.primary, COLORS.primaryLight]}
        style={styles.header}
      >
        <TouchableOpacity
          style={styles.backButton}
          onPress={() => navigation.goBack()}
        >
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Check In</Text>
        <View style={styles.placeholder} />
      </LinearGradient>

      <View style={styles.content}>
        {/* User Profile Card */}
        {userData && (
          <Card style={styles.profileCard}>
            <View style={styles.profileAvatar}>
              <MaterialCommunityIcons name="account" size={32} color={COLORS.primary} />
            </View>
            <View style={styles.profileInfo}>
              <Text style={styles.profileName}>{userData.username}</Text>
              <Text style={styles.profileRole}>{userData.role}</Text>
              <Text style={styles.profileEmail}>{userData.email}</Text>
            </View>
          </Card>
        )}

        {/* GPS Location */}
        <Card style={styles.locationCard}>
          <View style={styles.cardHeader}>
            <View style={styles.cardIcon}>
              <MaterialCommunityIcons name="map-marker" size={24} color={COLORS.success} />
            </View>
            <View style={styles.cardHeaderText}>
              <Text style={styles.cardTitle}>GPS Location</Text>
              <Text style={styles.cardSubtitle}>
                {gettingLocation ? 'Getting location...' : 
                 location ? 'Location captured' : 'Location not available'}
              </Text>
            </View>
            {gettingLocation ? (
              <ActivityIndicator color={COLORS.primary} />
            ) : location ? (
              <MaterialCommunityIcons name="check-circle" size={28} color={COLORS.success} />
            ) : (
              <TouchableOpacity onPress={getLocation}>
                <MaterialCommunityIcons name="refresh" size={24} color={COLORS.primary} />
              </TouchableOpacity>
            )}
          </View>
          {location && (
            <View style={styles.locationDetails}>
              <Text style={styles.coordLabel}>Coordinates:</Text>
              <Text style={styles.coordValue}>
                {location.coords.latitude.toFixed(6)}, {location.coords.longitude.toFixed(6)}
              </Text>
            </View>
          )}
        </Card>

        {/* Photo Capture */}
        <Card style={styles.photoCard}>
          <View style={styles.cardHeader}>
            <View style={styles.cardIcon}>
              <MaterialCommunityIcons name="camera" size={24} color={COLORS.info} />
            </View>
            <View style={styles.cardHeaderText}>
              <Text style={styles.cardTitle}>Selfie Photo</Text>
              <Text style={styles.cardSubtitle}>
                {photo ? 'Photo captured' : 'Take a selfie for verification'}
              </Text>
            </View>
            {photo && (
              <MaterialCommunityIcons name="check-circle" size={28} color={COLORS.success} />
            )}
          </View>
          
          {photo ? (
            <View style={styles.photoPreview}>
              <Image source={{ uri: photo }} style={styles.previewImage} />
              <TouchableOpacity style={styles.retakeButton} onPress={takePhoto}>
                <MaterialCommunityIcons name="camera" size={20} color={COLORS.white} />
                <Text style={styles.retakeText}>Retake</Text>
              </TouchableOpacity>
            </View>
          ) : (
            <TouchableOpacity style={styles.captureButton} onPress={takePhoto}>
              <MaterialCommunityIcons name="camera" size={32} color={COLORS.primary} />
              <Text style={styles.captureText}>Tap to take photo</Text>
            </TouchableOpacity>
          )}
        </Card>

        {/* Notes Input */}
        <Card style={styles.notesCard}>
          <View style={styles.cardHeader}>
            <View style={styles.cardIcon}>
              <MaterialCommunityIcons name="note-text" size={24} color={COLORS.warning} />
            </View>
            <View style={styles.cardHeaderText}>
              <Text style={styles.cardTitle}>Notes (Optional)</Text>
              <Text style={styles.cardSubtitle}>Add any additional information</Text>
            </View>
          </View>
          <TextInput
            style={styles.notesInput}
            placeholder="Enter any notes..."
            value={notes}
            onChangeText={setNotes}
            multiline
            numberOfLines={3}
            textAlignVertical="top"
          />
        </Card>

        {/* Time Display */}
        <Card style={styles.timeCard}>
          <MaterialCommunityIcons name="clock" size={24} color={COLORS.primary} />
          <View style={styles.timeInfo}>
            <Text style={styles.timeLabel}>Check-in Time</Text>
            <Text style={styles.timeValue}>{getCurrentTimeString()}</Text>
          </View>
          <Text style={styles.dateText}>{getCurrentDateString()}</Text>
        </Card>
      </View>

      {/* Submit Button */}
      <View style={styles.footer}>
        <Button
          title="Confirm Check In"
          onPress={handleCheckIn}
          loading={submitting}
          disabled={!location || !photo}
        />
      </View>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: SIZES.md,
    paddingVertical: SIZES.md,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: 20,
    justifyContent: 'center',
    alignItems: 'center',
  },
  headerTitle: {
    fontSize: FONTS.h4,
    fontWeight: '600',
    color: COLORS.white,
  },
  placeholder: {
    width: 40,
  },
  content: {
    flex: 1,
    padding: SIZES.md,
  },
  profileCard: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: SIZES.md,
    marginBottom: SIZES.md,
  },
  profileAvatar: {
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: COLORS.primary + '15',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: SIZES.md,
  },
  profileInfo: {
    flex: 1,
  },
  profileName: {
    fontSize: FONTS.h5,
    fontWeight: 'bold',
    color: COLORS.textPrimary,
  },
  profileRole: {
    fontSize: FONTS.bodySmall,
    color: COLORS.primary,
    fontWeight: '500',
    marginTop: 2,
  },
  profileEmail: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  locationCard: {
    padding: SIZES.md,
    marginBottom: SIZES.md,
  },
  photoCard: {
    padding: SIZES.md,
    marginBottom: SIZES.md,
  },
  notesCard: {
    padding: SIZES.md,
    marginBottom: SIZES.md,
  },
  cardHeader: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  cardIcon: {
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: COLORS.gray100,
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: SIZES.sm,
  },
  cardHeaderText: {
    flex: 1,
  },
  cardTitle: {
    fontSize: FONTS.body,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  cardSubtitle: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  locationDetails: {
    marginTop: SIZES.md,
    paddingTop: SIZES.md,
    borderTopWidth: 1,
    borderTopColor: COLORS.gray200,
  },
  coordLabel: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
  },
  coordValue: {
    fontSize: FONTS.bodySmall,
    color: COLORS.textPrimary,
    fontWeight: '500',
    marginTop: 2,
  },
  captureButton: {
    marginTop: SIZES.md,
    height: 120,
    borderRadius: SIZES.radiusMd,
    borderWidth: 2,
    borderStyle: 'dashed',
    borderColor: COLORS.gray300,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: COLORS.gray100,
  },
  captureText: {
    fontSize: FONTS.bodySmall,
    color: COLORS.textSecondary,
    marginTop: SIZES.sm,
  },
  photoPreview: {
    marginTop: SIZES.md,
    alignItems: 'center',
  },
  previewImage: {
    width: 150,
    height: 150,
    borderRadius: 75,
    borderWidth: 4,
    borderColor: COLORS.primary,
  },
  retakeButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.primary,
    paddingHorizontal: SIZES.md,
    paddingVertical: SIZES.sm,
    borderRadius: SIZES.radiusSm,
    marginTop: SIZES.md,
    gap: SIZES.xs,
  },
  retakeText: {
    fontSize: FONTS.bodySmall,
    color: COLORS.white,
    fontWeight: '500',
  },
  notesInput: {
    marginTop: SIZES.md,
    padding: SIZES.sm,
    borderWidth: 1,
    borderColor: COLORS.gray300,
    borderRadius: SIZES.radiusSm,
    fontSize: FONTS.body,
    color: COLORS.textPrimary,
    backgroundColor: COLORS.white,
  },
  timeCard: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: SIZES.md,
  },
  timeInfo: {
    flex: 1,
    marginLeft: SIZES.md,
  },
  timeLabel: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
  },
  timeValue: {
    fontSize: FONTS.h3,
    fontWeight: 'bold',
    color: COLORS.textPrimary,
  },
  dateText: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
  },
  footer: {
    padding: SIZES.md,
    backgroundColor: COLORS.white,
    ...SHADOWS.medium,
  },
});

export default CheckInScreen;
