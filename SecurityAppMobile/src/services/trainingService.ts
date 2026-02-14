import BaseApiService from './baseApiService';

export interface TrainingRecordItem {
  id: string;
  guardId: string;
  guardName: string;
  guardCode: string;
  trainingType: string;
  trainingName: string;
  trainingProvider?: string;
  trainingDate: string;
  expiryDate?: string;
  status: string;
  certificateNumber?: string;
  score?: number;
  isActive: boolean;
  createdDate: string;
}

export interface TrainingRecordListResponse {
  items: TrainingRecordItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

class TrainingService extends BaseApiService {
  async getTrainingRecords(params?: {
    pageNumber?: number;
    pageSize?: number;
    guardId?: string;
    trainingType?: string;
    status?: string;
    search?: string;
    sortBy?: string;
    sortDirection?: string;
  }): Promise<{ success: boolean; data?: TrainingRecordListResponse; error?: { code: string; message: string } }> {
    const res = await this.get<any>('/api/v1/TrainingRecords', params);
    if (!res.success) return res;
    const raw = res.data as any;
    const data: TrainingRecordListResponse = {
      items: raw?.items ?? raw?.data?.items ?? [],
      totalCount: raw?.totalCount ?? raw?.data?.totalCount ?? 0,
      pageNumber: raw?.pageNumber ?? raw?.data?.pageNumber ?? 1,
      pageSize: raw?.pageSize ?? raw?.data?.pageSize ?? 10,
      totalPages: raw?.totalPages ?? raw?.data?.totalPages ?? 0,
    };
    return { success: true, data };
  }
}

export const trainingService = new TrainingService();
