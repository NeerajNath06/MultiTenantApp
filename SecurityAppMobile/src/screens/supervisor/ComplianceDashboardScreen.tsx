import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Dimensions, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { complianceService, ComplianceItem } from '../../services/complianceService';
import { authService } from '../../services/authService';

const { width } = Dimensions.get('window');

function ComplianceDashboardScreen({ navigation }: any) {
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [complianceItems, setComplianceItems] = useState<ComplianceItem[]>([]);
  const [compliantCount, setCompliantCount] = useState(0);
  const [warningCount, setWarningCount] = useState(0);
  const [nonCompliantCount, setNonCompliantCount] = useState(0);
  const [overallScore, setOverallScore] = useState(0);

  const loadSummary = async () => {
    try {
      const user = await authService.getStoredUser();
      const result = await complianceService.getSummary(user?.isSupervisor ? user.id : undefined);
      if (result.success && result.data) {
        const d = result.data;
        setComplianceItems(d.items ?? []);
        setCompliantCount(d.compliantCount ?? 0);
        setWarningCount(d.warningCount ?? 0);
        setNonCompliantCount(d.nonCompliantCount ?? 0);
        setOverallScore(d.overallScorePercent ?? 0);
      }
    } catch (_) {
      setComplianceItems([]);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    loadSummary();
  }, []);

  const onRefresh = () => {
    setRefreshing(true);
    loadSummary();
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'compliant': return COLORS.success;
      case 'warning': return COLORS.warning;
      case 'non-compliant': return COLORS.error;
      default: return COLORS.gray500;
    }
  };

  const getCategoryIcon = (category: string): keyof typeof MaterialCommunityIcons.glyphMap => {
    switch (category) {
      case 'license': return 'card-account-details';
      case 'training': return 'school';
      case 'audit': return 'clipboard-check';
      case 'document': return 'file-document';
      default: return 'checkbox-marked-circle';
    }
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Compliance Dashboard</Text>
          <View style={styles.exportBtn} />
        </View>
        <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
          <ActivityIndicator size="large" color={COLORS.primary} />
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Compliance Dashboard</Text>
        <TouchableOpacity style={styles.exportBtn}>
          <MaterialCommunityIcons name="file-export" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
      >
        {/* Overall Score */}
        <View style={styles.scoreCard}>
          <View style={styles.scoreCircle}>
            <Text style={styles.scoreValue}>{overallScore}%</Text>
            <Text style={styles.scoreLabel}>Compliance</Text>
          </View>
          <View style={styles.scoreStats}>
            <View style={styles.scoreStat}>
              <View style={[styles.scoreStatDot, { backgroundColor: COLORS.success }]} />
              <Text style={styles.scoreStatValue}>{compliantCount}</Text>
              <Text style={styles.scoreStatLabel}>Compliant</Text>
            </View>
            <View style={styles.scoreStat}>
              <View style={[styles.scoreStatDot, { backgroundColor: COLORS.warning }]} />
              <Text style={styles.scoreStatValue}>{warningCount}</Text>
              <Text style={styles.scoreStatLabel}>Warning</Text>
            </View>
            <View style={styles.scoreStat}>
              <View style={[styles.scoreStatDot, { backgroundColor: COLORS.error }]} />
              <Text style={styles.scoreStatValue}>{nonCompliantCount}</Text>
              <Text style={styles.scoreStatLabel}>Non-Compliant</Text>
            </View>
          </View>
        </View>

        {/* Urgent Actions */}
        {nonCompliantCount > 0 && (
          <View style={styles.urgentSection}>
            <View style={styles.urgentHeader}>
              <MaterialCommunityIcons name="alert-circle" size={24} color={COLORS.error} />
              <Text style={styles.urgentTitle}>Requires Immediate Attention</Text>
            </View>
            {complianceItems.filter(i => i.status === 'non-compliant').map((item) => (
              <TouchableOpacity key={String(item.id)} style={styles.urgentCard}>
                <View style={[styles.urgentIcon, { backgroundColor: COLORS.error + '15' }]}>
                  <MaterialCommunityIcons name={getCategoryIcon(item.category)} size={24} color={COLORS.error} />
                </View>
                <View style={styles.urgentContent}>
                  <Text style={styles.urgentCardTitle}>{item.title}</Text>
                  <Text style={styles.urgentCardDetails}>{item.details}</Text>
                  {item.dueDate && (
                    <Text style={styles.urgentDue}>Due: {item.dueDate}</Text>
                  )}
                </View>
                <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.error} />
              </TouchableOpacity>
            ))}
          </View>
        )}

        {/* All Compliance Items */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>All Compliance Items</Text>
          {complianceItems.map((item) => (
            <TouchableOpacity key={String(item.id)} style={styles.complianceCard}>
              <View style={[styles.complianceIcon, { backgroundColor: getStatusColor(item.status) + '15' }]}>
                <MaterialCommunityIcons name={getCategoryIcon(item.category)} size={24} color={getStatusColor(item.status)} />
              </View>
              <View style={styles.complianceContent}>
                <View style={styles.complianceHeader}>
                  <Text style={styles.complianceTitle}>{item.title}</Text>
                  <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.status) + '15' }]}>
                    <View style={[styles.statusDot, { backgroundColor: getStatusColor(item.status) }]} />
                    <Text style={[styles.statusText, { color: getStatusColor(item.status) }]}>
                      {item.status === 'non-compliant' ? 'Non-Compliant' : item.status.charAt(0).toUpperCase() + item.status.slice(1)}
                    </Text>
                  </View>
                </View>
                <Text style={styles.complianceDetails}>{item.details}</Text>
                {item.dueDate && (
                  <View style={styles.dueDateRow}>
                    <MaterialCommunityIcons name="calendar" size={14} color={COLORS.gray500} />
                    <Text style={styles.dueDateText}>Due: {item.dueDate}</Text>
                  </View>
                )}
              </View>
            </TouchableOpacity>
          ))}
        </View>

        {/* Quick Actions */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Quick Actions</Text>
          <View style={styles.actionsGrid}>
            <TouchableOpacity style={styles.actionCard}>
              <MaterialCommunityIcons name="file-document-edit" size={28} color={COLORS.primaryBlue} />
              <Text style={styles.actionLabel}>Generate Report</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionCard}>
              <MaterialCommunityIcons name="bell-ring" size={28} color={COLORS.warning} />
              <Text style={styles.actionLabel}>Send Reminders</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionCard}>
              <MaterialCommunityIcons name="calendar-check" size={28} color={COLORS.success} />
              <Text style={styles.actionLabel}>Schedule Audit</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.actionCard}>
              <MaterialCommunityIcons name="upload" size={28} color={COLORS.secondary} />
              <Text style={styles.actionLabel}>Upload Documents</Text>
            </TouchableOpacity>
          </View>
        </View>

        <View style={{ height: 50 }} />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.primary },
  backBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.white },
  exportBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  scoreCard: { margin: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusLg, padding: SIZES.lg, flexDirection: 'row', alignItems: 'center', ...SHADOWS.medium },
  scoreCircle: { width: 100, height: 100, borderRadius: 50, borderWidth: 8, borderColor: COLORS.success, justifyContent: 'center', alignItems: 'center' },
  scoreValue: { fontSize: FONTS.h2, fontWeight: 'bold', color: COLORS.success },
  scoreLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary },
  scoreStats: { flex: 1, marginLeft: SIZES.lg },
  scoreStat: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.sm },
  scoreStatDot: { width: 12, height: 12, borderRadius: 6, marginRight: SIZES.sm },
  scoreStatValue: { fontSize: FONTS.h4, fontWeight: 'bold', color: COLORS.textPrimary, width: 30 },
  scoreStatLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  urgentSection: { marginHorizontal: SIZES.md, backgroundColor: COLORS.error + '10', borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.md },
  urgentHeader: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.md },
  urgentTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.error, marginLeft: SIZES.sm },
  urgentCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm },
  urgentIcon: { width: 48, height: 48, borderRadius: 24, justifyContent: 'center', alignItems: 'center' },
  urgentContent: { flex: 1, marginLeft: SIZES.sm },
  urgentCardTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  urgentCardDetails: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  urgentDue: { fontSize: FONTS.tiny, color: COLORS.error, marginTop: 4 },
  section: { padding: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  complianceCard: { flexDirection: 'row', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  complianceIcon: { width: 48, height: 48, borderRadius: 24, justifyContent: 'center', alignItems: 'center' },
  complianceContent: { flex: 1, marginLeft: SIZES.sm },
  complianceHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  complianceTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, flex: 1 },
  statusBadge: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusFull },
  statusDot: { width: 6, height: 6, borderRadius: 3, marginRight: 4 },
  statusText: { fontSize: FONTS.tiny, fontWeight: '600' },
  complianceDetails: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 4 },
  dueDateRow: { flexDirection: 'row', alignItems: 'center', marginTop: 4 },
  dueDateText: { fontSize: FONTS.tiny, color: COLORS.gray500, marginLeft: 4 },
  actionsGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: SIZES.sm },
  actionCard: { width: (width - 48) / 2, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, alignItems: 'center', ...SHADOWS.small },
  actionLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.sm, textAlign: 'center' },
});

export default ComplianceDashboardScreen;
