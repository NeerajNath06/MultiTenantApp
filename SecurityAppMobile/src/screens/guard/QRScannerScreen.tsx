import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Alert, Dimensions } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { patrolScanService } from '../../services/patrolScanService';
import { authService } from '../../services/authService';
import { deploymentService } from '../../services/deploymentService';

const { width, height } = Dimensions.get('window');
const SCAN_SIZE = width * 0.7;

function QRScannerScreen({ navigation }: any) {
  const [isScanning, setIsScanning] = useState(true);
  const [flashOn, setFlashOn] = useState(false);
  const [lastScanned, setLastScanned] = useState<string | null>(null);
  const [recentScans, setRecentScans] = useState<{ id: string; location: string; time: string; status: string }[]>([]);
  const [guardId, setGuardId] = useState<string | null>(null);
  const [siteId, setSiteId] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      const user = await authService.getStoredUser();
      const gid = (user as { guardId?: string })?.guardId || user?.id;
      if (!gid) return;
      setGuardId(gid);
      const depRes = await deploymentService.getDeployments({ guardId: gid, pageSize: 5, skipCache: true });
      const list = depRes?.data?.items ?? depRes?.data ?? (Array.isArray(depRes?.data) ? depRes.data : []);
      const first = Array.isArray(list) ? list[0] : null;
      if (first) {
        const sid = (first as { siteId?: string }).siteId ?? (first as { SiteId?: string }).SiteId;
        if (sid) setSiteId(String(sid));
      }
      const result = await patrolScanService.getScans({ guardId: gid, pageSize: 20 });
      if (result.success && result.data?.items?.length) {
        setRecentScans(result.data.items.map((s: any) => ({
          id: String(s.id),
          location: s.locationName ?? s.siteName ?? 'Checkpoint',
          time: s.scannedAt ? new Date(s.scannedAt).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) : '—',
          status: s.status ?? 'success',
        })));
      }
    })();
  }, []);

  const loadRecentScans = async () => {
    if (!guardId) return;
    const result = await patrolScanService.getScans({ guardId, pageSize: 20 });
    if (result.success && result.data?.items?.length) {
      setRecentScans(result.data.items.map((s: any) => ({
        id: String(s.id),
        location: s.locationName ?? s.siteName ?? 'Checkpoint',
        time: s.scannedAt ? new Date(s.scannedAt).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) : '—',
        status: s.status ?? 'success',
      })));
    }
  };

  const handleBarCodeScanned = async (data: string) => {
    setIsScanning(false);
    setLastScanned(data);
    const locationName = data && data.length > 0 ? data.replace(/^CHK-\d+-/i, '').replace(/-/g, ' ') || 'Checkpoint' : 'Checkpoint';
    if (guardId && siteId) {
      try {
        const result = await patrolScanService.recordScan({
          guardId,
          siteId,
          locationName,
          checkpointCode: data || undefined,
        });
        if (result.success) {
          await loadRecentScans();
          Alert.alert(
            'Checkpoint Scanned',
            `Location: ${locationName}\nTime: ${new Date().toLocaleTimeString()}\nCode: ${data}`,
            [{ text: 'Continue', onPress: () => setIsScanning(true) }]
          );
          return;
        }
      } catch (_) {}
    }
    Alert.alert(
      'Checkpoint Scanned',
      `Location: ${locationName}\nTime: ${new Date().toLocaleTimeString()}\nCode: ${data}`,
      [{ text: 'Continue', onPress: () => setIsScanning(true) }]
    );
  };

  const simulateScan = () => {
    handleBarCodeScanned('CHK-001-MAIN-GATE');
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.white} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Scan Checkpoint</Text>
        <TouchableOpacity style={styles.flashBtn} onPress={() => setFlashOn(!flashOn)}>
          <MaterialCommunityIcons 
            name={flashOn ? 'flash' : 'flash-off'} 
            size={24} 
            color={COLORS.white} 
          />
        </TouchableOpacity>
      </View>

      {/* Scanner Area */}
      <View style={styles.scannerContainer}>
        <View style={styles.scannerFrame}>
          {/* Scanner Preview Placeholder */}
          <View style={styles.scannerPreview}>
            <MaterialCommunityIcons name="qrcode-scan" size={100} color={COLORS.white + '30'} />
            <Text style={styles.scannerText}>Point camera at QR code</Text>
          </View>

          {/* Corner Markers */}
          <View style={[styles.corner, styles.topLeft]} />
          <View style={[styles.corner, styles.topRight]} />
          <View style={[styles.corner, styles.bottomLeft]} />
          <View style={[styles.corner, styles.bottomRight]} />

          {/* Scanning Line Animation */}
          {isScanning && <View style={styles.scanLine} />}
        </View>

        {/* Manual Scan Button for Demo */}
        <TouchableOpacity style={styles.manualScanBtn} onPress={simulateScan}>
          <MaterialCommunityIcons name="qrcode" size={24} color={COLORS.white} />
          <Text style={styles.manualScanText}>Tap to Simulate Scan</Text>
        </TouchableOpacity>
      </View>

      {/* Recent Scans */}
      <View style={styles.recentSection}>
        <Text style={styles.sectionTitle}>Recent Scans</Text>
        {(recentScans.length ? recentScans : [{ id: '1', location: 'No scans yet', time: '—', status: 'success' }]).map((scan) => (
          <View key={String(scan.id)} style={styles.scanCard}>
            <View style={styles.scanIcon}>
              <MaterialCommunityIcons name="qrcode-scan" size={24} color={COLORS.success} />
            </View>
            <View style={styles.scanInfo}>
              <Text style={styles.scanLocation}>{scan.location}</Text>
              <Text style={styles.scanTime}>{scan.time}</Text>
            </View>
            <MaterialCommunityIcons name="check-circle" size={24} color={COLORS.success} />
          </View>
        ))}
      </View>

      {/* Instructions */}
      <View style={styles.instructions}>
        <View style={styles.instructionItem}>
          <MaterialCommunityIcons name="numeric-1-circle" size={24} color={COLORS.primaryBlue} />
          <Text style={styles.instructionText}>Point camera at checkpoint QR code</Text>
        </View>
        <View style={styles.instructionItem}>
          <MaterialCommunityIcons name="numeric-2-circle" size={24} color={COLORS.primaryBlue} />
          <Text style={styles.instructionText}>Hold steady until scan completes</Text>
        </View>
        <View style={styles.instructionItem}>
          <MaterialCommunityIcons name="numeric-3-circle" size={24} color={COLORS.primaryBlue} />
          <Text style={styles.instructionText}>Checkpoint will be recorded automatically</Text>
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.black },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md },
  backBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.white },
  flashBtn: { width: 40, height: 40, borderRadius: 20, backgroundColor: 'rgba(255,255,255,0.15)', justifyContent: 'center', alignItems: 'center' },
  scannerContainer: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  scannerFrame: { width: SCAN_SIZE, height: SCAN_SIZE, position: 'relative' },
  scannerPreview: { flex: 1, backgroundColor: 'rgba(255,255,255,0.1)', borderRadius: SIZES.radiusMd, justifyContent: 'center', alignItems: 'center' },
  scannerText: { color: COLORS.white, fontSize: FONTS.bodySmall, marginTop: SIZES.md, opacity: 0.7 },
  corner: { position: 'absolute', width: 30, height: 30, borderColor: COLORS.primaryBlue, borderWidth: 3 },
  topLeft: { top: -2, left: -2, borderRightWidth: 0, borderBottomWidth: 0, borderTopLeftRadius: 8 },
  topRight: { top: -2, right: -2, borderLeftWidth: 0, borderBottomWidth: 0, borderTopRightRadius: 8 },
  bottomLeft: { bottom: -2, left: -2, borderRightWidth: 0, borderTopWidth: 0, borderBottomLeftRadius: 8 },
  bottomRight: { bottom: -2, right: -2, borderLeftWidth: 0, borderTopWidth: 0, borderBottomRightRadius: 8 },
  scanLine: { position: 'absolute', top: '50%', left: 10, right: 10, height: 2, backgroundColor: COLORS.primaryBlue },
  manualScanBtn: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.primary, paddingHorizontal: SIZES.lg, paddingVertical: SIZES.md, borderRadius: SIZES.radiusFull, marginTop: SIZES.lg, gap: SIZES.sm },
  manualScanText: { color: COLORS.white, fontSize: FONTS.body, fontWeight: '600' },
  recentSection: { backgroundColor: COLORS.white, borderTopLeftRadius: 30, borderTopRightRadius: 30, padding: SIZES.md, paddingTop: SIZES.lg },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  scanCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.gray50, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm },
  scanIcon: { width: 44, height: 44, borderRadius: 22, backgroundColor: COLORS.success + '15', justifyContent: 'center', alignItems: 'center' },
  scanInfo: { flex: 1, marginLeft: SIZES.sm },
  scanLocation: { fontSize: FONTS.body, fontWeight: '500', color: COLORS.textPrimary },
  scanTime: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  instructions: { backgroundColor: COLORS.white, paddingHorizontal: SIZES.md, paddingBottom: SIZES.lg },
  instructionItem: { flexDirection: 'row', alignItems: 'center', marginBottom: SIZES.sm },
  instructionText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginLeft: SIZES.sm },
});

export default QRScannerScreen;
