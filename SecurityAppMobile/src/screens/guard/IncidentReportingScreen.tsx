import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, Image } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import * as Location from 'expo-location';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Button from '../../components/common/Button';
import Input from '../../components/common/Input';
import Card from '../../components/common/Card';
import { IncidentReportingScreenProps } from '../../types/navigation';
import { incidentService } from '../../services/incidentService';
import { siteService } from '../../services/siteService';
import { authService } from '../../services/authService';
import { deploymentService } from '../../services/deploymentService';

interface IncidentType {
  id: string;
  name: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
}

interface MediaItem {
  uri: string;
}

const incidentTypes: IncidentType[] = [
  { id: 'theft', name: 'Theft/Robbery', icon: 'alert-circle' },
  { id: 'vandalism', name: 'Vandalism', icon: 'hammer' },
  { id: 'fire', name: 'Fire/Smoke', icon: 'fire' },
  { id: 'medical', name: 'Medical Emergency', icon: 'medical-bag' },
  { id: 'suspicious', name: 'Suspicious Activity', icon: 'eye' },
  { id: 'other', name: 'Other', icon: 'dots-horizontal' },
];

const IncidentReportingScreen: React.FC<IncidentReportingScreenProps> = ({ navigation }) => {
  const [selectedType, setSelectedType] = useState<string | null>(null);
  const [description, setDescription] = useState<string>('');
  const [media, setMedia] = useState<MediaItem[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [sites, setSites] = useState<any[]>([]);
  const [selectedSiteId, setSelectedSiteId] = useState<string | null>(null);
  const [location, setLocation] = useState<{ latitude: number; longitude: number } | null>(null);
  const [user, setUser] = useState<any>(null);

  const pickMedia = async (): Promise<void> => {
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images', 'videos'],
      allowsMultipleSelection: true,
      quality: 0.8,
    });
    if (!result.canceled) {
      setMedia([...media, ...result.assets]);
    }
  };

  const takePhoto = async (): Promise<void> => {
    const result = await ImagePicker.launchCameraAsync({ quality: 0.8 });
    if (!result.canceled) {
      setMedia([...media, result.assets[0]]);
    }
  };

  const removeMedia = (index: number): void => {
    setMedia(media.filter((_, i) => i !== index));
  };

  useEffect(() => {
    loadInitialData();
  }, []);

  const loadInitialData = async () => {
    try {
      // Get user data
      const userData = await authService.getStoredUser();
      setUser(userData);

      // Get sites – API returns { data: { items: [...], totalCount } } or { data: { Items: [...] } }
      const sitesResult = await siteService.getSites({ pageSize: 100 });
      if (sitesResult.success && sitesResult.data) {
        const raw = sitesResult.data as { items?: any[]; Items?: any[] };
        const list = raw?.items ?? raw?.Items ?? (Array.isArray(sitesResult.data) ? sitesResult.data : []);
        const sitesList = (list as any[]).map((s: any) => ({
          id: String(s?.id ?? s?.Id ?? ''),
          siteName: String(s?.siteName ?? s?.SiteName ?? ''),
        })).filter(s => s.id);
        setSites(sitesList);
        if (sitesList.length > 0) {
          setSelectedSiteId(sitesList[0].id);
        } else if (userData?.id) {
          // Fallback: get guard's deployments to use assigned site(s)
          const guardIdForApi = (userData as { guardId?: string }).guardId || userData.id;
          const today = new Date().toISOString().slice(0, 10);
          const depRes = await deploymentService.getDeployments({ guardId: guardIdForApi, dateFrom: today, dateTo: today, pageSize: 50, skipCache: true });
          const depRaw = depRes.success && depRes.data ? depRes.data : {};
          const depList = depRaw?.items ?? depRaw?.Items ?? (Array.isArray(depRes.data) ? depRes.data : []);
          const bySite = new Map<string, string>();
          (depList as any[]).forEach((d: any) => {
            const sid = String(d?.siteId ?? d?.SiteId ?? '');
            if (sid && !bySite.has(sid)) bySite.set(sid, String(d?.siteName ?? d?.SiteName ?? `Site ${sid.slice(0, 8)}`));
          });
          if (bySite.size > 0) {
            const fallbackSites = Array.from(bySite.entries()).map(([id, siteName]) => ({ id, siteName }));
            setSites(fallbackSites);
            setSelectedSiteId(fallbackSites[0].id);
          }
        }
      }

      // Get current location
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status === 'granted') {
        const locationData = await Location.getCurrentPositionAsync({});
        setLocation({
          latitude: locationData.coords.latitude,
          longitude: locationData.coords.longitude,
        });
      }
    } catch (error) {
      console.error('Error loading initial data:', error);
    }
  };

  const convertImageToBase64 = async (uri: string): Promise<string> => {
    const response = await fetch(uri);
    const blob = await response.blob();
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onloadend = () => {
        const base64 = reader.result as string;
        resolve(base64.split(',')[1]); // Remove data:image/jpeg;base64, prefix
      };
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  };

  const handleSubmit = async (): Promise<void> => {
    if (!selectedType) {
      Alert.alert('Error', 'Please select incident type');
      return;
    }
    if (!description.trim()) {
      Alert.alert('Error', 'Please provide description');
      return;
    }
    if (!selectedSiteId) {
      Alert.alert('Error', 'Please select a site');
      return;
    }
    if (!location) {
      Alert.alert('Error', 'Location not available. Please enable location services.');
      return;
    }
    if (!user) {
      Alert.alert('Error', 'User information not available. Please login again.');
      return;
    }

    setLoading(true);
    try {
      // Create incident
      const guardIdForApi = (user as { guardId?: string }).guardId || user.id;
      const incidentData = {
        incidentType: selectedType,
        description: description.trim(),
        siteId: selectedSiteId,
        guardId: guardIdForApi,
        agencyId: user.agencyId,
      };

      const result = await incidentService.createIncident(incidentData);

      if (result.success && result.data) {
        const incidentId = result.data.id;

        // Upload evidence if any
        if (media.length > 0) {
          for (const mediaItem of media) {
            try {
              const base64 = await convertImageToBase64(mediaItem.uri);
              const fileName = mediaItem.uri.split('/').pop() || 'evidence.jpg';
              const fileType = fileName.split('.').pop() || 'jpg';

              await incidentService.addIncidentEvidence(incidentId, {
                fileBase64: base64,
                fileName: fileName,
                fileType: fileType,
                description: 'Evidence photo',
              });
            } catch (error) {
              console.error('Error uploading evidence:', error);
              // Continue even if evidence upload fails
            }
          }
        }

        Alert.alert('Success', 'Incident reported successfully', [
          { text: 'OK', onPress: () => navigation.goBack() }
        ]);
      } else {
        Alert.alert('Error', result.error?.message || 'Failed to report incident');
      }
    } catch (error) {
      console.error('Error submitting incident:', error);
      Alert.alert('Error', 'An error occurred while reporting the incident. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Report Incident</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView contentContainerStyle={styles.content} showsVerticalScrollIndicator={false}>
        <Text style={styles.sectionTitle}>Incident Type</Text>
        <View style={styles.typeGrid}>
          {incidentTypes.map((type) => (
            <TouchableOpacity
              key={type.id}
              style={[styles.typeCard, selectedType === type.id && styles.selectedType]}
              onPress={() => setSelectedType(type.id)}
            >
              <MaterialCommunityIcons 
                name={type.icon} 
                size={28} 
                color={selectedType === type.id ? COLORS.white : COLORS.primary} 
              />
              <Text style={[styles.typeName, selectedType === type.id && styles.selectedTypeName]}>
                {type.name}
              </Text>
            </TouchableOpacity>
          ))}
        </View>

        <Text style={styles.sectionTitle}>Description</Text>
        <Input
          placeholder="Describe the incident in detail..."
          value={description}
          onChangeText={setDescription}
          multiline
          numberOfLines={5}
          style={styles.descriptionInput}
        />

        <Text style={styles.sectionTitle}>Evidence (Photos/Videos)</Text>
        <View style={styles.mediaSection}>
          <View style={styles.mediaButtons}>
            <TouchableOpacity style={styles.mediaButton} onPress={takePhoto}>
              <MaterialCommunityIcons name="camera" size={24} color={COLORS.primary} />
              <Text style={styles.mediaButtonText}>Camera</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.mediaButton} onPress={pickMedia}>
              <MaterialCommunityIcons name="image-multiple" size={24} color={COLORS.primary} />
              <Text style={styles.mediaButtonText}>Gallery</Text>
            </TouchableOpacity>
          </View>

          {media.length > 0 && (
            <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.mediaPreview}>
              {media.map((item, index) => (
                <View key={index} style={styles.mediaItem}>
                  <Image source={{ uri: item.uri }} style={styles.mediaImage} />
                  <TouchableOpacity style={styles.removeMedia} onPress={() => removeMedia(index)}>
                    <MaterialCommunityIcons name="close-circle" size={24} color={COLORS.error} />
                  </TouchableOpacity>
                </View>
              ))}
            </ScrollView>
          )}
        </View>

        <Text style={styles.sectionTitle}>Site</Text>
        {sites.length > 0 ? (
          <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.sitesContainer}>
            {sites.map((site) => (
              <TouchableOpacity
                key={site.id}
                style={[styles.siteCard, selectedSiteId === site.id && styles.selectedSite]}
                onPress={() => setSelectedSiteId(site.id)}
              >
                <Text style={[styles.siteName, selectedSiteId === site.id && styles.selectedSiteName]}>
                  {site.siteName}
                </Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        ) : (
          <Card style={styles.noSitesCard}>
            <MaterialCommunityIcons name="map-marker-off" size={24} color={COLORS.warning} />
            <Text style={styles.noSitesText}>No sites available. Contact your supervisor to get assigned to a site, then try again.</Text>
          </Card>
        )}

        <Card style={styles.locationCard}>
          <View style={styles.locationRow}>
            <MaterialCommunityIcons 
              name="map-marker" 
              size={24} 
              color={location ? COLORS.success : COLORS.warning} 
            />
            <View style={styles.locationInfo}>
              <Text style={styles.locationLabel}>Current Location</Text>
              <Text style={styles.locationValue}>
                {location 
                  ? `${location.latitude.toFixed(6)}, ${location.longitude.toFixed(6)}`
                  : 'Getting location...'}
              </Text>
            </View>
            {location && (
              <MaterialCommunityIcons name="check-circle" size={24} color={COLORS.success} />
            )}
          </View>
        </Card>

        <Button
          title="Submit Report"
          onPress={handleSubmit}
          loading={loading}
          style={styles.submitButton}
        />
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  content: { padding: SIZES.md },
  sectionTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm, marginTop: SIZES.md },
  typeGrid: { flexDirection: 'row', flexWrap: 'wrap', justifyContent: 'space-between' },
  typeCard: { width: '31%', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, alignItems: 'center', marginBottom: SIZES.sm, borderWidth: 2, borderColor: 'transparent', ...SHADOWS.small },
  selectedType: { backgroundColor: COLORS.primary, borderColor: COLORS.primary },
  typeName: { fontSize: FONTS.caption, color: COLORS.textPrimary, textAlign: 'center', marginTop: SIZES.xs },
  selectedTypeName: { color: COLORS.white },
  descriptionInput: { marginBottom: 0 },
  mediaSection: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.md, ...SHADOWS.small },
  mediaButtons: { flexDirection: 'row', gap: SIZES.md },
  mediaButton: { flex: 1, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.gray100, paddingVertical: SIZES.md, borderRadius: SIZES.radiusMd, gap: SIZES.sm },
  mediaButtonText: { fontSize: FONTS.bodySmall, color: COLORS.primary, fontWeight: '500' },
  mediaPreview: { marginTop: SIZES.md },
  mediaItem: { marginRight: SIZES.sm, position: 'relative' },
  mediaImage: { width: 80, height: 80, borderRadius: SIZES.radiusSm },
  removeMedia: { position: 'absolute', top: -8, right: -8 },
  locationCard: { padding: SIZES.md, marginBottom: SIZES.lg },
  locationRow: { flexDirection: 'row', alignItems: 'center' },
  locationInfo: { flex: 1, marginLeft: SIZES.sm },
  locationLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  locationValue: { fontSize: FONTS.body, fontWeight: '500', color: COLORS.textPrimary },
  submitButton: { marginBottom: SIZES.xl },
  sitesContainer: { marginBottom: SIZES.md },
  siteCard: { 
    paddingHorizontal: SIZES.md, 
    paddingVertical: SIZES.sm, 
    backgroundColor: COLORS.white, 
    borderRadius: SIZES.radiusMd, 
    marginRight: SIZES.sm,
    borderWidth: 2,
    borderColor: 'transparent',
    ...SHADOWS.small 
  },
  selectedSite: { 
    backgroundColor: COLORS.primary, 
    borderColor: COLORS.primary 
  },
  siteName: { 
    fontSize: FONTS.bodySmall, 
    color: COLORS.textPrimary, 
    fontWeight: '500' 
  },
  selectedSiteName: { 
    color: COLORS.white 
  },
  noSitesCard: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: SIZES.md,
    marginBottom: SIZES.md,
    gap: SIZES.sm,
    backgroundColor: COLORS.warning + '15',
    borderWidth: 1,
    borderColor: COLORS.warning + '40',
  },
  noSitesText: {
    flex: 1,
    fontSize: FONTS.caption,
    color: COLORS.textSecondary,
  },
});

export default IncidentReportingScreen;
