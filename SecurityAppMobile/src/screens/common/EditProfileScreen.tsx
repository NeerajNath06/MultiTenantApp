import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Image, Alert, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import * as FileSystem from 'expo-file-system';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Input from '../../components/common/Input';
import Button from '../../components/common/Button';
import { authService } from '../../services/authService';
import { userService } from '../../services/userService';

const PROFILE_PHOTO_KEY = (userId: string) => `profilePhoto_${userId}`;

function EditProfileScreen({ navigation }: any) {
  const [loading, setLoading] = useState(false);
  const [loadingProfile, setLoadingProfile] = useState(true);
  const [profileImage, setProfileImage] = useState<string | null>(null);
  const [userId, setUserId] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    employeeId: '',
    email: '',
    phone: '',
    newPassword: '',
  });

  const loadProfile = useCallback(async () => {
    const stored = await authService.getStoredUser();
    if (!stored?.id) {
      setLoadingProfile(false);
      return;
    }
    setUserId(stored.id);
    const result = await userService.getProfile(stored.id);
    if (result.success && result.data) {
      const p = result.data;
      setFormData(prev => ({
        ...prev,
        firstName: p.firstName || '',
        lastName: p.lastName || '',
        email: p.email || '',
        phone: p.phoneNumber || '',
        employeeId: (stored as { guardId?: string }).guardId ? `Guard: ${(stored as { guardId?: string }).guardId}` : stored.id,
      }));
    }
    try {
      const base64 = await AsyncStorage.getItem(PROFILE_PHOTO_KEY(stored.id));
      if (base64) setProfileImage('data:image/jpeg;base64,' + base64);
    } catch (_) {}
    setLoadingProfile(false);
  }, []);

  useEffect(() => {
    loadProfile();
  }, [loadProfile]);

  const persistProfilePhoto = useCallback(async (base64OrUri: string, isBase64?: boolean) => {
    const stored = await authService.getStoredUser();
    if (!stored?.id) return;
    try {
      const base64 = isBase64 ? base64OrUri : await FileSystem.readAsStringAsync(base64OrUri, { encoding: FileSystem.EncodingType.Base64 });
      if (base64) await AsyncStorage.setItem(PROFILE_PHOTO_KEY(stored.id), base64);
    } catch (e) {
      console.warn('persistProfilePhoto failed', e);
    }
  }, []);

  const handlePickImage = async () => {
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Permission Denied', 'Gallery permission is required.');
      return;
    }

    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images'],
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.8,
      base64: true,
    });

    if (!result.canceled) {
      const asset = result.assets[0];
      const uri = asset.uri;
      const base64 = asset.base64;
      setProfileImage(uri);
      if (base64) await persistProfilePhoto(base64, true);
      else await persistProfilePhoto(uri, false);
    }
  };

  const handleTakePhoto = async () => {
    const { status } = await ImagePicker.requestCameraPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Permission Denied', 'Camera permission is required.');
      return;
    }

    const result = await ImagePicker.launchCameraAsync({
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.8,
      base64: true,
    });

    if (!result.canceled) {
      const asset = result.assets[0];
      const uri = asset.uri;
      const base64 = asset.base64;
      setProfileImage(uri);
      if (base64) await persistProfilePhoto(base64, true);
      else await persistProfilePhoto(uri, false);
    }
  };

  const handleSave = async () => {
    if (!userId) return;
    const trimmed = {
      firstName: (formData.firstName || '').trim(),
      lastName: (formData.lastName || '').trim(),
      email: (formData.email || '').trim(),
      phone: (formData.phone || '').trim(),
      newPassword: (formData.newPassword || '').trim() || undefined,
    };
    if (!trimmed.firstName || !trimmed.email) {
      Alert.alert('Validation', 'First name and email are required.');
      return;
    }
    setLoading(true);
    const result = await userService.updateProfile(userId, trimmed);
    setLoading(false);
    if (result.success) {
      Alert.alert('Success', result.message || 'Profile updated successfully', [
        { text: 'OK', onPress: () => navigation.goBack() }
      ]);
    } else {
      Alert.alert('Error', result.error?.message || 'Failed to update profile.');
    }
  };

  const updateField = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const displayInitials = (formData.firstName || formData.lastName)
    ? `${(formData.firstName || '').trim()[0] || ''}${(formData.lastName || '').trim()[0] || ''}`.toUpperCase() || '?'
    : '?';

  if (loadingProfile) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading profile...</Text>
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
        <Text style={styles.headerTitle}>Edit Profile</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* Profile Image */}
        <View style={styles.imageSection}>
          <View style={styles.imageContainer}>
            {profileImage ? (
              <Image source={{ uri: profileImage }} style={styles.profileImage} />
            ) : (
              <View style={styles.profileImagePlaceholder}>
                <Text style={styles.profileInitials}>{displayInitials}</Text>
              </View>
            )}
            <TouchableOpacity style={styles.editImageBtn} onPress={() => {
              Alert.alert('Change Photo', 'Choose an option', [
                { text: 'Cancel', style: 'cancel' },
                { text: 'Take Photo', onPress: handleTakePhoto },
                { text: 'Choose from Gallery', onPress: handlePickImage },
              ]);
            }}>
              <MaterialCommunityIcons name="camera" size={18} color={COLORS.white} />
            </TouchableOpacity>
          </View>
          <Text style={styles.changePhotoText}>Tap to change photo</Text>
        </View>

        {/* Form Fields */}
        <View style={styles.formSection}>
          <Input
            label="First Name"
            placeholder="Enter first name"
            value={formData.firstName}
            onChangeText={(text) => updateField('firstName', text)}
            icon="account"
          />

          <Input
            label="Last Name"
            placeholder="Enter last name"
            value={formData.lastName}
            onChangeText={(text) => updateField('lastName', text)}
            icon="account"
          />

          <Input
            label="Employee / Guard ID"
            placeholder="—"
            value={formData.employeeId}
            onChangeText={() => {}}
            icon="card-account-details"
            editable={false}
          />

          <Input
            label="Email"
            placeholder="Enter email"
            value={formData.email}
            onChangeText={(text) => updateField('email', text)}
            icon="email"
            keyboardType="email-address"
          />

          <Input
            label="Phone Number"
            placeholder="Enter phone number"
            value={formData.phone}
            onChangeText={(text) => updateField('phone', text)}
            icon="phone"
            keyboardType="phone-pad"
          />

          <Input
            label="New Password (optional)"
            placeholder="Leave blank to keep current"
            value={formData.newPassword}
            onChangeText={(text) => updateField('newPassword', text)}
            icon="lock"
            secureTextEntry
          />
        </View>

        <Button
          title="Save Changes"
          onPress={handleSave}
          loading={loading}
          style={styles.saveButton}
        />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  content: { padding: SIZES.md },
  imageSection: { alignItems: 'center', marginBottom: SIZES.xl },
  imageContainer: { position: 'relative' },
  profileImage: { width: 120, height: 120, borderRadius: 60 },
  profileImagePlaceholder: { width: 120, height: 120, borderRadius: 60, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  profileInitials: { fontSize: 40, fontWeight: 'bold', color: COLORS.white },
  editImageBtn: { position: 'absolute', bottom: 0, right: 0, width: 36, height: 36, borderRadius: 18, backgroundColor: COLORS.secondary, justifyContent: 'center', alignItems: 'center', borderWidth: 3, borderColor: COLORS.white },
  changePhotoText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.sm },
  formSection: { marginBottom: SIZES.lg },
  saveButton: { marginBottom: SIZES.xxl },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', gap: SIZES.sm },
  loadingText: { fontSize: FONTS.body, color: COLORS.textSecondary },
});

export default EditProfileScreen;
