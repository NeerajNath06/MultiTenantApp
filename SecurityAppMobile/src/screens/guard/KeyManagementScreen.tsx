import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, TextInput } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';

interface Key {
  id: number;
  keyNumber: string;
  keyName: string;
  location: string;
  status: 'available' | 'issued' | 'missing';
  issuedTo?: string;
  issuedAt?: string;
  returnDue?: string;
}

function KeyManagementScreen({ navigation }: any) {
  const [searchQuery, setSearchQuery] = useState('');
  const [activeTab, setActiveTab] = useState<'all' | 'available' | 'issued'>('all');

  const [keys, setKeys] = useState<Key[]>([
    { id: 1, keyNumber: 'K-001', keyName: 'Main Gate Key', location: 'Key Box A', status: 'available' },
    { id: 2, keyNumber: 'K-002', keyName: 'Server Room Key', location: 'Key Box A', status: 'issued', issuedTo: 'Amit Singh', issuedAt: '08:30 AM', returnDue: '06:00 PM' },
    { id: 3, keyNumber: 'K-003', keyName: 'Building A Master Key', location: 'Key Box A', status: 'available' },
    { id: 4, keyNumber: 'K-004', keyName: 'Parking Barrier Key', location: 'Key Box B', status: 'issued', issuedTo: 'Rajesh Kumar', issuedAt: '09:00 AM', returnDue: '06:00 PM' },
    { id: 5, keyNumber: 'K-005', keyName: 'Emergency Exit Key', location: 'Key Box A', status: 'available' },
    { id: 6, keyNumber: 'K-006', keyName: 'Storage Room Key', location: 'Key Box B', status: 'missing' },
    { id: 7, keyNumber: 'K-007', keyName: 'Conference Room Key', location: 'Key Box A', status: 'issued', issuedTo: 'HR Department', issuedAt: '10:00 AM', returnDue: '05:00 PM' },
    { id: 8, keyNumber: 'K-008', keyName: 'Roof Access Key', location: 'Key Box A', status: 'available' },
  ]);

  const handleIssueKey = (key: Key) => {
    Alert.alert(
      'Issue Key',
      `Issue "${key.keyName}" to:`,
      [
        { text: 'Cancel', style: 'cancel' },
        { 
          text: 'Issue', 
          onPress: () => {
            const now = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            setKeys(keys.map(k => 
              k.id === key.id ? { ...k, status: 'issued' as const, issuedTo: 'Self', issuedAt: now, returnDue: '06:00 PM' } : k
            ));
            Alert.alert('Success', 'Key issued successfully');
          }
        }
      ]
    );
  };

  const handleReturnKey = (key: Key) => {
    Alert.alert(
      'Return Key',
      `Return "${key.keyName}"?`,
      [
        { text: 'Cancel', style: 'cancel' },
        { 
          text: 'Return', 
          onPress: () => {
            setKeys(keys.map(k => 
              k.id === key.id ? { ...k, status: 'available' as const, issuedTo: undefined, issuedAt: undefined, returnDue: undefined } : k
            ));
            Alert.alert('Success', 'Key returned successfully');
          }
        }
      ]
    );
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'available': return COLORS.success;
      case 'issued': return COLORS.warning;
      case 'missing': return COLORS.error;
      default: return COLORS.gray500;
    }
  };

  const filteredKeys = keys.filter(key => {
    const matchesSearch = key.keyName.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          key.keyNumber.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesTab = activeTab === 'all' ? true : key.status === activeTab;
    return matchesSearch && matchesTab;
  });

  const availableCount = keys.filter(k => k.status === 'available').length;
  const issuedCount = keys.filter(k => k.status === 'issued').length;
  const missingCount = keys.filter(k => k.status === 'missing').length;

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Key Management</Text>
        <TouchableOpacity style={styles.historyBtn}>
          <MaterialCommunityIcons name="history" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
      </View>

      {/* Stats */}
      <View style={styles.statsRow}>
        <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
          <MaterialCommunityIcons name="key-variant" size={24} color={COLORS.success} />
          <Text style={[styles.statValue, { color: COLORS.success }]}>{availableCount}</Text>
          <Text style={styles.statLabel}>Available</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.warning + '15' }]}>
          <MaterialCommunityIcons name="key-remove" size={24} color={COLORS.warning} />
          <Text style={[styles.statValue, { color: COLORS.warning }]}>{issuedCount}</Text>
          <Text style={styles.statLabel}>Issued</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.error + '15' }]}>
          <MaterialCommunityIcons name="key-alert" size={24} color={COLORS.error} />
          <Text style={[styles.statValue, { color: COLORS.error }]}>{missingCount}</Text>
          <Text style={styles.statLabel}>Missing</Text>
        </View>
      </View>

      {/* Search */}
      <View style={styles.searchContainer}>
        <MaterialCommunityIcons name="magnify" size={20} color={COLORS.gray400} />
        <TextInput
          style={styles.searchInput}
          placeholder="Search keys..."
          value={searchQuery}
          onChangeText={setSearchQuery}
          placeholderTextColor={COLORS.gray400}
        />
      </View>

      {/* Tabs */}
      <View style={styles.tabContainer}>
        {(['all', 'available', 'issued'] as const).map((tab) => (
          <TouchableOpacity
            key={tab}
            style={[styles.tab, activeTab === tab && styles.tabActive]}
            onPress={() => setActiveTab(tab)}
          >
            <Text style={[styles.tabText, activeTab === tab && styles.tabTextActive]}>
              {tab.charAt(0).toUpperCase() + tab.slice(1)}
            </Text>
          </TouchableOpacity>
        ))}
      </View>

      {/* Keys List */}
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {filteredKeys.map((key) => (
          <View key={key.id} style={styles.keyCard}>
            <View style={[styles.keyIcon, { backgroundColor: getStatusColor(key.status) + '15' }]}>
              <MaterialCommunityIcons name="key-variant" size={28} color={getStatusColor(key.status)} />
            </View>
            
            <View style={styles.keyInfo}>
              <View style={styles.keyHeader}>
                <Text style={styles.keyNumber}>{key.keyNumber}</Text>
                <View style={[styles.statusBadge, { backgroundColor: getStatusColor(key.status) + '15' }]}>
                  <View style={[styles.statusDot, { backgroundColor: getStatusColor(key.status) }]} />
                  <Text style={[styles.statusText, { color: getStatusColor(key.status) }]}>
                    {key.status.charAt(0).toUpperCase() + key.status.slice(1)}
                  </Text>
                </View>
              </View>
              <Text style={styles.keyName}>{key.keyName}</Text>
              <View style={styles.keyDetails}>
                <MaterialCommunityIcons name="map-marker" size={14} color={COLORS.gray500} />
                <Text style={styles.keyLocation}>{key.location}</Text>
              </View>
              
              {key.issuedTo && (
                <View style={styles.issuedInfo}>
                  <Text style={styles.issuedText}>Issued to: {key.issuedTo}</Text>
                  <Text style={styles.issuedTime}>At: {key.issuedAt} • Due: {key.returnDue}</Text>
                </View>
              )}
            </View>

            {key.status === 'available' && (
              <TouchableOpacity style={styles.issueBtn} onPress={() => handleIssueKey(key)}>
                <MaterialCommunityIcons name="arrow-right-bold-circle" size={32} color={COLORS.primaryBlue} />
              </TouchableOpacity>
            )}
            
            {key.status === 'issued' && key.issuedTo === 'Self' && (
              <TouchableOpacity style={styles.returnBtn} onPress={() => handleReturnKey(key)}>
                <MaterialCommunityIcons name="arrow-left-bold-circle" size={32} color={COLORS.success} />
              </TouchableOpacity>
            )}
          </View>
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
  historyBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
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
  content: { padding: SIZES.md },
  keyCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  keyIcon: { width: 56, height: 56, borderRadius: 28, justifyContent: 'center', alignItems: 'center' },
  keyInfo: { flex: 1, marginLeft: SIZES.sm },
  keyHeader: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  keyNumber: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.primaryBlue, backgroundColor: COLORS.primaryBlue + '10', paddingHorizontal: SIZES.xs, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  statusBadge: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusFull },
  statusDot: { width: 6, height: 6, borderRadius: 3, marginRight: 4 },
  statusText: { fontSize: FONTS.tiny, fontWeight: '600' },
  keyName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginTop: SIZES.xs },
  keyDetails: { flexDirection: 'row', alignItems: 'center', marginTop: 4 },
  keyLocation: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginLeft: 4 },
  issuedInfo: { marginTop: SIZES.xs, paddingTop: SIZES.xs, borderTopWidth: 1, borderTopColor: COLORS.gray100 },
  issuedText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  issuedTime: { fontSize: FONTS.tiny, color: COLORS.gray500, marginTop: 2 },
  issueBtn: { padding: SIZES.xs },
  returnBtn: { padding: SIZES.xs },
});

export default KeyManagementScreen;
