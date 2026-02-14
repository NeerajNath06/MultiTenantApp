import BaseApiService from './baseApiService';

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  userId: string;
}

class NotificationService extends BaseApiService {
  async getNotifications(params: {
    userId: string;
    isRead?: boolean;
    type?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: { items: Notification[]; unreadCount: number }; error?: { code: string; message: string } }> {
    const res = await this.get<any>('/api/v1/Notifications', params);
    if (!res.success) return res;
    const raw = res.data as any;
    const items = raw?.items ?? raw?.data?.items ?? [];
    const unreadCount = raw?.unreadCount ?? raw?.data?.unreadCount ?? 0;
    return { success: true, data: { items, unreadCount } };
  }

  async markAsRead(notificationId: string, userId: string): Promise<{ success: boolean; error?: { code: string; message: string } }> {
    const result = await this.patch(`/api/v1/Notifications/${notificationId}/read?userId=${encodeURIComponent(userId)}`, {});
    return { success: result.success, error: result.error };
  }

  async markAllAsRead(userId: string): Promise<{ success: boolean; error?: { code: string; message: string } }> {
    const result = await this.patch(`/api/v1/Notifications/read-all?userId=${encodeURIComponent(userId)}`, {});
    return { success: result.success, error: result.error };
  }
}

export const notificationService = new NotificationService();
