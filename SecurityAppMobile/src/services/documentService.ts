import AsyncStorage from '@react-native-async-storage/async-storage';
import * as FileSystem from 'expo-file-system/legacy';
import { getBaseUrl } from '../config/api.config';
import BaseApiService from './baseApiService';

export interface GuardDocumentItem {
  id: string;
  guardId: string;
  documentType: string;
  documentNumber?: string;
  fileName: string;
  expiryDate?: string;
  isVerified: boolean;
  createdDate: string;
}

class DocumentService extends BaseApiService {
  async getDocumentList(guardId: string): Promise<{ success: boolean; data?: GuardDocumentItem[]; error?: { code: string; message: string } }> {
    const result = await this.get<unknown>('/api/v1/GuardDocuments', { guardId });
    if (!result.success) return { success: false, error: result.error };
    const raw = result.data;
    if (Array.isArray(raw)) return { success: true, data: raw.map(normalizeDoc) };
    const list = (raw as any)?.items ?? (raw as any)?.data ?? raw;
    const arr = Array.isArray(list) ? list : [];
    return { success: true, data: arr.map(normalizeDoc) };
  }

  async uploadDocument(
    guardId: string,
    documentType: string,
    fileUri: string,
    fileName: string,
    options?: { documentNumber?: string; expiryDate?: string }
  ): Promise<{ success: boolean; data?: { id?: string }; error?: { code: string; message: string } }> {
    const token = await AsyncStorage.getItem('authToken');
    const userData = await AsyncStorage.getItem('userData');
    const parsed = userData ? JSON.parse(userData) : {};
    const agencyId = parsed.agencyId;

    const formData = new FormData();
    formData.append('guardId', guardId);
    formData.append('documentType', documentType);
    if (options?.documentNumber) formData.append('documentNumber', options.documentNumber);
    if (options?.expiryDate) formData.append('expiryDate', options.expiryDate);

    const file = {
      uri: fileUri,
      name: fileName || 'document',
      type: getMimeType(fileName),
    } as any;
    formData.append('file', file);

    const headers: HeadersInit = {
      Authorization: token ? `Bearer ${token}` : '',
    };
    if (agencyId) {
      headers['X-Agency-Id'] = agencyId;
      headers['X-Tenant-Id'] = agencyId;
    }

    try {
      const response = await fetch(`${getBaseUrl()}/api/v1/GuardDocuments`, {
        method: 'POST',
        headers,
        body: formData,
      });
      const text = await response.text();
      let data: any = null;
      try {
        data = text ? JSON.parse(text) : null;
      } catch (_) {}
      if (response.ok) {
        const id = data?.data ?? data?.Data ?? data?.id ?? data?.Id;
        return { success: true, data: id != null ? { id: String(id) } : undefined };
      }
      return {
        success: false,
        error: { code: 'API_ERROR', message: (data?.message) || text || `Upload failed (${response.status})` },
      };
    } catch (error) {
      console.error('Upload document error:', error);
      return { success: false, error: { code: 'NETWORK_ERROR', message: 'Network error. Please check your connection.' } };
    }
  }

  async getDownloadUrl(documentId: string): Promise<string> {
    return `${getBaseUrl()}/api/v1/GuardDocuments/${documentId}/download`;
  }

  async downloadDocument(documentId: string, fileName: string): Promise<{ success: boolean; localUri?: string; error?: string }> {
    const token = await AsyncStorage.getItem('authToken');
    const userData = await AsyncStorage.getItem('userData');
    const parsed = userData ? JSON.parse(userData) : {};
    const agencyId = parsed.agencyId;
    const headers: Record<string, string> = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    if (agencyId) {
      headers['X-Agency-Id'] = agencyId;
      headers['X-Tenant-Id'] = agencyId;
    }
    try {
      const url = await this.getDownloadUrl(documentId);
      const name = fileName || `${documentId}.bin`;
      const path = `${FileSystem.cacheDirectory ?? ''}${name}`;
      const result = await FileSystem.downloadAsync(url, path, { headers });
      if (result.status !== 200) return { success: false, error: `Download failed (${result.status})` };
      return { success: true, localUri: result.uri };
    } catch (e) {
      return { success: false, error: (e as Error).message };
    }
  }
}

function normalizeDoc(d: any): GuardDocumentItem {
  return {
    id: d.id ?? d.Id,
    guardId: d.guardId ?? d.GuardId,
    documentType: d.documentType ?? d.DocumentType ?? '',
    documentNumber: d.documentNumber ?? d.DocumentNumber,
    fileName: d.fileName ?? d.FileName ?? 'document',
    expiryDate: d.expiryDate ?? d.ExpiryDate,
    isVerified: d.isVerified ?? d.IsVerified ?? false,
    createdDate: d.createdDate ?? d.CreatedDate ?? '',
  };
}

function getMimeType(fileName: string): string {
  const ext = (fileName || '').split('.').pop()?.toLowerCase();
  switch (ext) {
    case 'pdf': return 'application/pdf';
    case 'jpg':
    case 'jpeg': return 'image/jpeg';
    case 'png': return 'image/png';
    case 'gif': return 'image/gif';
    default: return 'application/octet-stream';
  }
}

export const documentService = new DocumentService();
