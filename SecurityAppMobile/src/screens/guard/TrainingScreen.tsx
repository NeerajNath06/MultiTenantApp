import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, Image, RefreshControl, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { trainingService, type TrainingRecordItem } from '../../services/trainingService';

interface TrainingDisplay {
  id: string;
  title: string;
  category: string;
  duration: string;
  progress: number;
  status: 'not_started' | 'in_progress' | 'completed';
  mandatory: boolean;
  deadline?: string;
  modules: number;
  completedModules: number;
}

interface CertificateDisplay {
  id: string;
  name: string;
  issuedOn: string;
  validUntil: string;
  status: 'valid' | 'expiring' | 'expired';
}

function TrainingScreen({ navigation }: any) {
  const [activeTab, setActiveTab] = useState<'courses' | 'certificates'>('courses');
  const [trainings, setTrainings] = useState<TrainingDisplay[]>([]);
  const [certificates, setCertificates] = useState<CertificateDisplay[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const load = useCallback(async () => {
    let guardId: string | undefined;
    try {
      const userData = await AsyncStorage.getItem('userData');
      if (userData) {
        const parsed = JSON.parse(userData);
        guardId = parsed?.guardId ?? parsed?.id ?? parsed?.userId ?? undefined;
      }
    } catch {}
    const res = await trainingService.getTrainingRecords({ guardId, pageSize: 100 });
    if (res.success && res.data?.items) {
      const items = res.data.items as TrainingRecordItem[];
      const courses: TrainingDisplay[] = items.map(r => ({
        id: r.id,
        title: r.trainingName || r.trainingType,
        category: r.trainingType || 'General',
        duration: '-',
        progress: (r.status || '').toLowerCase() === 'completed' ? 100 : (r.status || '').toLowerCase() === 'scheduled' ? 0 : 50,
        status: (r.status || '').toLowerCase() === 'completed' ? 'completed' : (r.status || '').toLowerCase() === 'scheduled' ? 'not_started' : 'in_progress',
        mandatory: false,
        deadline: r.expiryDate ? r.expiryDate.slice(0, 10) : undefined,
        modules: 1,
        completedModules: (r.status || '').toLowerCase() === 'completed' ? 1 : 0,
      }));
      setTrainings(courses);
      const certs: CertificateDisplay[] = items
        .filter(r => r.certificateNumber || r.expiryDate)
        .map(r => {
          let status: 'valid' | 'expiring' | 'expired' = 'valid';
          if (r.expiryDate) {
            const exp = new Date(r.expiryDate);
            if (exp < new Date()) status = 'expired';
            else if (exp < new Date(Date.now() + 90 * 86400000)) status = 'expiring';
          }
          return {
            id: r.id,
            name: r.trainingName || r.trainingType,
            issuedOn: r.trainingDate?.slice(0, 10) ?? '-',
            validUntil: r.expiryDate?.slice(0, 10) ?? '-',
            status,
          };
        });
      setCertificates(certs);
    } else {
      setTrainings([]);
      setCertificates([]);
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

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed':
      case 'valid': return COLORS.success;
      case 'in_progress':
      case 'expiring': return COLORS.warning;
      case 'not_started':
      case 'expired': return COLORS.error;
      default: return COLORS.gray500;
    }
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

  const handleStartTraining = (training: TrainingDisplay) => {
    Alert.alert('Start Training', `Starting "${training.title}"...`);
  };

  const handleDownloadCertificate = (certificate: CertificateDisplay) => {
    Alert.alert('Download', `Downloading ${certificate.name}...`);
  };

  const completedCount = trainings.filter(t => t.status === 'completed').length;
  const inProgressCount = trainings.filter(t => t.status === 'in_progress').length;
  const mandatoryPending = trainings.filter(t => t.mandatory && t.status !== 'completed').length;

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
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Training & Certifications</Text>
        <View style={styles.placeholder} />
      </View>

      {/* Stats */}
      <View style={styles.statsRow}>
        <View style={[styles.statCard, { backgroundColor: COLORS.success + '15' }]}>
          <MaterialCommunityIcons name="check-circle" size={24} color={COLORS.success} />
          <Text style={[styles.statValue, { color: COLORS.success }]}>{completedCount}</Text>
          <Text style={styles.statLabel}>Completed</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.warning + '15' }]}>
          <MaterialCommunityIcons name="progress-clock" size={24} color={COLORS.warning} />
          <Text style={[styles.statValue, { color: COLORS.warning }]}>{inProgressCount}</Text>
          <Text style={styles.statLabel}>In Progress</Text>
        </View>
        <View style={[styles.statCard, { backgroundColor: COLORS.error + '15' }]}>
          <MaterialCommunityIcons name="alert-circle" size={24} color={COLORS.error} />
          <Text style={[styles.statValue, { color: COLORS.error }]}>{mandatoryPending}</Text>
          <Text style={styles.statLabel}>Mandatory Due</Text>
        </View>
      </View>

      {/* Tabs */}
      <View style={styles.tabContainer}>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'courses' && styles.tabActive]}
          onPress={() => setActiveTab('courses')}
        >
          <MaterialCommunityIcons name="school" size={20} color={activeTab === 'courses' ? COLORS.primary : COLORS.gray500} />
          <Text style={[styles.tabText, activeTab === 'courses' && styles.tabTextActive]}>Courses</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.tab, activeTab === 'certificates' && styles.tabActive]}
          onPress={() => setActiveTab('certificates')}
        >
          <MaterialCommunityIcons name="certificate" size={20} color={activeTab === 'certificates' ? COLORS.primary : COLORS.gray500} />
          <Text style={[styles.tabText, activeTab === 'certificates' && styles.tabTextActive]}>Certificates</Text>
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.content}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
      >
        {activeTab === 'courses' ? (
          <>
            {trainings.map((training) => (
              <TouchableOpacity
                key={training.id}
                style={styles.trainingCard}
                onPress={() => handleStartTraining(training)}
              >
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

                <View style={styles.trainingMeta}>
                  <View style={styles.metaItem}>
                    <MaterialCommunityIcons name="clock-outline" size={14} color={COLORS.gray500} />
                    <Text style={styles.metaText}>{training.duration}</Text>
                  </View>
                  <View style={styles.metaItem}>
                    <MaterialCommunityIcons name="view-list" size={14} color={COLORS.gray500} />
                    <Text style={styles.metaText}>{training.modules} modules</Text>
                  </View>
                  {training.deadline && (
                    <View style={styles.metaItem}>
                      <MaterialCommunityIcons name="calendar-alert" size={14} color={COLORS.error} />
                      <Text style={[styles.metaText, { color: COLORS.error }]}>Due: {training.deadline}</Text>
                    </View>
                  )}
                </View>

                <View style={styles.progressContainer}>
                  <View style={styles.progressHeader}>
                    <Text style={styles.progressLabel}>{training.completedModules}/{training.modules} modules</Text>
                    <Text style={styles.progressPercent}>{training.progress}%</Text>
                  </View>
                  <View style={styles.progressBar}>
                    <View style={[styles.progressFill, { width: `${training.progress}%`, backgroundColor: getStatusColor(training.status) }]} />
                  </View>
                </View>

                <View style={styles.trainingFooter}>
                  <View style={[styles.statusBadge, { backgroundColor: getStatusColor(training.status) + '15' }]}>
                    <Text style={[styles.statusText, { color: getStatusColor(training.status) }]}>
                      {training.status === 'not_started' ? 'Not Started' : training.status === 'in_progress' ? 'In Progress' : 'Completed'}
                    </Text>
                  </View>
                  <TouchableOpacity style={styles.startBtn}>
                    <Text style={styles.startBtnText}>
                      {training.status === 'not_started' ? 'Start' : training.status === 'in_progress' ? 'Continue' : 'Review'}
                    </Text>
                    <MaterialCommunityIcons name="arrow-right" size={18} color={COLORS.primary} />
                  </TouchableOpacity>
                </View>
              </TouchableOpacity>
            ))}
          </>
        ) : (
          <>
            {certificates.map((cert) => (
              <View key={cert.id} style={styles.certCard}>
                <View style={styles.certIcon}>
                  <MaterialCommunityIcons name="certificate" size={32} color={getStatusColor(cert.status)} />
                </View>
                <View style={styles.certInfo}>
                  <Text style={styles.certName}>{cert.name}</Text>
                  <Text style={styles.certDate}>Issued: {cert.issuedOn}</Text>
                  <View style={styles.certValidity}>
                    <MaterialCommunityIcons 
                      name={cert.status === 'valid' ? 'check-circle' : cert.status === 'expiring' ? 'alert-circle' : 'close-circle'} 
                      size={14} 
                      color={getStatusColor(cert.status)} 
                    />
                    <Text style={[styles.certValidText, { color: getStatusColor(cert.status) }]}>
                      {cert.status === 'expired' ? 'Expired' : `Valid until ${cert.validUntil}`}
                    </Text>
                  </View>
                </View>
                <TouchableOpacity style={styles.downloadBtn} onPress={() => handleDownloadCertificate(cert)}>
                  <MaterialCommunityIcons name="download" size={24} color={COLORS.primaryBlue} />
                </TouchableOpacity>
              </View>
            ))}

            {/* Renew Alert */}
            <View style={styles.renewCard}>
              <MaterialCommunityIcons name="alert" size={24} color={COLORS.warning} />
              <View style={styles.renewContent}>
                <Text style={styles.renewTitle}>Certificates Expiring Soon</Text>
                <Text style={styles.renewText}>You have {certificates.filter(c => c.status === 'expiring' || c.status === 'expired').length} certificates that need renewal.</Text>
              </View>
              <TouchableOpacity style={styles.renewBtn}>
                <Text style={styles.renewBtnText}>Renew Now</Text>
              </TouchableOpacity>
            </View>
          </>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  statsRow: { flexDirection: 'row', paddingHorizontal: SIZES.md, paddingTop: SIZES.md, gap: SIZES.sm },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.md, borderRadius: SIZES.radiusMd },
  statValue: { fontSize: FONTS.h3, fontWeight: 'bold', marginTop: SIZES.xs },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2, textAlign: 'center' },
  tabContainer: { flexDirection: 'row', marginHorizontal: SIZES.md, marginTop: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: 4, ...SHADOWS.small },
  tab: { flex: 1, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm, gap: SIZES.xs },
  tabActive: { backgroundColor: COLORS.primary + '10' },
  tabText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  tabTextActive: { color: COLORS.primary, fontWeight: '600' },
  content: { padding: SIZES.md },
  trainingCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.md, ...SHADOWS.small },
  trainingHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.sm },
  categoryBadge: { paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  categoryText: { fontSize: FONTS.tiny, fontWeight: '600' },
  mandatoryBadge: { flexDirection: 'row', alignItems: 'center', gap: 2 },
  mandatoryText: { fontSize: FONTS.tiny, color: COLORS.error, fontWeight: '600' },
  trainingTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  trainingMeta: { flexDirection: 'row', flexWrap: 'wrap', gap: SIZES.md, marginBottom: SIZES.sm },
  metaItem: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  metaText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  progressContainer: { marginBottom: SIZES.sm },
  progressHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: 4 },
  progressLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  progressPercent: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.textPrimary },
  progressBar: { height: 6, backgroundColor: COLORS.gray200, borderRadius: 3, overflow: 'hidden' },
  progressFill: { height: '100%', borderRadius: 3 },
  trainingFooter: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: SIZES.sm, paddingTop: SIZES.sm, borderTopWidth: 1, borderTopColor: COLORS.gray100 },
  statusBadge: { paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  statusText: { fontSize: FONTS.caption, fontWeight: '600' },
  startBtn: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  startBtnText: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.primary },
  certCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  certIcon: { width: 56, height: 56, borderRadius: 28, backgroundColor: COLORS.gray100, justifyContent: 'center', alignItems: 'center' },
  certInfo: { flex: 1, marginLeft: SIZES.sm },
  certName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  certDate: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  certValidity: { flexDirection: 'row', alignItems: 'center', gap: 4, marginTop: 4 },
  certValidText: { fontSize: FONTS.caption, fontWeight: '500' },
  downloadBtn: { width: 44, height: 44, borderRadius: 22, backgroundColor: COLORS.primaryBlue + '10', justifyContent: 'center', alignItems: 'center' },
  renewCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.warning + '10', borderRadius: SIZES.radiusMd, padding: SIZES.md, marginTop: SIZES.md },
  renewContent: { flex: 1, marginLeft: SIZES.sm },
  renewTitle: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.warning },
  renewText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  renewBtn: { backgroundColor: COLORS.warning, paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusSm },
  renewBtnText: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.white },
});

export default TrainingScreen;
