import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, FlatList, TouchableOpacity, TextInput, ActivityIndicator, RefreshControl, Alert } from 'react-native';
import * as Sharing from 'expo-sharing';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { LiveAttendanceScreenProps } from '../../types/navigation';
import { authService } from '../../services/authService';
import { guardService } from '../../services/guardService';
import { attendanceService } from '../../services/attendanceService';

interface Guard {
  id: string;
  name: string;
  site: string;
  status: 'present' | 'late' | 'absent';
  checkIn: string;
  lastLocation: string;
}

interface FilterType {
  id: string;
  label: string;
}

function formatTime(iso?: string | null): string {
  if (!iso) return '—';
  try {
    const d = new Date(iso);
    return d.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true });
  } catch {
    return '—';
  }
}

const LiveAttendanceScreen: React.FC<LiveAttendanceScreenProps> = ({ navigation }) => {
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [filter, setFilter] = useState<string>('all');
  const [guards, setGuards] = useState<Guard[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [exporting, setExporting] = useState(false);

  const loadData = useCallback(async () => {
    const user = await authService.getStoredUser();
    const supervisorId = user?.isSupervisor ? user.id : undefined;
    const todayStr = new Date().toISOString().split('T')[0];

    const [guardsRes, attendanceRes] = await Promise.all([
      guardService.getGuards({ supervisorId, pageSize: 200, includeInactive: false }),
      attendanceService.getAttendanceList({ startDate: todayStr, endDate: todayStr, pageNumber: 1, pageSize: 500 }),
    ]);

    const guardList = guardsRes.success && guardsRes.data?.items ? guardsRes.data.items : [];
    const rawAtt = attendanceRes.success && attendanceRes.data ? attendanceRes.data : {};
    const attItems = rawAtt?.items ?? rawAtt?.Items ?? (Array.isArray(rawAtt) ? rawAtt : []);
    const attByGuard = (attItems as any[]).reduce((acc: Record<string, any>, a: any) => {
      const gId = (a.guardId ?? a.GuardId)?.toString?.() ?? '';
      if (gId && (!acc[gId] || (a.checkInTime ?? a.CheckInTime))) acc[gId] = a;
      return acc;
    }, {});

    const list: Guard[] = guardList.map((g: any) => {
      const gId = String(g.id ?? g.Id);
      const att = attByGuard[gId];
      const hasCheckIn = att && (att.checkInTime ?? att.CheckInTime);
      const checkInStr = hasCheckIn ? formatTime(att.checkInTime ?? att.CheckInTime) : '—';
      const status: 'present' | 'late' | 'absent' = hasCheckIn ? 'present' : 'absent';
      return {
        id: gId,
        name: `${g.firstName ?? g.FirstName ?? ''} ${g.lastName ?? g.LastName ?? ''}`.trim() || (g.guardCode ?? g.GuardCode ?? '—'),
        site: g.siteName ?? g.SiteName ?? '—',
        status,
        checkIn: checkInStr,
        lastLocation: hasCheckIn ? 'On site' : '—',
      };
    });
    setGuards(list);
    setLoading(false);
    setRefreshing(false);
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const filters: FilterType[] = [
    { id: 'all', label: 'All' },
    { id: 'present', label: 'Present' },
    { id: 'late', label: 'Late' },
    { id: 'absent', label: 'Absent' },
  ];

  const filteredGuards = guards.filter(g => {
    const matchesSearch = g.name.toLowerCase().includes(searchQuery.toLowerCase()) || (g.site || '').toLowerCase().includes(searchQuery.toLowerCase());
    const matchesFilter = filter === 'all' || g.status === filter;
    return matchesSearch && matchesFilter;
  });

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadData();
  }, [loadData]);

  const handleExport = useCallback(async (format: 'csv' | 'xlsx' | 'pdf') => {
    setExporting(true);
    try {
      const todayStr = new Date().toISOString().split('T')[0];
      const res = await attendanceService.exportAttendance(format, {
        startDate: todayStr,
        endDate: todayStr,
        sortBy: 'date',
        sortDirection: 'desc',
      });
      if (!res.success) {
        Alert.alert('Export Failed', res.error ?? 'Could not export attendance.');
        return;
      }
      if (res.localUri && res.fileName) {
        const canShare = await Sharing.isAvailableAsync();
        if (canShare) {
          await Sharing.shareAsync(res.localUri, {
            mimeType: res.mimeType ?? 'text/csv',
            dialogTitle: `Attendance Report (${format.toUpperCase()}) - Open or share`,
          });
        } else {
          Alert.alert('Report Downloaded', `File saved: ${res.fileName}. Open from your device to view.`);
        }
      }
    } catch (e) {
      Alert.alert('Export Failed', (e as Error).message);
    } finally {
      setExporting(false);
    }
  }, []);

  const getStatusColor = (status: string): string => {
    switch (status) {
      case 'present': return COLORS.success;
      case 'late': return COLORS.warning;
      case 'absent': return COLORS.error;
      default: return COLORS.gray400;
    }
  };

  const renderGuard = ({ item }: { item: Guard }) => (
    <Card style={styles.guardCard}>
      <View style={styles.guardHeader}>
        <View style={styles.avatarContainer}>
          <MaterialCommunityIcons name="account" size={24} color={COLORS.primary} />
        </View>
        <View style={styles.guardInfo}>
          <Text style={styles.guardName}>{item.name}</Text>
          <Text style={styles.guardSite}>{item.site}</Text>
        </View>
        <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) + '15' }]}>
          <View style={[styles.statusDot, { backgroundColor: getStatusColor(item.status) }]} />
          <Text style={[styles.statusText, { color: getStatusColor(item.status) }]}>
            {item.status.charAt(0).toUpperCase() + item.status.slice(1)}
          </Text>
        </View>
      </View>
      <View style={styles.guardDetails}>
        <View style={styles.detailItem}>
          <MaterialCommunityIcons name="clock-outline" size={16} color={COLORS.textSecondary} />
          <Text style={styles.detailText}>Check-in: {item.checkIn}</Text>
        </View>
        <View style={styles.detailItem}>
          <MaterialCommunityIcons name="map-marker-outline" size={16} color={COLORS.textSecondary} />
          <Text style={styles.detailText}>Last seen: {item.lastLocation}</Text>
        </View>
      </View>
    </Card>
  );

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Live Attendance</Text>
          <View style={styles.placeholder} />
        </View>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading attendance...</Text>
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
        <Text style={styles.headerTitle}>Live Attendance</Text>
        <TouchableOpacity
          style={styles.placeholder}
          onPress={() => Alert.alert('Download Report', 'Choose format (agency header + guard details)', [
            { text: 'Cancel', style: 'cancel' },
            { text: 'Download as CSV', onPress: () => handleExport('csv') },
            { text: 'Download as Excel', onPress: () => handleExport('xlsx') },
            { text: 'Download as PDF', onPress: () => handleExport('pdf') },
          ])}
          disabled={exporting}
        >
          {exporting ? (
            <ActivityIndicator size="small" color={COLORS.primary} />
          ) : (
            <MaterialCommunityIcons name="file-download" size={24} color={COLORS.primary} />
          )}
        </TouchableOpacity>
      </View>

      <View style={styles.searchContainer}>
        <MaterialCommunityIcons name="magnify" size={20} color={COLORS.gray400} />
        <TextInput
          style={styles.searchInput}
          placeholder="Search guards..."
          value={searchQuery}
          onChangeText={setSearchQuery}
          placeholderTextColor={COLORS.gray400}
        />
      </View>

      <View style={styles.filterRow}>
        {filters.map((f) => (
          <TouchableOpacity
            key={f.id}
            style={[styles.filterButton, filter === f.id && styles.activeFilter]}
            onPress={() => setFilter(f.id)}
          >
            <Text style={[styles.filterText, filter === f.id && styles.activeFilterText]}>
              {f.label}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      <FlatList
        data={filteredGuards}
        keyExtractor={item => item.id}
        renderItem={renderGuard}
        contentContainerStyle={styles.list}
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        ListEmptyComponent={<Text style={styles.emptyText}>No guards to show. Assign guards from the web portal.</Text>}
      />
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  searchContainer: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, margin: SIZES.md, paddingHorizontal: SIZES.md, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  searchInput: { flex: 1, height: 44, marginLeft: SIZES.sm, fontSize: FONTS.body, color: COLORS.textPrimary },
  filterRow: { flexDirection: 'row', paddingHorizontal: SIZES.md, marginBottom: SIZES.sm, gap: SIZES.sm },
  filterButton: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.white, borderRadius: SIZES.radiusFull, ...SHADOWS.small },
  activeFilter: { backgroundColor: COLORS.primary },
  filterText: { fontSize: FONTS.caption, color: COLORS.textSecondary, fontWeight: '500' },
  activeFilterText: { color: COLORS.white },
  list: { paddingHorizontal: SIZES.md, paddingBottom: SIZES.xl },
  guardCard: { padding: SIZES.md, marginBottom: SIZES.sm },
  guardHeader: { flexDirection: 'row', alignItems: 'center' },
  avatarContainer: { width: 48, height: 48, borderRadius: 24, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center' },
  guardInfo: { flex: 1, marginLeft: SIZES.sm },
  guardName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  guardSite: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  statusBadge: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusFull, gap: 4 },
  statusDot: { width: 8, height: 8, borderRadius: 4 },
  statusText: { fontSize: FONTS.caption, fontWeight: '600' },
  guardDetails: { flexDirection: 'row', marginTop: SIZES.sm, paddingTop: SIZES.sm, borderTopWidth: 1, borderTopColor: COLORS.gray100 },
  detailItem: { flex: 1, flexDirection: 'row', alignItems: 'center', gap: 4 },
  detailText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: SIZES.xl },
  loadingText: { marginTop: SIZES.md, fontSize: FONTS.body, color: COLORS.textSecondary },
  emptyText: { textAlign: 'center', padding: SIZES.xl, color: COLORS.textSecondary, fontSize: FONTS.body },
});

export default LiveAttendanceScreen;
