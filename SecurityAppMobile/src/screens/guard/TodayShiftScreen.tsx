import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { TodayShiftScreenProps } from '../../types/navigation';
import { authService } from '../../services/authService';
import { deploymentService } from '../../services/deploymentService';
import { siteService } from '../../services/siteService';
import { formatFullDateIST } from '../../utils/dateUtils';

interface ShiftDetails {
  date: string;
  startTime: string;
  endTime: string;
  site: string;
  address: string;
  supervisor: string;
  supervisorPhone: string;
  status: string;
}

interface Checkpoint {
  id: number;
  name: string;
  time: string;
  status: 'completed' | 'pending';
}

const TodayShiftScreen: React.FC<TodayShiftScreenProps> = ({ navigation }) => {
  const [loading, setLoading] = useState(true);
  const [shiftDetails, setShiftDetails] = useState<ShiftDetails | null>(null);
  const [checkpoints, setCheckpoints] = useState<Checkpoint[]>([]);

  useEffect(() => {
    loadShiftData();
  }, []);

  const loadShiftData = async () => {
    try {
      setLoading(true);
      const user = await authService.getStoredUser();
      if (!user) {
        setLoading(false);
        return;
      }
      const guardId = (user as { guardId?: string }).guardId || user.id;
      const today = new Date();
      const todayStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
      const deploymentsRes = await deploymentService.getDeployments({
        guardId,
        dateFrom: todayStr,
        dateTo: todayStr,
        pageSize: 50,
        skipCache: true,
      });
      const rawDep = deploymentsRes.data;
      const allDeployments: any[] = Array.isArray(rawDep) ? rawDep : (rawDep?.items ?? rawDep?.data ?? []);
      const deploymentsList = allDeployments.filter(
        (d: any) => (d.deploymentDate ?? d.DeploymentDate ?? '').toString().slice(0, 10) === todayStr
      );
      const firstDeployment = deploymentsList[0];
      if (firstDeployment) {
        let address = '';
        const sitesRes = await siteService.getSites({ pageSize: 100 });
        if (sitesRes.success && sitesRes.data) {
          const sites = Array.isArray(sitesRes.data) ? sitesRes.data : (sitesRes.data as any)?.data ?? [];
          const site = sites.find((s: any) => (s.id ?? s.Id) === (firstDeployment.siteId ?? firstDeployment.SiteId));
          address = site?.address ?? site?.Address ?? '';
        }
        setShiftDetails({
          date: formatFullDateIST(new Date()) ?? new Date().toLocaleDateString('en-IN'),
          startTime: firstDeployment.startTime ?? '08:00',
          endTime: firstDeployment.endTime ?? '20:00',
          site: firstDeployment.siteName ?? firstDeployment.SiteName ?? '—',
          address: address || '—',
          supervisor: firstDeployment.supervisorName ?? '—',
          supervisorPhone: firstDeployment.supervisorPhone ?? '—',
          status: 'Active',
        });
        setCheckpoints([
          { id: 1, name: 'Main Gate', time: firstDeployment.startTime ?? '08:00', status: 'pending' },
          { id: 2, name: 'Block Patrol', time: '10:00', status: 'pending' },
          { id: 3, name: 'Visitor Log', time: '12:00', status: 'pending' },
          { id: 4, name: 'Evening Round', time: firstDeployment.endTime ?? '20:00', status: 'pending' },
        ]);
      } else {
        setShiftDetails(null);
        setCheckpoints([]);
      }
    } catch (e) {
      console.error(e);
      setShiftDetails(null);
      setCheckpoints([]);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Today's Shift</Text>
          <View style={styles.placeholder} />
        </View>
        <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={{ marginTop: SIZES.md, color: COLORS.textSecondary }}>Loading shift...</Text>
        </View>
      </SafeAreaView>
    );
  }

  if (!shiftDetails) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Today's Shift</Text>
          <View style={styles.placeholder} />
        </View>
        <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', padding: SIZES.lg }}>
          <Text style={{ fontSize: FONTS.body, color: COLORS.textSecondary, textAlign: 'center' }}>
            No shift scheduled for today.
          </Text>
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
        <Text style={styles.headerTitle}>Today's Shift</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView contentContainerStyle={styles.content} showsVerticalScrollIndicator={false}>
        <Card style={styles.mainCard}>
          <View style={styles.statusBadge}>
            <View style={[styles.statusDot, { backgroundColor: COLORS.success }]} />
            <Text style={[styles.statusText, { color: COLORS.success }]}>{shiftDetails.status}</Text>
          </View>
          
          <View style={styles.timeRow}>
            <View style={styles.timeBlock}>
              <Text style={styles.timeLabel}>Start Time</Text>
              <Text style={styles.timeValue}>{shiftDetails.startTime}</Text>
            </View>
            <View style={styles.timeDivider}>
              <MaterialCommunityIcons name="arrow-right" size={20} color={COLORS.gray400} />
            </View>
            <View style={styles.timeBlock}>
              <Text style={styles.timeLabel}>End Time</Text>
              <Text style={styles.timeValue}>{shiftDetails.endTime}</Text>
            </View>
          </View>

          <View style={styles.divider} />

          <View style={styles.infoRow}>
            <MaterialCommunityIcons name="office-building" size={20} color={COLORS.primary} />
            <View style={styles.infoContent}>
              <Text style={styles.infoLabel}>Site</Text>
              <Text style={styles.infoValue}>{shiftDetails.site}</Text>
              <Text style={styles.infoSubtext}>{shiftDetails.address}</Text>
            </View>
          </View>

          <View style={styles.infoRow}>
            <MaterialCommunityIcons name="account" size={20} color={COLORS.secondary} />
            <View style={styles.infoContent}>
              <Text style={styles.infoLabel}>Supervisor</Text>
              <Text style={styles.infoValue}>{shiftDetails.supervisor}</Text>
              <TouchableOpacity style={styles.callButton}>
                <MaterialCommunityIcons name="phone" size={14} color={COLORS.primary} />
                <Text style={styles.callText}>{shiftDetails.supervisorPhone}</Text>
              </TouchableOpacity>
            </View>
          </View>
        </Card>

        <Text style={styles.sectionTitle}>Patrol Checkpoints</Text>
        <Card style={styles.checkpointsCard}>
          {checkpoints.map((checkpoint, index) => (
            <View key={checkpoint.id}>
              <View style={styles.checkpointItem}>
                <View style={[
                  styles.checkpointIcon,
                  checkpoint.status === 'completed' ? styles.completedIcon : styles.pendingIcon
                ]}>
                  <MaterialCommunityIcons 
                    name={checkpoint.status === 'completed' ? 'check' : 'clock'} 
                    size={16} 
                    color={COLORS.white} 
                  />
                </View>
                <View style={styles.checkpointContent}>
                  <Text style={styles.checkpointName}>{checkpoint.name}</Text>
                  <Text style={styles.checkpointTime}>{checkpoint.time}</Text>
                </View>
                <Text style={[
                  styles.checkpointStatus,
                  { color: checkpoint.status === 'completed' ? COLORS.success : COLORS.warning }
                ]}>
                  {checkpoint.status === 'completed' ? 'Done' : 'Pending'}
                </Text>
              </View>
              {index < checkpoints.length - 1 && <View style={styles.checkpointLine} />}
            </View>
          ))}
        </Card>

        <TouchableOpacity 
          style={styles.navigateButton}
          onPress={() => navigation.navigate('SiteNavigation')}
        >
          <MaterialCommunityIcons name="navigation" size={24} color={COLORS.white} />
          <Text style={styles.navigateText}>Navigate to Site</Text>
        </TouchableOpacity>
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  content: { padding: SIZES.md },
  mainCard: { padding: SIZES.md, marginBottom: SIZES.md },
  statusBadge: { flexDirection: 'row', alignItems: 'center', alignSelf: 'flex-start', backgroundColor: COLORS.success + '15', paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm, marginBottom: SIZES.md },
  statusDot: { width: 8, height: 8, borderRadius: 4, marginRight: SIZES.xs },
  statusText: { fontSize: FONTS.caption, fontWeight: '600' },
  timeRow: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
  timeBlock: { flex: 1, alignItems: 'center' },
  timeLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  timeValue: { fontSize: FONTS.h3, fontWeight: 'bold', color: COLORS.textPrimary, marginTop: 4 },
  timeDivider: { paddingHorizontal: SIZES.md },
  divider: { height: 1, backgroundColor: COLORS.gray200, marginVertical: SIZES.md },
  infoRow: { flexDirection: 'row', marginBottom: SIZES.md },
  infoContent: { flex: 1, marginLeft: SIZES.sm },
  infoLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  infoValue: { fontSize: FONTS.body, fontWeight: '500', color: COLORS.textPrimary, marginTop: 2 },
  infoSubtext: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  callButton: { flexDirection: 'row', alignItems: 'center', marginTop: 4, gap: 4 },
  callText: { fontSize: FONTS.caption, color: COLORS.primary },
  sectionTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  checkpointsCard: { padding: SIZES.md, marginBottom: SIZES.lg },
  checkpointItem: { flexDirection: 'row', alignItems: 'center' },
  checkpointIcon: { width: 28, height: 28, borderRadius: 14, justifyContent: 'center', alignItems: 'center' },
  completedIcon: { backgroundColor: COLORS.success },
  pendingIcon: { backgroundColor: COLORS.warning },
  checkpointContent: { flex: 1, marginLeft: SIZES.sm },
  checkpointName: { fontSize: FONTS.body, fontWeight: '500', color: COLORS.textPrimary },
  checkpointTime: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  checkpointStatus: { fontSize: FONTS.caption, fontWeight: '600' },
  checkpointLine: { width: 2, height: 20, backgroundColor: COLORS.gray200, marginLeft: 13, marginVertical: 4 },
  navigateButton: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.primary, paddingVertical: SIZES.md, borderRadius: SIZES.radiusMd, gap: SIZES.sm, marginBottom: SIZES.xl },
  navigateText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.white },
});

export default TodayShiftScreen;
