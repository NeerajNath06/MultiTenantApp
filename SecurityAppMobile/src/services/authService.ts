import AsyncStorage from '@react-native-async-storage/async-storage';
import { getBaseUrl } from '../config/api.config';

export interface LoginRequest {
  userEmail: string;
  password: string;
}

/** API returns: { success, message, data?, errors?, timestamp } */
export interface LoginApiResponse {
  success: boolean;
  message: string;
  data?: {
    accessToken: string;
    refreshToken: string;
    expiresIn: number;
      user: {
        id: string;
        userName: string;
        email: string;
        firstName: string;
        lastName: string;
        tenantId: string;
        tenantName: string;
        roles: string[];
        permissions: string[];
        guardId?: string;
      };
  };
  errors?: string[];
  timestamp?: string;
}

export interface ApiError {
  code: string;
  message: string;
}

export interface StoredUserData {
  id: string;
  username: string;
  email: string;
  mobile: string;
  role: string;
  agencyId: string;
  guardId?: string;
  isSupervisor?: boolean;
}

class AuthService {
  private readonly BASE_URL = getBaseUrl();

  async login(credentials: LoginRequest): Promise<{ success: boolean; data?: { token: string; user: StoredUserData; message: string }; error?: ApiError }> {
    try {
      console.log('Attempting login to:', `${this.BASE_URL}/api/v1/Auth/login`);
      console.log('Credentials:', credentials);

      const response = await fetch(`${this.BASE_URL}/api/v1/Auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(credentials),
      });

      console.log('Response status:', response.status);

      const raw = await response.json();
      console.log('Response data:', raw);

      // API may return PascalCase (Success, Data, User, Roles) or camelCase
      const success = raw.success ?? raw.Success ?? false;
      const payload = raw.data ?? raw.Data;
      if (success && payload) {
        const u = payload.user ?? payload.User ?? {};
        const roles: string[] = Array.isArray(u.roles) ? u.roles : (Array.isArray(u.Roles) ? u.Roles : []);
        const roleNames = roles.join(', ') || 'No role assigned';
        // Allow Security Guard, Supervisor, or any role containing "Supervisor" (e.g. "Security Supervisor")
        const allowedRoles = ['Security Guard', 'Supervisor', 'Security Supervisor'];
        const isAllowedByRole = roles.some((r: string) => {
          const role = (r || '').toLowerCase();
          return allowedRoles.some((a) => a.toLowerCase() === role) || role.includes('supervisor');
        });
        const isSupervisorFromApi = u.isSupervisor === true || u.IsSupervisor === true;
        const isAllowed = isAllowedByRole || isSupervisorFromApi;
        if (!isAllowed) {
          return {
            success: false,
            error: {
              code: 'MOBILE_ACCESS_DENIED',
              message: `Only Security Guard or Supervisor can login to the mobile app. Your role: ${roleNames}. Please use the web portal.`,
            },
          };
        }
        const userData: StoredUserData = {
          id: u.id ?? u.Id ?? '',
          username: u.userName ?? u.UserName ?? u.email ?? u.Email ?? '',
          email: u.email ?? u.Email ?? '',
          mobile: '',
          role: roles[0] || (u.role ?? u.Role) || '',
          agencyId: u.tenantId ?? u.TenantId ?? '',
          guardId: u.guardId ?? u.GuardId ?? undefined,
          isSupervisor: isSupervisorFromApi || roles.some((r: string) => (r || '').toLowerCase().includes('supervisor')),
        };

        await this.storeAuthData(payload.accessToken ?? payload.AccessToken, payload.refreshToken ?? payload.RefreshToken, userData);

        return {
          success: true,
          data: {
            token: payload.accessToken ?? payload.AccessToken,
            user: userData,
            message: raw.message ?? raw.Message ?? 'Login successful',
          },
        };
      } else {
        return {
          success: false,
          error: {
            code: 'LOGIN_FAILED',
            message: raw.message ?? raw.Message ?? 'Login failed. Please check your credentials.'
          }
        };
      }
    } catch (error) {
      console.error('Login API error:', error);
      console.error('Error details:', error);
      
      // More specific error messages
      let errorMessage = 'Network error. Please check your connection and try again.';
      
      if (error instanceof TypeError && error.message.includes('Network request failed')) {
        errorMessage = `Unable to connect to server at ${this.BASE_URL}. Please check your IP address in src/config/api.config.ts and ensure the backend is running.`;
      }
      
      return {
        success: false,
        error: {
          code: 'NETWORK_ERROR',
          message: errorMessage
        }
      };
    }
  }

  async logout(): Promise<void> {
    try {
      await AsyncStorage.multiRemove(['authToken', 'refreshToken', 'userData']);
    } catch (error) {
      console.error('Logout error:', error);
    }
  }

  async getStoredToken(): Promise<string | null> {
    try {
      return await AsyncStorage.getItem('authToken');
    } catch (error) {
      console.error('Get token error:', error);
      return null;
    }
  }

  async getStoredRefreshToken(): Promise<string | null> {
    try {
      return await AsyncStorage.getItem('refreshToken');
    } catch (error) {
      console.error('Get refresh token error:', error);
      return null;
    }
  }

  async getStoredUser(): Promise<StoredUserData | null> {
    try {
      const userData = await AsyncStorage.getItem('userData');
      return userData ? JSON.parse(userData) : null;
    } catch (error) {
      console.error('Get user error:', error);
      return null;
    }
  }

  private async storeAuthData(token: string, refreshToken: string, userData: StoredUserData): Promise<void> {
    try {
      await AsyncStorage.multiSet([
        ['authToken', token],
        ['refreshToken', refreshToken],
        ['userData', JSON.stringify(userData)]
      ]);
    } catch (error) {
      console.error('Store auth data error:', error);
    }
  }

  async isAuthenticated(): Promise<boolean> {
    const token = await this.getStoredToken();
    return !!token;
  }

  async refreshToken(refreshToken: string): Promise<{ success: boolean; token?: string; error?: ApiError }> {
    try {
      const response = await fetch(`${this.BASE_URL}/api/v1/Auth/refresh-token`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refreshToken }),
      });

      const data = await response.json();

      if (data.success && data.data?.accessToken) {
        await AsyncStorage.setItem('authToken', data.data.accessToken);
        return {
          success: true,
          token: data.data.accessToken,
        };
      } else {
        return {
          success: false,
          error: {
            code: 'REFRESH_FAILED',
            message: data.message || 'Token refresh failed'
          }
        };
      }
    } catch (error) {
      console.error('Refresh token error:', error);
      return {
        success: false,
        error: {
          code: 'NETWORK_ERROR',
          message: 'Network error during token refresh'
        }
      };
    }
  }

  // Helper method to get the current API URL for debugging
  getCurrentApiUrl(): string {
    return this.BASE_URL;
  }
}

export const authService = new AuthService();
