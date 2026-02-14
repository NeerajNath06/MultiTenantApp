import BaseApiService from './baseApiService';

export interface EmergencyAlert {
  id: string;
  guardId: string;
  guardName?: string;
  siteId: string;
  siteName?: string;
  alertType: string;
  latitude: number;
  longitude: number;
  message?: string;
  status: string;
  acknowledgedAt?: string;
  resolvedAt?: string;
  createdAt: string;
}

class EmergencyService extends BaseApiService {
  async getEmergencyAlerts(filters?: {
    guardId?: string;
    siteId?: string;
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/Emergency/alerts', filters);
  }

  async getEmergencyAlertById(id: string): Promise<{ success: boolean; data?: EmergencyAlert; error?: { code: string; message: string } }> {
    return await this.get<EmergencyAlert>(`/api/v1/Emergency/alerts/${id}`);
  }

  async createEmergencyAlert(alert: {
    guardId: string;
    siteId: string;
    alertType: string;
    latitude: number;
    longitude: number;
    message?: string;
    agencyId?: string;
  }): Promise<{ success: boolean; data?: EmergencyAlert; error?: { code: string; message: string } }> {
    return await this.post<EmergencyAlert>('/api/v1/Emergency/alerts', alert);
  }

  async acknowledgeAlert(id: string): Promise<{ success: boolean; data?: EmergencyAlert; error?: { code: string; message: string } }> {
    return await this.patch<EmergencyAlert>(`/api/v1/Emergency/alerts/${id}/acknowledge`, {});
  }

  async resolveAlert(id: string, resolutionNotes?: string): Promise<{ success: boolean; data?: EmergencyAlert; error?: { code: string; message: string } }> {
    return await this.patch<EmergencyAlert>(`/api/v1/Emergency/alerts/${id}/resolve`, { resolutionNotes });
  }
}

export const emergencyService = new EmergencyService();
