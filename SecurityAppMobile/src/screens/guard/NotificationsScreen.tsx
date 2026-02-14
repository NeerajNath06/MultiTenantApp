import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { notificationService } from '../../services/notificationService';
import { authService } from '../../services/authService';

interface NotificationItem {
  id: string;
  title: string;
  message: string;
  time: string;
  type: 'info' | 'warning' | 'success' | 'error';
  read: boolean;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
}

function formatTimeAgo(dateStr: string): string {
  const d = new Date(dateStr);
  const now = new Date();
  const diffMs = now.getTime() - d.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);
  if (diffMins < 60) return `${diffMins} min ago`;
  if (diffHours < 24) return `${diffHours} hours ago`;
  if (diffDays === 1) return 'Yesterday';
  if (diffDays < 7) return `${diffDays} days ago`;
  return d.toLocaleDateString();
}

function typeToIcon(type: string): keyof typeof MaterialCommunityIcons.glyphMap {
  const t = (type || 'info').toLowerCase();
  if (t === 'error' || t === 'alert') return 'alert';
  if (t === 'warning') return 'clipboard-list';
  if (t === 'success') return 'check-circle';
  return 'bell';
}

function NotificationsScreen({ navigation }: any) {
  const [refreshing, setRefreshing] = useState(false);
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [selectedFilter, setSelectedFilter] = useState<'all' | 'unread'>('all');

  const loadNotifications = async () => {
    const user = await authService.getStoredUser();
    if (!user?.id) return;
    const result = await notificationService.getNotifications({ userId: user.id, pageSize: 100 });
    if (result.success && result.data) {
      const items: NotificationItem[] = (result.data.items ?? []).map((n: any) => ({
        id: String(n.id),
        title: n.title ?? '',
        message: n.body ?? n.message ?? '',
        time: formatTimeAgo(n.createdAt ?? n.createdDate ?? new Date().toISOString()),
        type: (n.type ?? 'info').toLowerCase() as NotificationItem['type'],
        read: n.isRead ?? false,
        icon: typeToIcon(n.type),
      }));
      setNotifications(items);
      setUnreadCount(result.data.unreadCount ?? 0);
    }
  };

  useEffect(() => {
    loadNotifications();
  }, []);

  const onRefresh = async () => {
    setRefreshing(true);
    await loadNotifications();
    setRefreshing(false);
  };

  const handleNotificationPress = async (notification: NotificationItem) => {
    if (!notification.read) {
      const user = await authService.getStoredUser();
      if (user?.id) await notificationService.markAsRead(notification.id, user.id);
      setNotifications(notifications.map(n => n.id === notification.id ? { ...n, read: true } : n));
      setUnreadCount(Math.max(0, unreadCount - 1));
    }
    if (notification.title.includes('Shift') || notification.title.includes('Duty')) {
      navigation.navigate('AssignedDuties');
    } else if (notification.title.includes('Check-in') || notification.title.includes('Attendance')) {
      navigation.navigate('AttendanceHistory');
    } else if (notification.title.includes('Document')) {
      navigation.navigate('Documents');
    }
  };

  const handleMarkAllRead = async () => {
    const user = await authService.getStoredUser();
    if (!user?.id) return;
    await notificationService.markAllAsRead(user.id);
    setNotifications(notifications.map(n => ({ ...n, read: true })));
    setUnreadCount(0);
  };

  const getTypeColor = (type: string) => {
    switch (type) {
      case 'info': return COLORS.info;
      case 'warning': return COLORS.warning;
      case 'success': return COLORS.success;
      case 'error': return COLORS.error;
      default: return COLORS.gray500;
    }
  };

  const filteredNotifications = selectedFilter === 'unread'
    ? notifications.filter(n => !n.read)
    : notifications;

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Notifications</Text>
        <TouchableOpacity style={styles.settingsButton} onPress={() => navigation.navigate('NotificationSettings')}>
          <MaterialCommunityIcons name="cog" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        contentContainerStyle={styles.content}
      >
        {/* Filter & Actions */}
        <View style={styles.actionsBar}>
          <View style={styles.filterTabs}>
            <TouchableOpacity 
              style={[styles.filterTab, selectedFilter === 'all' && styles.filterTabActive]}
              onPress={() => setSelectedFilter('all')}
            >
              <Text style={[styles.filterTabText, selectedFilter === 'all' && styles.filterTabTextActive]}>All</Text>
            </TouchableOpacity>
            <TouchableOpacity 
              style={[styles.filterTab, selectedFilter === 'unread' && styles.filterTabActive]}
              onPress={() => setSelectedFilter('unread')}
            >
              <Text style={[styles.filterTabText, selectedFilter === 'unread' && styles.filterTabTextActive]}>
                Unread {unreadCount > 0 && `(${unreadCount})`}
              </Text>
            </TouchableOpacity>
          </View>
          {unreadCount > 0 && (
            <TouchableOpacity onPress={handleMarkAllRead}>
              <Text style={styles.markAllReadText}>Mark all read</Text>
            </TouchableOpacity>
          )}
        </View>

        {/* Quick Actions */}
        <Card style={styles.quickActionsCard}>
          <Text style={styles.quickActionsTitle}>Quick Actions</Text>
          <View style={styles.actionsGrid}>
            <TouchableOpacity style={styles.actionCard} onPress={() => navigation.navigate('AssignedDuties')}>
              <MaterialCommunityIcons name="clock" size={24} color={COLORS.primary} />
              <Text style={styles.actionLabel}>Shift Alerts</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={() => navigation.navigate('IncidentReporting')}>
              <MaterialCommunityIcons name="alert" size={24} color={COLORS.error} />
              <Text style={styles.actionLabel}>Emergency</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={() => navigation.navigate('NotificationSettings')}>
              <MaterialCommunityIcons name="cog" size={24} color={COLORS.secondary} />
              <Text style={styles.actionLabel}>Settings</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={() => navigation.navigate('AttendanceHistory')}>
              <MaterialCommunityIcons name="archive" size={24} color={COLORS.info} />
              <Text style={styles.actionLabel}>History</Text>
            </TouchableOpacity>
          </View>
        </Card>

        {/* Notifications List */}
        {filteredNotifications.length === 0 ? (
          <View style={styles.emptyState}>
            <MaterialCommunityIcons name="bell-off-outline" size={64} color={COLORS.gray300} />
            <Text style={styles.emptyTitle}>No notifications</Text>
            <Text style={styles.emptyText}>
              {selectedFilter === 'unread' ? 'You have read all notifications' : 'You have no notifications yet'}
            </Text>
          </View>
        ) : (
          filteredNotifications.map((notification) => (
            <TouchableOpacity
              key={String(notification.id)}
              style={[styles.notificationCard, !notification.read && styles.notificationCardUnread]}
              onPress={() => handleNotificationPress(notification)}
            >
              <View style={[styles.notificationIcon, { backgroundColor: getTypeColor(notification.type) + '15' }]}>
                <MaterialCommunityIcons name={notification.icon} size={24} color={getTypeColor(notification.type)} />
              </View>
              <View style={styles.notificationContent}>
                <View style={styles.notificationHeader}>
                  <Text style={[styles.notificationTitle, !notification.read && styles.notificationTitleUnread]}>
                    {notification.title}
                  </Text>
                  {!notification.read && <View style={styles.unreadDot} />}
                </View>
                <Text style={styles.notificationMessage} numberOfLines={2}>{notification.message}</Text>
                <Text style={styles.notificationTime}>{notification.time}</Text>
              </View>
            </TouchableOpacity>
          ))
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  settingsButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  content: { padding: SIZES.md },
  actionsBar: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SIZES.md },
  filterTabs: { flexDirection: 'row', backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusSm, padding: 4 },
  filterTab: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  filterTabActive: { backgroundColor: COLORS.white, ...SHADOWS.small },
  filterTabText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  filterTabTextActive: { color: COLORS.primary, fontWeight: '600' },
  markAllReadText: { fontSize: FONTS.bodySmall, color: COLORS.primary, fontWeight: '500' },
  quickActionsCard: { marginBottom: SIZES.md, padding: SIZES.md },
  quickActionsTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  actionsGrid: { flexDirection: 'row', justifyContent: 'space-around' },
  actionCard: { alignItems: 'center', padding: SIZES.sm },
  actionLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  notificationCard: { flexDirection: 'row', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  notificationCardUnread: { backgroundColor: COLORS.primary + '08', borderLeftWidth: 3, borderLeftColor: COLORS.primary },
  notificationIcon: { width: 48, height: 48, borderRadius: 24, justifyContent: 'center', alignItems: 'center', marginRight: SIZES.md },
  notificationContent: { flex: 1 },
  notificationHeader: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.xs },
  notificationTitle: { fontSize: FONTS.body, fontWeight: '500', color: COLORS.textPrimary, flex: 1 },
  notificationTitleUnread: { fontWeight: '600' },
  unreadDot: { width: 8, height: 8, borderRadius: 4, backgroundColor: COLORS.primary, marginLeft: SIZES.xs },
  notificationMessage: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, lineHeight: 18, marginBottom: SIZES.xs },
  notificationTime: { fontSize: FONTS.caption, color: COLORS.gray400 },
  emptyState: { alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.xxl },
  emptyTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginTop: SIZES.md },
  emptyText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginTop: SIZES.xs, textAlign: 'center' },
});

export default NotificationsScreen;
