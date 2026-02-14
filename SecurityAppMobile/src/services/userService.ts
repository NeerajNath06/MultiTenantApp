import BaseApiService from './baseApiService';

export interface UserProfile {
  id: string;
  userName: string;
  email: string;
  firstName: string;
  lastName?: string;
  phoneNumber?: string;
  departmentId?: string;
  departmentName?: string;
  designationId?: string;
  designationName?: string;
  isActive: boolean;
  roles: string[];
  createdDate?: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName?: string;
  email: string;
  phoneNumber?: string;
  newPassword?: string;
}

function normalizeProfile(raw: any): UserProfile | null {
  if (!raw || typeof raw !== 'object') return null;
  return {
    id: raw.id ?? raw.Id,
    userName: raw.userName ?? raw.UserName ?? '',
    email: raw.email ?? raw.Email ?? '',
    firstName: raw.firstName ?? raw.FirstName ?? '',
    lastName: raw.lastName ?? raw.LastName ?? undefined,
    phoneNumber: raw.phoneNumber ?? raw.PhoneNumber ?? undefined,
    departmentId: raw.departmentId ?? raw.DepartmentId,
    departmentName: raw.departmentName ?? raw.DepartmentName,
    designationId: raw.designationId ?? raw.DesignationId,
    designationName: raw.designationName ?? raw.DesignationName,
    isActive: raw.isActive ?? raw.IsActive ?? true,
    roles: Array.isArray(raw.roles) ? raw.roles : Array.isArray(raw.Roles) ? raw.Roles : [],
    createdDate: raw.createdDate ?? raw.CreatedDate,
  } as UserProfile;
}

class UserService extends BaseApiService {
  async getProfile(userId: string): Promise<{ success: boolean; data?: UserProfile; error?: { code: string; message: string } }> {
    const result = await this.get<any>(`/api/v1/Users/${userId}`);
    if (result.success && result.data) {
      const normalized = normalizeProfile(result.data);
      if (normalized) return { success: true, data: normalized };
    }
    return result.success ? { success: true, data: undefined } : { success: false, error: result.error };
  }

  async updateProfile(userId: string, data: UpdateProfileRequest): Promise<{ success: boolean; message?: string; error?: { code: string; message: string } }> {
    const result = await this.put<unknown>(`/api/v1/Users/${userId}`, {
      firstName: data.firstName,
      lastName: data.lastName ?? '',
      email: data.email,
      phoneNumber: data.phoneNumber ?? '',
      newPassword: data.newPassword ?? undefined,
    });
    if (result.success) return { success: true, message: 'Profile updated successfully' };
    return { success: false, error: result.error };
  }
}

export const userService = new UserService();
