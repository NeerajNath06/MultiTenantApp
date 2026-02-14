import React, { useState, useEffect, useCallback } from 'react';
import { useFocusEffect } from '@react-navigation/native';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, RefreshControl, ActivityIndicator, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { announcementService, type AnnouncementItem } from '../../services/announcementService';

interface AnnouncementDisplay extends AnnouncementItem {
  isRead: boolean;
  postedBy: string;
  postedAtLabel: string;
}

function formatPostedAt(iso: string): string {
  try {
    const d = new Date(iso);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} mins ago`;
    if (diffHours < 24) return `${diffHours} hours ago`;
    if (diffDays < 7) return `${diffDays} days ago`;
    return d.toLocaleDateString();
  } catch {
    return iso;
  }
}

function AnnouncementsScreen({ navigation, route }: any) {
  const manageMode = route?.params?.manageMode === true;
  const [refreshing, setRefreshing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [announcements, setAnnouncements] = useState<AnnouncementDisplay[]>([]);
  const [readIds, setReadIds] = useState<Set<string>>(new Set());

  const load = useCallback(async () => {
    const res = await announcementService.getAnnouncements({
      pageSize: 100,
      includeInactive: manageMode,
    });
    if (res.success && res.data?.items) {
      const list: AnnouncementDisplay[] = res.data.items.map((a: AnnouncementItem) => ({
        ...a,
        isRead: false,
        postedBy: a.postedByName ?? 'Admin',
        postedAtLabel: formatPostedAt(a.postedAt),
      }));
      setAnnouncements(list);
    } else {
      setAnnouncements([]);
    }
  }, [manageMode]);

  useEffect(() => {
    load().finally(() => setLoading(false));
  }, [load]);

  useFocusEffect(
    useCallback(() => {
      if (manageMode) load();
    }, [manageMode, load])
  );

  const handleDelete = (announcement: AnnouncementDisplay) => {
    Alert.alert(
      'Delete Announcement',
      `Delete "${announcement.title}"?`,
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Delete',
          style: 'destructive',
          onPress: async () => {
            const res = await announcementService.deleteAnnouncement(announcement.id);
            if (res.success) load();
            else Alert.alert('Error', res.error?.message ?? 'Failed to delete');
          },
        },
      ]
    );
  };

  const handleEdit = (announcement: AnnouncementDisplay) => {
    navigation.navigate('EditAnnouncement', { id: announcement.id });
  };

  const onRefresh = async () => {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  };

  const handleAnnouncementPress = (announcement: AnnouncementDisplay) => {
    setReadIds(prev => new Set(prev).add(announcement.id));
  };

  const displayList = announcements.map(a => ({
    ...a,
    isRead: readIds.has(a.id),
    postedBy: a.postedByName ?? 'Admin',
    postedAtLabel: formatPostedAt(a.postedAt),
  }));
  const unreadCount = displayList.filter(a => !a.isRead).length;
  const pinnedAnnouncements = displayList.filter(a => a.isPinned);
  const otherAnnouncements = displayList.filter(a => !a.isPinned);

  const getCategoryIcon = (category: string): keyof typeof MaterialCommunityIcons.glyphMap => {
    const c = (category || 'general').toLowerCase();
    switch (c) {
      case 'urgent': return 'alert-circle';
      case 'policy': return 'file-document';
      case 'event': return 'calendar-star';
      case 'training': return 'school';
      default: return 'bullhorn';
    }
  };

  const getCategoryColor = (category: string) => {
    const c = (category || 'general').toLowerCase();
    switch (c) {
      case 'urgent': return COLORS.error;
      case 'policy': return COLORS.primaryBlue;
      case 'event': return COLORS.success;
      case 'training': return COLORS.warning;
      default: return COLORS.secondary;
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <View style={styles.headerCenter}>
          <Text style={styles.headerTitle}>{manageMode ? 'Manage Announcements' : 'Announcements'}</Text>
          {!manageMode && unreadCount > 0 && (
            <View style={styles.unreadBadge}>
              <Text style={styles.unreadText}>{unreadCount} new</Text>
            </View>
          )}
        </View>
        {manageMode ? (
          <TouchableOpacity style={styles.filterBtn} onPress={() => navigation.navigate('CreateAnnouncement')}>
            <MaterialCommunityIcons name="plus" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
        ) : (
          <TouchableOpacity style={styles.filterBtn}>
            <MaterialCommunityIcons name="filter-variant" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
        )}
      </View>

      {loading ? (
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
        </View>
      ) : (
      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        contentContainerStyle={styles.content}
      >
        {announcements.length === 0 ? (
          <View style={styles.emptyWrap}>
            <MaterialCommunityIcons name="bullhorn-outline" size={48} color={COLORS.gray400} />
            <Text style={styles.emptyText}>No announcements yet</Text>
          </View>
        ) : (
        <>
        {/* Pinned Announcements */}
        {pinnedAnnouncements.length > 0 && (
          <View style={styles.section}>
            <View style={styles.sectionHeader}>
              <MaterialCommunityIcons name="pin" size={18} color={COLORS.primary} />
              <Text style={styles.sectionTitle}>Pinned</Text>
            </View>
            {pinnedAnnouncements.map((announcement) => (
              <TouchableOpacity
                key={announcement.id}
                style={[styles.announcementCard, !announcement.isRead && !manageMode && styles.unreadCard]}
                onPress={() => !manageMode && handleAnnouncementPress(announcement)}
                onLongPress={manageMode ? () => Alert.alert('Options', 'Edit or Delete?', [
                  { text: 'Cancel', style: 'cancel' },
                  { text: 'Edit', onPress: () => handleEdit(announcement) },
                  { text: 'Delete', style: 'destructive', onPress: () => handleDelete(announcement) },
                ]) : undefined}
              >
                <View style={styles.cardHeader}>
                  <View style={[styles.categoryIcon, { backgroundColor: getCategoryColor(announcement.category) + '15' }]}>
                    <MaterialCommunityIcons name={getCategoryIcon(announcement.category)} size={20} color={getCategoryColor(announcement.category)} />
                  </View>
                  <View style={styles.cardMeta}>
                    <View style={[styles.categoryBadge, { backgroundColor: getCategoryColor(announcement.category) + '15' }]}>
                      <Text style={[styles.categoryText, { color: getCategoryColor(announcement.category) }]}>
                        {(announcement.category || 'general').charAt(0).toUpperCase() + (announcement.category || 'general').slice(1)}
                      </Text>
                    </View>
                    <Text style={styles.postedTime}>{announcement.postedAtLabel}</Text>
                  </View>
                  {!announcement.isRead && <View style={styles.unreadDot} />}
                </View>

                <Text style={styles.cardTitle}>{announcement.title}</Text>
                <Text style={styles.cardContent} numberOfLines={2}>{announcement.content}</Text>

                <View style={styles.cardFooter}>
                  <Text style={styles.postedBy}>By {announcement.postedBy}</Text>
                  {manageMode && (
                    <View style={styles.cardActions}>
                      <TouchableOpacity onPress={() => handleEdit(announcement)} style={styles.actionBtn}>
                        <MaterialCommunityIcons name="pencil" size={18} color={COLORS.primaryBlue} />
                      </TouchableOpacity>
                      <TouchableOpacity onPress={() => handleDelete(announcement)} style={styles.actionBtn}>
                        <MaterialCommunityIcons name="delete-outline" size={18} color={COLORS.error} />
                      </TouchableOpacity>
                    </View>
                  )}
                </View>
              </TouchableOpacity>
            ))}
          </View>
        )}

        {/* Other Announcements */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Recent</Text>
          {otherAnnouncements.map((announcement) => (
            <TouchableOpacity
              key={announcement.id}
              style={[styles.announcementCard, !announcement.isRead && !manageMode && styles.unreadCard]}
              onPress={() => !manageMode && handleAnnouncementPress(announcement)}
              onLongPress={manageMode ? () => Alert.alert('Options', 'Edit or Delete?', [
                { text: 'Cancel', style: 'cancel' },
                { text: 'Edit', onPress: () => handleEdit(announcement) },
                { text: 'Delete', style: 'destructive', onPress: () => handleDelete(announcement) },
              ]) : undefined}
            >
              <View style={styles.cardHeader}>
                <View style={[styles.categoryIcon, { backgroundColor: getCategoryColor(announcement.category) + '15' }]}>
                  <MaterialCommunityIcons name={getCategoryIcon(announcement.category)} size={20} color={getCategoryColor(announcement.category)} />
                </View>
                <View style={styles.cardMeta}>
                  <View style={[styles.categoryBadge, { backgroundColor: getCategoryColor(announcement.category) + '15' }]}>
                    <Text style={[styles.categoryText, { color: getCategoryColor(announcement.category) }]}>
                      {(announcement.category || 'general').charAt(0).toUpperCase() + (announcement.category || 'general').slice(1)}
                    </Text>
                  </View>
                  <Text style={styles.postedTime}>{announcement.postedAtLabel}</Text>
                </View>
                {!announcement.isRead && <View style={styles.unreadDot} />}
              </View>

              <Text style={styles.cardTitle}>{announcement.title}</Text>
              <Text style={styles.cardContent} numberOfLines={2}>{announcement.content}</Text>

              <View style={styles.cardFooter}>
                <Text style={styles.postedBy}>By {announcement.postedBy}</Text>
                {manageMode && (
                  <View style={styles.cardActions}>
                    <TouchableOpacity onPress={() => handleEdit(announcement)} style={styles.actionBtn}>
                      <MaterialCommunityIcons name="pencil" size={18} color={COLORS.primaryBlue} />
                    </TouchableOpacity>
                    <TouchableOpacity onPress={() => handleDelete(announcement)} style={styles.actionBtn}>
                      <MaterialCommunityIcons name="delete-outline" size={18} color={COLORS.error} />
                    </TouchableOpacity>
                  </View>
                )}
              </View>
            </TouchableOpacity>
          ))}
        </View>
        </>
        )}
      </ScrollView>
      )}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerCenter: { flexDirection: 'row', alignItems: 'center', gap: SIZES.sm },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  unreadBadge: { backgroundColor: COLORS.error, paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusFull },
  unreadText: { fontSize: FONTS.tiny, color: COLORS.white, fontWeight: '600' },
  filterBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  content: { padding: SIZES.md },
  section: { marginBottom: SIZES.lg },
  sectionHeader: { flexDirection: 'row', alignItems: 'center', gap: SIZES.xs, marginBottom: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  announcementCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  unreadCard: { borderLeftWidth: 4, borderLeftColor: COLORS.primary },
  cardHeader: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.sm },
  categoryIcon: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  cardMeta: { flex: 1, marginLeft: SIZES.sm },
  categoryBadge: { paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm, alignSelf: 'flex-start' },
  categoryText: { fontSize: FONTS.tiny, fontWeight: '600' },
  postedTime: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2 },
  unreadDot: { width: 10, height: 10, borderRadius: 5, backgroundColor: COLORS.primary },
  cardTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.xs },
  cardContent: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, lineHeight: 20 },
  cardFooter: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: SIZES.sm, paddingTop: SIZES.sm, borderTopWidth: 1, borderTopColor: COLORS.gray100 },
  postedBy: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  attachmentBadge: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  attachmentText: { fontSize: FONTS.caption, color: COLORS.gray500 },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  emptyWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', paddingVertical: SIZES.xl * 2 },
  emptyText: { fontSize: FONTS.body, color: COLORS.textSecondary, marginTop: SIZES.sm },
  cardActions: { flexDirection: 'row', gap: SIZES.sm },
  actionBtn: { padding: 4 },
});

export default AnnouncementsScreen;
