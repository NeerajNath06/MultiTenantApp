import React, { useState, useEffect, useCallback } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert, RefreshControl, Share, Linking, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Card from '../../components/common/Card';
import { authService } from '../../services/authService';
import { documentService, GuardDocumentItem } from '../../services/documentService';

function DocumentsScreen({ navigation }: any) {
  const [refreshing, setRefreshing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [guardId, setGuardId] = useState<string | null>(null);
  const [documents, setDocuments] = useState<GuardDocumentItem[]>([]);

  const loadDocuments = useCallback(async () => {
    const user = await authService.getStoredUser();
    const gid = (user as { guardId?: string })?.guardId;
    setGuardId(gid ?? null);
    if (!gid) {
      setDocuments([]);
      setLoading(false);
      setRefreshing(false);
      return;
    }
    const result = await documentService.getDocumentList(gid);
    setDocuments(result.data ?? []);
    setLoading(false);
    setRefreshing(false);
  }, []);

  useEffect(() => {
    loadDocuments();
  }, [loadDocuments]);

  const onRefresh = () => {
    setRefreshing(true);
    loadDocuments();
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'verified': return COLORS.success;
      case 'pending': return COLORS.warning;
      case 'expired': return COLORS.error;
      case 'rejected': return COLORS.error;
      default: return COLORS.gray500;
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'verified': return 'Verified';
      case 'pending': return 'Pending Review';
      case 'expired': return 'Expired';
      case 'rejected': return 'Rejected';
      default: return status;
    }
  };

  const getDocStatus = (doc: GuardDocumentItem): 'verified' | 'pending' | 'expired' | 'rejected' => {
    if (doc.isVerified) return 'verified';
    if (doc.expiryDate && new Date(doc.expiryDate) < new Date()) return 'expired';
    return 'pending';
  };

  const getDocIcon = (doc: GuardDocumentItem): keyof typeof MaterialCommunityIcons.glyphMap => {
    const t = (doc.documentType || '').toLowerCase();
    if (t.includes('license')) return 'card-account-details';
    if (t.includes('id') || t.includes('proof')) return 'card-account-details-outline';
    if (t.includes('certificate') || t.includes('training')) return 'certificate';
    if (t.includes('medical')) return 'medical-bag';
    return 'file-document';
  };

  const handleDocumentPress = (document: GuardDocumentItem) => {
    const status = getDocStatus(document);
    const uploadDate = document.createdDate ? new Date(document.createdDate).toISOString().split('T')[0] : '—';
    const actions: Array<{ text: string; style?: 'cancel' | 'default'; onPress?: () => void }> = [{ text: 'Close', style: 'cancel' }];
    actions.push({ text: 'Download / View', style: 'default', onPress: () => handleViewDocument(document) });
    actions.push({ text: 'Share', style: 'default', onPress: () => handleShareDocument(document) });
    Alert.alert(
      document.fileName || document.documentType,
      `Status: ${getStatusText(status)}\nUploaded: ${uploadDate}${document.expiryDate ? `\nExpiry: ${document.expiryDate}` : ''}`,
      actions
    );
  };

  const handleViewDocument = async (document: GuardDocumentItem) => {
    const result = await documentService.downloadDocument(document.id, document.fileName || `${document.id}.bin`);
    if (!result.success) {
      Alert.alert('Error', result.error || 'Download failed');
      return;
    }
    if (result.localUri) {
      try {
        const canOpen = await Linking.canOpenURL(result.localUri);
        if (canOpen) await Linking.openURL(result.localUri);
        else await Share.share({ url: result.localUri, title: document.fileName });
      } catch (e) {
        await Share.share({ url: result.localUri, title: document.fileName });
      }
    }
  };

  const handleShareDocument = async (document: GuardDocumentItem) => {
    const result = await documentService.downloadDocument(document.id, document.fileName || `${document.id}.bin`);
    if (!result.success) {
      Alert.alert('Error', result.error || 'Download failed');
      return;
    }
    try {
      await Share.share({ url: result.localUri!, title: document.fileName || document.documentType });
    } catch (error) {
      Alert.alert('Error', 'Unable to share document');
    }
  };

  const documentTypes = [
    { id: 'License', label: 'Security License' },
    { id: 'ID Proof', label: 'ID Proof' },
    { id: 'Certificate', label: 'Certificate' },
    { id: 'Medical', label: 'Medical Certificate' },
  ];

  const handleUploadDocument = async (type?: string) => {
    if (!guardId) {
      Alert.alert('Not Available', 'Document upload is available only for guards with an assigned guard profile.');
      return;
    }
    const pickImage = async (docType: string) => {
      const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permission Denied', 'Gallery permission is required to upload documents.');
        return;
      }
      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ['images'],
        allowsEditing: true,
        quality: 0.8,
      });
      if (result.canceled) return;
      setUploading(true);
      const uri = result.assets[0].uri;
      const fileName = uri.split('/').pop() || `document_${Date.now()}.jpg`;
      const uploadResult = await documentService.uploadDocument(guardId, docType, uri, fileName);
      setUploading(false);
      if (uploadResult.success) {
        Alert.alert('Success', 'Document uploaded successfully.');
        loadDocuments();
      } else {
        Alert.alert('Upload Failed', uploadResult.error?.message || 'Please try again.');
      }
    };

    if (type) {
      pickImage(type);
    } else {
      Alert.alert(
        'Upload Document',
        'Select document type to upload',
        [{ text: 'Cancel', style: 'cancel' }, ...documentTypes.map(dt => ({ text: dt.label, onPress: () => pickImage(dt.id) }))]
      );
    }
  };

  const handleDownloadAll = async () => {
    const list = documents.filter(d => d.isVerified);
    if (list.length === 0) {
      Alert.alert('No Documents', 'No verified documents available for download.');
      return;
    }
    for (const doc of list) {
      const result = await documentService.downloadDocument(doc.id, doc.fileName || `${doc.id}.bin`);
      if (result.success && result.localUri) {
        try { await Share.share({ url: result.localUri, title: doc.fileName }); } catch (_) {}
      }
    }
  };

  const verifiedCount = documents.filter(d => d.isVerified).length;
  const pendingCount = documents.filter(d => !d.isVerified).length;
  const expiredCount = documents.filter(d => d.expiryDate && new Date(d.expiryDate) < new Date()).length;

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>My Documents</Text>
        <TouchableOpacity style={styles.uploadButton} onPress={() => handleUploadDocument()}>
          <MaterialCommunityIcons name="plus" size={24} color={COLORS.primary} />
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[COLORS.primary]} />}
        contentContainerStyle={styles.content}
      >
        {/* Stats Summary */}
        <View style={styles.statsContainer}>
          <Card style={[styles.statCard, { backgroundColor: COLORS.success + '10' }]}>
            <MaterialCommunityIcons name="check-circle" size={24} color={COLORS.success} />
            <Text style={[styles.statNumber, { color: COLORS.success }]}>{verifiedCount}</Text>
            <Text style={styles.statLabel}>Verified</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.warning + '10' }]}>
            <MaterialCommunityIcons name="clock" size={24} color={COLORS.warning} />
            <Text style={[styles.statNumber, { color: COLORS.warning }]}>{pendingCount}</Text>
            <Text style={styles.statLabel}>Pending</Text>
          </Card>
          <Card style={[styles.statCard, { backgroundColor: COLORS.error + '10' }]}>
            <MaterialCommunityIcons name="alert-circle" size={24} color={COLORS.error} />
            <Text style={[styles.statNumber, { color: COLORS.error }]}>{expiredCount}</Text>
            <Text style={styles.statLabel}>Action Needed</Text>
          </Card>
        </View>

        {/* Quick Actions */}
        <Card style={styles.quickActionsCard}>
          <Text style={styles.quickActionsTitle}>Quick Actions</Text>
          <View style={styles.actionsGrid}>
            <TouchableOpacity style={styles.actionCard} onPress={() => handleUploadDocument()}>
              <MaterialCommunityIcons name="upload" size={24} color={COLORS.primary} />
              <Text style={styles.actionLabel}>Upload</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={handleDownloadAll}>
              <MaterialCommunityIcons name="download" size={24} color={COLORS.secondary} />
              <Text style={styles.actionLabel}>Download</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={() => Alert.alert('Share All', 'Select documents to share from the list below.')}>
              <MaterialCommunityIcons name="share-variant" size={24} color={COLORS.warning} />
              <Text style={styles.actionLabel}>Share</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={() => navigation.navigate('Support')}>
              <MaterialCommunityIcons name="help-circle" size={24} color={COLORS.info} />
              <Text style={styles.actionLabel}>Help</Text>
            </TouchableOpacity>
          </View>
        </Card>

        {/* Documents List */}
        <Text style={styles.sectionTitle}>All Documents</Text>
        {!guardId ? (
          <Card style={styles.emptyCard}>
            <MaterialCommunityIcons name="file-document-outline" size={48} color={COLORS.gray400} />
            <Text style={styles.emptyText}>Document upload is available only for guards with an assigned profile.</Text>
          </Card>
        ) : documents.length === 0 ? (
          <Card style={styles.emptyCard}>
            <MaterialCommunityIcons name="file-document-outline" size={48} color={COLORS.gray400} />
            <Text style={styles.emptyText}>No documents yet. Tap + to upload.</Text>
          </Card>
        ) : (
          documents.map((document) => {
            const status = getDocStatus(document);
            return (
              <TouchableOpacity
                key={document.id}
                style={styles.documentCard}
                onPress={() => handleDocumentPress(document)}
              >
                <View style={[styles.documentIcon, { backgroundColor: getStatusColor(status) + '15' }]}>
                  <MaterialCommunityIcons name={getDocIcon(document)} size={28} color={getStatusColor(status)} />
                </View>
                <View style={styles.documentContent}>
                  <Text style={styles.documentName}>{document.fileName || document.documentType || 'Document'}</Text>
                  <View style={styles.documentMeta}>
                    <View style={[styles.statusBadge, { backgroundColor: getStatusColor(status) + '15' }]}>
                      <View style={[styles.statusDot, { backgroundColor: getStatusColor(status) }]} />
                      <Text style={[styles.statusText, { color: getStatusColor(status) }]}>{getStatusText(status)}</Text>
                    </View>
                  </View>
                  {document.expiryDate && (
                    <Text style={[styles.expiryText, status === 'expired' && styles.expiryTextExpired]}>
                      {status === 'expired' ? 'Expired: ' : 'Expires: '}{document.expiryDate}
                    </Text>
                  )}
                </View>
                <MaterialCommunityIcons name="chevron-right" size={24} color={COLORS.gray400} />
              </TouchableOpacity>
            );
          })
        )}

        {/* Info Card */}
        <Card style={styles.infoCard}>
          <MaterialCommunityIcons name="information" size={24} color={COLORS.info} />
          <View style={styles.infoContent}>
            <Text style={styles.infoTitle}>Document Guidelines</Text>
            <Text style={styles.infoText}>
              • Upload clear, readable copies{'\n'}
              • Supported formats: JPG, PNG, PDF{'\n'}
              • Maximum file size: 5MB{'\n'}
              • Documents are reviewed within 24-48 hours
            </Text>
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
  uploadButton: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center' },
  content: { padding: SIZES.md },
  statsContainer: { flexDirection: 'row', justifyContent: 'space-between', marginBottom: SIZES.md },
  statCard: { flex: 1, alignItems: 'center', padding: SIZES.md, marginHorizontal: SIZES.xs },
  statNumber: { fontSize: FONTS.h3, fontWeight: 'bold', marginTop: SIZES.xs },
  statLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  quickActionsCard: { marginBottom: SIZES.md, padding: SIZES.md },
  quickActionsTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  actionsGrid: { flexDirection: 'row', justifyContent: 'space-around' },
  actionCard: { alignItems: 'center', padding: SIZES.sm },
  actionLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  sectionTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.md },
  documentCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.sm, ...SHADOWS.small },
  documentIcon: { width: 56, height: 56, borderRadius: 28, justifyContent: 'center', alignItems: 'center', marginRight: SIZES.md },
  documentContent: { flex: 1 },
  documentName: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.xs },
  documentMeta: { flexDirection: 'row', alignItems: 'center' },
  statusBadge: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusFull, marginRight: SIZES.sm },
  statusDot: { width: 6, height: 6, borderRadius: 3, marginRight: 4 },
  statusText: { fontSize: FONTS.caption, fontWeight: '500' },
  documentSize: { fontSize: FONTS.caption, color: COLORS.gray400 },
  expiryText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: SIZES.xs },
  expiryTextExpired: { color: COLORS.error },
  infoCard: { flexDirection: 'row', backgroundColor: COLORS.info + '10', padding: SIZES.md, marginTop: SIZES.md, marginBottom: SIZES.xxl },
  infoContent: { flex: 1, marginLeft: SIZES.sm },
  infoTitle: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.info, marginBottom: SIZES.xs },
  infoText: { fontSize: FONTS.caption, color: COLORS.textSecondary, lineHeight: 18 },
  loadingWrap: { flex: 1, justifyContent: 'center', alignItems: 'center', gap: SIZES.sm },
  loadingText: { fontSize: FONTS.body, color: COLORS.textSecondary },
  emptyCard: { alignItems: 'center', padding: SIZES.xl },
  emptyText: { fontSize: FONTS.body, color: COLORS.textSecondary, marginTop: SIZES.sm, textAlign: 'center' },
});

export default DocumentsScreen;
