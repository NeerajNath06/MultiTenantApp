import BaseApiService from './baseApiService';

export interface LeaveRequest {
  id: string;
  guardId: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  reason: string;
  status: string;
  approvedBy?: string;
  rejectedReason?: string;
}

export interface OvertimeRequest {
  id: string;
  guardId: string;
  date: string;
  hours: number;
  reason: string;
  status: string;
  approvedBy?: string;
  rejectedReason?: string;
}

export interface LeaveBalance {
  guardId: string;
  totalLeaves: number;
  usedLeaves: number;
  remainingLeaves: number;
}

class LeaveService extends BaseApiService {
  async getLeaveRequests(filters?: {
    guardId?: string;
    supervisorId?: string;
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/LeaveRequests', filters);
  }

  async getLeaveRequestById(id: string): Promise<{ success: boolean; data?: LeaveRequest; error?: { code: string; message: string } }> {
    return await this.get<LeaveRequest>(`/api/v1/LeaveRequests/${id}`);
  }

  async createLeaveRequest(request: {
    guardId: string;
    leaveType: string;
    startDate: string;
    endDate: string;
    reason: string;
    agencyId?: string;
  }): Promise<{ success: boolean; data?: LeaveRequest; error?: { code: string; message: string } }> {
    return await this.post<LeaveRequest>('/api/v1/LeaveRequests', request);
  }

  async approveLeaveRequest(id: string, payload: { isApproved: boolean; rejectionReason?: string }): Promise<{ success: boolean; data?: boolean; error?: { code: string; message: string } }> {
    return await this.post<boolean>(`/api/v1/LeaveRequests/${id}/approve`, payload);
  }

  async getLeaveBalance(guardId: string): Promise<{ success: boolean; data?: LeaveBalance; error?: { code: string; message: string } }> {
    return await this.get<LeaveBalance>(`/api/v1/LeaveRequests/${guardId}/balance`);
  }

  async getOvertimeRequests(filters?: {
    guardId?: string;
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/LeaveRequests', { ...filters, leaveType: 'overtime' } as any);
  }

  async createOvertimeRequest(request: {
    guardId: string;
    date: string;
    hours: number;
    reason: string;
    agencyId?: string;
  }): Promise<{ success: boolean; data?: OvertimeRequest; error?: { code: string; message: string } }> {
    return await this.post<OvertimeRequest>('/api/v1/LeaveRequests', request as any);
  }
}

export const leaveService = new LeaveService();
