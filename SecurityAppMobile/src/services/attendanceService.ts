import * as FileSystem from 'expo-file-system/legacy';
import { getBaseUrl } from '../config/api.config';
import BaseApiService from './baseApiService';

export interface CheckInRequest {
  guardId: string;
  siteId: string;
  shiftId: string;
  latitude: number;
  longitude: number;
  /** Mobile current time (ISO string) - used as check-in time */
  checkInTime?: string;
  photoBase64: string;
  photoFileName: string;
  notes: string;
  agencyId: string;
}

export interface CheckInResponse {
  success: boolean;
  message?: string;
  data?: any;
  error?: {
    code: string;
    message: string;
  };
}

class AttendanceService extends BaseApiService {
  async checkIn(checkInData: CheckInRequest): Promise<CheckInResponse> {
    console.log('Check-in request data:', checkInData);
    const result = await this.post<any>('/api/v1/Attendance/check-in', checkInData);
    
    if (result.success && result.data) {
      const apiResponse = result.data as any;
      return {
        success: true,
        message: apiResponse.message || 'Check-in successful',
        data: apiResponse.elements || apiResponse.data || apiResponse,
      };
    }
    
    return {
      success: false,
      message: 'Check-in failed',
      error: result.error || {
        code: 'CHECKIN_FAILED',
        message: 'Check-in failed. Please try again.'
      }
    };
  }

  async checkOut(checkOutData: {
    attendanceId: string;
    guardId: string;
    /** Mobile current time (ISO string) - used as check-out time */
    checkOutTime?: string;
    latitude: number;
    longitude: number;
    photoBase64: string;
    photoFileName: string;
    notes?: string;
    agencyId: string;
  }): Promise<CheckInResponse> {
    console.log('Check-out request data:', checkOutData);
    const result = await this.post<any>('/api/v1/Attendance/check-out', checkOutData);
    
    if (result.success && result.data) {
      const apiResponse = result.data as any;
      return {
        success: true,
        message: apiResponse.message || 'Check-out successful',
        data: apiResponse.elements || apiResponse.data || apiResponse,
      };
    }
    
    return {
      success: false,
      message: 'Check-out failed',
      error: result.error || {
        code: 'CHECKOUT_FAILED',
        message: 'Check-out failed. Please try again.'
      }
    };
  }

  // Helper method to convert image URI to base64
  async convertImageToBase64(imageUri: string): Promise<string> {
    try {
      const response = await fetch(imageUri);
      const blob = await response.blob();
      
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = () => {
          const base64 = reader.result as string;
          resolve(base64.split(',')[1]); // Remove data:image/jpeg;base64, prefix
        };
        reader.onerror = reject;
        reader.readAsDataURL(blob);
      });
    } catch (error) {
      console.error('Error converting image to base64:', error);
      throw new Error('Failed to process image');
    }
  }

  async getGuardAttendance(guardId: string, date?: string): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    const params: Record<string, any> = {};
    if (date) params.date = date;
    return await this.get(`/api/v1/Attendance/guard/${guardId}`, params);
  }

  /** Get attendance list (guard-wise) for Web/mobile. */
  async getAttendanceList(params?: {
    guardId?: string;
    startDate?: string;
    endDate?: string;
    status?: string;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: string;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/Attendance', params);
  }

  /**
   * Export attendance as CSV, Excel, or PDF. format: 'csv' | 'xlsx' | 'pdf'.
   * Returns local file URI for sharing/saving.
   */
  async exportAttendance(
    format: 'csv' | 'xlsx' | 'pdf' = 'csv',
    filters?: {
      guardId?: string;
      startDate?: string;
      endDate?: string;
      status?: string;
      search?: string;
      sortBy?: string;
      sortDirection?: string;
    }
  ): Promise<{ success: boolean; localUri?: string; fileName?: string; mimeType?: string; error?: string }> {
    const params = new URLSearchParams({ format });
    if (filters) {
      if (filters.guardId) params.set('guardId', filters.guardId);
      if (filters.startDate) params.set('startDate', filters.startDate);
      if (filters.endDate) params.set('endDate', filters.endDate);
      if (filters.status) params.set('status', filters.status);
      if (filters.search) params.set('search', filters.search);
      if (filters.sortBy) params.set('sortBy', filters.sortBy);
      if (filters.sortDirection) params.set('sortDirection', filters.sortDirection);
    }
    const ext = format === 'pdf' ? 'pdf' : format === 'xlsx' ? 'xlsx' : 'csv';
    const url = `${getBaseUrl()}/api/v1/Attendance/export?${params.toString()}`;
    const headers = await this.getAuthHeaders();
    const fileName = `Attendance_${new Date().toISOString().slice(0, 10)}.${ext}`;
    const path = `${FileSystem.cacheDirectory ?? ''}${fileName}`;
    const mimeType = format === 'pdf' ? 'application/pdf' : format === 'xlsx' ? 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' : 'text/csv';
    try {
      const result = await FileSystem.downloadAsync(url, path, { headers });
      if (result.status !== 200)
        return { success: false, error: `Export failed (${result.status})` };
      return { success: true, localUri: result.uri, fileName, mimeType };
    } catch (e) {
      return { success: false, error: (e as Error).message };
    }
  }
}

export const attendanceService = new AttendanceService();
