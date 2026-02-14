import BaseApiService from './baseApiService';

export interface GuardItem {
  id: string;
  guardCode: string;
  firstName: string;
  lastName: string;
  email?: string;
  phoneNumber: string;
  gender: string;
  dateOfBirth: string;
  isActive: boolean;
  supervisorId?: string;
  supervisorName?: string;
}

class GuardService extends BaseApiService {
  /**
   * Get guards list. When supervisorId is passed (e.g. current user when they are supervisor),
   * returns only guards assigned to that supervisor.
   */
  async getGuards(params?: {
    supervisorId?: string;
    pageNumber?: number;
    pageSize?: number;
    includeInactive?: boolean;
    search?: string;
  }): Promise<{ success: boolean; data?: { items: GuardItem[]; totalCount: number }; error?: { code: string; message: string } }> {
    const query: Record<string, string> = {};
    if (params?.supervisorId) query.supervisorId = params.supervisorId;
    if (params?.pageNumber != null) query.pageNumber = String(params.pageNumber);
    if (params?.pageSize != null) query.pageSize = String(params.pageSize);
    if (params?.includeInactive != null) query.includeInactive = String(params.includeInactive);
    if (params?.search) query.search = params.search;
    const result = await this.get<any>('/api/v1/SecurityGuards', query);
    if (!result.success) return result;
    const raw = result.data as any;
    const items: GuardItem[] = Array.isArray(raw) ? raw : (raw?.items ?? raw?.data ?? []);
    const totalCount = typeof raw?.totalCount === 'number' ? raw.totalCount : items.length;
    return { success: true, data: { items, totalCount } };
  }
}

export const guardService = new GuardService();
