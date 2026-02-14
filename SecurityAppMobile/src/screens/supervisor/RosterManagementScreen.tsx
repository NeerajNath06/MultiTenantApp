import React, { useState, useCallback, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Alert,
  Dimensions,
  ActivityIndicator,
  RefreshControl,
  Modal,
  FlatList,
  Share,
  Platform,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { authService } from '../../services/authService';
import { deploymentService, Deployment, Roster } from '../../services/deploymentService';
import { siteService } from '../../services/siteService';
import { guardService, GuardItem } from '../../services/guardService';
import { shiftService, Shift } from '../../services/shiftService';

const { width } = Dimensions.get('window');
const DAY_WIDTH = (width - 80) / 7;
const WEEK_DAYS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

function getWeekStart(d: Date): Date {
  const date = new Date(d);
  const day = date.getDay();
  const diff = date.getDate() - day + (day === 0 ? -6 : 1);
  date.setDate(diff);
  date.setHours(0, 0, 0, 0);
  return date;
}

function formatWeekLabel(weekStart: Date): string {
  const end = new Date(weekStart);
  end.setDate(end.getDate() + 6);
  const m1 = weekStart.toLocaleDateString('en-IN', { month: 'short' });
  const m2 = end.toLocaleDateString('en-IN', { month: 'short' });
  const d1 = weekStart.getDate();
  const d2 = end.getDate();
  const y = weekStart.getFullYear();
  if (m1 === m2) return `${m1} ${d1} - ${d2}, ${y}`;
  return `${m1} ${d1} - ${m2} ${d2}, ${y}`;
}

function toYMD(d: Date): string {
  return d.toISOString().split('T')[0];
}

function dateForDayIndex(weekStart: Date, dayIndex: number): string {
  const d = new Date(weekStart);
  d.setDate(d.getDate() + dayIndex);
  return toYMD(d);
}

/** Map shift name to display type for color/abbr */
function shiftNameToType(shiftName?: string): 'morning' | 'evening' | 'night' | 'off' | 'leave' | 'shift' {
  if (!shiftName) return 'off';
  const n = shiftName.toLowerCase();
  if (n.includes('morn')) return 'morning';
  if (n.includes('eve')) return 'evening';
  if (n.includes('night')) return 'night';
  if (n.includes('leave')) return 'leave';
  return 'shift';
}

interface SiteOption {
  id: string;
  siteName: string;
}

interface RosterRow {
  guardId: string;
  guardName: string;
  deploymentsByDay: (Deployment | null)[]; // index 0 = Mon .. 6 = Sun
}

function RosterManagementScreen({ navigation }: any) {
  const [weekStart, setWeekStart] = useState(() => getWeekStart(new Date()));
  const [selectedSiteId, setSelectedSiteId] = useState<string | null>(null);
  const [sites, setSites] = useState<SiteOption[]>([]);
  const [roster, setRoster] = useState<Roster | null>(null);
  const [guards, setGuards] = useState<GuardItem[]>([]);
  const [shifts, setShifts] = useState<Shift[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [editModal, setEditModal] = useState<{
    visible: boolean;
    guardId: string;
    guardName: string;
    dayIndex: number;
    dateStr: string;
    deployment: Deployment | null;
    selectedSiteIdForAdd: string | null;
    step: 'site' | 'shift';
  } | null>(null);
  const [addModalVisible, setAddModalVisible] = useState(false);
  const [saving, setSaving] = useState(false);
  const [sitePickerVisible, setSitePickerVisible] = useState(false);

  const dateFrom = toYMD(weekStart);
  const dateTo = dateForDayIndex(weekStart, 6);

  const loadSites = useCallback(async () => {
    const user = await authService.getStoredUser();
    const opts: { pageSize: number; supervisorId?: string } = { pageSize: 100 };
    if (user?.isSupervisor && user?.id) opts.supervisorId = user.id;
    const res = await siteService.getSites(opts);
    if (res.success && res.data) {
      const raw = res.data as { items?: any[]; Items?: any[] };
      const list = raw.items ?? raw.Items ?? (Array.isArray(res.data) ? res.data : []);
      const mapped: SiteOption[] = list.map((s: any) => ({
        id: String(s.id ?? s.Id ?? ''),
        siteName: s.siteName ?? s.SiteName ?? '—',
      }));
      setSites([{ id: '', siteName: 'All Sites' }, ...mapped.filter(s => s.id)]);
    }
  }, []);

  const loadRoster = useCallback(async () => {
    const user = await authService.getStoredUser();
    const supervisorId = user?.isSupervisor ? user.id : undefined;
    const res = await deploymentService.getRoster(dateFrom, dateTo, selectedSiteId || undefined, supervisorId);
    if (res.success && res.data) {
      setRoster(res.data);
      setError(null);
    } else {
      setRoster({ dateFrom, dateTo, deployments: [] });
      setError(res.error?.message ?? 'Failed to load roster');
    }
  }, [dateFrom, dateTo, selectedSiteId]);

  const loadGuardsAndShifts = useCallback(async () => {
    const user = await authService.getStoredUser();
    const supervisorId = user?.isSupervisor ? user.id : undefined;
    const [gRes, sRes] = await Promise.all([
      guardService.getGuards({ supervisorId, pageSize: 200, includeInactive: false }),
      shiftService.getShifts({ pageSize: 50 }),
    ]);
    if (gRes.success && gRes.data?.items) setGuards(gRes.data.items);
    if (sRes.success && sRes.data) {
      const raw = sRes.data as { items?: Shift[]; Items?: Shift[] };
      const list = raw.items ?? raw.Items ?? (Array.isArray(sRes.data) ? sRes.data : []);
      setShifts(list);
    }
  }, []);

  const loadAll = useCallback(async () => {
    setLoading(true);
    await Promise.all([loadSites(), loadGuardsAndShifts()]);
    await loadRoster();
    setLoading(false);
    setRefreshing(false);
  }, [loadSites, loadGuardsAndShifts, loadRoster]);

  useEffect(() => {
    loadAll();
  }, [weekStart, selectedSiteId]);

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadAll();
  }, [loadAll]);

  const prevWeek = () => {
    const prev = new Date(weekStart);
    prev.setDate(prev.getDate() - 7);
    setWeekStart(prev);
  };

  const nextWeek = () => {
    const next = new Date(weekStart);
    next.setDate(next.getDate() + 7);
    setWeekStart(next);
  };

  const deployments = roster?.deployments ?? [];
  const deploymentsList = Array.isArray(deployments) ? deployments : [];

  const guardIds = Array.from(new Set(deploymentsList.map((d: any) => String(d.guardId ?? d.GuardId ?? '')))).filter(Boolean);
  const guardMap = deploymentsList.reduce((acc: Record<string, { name: string }>, d: any) => {
    const id = String(d.guardId ?? d.GuardId ?? '');
    if (id && !acc[id]) acc[id] = { name: (d.guardName ?? d.GuardName) || '—' };
    return acc;
  }, {});
  const rosterRows: RosterRow[] = guardIds
    .map(guardId => ({
      guardId,
      guardName: guardMap[guardId]?.name ?? '—',
      deploymentsByDay: WEEK_DAYS.map((_, dayIndex) => {
        const dateStr = dateForDayIndex(weekStart, dayIndex);
        return deploymentsList.find(
          (dep: any) =>
            String(dep.guardId ?? dep.GuardId) === guardId &&
            (dep.deploymentDate ?? dep.DeploymentDate) === dateStr
        ) ?? null;
      }),
    }))
    .sort((a, b) => a.guardName.localeCompare(b.guardName));

  const totalGuards = rosterRows.length;
  const scheduled = deploymentsList.length;
  const stats = { totalGuards, scheduled, onLeave: 0, available: Math.max(0, totalGuards * 7 - scheduled) };

  const getShiftColor = (shiftType: string) => {
    switch (shiftType) {
      case 'morning': return COLORS.success;
      case 'evening': return COLORS.warning;
      case 'night': return COLORS.primaryBlue;
      case 'leave': return COLORS.error;
      case 'shift': return COLORS.secondary;
      default: return COLORS.gray300;
    }
  };

  const getShiftAbbr = (shiftType: string) => {
    switch (shiftType) {
      case 'morning': return 'M';
      case 'evening': return 'E';
      case 'night': return 'N';
      case 'leave': return 'L';
      case 'shift': return 'S';
      default: return '—';
    }
  };

  const openEditModal = (row: RosterRow, dayIndex: number, deployment: Deployment | null) => {
    const dateStr = dateForDayIndex(weekStart, dayIndex);
    setEditModal({
      visible: true,
      guardId: row.guardId,
      guardName: row.guardName,
      dayIndex,
      dateStr,
      deployment,
      selectedSiteIdForAdd: deployment ? String((deployment as any).siteId ?? (deployment as any).SiteId) : null,
      step: deployment ? 'shift' : 'site',
    });
  };

  const handleEditSave = async (shiftId: string, siteId: string) => {
    if (!editModal) return;
    setSaving(true);
    const user = await authService.getStoredUser();
    const sid = siteId || editModal.selectedSiteIdForAdd || (editModal.deployment && String((editModal.deployment as any).siteId ?? (editModal.deployment as any).SiteId));
    if (!sid) {
      Alert.alert('Error', 'Please select a site.');
      setSaving(false);
      return;
    }
    if (editModal.deployment) {
      const res = await deploymentService.updateAssignment(editModal.deployment.id, {
        guardId: editModal.guardId,
        siteId: sid,
        shiftId,
        supervisorId: user?.isSupervisor ? user.id : undefined,
        assignmentStartDate: editModal.dateStr,
        assignmentEndDate: editModal.dateStr,
      });
      if (res.success) {
        setEditModal(null);
        await loadRoster();
      } else {
        Alert.alert('Error', res.error?.message ?? 'Update failed');
      }
    } else {
      const res = await deploymentService.createAssignment({
        guardId: editModal.guardId,
        siteId: sid,
        shiftId,
        supervisorId: user?.isSupervisor ? user.id : undefined,
        assignmentStartDate: editModal.dateStr,
        assignmentEndDate: editModal.dateStr,
      });
      if (res.success) {
        setEditModal(null);
        await loadRoster();
      } else {
        Alert.alert('Error', res.error?.message ?? 'Create failed');
      }
    }
    setSaving(false);
  };

  const handleAddShift = async (guardId: string, siteId: string, shiftId: string, fromDate: string, toDate: string) => {
    setSaving(true);
    const user = await authService.getStoredUser();
    const res = await deploymentService.createAssignment({
      guardId,
      siteId,
      shiftId,
      supervisorId: user?.isSupervisor ? user.id : undefined,
      assignmentStartDate: fromDate,
      assignmentEndDate: toDate,
    });
    setSaving(false);
    if (res.success) {
      setAddModalVisible(false);
      await loadRoster();
    } else {
      Alert.alert('Error', res.error?.message ?? 'Create failed');
    }
  };

  const handleCopyWeek = async () => {
    const nextStart = new Date(weekStart);
    nextStart.setDate(nextStart.getDate() + 7);
    const nextFrom = toYMD(nextStart);
    const nextTo = dateForDayIndex(nextStart, 6);
    const user = await authService.getStoredUser();
    let created = 0;
    for (const dep of deploymentsList) {
      const d = dep as { guardId?: string; siteId?: string; shiftId?: string; deploymentDate?: string; GuardId?: string; SiteId?: string; ShiftId?: string; DeploymentDate?: string };
      const guardId = String(d.guardId ?? d.GuardId ?? '');
      const siteId = String(d.siteId ?? d.SiteId ?? '');
      const shiftId = String(d.shiftId ?? d.ShiftId ?? '');
      const oldDate = d.deploymentDate ?? d.DeploymentDate;
      if (!oldDate) continue;
      const dayOffset = Math.floor((new Date(oldDate).getTime() - weekStart.getTime()) / (24 * 60 * 60 * 1000));
      if (dayOffset < 0 || dayOffset > 6) continue;
      const newDate = dateForDayIndex(nextStart, dayOffset);
      const res = await deploymentService.createAssignment({
        guardId,
        siteId,
        shiftId,
        supervisorId: user?.isSupervisor ? user.id : undefined,
        assignmentStartDate: newDate,
        assignmentEndDate: newDate,
      });
      if (res.success) created++;
    }
    Alert.alert('Copy Week', `Created ${created} assignments for next week.`);
    await loadRoster();
  };

  const handleExport = async () => {
    const lines = ['Roster ' + formatWeekLabel(weekStart), ''];
    for (const row of rosterRows) {
      const dayParts = row.deploymentsByDay.map((d, i) => {
        const dateStr = dateForDayIndex(weekStart, i);
        if (!d) return `${WEEK_DAYS[i]} ${dateStr}: —`;
        const name = (d as any).shiftName ?? (d as any).ShiftName ?? 'Shift';
        return `${WEEK_DAYS[i]} ${dateStr}: ${name}`;
      });
      lines.push(`${row.guardName}: ${dayParts.join(' | ')}`);
    }
    const text = lines.join('\n');
    if (Platform.OS === 'web') {
      Alert.alert('Roster', text);
    } else {
      try {
        await Share.share({ message: text, title: 'Roster Export' });
      } catch {
        Alert.alert('Roster', text);
      }
    }
  };

  const selectedSiteName = selectedSiteId
    ? (sites.find(s => s.id === selectedSiteId)?.siteName ?? 'All Sites')
    : 'All Sites';

  if (loading && !roster) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading roster...</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Roster Management</Text>
        <TouchableOpacity style={styles.addBtn} onPress={() => setAddModalVisible(true)}>
          <MaterialCommunityIcons name="plus" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      <TouchableOpacity style={styles.siteFilter} onPress={() => setSitePickerVisible(true)}>
        <MaterialCommunityIcons name="map-marker" size={20} color={COLORS.primary} />
        <Text style={styles.siteFilterText}>{selectedSiteName}</Text>
        <MaterialCommunityIcons name="chevron-down" size={20} color={COLORS.textSecondary} />
      </TouchableOpacity>

      <View style={styles.weekSelector}>
        <TouchableOpacity style={styles.weekArrow} onPress={prevWeek}>
          <MaterialCommunityIcons name="chevron-left" size={24} color={COLORS.primary} />
        </TouchableOpacity>
        <View style={styles.weekInfo}>
          <Text style={styles.weekText}>{formatWeekLabel(weekStart)}</Text>
          <Text style={styles.weekSubtext}>Week</Text>
        </View>
        <TouchableOpacity style={styles.weekArrow} onPress={nextWeek}>
          <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.primary} />
        </TouchableOpacity>
      </View>

      {error ? (
        <View style={styles.errorBanner}>
          <Text style={styles.errorText}>{error}</Text>
        </View>
      ) : null}

      <View style={styles.statsRow}>
        <View style={[styles.statCard, { backgroundColor: COLORS.primaryBlue + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.primaryBlue }]}>{stats.totalGuards}</Text>
          <Text style={styles.statLabel}>Guards</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.success }]}>{stats.scheduled}</Text>
          <Text style={styles.statLabel}>Scheduled</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.warning + '15' }]}>
          <Text style={[styles.statValue, { color: COLORS.warning }]}>{stats.available}</Text>
          <Text style={styles.statLabel}>Slots</Text>
        </View>
      </View>

      <View style={styles.legend}>
        <View style={styles.legendItem}>
          <View style={[styles.legendDot, { backgroundColor: COLORS.success }]} />
          <Text style={styles.legendText}>Morning</Text>
        </View>
        <View style={styles.legendItem}>
          <View style={[styles.legendDot, { backgroundColor: COLORS.warning }]} />
          <Text style={styles.legendText}>Evening</Text>
        </View>
        <View style={styles.legendItem}>
          <View style={[styles.legendDot, { backgroundColor: COLORS.primaryBlue }]} />
          <Text style={styles.legendText}>Night</Text>
        </View>
        <View style={styles.legendItem}>
          <View style={[styles.legendDot, { backgroundColor: COLORS.secondary }]} />
          <Text style={styles.legendText}>Shift</Text>
        </View>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        <View style={styles.gridContainer}>
          <View style={styles.gridRow}>
            <View style={styles.guardCell}>
              <Text style={styles.headerCellText}>Guard</Text>
            </View>
            {WEEK_DAYS.map((day, index) => (
              <View key={day} style={styles.dayCell}>
                <Text style={styles.dayText}>{day}</Text>
                <Text style={styles.dateText}>{new Date(weekStart.getTime() + index * 24 * 60 * 60 * 1000).getDate()}</Text>
              </View>
            ))}
          </View>
          {rosterRows.length === 0 ? (
            <View style={styles.emptyRow}>
              <Text style={styles.emptyText}>No roster data for this week. Pull to refresh or add shifts.</Text>
            </View>
          ) : (
            rosterRows.map(row => (
              <View key={row.guardId} style={styles.gridRow}>
                <View style={styles.guardCell}>
                  <View style={styles.guardAvatar}>
                    <Text style={styles.avatarText}>
                      {row.guardName.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase()}
                    </Text>
                  </View>
                  <Text style={styles.guardName} numberOfLines={1}>{row.guardName}</Text>
                </View>
                {row.deploymentsByDay.map((dep, dayIndex) => {
                  const shiftType = dep
                    ? shiftNameToType((dep as any).shiftName ?? (dep as any).ShiftName)
                    : 'off';
                  return (
                    <TouchableOpacity
                      key={dayIndex}
                      style={styles.shiftCell}
                      onPress={() => openEditModal(row, dayIndex, dep)}
                    >
                      <View style={[styles.shiftBadge, { backgroundColor: getShiftColor(shiftType) }]}>
                        <Text style={styles.shiftText}>{getShiftAbbr(shiftType)}</Text>
                      </View>
                    </TouchableOpacity>
                  );
                })}
              </View>
            ))
          )}
        </View>

        <View style={styles.actionsSection}>
          <Text style={styles.sectionTitle}>Quick Actions</Text>
          <View style={styles.actionsRow}>
            <TouchableOpacity style={styles.actionCard} onPress={() => setAddModalVisible(true)}>
              <MaterialCommunityIcons name="account-plus" size={28} color={COLORS.success} />
              <Text style={styles.actionLabel}>Add Shift</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionCard} onPress={handleCopyWeek}>
              <MaterialCommunityIcons name="content-copy" size={28} color={COLORS.primaryBlue} />
              <Text style={styles.actionLabel}>Copy Week</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionCard} onPress={handleExport}>
              <MaterialCommunityIcons name="file-export" size={28} color={COLORS.secondary} />
              <Text style={styles.actionLabel}>Export</Text>
            </TouchableOpacity>
          </View>
        </View>
        <View style={{ height: 50 }} />
      </ScrollView>

      {/* Site picker modal */}
      <Modal visible={sitePickerVisible} transparent animationType="fade">
        <TouchableOpacity style={styles.modalOverlay} activeOpacity={1} onPress={() => setSitePickerVisible(false)}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Filter by Site</Text>
            <FlatList
              data={sites}
              keyExtractor={item => item.id}
              renderItem={({ item }) => (
                <TouchableOpacity
                  style={styles.modalOption}
                  onPress={() => {
                    setSelectedSiteId(item.id || null);
                    setSitePickerVisible(false);
                  }}
                >
                  <Text style={styles.modalOptionText}>{item.siteName}</Text>
                </TouchableOpacity>
              )}
            />
          </View>
        </TouchableOpacity>
      </Modal>

      {/* Edit/Add single cell modal */}
      {editModal?.visible && (
        <Modal visible transparent animationType="slide">
          <View style={styles.modalOverlay}>
            <View style={[styles.modalContent, { maxHeight: '70%' }]}>
              <Text style={styles.modalTitle}>
                {editModal.deployment ? 'Edit' : 'Add'} shift – {editModal.guardName}, {WEEK_DAYS[editModal.dayIndex]} {editModal.dateStr}
              </Text>
              {editModal.step === 'site' && (
                <>
                  <Text style={styles.addModalLabel}>Select site</Text>
                  <FlatList
                    data={sites.filter(s => s.id)}
                    keyExtractor={s => s.id}
                    style={{ maxHeight: 220 }}
                    renderItem={({ item }) => (
                      <TouchableOpacity
                        style={styles.modalOption}
                        onPress={() => setEditModal(prev => prev ? { ...prev, selectedSiteIdForAdd: item.id, step: 'shift' } : null)}
                      >
                        <Text style={styles.modalOptionText}>{item.siteName}</Text>
                      </TouchableOpacity>
                    )}
                  />
                </>
              )}
              {editModal.step === 'shift' && (
                <>
                  {editModal.selectedSiteIdForAdd ? (
                    <Text style={styles.addModalLabel}>Selected site: {sites.find(s => s.id === editModal.selectedSiteIdForAdd)?.siteName}</Text>
                  ) : null}
                  {shifts.length === 0 ? (
                    <Text style={styles.emptyText}>No shifts available.</Text>
                  ) : (
                    <FlatList
                      data={shifts}
                      keyExtractor={s => s.id}
                      style={{ maxHeight: 220 }}
                      renderItem={({ item }) => {
                        const siteId = editModal.deployment
                          ? String((editModal.deployment as any).siteId ?? (editModal.deployment as any).SiteId)
                          : (editModal.selectedSiteIdForAdd || '');
                        return (
                          <TouchableOpacity
                            style={styles.modalOption}
                            onPress={() => handleEditSave(item.id, siteId)}
                            disabled={saving || (!editModal.deployment && !editModal.selectedSiteIdForAdd)}
                          >
                            <Text style={styles.modalOptionText}>{item.shiftName} ({item.startTime} - {item.endTime})</Text>
                          </TouchableOpacity>
                        );
                      }}
                    />
                  )}
                </>
              )}
              <TouchableOpacity
                style={[styles.modalOption, { borderTopWidth: 1, borderTopColor: COLORS.gray200 }]}
                onPress={() => setEditModal(null)}
              >
                <Text style={[styles.modalOptionText, { color: COLORS.error }]}>Cancel</Text>
              </TouchableOpacity>
            </View>
          </View>
        </Modal>
      )}

      {/* Add shift modal - simplified: guard, site, shift, single date */}
      {addModalVisible && (
        <AddShiftModal
          guards={guards}
          sites={sites.filter(s => s.id)}
          shifts={shifts}
          defaultDate={dateFrom}
          saving={saving}
          onSave={handleAddShift}
          onClose={() => setAddModalVisible(false)}
        />
      )}
    </SafeAreaView>
  );
}

interface AddShiftModalProps {
  guards: GuardItem[];
  sites: SiteOption[];
  shifts: Shift[];
  defaultDate: string;
  saving: boolean;
  onSave: (guardId: string, siteId: string, shiftId: string, from: string, to: string) => Promise<void>;
  onClose: () => void;
}

function AddShiftModal({ guards, sites, shifts, defaultDate, saving, onSave, onClose }: AddShiftModalProps) {
  const [guardId, setGuardId] = useState('');
  const [siteId, setSiteId] = useState('');
  const [shiftId, setShiftId] = useState('');
  const [fromDate, setFromDate] = useState(defaultDate);
  const [toDate, setToDate] = useState(defaultDate);
  const [step, setStep] = useState<'guard' | 'site' | 'shift' | 'date'>('guard');
  useEffect(() => {
    setFromDate(defaultDate);
    setToDate(defaultDate);
  }, [defaultDate]);

  const guardName = guards.find(g => g.id === guardId)
    ? `${guards.find(g => g.id === guardId)?.firstName} ${guards.find(g => g.id === guardId)?.lastName}`.trim()
    : 'Select guard';
  const siteName = sites.find(s => s.id === siteId)?.siteName ?? 'Select site';
  const shiftName = shifts.find(s => s.id === shiftId)?.shiftName ?? 'Select shift';

  const canSave = guardId && siteId && shiftId && fromDate && toDate;

  const handleSave = () => {
    if (!canSave) return;
    onSave(guardId, siteId, shiftId, fromDate, toDate);
  };

  return (
    <Modal visible transparent animationType="slide">
      <View style={styles.modalOverlay}>
        <View style={[styles.modalContent, { maxHeight: '85%' }]}>
          <Text style={styles.modalTitle}>Add Shift</Text>
          <TouchableOpacity style={styles.addModalRow} onPress={() => setStep('guard')}>
            <Text style={styles.addModalLabel}>Guard</Text>
            <Text style={styles.addModalValue}>{guardName}</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.addModalRow} onPress={() => setStep('site')}>
            <Text style={styles.addModalLabel}>Site</Text>
            <Text style={styles.addModalValue}>{siteName}</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.addModalRow} onPress={() => setStep('shift')}>
            <Text style={styles.addModalLabel}>Shift</Text>
            <Text style={styles.addModalValue}>{shiftName}</Text>
          </TouchableOpacity>
          <View style={styles.addModalRow}>
            <Text style={styles.addModalLabel}>From</Text>
            <Text style={styles.addModalValue}>{fromDate}</Text>
          </View>
          <View style={styles.addModalRow}>
            <Text style={styles.addModalLabel}>To</Text>
            <Text style={styles.addModalValue}>{toDate}</Text>
          </View>

          {step === 'guard' && (
            <FlatList
              data={guards}
              keyExtractor={g => g.id}
              style={{ maxHeight: 200 }}
              renderItem={({ item }) => (
                <TouchableOpacity
                  style={styles.modalOption}
                  onPress={() => {
                    setGuardId(item.id);
                    setStep('site');
                  }}
                >
                  <Text style={styles.modalOptionText}>{item.firstName} {item.lastName}</Text>
                </TouchableOpacity>
              )}
            />
          )}
          {step === 'site' && (
            <FlatList
              data={sites}
              keyExtractor={s => s.id}
              style={{ maxHeight: 200 }}
              renderItem={({ item }) => (
                <TouchableOpacity
                  style={styles.modalOption}
                  onPress={() => {
                    setSiteId(item.id);
                    setStep('shift');
                  }}
                >
                  <Text style={styles.modalOptionText}>{item.siteName}</Text>
                </TouchableOpacity>
              )}
            />
          )}
          {step === 'shift' && (
            <FlatList
              data={shifts}
              keyExtractor={s => s.id}
              style={{ maxHeight: 200 }}
              renderItem={({ item }) => (
                <TouchableOpacity
                  style={styles.modalOption}
                  onPress={() => {
                    setShiftId(item.id);
                    setStep('date');
                  }}
                >
                  <Text style={styles.modalOptionText}>{item.shiftName} ({item.startTime}-{item.endTime})</Text>
                </TouchableOpacity>
              )}
            />
          )}
          {step === 'date' && (
            <>
              <Text style={styles.addModalHint}>Using single day: from = to = {fromDate}. Change in future if needed.</Text>
              <TouchableOpacity style={[styles.addModalButton, !canSave && styles.addModalButtonDisabled]} onPress={handleSave} disabled={!canSave || saving}>
                <Text style={styles.addModalButtonText}>{saving ? 'Saving...' : 'Save'}</Text>
              </TouchableOpacity>
            </>
          )}
          <TouchableOpacity style={[styles.modalOption, { marginTop: 8 }]} onPress={onClose}>
            <Text style={[styles.modalOptionText, { color: COLORS.error }]}>Cancel</Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', gap: SIZES.md },
  loadingText: { fontSize: FONTS.body, color: COLORS.textSecondary },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  addBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  siteFilter: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.white, marginTop: 1, gap: SIZES.xs },
  siteFilterText: { flex: 1, fontSize: FONTS.body, color: COLORS.textPrimary },
  weekSelector: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, marginTop: 1 },
  weekArrow: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.gray100, justifyContent: 'center', alignItems: 'center' },
  weekInfo: { alignItems: 'center' },
  weekText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  weekSubtext: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  errorBanner: { backgroundColor: COLORS.error + '20', padding: SIZES.sm, marginHorizontal: SIZES.md },
  errorText: { fontSize: FONTS.caption, color: COLORS.error },
  statsRow: { flexDirection: 'row', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, gap: SIZES.sm },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.sm, borderRadius: SIZES.radiusMd },
  statValue: { fontSize: FONTS.h4, fontWeight: 'bold' },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  legend: { flexDirection: 'row', justifyContent: 'center', gap: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.white, marginBottom: SIZES.sm },
  legendItem: { flexDirection: 'row', alignItems: 'center' },
  legendDot: { width: 12, height: 12, borderRadius: 6, marginRight: 4 },
  legendText: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  gridContainer: { marginHorizontal: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, ...SHADOWS.small, overflow: 'hidden' },
  gridRow: { flexDirection: 'row', borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  guardCell: { width: 80, flexDirection: 'row', alignItems: 'center', padding: SIZES.sm, borderRightWidth: 1, borderRightColor: COLORS.gray100 },
  guardAvatar: { width: 28, height: 28, borderRadius: 14, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center' },
  avatarText: { fontSize: FONTS.tiny, fontWeight: 'bold', color: COLORS.white },
  guardName: { flex: 1, fontSize: FONTS.tiny, color: COLORS.textPrimary, marginLeft: 4 },
  dayCell: { width: DAY_WIDTH, alignItems: 'center', padding: SIZES.sm, backgroundColor: COLORS.gray50 },
  dayText: { fontSize: FONTS.tiny, fontWeight: '600', color: COLORS.textPrimary },
  dateText: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  headerCellText: { fontSize: FONTS.tiny, fontWeight: '600', color: COLORS.textPrimary },
  shiftCell: { width: DAY_WIDTH, alignItems: 'center', justifyContent: 'center', padding: SIZES.xs },
  shiftBadge: { width: 28, height: 28, borderRadius: 6, justifyContent: 'center', alignItems: 'center' },
  shiftText: { fontSize: FONTS.caption, fontWeight: 'bold', color: COLORS.white },
  emptyRow: { padding: SIZES.lg, alignItems: 'center' },
  emptyText: { fontSize: FONTS.caption, color: COLORS.textSecondary, textAlign: 'center' },
  actionsSection: { padding: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  actionsRow: { flexDirection: 'row', gap: SIZES.sm },
  actionCard: { flex: 1, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, alignItems: 'center', ...SHADOWS.small },
  actionLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: SIZES.xs, textAlign: 'center' },
  modalOverlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'center', padding: SIZES.md },
  modalContent: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, maxHeight: '80%' },
  modalTitle: { fontSize: FONTS.h4, fontWeight: '600', marginBottom: SIZES.md },
  modalOption: { padding: SIZES.md, borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  modalOptionText: { fontSize: FONTS.body, color: COLORS.textPrimary },
  addModalRow: { flexDirection: 'row', justifyContent: 'space-between', padding: SIZES.sm, borderBottomWidth: 1, borderBottomColor: COLORS.gray100 },
  addModalLabel: { fontSize: FONTS.body, color: COLORS.textSecondary },
  addModalValue: { fontSize: FONTS.body, color: COLORS.textPrimary },
  addModalHint: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.sm },
  addModalButton: { marginTop: SIZES.md, padding: SIZES.md, backgroundColor: COLORS.primary, borderRadius: SIZES.radiusMd, alignItems: 'center' },
  addModalButtonDisabled: { opacity: 0.5 },
  addModalButtonText: { color: COLORS.white, fontWeight: '600' },
});

export default RosterManagementScreen;
