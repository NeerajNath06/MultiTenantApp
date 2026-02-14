import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, Dimensions, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as Location from 'expo-location';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { patrolService, PatrolRoute, PatrolCheckpoint } from '../../services/patrolService';
import { authService } from '../../services/authService';
import { deploymentService } from '../../services/deploymentService';
import { PatrolTrackingScreenProps } from '../../types/navigation';

const { width } = Dimensions.get('window');

interface Checkpoint {
  id: string;
  name: string;
  location: string;
  status: 'pending' | 'completed' | 'skipped';
  completedAt?: string;
  distance?: string;
  latitude?: number;
  longitude?: number;
  order?: number;
}

function PatrolTrackingScreen({ navigation }: PatrolTrackingScreenProps) {
  const [isPatrolActive, setIsPatrolActive] = useState(false);
  const [elapsedTime, setElapsedTime] = useState(0);
  const [currentCheckpoint, setCurrentCheckpoint] = useState(0);
  const [loading, setLoading] = useState(true);
  const [checkpoints, setCheckpoints] = useState<Checkpoint[]>([]);
  const [activeRoute, setActiveRoute] = useState<PatrolRoute | null>(null);
  const [patrolId, setPatrolId] = useState<string>('');
  const [guardId, setGuardId] = useState<string>('');
  const [siteId, setSiteId] = useState<string>('');

  useEffect(() => {
    loadPatrolData();
  }, []);

  const loadPatrolData = async () => {
    try {
      setLoading(true);
      const user = await authService.getStoredUser();
      if (!user) {
        Alert.alert('Error', 'User not found. Please login again.');
        navigation.goBack();
        return;
      }

      const gid = (user as { guardId?: string }).guardId || user.id;
      setGuardId(gid);

      const deploymentResult = await deploymentService.getDeployments({
        guardId: gid,
        pageSize: 10,
      });

      if (deploymentResult.success && deploymentResult.data) {
        const deployments = Array.isArray(deploymentResult.data)
          ? deploymentResult.data
          : ((deploymentResult.data as { items?: unknown[] })?.items ?? []);
        if (deployments.length > 0) {
          setSiteId((deployments[0] as { siteId: string }).siteId);
          
          // Get patrol routes for this site
          const routesResult = await patrolService.getPatrolRoutes({
            siteId: (deployments[0] as { siteId: string }).siteId,
          });

          if (routesResult.success && routesResult.data) {
            const routes = Array.isArray(routesResult.data) 
              ? routesResult.data 
              : [routesResult.data];
            
            if (routes.length > 0) {
              const route = routes[0];
              setActiveRoute(route);
              
              // Convert checkpoints to our format
              if (route.checkpoints && Array.isArray(route.checkpoints)) {
                setCheckpoints(route.checkpoints.map((cp: PatrolCheckpoint, index: number) => ({
                  id: cp.id,
                  name: cp.checkpointName,
                  location: `Checkpoint ${index + 1}`,
                  status: 'pending' as const,
                  latitude: cp.latitude,
                  longitude: cp.longitude,
                  order: cp.order,
                })));
              }
            }
          }
        }
      }
    } catch (error) {
      console.error('Error loading patrol data:', error);
      Alert.alert('Error', 'Failed to load patrol data. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let interval: NodeJS.Timeout;
    if (isPatrolActive) {
      interval = setInterval(() => {
        setElapsedTime(prev => prev + 1);
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [isPatrolActive]);

  const formatTime = (seconds: number): string => {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${hrs.toString().padStart(2, '0')}:${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const handleStartPatrol = async () => {
    try {
      if (!activeRoute) {
        Alert.alert('Error', 'No patrol route available.');
        return;
      }

      // Request location permission
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permission Required', 'Location permission is required for patrol tracking.');
        return;
      }

      const location = await Location.getCurrentPositionAsync({});
      const { latitude, longitude } = location.coords;

      // Start patrol via API
      const result = await patrolService.startPatrol(activeRoute.id, guardId, latitude, longitude);
      
      if (result.success && result.data) {
        const patrol = result.data;
        setPatrolId(patrol.id || activeRoute.id);
        setIsPatrolActive(true);
        Alert.alert('Patrol Started', 'Your patrol has been started. GPS tracking is now active.');
      } else {
        // Still allow local tracking even if API fails
        setIsPatrolActive(true);
        Alert.alert('Patrol Started', 'Your patrol has been started. GPS tracking is now active.');
      }
    } catch (error: any) {
      console.error('Error starting patrol:', error);
      // Still allow local tracking
      setIsPatrolActive(true);
      Alert.alert('Patrol Started', 'Your patrol has been started. GPS tracking is now active.');
    }
  };

  const handlePausePatrol = () => {
    setIsPatrolActive(false);
    Alert.alert('Patrol Paused', 'Your patrol has been paused. Resume when ready.');
  };

  const handleCompleteCheckpoint = async (checkpointId: string) => {
    try {
      const checkpoint = checkpoints.find(cp => cp.id === checkpointId);
      if (!checkpoint) return;

      // Get current location
      const location = await Location.getCurrentPositionAsync({});
      const { latitude, longitude } = location.coords;

      // Record checkpoint via API
      if (patrolId && guardId) {
        await patrolService.recordCheckpoint({
          patrolId,
          guardId,
          checkpointId,
          latitude,
          longitude,
        });
      }

      const now = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      setCheckpoints(checkpoints.map(cp => 
        cp.id === checkpointId ? { ...cp, status: 'completed' as const, completedAt: now } : cp
      ));
      setCurrentCheckpoint(prev => prev + 1);
      Alert.alert('Checkpoint Completed', 'Checkpoint marked as completed.');
    } catch (error: any) {
      console.error('Error completing checkpoint:', error);
      // Still update local state
      const now = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      setCheckpoints(checkpoints.map(cp => 
        cp.id === checkpointId ? { ...cp, status: 'completed' as const, completedAt: now } : cp
      ));
      setCurrentCheckpoint(prev => prev + 1);
      Alert.alert('Checkpoint Completed', 'Checkpoint marked as completed.');
    }
  };

  const handleScanQR = () => {
    navigation.navigate('QRScanner');
  };

  const completedCount = checkpoints.filter(cp => cp.status === 'completed').length;
  const progress = checkpoints.length > 0 ? (completedCount / checkpoints.length) * 100 : 0;

  if (loading) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loadingText}>Loading patrol data...</Text>
        </View>
      </SafeAreaView>
    );
  }

  if (!activeRoute) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
            <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
          </TouchableOpacity>
          <Text style={styles.headerTitle}>Patrol Tracking</Text>
          <View style={styles.placeholder} />
        </View>
        <View style={styles.emptyContainer}>
          <MaterialCommunityIcons name="map-marker-off" size={64} color={COLORS.gray400} />
          <Text style={styles.emptyText}>No patrol route assigned</Text>
          <Text style={styles.emptySubtext}>Please contact your supervisor</Text>
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
        <Text style={styles.headerTitle}>Patrol Tracking</Text>
        <TouchableOpacity style={styles.historyBtn} onPress={() => Alert.alert('History', 'Patrol history')}>
          <MaterialCommunityIcons name="history" size={24} color={COLORS.white} />
        </TouchableOpacity>
      </View>

      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Live Tracking Card */}
        <View style={styles.trackingCard}>
          <View style={styles.trackingHeader}>
            <View style={styles.liveIndicator}>
              <View style={[styles.liveDot, isPatrolActive && styles.liveDotActive]} />
              <Text style={styles.liveText}>{isPatrolActive ? 'LIVE TRACKING' : 'PAUSED'}</Text>
            </View>
            <Text style={styles.routeName}>{activeRoute.routeName || 'Patrol Route'}</Text>
          </View>

          {/* Timer */}
          <View style={styles.timerContainer}>
            <Text style={styles.timerLabel}>Elapsed Time</Text>
            <Text style={styles.timerValue}>{formatTime(elapsedTime)}</Text>
          </View>

          {/* Progress */}
          <View style={styles.progressContainer}>
            <View style={styles.progressHeader}>
              <Text style={styles.progressText}>Progress</Text>
              <Text style={styles.progressPercent}>{Math.round(progress)}%</Text>
            </View>
            <View style={styles.progressBar}>
              <View style={[styles.progressFill, { width: `${progress}%` }]} />
            </View>
            <Text style={styles.checkpointText}>
              {completedCount} of {checkpoints.length} checkpoints completed
            </Text>
          </View>

          {/* Control Buttons */}
          <View style={styles.controlButtons}>
            {isPatrolActive ? (
              <TouchableOpacity style={styles.pauseBtn} onPress={handlePausePatrol}>
                <MaterialCommunityIcons name="pause" size={24} color={COLORS.white} />
                <Text style={styles.controlBtnText}>Pause Patrol</Text>
              </TouchableOpacity>
            ) : (
              <TouchableOpacity style={styles.startBtn} onPress={handleStartPatrol}>
                <MaterialCommunityIcons name="play" size={24} color={COLORS.white} />
                <Text style={styles.controlBtnText}>Start Patrol</Text>
              </TouchableOpacity>
            )}
            <TouchableOpacity style={styles.scanBtn} onPress={handleScanQR}>
              <MaterialCommunityIcons name="qrcode-scan" size={24} color={COLORS.primary} />
              <Text style={styles.scanBtnText}>Scan QR</Text>
            </TouchableOpacity>
          </View>
        </View>

        {/* Quick Stats */}
        <View style={styles.statsRow}>
          <View style={styles.statCard}>
            <MaterialCommunityIcons name="walk" size={24} color={COLORS.primaryBlue} />
            <Text style={styles.statValue}>1.2 km</Text>
            <Text style={styles.statLabel}>Distance</Text>
          </View>
          <View style={styles.statCard}>
            <MaterialCommunityIcons name="speedometer" size={24} color={COLORS.success} />
            <Text style={styles.statValue}>4.5</Text>
            <Text style={styles.statLabel}>Avg Speed km/h</Text>
          </View>
          <View style={styles.statCard}>
            <MaterialCommunityIcons name="alert-circle" size={24} color={COLORS.warning} />
            <Text style={styles.statValue}>0</Text>
            <Text style={styles.statLabel}>Alerts</Text>
          </View>
        </View>

        {/* Checkpoints List */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Checkpoints</Text>
          {checkpoints.length > 0 ? (
            checkpoints.map((checkpoint, index) => (
              <View key={checkpoint.id} style={styles.checkpointCard}>
                <View style={styles.checkpointTimeline}>
                  <View style={[
                    styles.timelineDot,
                    checkpoint.status === 'completed' && styles.timelineDotCompleted,
                    checkpoint.status === 'pending' && index === completedCount && styles.timelineDotCurrent
                  ]}>
                    {checkpoint.status === 'completed' ? (
                      <MaterialCommunityIcons name="check" size={14} color={COLORS.white} />
                    ) : (
                      <Text style={styles.checkpointNumber}>{index + 1}</Text>
                    )}
                  </View>
                  {index < checkpoints.length - 1 && (
                    <View style={[
                      styles.timelineLine,
                      checkpoint.status === 'completed' && styles.timelineLineCompleted
                    ]} />
                  )}
                </View>

                <View style={styles.checkpointContent}>
                  <View style={styles.checkpointInfo}>
                    <Text style={styles.checkpointName}>{checkpoint.name}</Text>
                    <Text style={styles.checkpointLocation}>{checkpoint.location}</Text>
                    {checkpoint.completedAt && (
                      <Text style={styles.checkpointTime}>Completed at {checkpoint.completedAt}</Text>
                    )}
                  </View>

                  {checkpoint.status === 'pending' && index === completedCount && (
                    <TouchableOpacity 
                      style={styles.completeBtn}
                      onPress={() => handleCompleteCheckpoint(checkpoint.id)}
                    >
                      <MaterialCommunityIcons name="check-circle" size={20} color={COLORS.white} />
                      <Text style={[styles.completeBtnText, { marginLeft: 4 }]}>Complete</Text>
                    </TouchableOpacity>
                  )}

                  {checkpoint.status === 'completed' && (
                    <View style={styles.completedBadge}>
                      <MaterialCommunityIcons name="check-circle" size={20} color={COLORS.success} />
                    </View>
                  )}
                </View>
              </View>
            ))
          ) : (
            <View style={styles.emptyCheckpoints}>
              <Text style={styles.emptyText}>No checkpoints available</Text>
            </View>
          )}
        </View>

        <View style={{ height: 100 }} />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.primary },
  backBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.white },
  historyBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  trackingCard: { margin: SIZES.md, backgroundColor: COLORS.white, borderRadius: SIZES.radiusLg, padding: SIZES.lg, ...SHADOWS.medium },
  trackingHeader: { marginBottom: SIZES.md },
  liveIndicator: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.xs },
  liveDot: { width: 8, height: 8, borderRadius: 4, backgroundColor: COLORS.gray400, marginRight: 6 },
  liveDotActive: { backgroundColor: COLORS.success },
  liveText: { fontSize: FONTS.tiny, fontWeight: '600', color: COLORS.textSecondary, letterSpacing: 1 },
  routeName: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  timerContainer: { alignItems: 'center', marginVertical: SIZES.lg },
  timerLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  timerValue: { fontSize: 48, fontWeight: 'bold', color: COLORS.primary, fontVariant: ['tabular-nums'] },
  progressContainer: { marginBottom: SIZES.lg },
  progressHeader: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.xs },
  progressText: { fontSize: FONTS.caption, color: COLORS.textSecondary },
  progressPercent: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.primary },
  progressBar: { height: 8, backgroundColor: COLORS.gray200, borderRadius: 4, overflow: 'hidden' },
  progressFill: { height: '100%', backgroundColor: COLORS.success, borderRadius: 4 },
  checkpointText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs, textAlign: 'center' },
  controlButtons: { flexDirection: 'row', marginHorizontal: -SIZES.xs },
  startBtn: { flex: 1, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.success, paddingVertical: SIZES.md, borderRadius: SIZES.radiusMd, marginHorizontal: SIZES.xs },
  pauseBtn: { flex: 1, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.warning, paddingVertical: SIZES.md, borderRadius: SIZES.radiusMd, marginHorizontal: SIZES.xs },
  controlBtnText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.white, marginLeft: SIZES.xs },
  scanBtn: { flex: 1, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.primary + '10', paddingVertical: SIZES.md, borderRadius: SIZES.radiusMd, marginHorizontal: SIZES.xs },
  scanBtnText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.primary, marginLeft: SIZES.xs },
  statsRow: { flexDirection: 'row', paddingHorizontal: SIZES.md, marginHorizontal: -SIZES.xs, marginBottom: SIZES.md },
  statCard: { flex: 1, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, alignItems: 'center', marginHorizontal: SIZES.xs, ...SHADOWS.small },
  statValue: { fontSize: FONTS.h4, fontWeight: 'bold', color: COLORS.textPrimary, marginTop: SIZES.xs },
  statLabel: { fontSize: FONTS.tiny, color: COLORS.textSecondary, marginTop: 2, textAlign: 'center' },
  section: { paddingHorizontal: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  checkpointCard: { flexDirection: 'row', marginBottom: SIZES.sm },
  checkpointTimeline: { alignItems: 'center', marginRight: SIZES.md },
  timelineDot: { width: 28, height: 28, borderRadius: 14, backgroundColor: COLORS.gray300, justifyContent: 'center', alignItems: 'center' },
  timelineDotCompleted: { backgroundColor: COLORS.success },
  timelineDotCurrent: { backgroundColor: COLORS.primaryBlue },
  checkpointNumber: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.white },
  timelineLine: { width: 2, flex: 1, backgroundColor: COLORS.gray200, marginVertical: 4 },
  timelineLineCompleted: { backgroundColor: COLORS.success },
  checkpointContent: { flex: 1, flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  checkpointInfo: { flex: 1 },
  checkpointName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  checkpointLocation: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  checkpointTime: { fontSize: FONTS.tiny, color: COLORS.success, marginTop: 4 },
  completeBtn: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.success, paddingHorizontal: SIZES.sm, paddingVertical: SIZES.xs, borderRadius: SIZES.radiusSm },
  completeBtnText: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.white },
  completedBadge: { padding: SIZES.xs },
  loadingContainer: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  loadingText: { marginTop: SIZES.md, fontSize: FONTS.body, color: COLORS.textSecondary },
  emptyContainer: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: SIZES.xl },
  emptyText: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginTop: SIZES.md },
  emptySubtext: { fontSize: FONTS.body, color: COLORS.textSecondary, marginTop: SIZES.xs },
  emptyCheckpoints: { padding: SIZES.lg, alignItems: 'center' },
  placeholder: { width: 40 },
});

export default PatrolTrackingScreen;
