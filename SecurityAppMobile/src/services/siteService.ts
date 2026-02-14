import BaseApiService from './baseApiService';

export interface Site {
  id: string;
  siteName: string;
  address?: string;
  latitude?: number;
  longitude?: number;
  geofenceRadiusMeters?: number;
  requiredGuards: number;
  siteType: string;
  clientId: string;
  clientName?: string;
  agencyId?: string;
  agencyName?: string;
}

class SiteService extends BaseApiService {
  async getSites(filters?: {
    clientId?: string;
    siteType?: string;
    pageNumber?: number;
    pageSize?: number;
    /** When set (e.g. for supervisor), returns only sites assigned to this supervisor. */
    supervisorId?: string;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/Sites', filters);
  }

  async getSiteById(id: string): Promise<{ success: boolean; data?: Site; error?: { code: string; message: string } }> {
    return await this.get<Site>(`/api/v1/Sites/${id}`);
  }

  async getSiteShifts(siteId: string): Promise<{ success: boolean; data?: any[]; error?: { code: string; message: string } }> {
    return await this.get<any[]>(`/api/v1/Sites/${siteId}/shifts`);
  }
}

export const siteService = new SiteService();

