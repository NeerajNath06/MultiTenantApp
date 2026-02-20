import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Linking, Alert, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as Location from 'expo-location';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { SiteNavigationScreenProps } from '../../types/navigation';
import { authService } from '../../services/authService';
import { deploymentService } from '../../services/deploymentService';
import { siteService } from '../../services/siteService';
import { getDistanceMeters, formatDistanceMeters } from '../../utils/geoUtils';

interface SiteLocation {
  id: string;
  name: string;
  address: string;
  coordinates: { latitude: number; longitude: number } | null;
  contactPerson: string;
  contactPhone: string;
  distance: string;
  estimatedTime: string;
}

const SiteNavigationScreen: React.FC<SiteNavigationScreenProps> = ({ navigation }) => {
  const [currentLocation, setCurrentLocation] = useState<string>('Getting location...');
  const [userLocation, setUserLocation] = useState<{ latitude: number; longitude: number } | null>(null);
  const [currentSite, setCurrentSite] = useState<SiteLocation | null>(null);
  const [alternativeSites, setAlternativeSites] = useState<SiteLocation[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const formatAddress = (addr?: string, city?: string, state?: string, pin?: string) => {
    const parts = [addr, city, state, pin].filter(Boolean);
    return parts.length ? parts.join(', ') : '—';
  };

  const loadData = useCallback(async () => {
    try {
      const user = await authService.getStoredUser();
      const guardId = (user as { guardId?: string })?.guardId ?? (user as { id?: string })?.id;
      if (!guardId) {
        setCurrentSite(null);
        setAlternativeSites([]);
        return;
      }

      const today = new Date().toISOString().slice(0, 10);
      const [deploymentsRes, sitesRes] = await Promise.all([
        deploymentService.getDeployments({ guardId, dateFrom: today, dateTo: today, pageSize: 50, skipCache: true }),
        siteService.getSites({ pageSize: 100 }),
      ]);

      const depRaw = deploymentsRes.success && deploymentsRes.data ? deploymentsRes.data : {};
      const depList = depRaw?.items ?? depRaw?.Items ?? (Array.isArray(deploymentsRes.data) ? deploymentsRes.data : []) as any[];
      const sitesRaw = sitesRes.success && sitesRes.data ? sitesRes.data : {};
      const siteList = sitesRaw?.items ?? sitesRaw?.Items ?? (Array.isArray(sitesRes.data) ? sitesRes.data : []) as any[];

      const siteMap = new Map<string, any>();
      (siteList || []).forEach((s: any) => {
        const id = String(s?.id ?? s?.Id ?? '');
        if (id) siteMap.set(id, s);
      });

      let userLat: number | null = null;
      let userLon: number | null = null;
      try {
        const { status } = await Location.getForegroundPermissionsAsync();
        if (status === 'granted') {
          const loc = await Location.getCurrentPositionAsync({});
          userLat = loc.coords.latitude;
          userLon = loc.coords.longitude;
          setUserLocation({ latitude: userLat, longitude: userLon });
          setCurrentLocation(`${userLat.toFixed(4)}, ${userLon.toFixed(4)}`);
        } else {
          setCurrentLocation('Location not available');
        }
      } catch {
        setCurrentLocation('Location not available');
      }

      const toSiteLocation = (s: any, isCurrent: boolean): SiteLocation => {
        const sid = String(s?.id ?? s?.Id ?? '');
        const site = siteMap.get(sid) ?? s;
        const lat = site?.latitude ?? site?.Latitude;
        const lon = site?.longitude ?? site?.Longitude;
        const name = site?.siteName ?? site?.SiteName ?? s?.siteName ?? s?.SiteName ?? 'Site';
        const address = formatAddress(
          site?.address ?? site?.Address ?? s?.address,
          site?.city ?? site?.City ?? s?.city,
          site?.state ?? site?.State ?? s?.state,
          site?.pinCode ?? site?.PinCode ?? s?.pinCode
        );
        let distance = '—';
        let estimatedTime = '—';
        if (userLat != null && userLon != null && typeof lat === 'number' && typeof lon === 'number') {
          const meters = getDistanceMeters(userLat, userLon, lat, lon);
          distance = formatDistanceMeters(meters);
          const minutes = Math.max(1, Math.round(meters / 80));
          estimatedTime = `~${minutes} min`;
        }
        return {
          id: sid,
          name,
          address,
          coordinates: typeof lat === 'number' && typeof lon === 'number' ? { latitude: lat, longitude: lon } : null,
          contactPerson: site?.contactPerson ?? site?.ContactPerson ?? '—',
          contactPhone: site?.contactPhone ?? site?.ContactPhone ?? '—',
          distance,
          estimatedTime,
        };
      };

      if (depList.length > 0) {
        const first = depList[0];
        const firstSiteId = String(first?.siteId ?? first?.SiteId ?? '');
        const firstSite = siteMap.get(firstSiteId) ?? first;
        setCurrentSite(toSiteLocation(firstSite || { id: firstSiteId, siteName: first?.siteName ?? first?.SiteName ?? 'Current Site' }, true));

        const otherSites: SiteLocation[] = [];
        const seen = new Set<string>([firstSiteId]);
        depList.slice(1).forEach((d: any) => {
          const sid = String(d?.siteId ?? d?.SiteId ?? '');
          if (sid && !seen.has(sid)) {
            seen.add(sid);
            const site = siteMap.get(sid) ?? d;
            otherSites.push(toSiteLocation(site, false));
          }
        });
        setAlternativeSites(otherSites);
      } else {
        setCurrentSite(null);
        const allSites = (siteList || []).map((s: any) => toSiteLocation(s, false));
        setAlternativeSites(allSites.slice(0, 10));
      }
    } catch (e) {
      console.error('SiteNavigation load error:', e);
      setCurrentSite(null);
      setAlternativeSites([]);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadData();
  }, [loadData]);

  const openMaps = (latitude: number, longitude: number, name: string) => {
    const url = `https://www.google.com/maps/dir/?api=1&destination=${latitude},${longitude}&destination_place_id=${encodeURIComponent(name)}`;
    Linking.openURL(url).catch(() => Alert.alert('Error', 'Unable to open maps application'));
  };

  const callContact = (phoneNumber: string) => {
    if (!phoneNumber || phoneNumber === '—') return;
    Linking.openURL(`tel:${phoneNumber}`).catch(() => Alert.alert('Error', 'Unable to make phone call'));
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Site Navigation</Text>
          <View style={styles.placeholder} />
        </View>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading sites...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Site Navigation</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView
        contentContainerStyle={styles.content}
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {currentSite ? (
          <Card style={styles.currentSiteCard}>
            <View style={styles.currentSiteHeader}>
              <View style={styles.currentBadge}>
                <MaterialCommunityIcons name="map-marker" size={16} color={COLORS.primary} />
                <Text style={styles.currentBadgeText}>Current Assignment</Text>
              </View>
              {currentSite.coordinates && (
                <TouchableOpacity
                  style={styles.directionsButton}
                  onPress={() => openMaps(currentSite.coordinates!.latitude, currentSite.coordinates!.longitude, currentSite.name)}
                >
                  <MaterialCommunityIcons name="navigation" size={16} color={COLORS.white} />
                  <Text style={styles.directionsText}>Directions</Text>
                </TouchableOpacity>
              )}
            </View>
            <Text style={styles.siteName}>{currentSite.name}</Text>
            <Text style={styles.siteAddress}>{currentSite.address}</Text>
            <View style={styles.siteInfo}>
              <View style={styles.infoItem}>
                <MaterialCommunityIcons name="map-marker-distance" size={16} color={COLORS.gray400} />
                <Text style={styles.infoText}>{currentSite.distance} • {currentSite.estimatedTime}</Text>
              </View>
            </View>
            {(currentSite.contactPerson !== '—' || currentSite.contactPhone !== '—') && (
              <TouchableOpacity style={styles.contactButton} onPress={() => callContact(currentSite.contactPhone)}>
                <MaterialCommunityIcons name="phone-outline" size={18} color={COLORS.primary} />
                <Text style={styles.contactButtonText}>Call {currentSite.contactPerson}</Text>
              </TouchableOpacity>
            )}
          </Card>
        ) : (
          <Card style={styles.currentSiteCard}>
            <Text style={styles.siteName}>No assignment today</Text>
            <Text style={styles.siteAddress}>You have no site assigned for today. Check with your supervisor.</Text>
          </Card>
        )}

        <Text style={styles.sectionTitle}>{currentSite ? 'Other Sites' : 'Sites'}</Text>
        {alternativeSites.length === 0 ? (
          <Card style={styles.alternativeSiteCard}>
            <Text style={styles.alternativeSiteAddress}>No other sites to show.</Text>
          </Card>
        ) : (
          alternativeSites.map((site) => (
            <Card key={site.id} style={styles.alternativeSiteCard}>
              <View style={styles.alternativeSiteHeader}>
                <Text style={styles.alternativeSiteName}>{site.name}</Text>
                {site.coordinates && (
                  <TouchableOpacity
                    style={styles.navigateBtn}
                    onPress={() => openMaps(site.coordinates!.latitude, site.coordinates!.longitude, site.name)}
                  >
                    <MaterialCommunityIcons name="navigation" size={16} color={COLORS.white} />
                    <Text style={styles.navigateBtnText}>Navigate</Text>
                  </TouchableOpacity>
                )}
              </View>
              <Text style={styles.alternativeSiteAddress}>{site.address}</Text>
              <View style={styles.siteInfo}>
                <View style={styles.infoItem}>
                  <MaterialCommunityIcons name="clock" size={16} color={COLORS.gray400} />
                  <Text style={styles.infoText}>{site.distance} • {site.estimatedTime}</Text>
                </View>
              </View>
              {(site.contactPerson !== '—' || site.contactPhone !== '—') && (
                <TouchableOpacity style={styles.contactButtonAlt} onPress={() => callContact(site.contactPhone)}>
                  <MaterialCommunityIcons name="phone-outline" size={18} color={COLORS.secondary} />
                  <Text style={[styles.contactButtonText, { color: COLORS.secondary }]}>Call {site.contactPerson}</Text>
                </TouchableOpacity>
              )}
            </Card>
          ))
        )}

        <Card style={styles.quickActionsCard}>
          <Text style={styles.quickActionsTitle}>Quick Actions</Text>
          <View style={styles.actionButtons}>
            <TouchableOpacity style={styles.actionButton} onPress={() => navigation.navigate('IncidentReporting')}>
              <MaterialCommunityIcons name="alert-outline" size={20} color={COLORS.warning} />
              <Text style={styles.actionButtonText}>Report Issue</Text>
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
    ...SHADOWS.small,
  },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  content: { padding: SIZES.md },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: SIZES.xl },
  loadingText: { marginTop: SIZES.md, fontSize: FONTS.body, color: COLORS.textSecondary },
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
