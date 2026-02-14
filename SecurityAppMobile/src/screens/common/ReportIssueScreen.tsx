import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, Image } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Input from '../../components/common/Input';
import Button from '../../components/common/Button';

interface IssueType {
  id: string;
  label: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
  color: string;
}

const issueTypes: IssueType[] = [
  { id: 'app_crash', label: 'App Crash', icon: 'close-circle', color: COLORS.error },
  { id: 'login', label: 'Login Issue', icon: 'account-lock', color: COLORS.warning },
  { id: 'checkin', label: 'Check-In Problem', icon: 'login', color: COLORS.info },
  { id: 'location', label: 'Location Error', icon: 'map-marker-off', color: COLORS.secondary },
  { id: 'sync', label: 'Sync Problem', icon: 'sync-alert', color: COLORS.primary },
  { id: 'notification', label: 'Notification Issue', icon: 'bell-off', color: COLORS.warning },
  { id: 'display', label: 'Display Problem', icon: 'monitor', color: COLORS.info },
  { id: 'other', label: 'Other', icon: 'dots-horizontal', color: COLORS.gray500 },
];

function ReportIssueScreen({ navigation }: any) {
  const [loading, setLoading] = useState(false);
  const [selectedType, setSelectedType] = useState<string | null>(null);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [stepsToReproduce, setStepsToReproduce] = useState('');
  const [screenshots, setScreenshots] = useState<string[]>([]);

  const handleAddScreenshot = async () => {
    if (screenshots.length >= 3) {
      Alert.alert('Limit Reached', 'You can add up to 3 screenshots only.');
      return;
    }

    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Permission Denied', 'Gallery permission is required.');
      return;
    }

    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images'],
      quality: 0.7,
    });

    if (!result.canceled) {
      setScreenshots([...screenshots, result.assets[0].uri]);
    }
  };

  const removeScreenshot = (index: number) => {
    setScreenshots(screenshots.filter((_, i) => i !== index));
  };

  const handleSubmit = () => {
    if (!selectedType) {
      Alert.alert('Error', 'Please select an issue type');
      return;
    }
    if (!title.trim()) {
      Alert.alert('Error', 'Please enter a title for the issue');
      return;
    }
    if (!description.trim()) {
      Alert.alert('Error', 'Please describe the issue');
      return;
    }

    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      Alert.alert(
        'Issue Reported',
        'Thank you for reporting this issue. Our team will investigate and get back to you soon. Reference ID: #ISS-' + Date.now().toString().slice(-6),
        [{ text: 'OK', onPress: () => navigation.goBack() }]
      );
    }, 1500);
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Report Issue</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* Issue Type Selection */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>What type of issue are you facing?</Text>
          <View style={styles.typesGrid}>
            {issueTypes.map((type) => (
              <TouchableOpacity
                key={type.id}
                style={[styles.typeCard, selectedType === type.id && styles.typeCardActive]}
                onPress={() => setSelectedType(type.id)}
              >
                <View style={[styles.typeIcon, { backgroundColor: type.color + '15' }, selectedType === type.id && { backgroundColor: type.color }]}>
                  <MaterialCommunityIcons
                    name={type.icon}
                    size={24}
                    color={selectedType === type.id ? COLORS.white : type.color}
                  />
                </View>
                <Text style={[styles.typeLabel, selectedType === type.id && styles.typeLabelActive]}>
                  {type.label}
                </Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>

        {/* Issue Title */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Issue Title</Text>
          <Input
            placeholder="Brief title describing the issue"
            value={title}
            onChangeText={setTitle}
            icon="text"
          />
        </View>

        {/* Description */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Description</Text>
          <Input
            placeholder="Describe the issue in detail. What were you trying to do? What happened instead?"
            value={description}
            onChangeText={setDescription}
            multiline
            numberOfLines={4}
          />
        </View>

        {/* Steps to Reproduce */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Steps to Reproduce (Optional)</Text>
          <Input
            placeholder="1. Open the app&#10;2. Tap on...&#10;3. The issue appears when..."
            value={stepsToReproduce}
            onChangeText={setStepsToReproduce}
            multiline
            numberOfLines={4}
          />
        </View>

        {/* Screenshots */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Screenshots (Optional)</Text>
          <Text style={styles.sectionSubtitle}>Add up to 3 screenshots to help us understand the issue</Text>
          
          <View style={styles.screenshotsContainer}>
            {screenshots.map((uri, index) => (
              <View key={index} style={styles.screenshotItem}>
                <Image source={{ uri }} style={styles.screenshotImage} />
                <TouchableOpacity style={styles.removeScreenshot} onPress={() => removeScreenshot(index)}>
                  <MaterialCommunityIcons name="close-circle" size={24} color={COLORS.error} />
                </TouchableOpacity>
              </View>
            ))}
            
            {screenshots.length < 3 && (
              <TouchableOpacity style={styles.addScreenshot} onPress={handleAddScreenshot}>
                <MaterialCommunityIcons name="camera-plus" size={32} color={COLORS.gray400} />
                <Text style={styles.addScreenshotText}>Add Screenshot</Text>
              </TouchableOpacity>
            )}
          </View>
        </View>

        {/* Device Info */}
        <View style={styles.deviceInfoCard}>
          <View style={styles.deviceInfoHeader}>
            <MaterialCommunityIcons name="cellphone" size={20} color={COLORS.primary} />
            <Text style={styles.deviceInfoTitle}>Device Information</Text>
          </View>
          <Text style={styles.deviceInfoText}>This information will be automatically included to help diagnose the issue.</Text>
          <View style={styles.deviceInfoRow}>
            <Text style={styles.deviceInfoLabel}>App Version:</Text>
            <Text style={styles.deviceInfoValue}>1.0.0</Text>
          </View>
          <View style={styles.deviceInfoRow}>
            <Text style={styles.deviceInfoLabel}>Device:</Text>
            <Text style={styles.deviceInfoValue}>Auto-detected</Text>
          </View>
          <View style={styles.deviceInfoRow}>
            <Text style={styles.deviceInfoLabel}>OS:</Text>
            <Text style={styles.deviceInfoValue}>Auto-detected</Text>
          </View>
        </View>

        <Button
          title="Submit Report"
          onPress={handleSubmit}
          loading={loading}
          style={styles.submitButton}
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
  section: { marginBottom: SIZES.lg },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  sectionSubtitle: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginBottom: SIZES.sm, marginTop: -SIZES.xs },
  typesGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: SIZES.sm },
  typeCard: { width: '23%', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.sm, alignItems: 'center', borderWidth: 2, borderColor: 'transparent', ...SHADOWS.small },
  typeCardActive: { borderColor: COLORS.primary },
  typeIcon: { width: 44, height: 44, borderRadius: 22, justifyContent: 'center', alignItems: 'center', marginBottom: SIZES.xs },
  typeLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, textAlign: 'center' },
  typeLabelActive: { color: COLORS.primary, fontWeight: '600' },
  screenshotsContainer: { flexDirection: 'row', flexWrap: 'wrap', gap: SIZES.sm },
  screenshotItem: { position: 'relative', width: 100, height: 100 },
  screenshotImage: { width: 100, height: 100, borderRadius: SIZES.radiusSm },
  removeScreenshot: { position: 'absolute', top: -8, right: -8, backgroundColor: COLORS.white, borderRadius: 12 },
  addScreenshot: { width: 100, height: 100, borderRadius: SIZES.radiusSm, borderWidth: 2, borderStyle: 'dashed', borderColor: COLORS.gray300, justifyContent: 'center', alignItems: 'center' },
  addScreenshotText: { fontSize: FONTS.tiny, color: COLORS.gray400, marginTop: SIZES.xs, textAlign: 'center' },
  deviceInfoCard: { backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.lg },
  deviceInfoHeader: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.sm },
  deviceInfoTitle: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.primary, marginLeft: SIZES.xs },
  deviceInfoText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginBottom: SIZES.sm },
  deviceInfoRow: { flexDirection: 'row', justifyContent: 'space-between', marginTop: SIZES.xs },
  deviceInfoLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  deviceInfoValue: { fontSize: FONTS.caption, color: COLORS.textPrimary, fontWeight: '500' },
  submitButton: { marginBottom: SIZES.xxl },
});

export default ReportIssueScreen;
