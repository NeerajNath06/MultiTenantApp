import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, ActivityIndicator, KeyboardAvoidingView, Platform } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Input from '../../components/common/Input';
import Button from '../../components/common/Button';
import { vehicleService } from '../../services/vehicleService';

const VEHICLE_TYPES = [
  { value: 'Car', label: 'Car' },
  { value: 'Bike', label: 'Bike' },
  { value: 'Truck', label: 'Truck' },
  { value: 'Auto', label: 'Auto' },
  { value: 'Other', label: 'Other' },
];

const PURPOSE_OPTIONS = [
  'Employee',
  'Visitor',
  'Delivery',
  'Vendor',
  'Client Meeting',
  'Service',
  'Other',
];

function AddVehicleEntryScreen({ navigation, route }: any) {
  const { guardId, siteId } = route.params ?? {};
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState({
    vehicleNumber: '',
    vehicleType: 'Car',
    driverName: '',
    driverPhone: '',
    purpose: 'Visitor',
    parkingSlot: '',
  });

  const update = (key: string, value: string) => setForm(prev => ({ ...prev, [key]: value }));

  const handleSubmit = async () => {
    const vehicleNumber = form.vehicleNumber.trim().toUpperCase();
    const driverName = form.driverName.trim();
    if (!vehicleNumber || !driverName) {
      Alert.alert('Validation', 'Vehicle number and driver name are required.');
      return;
    }
    if (!guardId || !siteId) {
      Alert.alert('Error', 'Missing guard or site context. Go back and try again.');
      return;
    }

    setLoading(true);
    const result = await vehicleService.registerVehicleEntry({
      vehicleNumber,
      vehicleType: form.vehicleType,
      driverName,
      driverPhone: form.driverPhone.trim() || undefined,
      purpose: form.purpose,
      parkingSlot: form.parkingSlot.trim() || undefined,
      siteId,
      guardId,
    });
    setLoading(false);

    if (result.success) {
      Alert.alert('Success', 'Vehicle entry registered.', [
        { text: 'OK', onPress: () => navigation.navigate('VehicleLog') },
      ]);
    } else {
      Alert.alert('Error', result.error?.message ?? 'Failed to register vehicle entry.');
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Add Vehicle Entry</Text>
        <View style={styles.placeholder} />
      </View>

      <KeyboardAvoidingView
        style={styles.keyboard}
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        keyboardVerticalOffset={Platform.OS === 'ios' ? 90 : 0}
      >
        <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.scroll}>
          <Input
            label="Vehicle Number"
            placeholder="e.g. DL 01 AB 1234"
            value={form.vehicleNumber}
            onChangeText={text => update('vehicleNumber', text.toUpperCase())}
            autoCapitalize="characters"
          />
          <Text style={styles.fieldLabel}>Vehicle Type</Text>
          <View style={styles.chipRow}>
            {VEHICLE_TYPES.map(t => (
              <TouchableOpacity
                key={t.value}
                style={[styles.chip, form.vehicleType === t.value && styles.chipActive]}
                onPress={() => update('vehicleType', t.value)}
              >
                <Text style={[styles.chipText, form.vehicleType === t.value && styles.chipTextActive]}>{t.label}</Text>
              </TouchableOpacity>
            ))}
          </View>
          <Input
            label="Driver Name"
            placeholder="Full name"
            value={form.driverName}
            onChangeText={text => update('driverName', text)}
          />
          <Input
            label="Driver Phone (optional)"
            placeholder="10-digit mobile"
            value={form.driverPhone}
            onChangeText={text => update('driverPhone', text)}
            keyboardType="phone-pad"
          />
          <Text style={styles.fieldLabel}>Purpose</Text>
          <View style={styles.chipRow}>
            {PURPOSE_OPTIONS.map(p => (
              <TouchableOpacity
                key={p}
                style={[styles.chip, form.purpose === p && styles.chipActive]}
                onPress={() => update('purpose', p)}
              >
                <Text style={[styles.chipText, form.purpose === p && styles.chipTextActive]} numberOfLines={1}>{p}</Text>
              </TouchableOpacity>
            ))}
          </View>
          <Input
            label="Parking Slot (optional)"
            placeholder="e.g. A-15"
            value={form.parkingSlot}
            onChangeText={text => update('parkingSlot', text)}
          />
          <View style={styles.submitWrap}>
            <Button
              title={loading ? 'Registering...' : 'Register Entry'}
              onPress={handleSubmit}
              disabled={loading}
              loading={loading}
            />
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40, height: 40 },
  keyboard: { flex: 1 },
  scroll: { padding: SIZES.md, paddingBottom: SIZES.xl * 2 },
  fieldLabel: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.xs, marginTop: SIZES.sm },
  chipRow: { flexDirection: 'row', flexWrap: 'wrap', gap: SIZES.xs, marginBottom: SIZES.sm },
  chip: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusMd, backgroundColor: COLORS.gray100 },
  chipActive: { backgroundColor: COLORS.primary, },
  chipText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  chipTextActive: { color: COLORS.white, fontWeight: '600' },
  submitWrap: { marginTop: SIZES.xl },
});

export default AddVehicleEntryScreen;
