import BaseApiService from './baseApiService';

export interface EquipmentItem {
  id: string;
  equipmentCode: string;
  equipmentName: string;
  category: string;
  manufacturer?: string;
  modelNumber?: string;
  purchaseDate: string;
  purchaseCost: number;
  status: string;
  assignedToGuardId?: string;
  assignedToGuardName?: string;
  assignedToSiteId?: string;
  assignedToSiteName?: string;
  nextMaintenanceDate?: string;
  isActive: boolean;
  createdDate: string;
}

export interface EquipmentListResponse {
  items: EquipmentItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

class EquipmentService extends BaseApiService {
  async getEquipment(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    category?: string;
    status?: string;
    assignedToGuardId?: string;
    assignedToSiteId?: string;
    sortBy?: string;
    sortDirection?: string;
  }): Promise<{ success: boolean; data?: EquipmentListResponse; error?: { code: string; message: string } }> {
    const res = await this.get<any>('/api/v1/Equipment', params);
    if (!res.success) return res;
    const raw = res.data as any;
    const data: EquipmentListResponse = {
      items: raw?.items ?? raw?.data?.items ?? [],
      totalCount: raw?.totalCount ?? raw?.data?.totalCount ?? 0,
      pageNumber: raw?.pageNumber ?? raw?.data?.pageNumber ?? 1,
      pageSize: raw?.pageSize ?? raw?.data?.pageSize ?? 10,
      totalPages: raw?.totalPages ?? raw?.data?.totalPages ?? 0,
    };
    return { success: true, data };
  }
}

export const equipmentService = new EquipmentService();
