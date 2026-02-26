import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, TextInput, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useFocusEffect } from '@react-navigation/native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { authService } from '../../services/authService';
import { vehicleService, VehicleLogItem } from '../../services/vehicleService';
import { deploymentService } from '../../services/deploymentService';

function formatTime(iso: string): string {
  if (!iso) return '—';
  try {
    const d = new Date(iso);
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  } catch {
    return iso;
  }
}

function VehicleLogScreen({ navigation }: any) {
  const [searchQuery, setSearchQuery] = useState('');
  const [activeTab, setActiveTab] = useState<'in' | 'out' | 'all'>('in');
  const [vehicles, setVehicles] = useState<VehicleLogItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [guardId, setGuardId] = useState<string | null>(null);
  const [siteId, setSiteId] = useState<string | null>(null);
  const [slotsAvailable, setSlotsAvailable] = useState<number | null>(null);

  const loadVehicleLogs = useCallback(async (isRefresh = false) => {
    const user = await authService.getStoredUser();
    const gid = (user as { guardId?: string })?.guardId ?? (user as { id?: string })?.id;
    if (!gid) {
      setVehicles([]);
      setGuardId(null);
      setSiteId(null);
      setLoading(false);
      setRefreshing(false);
      return;
    }
    setGuardId(gid);

    const today = new Date().toISOString().slice(0, 10);
    const deploymentsRes = await deploymentService.getDeployments({
      guardId: gid,
      dateFrom: today,
      dateTo: today,
      pageSize: 10,
      skipCache: true,
    });
    const depRaw = deploymentsRes.data;
    const depList = Array.isArray(depRaw) ? depRaw : (depRaw?.items ?? depRaw?.Items ?? []);
    const firstDeployment = depList[0];
    const sid = firstDeployment?.siteId ?? firstDeployment?.SiteId ?? null;
    setSiteId(sid);

    // Guard sees only their assigned site's vehicle logs (siteId from today's deployment).
    const result = await vehicleService.getVehicleLogs({
      guardId: gid,
      siteId: sid ?? undefined,
      dateFrom: today,
      dateTo: today,
      pageSize: 100,
      sortDirection: 'desc',
    });

    if (result.success && result.data) {
      setVehicles(result.data.items);
      setSlotsAvailable(null);
    } else {
      setVehicles([]);
    }
    setLoading(false);
    setRefreshing(false);
  }, []);

  useEffect(() => {
    loadVehicleLogs();
  }, [loadVehicleLogs]);

  useFocusEffect(
    useCallback(() => {
      if (!loading && guardId) loadVehicleLogs();
    }, [guardId])
  );

  const onRefresh = () => {
    setRefreshing(true);
    loadVehicleLogs(true);
  };

  const getVehicleIcon = (type: string): keyof typeof MaterialCommunityIcons.glyphMap => {
    const t = (type || '').toLowerCase();
    if (t === 'bike') return 'motorbike';
    if (t === 'truck') return 'truck';
    if (t === 'auto') return 'rickshaw';
    return 'car';
  };

  const handleVehicleExit = (vehicle: VehicleLogItem) => {
    Alert.alert(
      'Record Exit',
      `Record exit for ${vehicle.vehicleNumber}?`,
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Exit',
          onPress: async () => {
            const result = await vehicleService.recordVehicleExit(vehicle.id);
            if (result.success) {
              setVehicles(prev => prev.map(v => v.id === vehicle.id ? { ...v, status: 'out' as const, exitTime: new Date().toISOString() } : v));
              Alert.alert('Success', 'Vehicle exit recorded.');
            } else {
              Alert.alert('Error', result.error?.message ?? 'Failed to record exit.');
            }
          },
        },
      ]
    );
  };

  const handleAddVehicle = () => {
    if (!guardId || !siteId) {
      Alert.alert('No site', 'You need an assignment for today to log vehicles. Please check your deployment.');
      return;
    }
    navigation.navigate('AddVehicleEntry', { guardId, siteId });
  };

  const filteredVehicles = vehicles.filter(v => {
    const matchesSearch =
      v.vehicleNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
      v.driverName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (v.purpose && v.purpose.toLowerCase().includes(searchQuery.toLowerCase()));
    const matchesTab = activeTab === 'all' ? true : v.status === activeTab;
    return matchesSearch && matchesTab;
  });

  const inCount = vehicles.filter(v => v.status === 'in').length;
  const outCount = vehicles.filter(v => v.status === 'out').length;

  if (loading && !refreshing) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Vehicle Log</Text>
          <View style={styles.placeholder} />
        </View>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading vehicle log...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Vehicle Log</Text>
        <TouchableOpacity style={styles.addBtn} onPress={handleAddVehicle}>
          <MaterialCommunityIcons name="plus" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      <View style={styles.statsRow}>
        <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
          <MaterialCommunityIcons name="car-arrow-right" size={28} color={COLORS.success} />
          <Text style={[styles.statValue, { color: COLORS.success }]}>{inCount}</Text>
          <Text style={styles.statLabel}>Vehicles In</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.gray500 + '15' }]}>
          <MaterialCommunityIcons name="car-arrow-left" size={28} color={COLORS.gray500} />
          <Text style={[styles.statValue, { color: COLORS.gray500 }]}>{outCount}</Text>
          <Text style={styles.statLabel}>Vehicles Out</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.primaryBlue + '15' }]}>
          <MaterialCommunityIcons name="parking" size={28} color={COLORS.primaryBlue} />
          <Text style={[styles.statValue, { color: COLORS.primaryBlue }]}>{slotsAvailable ?? '—'}</Text>
          <Text style={styles.statLabel}>Slots Available</Text>
        </View>
      </View>

      <View style={styles.searchContainer}>
        <MaterialCommunityIcons name="magnify" size={20} color={COLORS.gray400} />
        <TextInput
          style={styles.searchInput}
          placeholder="Search by number or driver..."
          value={searchQuery}
          onChangeText={setSearchQuery}
          placeholderTextColor={COLORS.gray400}
        />
      </View>

      <View style={styles.tabContainer}>
        {(['in', 'out', 'all'] as const).map(tab => (
          <TouchableOpacity
            key={tab}
            style={[styles.tab, activeTab === tab && styles.tabActive]}
            onPress={() => setActiveTab(tab)}
          >
            <Text style={[styles.tabText, activeTab === tab && styles.tabTextActive]}>
              {tab === 'in' ? 'Inside' : tab === 'out' ? 'Exited' : 'All'}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.listContent}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {filteredVehicles.length === 0 ? (
          <View style={styles.emptyWrap}>
            <MaterialCommunityIcons name="car-off" size={64} color={COLORS.gray400} />
            <Text style={styles.emptyTitle}>No vehicles</Text>
            <Text style={styles.emptySubtext}>
              {vehicles.length === 0
                ? 'No vehicle entries for today. Tap + to add an entry.'
                : 'No matching vehicles for your search or filter.'}
            </Text>
            {vehicles.length === 0 && guardId && siteId && (
              <TouchableOpacity style={styles.emptyCta} onPress={handleAddVehicle}>
                <Text style={styles.emptyCtaText}>Add Vehicle Entry</Text>
              </TouchableOpacity>
            )}
          </View>
        ) : (
          filteredVehicles.map(vehicle => (
            <View key={vehicle.id} style={styles.vehicleCard}>
              <View style={[styles.vehicleIconContainer, { backgroundColor: vehicle.status === 'in' ? COLORS.success + '15' : COLORS.gray200 }]}>
                <MaterialCommunityIcons
                  name={getVehicleIcon(vehicle.vehicleType)}
                  size={28}
                  color={vehicle.status === 'in' ? COLORS.success : COLORS.gray500}
                />
              </View>
              <View style={styles.vehicleInfo}>
                <Text style={styles.vehicleNumber}>{vehicle.vehicleNumber}</Text>
                <Text style={styles.driverName}>{vehicle.driverName}</Text>
                <View style={styles.vehicleDetails}>
                  <View style={styles.detailItem}>
                    <MaterialCommunityIcons name="briefcase-outline" size={12} color={COLORS.gray500} />
                    <Text style={styles.detailText}>{vehicle.purpose}</Text>
                  </View>
                  {vehicle.parkingSlot && (
                    <View style={styles.detailItem}>
                      <MaterialCommunityIcons name="parking" size={12} color={COLORS.gray500} />
                      <Text style={styles.detailText}>{vehicle.parkingSlot}</Text>
                    </View>
                  )}
                </View>
                <View style={styles.timeRow}>
                  <Text style={styles.timeText}>In: {formatTime(vehicle.entryTime)}</Text>
                  {vehicle.exitTime && <Text style={styles.timeText}>Out: {formatTime(vehicle.exitTime)}</Text>}
                </View>
              </View>
              <View style={styles.actionContainer}>
                <View style={[styles.statusBadge, { backgroundColor: vehicle.status === 'in' ? COLORS.success + '15' : COLORS.gray200 }]}>
                  <Text style={[styles.statusText, { color: vehicle.status === 'in' ? COLORS.success : COLORS.gray500 }]}>
                    {vehicle.status === 'in' ? 'Inside' : 'Exited'}
                  </Text>
                </View>
                {vehicle.status === 'in' && (
                  <TouchableOpacity style={styles.exitBtn} onPress={() => handleVehicleExit(vehicle)}>
                    <MaterialCommunityIcons name="logout" size={18} color={COLORS.error} />
                  </TouchableOpacity>
                )}
              </View>
            </View>
          ))
        )}
      </ScrollView>

      <TouchableOpacity style={styles.fab} onPress={handleAddVehicle}>
        <MaterialCommunityIcons name="cart-plus" size={28} color={COLORS.white} />
      </TouchableOpacity>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  addBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  placeholder: { width: 40, height: 40 },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  loadingText: { marginTop: SIZES.sm, fontSize: FONTS.body, color: COLORS.textSecondary },
  statsRow: { flexDirection: 'row', paddingHorizontal: SIZES.md, paddingTop: SIZES.md, gap: SIZES.sm },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.md, borderRadius: SIZES.radiusMd },
  statValue: { fontSize: FONTS.h3, fontWeight: 'bold', marginTop: SIZES.xs },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2 },
  searchContainer: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, marginHorizontal: SIZES.md, marginTop: SIZES.md, paddingHorizontal: SIZES.md, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  searchInput: { flex: 1, height: 44, marginLeft: SIZES.sm, fontSize: FONTS.body, color: COLORS.textPrimary },
  tabContainer: { flexDirection: 'row', marginHorizontal: SIZES.md, marginTop: SIZES.md, backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusMd, padding: 4 },
  tab: { flex: 1, paddingVertical: SIZES.sm, alignItems: 'center', borderRadius: SIZES.radiusSm },
  tabActive: { backgroundColor: COLORS.white, ...SHADOWS.small },
  tabText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  tabTextActive: { color: COLORS.primary, fontWeight: '600' },
  listContent: { padding: SIZES.md, paddingBottom: 100 },
  emptyWrap: { alignItems: 'center', paddingVertical: SIZES.xl * 2 },
  emptyTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginTop: SIZES.md },
  emptySubtext: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginTop: SIZES.xs, textAlign: 'center' },
  emptyCta: { marginTop: SIZES.lg, paddingHorizontal: SIZES.lg, paddingVertical: SIZES.sm, backgroundColor: COLORS.primary, borderRadius: SIZES.radiusMd },
  emptyCtaText: { color: COLORS.white, fontWeight: '600' },
  vehicleCard: { flexDirection: 'row', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  vehicleIconContainer: { width: 56, height: 56, borderRadius: 28, justifyContent: 'center', alignItems: 'center' },
  vehicleInfo: { flex: 1, marginLeft: SIZES.sm },
  vehicleNumber: { fontSize: FONTS.body, fontWeight: '700', color: COLORS.textPrimary, letterSpacing: 1 },
  driverName: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginTop: 2 },
  vehicleDetails: { flexDirection: 'row', marginTop: SIZES.xs, gap: SIZES.sm },
  detailItem: { flexDirection: 'row', alignItems: 'center' },
  detailText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginLeft: 2 },
  timeRow: { flexDirection: 'row', marginTop: SIZES.xs, gap: SIZES.md },
  timeText: { fontSize: FONTS.tiny, color: COLORS.gray500 },
  actionContainer: { alignItems: 'flex-end', justifyContent: 'space-between' },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.tiny, fontWeight: '600' },
  exitBtn: { width: 36, height: 36, borderRadius: 18, backgroundColor: COLORS.error + '10', justifyContent: 'center', alignItems: 'center', marginTop: SIZES.xs },
  fab: { position: 'absolute', bottom: SIZES.xl, right: SIZES.md, width: 56, height: 56, borderRadius: 28, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center', ...SHADOWS.large },
});

export default VehicleLogScreen;
