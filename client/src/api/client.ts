import axios from 'axios';
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  CreateShortUrlRequest,
  CreateShortUrlResponse,
  DashboardStatsResponse,
  DashboardSummaryResponse,
  GeoItem,
  LinkAnalyticsDetailResponse,
  LinkVelocityResponse,
  PagedLinksResponse,
  ReferrerItem,
  ClickTimePoint,
  UrlDetailsResponse,
  UserPreferencesResponse,
  UserProfileResponse,
} from './types';

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL,
});

// Attach JWT token from localStorage to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('linkswift_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const authApi = {
  register: (payload: RegisterRequest) =>
    api.post<AuthResponse>('/auth/register', payload),
  login: (payload: LoginRequest) =>
    api.post<AuthResponse>('/auth/login', payload),
};

export const urlApi = {
  shorten: (payload: CreateShortUrlRequest) =>
    api.post<CreateShortUrlResponse>('/Url/shorten', payload),
  getRecent: (userId: string, limit = 5) =>
    api.get<UrlDetailsResponse[]>('/Url/recent', { params: { userId, limit } }),
  getPaged: (params: { userId: string; search?: string; status?: string; page?: number; pageSize?: number }) =>
    api.get<PagedLinksResponse>('/Url', { params: { ...params, page: params.page ?? 1, pageSize: params.pageSize ?? 10 } }),
  delete: (shortCode: string, userId: string) =>
    api.delete(`/Url/${shortCode}`, { params: { userId } }),
  recordQrScan: (shortCode: string) =>
    api.post(`/Url/${shortCode}/qr-scan`),
};

export const dashboardApi = {
  getStats: (userId: string) =>
    api.get<DashboardStatsResponse>('/dashboard/stats', { params: { userId } }),
  getSummary: (userId: string) =>
    api.get<DashboardSummaryResponse>('/dashboard/summary', { params: { userId } }),
  getVelocity: (userId: string) =>
    api.get<LinkVelocityResponse>('/dashboard/velocity', { params: { userId } }),
};

export const analyticsApi = {
  getDetail: (shortCode: string) =>
    api.get<LinkAnalyticsDetailResponse>(`/analytics/${shortCode}`),
  getClicks: (shortCode: string, period: 'daily' | 'weekly' = 'daily') =>
    api.get<{ points: ClickTimePoint[] }>(`/analytics/${shortCode}/clicks`, { params: { period } }),
  getReferrers: (shortCode: string) =>
    api.get<{ referrers: ReferrerItem[] }>(`/analytics/${shortCode}/referrers`),
  getGeo: (shortCode: string) =>
    api.get<{ countries: GeoItem[] }>(`/analytics/${shortCode}/geo`),
};

export const userApi = {
  getProfile: (userId: string) =>
    api.get<UserProfileResponse>('/user/profile', { params: { userId } }),
  updateProfile: (payload: { userId: string; displayName?: string; email?: string; defaultDomain?: string }) =>
    api.put<UserProfileResponse>('/user/profile', payload),
  getPreferences: (userId: string) =>
    api.get<UserPreferencesResponse>('/user/preferences', { params: { userId } }),
  updatePreferences: (payload: Partial<UserPreferencesResponse> & { userId: string }) =>
    api.patch<UserPreferencesResponse>('/user/preferences', payload),
  regenerateApiKey: (userId: string) =>
    api.post<{ maskedApiKey: string }>('/workspace/keys/regenerate', null, { params: { userId } }),
};

export default api;
