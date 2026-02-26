import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator, RefreshControl, TextInput } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { vehicleService, VehicleLogItem } from '../../services/vehicleService';

function formatTime(iso: string): string {
  if (!iso) return '—';
  try {
    const d = new Date(iso);
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  } catch {
    return iso;
  }
}

function getVehicleIcon(type: string): keyof typeof MaterialCommunityIcons.glyphMap {
  const t = (type || '').toLowerCase();
  if (t === 'bike') return 'motorbike';
  if (t === 'truck') return 'truck';
  if (t === 'auto') return 'rickshaw';
  return 'car';
}

/**
 * Full vehicle log list for a single site (Admin/Supervisor view).
 */
function SiteVehicleLogDetailScreen({ navigation, route }: any) {
  const { siteId, siteName } = route.params ?? {};
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [vehicles, setVehicles] = useState<VehicleLogItem[]>([]);
  const [searchQuery, setSearchQuery] = useState('');

  const loadLogs = useCallback(async () => {
    if (!siteId) {
      setLoading(false);
      return;
    }
    const today = new Date().toISOString().slice(0, 10);
    const result = await vehicleService.getVehicleLogs({
      siteId,
      dateFrom: today,
      dateTo: today,
      pageSize: 200,
      sortDirection: 'desc',
    });
    if (result.success && result.data) {
      setVehicles(result.data.items);
    } else {
      setVehicles([]);
    }
    setLoading(false);
    setRefreshing(false);
  }, [siteId]);

  useEffect(() => {
    loadLogs();
  }, [loadLogs]);

  const onRefresh = () => {
    setRefreshing(true);
    loadLogs();
  };

  const filtered = vehicles.filter(
    v =>
      v.vehicleNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
      v.driverName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (v.purpose && v.purpose.toLowerCase().includes(searchQuery.toLowerCase()))
  );
  const inCount = vehicles.filter(v => v.status === 'in').length;
  const outCount = vehicles.filter(v => v.status === 'out').length;

  if (!siteId) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backBtn}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Vehicle Log</Text>
        </View>
        <Text style={styles.errorText}>Missing site. Go back and select a site.</Text>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backBtn}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle} numberOfLines={1}>{siteName || 'Vehicle Log'}</Text>
        <View style={styles.placeholder} />
      </View>

      <View style={styles.statsRow}>
        <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.success }]}>{inCount}</Text>
          <Text style={styles.statLabel}>Inside</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.gray500 + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.gray500 }]}>{outCount}</Text>
          <Text style={styles.statLabel}>Exited</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.primaryBlue + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.primaryBlue }]}>{vehicles.length}</Text>
          <Text style={styles.statLabel}>Total</Text>
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

      {loading ? (
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
        </View>
      ) : (
        <ScrollView
          style={styles.scroll}
          contentContainerStyle={styles.scrollContent}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        >
          {filtered.length === 0 ? (
            <View style={styles.empty}>
              <MaterialCommunityIcons name="car-off" size={48} color={COLORS.gray400} />
              <Text style={styles.emptyText}>
                {vehicles.length === 0 ? 'No vehicle logs for this site today.' : 'No matching vehicles.'}
              </Text>
            </View>
          ) : (
            filtered.map((v) => (
              <View key={v.id} style={styles.card}>
                <View style={[styles.iconWrap, { backgroundColor: v.status === 'in' ? COLORS.success + '15' : COLORS.gray200 }]}>
                  <MaterialCommunityIcons
                    name={getVehicleIcon(v.vehicleType)}
                    size={24}
                    color={v.status === 'in' ? COLORS.success : COLORS.gray500}
                  />
                </View>
                <View style={styles.cardBody}>
                  <Text style={styles.vehicleNumber}>{v.vehicleNumber}</Text>
                  <Text style={styles.driverName}>{v.driverName}</Text>
                  <Text style={styles.purpose}>{v.purpose}</Text>
                  <View style={styles.timeRow}>
                    <Text style={styles.timeText}>In: {formatTime(v.entryTime)}</Text>
                    {v.exitTime && <Text style={styles.timeText}>Out: {formatTime(v.exitTime)}</Text>}
                  </View>
                  {v.guardName ? <Text style={styles.guardName}>By: {v.guardName}</Text> : null}
                </View>
                <View style={[styles.badge, { backgroundColor: v.status === 'in' ? COLORS.success + '15' : COLORS.gray200 }]}>
                  <Text style={[styles.badgeText, { color: v.status === 'in' ? COLORS.success : COLORS.gray500 }]}>
                    {v.status === 'in' ? 'Inside' : 'Exited'}
                  </Text>
                </View>
              </View>
            ))
          )}
        </ScrollView>
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { flex: 1, fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, textAlign: 'center' },
  placeholder: { width: 40 },
  errorText: { padding: SIZES.md, color: COLORS.error },
  statsRow: { flexDirection: 'row', padding: SIZES.md, gap: SIZES.sm },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.sm, borderRadius: SIZES.radiusMd },
  statValue: { fontSize: FONTS.h3, fontWeight: 'bold' },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2 },
  searchContainer: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, marginHorizontal: SIZES.md, marginBottom: SIZES.sm, paddingHorizontal: SIZES.md, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  searchInput: { flex: 1, height: 44, marginLeft: SIZES.sm, fontSize: FONTS.body, color: COLORS.textPrimary },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  scroll: { flex: 1 },
  scrollContent: { padding: SIZES.md, paddingBottom: 40 },
  empty: { alignItems: 'center', paddingVertical: SIZES.xl * 2 },
  emptyText: { marginTop: SIZES.sm, color: COLORS.textSecondary },
  card: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  iconWrap: { width: 44, height: 44, borderRadius: 22, justifyContent: 'center', alignItems: 'center', marginRight: SIZES.md },
  cardBody: { flex: 1 },
  vehicleNumber: { fontSize: FONTS.body, fontWeight: '700', color: COLORS.textPrimary },
  driverName: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  purpose: { fontSize: FONTS.caption, color: COLORS.gray500 },
  timeRow: { flexDirection: 'row', marginTop: 4, gap: SIZES.md },
  timeText: { fontSize: FONTS.tiny, color: COLORS.gray500 },
  guardName: { fontSize: FONTS.tiny, color: COLORS.gray400, marginTop: 2 },
  badge: { paddingHorizontal: SIZES.sm, paddingVertical: 4, borderRadius: SIZES.radiusSm },
  badgeText: { fontSize: FONTS.tiny, fontWeight: '600' },
});

export default SiteVehicleLogDetailScreen;
