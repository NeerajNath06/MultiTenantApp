import BaseApiService from './baseApiService';

export interface WageItem {
  id: string;
  wageSheetNumber: string;
  wagePeriodStart: string;
  wagePeriodEnd: string;
  paymentDate: string;
  status: string;
  totalWages: number;
  netAmount: number;
  isActive: boolean;
  createdDate: string;
}

export interface WageListResponse {
  items: WageItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface GuardPayslipItem {
  id: string;
  wageId: string;
  wageSheetNumber: string;
  wagePeriodStart: string;
  wagePeriodEnd: string;
  paymentDate: string;
  status: string;
  daysWorked: number;
  hoursWorked: number;
  basicAmount: number;
  overtimeAmount: number;
  allowances: number;
  deductions: number;
  grossAmount: number;
  netAmount: number;
  remarks?: string;
}

export interface GuardPayslipsResponse {
  items: GuardPayslipItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

class PayrollService extends BaseApiService {
  async getGuardPayslips(guardId: string, params?: {
    pageNumber?: number;
    pageSize?: number;
    periodStart?: string;
    periodEnd?: string;
  }): Promise<{ success: boolean; data?: GuardPayslipsResponse; error?: { code: string; message: string } }> {
    const res = await this.get<any>(`/api/v1/Wages/guard/${guardId}`, params);
    if (!res.success) return res;
    const raw = res.data as any;
    const data: GuardPayslipsResponse = {
      items: raw?.items ?? raw?.data?.items ?? [],
      totalCount: raw?.totalCount ?? raw?.data?.totalCount ?? 0,
      pageNumber: raw?.pageNumber ?? raw?.data?.pageNumber ?? 1,
      pageSize: raw?.pageSize ?? raw?.data?.pageSize ?? 24,
      totalPages: raw?.totalPages ?? raw?.data?.totalPages ?? 0,
    };
    return { success: true, data };
  }

  async getWages(params?: {
    pageNumber?: number;
    pageSize?: number;
    status?: string;
    periodStart?: string;
    periodEnd?: string;
    sortBy?: string;
    sortDirection?: string;
  }): Promise<{ success: boolean; data?: WageListResponse; error?: { code: string; message: string } }> {
    const res = await this.get<any>('/api/v1/Wages', params);
    if (!res.success) return res;
    const raw = res.data as any;
    const data: WageListResponse = {
      items: raw?.items ?? raw?.data?.items ?? [],
      totalCount: raw?.totalCount ?? raw?.data?.totalCount ?? 0,
      pageNumber: raw?.pageNumber ?? raw?.data?.pageNumber ?? 1,
      pageSize: raw?.pageSize ?? raw?.data?.pageSize ?? 10,
      totalPages: raw?.totalPages ?? raw?.data?.totalPages ?? 0,
    };
    return { success: true, data };
  }
}

export const payrollService = new PayrollService();
