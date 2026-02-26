import BaseApiService from './baseApiService';

export interface VehicleLogItem {
  id: string;
  vehicleNumber: string;
  vehicleType: string;
  driverName: string;
  driverPhone?: string;
  purpose: string;
  parkingSlot?: string;
  siteId: string;
  siteName?: string;
  guardId: string;
  guardName?: string;
  entryTime: string;
  exitTime?: string;
  status: 'in' | 'out';
}

export interface VehicleLogSiteSummary {
  siteId: string;
  siteName: string;
  siteAddress?: string;
  totalEntries: number;
  insideCount: number;
  exitedCount: number;
}

class VehicleService extends BaseApiService {
  /**
   * Get vehicle log summary by site (for admin/supervisor). Returns per-site counts.
   */
  async getVehicleLogSummary(filters?: {
    siteId?: string;
    dateFrom?: string;
    dateTo?: string;
  }): Promise<{ success: boolean; data?: { sites: VehicleLogSiteSummary[] }; error?: { code: string; message: string } }> {
    const params: Record<string, string> = {};
    if (filters?.siteId) params.siteId = filters.siteId;
    if (filters?.dateFrom) params.dateFrom = filters.dateFrom;
    if (filters?.dateTo) params.dateTo = filters.dateTo;
    const result = await this.get<{ sites?: VehicleLogSiteSummary[] }>('/api/v1/Vehicles/summary', params);
    if (!result.success) return { success: false, error: result.error };
    const raw = result.data as any;
    const sites = raw?.sites ?? raw?.Sites ?? [];
    const list = (Array.isArray(sites) ? sites : []).map(normalizeSiteSummary);
    return { success: true, data: { sites: list } };
  }

  /**
   * Get vehicle log list. Guard: pass guardId+siteId to see only their site. Admin/Supervisor: pass only siteId to see all logs for that site.
   */
  async getVehicleLogs(filters?: {
    siteId?: string;
    guardId?: string;
    search?: string;
    dateFrom?: string;
    dateTo?: string;
    insideOnly?: boolean;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDirection?: string;
  }): Promise<{ success: boolean; data?: { items: VehicleLogItem[]; totalCount: number }; error?: { code: string; message: string } }> {
    const params: Record<string, unknown> = {};
    if (filters?.siteId) params.siteId = filters.siteId;
    if (filters?.guardId) params.guardId = filters.guardId;
    if (filters?.search) params.search = filters.search;
    if (filters?.dateFrom) params.dateFrom = filters.dateFrom;
    if (filters?.dateTo) params.dateTo = filters.dateTo;
    if (filters?.insideOnly !== undefined) params.insideOnly = filters.insideOnly;
    if (filters?.pageNumber != null) params.pageNumber = filters.pageNumber;
    if (filters?.pageSize != null) params.pageSize = filters.pageSize;
    if (filters?.sortBy) params.sortBy = filters.sortBy;
    if (filters?.sortDirection) params.sortDirection = filters.sortDirection;

    const result = await this.get<{ items?: VehicleLogItem[]; totalCount?: number } | VehicleLogItem[]>('/api/v1/Vehicles', params as Record<string, string | number | boolean>);
    if (!result.success) return { success: false, error: result.error };

    const raw = result.data as any;
    const list = raw?.items ?? raw?.Items ?? (Array.isArray(raw) ? raw : []);
    const totalCount = raw?.totalCount ?? raw?.TotalCount ?? list.length;
    const items = (Array.isArray(list) ? list : []).map(normalizeVehicleLog);
    return { success: true, data: { items, totalCount } };
  }

  async getVehicleLogById(id: string): Promise<{ success: boolean; data?: VehicleLogItem; error?: { code: string; message: string } }> {
    const result = await this.get<VehicleLogItem>(`/api/v1/Vehicles/${id}`);
    if (!result.success || !result.data) return { success: false, error: result.error };
    return { success: true, data: normalizeVehicleLog(result.data as any) };
  }

  async registerVehicleEntry(entry: {
    vehicleNumber: string;
    vehicleType: string;
    driverName: string;
    driverPhone?: string;
    purpose: string;
    parkingSlot?: string;
    siteId: string;
    guardId: string;
  }): Promise<{ success: boolean; data?: { id: string; entryTime: string }; error?: { code: string; message: string } }> {
    const result = await this.post<{ id?: string; entryTime?: string }>('/api/v1/Vehicles', entry);
    if (!result.success) return { success: false, error: result.error };
    const data = result.data as any;
    const id = data?.id ?? data?.Id;
    const entryTime = data?.entryTime ?? data?.EntryTime;
    return { success: true, data: id != null ? { id: String(id), entryTime: entryTime ?? new Date().toISOString() } : undefined };
  }

  async recordVehicleExit(id: string, exitTime?: string): Promise<{ success: boolean; error?: { code: string; message: string } }> {
    const body = exitTime ? { exitTime } : {};
    const result = await this.patch<boolean>(`/api/v1/Vehicles/${id}/exit`, body);
    return result.success ? { success: true } : { success: false, error: result.error };
  }
}

function normalizeSiteSummary(d: any): VehicleLogSiteSummary {
  return {
    siteId: d.siteId ?? d.SiteId ?? '',
    siteName: d.siteName ?? d.SiteName ?? '',
    siteAddress: d.siteAddress ?? d.SiteAddress,
    totalEntries: d.totalEntries ?? d.TotalEntries ?? 0,
    insideCount: d.insideCount ?? d.InsideCount ?? 0,
    exitedCount: d.exitedCount ?? d.ExitedCount ?? 0,
  };
}

function normalizeVehicleLog(d: any): VehicleLogItem {
  const entryTime = d.entryTime ?? d.EntryTime ?? '';
  const exitTime = d.exitTime ?? d.ExitTime;
  return {
    id: d.id ?? d.Id ?? '',
    vehicleNumber: d.vehicleNumber ?? d.VehicleNumber ?? '',
    vehicleType: (d.vehicleType ?? d.VehicleType ?? 'car').toLowerCase(),
    driverName: d.driverName ?? d.DriverName ?? '',
    driverPhone: d.driverPhone ?? d.DriverPhone,
    purpose: d.purpose ?? d.Purpose ?? '',
    parkingSlot: d.parkingSlot ?? d.ParkingSlot,
    siteId: d.siteId ?? d.SiteId ?? '',
    siteName: d.siteName ?? d.SiteName,
    guardId: d.guardId ?? d.GuardId ?? '',
    guardName: d.guardName ?? d.GuardName,
    entryTime: typeof entryTime === 'string' ? entryTime : (entryTime?.toISO?.() ?? ''),
    exitTime: exitTime != null ? (typeof exitTime === 'string' ? exitTime : exitTime?.toISO?.()) : undefined,
    status: exitTime != null ? 'out' : 'in',
  };
}

export const vehicleService = new VehicleService();
