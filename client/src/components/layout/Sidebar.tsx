import { Box, Typography, Avatar, Button, Drawer, IconButton } from '@mui/material';
import { useEffect, useState } from 'react';
import { NavLink, useLocation, useNavigate } from 'react-router-dom';
import LinkIcon from '@mui/icons-material/Link';
import GridViewIcon from '@mui/icons-material/GridView';
import BarChartIcon from '@mui/icons-material/BarChart';
import SettingsIcon from '@mui/icons-material/Settings';
import LogoutIcon from '@mui/icons-material/Logout';
import LoginIcon from '@mui/icons-material/Login';
import RocketLaunchIcon from '@mui/icons-material/RocketLaunch';
import DarkModeOutlinedIcon from '@mui/icons-material/DarkModeOutlined';
import WbSunnyOutlinedIcon from '@mui/icons-material/WbSunnyOutlined';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import { useApp } from '../../context/AppContext';
import { dashboardApi } from '../../api/client';

const SIDEBAR_WIDTH = 260;

const navItems = [
  { label: 'Shorten', path: '/', icon: <LinkIcon fontSize="small" /> },
  { label: 'Links', path: '/links', icon: <GridViewIcon fontSize="small" />, showCount: true },
  { label: 'Analytics', path: '/analytics', icon: <BarChartIcon fontSize="small" /> },
  { label: 'Settings', path: '/settings', icon: <SettingsIcon fontSize="small" />, requireAuth: true },
];

interface SidebarProps {
  onCollapse?: () => void;
}

export default function Sidebar({ onCollapse }: SidebarProps) {
  const location = useLocation();
  const navigate = useNavigate();
  const { userId, profile, preferences, authUser, isAuthenticated, logout, updatePreferences } = useApp();
  const [linksCount, setLinksCount] = useState<number | null>(null);

  const isGuest = !isAuthenticated;

  const initials = authUser?.name
    ? authUser.name.split(' ').map((n) => n[0]).join('').slice(0, 2).toUpperCase()
    : 'G';

  useEffect(() => {
    if (userId) {
      dashboardApi.getStats(userId)
        .then((res) => setLinksCount(res.data.activeLinks))
        .catch(console.error);
    }
  }, [userId, profile]);

  const isActive = (path: string) => {
    if (path === '/') return location.pathname === '/';
    return location.pathname.startsWith(path);
  };

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  const toggleDarkMode = () => {
    updatePreferences({ theme: preferences?.theme === 'dark' ? 'light' : 'dark' });
  };

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: SIDEBAR_WIDTH,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: SIDEBAR_WIDTH,
          boxSizing: 'border-box',
          borderRight: '1px solid',
          borderColor: 'divider',
          bgcolor: 'background.paper',
          display: 'flex',
          flexDirection: 'column',
        },
      }}
    >
      {/* Header */}
      <Box sx={{ px: 3, py: 3, display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h6" sx={{ color: 'primary.main', fontWeight: 800, fontSize: '1.35rem' }}>
          LinkSwift
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <IconButton size="small" onClick={toggleDarkMode} title={preferences?.theme === 'dark' ? 'Light Mode' : 'Dark Mode'}>
            {preferences?.theme === 'dark' ? <WbSunnyOutlinedIcon fontSize="small" /> : <DarkModeOutlinedIcon fontSize="small" />}
          </IconButton>
          {onCollapse && (
            <IconButton size="small" onClick={onCollapse}>
              <ChevronLeftIcon />
            </IconButton>
          )}
        </Box>
      </Box>

      {/* Navigation */}
      <Box sx={{ px: 2, flex: 1 }}>
        {navItems.filter((item) => !item.requireAuth || !isGuest).map((item) => {
          const active = isActive(item.path);
          return (
            <Box
              key={item.path}
              component={NavLink}
              to={item.path}
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1.5,
                px: 2,
                py: 1.25,
                mb: 0.5,
                borderRadius: 2,
                textDecoration: 'none',
                color: active ? '#fff' : 'text.secondary',
                bgcolor: active ? 'primary.main' : 'transparent',
                fontWeight: 600,
                fontSize: '0.925rem',
                transition: 'all 0.15s',
                '&:hover': {
                  bgcolor: active ? 'primary.main' : 'action.hover',
                },
              }}
            >
              {item.icon}
              {item.label}
              {item.showCount && linksCount !== null && (
                <Box sx={{
                  ml: 'auto',
                  bgcolor: active ? 'rgba(255,255,255,0.2)' : 'secondary.main',
                  color: active ? '#fff' : 'primary.main',
                  px: 1,
                  py: 0.25,
                  borderRadius: 1.5,
                  fontSize: '0.75rem',
                  fontWeight: 700
                }}>
                  {linksCount}
                </Box>
              )}
            </Box>
          );
        })}

        {/* Go Pro card on Links page */}
        {location.pathname.startsWith('/links') && (
          <Box
            sx={{
              mt: 3,
              mx: 0.5,
              p: 2.5,
              borderRadius: 3,
              background: 'linear-gradient(135deg, #4F46E5 0%, #6366F1 100%)',
              color: '#fff',
            }}
          >
            <RocketLaunchIcon sx={{ mb: 1 }} />
            <Typography variant="subtitle2" fontWeight={700}>Go Pro</Typography>
            <Typography variant="caption" sx={{ opacity: 0.85, display: 'block', mt: 0.5, mb: 2 }}>
              Unlock advanced analytics and custom domains.
            </Typography>
            <Button
              size="small"
              onClick={() => navigate('/pricing')}
              sx={{ bgcolor: '#fff', color: 'primary.main', '&:hover': { bgcolor: '#EEF2FF' } }}
            >
              Upgrade Now
            </Button>
          </Box>
        )}
      </Box>

      {/* Footer: Log In button for guests, profile card for logged-in users */}
      <Box sx={{ p: 2, borderTop: '1px solid', borderColor: 'divider' }}>
        {isGuest ? (
          <Button
            fullWidth
            variant="contained"
            startIcon={<LoginIcon />}
            onClick={() => navigate('/login')}
            sx={{
              borderRadius: 2, py: 1.25, fontWeight: 700, textTransform: 'none',
              background: 'linear-gradient(135deg, #6366F1 0%, #4F46E5 100%)',
              boxShadow: '0 4px 16px rgba(99,102,241,0.3)',
              '&:hover': {
                background: 'linear-gradient(135deg, #818CF8 0%, #6366F1 100%)',
              },
            }}
          >
            Log In
          </Button>
        ) : (
          <Box sx={{
            display: 'flex', alignItems: 'center', gap: 1.5,
            p: 1.5, borderRadius: 2, bgcolor: 'secondary.main'
          }}>
            <Avatar sx={{ width: 36, height: 36, bgcolor: 'primary.main', fontSize: '0.85rem' }}>
              {initials}
            </Avatar>
            <Box sx={{ flex: 1, minWidth: 0 }}>
              <Typography variant="body2" fontWeight={700} noWrap>
                {authUser?.name ?? profile?.displayName}
              </Typography>
              <Typography variant="caption" color="text.secondary" noWrap>
                {authUser?.email ?? profile?.email}
              </Typography>
            </Box>
            <IconButton
              size="small"
              onClick={handleLogout}
              title="Log out"
              sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}
            >
              <LogoutIcon fontSize="small" />
            </IconButton>
          </Box>
        )}
      </Box>
    </Drawer>
  );
}

export { SIDEBAR_WIDTH };
