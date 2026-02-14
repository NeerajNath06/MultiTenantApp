import BaseApiService from './baseApiService';

export interface Visitor {
  id: string;
  visitorName: string;
  visitorType: string;
  companyName?: string;
  phoneNumber: string;
  email?: string;
  purpose: string;
  entryTime: string;
  exitTime?: string;
  siteId: string;
  siteName?: string;
  guardId: string;
  guardName?: string;
  idProofType?: string;
  idProofNumber?: string;
}

export interface VisitorAnalytics {
  totalVisitors: number;
  currentlyInside: number;
  avgDurationMinutes: number;
  peakHour: number;
  byPurpose: { purpose: string; count: number; percentage: number }[];
  byHour: { hour: number; hourLabel: string; count: number }[];
  topHosts: { hostName: string; visitorCount: number }[];
}

class VisitorService extends BaseApiService {
  async getVisitors(filters?: {
    siteId?: string;
    guardId?: string;
    dateFrom?: string;
    dateTo?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/Visitors', filters);
  }

  async getVisitorById(id: string): Promise<{ success: boolean; data?: Visitor; error?: { code: string; message: string } }> {
    return await this.get<Visitor>(`/api/v1/Visitors/${id}`);
  }

  async registerVisitor(visitor: {
    visitorName: string;
    visitorType?: string;
    companyName?: string;
    phoneNumber: string;
    email?: string;
    purpose: string;
    hostName?: string;
    hostDepartment?: string;
    siteId: string;
    guardId: string;
    idProofType?: string;
    idProofNumber?: string;
    agencyId?: string;
  }): Promise<{ success: boolean; data?: { id: string; badgeNumber?: string }; error?: { code: string; message: string } }> {
    return await this.post<{ id: string; badgeNumber?: string }>('/api/v1/Visitors', visitor);
  }

  async getVisitorAnalytics(params?: { dateFrom?: string; dateTo?: string; siteId?: string; supervisorId?: string }): Promise<{ success: boolean; data?: VisitorAnalytics; error?: { code: string; message: string } }> {
    return await this.get<VisitorAnalytics>('/api/v1/Visitors/analytics', params);
  }

  async updateVisitorExit(id: string, exitTime: string): Promise<{ success: boolean; data?: Visitor; error?: { code: string; message: string } }> {
    return await this.patch<Visitor>(`/api/v1/Visitors/${id}/exit`, { exitTime });
  }
}

export const visitorService = new VisitorService();
