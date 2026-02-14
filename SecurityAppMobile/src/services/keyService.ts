import BaseApiService from './baseApiService';

export interface Key {
  id: string;
  keyName: string;
  keyCode: string;
  location: string;
  siteId: string;
  siteName?: string;
  status: string;
  assignedToGuardId?: string;
  assignedToGuardName?: string;
}

class KeyService extends BaseApiService {
  async getKeys(filters?: {
    siteId?: string;
    status?: string;
    guardId?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/Keys', filters);
  }

  async getKeyById(id: string): Promise<{ success: boolean; data?: Key; error?: { code: string; message: string } }> {
    return await this.get<Key>(`/api/v1/Keys/${id}`);
  }

  async assignKey(keyId: string, guardId: string): Promise<{ success: boolean; data?: Key; error?: { code: string; message: string } }> {
    return await this.patch<Key>(`/api/v1/Keys/${keyId}/assign`, { guardId });
  }

  async returnKey(keyId: string): Promise<{ success: boolean; data?: Key; error?: { code: string; message: string } }> {
    return await this.patch<Key>(`/api/v1/Keys/${keyId}/return`, {});
  }
}

export const keyService = new KeyService();
