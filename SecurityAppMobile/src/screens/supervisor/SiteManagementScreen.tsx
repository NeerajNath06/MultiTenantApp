import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, TextInput, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { authService } from '../../services/authService';
import { siteService } from '../../services/siteService';

interface Site {
  id: number | string;
  name: string;
  address: string;
  type: 'office' | 'residential' | 'industrial' | 'commercial';
  totalGuards: number;
  activeGuards: number;
  checkpoints: number;
  status: 'active' | 'inactive';
  contactPerson: string;
  contactPhone: string;
}

function SiteManagementScreen({ navigation }: any) {
  const [searchQuery, setSearchQuery] = useState('');
  const [activeFilter, setActiveFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [sites, setSites] = useState<Site[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadSites = useCallback(async () => {
    const user = await authService.getStoredUser();
    const opts: { pageSize: number; supervisorId?: string } = { pageSize: 100 };
    if (user?.isSupervisor && user?.id) opts.supervisorId = user.id;
    const res = await siteService.getSites(opts);
    if (res.success && res.data) {
      const raw = res.data as { items?: any[]; Items?: any[] };
      const list = raw.items ?? raw.Items ?? [];
      const mapped: Site[] = list.map((s: any, idx: number) => ({
        id: s.id ?? s.Id ?? idx,
        name: s.siteName ?? s.SiteName ?? '—',
        address: [s.address ?? s.Address, s.city ?? s.City].filter(Boolean).join(', ') || '—',
        type: 'office' as const,
        totalGuards: s.guardCount ?? s.GuardCount ?? 0,
        activeGuards: s.guardCount ?? s.GuardCount ?? 0,
        checkpoints: 0,
        status: (s.isActive ?? s.IsActive) !== false ? 'active' : 'inactive',
        contactPerson: s.contactPerson ?? s.ContactPerson ?? '—',
        contactPhone: s.contactPhone ?? s.ContactPhone ?? '—',
      }));
      setSites(mapped);
    } else {
      setSites([]);
    }
    setLoading(false);
    setRefreshing(false);
  }, []);

  useEffect(() => {
    loadSites();
  }, [loadSites]);

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadSites();
  }, [loadSites]);

  const getSiteTypeIcon = (type: string): keyof typeof MaterialCommunityIcons.glyphMap => {
    switch (type) {
      case 'office': return 'office-building';
      case 'residential': return 'home-city';
      case 'industrial': return 'factory';
      case 'commercial': return 'store';
      default: return 'map-marker';
    }
  };

  const getSiteTypeColor = (type: string) => {
    switch (type) {
      case 'office': return COLORS.primaryBlue;
      case 'residential': return COLORS.success;
      case 'industrial': return COLORS.warning;
      case 'commercial': return COLORS.secondary;
      default: return COLORS.gray500;
    }
  };

  const handleSitePress = (site: Site) => {
    Alert.alert(
      site.name,
      `Address: ${site.address}\n\nContact: ${site.contactPerson}\nPhone: ${site.contactPhone}\n\nGuards: ${site.activeGuards}/${site.totalGuards}\nCheckpoints: ${site.checkpoints}`,
      [
        { text: 'Close', style: 'cancel' },
        { text: 'View Guards', onPress: () => navigation.navigate('SiteWiseGuardList') },
        { text: 'Edit Site', onPress: () => console.log('Edit') }
      ]
    );
  };

  const filteredSites = sites.filter(site => {
    const matchesSearch = site.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          site.address.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesFilter = activeFilter === 'all' ? true : site.status === activeFilter;
    return matchesSearch && matchesFilter;
  });

  const totalGuards = sites.reduce((sum, s) => sum + s.totalGuards, 0);
  const activeGuardsCount = sites.reduce((sum, s) => sum + s.activeGuards, 0);
  const totalCheckpoints = sites.reduce((sum, s) => sum + s.checkpoints, 0);

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Site Management</Text>
          <View style={styles.addBtn} />
        </View>
        <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={{ marginTop: SIZES.md, color: COLORS.textSecondary }}>Loading sites...</Text>
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
        <Text style={styles.headerTitle}>Site Management</Text>
        <TouchableOpacity style={styles.addBtn}>
          <MaterialCommunityIcons name="plus" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      {/* Stats */}
      <View style={styles.statsRow}>
        <View style={[styles.statCard, { backgroundColor: COLORS.primaryBlue + '15' }]}>
          <MaterialCommunityIcons name="map-marker-multiple" size={24} color={COLORS.primaryBlue} />
          <Text style={[styles.statValue, { color: COLORS.primaryBlue }]}>{sites.length}</Text>
          <Text style={styles.statLabel}>Total Sites</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
          <MaterialCommunityIcons name="account-group" size={24} color={COLORS.success} />
          <Text style={[styles.statValue, { color: COLORS.success }]}>{activeGuardsCount}/{totalGuards}</Text>
          <Text style={styles.statLabel}>Guards Active</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.warning + '15' }]}>
          <MaterialCommunityIcons name="qrcode-scan" size={24} color={COLORS.warning} />
          <Text style={[styles.statValue, { color: COLORS.warning }]}>{totalCheckpoints}</Text>
          <Text style={styles.statLabel}>Checkpoints</Text>
        </View>
      </View>

      {/* Search */}
      <View style={styles.searchContainer}>
        <MaterialCommunityIcons name="magnify" size={20} color={COLORS.gray400} />
        <TextInput
          style={styles.searchInput}
          placeholder="Search sites..."
          value={searchQuery}
          onChangeText={setSearchQuery}
          placeholderTextColor={COLORS.gray400}
        />
      </View>

      {/* Filter */}
      <View style={styles.filterContainer}>
        {(['all', 'active', 'inactive'] as const).map((filter) => (
          <TouchableOpacity
            key={filter}
            style={[styles.filterChip, activeFilter === filter && styles.filterChipActive]}
            onPress={() => setActiveFilter(filter)}
          >
            <Text style={[styles.filterText, activeFilter === filter && styles.filterTextActive]}>
              {filter.charAt(0).toUpperCase() + filter.slice(1)}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      {/* Sites List */}
      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.content}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {filteredSites.map((site) => (
          <TouchableOpacity key={site.id} style={styles.siteCard} onPress={() => handleSitePress(site)}>
            <View style={[styles.siteIcon, { backgroundColor: getSiteTypeColor(site.type) + '15' }]}>
              <MaterialCommunityIcons name={getSiteTypeIcon(site.type)} size={28} color={getSiteTypeColor(site.type)} />
            </View>

            <View style={styles.siteInfo}>
              <View style={styles.siteHeader}>
                <Text style={styles.siteName}>{site.name}</Text>
                <View style={[styles.statusBadge, { backgroundColor: site.status === 'active' ? COLORS.success + '15' : COLORS.gray200 }]}>
                  <View style={[styles.statusDot, { backgroundColor: site.status === 'active' ? COLORS.success : COLORS.gray400 }]} />
                  <Text style={[styles.statusText, { color: site.status === 'active' ? COLORS.success : COLORS.gray500 }]}>
                    {site.status.charAt(0).toUpperCase() + site.status.slice(1)}
                  </Text>
                </View>
              </View>

              <View style={styles.addressRow}>
                <MaterialCommunityIcons name="map-marker-outline" size={14} color={COLORS.gray500} />
                <Text style={styles.addressText}>{site.address}</Text>
              </View>

              <View style={styles.siteStats}>
                <View style={styles.siteStat}>
                  <MaterialCommunityIcons name="account" size={14} color={COLORS.gray500} />
                  <Text style={styles.siteStatText}>{site.activeGuards}/{site.totalGuards} Guards</Text>
                </View>
                <View style={styles.siteStat}>
                  <MaterialCommunityIcons name="qrcode" size={14} color={COLORS.gray500} />
                  <Text style={styles.siteStatText}>{site.checkpoints} Checkpoints</Text>
                </View>
              </View>
            </View>

            <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
          </TouchableOpacity>
        ))}

        <View style={{ height: 50 }} />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  addBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  statsRow: { flexDirection: 'row', paddingHorizontal: SIZES.md, paddingTop: SIZES.md, gap: SIZES.sm },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.md, borderRadius: SIZES.radiusMd },
  statValue: { fontSize: FONTS.h4, fontWeight: 'bold', marginTop: SIZES.xs },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2, textAlign: 'center' },
  searchContainer: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, marginHorizontal: SIZES.md, marginTop: SIZES.md, paddingHorizontal: SIZES.md, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  searchInput: { flex: 1, height: 44, marginLeft: SIZES.sm, fontSize: FONTS.body, color: COLORS.textPrimary },
  filterContainer: { flexDirection: 'row', paddingHorizontal: SIZES.md, marginTop: SIZES.md, gap: SIZES.sm },
  filterChip: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusFull },
  filterChipActive: { backgroundColor: COLORS.primary },
  filterText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  filterTextActive: { color: COLORS.white },
  content: { padding: SIZES.md },
  siteCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  siteIcon: { width: 56, height: 56, borderRadius: 28, justifyContent: 'center', alignItems: 'center' },
  siteInfo: { flex: 1, marginLeft: SIZES.sm },
  siteHeader: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  siteName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, flex: 1 },
  statusBadge: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusFull },
  statusDot: { width: 6, height: 6, borderRadius: 3, marginRight: 4 },
  statusText: { fontSize: FONTS.tiny, fontWeight: '600' },
  addressRow: { flexDirection: 'row', alignItems: 'center', marginTop: 4 },
  addressText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginLeft: 4 },
  siteStats: { flexDirection: 'row', marginTop: SIZES.xs, gap: SIZES.md },
  siteStat: { flexDirection: 'row', alignItems: 'center' },
  siteStatText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginLeft: 4 },
});

export default SiteManagementScreen;
