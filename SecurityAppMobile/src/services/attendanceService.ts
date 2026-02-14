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
}

export const attendanceService = new AttendanceService();
