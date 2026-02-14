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
import { CheckOutScreenProps } from '../../types/navigation';
import { attendanceService } from '../../services/attendanceService';
import { authService, StoredUserData } from '../../services/authService';
import { getDistanceMeters, formatDistanceMeters } from '../../utils/geoUtils';
import { formatTimeIST } from '../../utils/dateUtils';

const CheckOutScreen: React.FC<CheckOutScreenProps> = ({ navigation }) => {
  const [location, setLocation] = useState<Location.LocationObject | null>(null);
  const [photo, setPhoto] = useState<string | null>(null);
  const [notes, setNotes] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(true);
  const [gettingLocation, setGettingLocation] = useState<boolean>(true);
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [userData, setUserData] = useState<StoredUserData | null>(null);
  const [attendanceId, setAttendanceId] = useState<string>('');
  const [checkInTime, setCheckInTime] = useState<string>('');
  const [siteName, setSiteName] = useState<string>('');
  const [siteLatitude, setSiteLatitude] = useState<number | null>(null);
  const [siteLongitude, setSiteLongitude] = useState<number | null>(null);
  const [siteGeofenceRadiusMeters, setSiteGeofenceRadiusMeters] = useState<number>(100);

  useEffect(() => {
    initializeCheckOut();
  }, []);

  const initializeCheckOut = async (): Promise<void> => {
    // Get user data
    const storedUser = await authService.getStoredUser();
    if (storedUser) {
      setUserData(storedUser);
    } else {
      Alert.alert('Error', 'User data not found. Please login again.');
      navigation.goBack();
      return;
    }

    // Get current attendance (use guardId so API returns this guard's attendance)
    const guardIdForApi = (storedUser as { guardId?: string }).guardId || storedUser.id;
    await loadCurrentAttendance(guardIdForApi);

    // Get location
    getLocation();
  };

  const loadCurrentAttendance = async (guardId: string): Promise<void> => {
    try {
      setLoading(true);
      const today = new Date().toISOString().split('T')[0];
      const attendanceResult = await attendanceService.getGuardAttendance(guardId, today);
      
      if (attendanceResult.success && attendanceResult.data != null) {
        const raw = attendanceResult.data as any;
        const attendances = Array.isArray(raw)
          ? raw
          : Array.isArray(raw?.items) ? raw.items : Array.isArray(raw?.data) ? raw.data : [raw];
        
        // Find the attendance that is checked in but not checked out
        const currentAttendance = attendances.find((att: any) => {
          const hasCheckIn = att.checkInTime ?? att.CheckInTime;
          const hasCheckOut = att.checkOutTime ?? att.CheckOutTime;
          return hasCheckIn && !hasCheckOut;
        });

        if (currentAttendance) {
          const attId = currentAttendance.id ?? currentAttendance.Id ?? '';
          setAttendanceId(String(attId));
          
          const checkInRaw = currentAttendance.checkInTime ?? currentAttendance.CheckInTime;
          if (checkInRaw) {
            setCheckInTime(formatTimeIST(checkInRaw));
          }

          if (currentAttendance.siteName ?? currentAttendance.SiteName) {
            setSiteName(currentAttendance.siteName ?? currentAttendance.SiteName ?? '');
          }

          const lat = currentAttendance.siteLatitude ?? currentAttendance.SiteLatitude;
          const lng = currentAttendance.siteLongitude ?? currentAttendance.SiteLongitude;
          if (lat != null && lng != null) {
            setSiteLatitude(lat);
            setSiteLongitude(lng);
          }
          const radius = currentAttendance.siteGeofenceRadiusMeters ?? currentAttendance.SiteGeofenceRadiusMeters;
          if (radius != null) {
            setSiteGeofenceRadiusMeters(radius);
          }
        } else {
          Alert.alert(
            'No Active Check-in',
            'You are not currently checked in. Please check in first.',
            [{ text: 'OK', onPress: () => navigation.goBack() }]
          );
        }
      } else {
        Alert.alert(
          'Error',
          'Unable to load attendance data. Please try again.',
          [{ text: 'OK', onPress: () => navigation.goBack() }]
        );
      }
    } catch (error) {
      console.error('Error loading attendance:', error);
      Alert.alert('Error', 'Failed to load attendance data.');
    } finally {
      setLoading(false);
    }
  };

  const getLocation = async (): Promise<void> => {
    setGettingLocation(true);
    try {
      let { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permission Denied', 'Location permission is required for check-out.');
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
        Alert.alert('Permission Denied', 'Camera permission is required for check-out.');
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

  const handleCheckOut = async (): Promise<void> => {
    if (!location) {
      Alert.alert('Error', 'Location is required for check-out');
      return;
    }
    if (!photo) {
      Alert.alert('Error', 'Photo is required for check-out');
      return;
    }
    if (!userData) {
      Alert.alert('Error', 'User data not found');
      return;
    }
    if (!attendanceId) {
      Alert.alert('Error', 'No active attendance found. Please check in first.');
      return;
    }

    // Geofence: only allow check-out within allocated site
    if (siteLatitude != null && siteLongitude != null && location) {
      const radiusMeters = siteGeofenceRadiusMeters;
      const distanceMeters = getDistanceMeters(
        siteLatitude,
        siteLongitude,
        location.coords.latitude,
        location.coords.longitude
      );
      if (distanceMeters > radiusMeters) {
        const distanceText = formatDistanceMeters(distanceMeters);
        Alert.alert(
          'Location Not Allowed',
          `Check-out is only allowed at the allocated site. You are ${distanceText} away from the site (allowed: ${radiusMeters} m). Please reach the site to check out.`
        );
        return;
      }
    } else if (siteLatitude == null || siteLongitude == null) {
      Alert.alert('Error', 'Site location is not configured. Please contact your supervisor.');
      return;
    }

    setSubmitting(true);

    try {
      // Convert photo to base64
      const photoBase64 = await attendanceService.convertImageToBase64(photo);
      const photoFileName = `checkout_${Date.now()}.jpg`;

      // Create check-out data (guardId must be SecurityGuard.Id to match attendance record)
      const guardIdForApi = (userData as { guardId?: string }).guardId || userData.id;
      const checkOutData = {
        attendanceId,
        guardId: guardIdForApi,
        checkOutTime: new Date().toISOString(),
        latitude: location.coords.latitude,
        longitude: location.coords.longitude,
        photoBase64,
        photoFileName,
        notes: notes.trim(),
        agencyId: userData.agencyId,
      };

      console.log('Submitting check-out data:', checkOutData);

      const response = await attendanceService.checkOut(checkOutData);

      if (response.success) {
        Alert.alert(
          'Success',
          response.message || 'You have successfully checked out!',
          [{ text: 'OK', onPress: () => navigation.goBack() }]
        );
      } else {
        Alert.alert(
          'Check-out Failed',
          response.error?.message || 'Please try again.'
        );
      }
    } catch (error) {
      console.error('Check-out error:', error);
      Alert.alert('Error', 'Failed to process check-out. Please try again.');
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

  const calculateDuration = (): string => {
    if (!checkInTime) return 'N/A';
    try {
      const now = new Date();
      const checkInDate = new Date();
      const [time, period] = checkInTime.split(' ');
      const [hours, minutes] = time.split(':');
      let checkInHours = parseInt(hours, 10);
      const p = (period || '').toUpperCase();
      if (p === 'PM' && checkInHours !== 12) checkInHours += 12;
      if (p === 'AM' && checkInHours === 12) checkInHours = 0;
      checkInDate.setHours(checkInHours, parseInt(minutes), 0, 0);
      
      const diff = now.getTime() - checkInDate.getTime();
      const hoursDiff = Math.floor(diff / (1000 * 60 * 60));
      const minutesDiff = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      return `${hoursDiff}h ${minutesDiff}m`;
    } catch {
      return 'N/A';
    }
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading attendance data...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <LinearGradient
        colors={[COLORS.error, COLORS.error + 'DD']}
        style={styles.header}
      >
        <TouchableOpacity
          style={styles.backButton}
          onPress={() => navigation.goBack()}
        >
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Check Out</Text>
        <View style={styles.placeholder} />
      </LinearGradient>

      <View style={styles.content}>
        {/* User Profile Card */}
        {userData && (
          <Card style={styles.profileCard}>
            <View style={styles.profileAvatar}>
              <MaterialCommunityIcons name="account" size={32} color={COLORS.error} />
            </View>
            <View style={styles.profileInfo}>
              <Text style={styles.profileName}>{userData.username}</Text>
              <Text style={styles.profileRole}>{userData.role}</Text>
              <Text style={styles.profileEmail}>{userData.email}</Text>
            </View>
          </Card>
        )}

        {/* Shift Summary */}
        <Card style={styles.shiftCard}>
          <View style={styles.cardHeader}>
            <View style={[styles.cardIcon, { backgroundColor: COLORS.primary + '15' }]}>
              <MaterialCommunityIcons name="clock-outline" size={24} color={COLORS.primary} />
            </View>
            <View style={styles.cardHeaderText}>
              <Text style={styles.cardTitle}>Shift Summary</Text>
              <Text style={styles.cardSubtitle}>Your shift details</Text>
            </View>
          </View>
          <View style={styles.shiftInfo}>
            <View style={styles.shiftRow}>
              <Text style={styles.shiftLabel}>Site:</Text>
              <Text style={styles.shiftValue}>{siteName || 'N/A'}</Text>
            </View>
            <View style={styles.shiftRow}>
              <Text style={styles.shiftLabel}>Check-in:</Text>
              <Text style={styles.shiftValue}>{checkInTime || 'N/A'}</Text>
            </View>
            <View style={styles.shiftRow}>
              <Text style={styles.shiftLabel}>Duration:</Text>
              <Text style={styles.shiftValue}>{calculateDuration()}</Text>
            </View>
          </View>
        </Card>

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
          <MaterialCommunityIcons name="clock" size={24} color={COLORS.error} />
          <View style={styles.timeInfo}>
            <Text style={styles.timeLabel}>Check-out Time</Text>
            <Text style={styles.timeValue}>{getCurrentTimeString()}</Text>
          </View>
          <Text style={styles.dateText}>{getCurrentDateString()}</Text>
        </Card>
      </View>

      {/* Submit Button */}
      <View style={styles.footer}>
        <Button
          title="Confirm Check Out"
          onPress={handleCheckOut}
          loading={submitting}
          disabled={!location || !photo || !attendanceId}
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
    backgroundColor: COLORS.error + '15',
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
    color: COLORS.error,
    fontWeight: '500',
    marginTop: 2,
  },
  profileEmail: {
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  shiftCard: {
    padding: SIZES.md,
    marginBottom: SIZES.md,
  },
  shiftInfo: {
    marginTop: SIZES.md,
    paddingTop: SIZES.md,
    borderTopWidth: 1,
    borderTopColor: COLORS.gray200,
  },
  shiftRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: SIZES.sm,
  },
  shiftLabel: {
    fontSize: FONTS.body,
    color: COLORS.textSecondary,
  },
  shiftValue: {
    fontSize: FONTS.body,
    fontWeight: '500',
    color: COLORS.textPrimary,
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
    borderColor: COLORS.error,
  },
  retakeButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.error,
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
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    marginTop: SIZES.md,
    fontSize: FONTS.body,
    color: COLORS.textSecondary,
  },
});

export default CheckOutScreen;
