import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, RefreshControl, Share, ActivityIndicator } from 'react-native';
import * as Sharing from 'expo-sharing';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { incidentService } from '../../services/incidentService';

interface Incident {
  id: string;
  title: string;
  type: string;
  reportedBy: string;
  location: string;
  time: string;
  status: 'pending' | 'investigating' | 'resolved' | 'closed';
  priority: 'critical' | 'high' | 'medium' | 'low';
  description: string;
}

function mapApiStatusToLocal(s: string): Incident['status'] {
  const v = (s ?? '').toString().toLowerCase();
  if (v === 'open' || v === 'pending') return 'pending';
  if (v === 'inprogress' || v === 'investigating') return 'investigating';
  if (v === 'resolved') return 'resolved';
  if (v === 'closed') return 'closed';
  return 'pending';
}

function mapLocalStatusToApi(s: Incident['status']): number {
  if (s === 'pending') return 1;
  if (s === 'investigating') return 2;
  if (s === 'resolved') return 3;
  if (s === 'closed') return 4;
  return 1;
}

function formatIncidentTime(iso?: string | null): string {
  if (!iso) return '—';
  try {
    const d = new Date(iso);
    const now = new Date();
    const sameDay = d.toDateString() === now.toDateString();
    if (sameDay) return d.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true });
    const yesterday = new Date(now);
    yesterday.setDate(yesterday.getDate() - 1);
    if (d.toDateString() === yesterday.toDateString()) return 'Yesterday';
    return d.toLocaleDateString('en-IN', { day: 'numeric', month: 'short' });
  } catch {
    return '—';
  }
}

function IncidentReviewScreen({ navigation }: any) {
  const [refreshing, setRefreshing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [selectedFilter, setSelectedFilter] = useState<'all' | 'pending' | 'investigating' | 'resolved'>('all');
  const [incidents, setIncidents] = useState<Incident[]>([]);

  const loadIncidents = useCallback(async () => {
    const res = await incidentService.getIncidents({ pageNumber: 1, pageSize: 100 });
    if (res.success && res.data) {
      const raw = res.data as { items?: any[]; Items?: any[] };
      const list = raw?.items ?? raw?.Items ?? (Array.isArray(res.data) ? res.data : []);
      const mapped: Incident[] = (list as any[]).map((i: any) => {
        const desc = (i.description ?? i.Description ?? '').toString();
        const type = (i.incidentType ?? i.IncidentType ?? 'Incident').toString();
        const status = mapApiStatusToLocal(i.status ?? i.Status);
        return {
          id: String(i.id ?? i.Id),
          title: desc.slice(0, 60) || type,
          type,
          reportedBy: (i.guardName ?? i.GuardName ?? '—').toString(),
          location: (i.siteName ?? i.SiteName ?? '—').toString(),
          time: formatIncidentTime(i.reportedAt ?? i.ReportedAt ?? i.createdDate ?? i.CreatedDate),
          status,
          priority: 'medium' as const,
          description: desc || 'No description',
        };
      });
      setIncidents(mapped);
    } else {
      setIncidents([]);
    }
    setLoading(false);
    setRefreshing(false);
  }, []);

  useEffect(() => {
    loadIncidents();
  }, [loadIncidents]);

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadIncidents();
  }, [loadIncidents]);

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'pending': return COLORS.warning;
      case 'investigating': return COLORS.info;
      case 'resolved': return COLORS.success;
      case 'closed': return COLORS.gray500;
      default: return COLORS.gray500;
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'critical': return COLORS.error;
      case 'high': return COLORS.warning;
      case 'medium': return COLORS.info;
      case 'low': return COLORS.success;
      default: return COLORS.gray500;
    }
  };

  const handleIncidentPress = (incident: Incident) => {
    const actions = [{ text: 'Close', style: 'cancel' as const }];

    if (incident.status !== 'closed') {
      actions.push({
        text: 'Update Status',
        style: 'default' as const,
        onPress: () => handleUpdateStatus(incident),
      } as any);
    }

    if (incident.status === 'pending') {
      actions.push({
        text: 'Start Investigation',
        style: 'default' as const,
        onPress: () => updateIncidentStatus(incident.id, 'investigating'),
      } as any);
    }

    actions.push({
      text: 'View Full Details',
      style: 'default' as const,
      onPress: () => handleViewDetails(incident),
    } as any);

    Alert.alert(
      incident.title,
      `📍 ${incident.location}\n⏰ ${incident.time}\n👤 Reported by: ${incident.reportedBy}\n\n${incident.description}`,
      actions
    );
  };

  const handleViewDetails = (incident: Incident) => {
    Alert.alert(
      'Incident Details',
      `Title: ${incident.title}\n\nType: ${incident.type}\nPriority: ${incident.priority.toUpperCase()}\nStatus: ${incident.status.toUpperCase()}\n\nLocation: ${incident.location}\nReported by: ${incident.reportedBy}\nTime: ${incident.time}\n\nDescription:\n${incident.description}`,
      [
        { text: 'Close' },
        { text: 'Share Report', onPress: () => shareIncidentReport(incident) },
      ]
    );
  };

  const shareIncidentReport = async (incident: Incident) => {
    try {
      await Share.share({
        message: `INCIDENT REPORT\n\nTitle: ${incident.title}\nType: ${incident.type}\nPriority: ${incident.priority.toUpperCase()}\nStatus: ${incident.status.toUpperCase()}\n\nLocation: ${incident.location}\nReported by: ${incident.reportedBy}\nTime: ${incident.time}\n\nDescription:\n${incident.description}`,
        title: 'Incident Report',
      });
    } catch (error) {
      Alert.alert('Error', 'Unable to share report');
    }
  };

  const handleUpdateStatus = (incident: Incident) => {
    const statusOptions = [
      { text: 'Cancel', style: 'cancel' as const },
      { text: 'Mark Investigating', onPress: () => updateIncidentStatus(incident.id, 'investigating') },
      { text: 'Mark Resolved', onPress: () => updateIncidentStatus(incident.id, 'resolved') },
      { text: 'Close Incident', onPress: () => updateIncidentStatus(incident.id, 'closed') },
    ];
    Alert.alert('Update Status', 'Select new status for this incident:', statusOptions);
  };

  const updateIncidentStatus = async (id: string, status: Incident['status']) => {
    const apiStatusNum = mapLocalStatusToApi(status);
    const res = await incidentService.updateIncident(id, { status: apiStatusNum } as any);
    if (res.success) {
      setIncidents(incidents.map(i => i.id === id ? { ...i, status } : i));
      Alert.alert('Status Updated', `Incident status changed to ${status}.`);
    } else {
      Alert.alert('Error', res.error?.message ?? 'Failed to update status.');
    }
  };

  const handleAddIncident = () => {
    Alert.alert(
      'Create Incident Report',
      'Select incident type:',
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Security Breach', onPress: () => createIncident('Security Breach') },
        { text: 'Fire/Safety', onPress: () => createIncident('Fire/Safety') },
        { text: 'Suspicious Activity', onPress: () => createIncident('Suspicious Activity') },
        { text: 'Equipment Issue', onPress: () => createIncident('Equipment') },
        { text: 'Other', onPress: () => createIncident('Other') },
      ]
    );
  };

  const createIncident = (type: string) => {
    Alert.alert(
      'Create Incident',
      'To create an incident, use the guard app or web portal with site and description. You can update status from this list after creation.',
      [{ text: 'OK' }]
    );
  };

  const [exporting, setExporting] = useState(false);

  const handleExport = useCallback(async (format: 'csv' | 'xlsx' | 'pdf') => {
    setExporting(true);
    try {
      const statusMap: Record<string, string> = {
        all: '',
        pending: 'Open',
        investigating: 'InProgress',
        resolved: 'Resolved',
      };
      const res = await incidentService.exportIncidents(format, {
        status: statusMap[selectedFilter] || undefined,
        sortBy: 'date',
        sortDirection: 'desc',
      });
      if (!res.success) {
        Alert.alert('Export Failed', res.error ?? 'Could not export incidents.');
        return;
      }
      if (res.localUri && res.fileName) {
        const canShare = await Sharing.isAvailableAsync();
        if (canShare) {
          await Sharing.shareAsync(res.localUri, {
            mimeType: res.mimeType ?? 'text/csv',
            dialogTitle: `Incident Report (${format.toUpperCase()}) - Open or share`,
          });
        } else {
          Alert.alert('Report Downloaded', `File saved: ${res.fileName}. Open from your device to view.`);
        }
      }
    } catch (e) {
      Alert.alert('Export Failed', (e as Error).message);
    } finally {
      setExporting(false);
    }
  }, [selectedFilter]);

  const handleGenerateReport = () => {
    const pendingCount = incidents.filter(i => i.status === 'pending').length;
    const investigatingCount = incidents.filter(i => i.status === 'investigating').length;
    const resolvedCount = incidents.filter(i => i.status === 'resolved').length;
    
    Alert.alert(
      'Incident Report Summary',
      `Total Incidents: ${incidents.length}\n\n📋 Pending: ${pendingCount}\n🔍 Investigating: ${investigatingCount}\n✅ Resolved: ${resolvedCount}\n\nCritical: ${incidents.filter(i => i.priority === 'critical').length}\nHigh Priority: ${incidents.filter(i => i.priority === 'high').length}\n\nReport generated successfully.`,
      [
        { text: 'Close' },
        { text: 'Download as CSV', onPress: () => handleExport('csv') },
        { text: 'Download as Excel', onPress: () => handleExport('xlsx') },
        { text: 'Download as PDF', onPress: () => handleExport('pdf') },
      ]
    );
  };

  const handleAnalytics = () => {
    navigation.navigate('AttendanceApproval'); // Navigate to analytics (can be replaced with dedicated analytics screen)
    Alert.alert('Analytics', 'Viewing incident analytics dashboard...');
  };

  const handleEmergencyProtocol = () => {
    Alert.alert(
      'Emergency Protocol',
      'Initiating emergency protocol will:\n\n• Alert all on-duty guards\n• Notify management\n• Activate emergency procedures\n\nAre you sure you want to proceed?',
      [
        { text: 'Cancel', style: 'cancel' },
        { 
          text: 'Initiate', 
          style: 'destructive',
          onPress: () => Alert.alert('Emergency Initiated', 'All security personnel have been alerted. Management notified.')
        },
      ]
    );
  };

  const handleSendAlert = () => {
    Alert.alert(
      'Mass Notification',
      'Send notification to:',
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'All Guards', onPress: () => Alert.alert('Sent', 'Notification sent to all guards') },
        { text: 'Site A Guards', onPress: () => Alert.alert('Sent', 'Notification sent to Site A guards') },
        { text: 'Supervisors Only', onPress: () => Alert.alert('Sent', 'Notification sent to supervisors') },
      ]
    );
  };

  const filteredIncidents = incidents.filter(i => {
    if (selectedFilter === 'all') return true;
    return i.status === selectedFilter;
  });

  const pendingCount = incidents.filter(i => i.status === 'pending').length;
  const investigatingCount = incidents.filter(i => i.status === 'investigating').length;
  const criticalCount = incidents.filter(i => i.priority === 'critical' && i.status !== 'closed').length;

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Incident Review</Text>
          <View style={styles.addButton} />
        </View>
        <View style={styles.loadingWrap}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading incidents...</Text>
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
        <Text style={styles.headerTitle}>Incident Review</Text>
        <TouchableOpacity style={styles.addButton} onPress={handleAddIncident}>
          <MaterialCommunityIcons name="plus" size={24} color={COLORS.primary} />
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        contentContainerStyle={styles.content}
      >
        {/* Stats */}
        <View style={styles.statsContainer}>
          <Card style={[styles.statCard, { backgroundColor: COLORS.warning + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.warning }]}>{pendingCount}</Text>
            <Text style={styles.statLabel}>Pending</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.info + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.info }]}>{investigatingCount}</Text>
            <Text style={styles.statLabel}>Investigating</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.error + '10' }]}>
            <Text style={[styles.statNumber, { color: COLORS.error }]}>{criticalCount}</Text>
            <Text style={styles.statLabel}>Critical</Text>
          </Card>
        </View>

        {/* Quick Actions */}
        <View style={styles.actionsContainer}>
          <TouchableOpacity style={styles.actionButton} onPress={handleGenerateReport}>
            <MaterialCommunityIcons name="file-document-outline" size={20} color={COLORS.primary} />
            <Text style={styles.actionButtonText}>Report</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={styles.actionButton}
            onPress={() => Alert.alert('Download Report', 'Choose report format (agency header and data included)', [
              { text: 'Cancel', style: 'cancel' },
              { text: 'Download as CSV', onPress: () => handleExport('csv') },
              { text: 'Download as Excel', onPress: () => handleExport('xlsx') },
              { text: 'Download as PDF', onPress: () => handleExport('pdf') },
            ])}
            disabled={exporting}
          >
            {exporting ? (
              <ActivityIndicator size="small" color={COLORS.primary} />
            ) : (
              <MaterialCommunityIcons name="file-download" size={20} color={COLORS.primary} />
            )}
            <Text style={styles.actionButtonText}>{exporting ? 'Downloading…' : 'Download Report'}</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.actionButton} onPress={handleAnalytics}>
            <MaterialCommunityIcons name="chart-line" size={20} color={COLORS.secondary} />
            <Text style={styles.actionButtonText}>Analytics</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.actionButton} onPress={handleAddIncident}>
            <MaterialCommunityIcons name="plus-circle" size={20} color={COLORS.success} />
            <Text style={styles.actionButtonText}>New</Text>
          </TouchableOpacity>
        </View>

        {/* Filter Tabs */}
        <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.filterScroll}>
          {(['all', 'pending', 'investigating', 'resolved'] as const).map((filter) => (
            <TouchableOpacity
              key={filter}
              style={[styles.filterTab, selectedFilter === filter && styles.filterTabActive]}
              onPress={() => setSelectedFilter(filter)}
            >
              <Text style={[styles.filterTabText, selectedFilter === filter && styles.filterTabTextActive]}>
                {filter.charAt(0).toUpperCase() + filter.slice(1)}
              </Text>
            </TouchableOpacity>
          ))}
        </ScrollView>

        {/* Incidents List */}
        {filteredIncidents.map((incident) => (
          <TouchableOpacity
            key={incident.id}
            style={[styles.incidentCard, incident.priority === 'critical' && styles.incidentCardCritical]}
            onPress={() => handleIncidentPress(incident)}
          >
            <View style={styles.incidentHeader}>
              <View style={[styles.priorityBadge, { backgroundColor: getPriorityColor(incident.priority) }]}>
                <Text style={styles.priorityText}>{incident.priority.toUpperCase()}</Text>
              </View>
              <View style={[styles.statusBadge, { backgroundColor: getStatusColor(incident.status) + '15' }]}>
                <Text style={[styles.statusText, { color: getStatusColor(incident.status) }]}>
                  {incident.status.toUpperCase()}
                </Text>
              </View>
            </View>
            
            <Text style={styles.incidentTitle}>{incident.title}</Text>
            <Text style={styles.incidentType}>{incident.type}</Text>
            
            <View style={styles.incidentMeta}>
              <View style={styles.metaItem}>
                <MaterialCommunityIcons name="map-marker" size={14} color={COLORS.gray400} />
                <Text style={styles.metaText}>{incident.location}</Text>
              </View>
              <View style={styles.metaItem}>
                <MaterialCommunityIcons name="clock-outline" size={14} color={COLORS.gray400} />
                <Text style={styles.metaText}>{incident.time}</Text>
              </View>
            </View>
            
            <View style={styles.reporterRow}>
              <MaterialCommunityIcons name="account" size={14} color={COLORS.gray400} />
              <Text style={styles.reporterText}>Reported by {incident.reportedBy}</Text>
            </View>
          </TouchableOpacity>
        ))}

        {/* Emergency Actions */}
        <Card style={styles.emergencyCard}>
          <Text style={styles.emergencyTitle}>Emergency Actions</Text>
          <View style={styles.emergencyActions}>
            <TouchableOpacity style={styles.emergencyAction} onPress={handleEmergencyProtocol}>
              <MaterialCommunityIcons name="alert" size={24} color={COLORS.error} />
              <Text style={styles.emergencyActionText}>Emergency</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.emergencyAction} onPress={handleSendAlert}>
              <MaterialCommunityIcons name="bell" size={24} color={COLORS.warning} />
              <Text style={styles.emergencyActionText}>Send Alert</Text>
            </TouchableOpacity>
          </View>
        </Card>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  addButton: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center' },
  content: { padding: SIZES.md },
  statsContainer: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.md },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.md, marginHorizontal: SIZES.xs },
  statNumber: { fontSize: FONTS.h2, fontWeight: 'bold' },
  statLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  actionsContainer: { flexDirection: 'row', justifyContent: 'space-around', marginBottom: SIZES.md },
  actionButton: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  actionButtonText: { fontSize: FONTS.bodySmall, color: COLORS.textPrimary, marginLeft: SIZES.xs, fontWeight: '500' },
  filterScroll: { marginBottom: SIZES.md },
  filterTab: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.white, borderRadius: SIZES.radiusFull, marginRight: SIZES.sm, ...SHADOWS.small },
  filterTabActive: { backgroundColor: COLORS.primary },
  filterTabText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  filterTabTextActive: { color: COLORS.white },
  incidentCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  incidentCardCritical: { borderLeftWidth: 4, borderLeftColor: COLORS.error },
  incidentHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.sm },
  priorityBadge: { paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  priorityText: { fontSize: FONTS.tiny, color: COLORS.white, fontWeight: '600' },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.tiny, fontWeight: '600' },
  incidentTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: 2 },
  incidentType: { fontSize: FONTS.caption, color: COLORS.gray500, marginBottom: SIZES.sm },
  incidentMeta: { flexDirection: 'row', marginBottom: SIZES.xs },
  metaItem: { flexDirection: 'row', alignItems: 'center', marginRight: SIZES.md },
  metaText: { fontSize: FONTS.caption, color: COLORS.gray500, marginLeft: 4 },
  reporterRow: { flexDirection: 'row', alignItems: 'center' },
  reporterText: { fontSize: FONTS.caption, color: COLORS.gray400, marginLeft: 4 },
  emergencyCard: { marginBottom: SIZES.xxl, padding: SIZES.md, backgroundColor: COLORS.error + '08' },
  emergencyTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  emergencyActions: { flexDirection: 'row', justifyContent: 'space-around' },
  emergencyAction: { alignItems: 'center', padding: SIZES.md },
  emergencyActionText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: SIZES.xl },
  loadingText: { marginTop: SIZES.md, fontSize: FONTS.body, color: COLORS.textSecondary },
});

export default IncidentReviewScreen;
