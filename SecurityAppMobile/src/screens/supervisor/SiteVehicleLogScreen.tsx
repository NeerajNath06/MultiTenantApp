import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { authService } from '../../services/authService';
import { siteService } from '../../services/siteService';
import { vehicleService, VehicleLogSiteSummary } from '../../services/vehicleService';

/**
 * For Admin/Supervisor: List sites with vehicle log counts (total, in, out).
 * Tap a site to see full vehicle log detail for that site.
 */
function SiteVehicleLogScreen({ navigation }: any) {
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [siteSummaries, setSiteSummaries] = useState<VehicleLogSiteSummary[]>([]);
  const [sitesWithZero, setSitesWithZero] = useState<{ id: string; siteName: string }[]>([]);

  const loadData = useCallback(async () => {
    const user = await authService.getStoredUser();
    if (!user?.id) {
      setLoading(false);
      return;
    }
    const today = new Date().toISOString().slice(0, 10);
    const isSupervisor = (user as { isSupervisor?: boolean }).isSupervisor;

    const [sitesRes, summaryRes] = await Promise.all([
      siteService.getSites({
        supervisorId: isSupervisor ? user.id : undefined,
        pageSize: 200,
      }),
      vehicleService.getVehicleLogSummary({ dateFrom: today, dateTo: today }),
    ]);

    const sitesRaw = sitesRes.success && sitesRes.data ? sitesRes.data : {};
    const siteList = sitesRaw?.items ?? sitesRaw?.Items ?? (Array.isArray(sitesRes.data) ? sitesRes.data : []) as any[];
    const sites = (Array.isArray(siteList) ? siteList : []).map((s: any) => ({
      id: s.id ?? s.Id ?? '',
      siteName: s.siteName ?? s.SiteName ?? 'Unknown',
    })).filter((s: { id: string }) => s.id);

    const summaryList = summaryRes.success && summaryRes.data ? summaryRes.data.sites : [];
    const siteIdsSet = new Set(sites.map((s: { id: string }) => s.id));
    const filteredSummary = isSupervisor
      ? summaryList.filter((s: VehicleLogSiteSummary) => siteIdsSet.has(s.siteId))
      : summaryList;
    setSiteSummaries(filteredSummary);

    const summaryById = filteredSummary.reduce((acc: Record<string, VehicleLogSiteSummary>, s) => {
      acc[s.siteId] = s;
      return acc;
    }, {});

    const withZero = sites.map((s: { id: string; siteName: string }) => {
      const sum = summaryById[s.id];
      if (sum) return null;
      return { id: s.id, siteName: s.siteName };
    }).filter(Boolean) as { id: string; siteName: string }[];
    setSitesWithZero(withZero);

    setLoading(false);
    setRefreshing(false);
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const onRefresh = () => {
    setRefreshing(true);
    loadData();
  };

  const todayStr = new Date().toLocaleDateString('en-IN', { weekday: 'short', day: 'numeric', month: 'short' });

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backBtn}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Vehicle Log by Site</Text>
          <View style={styles.placeholder} />
        </View>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity onPress={() => navigation.goBack()} style={styles.backBtn}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Vehicle Log by Site</Text>
        <View style={styles.placeholder} />
      </View>
      <Text style={styles.subtitle}>Tap a site to see all vehicle log details</Text>
      <Text style={styles.dateLabel}>{todayStr}</Text>

      <ScrollView
        style={styles.scroll}
        contentContainerStyle={styles.scrollContent}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {siteSummaries.map((s) => (
          <TouchableOpacity
            key={s.siteId}
            style={styles.card}
            onPress={() => navigation.navigate('SiteVehicleLogDetail', { siteId: s.siteId, siteName: s.siteName })}
            activeOpacity={0.7}
          >
            <View style={styles.cardLeft}>
              <View style={styles.iconWrap}>
                <MaterialCommunityIcons name="office-building" size={28} color={COLORS.primary} />
              </View>
              <View style={styles.cardBody}>
                <Text style={styles.siteName}>{s.siteName}</Text>
                {s.siteAddress ? <Text style={styles.siteAddress} numberOfLines={1}>{s.siteAddress}</Text> : null}
                <View style={styles.statsRow}>
                  <Text style={styles.statText}><Text style={styles.statBold}>{s.totalEntries}</Text> total</Text>
                  <Text style={styles.statText}><Text style={[styles.statBold, { color: COLORS.success }]}>{s.insideCount}</Text> in</Text>
                  <Text style={styles.statText}><Text style={[styles.statBold, { color: COLORS.gray500 }]}>{s.exitedCount}</Text> out</Text>
                </View>
              </View>
            </View>
            <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
          </TouchableOpacity>
        ))}
        {sitesWithZero.map((s) => (
          <TouchableOpacity
            key={s.id}
            style={[styles.card, styles.cardZero]}
            onPress={() => navigation.navigate('SiteVehicleLogDetail', { siteId: s.id, siteName: s.siteName })}
            activeOpacity={0.7}
          >
            <View style={styles.cardLeft}>
              <View style={[styles.iconWrap, { backgroundColor: COLORS.gray100 }]}>
                <MaterialCommunityIcons name="office-building-outline" size={28} color={COLORS.gray500} />
              </View>
              <View style={styles.cardBody}>
                <Text style={styles.siteName}>{s.siteName}</Text>
                <Text style={styles.zeroText}>No vehicle logs today</Text>
              </View>
            </View>
            <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
          </TouchableOpacity>
        ))}
        {siteSummaries.length === 0 && sitesWithZero.length === 0 && (
          <View style={styles.empty}>
            <MaterialCommunityIcons name="car-off" size={48} color={COLORS.gray400} />
            <Text style={styles.emptyText}>No sites or no vehicle logs for today</Text>
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { flex: 1, fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, textAlign: 'center' },
  placeholder: { width: 40 },
  subtitle: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, paddingHorizontal: SIZES.md, marginTop: SIZES.xs },
  dateLabel: { fontSize: FONTS.caption, color: COLORS.gray500, paddingHorizontal: SIZES.md, marginTop: 2 },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  loadingText: { marginTop: SIZES.sm, color: COLORS.textSecondary },
  scroll: { flex: 1 },
  scrollContent: { padding: SIZES.md, paddingBottom: 40 },
  card: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  cardZero: { opacity: 0.9 },
  cardLeft: { flexDirection: 'row', alignItems: 'center', flex: 1 },
  iconWrap: { width: 48, height: 48, borderRadius: 24, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center', marginRight: SIZES.md },
  cardBody: { flex: 1 },
  siteName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  siteAddress: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  statsRow: { flexDirection: 'row', marginTop: SIZES.sm, gap: SIZES.md },
  statText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  statBold: { fontWeight: '700', color: COLORS.textPrimary },
  zeroText: { fontSize: FONTS.caption, color: COLORS.gray500, marginTop: 4 },
  empty: { alignItems: 'center', paddingVertical: SIZES.xl * 2 },
  emptyText: { marginTop: SIZES.sm, color: COLORS.textSecondary },
});

export default SiteVehicleLogScreen;
