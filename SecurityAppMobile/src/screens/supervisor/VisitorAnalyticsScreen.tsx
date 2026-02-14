import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Dimensions, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { visitorService } from '../../services/visitorService';
import { authService } from '../../services/authService';

const { width } = Dimensions.get('window');

interface VisitorStat {
  label: string;
  value: number;
  change: number;
  color: string;
}

function VisitorAnalyticsScreen({ navigation }: any) {
  const [selectedPeriod, setSelectedPeriod] = useState<'today' | 'week' | 'month'>('today');
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [stats, setStats] = useState<VisitorStat[]>([
    { label: 'Total Visitors', value: 0, change: 0, color: COLORS.primaryBlue },
    { label: 'Currently Inside', value: 0, change: 0, color: COLORS.success },
    { label: 'Avg Duration', value: 0, change: 0, color: COLORS.warning },
    { label: 'Peak Hour', value: 0, change: 0, color: COLORS.secondary },
  ]);
  const [purposeData, setPurposeData] = useState<{ purpose: string; count: number; percentage: number }[]>([]);
  const [hourlyData, setHourlyData] = useState<{ hour: string; count: number }[]>([]);
  const [topHosts, setTopHosts] = useState<{ name: string; visitors: number }[]>([]);

  const getDateRange = () => {
    const now = new Date();
    let dateFrom: Date;
    let dateTo: Date;
    if (selectedPeriod === 'today') {
      dateFrom = new Date(now.getFullYear(), now.getMonth(), now.getDate());
      dateTo = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    } else if (selectedPeriod === 'week') {
      const day = now.getDay();
      const start = new Date(now);
      start.setDate(now.getDate() - (day === 0 ? 6 : day - 1));
      start.setHours(0, 0, 0, 0);
      dateFrom = start;
      dateTo = new Date(now);
    } else {
      dateFrom = new Date(now.getFullYear(), now.getMonth(), 1);
      dateTo = new Date(now);
    }
    return {
      dateFrom: dateFrom.toISOString(),
      dateTo: dateTo.toISOString(),
    };
  };

  const loadAnalytics = async () => {
    try {
      const user = await authService.getStoredUser();
      const { dateFrom, dateTo } = getDateRange();
      const result = await visitorService.getVisitorAnalytics({
        dateFrom,
        dateTo,
        supervisorId: user?.isSupervisor ? user.id : undefined,
      });
      if (result.success && result.data) {
        const d = result.data;
        setStats([
          { label: 'Total Visitors', value: d.totalVisitors ?? 0, change: 0, color: COLORS.primaryBlue },
          { label: 'Currently Inside', value: d.currentlyInside ?? 0, change: 0, color: COLORS.success },
          { label: 'Avg Duration', value: d.avgDurationMinutes ?? 0, change: 0, color: COLORS.warning },
          { label: 'Peak Hour', value: d.peakHour ?? 0, change: 0, color: COLORS.secondary },
        ]);
        setPurposeData((d.byPurpose ?? []).map(p => ({ purpose: p.purpose, count: p.count, percentage: p.percentage })));
        setHourlyData((d.byHour ?? []).map(h => ({ hour: h.hourLabel || `${h.hour}`, count: h.count })));
        setTopHosts((d.topHosts ?? []).map(h => ({ name: h.hostName, visitors: h.visitorCount })));
      }
    } catch (_) {
      setPurposeData([]);
      setHourlyData([]);
      setTopHosts([]);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    loadAnalytics();
  }, [selectedPeriod]);

  const onRefresh = () => {
    setRefreshing(true);
    loadAnalytics();
  };

  const maxHourly = hourlyData.length ? Math.max(...hourlyData.map(d => d.count), 1) : 1;

  if (loading && !refreshing) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Visitor Analytics</Text>
          <View style={styles.exportBtn} />
        </View>
        <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
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
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Visitor Analytics</Text>
        <TouchableOpacity style={styles.exportBtn}>
          <MaterialCommunityIcons name="download" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
      >
        {/* Period Selector */}
        <View style={styles.periodContainer}>
          {(['today', 'week', 'month'] as const).map((period) => (
            <TouchableOpacity
              key={period}
              style={[styles.periodBtn, selectedPeriod === period && styles.periodBtnActive]}
              onPress={() => setSelectedPeriod(period)}
            >
              <Text style={[styles.periodText, selectedPeriod === period && styles.periodTextActive]}>
                {period.charAt(0).toUpperCase() + period.slice(1)}
              </Text>
            </TouchableOpacity>
          ))}
        </View>

        {/* Stats Grid */}
        <View style={styles.statsGrid}>
          {stats.map((stat, index) => (
            <View key={index} style={styles.statCard}>
              <Text style={[styles.statValue, { color: stat.color }]}>
                {stat.label === 'Avg Duration' ? `${stat.value}m` : stat.label === 'Peak Hour' ? (stat.value <= 12 ? `${stat.value}AM` : `${stat.value - 12}PM`) : stat.value}
              </Text>
              <Text style={styles.statLabel}>{stat.label}</Text>
              {stat.change !== 0 && (
                <View style={[styles.changeBadge, { backgroundColor: stat.change > 0 ? COLORS.success + '15' : COLORS.error + '15' }]}>
                  <MaterialCommunityIcons 
                    name={stat.change > 0 ? 'trending-up' : 'trending-down'} 
                    size={12} 
                    color={stat.change > 0 ? COLORS.success : COLORS.error} 
                  />
                  <Text style={[styles.changeText, { color: stat.change > 0 ? COLORS.success : COLORS.error }]}>
                    {Math.abs(stat.change)}%
                  </Text>
                </View>
              )}
            </View>
          ))}
        </View>

        {/* Hourly Chart */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Visitor Flow by Hour</Text>
          <View style={styles.chartCard}>
            <View style={styles.barChart}>
              {(hourlyData.length ? hourlyData : [{ hour: '—', count: 0 }]).map((data, index) => (
                <View key={index} style={styles.barContainer}>
                  <View style={[styles.bar, { height: Math.max(4, (data.count / maxHourly) * 100), backgroundColor: data.count === maxHourly && maxHourly > 0 ? COLORS.primaryBlue : COLORS.gray300 }]} />
                  <Text style={styles.barLabel}>{data.hour}</Text>
                </View>
              ))}
            </View>
          </View>
        </View>

        {/* Purpose Breakdown */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Visit Purpose</Text>
          <View style={styles.chartCard}>
            {(purposeData.length ? purposeData : [{ purpose: 'No data', count: 0, percentage: 0 }]).map((item, index) => (
              <View key={index} style={styles.purposeRow}>
                <View style={styles.purposeInfo}>
                  <Text style={styles.purposeName}>{item.purpose}</Text>
                  <Text style={styles.purposeCount}>{item.count} visitors</Text>
                </View>
                <View style={styles.purposeBar}>
                  <View style={[styles.purposeBarFill, { width: `${item.percentage}%` }]} />
                </View>
                <Text style={styles.purposePercent}>{item.percentage}%</Text>
              </View>
            ))}
          </View>
        </View>

        {/* Top Hosts */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Top Hosts</Text>
          <View style={styles.chartCard}>
            {(topHosts.length ? topHosts : [{ name: 'No data', visitors: 0 }]).map((host, index) => (
              <View key={index} style={styles.hostRow}>
                <View style={styles.rankBadge}>
                  <Text style={styles.rankText}>{index + 1}</Text>
                </View>
                <Text style={styles.hostName}>{host.name}</Text>
                <View style={styles.hostStats}>
                  <MaterialCommunityIcons name="account-group" size={16} color={COLORS.gray500} />
                  <Text style={styles.hostCount}>{host.visitors}</Text>
                </View>
              </View>
            ))}
          </View>
        </View>

        {/* Quick Insights */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Insights</Text>
          <View style={styles.insightCard}>
            <MaterialCommunityIcons name="lightbulb" size={24} color={COLORS.warning} />
            <View style={styles.insightContent}>
              <Text style={styles.insightTitle}>Peak Hours Alert</Text>
              <Text style={styles.insightText}>Visitor traffic peaks between 10AM-12PM. Consider adding an extra guard at reception during these hours.</Text>
            </View>
          </View>
          <View style={styles.insightCard}>
            <MaterialCommunityIcons name="trending-up" size={24} color={COLORS.success} />
            <View style={styles.insightContent}>
              <Text style={styles.insightTitle}>Weekly Trend</Text>
              <Text style={styles.insightText}>Visitor count increased by 12% compared to last week. Meetings are the primary purpose.</Text>
            </View>
          </View>
        </View>

        <View style={{ height: 50 }} />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.primary },
  backBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.white },
  exportBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  periodContainer: { flexDirection: 'row', marginHorizontal: SIZES.md, marginTop: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: 4, ...SHADOWS.small },
  periodBtn: { flex: 1, paddingVertical: SIZES.sm, alignItems: 'center', borderRadius: SIZES.radiusSm },
  periodBtnActive: { backgroundColor: COLORS.primary },
  periodText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  periodTextActive: { color: COLORS.white, fontWeight: '600' },
  statsGrid: { flexDirection: 'row', flexWrap: 'wrap', paddingHorizontal: SIZES.md, marginTop: SIZES.md, gap: SIZES.sm },
  statCard: { width: (width - 48) / 2, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, alignItems: 'center', ...SHADOWS.small },
  statValue: { fontSize: FONTS.h2, fontWeight: 'bold' },
  statLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 4 },
  changeBadge: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusFull, marginTop: SIZES.xs, gap: 2 },
  changeText: { fontSize: FONTS.tiny, fontWeight: '600' },
  section: { padding: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  chartCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  barChart: { flexDirection: 'row', alignItems: 'flex-end', justifyContent: 'space-between', height: 120 },
  barContainer: { alignItems: 'center', flex: 1 },
  bar: { width: 20, borderRadius: 4, minHeight: 4 },
  barLabel: { fontSize: 8, color: COLORS.textSecondary, marginTop: 4 },
  purposeRow: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.sm },
  purposeInfo: { width: 100 },
  purposeName: { fontSize: FONTS.bodySmall, fontWeight: '500', color: COLORS.textPrimary },
  purposeCount: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  purposeBar: { flex: 1, height: 8, backgroundColor: COLORS.gray200, borderRadius: 4, marginHorizontal: SIZES.sm },
  purposeBarFill: { height: '100%', backgroundColor: COLORS.primaryBlue, borderRadius: 4 },
  purposePercent: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary, width: 40, textAlign: 'right' },
  hostRow: { flexDirection: 'row', alignItems: 'center', paddingVertical: SIZES.sm, borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  rankBadge: { width: 24, height: 24, borderRadius: 12, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  rankText: { fontSize: FONTS.caption, fontWeight: 'bold', color: COLORS.white },
  hostName: { flex: 1, fontSize: FONTS.bodySmall, color: COLORS.textPrimary, marginLeft: SIZES.sm },
  hostStats: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  hostCount: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary },
  insightCard: { flexDirection: 'row', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  insightContent: { flex: 1, marginLeft: SIZES.sm },
  insightTitle: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary },
  insightText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 4, lineHeight: 18 },
});

export default VisitorAnalyticsScreen;
