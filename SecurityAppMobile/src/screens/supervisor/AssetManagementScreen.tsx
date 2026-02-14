import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, TextInput, RefreshControl, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { equipmentService, type EquipmentItem } from '../../services/equipmentService';

type Condition = 'good' | 'fair' | 'needs_repair' | 'out_of_service';

function statusToCondition(status: string): Condition {
  const s = (status || '').toLowerCase();
  if (s === 'available' || s === 'assigned') return 'good';
  if (s === 'maintenance') return 'needs_repair';
  if (s === 'damaged') return 'fair';
  if (s === 'retired') return 'out_of_service';
  return 'good';
}

function AssetManagementScreen({ navigation }: any) {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [assets, setAssets] = useState<EquipmentItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const load = useCallback(async () => {
    const res = await equipmentService.getEquipment({ pageSize: 100 });
    if (res.success && res.data?.items) {
      setAssets(res.data.items);
    } else {
      setAssets([]);
    }
  }, []);

  useEffect(() => {
    load().finally(() => setLoading(false));
  }, [load]);

  const onRefresh = async () => {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  };

  const categories = [
    { id: 'all', label: 'All', icon: 'view-grid' },
    { id: 'equipment', label: 'Equipment', icon: 'tools' },
    { id: 'communication', label: 'Comm', icon: 'radio' },
    { id: 'safety', label: 'Safety', icon: 'shield-check' },
    { id: 'uniform', label: 'Uniform', icon: 'tshirt-crew' },
    { id: 'vehicle', label: 'Vehicle', icon: 'car' },
  ];

  const getConditionColor = (condition: string) => {
    switch (condition) {
      case 'good': return COLORS.success;
      case 'fair': return COLORS.warning;
      case 'needs_repair': return COLORS.error;
      case 'out_of_service': return COLORS.gray500;
      default: return COLORS.gray500;
    }
  };

  const getConditionLabel = (condition: string) => {
    switch (condition) {
      case 'good': return 'Good';
      case 'fair': return 'Fair';
      case 'needs_repair': return 'Needs Repair';
      case 'out_of_service': return 'Out of Service';
      default: return condition;
    }
  };

  const getCategoryIcon = (category: string): keyof typeof MaterialCommunityIcons.glyphMap => {
    const c = (category || 'equipment').toLowerCase();
    switch (c) {
      case 'equipment':
      case 'other': return 'tools';
      case 'communication': return 'radio';
      case 'safety': return 'shield-check';
      case 'uniform': return 'tshirt-crew';
      case 'vehicle': return 'car';
      case 'weapon': return 'shield';
      default: return 'package-variant';
    }
  };

  const handleAssetPress = (asset: EquipmentItem) => {
    const condition = statusToCondition(asset.status);
    Alert.alert(
      asset.equipmentName,
      `Code: ${asset.equipmentCode}\nCondition: ${getConditionLabel(condition)}\n${asset.assignedToSiteName ? `Location: ${asset.assignedToSiteName}` : ''}${asset.assignedToGuardName ? `\nAssigned: ${asset.assignedToGuardName}` : ''}${asset.nextMaintenanceDate ? `\nNext Maintenance: ${asset.nextMaintenanceDate.slice(0, 10)}` : ''}`,
      [
        { text: 'Close', style: 'cancel' },
        { text: 'Edit', onPress: () => {} },
        condition === 'needs_repair' ? { text: 'Send for Repair', onPress: () => Alert.alert('Sent', 'Asset sent for repair') } : null
      ].filter(Boolean) as any
    );
  };

  const filteredAssets = assets.filter(asset => {
    const matchesSearch = asset.equipmentName.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          (asset.equipmentCode || '').toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCategory = selectedCategory === 'all' || (asset.category || '').toLowerCase() === selectedCategory;
    return matchesSearch && matchesCategory;
  });

  const stats = {
    total: assets.length,
    good: assets.filter(a => statusToCondition(a.status) === 'good').length,
    needsRepair: assets.filter(a => ['needs_repair', 'out_of_service'].includes(statusToCondition(a.status))).length,
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Asset Management</Text>
        <TouchableOpacity style={styles.addBtn}>
          <MaterialCommunityIcons name="plus" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      {/* Stats */}
      <View style={styles.statsRow}>
        <View style={[styles.statCard, { backgroundColor: COLORS.primaryBlue + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.primaryBlue }]}>{stats.total}</Text>
          <Text style={styles.statLabel}>Total Assets</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.success }]}>{stats.good}</Text>
          <Text style={styles.statLabel}>Good Condition</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.error + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.error }]}>{stats.needsRepair}</Text>
          <Text style={styles.statLabel}>Needs Attention</Text>
        </View>
      </View>

      {/* Search */}
      <View style={styles.searchContainer}>
        <MaterialCommunityIcons name="magnify" size={20} color={COLORS.gray400} />
        <TextInput
          style={styles.searchInput}
          placeholder="Search assets..."
          value={searchQuery}
          onChangeText={setSearchQuery}
          placeholderTextColor={COLORS.gray400}
        />
        <TouchableOpacity style={styles.scanBtn}>
          <MaterialCommunityIcons name="barcode-scan" size={20} color={COLORS.primary} />
        </TouchableOpacity>
      </View>

      {/* Categories */}
      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.categoryScroll} contentContainerStyle={styles.categoryContainer}>
        {categories.map((cat) => (
          <TouchableOpacity
            key={cat.id}
            style={[styles.categoryChip, selectedCategory === cat.id && styles.categoryChipActive]}
            onPress={() => setSelectedCategory(cat.id)}
          >
            <MaterialCommunityIcons 
              name={cat.icon as any} 
              size={18} 
              color={selectedCategory === cat.id ? COLORS.white : COLORS.gray500} 
            />
            <Text style={[styles.categoryText, selectedCategory === cat.id && styles.categoryTextActive]}>
              {cat.label}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      {/* Assets List */}
      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.content}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {loading ? (
          <View style={styles.loadingWrap}>
            <ActivityIndicator size="large" color={COLORS.primary} />
          </View>
        ) : filteredAssets.length === 0 ? (
          <View style={styles.emptyWrap}>
            <Text style={styles.emptyText}>No assets found</Text>
          </View>
        ) : (
        filteredAssets.map((asset) => {
          const condition = statusToCondition(asset.status);
          const cat = (asset.category || 'equipment').toLowerCase();
          return (
          <TouchableOpacity key={asset.id} style={styles.assetCard} onPress={() => handleAssetPress(asset)}>
            <View style={[styles.assetIcon, { backgroundColor: getConditionColor(condition) + '15' }]}>
              <MaterialCommunityIcons name={getCategoryIcon(cat)} size={24} color={getConditionColor(condition)} />
            </View>
            
            <View style={styles.assetInfo}>
              <View style={styles.assetHeader}>
                <Text style={styles.assetName}>{asset.equipmentName}</Text>
                <View style={[styles.conditionBadge, { backgroundColor: getConditionColor(condition) + '15' }]}>
                  <View style={[styles.conditionDot, { backgroundColor: getConditionColor(condition) }]} />
                  <Text style={[styles.conditionText, { color: getConditionColor(condition) }]}>
                    {getConditionLabel(condition)}
                  </Text>
                </View>
              </View>
              
              <Text style={styles.serialNumber}>{asset.equipmentCode}</Text>
              
              <View style={styles.assetMeta}>
                {(asset.assignedToSiteName || asset.assignedToGuardName) && (
                  <>
                    {asset.assignedToSiteName && (
                      <View style={styles.metaItem}>
                        <MaterialCommunityIcons name="map-marker" size={12} color={COLORS.gray500} />
                        <Text style={styles.metaText}>{asset.assignedToSiteName}</Text>
                      </View>
                    )}
                    {asset.assignedToGuardName && (
                      <View style={styles.metaItem}>
                        <MaterialCommunityIcons name="account" size={12} color={COLORS.gray500} />
                        <Text style={styles.metaText}>{asset.assignedToGuardName}</Text>
                      </View>
                    )}
                  </>
                )}
              </View>
            </View>

            <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
          </TouchableOpacity>
          );
        })
        )}

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
  statValue: { fontSize: FONTS.h3, fontWeight: 'bold' },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2, textAlign: 'center' },
  searchContainer: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, marginHorizontal: SIZES.md, marginTop: SIZES.md, paddingHorizontal: SIZES.md, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  searchInput: { flex: 1, height: 44, marginLeft: SIZES.sm, fontSize: FONTS.body, color: COLORS.textPrimary },
  scanBtn: { width: 36, height: 36, borderRadius: 18, backgroundColor: COLORS.primary + '10', justifyContent: 'center', alignItems: 'center' },
  categoryScroll: { marginTop: SIZES.md, maxHeight: 50 },
  categoryContainer: { paddingHorizontal: SIZES.md, gap: SIZES.sm },
  categoryChip: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusFull, gap: SIZES.xs },
  categoryChipActive: { backgroundColor: COLORS.primary },
  categoryText: { fontSize: FONTS.caption, fontWeight: '500', color: COLORS.textSecondary },
  categoryTextActive: { color: COLORS.white },
  content: { padding: SIZES.md },
  assetCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  assetIcon: { width: 48, height: 48, borderRadius: 24, justifyContent: 'center', alignItems: 'center' },
  assetInfo: { flex: 1, marginLeft: SIZES.sm },
  assetHeader: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  assetName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, flex: 1 },
  conditionBadge: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusFull },
  conditionDot: { width: 6, height: 6, borderRadius: 3, marginRight: 4 },
  conditionText: { fontSize: FONTS.tiny, fontWeight: '600' },
  serialNumber: { fontSize: FONTS.caption, color: COLORS.primaryBlue, marginTop: 2 },
  assetMeta: { flexDirection: 'row', marginTop: SIZES.xs, gap: SIZES.md },
  metaItem: { flexDirection: 'row', alignItems: 'center' },
  metaText: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginLeft: 2 },
  loadingWrap: { paddingVertical: SIZES.xl * 2, alignItems: 'center' },
  emptyWrap: { paddingVertical: SIZES.lg, alignItems: 'center' },
  emptyText: { fontSize: FONTS.body, color: COLORS.textSecondary },
});

export default AssetManagementScreen;
