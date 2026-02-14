import BaseApiService from './baseApiService';

export interface Shift {
  id: string;
  shiftName: string;
  startTime: string;
  endTime: string;
  durationHours: number;
  description?: string;
}

class ShiftService extends BaseApiService {
  async getShifts(filters?: {
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/Shifts', filters);
  }

  async getShiftById(id: string): Promise<{ success: boolean; data?: Shift; error?: { code: string; message: string } }> {
    return await this.get<Shift>(`/api/v1/Shifts/${id}`);
  }
}

export const shiftService = new ShiftService();

