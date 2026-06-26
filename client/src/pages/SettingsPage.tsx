import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Typography, Paper, TextField, Button, Switch, FormControlLabel,
  Select, MenuItem, InputAdornment, IconButton,
} from '@mui/material';
import PaletteOutlinedIcon from '@mui/icons-material/PaletteOutlined';
import WbSunnyOutlinedIcon from '@mui/icons-material/WbSunnyOutlined';
import DarkModeOutlinedIcon from '@mui/icons-material/DarkModeOutlined';
import ComputerOutlinedIcon from '@mui/icons-material/ComputerOutlined';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import { userApi } from '../api/client';
import { useApp } from '../context/AppContext';

const themeOptions = [
  { value: 'light', label: 'Light', icon: <WbSunnyOutlinedIcon /> },
  { value: 'dark', label: 'Dark', icon: <DarkModeOutlinedIcon /> },
  { value: 'system', label: 'System', icon: <ComputerOutlinedIcon /> },
];

export default function SettingsPage() {
  const { userId, profile, preferences, refreshProfile, updatePreferences, showSnackbar } = useApp();
  const navigate = useNavigate();

  const [defaultDomain, setDefaultDomain] = useState('l.swift');
  const [maskedApiKey, setMaskedApiKey] = useState('');
  const [weeklyReport, setWeeklyReport] = useState(true);
  const [linkAlerts, setLinkAlerts] = useState(true);
  const [newDeviceLogin, setNewDeviceLogin] = useState(false);
  const [compactView, setCompactView] = useState(false);
  const [theme, setTheme] = useState('light');

  const isGuest = !profile || profile.displayName === 'John Doe' || profile.email.startsWith(userId.slice(0, 8));

  // Redirect if Guest
  useEffect(() => {
    if (userId && isGuest) {
      navigate('/');
      showSnackbar('Please log in to access settings.');
    }
  }, [userId, isGuest, navigate, showSnackbar]);

  useEffect(() => {
    if (profile) {
      setDefaultDomain(profile.defaultDomain);
      setMaskedApiKey(profile.maskedApiKey);
    }
    if (preferences) {
      setWeeklyReport(preferences.weeklyAnalyticsReport);
      setLinkAlerts(preferences.linkThresholdAlerts);
      setNewDeviceLogin(preferences.newDeviceLogin);
      setCompactView(preferences.compactView);
      setTheme(preferences.theme);
    }
  }, [profile, preferences]);

  const saveWorkspaceDomain = async (domain: string) => {
    setDefaultDomain(domain);
    await userApi.updateProfile({ userId, defaultDomain: domain });
    await refreshProfile();
    showSnackbar('Default domain updated');
  };

  const handlePrefChange = async (updates: Record<string, boolean | string>) => {
    await updatePreferences(updates);
    showSnackbar('Preferences saved');
  };

  const regenerateKey = async () => {
    const res = await userApi.regenerateApiKey(userId);
    setMaskedApiKey(res.data.maskedApiKey);
    showSnackbar('API key regenerated');
  };

  if (isGuest) {
    return null;
  }

  return (
    <Box>
      <Typography variant="h5" fontWeight={800} sx={{ mb: 4 }}>Settings</Typography>

      <Paper sx={{ p: 3, borderRadius: 3, mb: 3, border: '1px solid', borderColor: 'divider' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h6" fontWeight={700}>Workspace</Typography>
          <Select
            size="small"
            value={defaultDomain}
            onChange={(e) => saveWorkspaceDomain(e.target.value)}
            sx={{ minWidth: 140 }}
          >
            <MenuItem value="l.swift">l.swift</MenuItem>
            <MenuItem value="lnks.wt">lnks.wt</MenuItem>
            <MenuItem value="go.link">go.link</MenuItem>
          </Select>
        </Box>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Use your API key to integrate LinkSwift into your workflows and automations.
        </Typography>
        <Button variant="outlined" sx={{ mb: 2 }} onClick={regenerateKey}>
          Regenerate Key
        </Button>
        <TextField
          fullWidth
          value={maskedApiKey}
          InputProps={{
            readOnly: true,
            sx: { bgcolor: 'secondary.main' },
            endAdornment: (
              <InputAdornment position="end">
                <IconButton onClick={() => { navigator.clipboard.writeText(maskedApiKey); showSnackbar('Copied'); }}>
                  <ContentCopyIcon fontSize="small" />
                </IconButton>
              </InputAdornment>
            ),
          }}
        />
      </Paper>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 3 }}>
        <Paper sx={{ p: 3, borderRadius: 3, border: '1px solid', borderColor: 'divider' }}>
          <Typography variant="h6" fontWeight={700} sx={{ mb: 2 }}>Notifications</Typography>
          {[
            { label: 'Weekly analytics report', value: weeklyReport, key: 'weeklyAnalyticsReport', setter: setWeeklyReport },
            { label: 'Link threshold alerts', value: linkAlerts, key: 'linkThresholdAlerts', setter: setLinkAlerts },
            { label: 'New device login', value: newDeviceLogin, key: 'newDeviceLogin', setter: setNewDeviceLogin },
          ].map((item) => (
            <Box key={item.key} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 1.5, borderBottom: '1px solid', borderColor: 'divider' }}>
              <Typography variant="body2">{item.label}</Typography>
              <Switch
                checked={item.value}
                onChange={(_, checked) => {
                  item.setter(checked);
                  handlePrefChange({ [item.key]: checked });
                }}
              />
            </Box>
          ))}
        </Paper>

        <Paper sx={{ p: 3, borderRadius: 3, border: '1px solid', borderColor: 'divider' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
            <PaletteOutlinedIcon color="primary" />
            <Typography variant="h6" fontWeight={700}>Appearance</Typography>
          </Box>
          <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 1.5, mb: 3 }}>
            {themeOptions.map((opt) => (
              <Box
                key={opt.value}
                onClick={() => { setTheme(opt.value); handlePrefChange({ theme: opt.value }); }}
                sx={{
                  p: 2, borderRadius: 2, textAlign: 'center', cursor: 'pointer',
                  border: '2px solid',
                  borderColor: theme === opt.value ? 'primary.main' : 'divider',
                  bgcolor: theme === opt.value ? 'secondary.main' : 'background.paper',
                }}
              >
                <Box sx={{ color: 'primary.main', mb: 0.5 }}>{opt.icon}</Box>
                <Typography variant="caption" fontWeight={600}>{opt.label}</Typography>
              </Box>
            ))}
          </Box>
          <FormControlLabel
            control={
              <Switch
                checked={compactView}
                onChange={(_, checked) => {
                  setCompactView(checked);
                  handlePrefChange({ compactView: checked });
                }}
              />
            }
            label="Compact View"
          />
        </Paper>
      </Box>
    </Box>
  );
}
