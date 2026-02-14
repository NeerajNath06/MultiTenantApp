import BaseApiService from './baseApiService';

export interface PatrolRoute {
  id: string;
  routeName: string;
  siteId: string;
  siteName?: string;
  description?: string;
  checkpoints: PatrolCheckpoint[];
}

export interface PatrolCheckpoint {
  id: string;
  checkpointName: string;
  latitude: number;
  longitude: number;
  order: number;
}

export interface PatrolLog {
  id: string;
  routeId: string;
  routeName?: string;
  guardId: string;
  guardName?: string;
  siteId?: string;
  siteName?: string;
  checkpointId: string;
  scannedAt: string;
  latitude: number;
  longitude: number;
  notes?: string;
}

class PatrolService extends BaseApiService {
  async getPatrolRoutes(filters?: {
    siteId?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/Patrol/routes', filters);
  }

  async getPatrolRouteById(id: string): Promise<{ success: boolean; data?: PatrolRoute; error?: { code: string; message: string } }> {
    return await this.get<PatrolRoute>(`/api/v1/Patrol/routes/${id}`);
  }

  async createPatrolLog(log: {
    routeId: string;
    guardId: string;
    checkpointId: string;
    latitude: number;
    longitude: number;
    notes?: string;
    agencyId?: string;
  }): Promise<{ success: boolean; data?: PatrolLog; error?: { code: string; message: string } }> {
    return await this.post<PatrolLog>('/api/v1/Patrol/logs', log);
  }

  async getPatrolLogs(filters?: {
    routeId?: string;
    guardId?: string;
    siteId?: string;
    dateFrom?: string;
    dateTo?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/Patrol/logs', filters);
  }

  /** Start a patrol session (local/API). Returns patrol id for recording checkpoints. */
  async startPatrol(
    routeId: string,
    guardId: string,
    _latitude: number,
    _longitude: number
  ): Promise<{ success: boolean; data?: { id: string }; error?: { code: string; message: string } }> {
    return { success: true, data: { id: routeId } };
  }

  /** Record a checkpoint scan during patrol. */
  async recordCheckpoint(params: {
    patrolId: string;
    guardId: string;
    checkpointId: string;
    latitude: number;
    longitude: number;
    notes?: string;
  }): Promise<{ success: boolean; data?: PatrolLog; error?: { code: string; message: string } }> {
    return await this.createPatrolLog({
      routeId: params.patrolId,
      guardId: params.guardId,
      checkpointId: params.checkpointId,
      latitude: params.latitude,
      longitude: params.longitude,
      notes: params.notes,
    });
  }
}

export const patrolService = new PatrolService();
