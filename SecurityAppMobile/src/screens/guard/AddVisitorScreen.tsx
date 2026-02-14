import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, Image } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Input from '../../components/common/Input';
import Button from '../../components/common/Button';
import { authService } from '../../services/authService';
import { visitorService } from '../../services/visitorService';
import { deploymentService } from '../../services/deploymentService';

function AddVisitorScreen({ navigation, route }: any) {
  const [loading, setLoading] = useState(false);
  const [photo, setPhoto] = useState<string | null>(null);
  const [guardId, setGuardId] = useState<string | null>(null);
  const [siteId, setSiteId] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    phone: '',
    email: '',
    company: '',
    purpose: '',
    hostName: '',
    hostDepartment: '',
    idType: 'Aadhaar',
    idNumber: '',
    vehicleNumber: '',
  });

  const purposes = ['Meeting', 'Interview', 'Delivery', 'Vendor Visit', 'Personal Visit', 'Service/Repair', 'Other'];
  const idTypes = ['Aadhaar', 'PAN Card', 'Driving License', 'Passport', 'Company ID', 'Other'];

  useEffect(() => {
    (async () => {
      const user = await authService.getStoredUser();
      if (!user) return;
      const gid = (user as { guardId?: string }).guardId || user.id;
      setGuardId(gid);
      const siteParam = route?.params?.siteId;
      if (siteParam) {
        setSiteId(siteParam);
        return;
      }
      const depRes = await deploymentService.getDeployments({
        guardId: gid,
        pageSize: 10,
        skipCache: true,
      });
      const list = depRes?.data?.items ?? depRes?.data ?? (Array.isArray(depRes?.data) ? depRes.data : []);
      const first = Array.isArray(list) ? list[0] : null;
      if (first) {
        const sid = (first as { siteId?: string }).siteId ?? (first as { SiteId?: string }).SiteId;
        if (sid) setSiteId(String(sid));
      }
    })();
  }, [route?.params?.siteId]);

  const handleTakePhoto = async () => {
    const permission = await ImagePicker.requestCameraPermissionsAsync();
    if (permission.granted) {
      const result = await ImagePicker.launchCameraAsync({
        allowsEditing: true,
        aspect: [1, 1],
        quality: 0.8,
      });
      if (!result.canceled) {
        setPhoto(result.assets[0].uri);
      }
    }
  };

  const handleSubmit = async () => {
    if (!formData.name || !formData.phone || !formData.purpose || !formData.hostName) {
      Alert.alert('Error', 'Please fill in all required fields');
      return;
    }
    if (!guardId || !siteId) {
      Alert.alert('Error', 'Could not determine your site. Please try again or select site from Visitor Log.');
      return;
    }

    setLoading(true);
    try {
      const result = await visitorService.registerVisitor({
        visitorName: formData.name.trim(),
        visitorType: 'Individual',
        companyName: formData.company.trim() || undefined,
        phoneNumber: formData.phone.trim(),
        email: formData.email.trim() || undefined,
        purpose: formData.purpose.trim(),
        hostName: formData.hostName.trim() || undefined,
        hostDepartment: formData.hostDepartment.trim() || undefined,
        siteId,
        guardId,
        idProofType: formData.idType || undefined,
        idProofNumber: formData.idNumber.trim() || undefined,
      });
      if (result.success) {
        const createData = result.data as { badgeNumber?: string; id?: string } | undefined;
        const badge = createData?.badgeNumber ?? `V-${String(Date.now()).slice(-4)}`;
        Alert.alert(
          'Visitor Registered',
          `Badge Number: ${badge}\n\nVisitor has been registered successfully. Badge will be printed.`,
          [{ text: 'OK', onPress: () => navigation.goBack() }]
        );
      } else {
        Alert.alert('Error', result.error?.message ?? 'Failed to register visitor');
      }
    } catch (e) {
      Alert.alert('Error', 'Failed to register visitor');
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Register Visitor</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* Photo Capture */}
        <View style={styles.photoSection}>
          <TouchableOpacity style={styles.photoContainer} onPress={handleTakePhoto}>
            {photo ? (
              <Image source={{ uri: photo }} style={styles.photo} />
            ) : (
              <View style={styles.photoPlaceholder}>
                <MaterialCommunityIcons name="camera" size={40} color={COLORS.gray400} />
                <Text style={styles.photoText}>Take Photo</Text>
              </View>
            )}
          </TouchableOpacity>
          <Text style={styles.photoHint}>Tap to capture visitor photo</Text>
        </View>

        {/* Basic Information */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Basic Information</Text>
          <View style={styles.formCard}>
            <Input
              label="Full Name *"
              placeholder="Enter visitor's full name"
              value={formData.name}
              onChangeText={(text) => setFormData({ ...formData, name: text })}
              leftIcon="account"
            />
            <Input
              label="Phone Number *"
              placeholder="Enter phone number"
              value={formData.phone}
              onChangeText={(text) => setFormData({ ...formData, phone: text })}
              keyboardType="phone-pad"
              leftIcon="phone"
            />
            <Input
              label="Email"
              placeholder="Enter email (optional)"
              value={formData.email}
              onChangeText={(text) => setFormData({ ...formData, email: text })}
              keyboardType="email-address"
              leftIcon="email"
            />
            <Input
              label="Company/Organization"
              placeholder="Enter company name"
              value={formData.company}
              onChangeText={(text) => setFormData({ ...formData, company: text })}
              leftIcon="office-building"
            />
          </View>
        </View>

        {/* Visit Details */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Visit Details</Text>
          <View style={styles.formCard}>
            <Text style={styles.label}>Purpose of Visit *</Text>
            <View style={styles.chipContainer}>
              {purposes.map((purpose) => (
                <TouchableOpacity
                  key={purpose}
                  style={[styles.chip, formData.purpose === purpose && styles.chipActive]}
                  onPress={() => setFormData({ ...formData, purpose })}
                >
                  <Text style={[styles.chipText, formData.purpose === purpose && styles.chipTextActive]}>
                    {purpose}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>
            
            <Input
              label="Host Name *"
              placeholder="Person to meet"
              value={formData.hostName}
              onChangeText={(text) => setFormData({ ...formData, hostName: text })}
              leftIcon="account-tie"
            />
            <Input
              label="Host Department"
              placeholder="Department name"
              value={formData.hostDepartment}
              onChangeText={(text) => setFormData({ ...formData, hostDepartment: text })}
              leftIcon="domain"
            />
          </View>
        </View>

        {/* ID Verification */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>ID Verification</Text>
          <View style={styles.formCard}>
            <Text style={styles.label}>ID Type</Text>
            <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.idTypeScroll}>
              {idTypes.map((type) => (
                <TouchableOpacity
                  key={type}
                  style={[styles.idTypeChip, formData.idType === type && styles.idTypeChipActive]}
                  onPress={() => setFormData({ ...formData, idType: type })}
                >
                  <Text style={[styles.idTypeText, formData.idType === type && styles.idTypeTextActive]}>
                    {type}
                  </Text>
                </TouchableOpacity>
              ))}
            </ScrollView>
            
            <Input
              label="ID Number"
              placeholder="Enter ID number"
              value={formData.idNumber}
              onChangeText={(text) => setFormData({ ...formData, idNumber: text })}
              leftIcon="card-account-details"
            />
          </View>
        </View>

        {/* Vehicle Information */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Vehicle Information (if any)</Text>
          <View style={styles.formCard}>
            <Input
              label="Vehicle Number"
              placeholder="Enter vehicle number"
              value={formData.vehicleNumber}
              onChangeText={(text) => setFormData({ ...formData, vehicleNumber: text.toUpperCase() })}
              leftIcon="car"
            />
          </View>
        </View>

        {/* Submit Button */}
        <Button
          title="Register Visitor"
          onPress={handleSubmit}
          loading={loading}
          style={styles.submitBtn}
        />

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
  placeholder: { width: 40 },
  content: { padding: SIZES.md },
  photoSection: { alignItems: 'center', marginBottom: SIZES.lg },
  photoContainer: { width: 120, height: 120, borderRadius: 60, overflow: 'hidden', backgroundColor: COLORS.gray100 },
  photo: { width: '100%', height: '100%' },
  photoPlaceholder: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  photoText: { fontSize: FONTS.caption, color: COLORS.gray500, marginTop: SIZES.xs },
  photoHint: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.sm },
  section: { marginBottom: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  formCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  label: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  chipContainer: { flexDirection: 'row', flexWrap: 'wrap', gap: SIZES.sm, marginBottom: SIZES.md },
  chip: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusFull, borderWidth: 1, borderColor: 'transparent' },
  chipActive: { backgroundColor: COLORS.primary + '15', borderColor: COLORS.primary },
  chipText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  chipTextActive: { color: COLORS.primary, fontWeight: '600' },
  idTypeScroll: { marginBottom: SIZES.md },
  idTypeChip: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusMd, marginRight: SIZES.sm },
  idTypeChipActive: { backgroundColor: COLORS.primaryBlue + '15' },
  idTypeText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  idTypeTextActive: { color: COLORS.primaryBlue, fontWeight: '600' },
  submitBtn: { marginTop: SIZES.md },
});

export default AddVisitorScreen;
