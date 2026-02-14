import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Linking, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { SiteNavigationScreenProps } from '../../types/navigation';

interface SiteLocation {
  id: number;
  name: string;
  address: string;
  coordinates: {
    latitude: number;
    longitude: number;
  };
  contactPerson: string;
  contactPhone: string;
  distance: string;
  estimatedTime: string;
}

const SiteNavigationScreen: React.FC<SiteNavigationScreenProps> = ({ navigation }) => {
  const [currentLocation, setCurrentLocation] = useState<string>('Getting location...');

  // Mock current site data
  const currentSite: SiteLocation = {
    id: 1,
    name: 'Tech Park - Block A',
    address: '123 Business Park, Sector 62, Noida, Uttar Pradesh 201301',
    coordinates: {
      latitude: 28.5355,
      longitude: 77.3910,
    },
    contactPerson: 'Mr. Sharma',
    contactPhone: '+91 98765 43210',
    distance: '0.0 km',
    estimatedTime: '0 min',
  };

  // Mock alternative sites (in case guard needs to switch sites)
  const alternativeSites: SiteLocation[] = [
    {
      id: 2,
      name: 'Mall Plaza - Security Point',
      address: '456 Shopping Mall, Sector 18, Noida, Uttar Pradesh 201301',
      coordinates: {
        latitude: 28.5698,
        longitude: 77.3234,
      },
      contactPerson: 'Ms. Gupta',
      contactPhone: '+91 98765 43211',
      distance: '2.5 km',
      estimatedTime: '8 min',
    },
    {
      id: 3,
      name: 'IT Hub - Main Gate',
      address: '789 Tech Campus, Sector 125, Noida, Uttar Pradesh 201303',
      coordinates: {
        latitude: 28.5449,
        longitude: 77.3374,
      },
      contactPerson: 'Mr. Patel',
      contactPhone: '+91 98765 43212',
      distance: '3.2 km',
      estimatedTime: '12 min',
    },
  ];

  const openMaps = (latitude: number, longitude: number, name: string) => {
    const url = `https://www.google.com/maps/dir/?api=1&destination=${latitude},${longitude}&destination_place_id=${encodeURIComponent(name)}`;
    Linking.openURL(url).catch(() => {
      Alert.alert('Error', 'Unable to open maps application');
    });
  };

  const callContact = (phoneNumber: string) => {
    Linking.openURL(`tel:${phoneNumber}`).catch(() => {
      Alert.alert('Error', 'Unable to make phone call');
    });
  };

  const navigateToCurrentSite = () => {
    openMaps(
      currentSite.coordinates.latitude,
      currentSite.coordinates.longitude,
      currentSite.name
    );
  };

  const navigateToAlternativeSite = (site: SiteLocation) => {
    openMaps(site.coordinates.latitude, site.coordinates.longitude, site.name);
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Site Navigation</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView contentContainerStyle={styles.content} showsVerticalScrollIndicator={false}>
        {/* Current Assignment */}
        <Card style={styles.currentSiteCard}>
          <View style={styles.currentSiteHeader}>
            <View style={styles.currentBadge}>
              <MaterialCommunityIcons name="map-marker" size={16} color={COLORS.primary} />
              <Text style={styles.currentBadgeText}>Current Assignment</Text>
            </View>
            <TouchableOpacity 
              style={styles.directionsButton}
              onPress={navigateToCurrentSite}
            >
              <MaterialCommunityIcons name="navigation" size={16} color={COLORS.white} />
              <Text style={styles.directionsText}>Directions</Text>
            </TouchableOpacity>
          </View>

          <Text style={styles.siteName}>{currentSite.name}</Text>
          <Text style={styles.siteAddress}>{currentSite.address}</Text>

          <View style={styles.siteInfo}>
            <View style={styles.infoItem}>
              <MaterialCommunityIcons name="clock" size={16} color={COLORS.gray400} />
              <Text style={styles.infoText}>Your shift site</Text>
            </View>
            <View style={styles.infoItem}>
              <MaterialCommunityIcons name="map-marker-distance" size={16} color={COLORS.gray400} />
              <Text style={styles.infoText}>{currentSite.distance} • {currentSite.estimatedTime}</Text>
            </View>
          </View>

          <TouchableOpacity 
            style={styles.contactButton}
            onPress={() => callContact(currentSite.contactPhone)}
          >
            <MaterialCommunityIcons name="phone-outline" size={18} color={COLORS.primary} />
            <Text style={styles.contactButtonText}>
              Call {currentSite.contactPerson}
            </Text>
          </TouchableOpacity>
        </Card>

        {/* Alternative Sites */}
        <Text style={styles.sectionTitle}>Alternative Sites</Text>
        {alternativeSites.map((site) => (
          <Card key={site.id} style={styles.alternativeSiteCard}>
            <View style={styles.alternativeSiteHeader}>
              <Text style={styles.alternativeSiteName}>{site.name}</Text>
              <TouchableOpacity 
                style={styles.navigateBtn}
                onPress={() => navigateToAlternativeSite(site)}
              >
                <MaterialCommunityIcons name="navigation" size={16} color={COLORS.white} />
                <Text style={styles.navigateBtnText}>Navigate</Text>
              </TouchableOpacity>
            </View>

            <Text style={styles.alternativeSiteAddress}>{site.address}</Text>

            <View style={styles.siteInfo}>
              <View style={styles.infoItem}>
                <MaterialCommunityIcons name="clock" size={16} color={COLORS.gray400} />
                <Text style={styles.infoText}>{site.distance} • {site.estimatedTime}</Text>
              </View>
            </View>

            <TouchableOpacity 
              style={styles.contactButtonAlt}
              onPress={() => callContact(site.contactPhone)}
            >
              <MaterialCommunityIcons name="phone-outline" size={18} color={COLORS.secondary} />
              <Text style={[styles.contactButtonText, { color: COLORS.secondary }]}>
                Call {site.contactPerson}
              </Text>
            </TouchableOpacity>
          </Card>
        ))}

        {/* Quick Actions */}
        <Card style={styles.quickActionsCard}>
          <Text style={styles.quickActionsTitle}>Quick Actions</Text>
          
          <View style={styles.actionButtons}>
            <TouchableOpacity style={styles.actionButton}>
              <MaterialCommunityIcons name="share-variant" size={20} color={COLORS.primary} />
              <Text style={styles.actionButtonText}>Share Location</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionButton}>
              <MaterialCommunityIcons name="alert-outline" size={20} color={COLORS.warning} />
              <Text style={styles.actionButtonText}>Report Issue</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionButton}>
              <MaterialCommunityIcons name="shield-outline" size={20} color={COLORS.success} />
              <Text style={styles.actionButtonText}>Emergency</Text>
            </TouchableOpacity>
          </View>
        </Card>
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { 
    flexDirection: 'row', 
    alignItems: 'center', 
    justifyContent: 'space-between', 
    paddingHorizontal: SIZES.md, 
    paddingVertical: SIZES.md, 
    backgroundColor: COLORS.white, 
    ...SHADOWS.small 
  },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  content: { padding: SIZES.md },
  currentSiteCard: { padding: SIZES.md, marginBottom: SIZES.md, backgroundColor: COLORS.primary + '08' },
  currentSiteHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SIZES.sm },
  currentBadge: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.primary + '15', paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  currentBadgeText: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.primary, marginLeft: 4 },
  directionsButton: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.primary, paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  directionsText: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.white, marginLeft: 4 },
  siteName: { fontSize: FONTS.h4, fontWeight: 'bold', color: COLORS.textPrimary, marginBottom: 4 },
  siteAddress: { fontSize: FONTS.body, color: COLORS.textSecondary, marginBottom: SIZES.md, lineHeight: 20 },
  siteInfo: { flexDirection: 'row', marginBottom: SIZES.md, gap: SIZES.lg },
  infoItem: { flexDirection: 'row', alignItems: 'center' },
  infoText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginLeft: 4 },
  contactButton: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.white, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm, borderWidth: 1, borderColor: COLORS.primary },
  contactButtonAlt: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.white, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm, borderWidth: 1, borderColor: COLORS.secondary },
  contactButtonText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.primary, marginLeft: SIZES.xs },
  sectionTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm, marginTop: SIZES.lg },
  alternativeSiteCard: { padding: SIZES.md, marginBottom: SIZES.md, backgroundColor: COLORS.secondary + '08' },
  alternativeSiteHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SIZES.sm },
  alternativeSiteName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, flex: 1 },
  navigateBtn: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.secondary, paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  navigateBtnText: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.white, marginLeft: 4 },
  alternativeSiteAddress: { fontSize: FONTS.body, color: COLORS.textSecondary, marginBottom: SIZES.md, lineHeight: 20 },
  quickActionsCard: { padding: SIZES.md, marginBottom: SIZES.xl },
  quickActionsTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  actionButtons: { flexDirection: 'row', justifyContent: 'space-around' },
  actionButton: { alignItems: 'center', flex: 1 },
  actionButtonText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs, textAlign: 'center' },
});

export default SiteNavigationScreen;
