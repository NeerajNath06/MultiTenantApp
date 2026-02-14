import BaseApiService from './baseApiService';

export interface Vehicle {
  id: string;
  vehicleNumber: string;
  vehicleType: string;
  driverName: string;
  driverPhone: string;
  entryTime: string;
  exitTime?: string;
  siteId: string;
  siteName?: string;
  guardId: string;
  guardName?: string;
  purpose?: string;
}

class VehicleService extends BaseApiService {
  async getVehicles(filters?: {
    siteId?: string;
    guardId?: string;
    status?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/Vehicles', filters);
  }

  async getVehicleById(id: string): Promise<{ success: boolean; data?: Vehicle; error?: { code: string; message: string } }> {
    return await this.get<Vehicle>(`/api/v1/Vehicles/${id}`);
  }

  async registerVehicle(vehicle: {
    vehicleNumber: string;
    vehicleType: string;
    driverName: string;
    driverPhone: string;
    siteId: string;
    guardId: string;
    purpose?: string;
    agencyId?: string;
  }): Promise<{ success: boolean; data?: Vehicle; error?: { code: string; message: string } }> {
    return await this.post<Vehicle>('/api/v1/Vehicles', vehicle);
  }

  async updateVehicleExit(id: string, exitTime: string): Promise<{ success: boolean; data?: Vehicle; error?: { code: string; message: string } }> {
    return await this.patch<Vehicle>(`/api/v1/Vehicles/${id}/exit`, { exitTime });
  }
}

export const vehicleService = new VehicleService();
