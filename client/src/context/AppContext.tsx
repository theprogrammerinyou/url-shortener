import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { ThemeProvider, CssBaseline, useMediaQuery } from '@mui/material';
import { userApi } from '../api/client';
import type { UserPreferencesResponse, UserProfileResponse } from '../api/types';
import { useUserId } from '../hooks/useUserId';
import { darkTheme, lightTheme } from '../theme';

const TOKEN_KEY = 'linkswift_token';
const AUTH_USER_KEY = 'linkswift_auth_user';

export interface AuthUser {
  userId: string;
  name: string;
  email: string;
  plan: string;
  expiresAt: string;
}

interface AppContextValue {
  userId: string;
  profile: UserProfileResponse | null;
  preferences: UserPreferencesResponse | null;
  authUser: AuthUser | null;         // non-null = JWT logged in
  isAuthenticated: boolean;
  login: (token: string, user: AuthUser) => void;
  logout: () => void;
  refreshProfile: () => Promise<void>;
  updatePreferences: (updates: Partial<UserPreferencesResponse>) => Promise<void>;
  showSnackbar: (message: string) => void;
}

const AppContext = createContext<AppContextValue | null>(null);

export function AppProvider({ children }: { children: React.ReactNode }) {
  const guestUserId = useUserId();
  const [profile, setProfile] = useState<UserProfileResponse | null>(null);
  const [preferences, setPreferences] = useState<UserPreferencesResponse | null>(null);
  const [snackbar, setSnackbar] = useState('');
  const prefersDark = useMediaQuery('(prefers-color-scheme: dark)');

  // Synchronous local theme to prevent FOUC before backend preferences load
  const [localTheme, setLocalTheme] = useState(() => localStorage.getItem('linkswift_theme') || 'system');

  useEffect(() => {
    if (preferences?.theme) {
      localStorage.setItem('linkswift_theme', preferences.theme);
      setLocalTheme(preferences.theme);
    }
  }, [preferences?.theme]);

  // ── JWT auth state ──────────────────────────────────────────────────────
  const [authUser, setAuthUser] = useState<AuthUser | null>(() => {
    try {
      const raw = localStorage.getItem(AUTH_USER_KEY);
      if (!raw) return null;
      const parsed: AuthUser = JSON.parse(raw);
      // Check token not expired
      if (new Date(parsed.expiresAt) <= new Date()) {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(AUTH_USER_KEY);
        return null;
      }
      return parsed;
    } catch {
      return null;
    }
  });

  const isAuthenticated = authUser !== null;

  // The effective userId: JWT user's id (UUID) or legacy guest id
  const userId = authUser?.userId ?? guestUserId;

  const login = useCallback((token: string, user: AuthUser) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(AUTH_USER_KEY, JSON.stringify(user));
    setAuthUser(user);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(AUTH_USER_KEY);
    setAuthUser(null);
    setProfile(null);
    setPreferences(null);
  }, []);

  // ── Profile / Prefs ─────────────────────────────────────────────────────
  const refreshProfile = useCallback(async () => {
    if (!userId) return;
    try {
      const [profileRes, prefsRes] = await Promise.all([
        userApi.getProfile(userId),
        userApi.getPreferences(userId),
      ]);
      setProfile(profileRes.data);
      setPreferences(prefsRes.data);
    } catch {
      // Guest or profile not yet created — ignore
    }
  }, [userId]);

  useEffect(() => {
    refreshProfile().catch(console.error);
  }, [refreshProfile]);

  const updatePreferences = useCallback(
    async (updates: Partial<UserPreferencesResponse>) => {
      if (!userId) return;
      const res = await userApi.updatePreferences({ userId, ...updates });
      setPreferences(res.data);
    },
    [userId]
  );

  const showSnackbar = useCallback((message: string) => setSnackbar(message), []);

  const themeMode = preferences?.theme ?? localTheme;
  const isDark = themeMode === 'dark' || (themeMode === 'system' && prefersDark);

  const value = useMemo(
    () => ({
      userId,
      profile,
      preferences,
      authUser,
      isAuthenticated,
      login,
      logout,
      refreshProfile,
      updatePreferences,
      showSnackbar,
    }),
    [userId, profile, preferences, authUser, isAuthenticated, login, logout, refreshProfile, updatePreferences, showSnackbar]
  );

  return (
    <AppContext.Provider value={value}>
      <ThemeProvider theme={isDark ? darkTheme : lightTheme}>
        <CssBaseline />
        {children}
        {snackbar && (
          <div
            role="status"
            style={{
              position: 'fixed',
              bottom: 24,
              left: '50%',
              transform: 'translateX(-50%)',
              background: '#1E1B4B',
              color: '#fff',
              padding: '10px 20px',
              borderRadius: 8,
              zIndex: 9999,
              fontSize: 14,
            }}
            onAnimationEnd={() => setTimeout(() => setSnackbar(''), 2000)}
          >
            {snackbar}
          </div>
        )}
      </ThemeProvider>
    </AppContext.Provider>
  );
}

export function useApp() {
  const ctx = useContext(AppContext);
  if (!ctx) throw new Error('useApp must be used within AppProvider');
  return ctx;
}
