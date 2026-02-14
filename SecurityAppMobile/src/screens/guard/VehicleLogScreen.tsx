import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, TextInput } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';

interface VehicleEntry {
  id: number;
  vehicleNumber: string;
  vehicleType: 'car' | 'bike' | 'truck' | 'auto' | 'other';
  driverName: string;
  purpose: string;
  entryTime: string;
  exitTime?: string;
  status: 'in' | 'out';
  parkingSlot?: string;
}

function VehicleLogScreen({ navigation }: any) {
  const [searchQuery, setSearchQuery] = useState('');
  const [activeTab, setActiveTab] = useState<'in' | 'out' | 'all'>('in');

  const [vehicles, setVehicles] = useState<VehicleEntry[]>([
    { id: 1, vehicleNumber: 'DL 01 AB 1234', vehicleType: 'car', driverName: 'Rajesh Kumar', purpose: 'Employee', entryTime: '08:30 AM', status: 'in', parkingSlot: 'A-15' },
    { id: 2, vehicleNumber: 'UP 16 CD 5678', vehicleType: 'bike', driverName: 'Amit Singh', purpose: 'Visitor', entryTime: '09:15 AM', status: 'in', parkingSlot: 'B-03' },
    { id: 3, vehicleNumber: 'HR 26 EF 9012', vehicleType: 'truck', driverName: 'Mohan Lal', purpose: 'Delivery', entryTime: '10:00 AM', exitTime: '10:45 AM', status: 'out', parkingSlot: 'C-01' },
    { id: 4, vehicleNumber: 'DL 03 GH 3456', vehicleType: 'car', driverName: 'Priya Sharma', purpose: 'Client Meeting', entryTime: '11:00 AM', status: 'in', parkingSlot: 'A-08' },
    { id: 5, vehicleNumber: 'UP 80 IJ 7890', vehicleType: 'auto', driverName: 'Driver', purpose: 'Vendor', entryTime: '11:30 AM', exitTime: '12:00 PM', status: 'out' },
  ]);

  const getVehicleIcon = (type: string): keyof typeof MaterialCommunityIcons.glyphMap => {
    switch (type) {
      case 'car': return 'car';
      case 'bike': return 'motorbike';
      case 'truck': return 'truck';
      case 'auto': return 'rickshaw';
      default: return 'car-outline';
    }
  };

  const handleVehicleExit = (vehicleId: number) => {
    const now = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    setVehicles(vehicles.map(v => 
      v.id === vehicleId ? { ...v, status: 'out' as const, exitTime: now } : v
    ));
    Alert.alert('Vehicle Exit', 'Vehicle exit recorded successfully.');
  };

  const handleAddVehicle = () => {
    Alert.alert('Add Vehicle', 'Vehicle entry form will open');
  };

  const filteredVehicles = vehicles.filter(v => {
    const matchesSearch = v.vehicleNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          v.driverName.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesTab = activeTab === 'all' ? true : v.status === activeTab;
    return matchesSearch && matchesTab;
  });

  const inCount = vehicles.filter(v => v.status === 'in').length;
  const outCount = vehicles.filter(v => v.status === 'out').length;

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Vehicle Log</Text>
        <TouchableOpacity style={styles.addBtn} onPress={handleAddVehicle}>
          <MaterialCommunityIcons name="plus" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      {/* Stats */}
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
          <Text style={[styles.statValue, { color: COLORS.primaryBlue }]}>45</Text>
          <Text style={styles.statLabel}>Slots Available</Text>
        </View>
      </View>

      {/* Search */}
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

      {/* Tabs */}
      <View style={styles.tabContainer}>
        {(['in', 'out', 'all'] as const).map((tab) => (
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

      {/* Vehicle List */}
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.listContent}>
        {filteredVehicles.map((vehicle) => (
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
                <Text style={styles.timeText}>In: {vehicle.entryTime}</Text>
                {vehicle.exitTime && <Text style={styles.timeText}>Out: {vehicle.exitTime}</Text>}
              </View>
            </View>

            <View style={styles.actionContainer}>
              <View style={[styles.statusBadge, { backgroundColor: vehicle.status === 'in' ? COLORS.success + '15' : COLORS.gray200 }]}>
                <Text style={[styles.statusText, { color: vehicle.status === 'in' ? COLORS.success : COLORS.gray500 }]}>
                  {vehicle.status === 'in' ? 'Inside' : 'Exited'}
                </Text>
              </View>
              {vehicle.status === 'in' && (
                <TouchableOpacity style={styles.exitBtn} onPress={() => handleVehicleExit(vehicle.id)}>
                  <MaterialCommunityIcons name="logout" size={18} color={COLORS.error} />
                </TouchableOpacity>
              )}
            </View>
          </View>
        ))}
      </ScrollView>

      {/* FAB */}
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
