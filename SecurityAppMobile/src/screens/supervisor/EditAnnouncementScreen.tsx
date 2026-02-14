import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, TextInput, Switch, Alert, ActivityIndicator, KeyboardAvoidingView, Platform } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { announcementService } from '../../services/announcementService';

const CATEGORIES = ['general', 'urgent', 'policy', 'event', 'training'];

function EditAnnouncementScreen({ navigation, route }: any) {
  const id = route?.params?.id;
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [category, setCategory] = useState('general');
  const [isPinned, setIsPinned] = useState(false);
  const [isActive, setIsActive] = useState(true);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    if (!id) return;
    const res = await announcementService.getAnnouncementById(id);
    if (res.success && res.data) {
      setTitle(res.data.title ?? '');
      setContent(res.data.content ?? '');
      setCategory(res.data.category ?? 'general');
      setIsPinned(res.data.isPinned ?? false);
      setIsActive(res.data.isActive ?? true);
    } else {
      Alert.alert('Error', 'Announcement not found', [{ text: 'OK', onPress: () => navigation.goBack() }]);
    }
  }, [id, navigation]);

  useEffect(() => {
    load().finally(() => setLoading(false));
  }, [load]);

  const handleSave = async () => {
    const t = title.trim();
    const c = content.trim();
    if (!t || !c) {
      Alert.alert('Required', 'Title and content are required.');
      return;
    }
    if (!id) return;
    setSaving(true);
    try {
      const res = await announcementService.updateAnnouncement(id, { title: t, content: c, category, isPinned, isActive });
      if (res.success) {
        Alert.alert('Done', 'Announcement updated.', [{ text: 'OK', onPress: () => navigation.goBack() }]);
      } else {
        Alert.alert('Error', res.error?.message ?? 'Failed to update');
      }
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = () => {
    Alert.alert('Delete Announcement', 'Are you sure? This cannot be undone.', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Delete',
        style: 'destructive',
        onPress: async () => {
          if (!id) return;
          const res = await announcementService.deleteAnnouncement(id);
          if (res.success) navigation.goBack();
          else Alert.alert('Error', res.error?.message ?? 'Failed to delete');
        },
      },
    ]);
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Edit Announcement</Text>
        <TouchableOpacity style={styles.saveBtn} onPress={handleSave} disabled={saving}>
          <Text style={styles.saveBtnText}>{saving ? 'Saving...' : 'Save'}</Text>
        </TouchableOpacity>
      </View>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={styles.flex}>
        <ScrollView style={styles.scroll} contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
          <Text style={styles.label}>Title *</Text>
          <TextInput
            style={styles.input}
            value={title}
            onChangeText={setTitle}
            placeholder="Enter title"
            placeholderTextColor={COLORS.gray400}
          />
          <Text style={styles.label}>Content *</Text>
          <TextInput
            style={[styles.input, styles.textArea]}
            value={content}
            onChangeText={setContent}
            placeholder="Enter content"
            placeholderTextColor={COLORS.gray400}
            multiline
            numberOfLines={6}
          />
          <Text style={styles.label}>Category</Text>
          <View style={styles.categoryRow}>
            {CATEGORIES.map((cat) => (
              <TouchableOpacity
                key={cat}
                style={[styles.categoryChip, category === cat && styles.categoryChipActive]}
                onPress={() => setCategory(cat)}
              >
                <Text style={[styles.categoryChipText, category === cat && styles.categoryChipTextActive]}>{cat}</Text>
              </TouchableOpacity>
            ))}
          </View>
          <View style={styles.row}>
            <Text style={styles.label}>Pin to top</Text>
            <Switch value={isPinned} onValueChange={setIsPinned} trackColor={{ false: COLORS.gray300, true: COLORS.primary + '80' }} thumbColor={isPinned ? COLORS.primary : COLORS.gray500} />
          </View>
          <View style={styles.row}>
            <Text style={styles.label}>Active (visible to guards)</Text>
            <Switch value={isActive} onValueChange={setIsActive} trackColor={{ false: COLORS.gray300, true: COLORS.success + '80' }} thumbColor={isActive ? COLORS.success : COLORS.gray500} />
          </View>
          <TouchableOpacity style={styles.deleteBtn} onPress={handleDelete}>
            <MaterialCommunityIcons name="delete-outline" size={20} color={COLORS.error} />
            <Text style={styles.deleteBtnText}>Delete Announcement</Text>
          </TouchableOpacity>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  flex: { flex: 1 },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  saveBtn: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm },
  saveBtnText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.primary },
  scroll: { flex: 1 },
  content: { padding: SIZES.md },
  label: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.xs, marginTop: SIZES.sm },
  input: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, fontSize: FONTS.body, color: COLORS.textPrimary, ...SHADOWS.small },
  textArea: { minHeight: 120, textAlignVertical: 'top' },
  categoryRow: { flexDirection: 'row', flexWrap: 'wrap', gap: SIZES.xs, marginTop: SIZES.xs },
  categoryChip: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusFull },
  categoryChipActive: { backgroundColor: COLORS.primary },
  categoryChipText: { fontSize: FONTS.caption, color: COLORS.textSecondary, fontWeight: '500' },
  categoryChipTextActive: { color: COLORS.white },
  row: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginTop: SIZES.lg, paddingVertical: SIZES.sm },
  deleteBtn: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', marginTop: SIZES.xl, padding: SIZES.md, backgroundColor: COLORS.error + '15', borderRadius: SIZES.radiusMd, gap: SIZES.sm },
  deleteBtnText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.error },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center' },
});

export default EditAnnouncementScreen;
