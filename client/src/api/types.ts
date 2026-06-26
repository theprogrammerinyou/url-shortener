export interface UrlDetailsResponse {
  shortCode: string;
  longUrl: string;
  shortUrl: string;
  createdAt: string;
  expiresAt: string;
  isCustom: boolean;
  userId?: string;
  clickCount: number;
  qrScanCount: number;
  isExpired: boolean;
  isPrivate: boolean;
  status: string;
  clicksToday: number;
}

export interface CreateShortUrlRequest {
  longUrl: string;
  customAlias?: string;
  expiresAt?: string;
  userId?: string;
  generateQrCode?: boolean;
  isPrivate?: boolean;
}

export interface CreateShortUrlResponse {
  shortUrl: string;
  shortCode: string;
  longUrl: string;
  createdAt: string;
}

export interface PagedLinksResponse {
  items: UrlDetailsResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface DashboardStatsResponse {
  totalClicks: number;
  activeLinks: number;
  linkLimit: number;
  topReferral: string;
  topReferralPercentage: number;
  clickGrowthPercentage: number;
}

export interface DashboardSummaryResponse {
  uptimePercentage: number;
  avgRedirectLatencyMs: number;
  qrScansThisMonth: number;
}

export interface LinkVelocityPoint {
  label: string;
  clicks: number;
}

export interface LinkVelocityResponse {
  points: LinkVelocityPoint[];
}

export interface LinkAnalyticsDetailResponse {
  shortCode: string;
  shortUrl: string;
  longUrl: string;
  createdAt: string;
  expiresAt: string;
  isExpired: boolean;
  isActive: boolean;
  totalEngagement: number;
  engagementGrowthPercentage: number;
  conversionRate: number;
  conversions: number;
}

export interface ClickTimePoint {
  label: string;
  clicks: number;
}

export interface ReferrerItem {
  name: string;
  clicks: number;
  percentage: number;
}

export interface GeoItem {
  country: string;
  countryCode: string;
  clicks: number;
  percentage: number;
}

export interface UserProfileResponse {
  userId: string;
  displayName: string;
  email: string;
  defaultDomain: string;
  maskedApiKey: string;
  plan: string;
}

export interface UserPreferencesResponse {
  theme: string;
  weeklyAnalyticsReport: boolean;
  linkThresholdAlerts: boolean;
  newDeviceLogin: boolean;
  compactView: boolean;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  userId: string;
  name: string;
  email: string;
  plan: string;
  expiresAt: string;
}
