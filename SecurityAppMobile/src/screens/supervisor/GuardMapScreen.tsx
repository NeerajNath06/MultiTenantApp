import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Dimensions, Alert, RefreshControl, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { deploymentService } from '../../services/deploymentService';
import { siteService } from '../../services/siteService';

const { width, height } = Dimensions.get('window');

const DEFAULT_LAT = 28.6139;
const DEFAULT_LNG = 77.209;

interface GuardLocation {
  id: string;
  name: string;
  site: string;
  status: 'on_duty' | 'patrolling' | 'break' | 'offline';
  lastSeen: string;
  battery: number;
  coordinates: { lat: number; lng: number };
}

function GuardMapScreen({ navigation }: any) {
  const [selectedGuard, setSelectedGuard] = useState<GuardLocation | null>(null);
  const [filterStatus, setFilterStatus] = useState<string>('all');
  const [guards, setGuards] = useState<GuardLocation[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const load = useCallback(async () => {
    let supervisorId: string | undefined;
    try {
      const userData = await AsyncStorage.getItem('userData');
      if (userData) {
        const parsed = JSON.parse(userData);
        supervisorId = parsed?.id ?? parsed?.userId ?? undefined;
      }
    } catch {}
    const today = new Date().toISOString().slice(0, 10);
    const [rosterRes, sitesRes] = await Promise.all([
      deploymentService.getRoster(today, today, undefined, supervisorId),
      siteService.getSites({ supervisorId, pageSize: 100 }),
    ]);
    const rosterData = (rosterRes.success && rosterRes.data ? rosterRes.data : {}) as { deployments?: any[]; Deployments?: any[] };
    const deployments = rosterData.deployments ?? rosterData.Deployments ?? [];
    const sitesData = sitesRes.success && sitesRes.data ? sitesRes.data : ({} as { items?: any[]; Items?: any[] });
    const sites = sitesData.items ?? sitesData.Items ?? [];
    const siteMap = new Map<string, any>(sites.map((s: any) => [s.id, s]));
    const list: GuardLocation[] = deployments
      .filter((d: any) => d.guardId && d.guardName)
      .map((d: any, idx: number) => {
        const site = siteMap.get(d.siteId);
        const lat = site?.latitude ?? DEFAULT_LAT + (idx * 0.001);
        const lng = site?.longitude ?? DEFAULT_LNG + (idx * 0.001);
        return {
          id: d.guardId || String(idx),
          name: d.guardName || 'Guard',
          site: d.siteName || site?.siteName || 'Site',
          status: 'on_duty' as const,
          lastSeen: 'On duty',
          battery: 100,
          coordinates: { lat, lng },
        };
      });
    setGuards(list);
  }, []);

  useEffect(() => {
    load().finally(() => setLoading(false));
  }, [load]);

  const onRefresh = async () => {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'on_duty': return COLORS.success;
      case 'patrolling': return COLORS.primaryBlue;
      case 'break': return COLORS.warning;
      case 'offline': return COLORS.error;
      default: return COLORS.gray500;
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'on_duty': return 'On Duty';
      case 'patrolling': return 'Patrolling';
      case 'break': return 'On Break';
      case 'offline': return 'Offline';
      default: return status;
    }
  };

  const handleGuardSelect = (guard: GuardLocation) => {
    setSelectedGuard(guard);
  };

  const handleContactGuard = (guard: GuardLocation) => {
    Alert.alert('Contact', `Call ${guard.name}?`, [
      { text: 'Cancel', style: 'cancel' },
      { text: 'Call', onPress: () => console.log('Calling...') }
    ]);
  };

  const filteredGuards = guards.filter(g => filterStatus === 'all' || g.status === filterStatus);

  const onDutyCount = guards.filter(g => g.status === 'on_duty').length;
  const patrollingCount = guards.filter(g => g.status === 'patrolling').length;
  const offlineCount = guards.filter(g => g.status === 'offline').length;

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Live Guard Map</Text>
          <View style={styles.refreshBtn} />
        </View>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Live Guard Map</Text>
        <TouchableOpacity style={styles.refreshBtn} onPress={onRefresh}>
          <MaterialCommunityIcons name="refresh" size={24} color={COLORS.primary} />
        </TouchableOpacity>
      </View>

      {/* Stats */}
      <View style={styles.statsRow}>
        <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.success }]}>{onDutyCount}</Text>
          <Text style={styles.statLabel}>On Duty</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.primaryBlue + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.primaryBlue }]}>{patrollingCount}</Text>
          <Text style={styles.statLabel}>Patrolling</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.error + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.error }]}>{offlineCount}</Text>
          <Text style={styles.statLabel}>Offline</Text>
        </View>
      </View>

      {/* Map Placeholder */}
      <View style={styles.mapContainer}>
        <View style={styles.mapPlaceholder}>
          <MaterialCommunityIcons name="map" size={64} color={COLORS.gray300} />
          <Text style={styles.mapText}>Map View</Text>
          <Text style={styles.mapSubtext}>Guard locations displayed here</Text>
          
          {/* Guard Markers from roster */}
          <View style={styles.mockMarkers}>
            {filteredGuards.slice(0, 6).map((guard, index) => (
              <TouchableOpacity
                key={guard.id}
                style={[styles.markerDot, { 
                  backgroundColor: getStatusColor(guard.status),
                  left: 50 + (index * 60),
                  top: 30 + (index % 2) * 40
                }]}
                onPress={() => handleGuardSelect(guard)}
              >
                <Text style={styles.markerText}>{guard.name.split(' ').map(n => n[0]).join('')}</Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>
      </View>

      {/* Filter */}
      <View style={styles.filterContainer}>
        {['all', 'on_duty', 'patrolling', 'break', 'offline'].map((status) => (
          <TouchableOpacity
            key={status}
            style={[styles.filterChip, filterStatus === status && styles.filterChipActive]}
            onPress={() => setFilterStatus(status)}
          >
            {status !== 'all' && (
              <View style={[styles.filterDot, { backgroundColor: getStatusColor(status) }]} />
            )}
            <Text style={[styles.filterText, filterStatus === status && styles.filterTextActive]}>
              {status === 'all' ? 'All' : getStatusLabel(status)}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      {/* Guard List */}
      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.listContent}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {filteredGuards.length === 0 ? (
          <View style={styles.emptyWrap}>
            <Text style={styles.emptyText}>No guards on duty for today</Text>
          </View>
        ) : filteredGuards.map((guard) => (
          <TouchableOpacity
            key={guard.id}
            style={[styles.guardCard, selectedGuard?.id === guard.id && styles.guardCardSelected]}
            onPress={() => handleGuardSelect(guard)}
          >
            <View style={styles.guardAvatar}>
              <Text style={styles.avatarText}>{guard.name.split(' ').map(n => n[0]).join('')}</Text>
              <View style={[styles.statusIndicator, { backgroundColor: getStatusColor(guard.status) }]} />
            </View>
            
            <View style={styles.guardInfo}>
              <Text style={styles.guardName}>{guard.name}</Text>
              <Text style={styles.guardSite}>{guard.site}</Text>
              <View style={styles.guardMeta}>
                <View style={styles.metaItem}>
                  <MaterialCommunityIcons name="clock-outline" size={12} color={COLORS.gray500} />
                  <Text style={styles.metaText}>{guard.lastSeen}</Text>
                </View>
                <View style={styles.metaItem}>
                  <MaterialCommunityIcons 
                    name={guard.battery > 20 ? 'battery' : 'battery-alert'} 
                    size={12} 
                    color={guard.battery > 20 ? COLORS.success : COLORS.error} 
                  />
                  <Text style={[styles.metaText, guard.battery <= 20 && { color: COLORS.error }]}>{guard.battery}%</Text>
                </View>
              </View>
            </View>

            <View style={styles.guardActions}>
              <View style={[styles.statusBadge, { backgroundColor: getStatusColor(guard.status) + '15' }]}>
                <Text style={[styles.statusText, { color: getStatusColor(guard.status) }]}>
                  {getStatusLabel(guard.status)}
                </Text>
              </View>
              <TouchableOpacity style={styles.callBtn} onPress={() => handleContactGuard(guard)}>
                <MaterialCommunityIcons name="phone" size={18} color={COLORS.success} />
              </TouchableOpacity>
            </View>
          </TouchableOpacity>
        ))}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  refreshBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  statsRow: { flexDirection: 'row', paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, gap: SIZES.sm, backgroundColor: COLORS.white },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.sm, borderRadius: SIZES.radiusMd },
  statValue: { fontSize: FONTS.h4, fontWeight: 'bold' },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  mapContainer: { marginHorizontal: SIZES.md, marginTop: SIZES.sm },
  mapPlaceholder: { height: 180, backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusMd, justifyContent: 'center', alignItems: 'center', position: 'relative', overflow: 'hidden' },
  mapText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.gray400, marginTop: SIZES.sm },
  mapSubtext: { fontSize: FONTS.caption, color: COLORS.gray400 },
  mockMarkers: { position: 'absolute', top: 0, left: 0, right: 0, bottom: 0 },
  markerDot: { position: 'absolute', width: 32, height: 32, borderRadius: 16, justifyContent: 'center', alignItems: 'center', borderWidth: 2, borderColor: COLORS.white },
  markerText: { fontSize: FONTS.tiny, fontWeight: 'bold', color: COLORS.white },
  filterContainer: { flexDirection: 'row', paddingHorizontal: SIZES.md, marginTop: SIZES.md, gap: SIZES.xs },
  filterChip: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusFull },
  filterChipActive: { backgroundColor: COLORS.primary },
  filterDot: { width: 8, height: 8, borderRadius: 4, marginRight: 4 },
  filterText: { fontSize: FONTS.tiny, color: COLORS.textSecondary, fontWeight: '500' },
  filterTextActive: { color: COLORS.white },
  listContent: { padding: SIZES.md },
  guardCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  guardCardSelected: { borderWidth: 2, borderColor: COLORS.primary },
  guardAvatar: { width: 48, height: 48, borderRadius: 24, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center', position: 'relative' },
  avatarText: { fontSize: FONTS.bodySmall, fontWeight: 'bold', color: COLORS.white },
  statusIndicator: { position: 'absolute', bottom: 0, right: 0, width: 14, height: 14, borderRadius: 7, borderWidth: 2, borderColor: COLORS.white },
  guardInfo: { flex: 1, marginLeft: SIZES.sm },
  guardName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  guardSite: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  guardMeta: { flexDirection: 'row', marginTop: 4, gap: SIZES.md },
  metaItem: { flexDirection: 'row', alignItems: 'center', gap: 2 },
  metaText: { fontSize: FONTS.tiny, color: COLORS.gray500 },
  guardActions: { alignItems: 'flex-end' },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.tiny, fontWeight: '600' },
  callBtn: { width: 32, height: 32, borderRadius: 16, backgroundColor: COLORS.success + '15', justifyContent: 'center', alignItems: 'center', marginTop: SIZES.xs },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  emptyWrap: { paddingVertical: SIZES.xl, alignItems: 'center' },
  emptyText: { fontSize: FONTS.body, color: COLORS.textSecondary },
});

export default GuardMapScreen;
