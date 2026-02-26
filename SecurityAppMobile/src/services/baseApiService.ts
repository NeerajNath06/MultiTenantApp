import AsyncStorage from '@react-native-async-storage/async-storage';
import { getBaseUrl, DEFAULT_APP_TIMEZONE } from '../config/api.config';

/**
 * Base API Service for all mobile services
 * Handles authentication, AgencyId injection, and common error handling
 */
class BaseApiService {
  protected readonly BASE_URL = getBaseUrl();

  /**
   * Get authentication headers with token and AgencyId
   */
  protected async getAuthHeaders(): Promise<HeadersInit> {
    const token = await this.getStoredToken();
    const userData = await this.getStoredUser();
    const agencyId = userData?.agencyId;

    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    // Add tenant/agency header for multitenancy (API expects X-Tenant-Id)
    if (agencyId) {
      headers['X-Agency-Id'] = agencyId;
      headers['X-Tenant-Id'] = agencyId;
    }

    // App timezone (default India) so API uses same region for times (punch-in, shifts, notifications)
    headers['X-Timezone'] = DEFAULT_APP_TIMEZONE;

    return headers;
  }

  /**
   * Get stored authentication token
   */
  protected async getStoredToken(): Promise<string | null> {
    try {
      return await AsyncStorage.getItem('authToken');
    } catch (error) {
      console.error('Get token error:', error);
      return null;
    }
  }

  /**
   * Get stored user data
   */
  protected async getStoredUser(): Promise<{ agencyId?: string } | null> {
    try {
      const userData = await AsyncStorage.getItem('userData');
      return userData ? JSON.parse(userData) : null;
    } catch (error) {
      console.error('Get user error:', error);
      return null;
    }
  }

  /**
   * Get AgencyId from stored user data
   */
  protected async getAgencyId(): Promise<string | null> {
    const user = await this.getStoredUser();
    return user?.agencyId || null;
  }

  /**
   * Add AgencyId to request body if not present
   */
  protected async enrichRequestBody<T extends Record<string, any>>(body: T): Promise<T> {
    const agencyId = await this.getAgencyId();
    if (agencyId && !body.agencyId) {
      return { ...body, agencyId };
    }
    return body;
  }

  /**
   * Add AgencyId to URL search params
   */
  protected async enrichUrlParams(params: URLSearchParams): Promise<URLSearchParams> {
    const agencyId = await this.getAgencyId();
    if (agencyId && !params.has('agencyId')) {
      params.append('agencyId', agencyId);
    }
    return params;
  }

  /**
   * Generic GET request with multitenancy support
   */
  protected async get<T>(
    endpoint: string,
    params?: Record<string, any>
  ): Promise<{ success: boolean; data?: T; error?: { code: string; message: string } }> {
    try {
      const headers = await this.getAuthHeaders();
      const urlParams = new URLSearchParams();

      if (params) {
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null) {
            urlParams.append(key, value.toString());
          }
        });
      }

      // Add AgencyId to params
      const enrichedParams = await this.enrichUrlParams(urlParams);
      const queryString = enrichedParams.toString();
      const url = queryString ? `${endpoint}?${queryString}` : endpoint;

      const response = await fetch(`${this.BASE_URL}${url}`, {
        method: 'GET',
        headers,
      });

      const text = await response.text();
      let data: any = null;
      try {
        data = text ? JSON.parse(text) : null;
      } catch (_) {
        if (response.ok) {
          return {
            success: false,
            error: {
              code: 'PARSE_ERROR',
              message: 'Invalid response from server. Please try again.',
            },
          };
        }
      }

      if (response.ok) {
        return {
          success: true,
          data: this.extractData<T>(data ?? {}),
        };
      } else {
        return {
          success: false,
          error: {
            code: 'API_ERROR',
            message: (data?.message) || (text && text.length < 200 ? text : `Request failed (${response.status})`),
          },
        };
      }
    } catch (error) {
      console.error(`GET ${endpoint} error:`, error);
      return {
        success: false,
        error: {
          code: 'NETWORK_ERROR',
          message: 'Network error. Please check your connection.',
        },
      };
    }
  }

  /**
   * POST FormData (e.g. file upload). Do not set Content-Type so fetch sets multipart boundary.
   */
  protected async postForm<T>(
    endpoint: string,
    formData: FormData
  ): Promise<{ success: boolean; data?: T; error?: { code: string; message: string } }> {
    try {
      const headers: HeadersInit = {};
      const token = await this.getStoredToken();
      const userData = await this.getStoredUser();
      if (token) headers['Authorization'] = `Bearer ${token}`;
      if (userData?.agencyId) {
        headers['X-Agency-Id'] = userData.agencyId;
        headers['X-Tenant-Id'] = userData.agencyId;
      }

      const response = await fetch(`${this.BASE_URL}${endpoint}`, {
        method: 'POST',
        headers,
        body: formData,
      });

      const text = await response.text();
      let data: any = null;
      try {
        data = text ? JSON.parse(text) : null;
      } catch (_) {
        if (response.ok) return { success: true, data: undefined as T };
      }

      if (response.ok) {
        return { success: true, data: this.extractData<T>(data ?? {}) };
      }
      return {
        success: false,
        error: {
          code: 'API_ERROR',
          message: (data?.message) || (text && text.length < 200 ? text : `Request failed (${response.status})`),
        },
      };
    } catch (error) {
      console.error(`POST Form ${endpoint} error:`, error);
      return {
        success: false,
        error: { code: 'NETWORK_ERROR', message: 'Network error. Please check your connection.' },
      };
    }
  }

  /**
   * Generic POST request with multitenancy support
   */
  protected async post<T>(
    endpoint: string,
    body?: any
  ): Promise<{ success: boolean; data?: T; error?: { code: string; message: string } }> {
    try {
      const headers = await this.getAuthHeaders();
      
      // Enrich body with AgencyId if it's an object
      let enrichedBody = body;
      if (body && typeof body === 'object' && !(body instanceof FormData)) {
        enrichedBody = await this.enrichRequestBody(body);
      }

      const response = await fetch(`${this.BASE_URL}${endpoint}`, {
        method: 'POST',
        headers,
        body: enrichedBody ? JSON.stringify(enrichedBody) : undefined,
      });

      const text = await response.text();
      let data: any = null;
      try {
        data = text ? JSON.parse(text) : null;
      } catch {
        if (response.ok) {
          return { success: false, error: { code: 'API_ERROR', message: 'Invalid response from server.' } };
        }
        return {
          success: false,
          error: {
            code: 'API_ERROR',
            message: text && text.length < 300 ? text : `Request failed (${response.status})`,
          },
        };
      }

      if (response.ok) {
        return {
          success: true,
          data: this.extractData<T>(data),
        };
      } else {
        return {
          success: false,
          error: {
            code: 'API_ERROR',
            message: (data?.message ?? data?.Message) || `Failed to post to ${endpoint}`,
          },
        };
      }
    } catch (error) {
      console.error(`POST ${endpoint} error:`, error);
      return {
        success: false,
        error: {
          code: 'NETWORK_ERROR',
          message: 'Network error. Please check your connection.',
        },
      };
    }
  }

  /**
   * Generic PUT request with multitenancy support
   */
  protected async put<T>(
    endpoint: string,
    body?: any
  ): Promise<{ success: boolean; data?: T; error?: { code: string; message: string } }> {
    try {
      const headers = await this.getAuthHeaders();
      
      // Enrich body with AgencyId if it's an object
      let enrichedBody = body;
      if (body && typeof body === 'object' && !(body instanceof FormData)) {
        enrichedBody = await this.enrichRequestBody(body);
      }

      const response = await fetch(`${this.BASE_URL}${endpoint}`, {
        method: 'PUT',
        headers,
        body: enrichedBody ? JSON.stringify(enrichedBody) : undefined,
      });

      const data = await response.json();

      if (response.ok) {
        return {
          success: true,
          data: this.extractData<T>(data),
        };
      } else {
        return {
          success: false,
          error: {
            code: 'API_ERROR',
            message: data.message || `Failed to update at ${endpoint}`,
          },
        };
      }
    } catch (error) {
      console.error(`PUT ${endpoint} error:`, error);
      return {
        success: false,
        error: {
          code: 'NETWORK_ERROR',
          message: 'Network error. Please check your connection.',
        },
      };
    }
  }

  /**
   * Generic PATCH request with multitenancy support
   */
  protected async patch<T>(
    endpoint: string,
    body?: any
  ): Promise<{ success: boolean; data?: T; error?: { code: string; message: string } }> {
    try {
      const headers = await this.getAuthHeaders();
      
      // Enrich body with AgencyId if it's an object
      let enrichedBody = body;
      if (body && typeof body === 'object' && !(body instanceof FormData)) {
        enrichedBody = await this.enrichRequestBody(body);
      }

      const response = await fetch(`${this.BASE_URL}${endpoint}`, {
        method: 'PATCH',
        headers,
        body: enrichedBody ? JSON.stringify(enrichedBody) : undefined,
      });

      const data = await response.json();

      if (response.ok) {
        return {
          success: true,
          data: this.extractData<T>(data),
        };
      } else {
        return {
          success: false,
          error: {
            code: 'API_ERROR',
            message: data.message || `Failed to patch at ${endpoint}`,
          },
        };
      }
    } catch (error) {
      console.error(`PATCH ${endpoint} error:`, error);
      return {
        success: false,
        error: {
          code: 'NETWORK_ERROR',
          message: 'Network error. Please check your connection.',
        },
      };
    }
  }

  /**
   * Generic DELETE request
   */
  protected async delete<T>(
    endpoint: string
  ): Promise<{ success: boolean; data?: T; error?: { code: string; message: string } }> {
    try {
      const headers = await this.getAuthHeaders();

      const response = await fetch(`${this.BASE_URL}${endpoint}`, {
        method: 'DELETE',
        headers,
      });

      const data = await response.json();

      if (response.ok) {
        return {
          success: true,
          data: this.extractData<T>(data),
        };
      } else {
        return {
          success: false,
          error: {
            code: 'API_ERROR',
            message: data.message || `Failed to delete at ${endpoint}`,
          },
        };
      }
    } catch (error) {
      console.error(`DELETE ${endpoint} error:`, error);
      return {
        success: false,
        error: {
          code: 'NETWORK_ERROR',
          message: 'Network error. Please check your connection.',
        },
      };
    }
  }

  /**
   * Extract data from API response (handles ResponseModel wrapper)
   * Supports both camelCase (data) and PascalCase (Data) from API
   */
  protected extractData<T>(data: any): T {
    if (data && typeof data === 'object') {
      if ('elements' in data) return data.elements as T;
      if (typeof data.data !== 'undefined') return data.data as T;
      if (typeof data.Data !== 'undefined') return data.Data as T;
      if (Array.isArray(data) || typeof data === 'object') return data as T;
    }
    return data as T;
  }
}

export default BaseApiService;
