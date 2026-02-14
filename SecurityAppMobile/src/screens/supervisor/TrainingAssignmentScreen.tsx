import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, RefreshControl, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { trainingService, type TrainingRecordItem } from '../../services/trainingService';
import { guardService, type GuardItem } from '../../services/guardService';

interface TrainingAgg {
  id: string;
  title: string;
  category: string;
  assignedGuards: number;
  completedGuards: number;
  mandatory?: boolean;
  deadline?: string;
}

interface GuardTrainingAgg {
  id: string;
  name: string;
  trainingsAssigned: number;
  trainingsCompleted: number;
  pendingMandatory: number;
}

function TrainingAssignmentScreen({ navigation }: any) {
  const [activeTab, setActiveTab] = useState<'trainings' | 'guards'>('trainings');
  const [trainings, setTrainings] = useState<TrainingAgg[]>([]);
  const [guards, setGuards] = useState<GuardTrainingAgg[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [supervisorId, setSupervisorId] = useState<string | null>(null);

  const load = useCallback(async () => {
    let uid: string | null = null;
    try {
      const userData = await AsyncStorage.getItem('userData');
      if (userData) {
        const parsed = JSON.parse(userData);
        uid = parsed?.id ?? parsed?.userId ?? null;
      }
    } catch {}
    setSupervisorId(uid);

    const [recordsRes, guardsRes] = await Promise.all([
      trainingService.getTrainingRecords({ pageSize: 200 }),
      guardService.getGuards({ supervisorId: uid ?? undefined, pageSize: 100 }),
    ]);

    if (recordsRes.success && recordsRes.data?.items) {
      const items = recordsRes.data.items as TrainingRecordItem[];
      const byType = new Map<string, { assigned: Set<string>; completed: number }>();
      items.forEach(r => {
        const key = r.trainingType + '|' + r.trainingName;
        if (!byType.has(key)) byType.set(key, { assigned: new Set(), completed: 0 });
        const agg = byType.get(key)!;
        agg.assigned.add(r.guardId);
        if ((r.status || '').toLowerCase() === 'completed') agg.completed += 1;
      });
      const list: TrainingAgg[] = Array.from(byType.entries()).map(([key, agg], idx) => {
        const [category, title] = key.split('|');
        return {
          id: key + idx,
          title: title || key,
          category: category || 'General',
          assignedGuards: agg.assigned.size,
          completedGuards: agg.completed,
        };
      });
      setTrainings(list);
    } else {
      setTrainings([]);
    }

    if (guardsRes.success && guardsRes.data?.items) {
      const guardList = guardsRes.data.items as GuardItem[];
      const recordRes2 = await trainingService.getTrainingRecords({ pageSize: 500 });
      const allRecords = (recordRes2.success && recordRes2.data?.items) ? recordRes2.data.items as TrainingRecordItem[] : [];
      const byGuard = new Map<string, { completed: number; total: number; expired: number }>();
      guardList.forEach(g => {
        byGuard.set(g.id, { completed: 0, total: 0, expired: 0 });
      });
      allRecords.forEach(r => {
        const agg = byGuard.get(r.guardId);
        if (agg) {
          agg.total += 1;
          if ((r.status || '').toLowerCase() === 'completed') agg.completed += 1;
          if (r.expiryDate && new Date(r.expiryDate) < new Date()) agg.expired += 1;
        }
      });
      const guardAggs: GuardTrainingAgg[] = guardList.map(g => {
        const agg = byGuard.get(g.id) ?? { completed: 0, total: 0, expired: 0 };
        return {
          id: g.id,
          name: [g.firstName, g.lastName].filter(Boolean).join(' ') || g.guardCode || 'Guard',
          trainingsAssigned: agg.total,
          trainingsCompleted: agg.completed,
          pendingMandatory: agg.expired,
        };
      });
      setGuards(guardAggs);
    } else {
      setGuards([]);
    }
  }, []);

  useEffect(() => {
    load().finally(() => setLoading(false));
  }, [load]);

  const onRefresh = async () => {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  };

  const handleAssignTraining = (training: TrainingAgg) => {
    Alert.alert(
      'Assign Training',
      `Assign "${training.title}" to guards?`,
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Select Guards', onPress: () => {} },
        { text: 'Assign All', onPress: () => Alert.alert('Success', 'Training assigned to all guards') }
      ]
    );
  };

  const handleGuardTrainings = (guard: GuardTrainingAgg) => {
    Alert.alert(
      guard.name,
      `Trainings Assigned: ${guard.trainingsAssigned}\nCompleted: ${guard.trainingsCompleted}\nPending Mandatory: ${guard.pendingMandatory}`,
      [
        { text: 'Close', style: 'cancel' },
        { text: 'Send Reminder', onPress: () => Alert.alert('Reminder Sent', 'Training reminder sent to guard') }
      ]
    );
  };

  const getCategoryColor = (category: string) => {
    switch (category) {
      case 'Safety': return COLORS.error;
      case 'Medical': return COLORS.success;
      case 'Technical': return COLORS.primaryBlue;
      case 'Soft Skills': return COLORS.warning;
      default: return COLORS.gray500;
    }
  };

  const stats = {
    totalTrainings: trainings.length,
    mandatoryPending: trainings.filter(t => t.mandatory && t.completedGuards < t.assignedGuards).length,
    avgCompletion: trainings.length > 0
      ? Math.round(trainings.reduce((sum, t) => sum + (t.assignedGuards ? (t.completedGuards / t.assignedGuards * 100) : 0), 0) / trainings.length)
      : 0,
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Training Assignment</Text>
        <TouchableOpacity style={styles.addBtn}>
          <MaterialCommunityIcons name="plus" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      {/* Stats */}
      <View style={styles.statsRow}>
        <View style={[styles.statCard, { backgroundColor: COLORS.primaryBlue + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.primaryBlue }]}>{stats.totalTrainings}</Text>
          <Text style={styles.statLabel}>Total Trainings</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.error + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.error }]}>{stats.mandatoryPending}</Text>
          <Text style={styles.statLabel}>Mandatory Pending</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.success }]}>{stats.avgCompletion}%</Text>
          <Text style={styles.statLabel}>Avg Completion</Text>
        </View>
      </View>

      {/* Tabs */}
      <View style={styles.tabContainer}>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'trainings' && styles.tabActive]}
          onPress={() => setActiveTab('trainings')}
        >
          <MaterialCommunityIcons name="school" size={20} color={activeTab === 'trainings' ? COLORS.primary : COLORS.gray500} />
          <Text style={[styles.tabText, activeTab === 'trainings' && styles.tabTextActive]}>Trainings</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'guards' && styles.tabActive]}
          onPress={() => setActiveTab('guards')}
        >
          <MaterialCommunityIcons name="account-group" size={20} color={activeTab === 'guards' ? COLORS.primary : COLORS.gray500} />
          <Text style={[styles.tabText, activeTab === 'guards' && styles.tabTextActive]}>Guards</Text>
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.content}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {loading ? (
          <View style={styles.loadingWrap}>
            <ActivityIndicator size="large" color={COLORS.primary} />
          </View>
        ) : activeTab === 'trainings' ? (
          <>
            {trainings.length === 0 ? (
              <View style={styles.emptyWrap}>
                <Text style={styles.emptyText}>No training records yet</Text>
              </View>
            ) : (
            trainings.map((training) => {
              const progress = training.assignedGuards ? Math.round((training.completedGuards / training.assignedGuards) * 100) : 0;
              return (
                <TouchableOpacity key={training.id} style={styles.trainingCard} onPress={() => handleAssignTraining(training)}>
                  <View style={styles.trainingHeader}>
                    <View style={[styles.categoryBadge, { backgroundColor: getCategoryColor(training.category) + '15' }]}>
                      <Text style={[styles.categoryText, { color: getCategoryColor(training.category) }]}>{training.category}</Text>
                    </View>
                    {training.mandatory && (
                      <View style={styles.mandatoryBadge}>
                        <MaterialCommunityIcons name="star" size={12} color={COLORS.error} />
                        <Text style={styles.mandatoryText}>Mandatory</Text>
                      </View>
                    )}
                  </View>

                  <Text style={styles.trainingTitle}>{training.title}</Text>

                  <View style={styles.progressSection}>
                    <View style={styles.progressHeader}>
                      <Text style={styles.progressText}>{training.completedGuards}/{training.assignedGuards} guards completed</Text>
                      <Text style={styles.progressPercent}>{progress}%</Text>
                    </View>
                    <View style={styles.progressBar}>
                      <View style={[styles.progressFill, { width: `${progress}%`, backgroundColor: progress === 100 ? COLORS.success : COLORS.primaryBlue }]} />
                    </View>
                  </View>
                </TouchableOpacity>
              );
            })
            )}
          </>
        ) : (
          <>
            {guards.length === 0 ? (
              <View style={styles.emptyWrap}>
                <Text style={styles.emptyText}>No guards</Text>
              </View>
            ) : (
            guards.map((guard) => (
              <TouchableOpacity key={guard.id} style={styles.guardCard} onPress={() => handleGuardTrainings(guard)}>
                <View style={styles.guardAvatar}>
                  <Text style={styles.avatarText}>{guard.name.split(' ').map(n => n[0]).join('')}</Text>
                </View>
                <View style={styles.guardInfo}>
                  <Text style={styles.guardName}>{guard.name}</Text>
                  <View style={styles.guardStats}>
                    <Text style={styles.guardStatText}>{guard.trainingsCompleted}/{guard.trainingsAssigned} completed</Text>
                    {guard.pendingMandatory > 0 && (
                      <View style={styles.pendingBadge}>
                        <Text style={styles.pendingText}>{guard.pendingMandatory} mandatory pending</Text>
                      </View>
                    )}
                  </View>
                </View>
                <TouchableOpacity style={styles.reminderBtn} onPress={() => Alert.alert('Reminder Sent')}>
                  <MaterialCommunityIcons name="bell-ring" size={20} color={COLORS.warning} />
                </TouchableOpacity>
              </TouchableOpacity>
            ))
            )}
          </>
        )}

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
  addBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  statsRow: { flexDirection: 'row', paddingHorizontal: SIZES.md, paddingTop: SIZES.md, gap: SIZES.sm },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.md, borderRadius: SIZES.radiusMd },
  statValue: { fontSize: FONTS.h3, fontWeight: 'bold' },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2, textAlign: 'center' },
  tabContainer: { flexDirection: 'row', marginHorizontal: SIZES.md, marginTop: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: 4, ...SHADOWS.small },
  tab: { flex: 1, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm, gap: SIZES.xs },
  tabActive: { backgroundColor: COLORS.primary + '10' },
  tabText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  tabTextActive: { color: COLORS.primary, fontWeight: '600' },
  content: { padding: SIZES.md },
  trainingCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  trainingHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.sm },
  categoryBadge: { paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  categoryText: { fontSize: FONTS.tiny, fontWeight: '600' },
  mandatoryBadge: { flexDirection: 'row', alignItems: 'center', gap: 2 },
  mandatoryText: { fontSize: FONTS.tiny, color: COLORS.error, fontWeight: '600' },
  trainingTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  trainingMeta: { flexDirection: 'row', gap: SIZES.md, marginBottom: SIZES.sm },
  metaItem: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  metaText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  progressSection: {},
  progressHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 4 },
  progressText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  progressPercent: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.textPrimary },
  progressBar: { height: 6, backgroundColor: COLORS.gray200, borderRadius: 3, overflow: 'hidden' },
  progressFill: { height: '100%', borderRadius: 3 },
  guardCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  guardAvatar: { width: 48, height: 48, borderRadius: 24, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  avatarText: { fontSize: FONTS.body, fontWeight: 'bold', color: COLORS.white },
  guardInfo: { flex: 1, marginLeft: SIZES.sm },
  guardName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  guardStats: { marginTop: 4 },
  guardStatText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  pendingBadge: { marginTop: 4 },
  pendingText: { fontSize: FONTS.tiny, color: COLORS.error, fontWeight: '500' },
  reminderBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.warning + '15', justifyContent: 'center', alignItems: 'center' },
  loadingWrap: { paddingVertical: SIZES.xl * 2, alignItems: 'center' },
  emptyWrap: { paddingVertical: SIZES.lg, alignItems: 'center' },
  emptyText: { fontSize: FONTS.body, color: COLORS.textSecondary },
});

export default TrainingAssignmentScreen;
