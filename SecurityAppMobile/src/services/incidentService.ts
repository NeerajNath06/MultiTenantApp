import * as FileSystem from 'expo-file-system/legacy';
import { getBaseUrl } from '../config/api.config';
import BaseApiService from './baseApiService';

export interface Incident {
  id: string;
  incidentType: string;
  description: string;
  reportedAt: string;
  status: string;
  siteId: string;
  siteName?: string;
  guardId?: string;
  guardName?: string;
  resolutionNotes?: string;
  resolvedAt?: string;
  agencyId?: string;
  agencyName?: string;
}

export interface IncidentEvidence {
  id: string;
  incidentId: string;
  fileBase64: string;
  fileName: string;
  fileType: string;
  description?: string;
}

class IncidentService extends BaseApiService {

  async getIncidents(filters?: {
    status?: string;
    siteId?: string;
    guardId?: string;
    dateFrom?: string;
    dateTo?: string;
    startDate?: string;
    endDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    const params: Record<string, string | number> = { ...(filters as Record<string, string | number>) };
    if (filters?.startDate) params.startDate = filters.startDate;
    if (filters?.endDate) params.endDate = filters.endDate;
    if (filters?.dateFrom && !params.startDate) params.startDate = filters.dateFrom;
    if (filters?.dateTo && !params.endDate) params.endDate = filters.dateTo;
    return await this.get('/api/v1/Incidents', params);
  }

  async getIncidentById(id: string): Promise<{ success: boolean; data?: Incident; error?: { code: string; message: string } }> {
    return await this.get<Incident>(`/api/v1/Incidents/${id}`);
  }

  async createIncident(incident: {
    incidentType: string;
    description: string;
    siteId: string;
    guardId?: string;
    agencyId?: string;
  }): Promise<{ success: boolean; data?: Incident; error?: { code: string; message: string } }> {
    return await this.post<Incident>('/api/v1/Incidents', incident);
  }

  async updateIncident(id: string, incident: Partial<Incident>): Promise<{ success: boolean; data?: Incident; error?: { code: string; message: string } }> {
    return await this.put<Incident>(`/api/v1/Incidents/${id}`, incident);
  }

  async updateIncidentStatus(id: string, status: string): Promise<{ success: boolean; data?: Incident; error?: { code: string; message: string } }> {
    return await this.patch<Incident>(`/api/v1/Incidents/${id}/status`, { status });
  }

  async getIncidentEvidence(incidentId: string): Promise<{ success: boolean; data?: IncidentEvidence[]; error?: { code: string; message: string } }> {
    return await this.get<IncidentEvidence[]>(`/api/v1/Incidents/${incidentId}/evidence`);
  }

  async addIncidentEvidence(incidentId: string, evidence: {
    fileBase64: string;
    fileName: string;
    fileType: string;
    description?: string;
  }): Promise<{ success: boolean; data?: IncidentEvidence; error?: { code: string; message: string } }> {
    return await this.post<IncidentEvidence>(`/api/v1/Incidents/${incidentId}/evidence`, evidence);
  }

  /**
   * Export incidents as CSV, Excel, or PDF. Uses same filters as getIncidents.
   * format: 'csv' | 'xlsx' | 'pdf'. Returns local URI for sharing/saving.
   */
  async exportIncidents(
    format: 'csv' | 'xlsx' | 'pdf' = 'csv',
    filters?: {
      status?: string;
      siteId?: string;
      guardId?: string;
      startDate?: string;
      endDate?: string;
      search?: string;
      sortBy?: string;
      sortDirection?: string;
    }
  ): Promise<{ success: boolean; localUri?: string; fileName?: string; mimeType?: string; error?: string }> {
    const params = new URLSearchParams({ format });
    if (filters) {
      if (filters.status) params.set('status', filters.status);
      if (filters.siteId) params.set('siteId', filters.siteId);
      if (filters.guardId) params.set('guardId', filters.guardId);
      if (filters.startDate) params.set('startDate', filters.startDate);
      if (filters.endDate) params.set('endDate', filters.endDate);
      if (filters.search) params.set('search', filters.search);
      if (filters.sortBy) params.set('sortBy', filters.sortBy);
      if (filters.sortDirection) params.set('sortDirection', filters.sortDirection);
    }
    const ext = format === 'pdf' ? 'pdf' : format === 'xlsx' ? 'xlsx' : 'csv';
    const url = `${getBaseUrl()}/api/v1/Incidents/export?${params.toString()}`;
    const headers = await this.getAuthHeaders();
    const fileName = `Incidents_${new Date().toISOString().slice(0, 10)}.${ext}`;
    const path = `${FileSystem.cacheDirectory ?? ''}${fileName}`;
    const mimeType = format === 'pdf' ? 'application/pdf' : format === 'xlsx' ? 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' : 'text/csv';
    try {
      const result = await FileSystem.downloadAsync(url, path, { headers });
      if (result.status !== 200)
        return { success: false, error: `Export failed (${result.status})` };
      return { success: true, localUri: result.uri, fileName, mimeType };
    } catch (e) {
      return { success: false, error: (e as Error).message };
    }
  }
}

export const incidentService = new IncidentService();

