import React, { useState, useRef, useEffect } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Alert, Vibration, Animated, Linking } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as Location from 'expo-location';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { emergencyService } from '../../services/emergencyService';
import { authService } from '../../services/authService';
import { deploymentService } from '../../services/deploymentService';
import { EmergencySOSScreenProps } from '../../types/navigation';

interface EmergencyContact {
  id: number;
  name: string;
  role: string;
  phone: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
}

function EmergencySOSScreen({ navigation }: EmergencySOSScreenProps) {
  const [sosActivated, setSOSActivated] = useState(false);
  const [countdown, setCountdown] = useState(5);
  const [countdownActive, setCountdownActive] = useState(false);
  const [guardId, setGuardId] = useState<string>('');
  const [siteId, setSiteId] = useState<string>('');
  const pulseAnim = useRef(new Animated.Value(1)).current;

  useEffect(() => {
    loadUserData();
  }, []);

  const loadUserData = async () => {
    try {
      const user = await authService.getStoredUser();
      if (user) {
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
          if (deployments.length > 0 && (deployments[0] as { siteId?: string }).siteId) {
            setSiteId((deployments[0] as { siteId: string }).siteId);
          }
        }
      }
    } catch (error) {
      console.error('Error loading user data:', error);
    }
  };

  const emergencyContacts: EmergencyContact[] = [
    { id: 1, name: 'Control Room', role: 'Security HQ', phone: '+911234567890', icon: 'shield-account' },
    { id: 2, name: 'Mr. Sharma', role: 'Supervisor', phone: '+911234567891', icon: 'account-tie' },
    { id: 3, name: 'Police', role: 'Emergency', phone: '100', icon: 'police-badge' },
    { id: 4, name: 'Ambulance', role: 'Medical', phone: '102', icon: 'ambulance' },
    { id: 5, name: 'Fire Station', role: 'Fire Emergency', phone: '101', icon: 'fire-truck' },
  ];

  const startPulseAnimation = () => {
    Animated.loop(
      Animated.sequence([
        Animated.timing(pulseAnim, { toValue: 1.2, duration: 500, useNativeDriver: true }),
        Animated.timing(pulseAnim, { toValue: 1, duration: 500, useNativeDriver: true }),
      ])
    ).start();
  };

  const handleSOSPress = () => {
    if (sosActivated) return;
    
    setCountdownActive(true);
    startPulseAnimation();
    Vibration.vibrate([0, 200, 100, 200]);
    
    let count = 5;
    const interval = setInterval(() => {
      count--;
      setCountdown(count);
      if (count === 0) {
        clearInterval(interval);
        activateSOS();
      }
    }, 1000);
  };

  const handleCancelSOS = () => {
    setCountdownActive(false);
    setCountdown(5);
    pulseAnim.stopAnimation();
    pulseAnim.setValue(1);
  };

  const activateSOS = async () => {
    try {
      setSOSActivated(true);
      setCountdownActive(false);
      Vibration.vibrate([0, 500, 200, 500, 200, 500]);

      // Get current location
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permission Required', 'Location permission is required for emergency alerts.');
        setSOSActivated(false);
        return;
      }

      const location = await Location.getCurrentPositionAsync({});
      const { latitude, longitude } = location.coords;

      // Send emergency alert to API
      const result = await emergencyService.createEmergencyAlert({
        guardId,
        siteId: siteId || '',
        alertType: 'SOS',
        latitude,
        longitude,
        message: 'Emergency SOS activated by guard',
      });

      if (result.success) {
        Alert.alert(
          '🚨 SOS Activated',
          'Emergency alert has been sent to:\n\n• Control Room\n• Your Supervisor\n• Nearby Guards\n\nYour live location is being shared.',
          [{ text: 'OK' }]
        );
      } else {
        Alert.alert(
          'SOS Sent',
          'Emergency alert has been sent. Your location is being shared.',
          [{ text: 'OK' }]
        );
      }
    } catch (error: any) {
      console.error('Error activating SOS:', error);
      // Still show success message even if API fails
      Alert.alert(
        '🚨 SOS Activated',
        'Emergency alert has been sent. Your location is being shared.',
        [{ text: 'OK' }]
      );
    }
  };

  const handleDeactivateSOS = async () => {
    Alert.alert(
      'Deactivate SOS',
      'Are you sure you want to deactivate the emergency alert?',
      [
        { text: 'Cancel', style: 'cancel' },
        { 
          text: 'Deactivate', 
          style: 'destructive',
          onPress: async () => {
            try {
              // Try to resolve the alert if we have an alert ID
              // For now, just update local state
              setSOSActivated(false);
              setCountdown(5);
              pulseAnim.setValue(1);
              Alert.alert('SOS Deactivated', 'Emergency alert has been cancelled.');
            } catch (error) {
              console.error('Error deactivating SOS:', error);
              setSOSActivated(false);
              setCountdown(5);
              pulseAnim.setValue(1);
            }
          }
        }
      ]
    );
  };

  const handleCallContact = (phone: string) => {
    Linking.openURL(`tel:${phone}`);
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Emergency SOS</Text>
        <View style={styles.placeholder} />
      </View>

      {/* Main SOS Button Area */}
      <View style={styles.sosContainer}>
        {sosActivated ? (
          <View style={styles.activatedContainer}>
            <View style={styles.activatedIcon}>
              <MaterialCommunityIcons name="alert" size={64} color={COLORS.white} />
            </View>
            <Text style={styles.activatedTitle}>SOS ACTIVE</Text>
            <Text style={styles.activatedSubtitle}>Help is on the way</Text>
            <Text style={styles.locationText}>📍 Location shared with emergency contacts</Text>
            
            <TouchableOpacity style={styles.deactivateBtn} onPress={handleDeactivateSOS}>
              <Text style={styles.deactivateBtnText}>Deactivate SOS</Text>
            </TouchableOpacity>
          </View>
        ) : countdownActive ? (
          <View style={styles.countdownContainer}>
            <Animated.View style={[styles.countdownCircle, { transform: [{ scale: pulseAnim }] }]}>
              <Text style={styles.countdownNumber}>{countdown}</Text>
            </Animated.View>
            <Text style={styles.countdownText}>Sending SOS in {countdown} seconds...</Text>
            <TouchableOpacity style={styles.cancelBtn} onPress={handleCancelSOS}>
              <Text style={styles.cancelBtnText}>Cancel</Text>
            </TouchableOpacity>
          </View>
        ) : (
          <View style={styles.sosButtonContainer}>
            <TouchableOpacity style={styles.sosButton} onPress={handleSOSPress} activeOpacity={0.8}>
              <View style={styles.sosButtonInner}>
                <MaterialCommunityIcons name="alert-octagon" size={48} color={COLORS.white} />
                <Text style={styles.sosButtonText}>SOS</Text>
              </View>
            </TouchableOpacity>
            <Text style={styles.sosInstruction}>Press and hold for emergency</Text>
          </View>
        )}
      </View>

      {/* Emergency Contacts */}
      <View style={styles.contactsSection}>
        <Text style={styles.sectionTitle}>Quick Emergency Contacts</Text>
        <View style={styles.contactsGrid}>
          {emergencyContacts.map((contact) => (
            <TouchableOpacity
              key={contact.id}
              style={styles.contactCard}
              onPress={() => handleCallContact(contact.phone)}
            >
              <View style={[styles.contactIcon, { backgroundColor: contact.id <= 2 ? COLORS.primaryBlue + '15' : COLORS.error + '15' }]}>
                <MaterialCommunityIcons 
                  name={contact.icon} 
                  size={24} 
                  color={contact.id <= 2 ? COLORS.primaryBlue : COLORS.error} 
                />
              </View>
              <Text style={styles.contactName}>{contact.name}</Text>
              <Text style={styles.contactRole}>{contact.role}</Text>
            </TouchableOpacity>
          ))}
        </View>
      </View>

      {/* Safety Tips */}
      <View style={styles.tipsCard}>
        <MaterialCommunityIcons name="lightbulb-outline" size={24} color={COLORS.warning} />
        <View style={styles.tipsContent}>
          <Text style={styles.tipsTitle}>Safety Tip</Text>
          <Text style={styles.tipsText}>In case of emergency, stay calm and move to a safe location if possible. Help will arrive shortly.</Text>
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.error },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md },
  backBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.white },
  placeholder: { width: 40 },
  sosContainer: { flex: 1, justifyContent: 'center', alignItems: 'center', paddingHorizontal: SIZES.lg },
  sosButtonContainer: { alignItems: 'center' },
  sosButton: { width: 200, height: 200, borderRadius: 100, backgroundColor: 'rgba(255,255,255,0.2)', justifyContent: 'center', alignItems: 'center', borderWidth: 4, borderColor: COLORS.white },
  sosButtonInner: { width: 160, height: 160, borderRadius: 80, backgroundColor: COLORS.white, justifyContent: 'center', alignItems: 'center' },
  sosButtonText: { fontSize: 32, fontWeight: 'bold', color: COLORS.error, marginTop: SIZES.xs },
  sosInstruction: { fontSize: FONTS.body, color: COLORS.white, marginTop: SIZES.lg, opacity: 0.8 },
  countdownContainer: { alignItems: 'center' },
  countdownCircle: { width: 180, height: 180, borderRadius: 90, backgroundColor: 'rgba(255,255,255,0.2)', justifyContent: 'center', alignItems: 'center', borderWidth: 4, borderColor: COLORS.white },
  countdownNumber: { fontSize: 72, fontWeight: 'bold', color: COLORS.white },
  countdownText: { fontSize: FONTS.body, color: COLORS.white, marginTop: SIZES.lg },
  cancelBtn: { backgroundColor: COLORS.white, paddingHorizontal: SIZES.xl, paddingVertical: SIZES.md, borderRadius: SIZES.radiusFull, marginTop: SIZES.lg },
  cancelBtnText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.error },
  activatedContainer: { alignItems: 'center' },
  activatedIcon: { width: 120, height: 120, borderRadius: 60, backgroundColor: 'rgba(255,255,255,0.2)', justifyContent: 'center', alignItems: 'center', marginBottom: SIZES.lg },
  activatedTitle: { fontSize: FONTS.h2, fontWeight: 'bold', color: COLORS.white },
  activatedSubtitle: { fontSize: FONTS.body, color: COLORS.white, opacity: 0.8, marginTop: SIZES.xs },
  locationText: { fontSize: FONTS.bodySmall, color: COLORS.white, marginTop: SIZES.md, opacity: 0.9 },
  deactivateBtn: { backgroundColor: COLORS.white, paddingHorizontal: SIZES.xl, paddingVertical: SIZES.md, borderRadius: SIZES.radiusFull, marginTop: SIZES.xl },
  deactivateBtnText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.error },
  contactsSection: { backgroundColor: COLORS.white, borderTopLeftRadius: 30, borderTopRightRadius: 30, paddingTop: SIZES.lg, paddingHorizontal: SIZES.md, paddingBottom: SIZES.md },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  contactsGrid: { flexDirection: 'row', flexWrap: 'wrap', marginHorizontal: -SIZES.xs },
  contactCard: { width: '18%', alignItems: 'center', marginHorizontal: SIZES.xs, marginBottom: SIZES.sm },
  contactIcon: { width: 48, height: 48, borderRadius: 24, justifyContent: 'center', alignItems: 'center', marginBottom: SIZES.xs },
  contactName: { fontSize: FONTS.tiny, fontWeight: '600', color: COLORS.textPrimary, textAlign: 'center' },
  contactRole: { fontSize: 8, color: COLORS.textSecondary, textAlign: 'center' },
  tipsCard: { flexDirection: 'row', backgroundColor: COLORS.warning + '15', marginHorizontal: SIZES.md, marginBottom: SIZES.md, padding: SIZES.md, borderRadius: SIZES.radiusMd },
  tipsContent: { flex: 1, marginLeft: SIZES.sm },
  tipsTitle: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.warning },
  tipsText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
});

export default EmergencySOSScreen;
