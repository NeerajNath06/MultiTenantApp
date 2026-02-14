import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, TextInput, Image, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { visitorService } from '../../services/visitorService';
import { authService } from '../../services/authService';

interface Visitor {
  id: number;
  name: string;
  phone: string;
  purpose: string;
  hostName: string;
  checkIn: string;
  checkOut?: string;
  photo?: string;
  idType: string;
  idNumber: string;
  status: 'checked_in' | 'checked_out' | 'expected';
  badge: string;
}

function VisitorManagementScreen({ navigation }: any) {
  const [activeTab, setActiveTab] = useState<'current' | 'expected' | 'history'>('current');
  const [searchQuery, setSearchQuery] = useState('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [visitors, setVisitors] = useState<Visitor[]>([]);

  useEffect(() => {
    loadVisitors();
  }, []);

  const loadVisitors = async () => {
    try {
      setLoading(true);
      const user = await authService.getStoredUser();
      if (!user) return;

      const today = new Date();
      today.setHours(0, 0, 0, 0);
      
      const guardIdForApi = (user as { guardId?: string }).guardId || user.id;
      const result = await visitorService.getVisitors({
        guardId: guardIdForApi,
        dateFrom: today.toISOString(),
        pageSize: 100,
      });

      if (result.success && result.data) {
        const raw = result.data as { items?: any[]; data?: any[] };
        const visitorsData = Array.isArray(result.data)
          ? result.data
          : (raw?.items ?? raw?.data ?? []);

        const mappedVisitors = visitorsData.map((v: any, index: number) => ({
          id: v.id || index + 1,
          name: v.visitorName || 'Unknown',
          phone: v.phoneNumber || '',
          purpose: v.purpose || '',
          hostName: v.hostName || 'N/A',
          checkIn: v.entryTime ? new Date(v.entryTime).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) : '',
          checkOut: v.exitTime ? new Date(v.exitTime).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) : undefined,
          idType: v.idProofType || 'N/A',
          idNumber: v.idProofNumber || '',
          status: v.exitTime ? 'checked_out' as const : (v.entryTime ? 'checked_in' as const : 'expected' as const),
          badge: `V-${String(index + 1).padStart(3, '0')}`,
        }));

        setVisitors(mappedVisitors);
      }
    } catch (error) {
      console.error('Error loading visitors:', error);
      Alert.alert('Error', 'Failed to load visitors');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const onRefresh = () => {
    setRefreshing(true);
    loadVisitors();
  };

  const handleAddVisitor = () => {
    Alert.alert('Add Visitor', 'Visitor registration form will open');
    navigation.navigate('AddVisitor');
  };

  const handleCheckOut = (visitorId: number) => {
    const now = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    setVisitors(visitors.map(v => 
      v.id === visitorId ? { ...v, status: 'checked_out' as const, checkOut: now } : v
    ));
    Alert.alert('Checked Out', 'Visitor has been checked out successfully.');
  };

  const handleVisitorPress = (visitor: Visitor) => {
    Alert.alert(
      visitor.name,
      `Phone: ${visitor.phone}\nPurpose: ${visitor.purpose}\nHost: ${visitor.hostName}\nID: ${visitor.idType} - ${visitor.idNumber}\nBadge: ${visitor.badge}\n\nCheck-in: ${visitor.checkIn || 'Expected'}${visitor.checkOut ? `\nCheck-out: ${visitor.checkOut}` : ''}`,
      [
        { text: 'Close', style: 'cancel' },
        ...(visitor.status === 'checked_in' ? [{ text: 'Check Out', onPress: () => handleCheckOut(visitor.id) }] : []),
      ]
    );
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'checked_in': return COLORS.success;
      case 'checked_out': return COLORS.gray500;
      case 'expected': return COLORS.warning;
      default: return COLORS.gray400;
    }
  };

  const filteredVisitors = visitors.filter(v => {
    const matchesSearch = v.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          v.phone.includes(searchQuery);
    const matchesTab = activeTab === 'current' ? v.status === 'checked_in' :
                       activeTab === 'expected' ? v.status === 'expected' :
                       v.status === 'checked_out';
    return matchesSearch && matchesTab;
  });

  const currentCount = visitors.filter(v => v.status === 'checked_in').length;
  const expectedCount = visitors.filter(v => v.status === 'expected').length;
  const todayTotal = visitors.length;

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Visitor Management</Text>
        <TouchableOpacity style={styles.addBtn} onPress={handleAddVisitor}>
          <MaterialCommunityIcons name="plus" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      <ScrollView 
        showsVerticalScrollIndicator={false} 
        contentContainerStyle={styles.content}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />
        }
      >
        {loading ? (
          <View style={styles.loadingContainer}>
            <ActivityIndicator size="large" color={COLORS.primary} />
            <Text style={styles.loadingText}>Loading visitors...</Text>
          </View>
        ) : (
          <>
            {/* Stats Cards */}
            <View style={styles.statsRow}>
        <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
          <MaterialCommunityIcons name="account-check" size={24} color={COLORS.success} />
          <Text style={[styles.statValue, { color: COLORS.success }]}>{currentCount}</Text>
          <Text style={styles.statLabel}>Currently In</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.warning + '15' }]}>
          <MaterialCommunityIcons name="account-clock" size={24} color={COLORS.warning} />
          <Text style={[styles.statValue, { color: COLORS.warning }]}>{expectedCount}</Text>
          <Text style={styles.statLabel}>Expected</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.primaryBlue + '15' }]}>
          <MaterialCommunityIcons name="account-group" size={24} color={COLORS.primaryBlue} />
          <Text style={[styles.statValue, { color: COLORS.primaryBlue }]}>{todayTotal}</Text>
          <Text style={styles.statLabel}>Today Total</Text>
            </View>
          </View>

          {/* Search */}
          <View style={styles.searchContainer}>
        <MaterialCommunityIcons name="magnify" size={20} color={COLORS.gray400} />
        <TextInput
          style={styles.searchInput}
          placeholder="Search visitors..."
          value={searchQuery}
          onChangeText={setSearchQuery}
          placeholderTextColor={COLORS.gray400}
        />
        {searchQuery.length > 0 && (
          <TouchableOpacity onPress={() => setSearchQuery('')}>
            <MaterialCommunityIcons name="close-circle" size={20} color={COLORS.gray400} />
          </TouchableOpacity>
            )}
          </View>

          {/* Tabs */}
          <View style={styles.tabContainer}>
        {(['current', 'expected', 'history'] as const).map((tab) => (
          <TouchableOpacity
            key={tab}
            style={[styles.tab, activeTab === tab && styles.tabActive]}
            onPress={() => setActiveTab(tab)}
          >
            <Text style={[styles.tabText, activeTab === tab && styles.tabTextActive]}>
              {tab === 'current' ? 'Currently In' : tab === 'expected' ? 'Expected' : 'History'}
            </Text>
          </TouchableOpacity>
            ))}
          </View>

            {/* Visitors List */}
            {filteredVisitors.length === 0 ? (
          <View style={styles.emptyState}>
            <MaterialCommunityIcons name="account-off" size={64} color={COLORS.gray300} />
            <Text style={styles.emptyTitle}>No visitors found</Text>
            <Text style={styles.emptyText}>
              {activeTab === 'current' ? 'No visitors currently checked in' :
               activeTab === 'expected' ? 'No expected visitors' : 'No visitor history'}
            </Text>
          </View>
        ) : (
          filteredVisitors.map((visitor) => (
            <TouchableOpacity
              key={visitor.id}
              style={styles.visitorCard}
              onPress={() => handleVisitorPress(visitor)}
            >
              <View style={styles.visitorAvatar}>
                <Text style={styles.avatarText}>{visitor.name.split(' ').map(n => n[0]).join('')}</Text>
              </View>
              
              <View style={styles.visitorInfo}>
                <View style={styles.visitorHeader}>
                  <Text style={styles.visitorName}>{visitor.name}</Text>
                  <View style={[styles.statusBadge, { backgroundColor: getStatusColor(visitor.status) + '15' }]}>
                    <View style={[styles.statusDot, { backgroundColor: getStatusColor(visitor.status) }]} />
                    <Text style={[styles.statusText, { color: getStatusColor(visitor.status) }]}>
                      {visitor.status.replace('_', ' ')}
                    </Text>
                  </View>
                </View>
                
                <View style={styles.visitorDetails}>
                  <View style={styles.detailItem}>
                    <MaterialCommunityIcons name="briefcase-outline" size={14} color={COLORS.gray500} />
                    <Text style={styles.detailText}>{visitor.purpose}</Text>
                  </View>
                  <View style={styles.detailItem}>
                    <MaterialCommunityIcons name="account-outline" size={14} color={COLORS.gray500} />
                    <Text style={styles.detailText}>Host: {visitor.hostName}</Text>
                  </View>
                </View>
                
                <View style={styles.visitorFooter}>
                  <View style={styles.badgeTag}>
                    <MaterialCommunityIcons name="badge-account" size={12} color={COLORS.primaryBlue} />
                    <Text style={styles.badgeText}>{visitor.badge}</Text>
                  </View>
                  {visitor.checkIn && (
                    <Text style={styles.timeText}>In: {visitor.checkIn}</Text>
                  )}
                  {visitor.checkOut && (
                    <Text style={styles.timeText}>Out: {visitor.checkOut}</Text>
                  )}
                </View>
              </View>

              {visitor.status === 'checked_in' && (
                <TouchableOpacity 
                  style={styles.checkOutBtn}
                  onPress={() => handleCheckOut(visitor.id)}
                >
                  <MaterialCommunityIcons name="logout" size={20} color={COLORS.error} />
                </TouchableOpacity>
              )}
            </TouchableOpacity>
          ))
        )}
          </>
        )}
      </ScrollView>

      {/* Floating Add Button */}
      <TouchableOpacity style={styles.fab} onPress={handleAddVisitor}>
        <MaterialCommunityIcons name="account-plus" size={28} color={COLORS.white} />
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
  content: { paddingBottom: 100 },
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
  visitorCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  visitorAvatar: { width: 50, height: 50, borderRadius: 25, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  avatarText: { fontSize: FONTS.body, fontWeight: 'bold', color: COLORS.white },
  visitorInfo: { flex: 1, marginLeft: SIZES.sm },
  visitorHeader: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  visitorName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  statusBadge: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusFull },
  statusDot: { width: 6, height: 6, borderRadius: 3, marginRight: 4 },
  statusText: { fontSize: FONTS.tiny, fontWeight: '600', textTransform: 'capitalize' },
  visitorDetails: { marginTop: SIZES.xs },
  detailItem: { flexDirection: 'row', alignItems: 'center', marginTop: 2 },
  detailText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginLeft: 4 },
  visitorFooter: { flexDirection: 'row', alignItems: 'center', marginTop: SIZES.xs, gap: SIZES.sm },
  badgeTag: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.primaryBlue + '10', paddingHorizontal: SIZES.xs, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  badgeText: { fontSize: FONTS.tiny, color: COLORS.primaryBlue, fontWeight: '600', marginLeft: 2 },
  timeText: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  checkOutBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.error + '10', justifyContent: 'center', alignItems: 'center' },
  emptyState: { alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.xxl },
  emptyTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginTop: SIZES.md },
  emptyText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginTop: SIZES.xs },
  fab: { position: 'absolute', bottom: SIZES.xl, right: SIZES.md, width: 56, height: 56, borderRadius: 28, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center', ...SHADOWS.large },
  loadingContainer: { alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.xxl },
  loadingText: { fontSize: FONTS.body, color: COLORS.textSecondary, marginTop: SIZES.md },
});

export default VisitorManagementScreen;
