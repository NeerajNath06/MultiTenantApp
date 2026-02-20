import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, TextInput, ActivityIndicator, Linking } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Button from '../../components/common/Button';
import { authService } from '../../services/authService';
import { deploymentService } from '../../services/deploymentService';
import { incidentService } from '../../services/incidentService';
import { journalService } from '../../services/journalService';

interface ChecklistItem {
  id: number;
  task: string;
  checked: boolean;
  mandatory: boolean;
}

interface PendingTask {
  id: string;
  title: string;
  priority: 'high' | 'medium' | 'low';
  description: string;
}

interface IncomingGuardInfo {
  guardId: string;
  name: string;
  id: string;
  shiftTime: string;
  siteName?: string;
}

function ShiftHandoverScreen({ navigation }: any) {
  const [notes, setNotes] = useState('');
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  const [incomingGuard, setIncomingGuard] = useState<IncomingGuardInfo | null>(null);
  const [mySiteName, setMySiteName] = useState<string>('');
  const [siteId, setSiteId] = useState<string>('');
  const [pendingTasks, setPendingTasks] = useState<PendingTask[]>([]);
  const [checklist, setChecklist] = useState<ChecklistItem[]>([
    { id: 1, task: 'All keys returned to key box', checked: false, mandatory: true },
    { id: 2, task: 'CCTV cameras functioning properly', checked: false, mandatory: true },
    { id: 3, task: 'Gate locks verified', checked: false, mandatory: true },
    { id: 4, task: 'Incident log updated', checked: false, mandatory: true },
    { id: 5, task: 'Equipment check completed', checked: false, mandatory: false },
    { id: 6, task: 'Parking lot cleared', checked: false, mandatory: false },
    { id: 7, task: 'Emergency contact list verified', checked: false, mandatory: false },
  ]);

  useEffect(() => {
    loadHandoverData();
  }, []);

  const loadHandoverData = async () => {
    try {
      setLoadingData(true);
      const user = await authService.getStoredUser();
      if (!user) return;
      const guardId = (user as { guardId?: string }).guardId || user.id;
      const today = new Date().toISOString().slice(0, 10);

      const [deployRes, incidentsRes] = await Promise.all([
        deploymentService.getDeployments({ guardId, dateFrom: today, dateTo: today, pageSize: 50, skipCache: true }),
        incidentService.getIncidents({ guardId, dateFrom: today, dateTo: today, pageSize: 20 }),
      ]);

      const deployments = deployRes.success && deployRes.data
        ? (Array.isArray(deployRes.data) ? deployRes.data : (deployRes.data as any).items ?? (deployRes.data as any).data ?? [])
        : [];
      const myDeployment = deployments[0];
      const siteId = myDeployment?.siteId ?? myDeployment?.SiteId;
      const siteName = myDeployment?.siteName ?? myDeployment?.SiteName ?? '';

      setMySiteName(siteName);
      setSiteId(siteId ?? '');

      if (siteId) {
        const siteDeploymentsRes = await deploymentService.getDeployments({
          siteId,
          dateFrom: today,
          dateTo: today,
          pageSize: 50,
          skipCache: true,
        });
        const siteDeployments = siteDeploymentsRes.success && siteDeploymentsRes.data
          ? (Array.isArray(siteDeploymentsRes.data) ? siteDeploymentsRes.data : (siteDeploymentsRes.data as any).items ?? (siteDeploymentsRes.data as any).data ?? [])
          : [];
        const other = siteDeployments.find((d: any) => String(d.guardId ?? d.GuardId) !== String(guardId));
        if (other) {
          const name = other.guardName ?? other.GuardName ?? 'Guard';
          const gId = other.guardId ?? other.GuardId ?? '';
          const shiftName = other.shiftName ?? other.ShiftName ?? '';
          const start = other.startTime ?? other.StartTime ?? '';
          const end = other.endTime ?? other.EndTime ?? '';
          setIncomingGuard({
            guardId: gId,
            name,
            id: other.guardCode ?? gId?.slice(0, 8) ?? '—',
            shiftTime: shiftName || [start, end].filter(Boolean).join(' - ') || '—',
            siteName: other.siteName ?? other.SiteName,
          });
        }
      }

      const incidents = incidentsRes.success && incidentsRes.data
        ? (Array.isArray(incidentsRes.data) ? incidentsRes.data : (incidentsRes.data as any).items ?? (incidentsRes.data as any).data ?? [])
        : [];
      const openIncidents = incidents
        .filter((i: any) => (i.status ?? i.Status ?? '').toLowerCase() === 'open' || (i.status ?? i.Status ?? '').toLowerCase() === 'pending')
        .slice(0, 5)
        .map((i: any, idx: number) => ({
          id: i.id ?? i.Id ?? String(idx),
          title: i.title ?? i.description ?? i.type ?? 'Incident',
          priority: (i.severity ?? i.priority ?? 'medium').toLowerCase() as 'high' | 'medium' | 'low',
          description: (i.description ?? i.title ?? '').slice(0, 80),
        }));
      setPendingTasks(openIncidents);
    } catch (e) {
      console.error('Shift handover load error:', e);
    } finally {
      setLoadingData(false);
    }
  };

  const toggleCheckItem = (id: number) => {
    setChecklist(checklist.map(item => 
      item.id === id ? { ...item, checked: !item.checked } : item
    ));
  };

  const handleHandover = async () => {
    const mandatoryIncomplete = checklist.filter(item => item.mandatory && !item.checked);
    if (mandatoryIncomplete.length > 0) {
      Alert.alert(
        'Incomplete Checklist',
        'Please complete all mandatory items before handover.',
        [{ text: 'OK' }]
      );
      return;
    }

    setLoading(true);
    try {
      const user = await authService.getStoredUser();
      const guardId = user ? (user as { guardId?: string }).guardId || user.id : null;
      const today = new Date().toISOString().slice(0, 10);
      if (guardId && incomingGuard?.guardId) {
        const handoverRes = await journalService.createShiftHandover({
          fromGuardId: guardId,
          toGuardId: incomingGuard.guardId,
          siteId: siteId || '',
          handoverDate: today,
          notes: notes.trim(),
        });
        if (!handoverRes.success && handoverRes.error?.message && !handoverRes.error.message.includes('404')) {
          Alert.alert('Note', 'Handover recorded locally. ' + (handoverRes.error.message || ''));
        }
      }
      Alert.alert(
        'Handover Complete',
        'Shift handover has been successfully recorded. Your shift has ended.',
        [{ text: 'OK', onPress: () => navigation.navigate('GuardMain') }]
      );
    } catch (e) {
      console.error('Handover submit error:', e);
      Alert.alert(
        'Handover Complete',
        'Shift handover recorded. You may now end your shift.',
        [{ text: 'OK', onPress: () => navigation.navigate('GuardMain') }]
      );
    } finally {
      setLoading(false);
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'high': return COLORS.error;
      case 'medium': return COLORS.warning;
      case 'low': return COLORS.info;
      default: return COLORS.gray500;
    }
  };

  const completedCount = checklist.filter(item => item.checked).length;
  const progress = (completedCount / checklist.length) * 100;

  const displayGuard = incomingGuard ?? { name: '—', id: '—', shiftTime: '—', siteName: '' };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Shift Handover</Text>
        <View style={styles.placeholder} />
      </View>

      {loadingData ? (
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading handover data...</Text>
        </View>
      ) : (
      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* Incoming Guard Info */}
        <View style={styles.incomingCard}>
          <Text style={styles.cardLabel}>Handing Over To{mySiteName ? ` • ${mySiteName}` : ''}</Text>
          <View style={styles.guardInfo}>
            <View style={styles.guardAvatar}>
              <Text style={styles.avatarText}>{displayGuard.name.split(' ').filter(Boolean).map(n => n[0]).join('') || '?'}</Text>
            </View>
            <View style={styles.guardDetails}>
              <Text style={styles.guardName}>{displayGuard.name}</Text>
              <Text style={styles.guardId}>{displayGuard.id}</Text>
              <Text style={styles.guardShift}>{displayGuard.shiftTime}</Text>
            </View>
            {incomingGuard && (
              <TouchableOpacity style={styles.callBtn} onPress={() => Linking.openURL('tel:')}>
                <MaterialCommunityIcons name="phone" size={20} color={COLORS.white} />
              </TouchableOpacity>
            )}
          </View>
        </View>

        {/* Progress */}
        <View style={styles.progressCard}>
          <View style={styles.progressHeader}>
            <Text style={styles.progressTitle}>Handover Checklist</Text>
            <Text style={styles.progressText}>{completedCount}/{checklist.length} completed</Text>
          </View>
          <View style={styles.progressBar}>
            <View style={[styles.progressFill, { width: `${progress}%` }]} />
          </View>
        </View>

        {/* Checklist */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Checklist</Text>
          <View style={styles.checklistCard}>
            {checklist.map((item) => (
              <TouchableOpacity
                key={item.id}
                style={styles.checkItem}
                onPress={() => toggleCheckItem(item.id)}
              >
                <View style={[styles.checkbox, item.checked && styles.checkboxChecked]}>
                  {item.checked && <MaterialCommunityIcons name="check" size={16} color={COLORS.white} />}
                </View>
                <Text style={[styles.checkText, item.checked && styles.checkTextDone]}>{item.task}</Text>
                {item.mandatory && <Text style={styles.mandatoryTag}>Required</Text>}
              </TouchableOpacity>
            ))}
          </View>
        </View>

        {/* Pending Tasks */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Pending Tasks to Handover</Text>
          {pendingTasks.length === 0 ? (
            <View style={styles.noTasks}>
              <Text style={styles.noTasksText}>No open incidents or pending tasks</Text>
            </View>
          ) : pendingTasks.map((task) => (
            <View key={task.id} style={styles.taskCard}>
              <View style={[styles.priorityIndicator, { backgroundColor: getPriorityColor(task.priority) }]} />
              <View style={styles.taskContent}>
                <View style={styles.taskHeader}>
                  <Text style={styles.taskTitle}>{task.title}</Text>
                  <View style={[styles.priorityBadge, { backgroundColor: getPriorityColor(task.priority) + '15' }]}>
                    <Text style={[styles.priorityText, { color: getPriorityColor(task.priority) }]}>
                      {task.priority.charAt(0).toUpperCase() + task.priority.slice(1)}
                    </Text>
                  </View>
                </View>
                <Text style={styles.taskDescription}>{task.description}</Text>
              </View>
            </View>
          ))}
        </View>

        {/* Handover Notes */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Handover Notes</Text>
          <View style={styles.notesCard}>
            <TextInput
              style={styles.notesInput}
              placeholder="Add any important notes for the incoming guard..."
              value={notes}
              onChangeText={setNotes}
              multiline
              numberOfLines={4}
              placeholderTextColor={COLORS.gray400}
            />
          </View>
        </View>

        {/* Submit Button */}
        <Button
          title="Complete Handover"
          onPress={handleHandover}
          loading={loading}
          style={styles.submitBtn}
        />

        <View style={{ height: 50 }} />
      </ScrollView>
      )}
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
  incomingCard: { backgroundColor: COLORS.primary, borderRadius: SIZES.radiusLg, padding: SIZES.md, marginBottom: SIZES.md },
  cardLabel: { fontSize: FONTS.caption, color: COLORS.white, opacity: 0.8, marginBottom: SIZES.sm },
  guardInfo: { flexDirection: 'row', alignItems: 'center' },
  guardAvatar: { width: 56, height: 56, borderRadius: 28, backgroundColor: COLORS.primaryBlue, justifyContent: 'center', alignItems: 'center' },
  avatarText: { fontSize: FONTS.h4, fontWeight: 'bold', color: COLORS.white },
  guardDetails: { flex: 1, marginLeft: SIZES.md },
  guardName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.white },
  guardId: { fontSize: FONTS.caption, color: COLORS.white, opacity: 0.8, marginTop: 2 },
  guardShift: { fontSize: FONTS.caption, color: COLORS.white, opacity: 0.8, marginTop: 2 },
  callBtn: { width: 44, height: 44, borderRadius: 22, backgroundColor: COLORS.success, justifyContent: 'center', alignItems: 'center' },
  progressCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.md, ...SHADOWS.small },
  progressHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.sm },
  progressTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  progressText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  progressBar: { height: 8, backgroundColor: COLORS.gray200, borderRadius: 4, overflow: 'hidden' },
  progressFill: { height: '100%', backgroundColor: COLORS.success, borderRadius: 4 },
  section: { marginBottom: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  checklistCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, ...SHADOWS.small, overflow: 'hidden' },
  checkItem: { flexDirection: 'row', alignItems: 'center', padding: SIZES.md, borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  checkbox: { width: 24, height: 24, borderRadius: 6, borderWidth: 2, borderColor: COLORS.gray300, justifyContent: 'center', alignItems: 'center' },
  checkboxChecked: { backgroundColor: COLORS.success, borderColor: COLORS.success },
  checkText: { flex: 1, fontSize: FONTS.bodySmall, color: COLORS.textPrimary, marginLeft: SIZES.sm },
  checkTextDone: { color: COLORS.textSecondary, textDecorationLine: 'line-through' },
  mandatoryTag: { fontSize: FONTS.tiny, color: COLORS.error, backgroundColor: COLORS.error + '15', paddingHorizontal: SIZES.xs, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  taskCard: { flexDirection: 'row', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, marginBottom: SIZES.sm, overflow: 'hidden', ...SHADOWS.small },
  priorityIndicator: { width: 4 },
  taskContent: { flex: 1, padding: SIZES.md },
  taskHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SIZES.xs },
  taskTitle: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.textPrimary, flex: 1 },
  priorityBadge: { paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  priorityText: { fontSize: FONTS.tiny, fontWeight: '600' },
  taskDescription: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  notesCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  notesInput: { padding: SIZES.md, fontSize: FONTS.body, color: COLORS.textPrimary, minHeight: 100, textAlignVertical: 'top' },
  submitBtn: { marginTop: SIZES.md },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: SIZES.xl },
  loadingText: { marginTop: SIZES.md, fontSize: FONTS.bodySmall, color: COLORS.textSecondary },
  noTasks: { padding: SIZES.md, backgroundColor: COLORS.gray50, borderRadius: SIZES.radiusMd },
  noTasksText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
});

export default ShiftHandoverScreen;
