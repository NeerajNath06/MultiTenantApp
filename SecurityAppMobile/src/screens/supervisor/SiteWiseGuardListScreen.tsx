import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, FlatList, TouchableOpacity, TextInput, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { SiteWiseGuardListScreenProps } from '../../types/navigation';
import { authService } from '../../services/authService';
import { guardService } from '../../services/guardService';

interface Guard {
  id: string;
  name: string;
  employeeId: string;
  site: string;
  shift: string;
  status: 'active' | 'inactive' | 'on_leave';
  phone: string;
  joinDate: string;
  rating: number;
}

const SiteWiseGuardListScreen: React.FC<SiteWiseGuardListScreenProps> = ({ navigation }) => {
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [selectedSite, setSelectedSite] = useState<string>('all');
  const [guards, setGuards] = useState<Guard[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadGuards = useCallback(async () => {
    const user = await authService.getStoredUser();
    if (!user) {
      setGuards([]);
      return;
    }
    const supervisorId = user.isSupervisor ? user.id : undefined;
    const res = await guardService.getGuards({
      supervisorId,
      pageSize: 200,
      includeInactive: false,
    });
    if (res.success && res.data?.items) {
      const list = res.data.items.map((g: any) => ({
        id: String(g.id ?? g.Id),
        name: `${g.firstName ?? g.FirstName ?? ''} ${g.lastName ?? g.LastName ?? ''}`.trim(),
        employeeId: g.guardCode ?? g.GuardCode ?? '-',
        site: g.siteName ?? g.SiteName ?? '-',
        shift: '-',
        status: ((g.isActive ?? g.IsActive) ? 'active' : 'inactive') as 'active' | 'inactive' | 'on_leave',
        phone: g.phoneNumber ?? g.PhoneNumber ?? '-',
        joinDate: '-',
        rating: 4.5,
      }));
      setGuards(list);
    } else {
      setGuards([]);
    }
  }, []);

  useEffect(() => {
    loadGuards().finally(() => setLoading(false));
  }, [loadGuards]);

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    await loadGuards();
    setRefreshing(false);
  }, [loadGuards]);

  const sites = ['all', ...Array.from(new Set(guards.map(g => g.site).filter(Boolean)))];
  const filteredGuards = guards.filter(guard => {
    const matchesSearch = guard.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                      guard.employeeId.toLowerCase().includes(searchQuery.toLowerCase()) ||
                      guard.site.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesSite = selectedSite === 'all' || guard.site.includes(selectedSite);
    return matchesSearch && matchesSite;
  });

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active': return COLORS.success;
      case 'on_leave': return COLORS.warning;
      case 'inactive': return COLORS.error;
      default: return COLORS.gray400;
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'active': return 'Active';
      case 'on_leave': return 'On Leave';
      case 'inactive': return 'Inactive';
      default: return 'Unknown';
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
          <Text style={styles.guardEmployeeId}>{item.employeeId}</Text>
          <Text style={styles.guardSite}>{item.site}</Text>
        </View>
        <View style={styles.guardActions}>
          <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) + '15' }]}>
            <Text style={[styles.statusText, { color: getStatusColor(item.status) }]}>
              {getStatusText(item.status)}
            </Text>
          </View>
          <View style={styles.actionButtons}>
            <TouchableOpacity style={styles.actionButton}>
              <MaterialCommunityIcons name="phone" size={16} color={COLORS.primary} />
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionButton}>
              <MaterialCommunityIcons name="map-marker" size={16} color={COLORS.info} />
            </TouchableOpacity>
          </View>
        </View>
      </View>
      
      <View style={styles.guardDetails}>
        <View style={styles.detailRow}>
          <MaterialCommunityIcons name="clock-outline" size={16} color={COLORS.textSecondary} />
          <Text style={styles.detailText}>{item.shift}</Text>
        </View>
        <View style={styles.detailRow}>
          <MaterialCommunityIcons name="star" size={16} color={COLORS.warning} />
          <Text style={styles.detailText}>{item.rating}/5.0</Text>
        </View>
        <View style={styles.detailRow}>
          <MaterialCommunityIcons name="calendar-outline" size={16} color={COLORS.textSecondary} />
          <Text style={styles.detailText}>Joined {item.joinDate}</Text>
        </View>
      </View>
    </Card>
  );

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Site-wise Guard List</Text>
        <View style={styles.placeholder} />
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
        {sites.map((site) => (
          <TouchableOpacity
            key={site}
            style={[styles.filterButton, selectedSite === site && styles.activeFilter]}
            onPress={() => setSelectedSite(site)}
          >
            <Text style={[styles.filterText, selectedSite === site && styles.activeFilterText]}>
              {site === 'all' ? 'All Sites' : site}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      <View style={styles.statsContainer}>
        <Card style={styles.statCard}>
          <Text style={styles.statNumber}>{guards.filter(g => g.status === 'active').length}</Text>
          <Text style={styles.statLabel}>Active Guards</Text>
        </Card>
        <Card style={styles.statCard}>
          <Text style={styles.statNumber}>{guards.filter(g => g.status === 'on_leave').length}</Text>
          <Text style={styles.statLabel}>On Leave</Text>
        </Card>
        <Card style={styles.statCard}>
          <Text style={styles.statNumber}>{guards.length}</Text>
          <Text style={styles.statLabel}>Total Guards</Text>
        </Card>
      </View>

      {loading ? (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading assigned guards...</Text>
        </View>
      ) : (
        <FlatList
          data={filteredGuards}
          keyExtractor={item => String(item.id)}
          renderItem={renderGuard}
          contentContainerStyle={styles.list}
          showsVerticalScrollIndicator={false}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
          ListEmptyComponent={
            <View style={styles.emptyContainer}>
              <MaterialCommunityIcons name="account-group-outline" size={48} color={COLORS.gray400} />
              <Text style={styles.emptyText}>No guards found</Text>
              <Text style={styles.emptySubtext}>Guards assigned to you will appear here</Text>
            </View>
          }
        />
      )}
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  loadingContainer: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: SIZES.lg },
  loadingText: { marginTop: SIZES.sm, fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  emptyContainer: { padding: SIZES.xl, alignItems: 'center' },
  emptyText: { fontSize: FONTS.h4, color: COLORS.textSecondary, marginTop: SIZES.sm },
  emptySubtext: { fontSize: FONTS.bodySmall, color: COLORS.gray400, marginTop: SIZES.xs },
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
  statsContainer: { flexDirection: 'row', paddingHorizontal: SIZES.md, marginBottom: SIZES.md, gap: SIZES.sm },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.sm },
  statNumber: { fontSize: FONTS.h3, fontWeight: 'bold', color: COLORS.textPrimary },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, textAlign: 'center', marginTop: SIZES.xs },
  list: { paddingHorizontal: SIZES.md, paddingBottom: SIZES.xl },
  guardCard: { padding: SIZES.md, marginBottom: SIZES.sm },
  guardHeader: { flexDirection: 'row', alignItems: 'flex-start' },
  avatarContainer: { width: 48, height: 48, borderRadius: 24, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center' },
  guardInfo: { flex: 1, marginLeft: SIZES.sm },
  guardName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  guardEmployeeId: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  guardSite: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginTop: 2 },
  guardActions: { alignItems: 'flex-end' },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm, marginBottom: SIZES.sm },
  statusText: { fontSize: FONTS.caption, fontWeight: '600' },
  actionButtons: { flexDirection: 'row', gap: SIZES.xs },
  actionButton: { width: 32, height: 32, borderRadius: 16, backgroundColor: COLORS.gray100, justifyContent: 'center', alignItems: 'center' },
  guardDetails: { flexDirection: 'row', marginTop: SIZES.sm, paddingTop: SIZES.sm, borderTopWidth: 1, borderTopColor: COLORS.gray100 },
  detailRow: { flex: 1, flexDirection: 'row', alignItems: 'center', gap: SIZES.xs },
  detailText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
});

export default SiteWiseGuardListScreen;
