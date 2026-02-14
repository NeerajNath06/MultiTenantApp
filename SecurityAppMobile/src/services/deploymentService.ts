import BaseApiService from './baseApiService';
import { cacheService } from './cacheService';

export interface Deployment {
  id: string;
  guardId: string;
  guardName?: string;
  siteId: string;
  siteName?: string;
  shiftId: string;
  shiftName?: string;
  deploymentDate: string;
  startTime: string;
  endTime: string;
  status: string;
  agencyId?: string;
  agencyName?: string;
}

export interface Roster {
  dateFrom: string;
  dateTo: string;
  deployments: Deployment[];
}

class DeploymentService extends BaseApiService {
  async getDeployments(filters?: {
    siteId?: string;
    guardId?: string;
    shiftId?: string;
    dateFrom?: string;
    dateTo?: string;
    pageNumber?: number;
    pageSize?: number;
    skipCache?: boolean;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    const { skipCache, ...apiFilters } = filters || {};
    const cacheKey = `deployments_${JSON.stringify(apiFilters)}`;

    if (!skipCache) {
      const cached = await cacheService.get(cacheKey);
      if (cached) {
        return { success: true, data: cached };
      }
    }

    const result = await this.get('/api/v1/Deployments', apiFilters);

    if (result.success && result.data != null && !skipCache) {
      await cacheService.set(cacheKey, result.data, 5 * 60 * 1000);
    } else if (!result.success && !skipCache) {
      const cached = await cacheService.get(cacheKey);
      if (cached) {
        return { success: true, data: cached };
      }
    }

    return result;
  }

  async getDeploymentById(id: string): Promise<{ success: boolean; data?: Deployment; error?: { code: string; message: string } }> {
    return await this.get<Deployment>(`/api/v1/Deployments/${id}`);
  }

  async getRoster(dateFrom: string, dateTo: string, siteId?: string, supervisorId?: string): Promise<{ success: boolean; data?: Roster; error?: { code: string; message: string } }> {
    const params: Record<string, any> = { dateFrom, dateTo };
    if (siteId) params.siteId = siteId;
    if (supervisorId) params.supervisorId = supervisorId;
    return await this.get<Roster>('/api/v1/Deployments/roster', params);
  }

  async createAssignment(body: {
    guardId: string;
    siteId: string;
    shiftId: string;
    supervisorId?: string;
    assignmentStartDate: string;
    assignmentEndDate?: string;
    remarks?: string;
  }): Promise<{ success: boolean; data?: string; error?: { code: string; message: string } }> {
    const res = await this.post<{ id?: string } | string>('/api/v1/GuardAssignments', {
      guardId: body.guardId,
      siteId: body.siteId,
      shiftId: body.shiftId,
      supervisorId: body.supervisorId ?? null,
      assignmentStartDate: body.assignmentStartDate,
      assignmentEndDate: body.assignmentEndDate ?? null,
      remarks: body.remarks ?? null,
    });
    if (res.success && res.data != null) {
      const id = typeof res.data === 'string' ? res.data : (res.data as { id?: string }).id;
      return { success: true, data: id ?? undefined };
    }
    return { success: false, error: res.error };
  }

  async updateAssignment(id: string, body: {
    guardId: string;
    siteId: string;
    shiftId: string;
    supervisorId?: string;
    assignmentStartDate: string;
    assignmentEndDate?: string;
    remarks?: string;
  }): Promise<{ success: boolean; error?: { code: string; message: string } }> {
    const res = await this.put<boolean>(`/api/v1/GuardAssignments/${id}`, {
      guardId: body.guardId,
      siteId: body.siteId,
      shiftId: body.shiftId,
      supervisorId: body.supervisorId ?? null,
      assignmentStartDate: body.assignmentStartDate,
      assignmentEndDate: body.assignmentEndDate ?? null,
      remarks: body.remarks ?? null,
    });
    return res;
  }
}

export const deploymentService = new DeploymentService();

