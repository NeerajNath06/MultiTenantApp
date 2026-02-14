import BaseApiService from './baseApiService';

export interface AnnouncementItem {
  id: string;
  title: string;
  content: string;
  category: string;
  postedByName?: string;
  postedAt: string;
  isPinned: boolean;
  isActive: boolean;
  createdDate: string;
}

export interface AnnouncementListResponse {
  items: AnnouncementItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateAnnouncementInput {
  title: string;
  content: string;
  category?: string;
  postedByUserId?: string;
  postedByName?: string;
  isPinned?: boolean;
}

export interface UpdateAnnouncementInput {
  title: string;
  content: string;
  category?: string;
  isPinned?: boolean;
  isActive?: boolean;
}

class AnnouncementService extends BaseApiService {
  async getAnnouncements(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    category?: string;
    isPinned?: boolean;
    includeInactive?: boolean;
    sortBy?: string;
    sortDirection?: string;
  }): Promise<{ success: boolean; data?: AnnouncementListResponse; error?: { code: string; message: string } }> {
    const res = await this.get<any>('/api/v1/Announcements', params);
    if (!res.success) return res;
    const raw = res.data as any;
    const data: AnnouncementListResponse = {
      items: raw?.items ?? raw?.data?.items ?? [],
      totalCount: raw?.totalCount ?? raw?.data?.totalCount ?? 0,
      pageNumber: raw?.pageNumber ?? raw?.data?.pageNumber ?? 1,
      pageSize: raw?.pageSize ?? raw?.data?.pageSize ?? 50,
      totalPages: raw?.totalPages ?? raw?.data?.totalPages ?? 0,
    };
    return { success: true, data };
  }

  async getAnnouncementById(id: string): Promise<{ success: boolean; data?: AnnouncementItem; error?: { code: string; message: string } }> {
    return await this.get<AnnouncementItem>(`/api/v1/Announcements/${id}`);
  }

  async createAnnouncement(input: CreateAnnouncementInput): Promise<{ success: boolean; data?: string; error?: { code: string; message: string } }> {
    const res = await this.post<{ id?: string } | string>('/api/v1/Announcements', {
      title: input.title,
      content: input.content,
      category: input.category ?? 'general',
      postedByUserId: input.postedByUserId ?? null,
      postedByName: input.postedByName ?? null,
      isPinned: input.isPinned ?? false,
    });
    if (res.success && res.data != null) {
      const id = typeof res.data === 'string' ? res.data : (res.data as { id?: string }).id;
      return { success: true, data: id ?? undefined };
    }
    return { success: false, error: res.error };
  }

  async updateAnnouncement(id: string, input: UpdateAnnouncementInput): Promise<{ success: boolean; error?: { code: string; message: string } }> {
    const res = await this.put<boolean>(`/api/v1/Announcements/${id}`, {
      title: input.title,
      content: input.content,
      category: input.category ?? 'general',
      isPinned: input.isPinned ?? false,
      isActive: input.isActive ?? true,
    });
    return res;
  }

  async deleteAnnouncement(id: string): Promise<{ success: boolean; error?: { code: string; message: string } }> {
    return await this.delete<boolean>(`/api/v1/Announcements/${id}`);
  }
}

export const announcementService = new AnnouncementService();
