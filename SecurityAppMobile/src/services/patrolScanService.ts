import BaseApiService from './baseApiService';

export interface PatrolScanItem {
  id: string;
  guardId: string;
  siteId: string;
  siteName?: string;
  scannedAt: string;
  locationName: string;
  checkpointCode?: string;
  status: string;
}

class PatrolScanService extends BaseApiService {
  async getScans(params: {
    guardId: string;
    siteId?: string;
    dateFrom?: string;
    dateTo?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: { items: PatrolScanItem[]; totalCount: number }; error?: { code: string; message: string } }> {
    const res = await this.get<any>('/api/v1/PatrolScans', params);
    if (!res.success) return res;
    const raw = res.data as any;
    const items = raw?.items ?? raw?.data?.items ?? [];
    const totalCount = raw?.totalCount ?? raw?.data?.totalCount ?? 0;
    return { success: true, data: { items, totalCount } };
  }

  async recordScan(body: {
    guardId: string;
    siteId: string;
    locationName: string;
    checkpointCode?: string;
  }): Promise<{ success: boolean; data?: string; error?: { code: string; message: string } }> {
    const res = await this.post<{ data?: string }>('/api/v1/PatrolScans', body);
    if (!res.success) return { success: false, error: res.error };
    const id = (res.data as any)?.data ?? (res.data as any);
    return { success: true, data: typeof id === 'string' ? id : undefined };
  }
}

export const patrolScanService = new PatrolScanService();
