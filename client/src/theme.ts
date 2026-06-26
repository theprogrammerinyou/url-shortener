import { createTheme, ThemeOptions } from '@mui/material/styles';

const baseTheme: ThemeOptions = {
  palette: {
    primary: { main: '#4F46E5', light: '#6366F1', dark: '#3730A3' },
    secondary: { main: '#EEF2FF' },
    background: { default: '#F8F9FC', paper: '#FFFFFF' },
    text: { primary: '#1E1B4B', secondary: '#64748B' },
    success: { main: '#10B981' },
    error: { main: '#EF4444' },
    divider: '#E2E8F0',
  },
  typography: {
    fontFamily: '"Inter", "Roboto", system-ui, sans-serif',
    h4: { fontWeight: 800, letterSpacing: '-0.02em' },
    h5: { fontWeight: 700 },
    h6: { fontWeight: 700 },
    subtitle1: { fontWeight: 600 },
    button: { textTransform: 'none', fontWeight: 600 },
  },
  shape: { borderRadius: 12 },
  components: {
    MuiButton: {
      styleOverrides: {
        root: { borderRadius: 10, boxShadow: 'none' },
        contained: { boxShadow: '0 1px 2px rgba(79,70,229,0.2)' },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: { boxShadow: '0 1px 3px rgba(0,0,0,0.06)', border: '1px solid #E2E8F0' },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: { backgroundImage: 'none' },
      },
    },
  },
};

export const lightTheme = createTheme(baseTheme);

export const darkTheme = createTheme({
  ...baseTheme,
  palette: {
    mode: 'dark',
    primary: { main: '#818CF8', light: '#A5B4FC', dark: '#6366F1' },
    secondary: { main: '#334155' },
    background: { default: '#0F172A', paper: '#1E293B' },
    text: { primary: '#F1F5F9', secondary: '#94A3B8' },
    divider: '#334155',
  },
});
