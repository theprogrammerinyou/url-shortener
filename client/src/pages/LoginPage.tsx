import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Typography, TextField, Button, Paper, InputAdornment,
  Divider, Alert, CircularProgress, IconButton, Tab, Tabs
} from '@mui/material';
import PersonOutlineIcon from '@mui/icons-material/PersonOutline';
import EmailOutlinedIcon from '@mui/icons-material/EmailOutlined';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined';
import VisibilityOffOutlinedIcon from '@mui/icons-material/VisibilityOffOutlined';
import LinkIcon from '@mui/icons-material/Link';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import PersonIcon from '@mui/icons-material/Person';
import { authApi } from '../api/client';
import { useApp } from '../context/AppContext';
import type { AuthUser } from '../context/AppContext';

type TabId = 'signin' | 'signup';

const inputSx = {
  mb: 2.5,
  '& .MuiOutlinedInput-root': {
    color: '#fff',
    borderRadius: 2,
    bgcolor: 'rgba(255,255,255,0.06)',
    '& fieldset': { borderColor: 'rgba(255,255,255,0.15)' },
    '&:hover fieldset': { borderColor: 'rgba(255,255,255,0.3)' },
    '&.Mui-focused fieldset': { borderColor: '#6366F1' },
  },
  '& .MuiInputLabel-root': { color: 'rgba(255,255,255,0.5)' },
  '& .MuiInputLabel-root.Mui-focused': { color: '#a5b4fc' },
};

export default function LoginPage() {
  const { login } = useApp();
  const navigate = useNavigate();
  const [tab, setTab] = useState<TabId>('signin');
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [guestLoading, setGuestLoading] = useState(false);
  const [error, setError] = useState('');

  const reset = () => {
    setName(''); setEmail(''); setPassword(''); setError('');
  };

  const handleTabChange = (_: React.SyntheticEvent, v: TabId) => {
    setTab(v);
    reset();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (tab === 'signup' && !name.trim()) { setError('Please enter your name.'); return; }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) { setError('Please enter a valid email.'); return; }
    if (password.length < 6) { setError('Password must be at least 6 characters.'); return; }

    setLoading(true);
    try {
      const res = tab === 'signup'
        ? await authApi.register({ name: name.trim(), email: email.trim(), password })
        : await authApi.login({ email: email.trim(), password });

      const authUser: AuthUser = {
        userId: res.data.userId,
        name: res.data.name,
        email: res.data.email,
        plan: res.data.plan,
        expiresAt: res.data.expiresAt,
      };
      login(res.data.token, authUser);
      navigate('/', { replace: true });
    } catch (err: any) {
      const msg = err?.response?.data?.message ?? (tab === 'signup' ? 'Registration failed.' : 'Login failed.');
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleGuest = async () => {
    setGuestLoading(true);
    // Just navigate home — guest state is the default (no token)
    await new Promise((r) => setTimeout(r, 300));
    navigate('/', { replace: true });
    setGuestLoading(false);
  };

  const passwordField = (
    <TextField
      fullWidth label="Password" variant="outlined"
      type={showPassword ? 'text' : 'password'}
      value={password} onChange={(e) => setPassword(e.target.value)}
      autoComplete={tab === 'signup' ? 'new-password' : 'current-password'}
      InputProps={{
        startAdornment: (
          <InputAdornment position="start">
            <LockOutlinedIcon sx={{ color: 'rgba(255,255,255,0.4)' }} />
          </InputAdornment>
        ),
        endAdornment: (
          <InputAdornment position="end">
            <IconButton
              onClick={() => setShowPassword((v) => !v)} edge="end"
              sx={{ color: 'rgba(255,255,255,0.4)', '&:hover': { color: 'rgba(255,255,255,0.7)' } }}
            >
              {showPassword ? <VisibilityOffOutlinedIcon fontSize="small" /> : <VisibilityOutlinedIcon fontSize="small" />}
            </IconButton>
          </InputAdornment>
        ),
      }}
      sx={{ ...inputSx, mb: 3 }}
    />
  );

  return (
    <Box sx={{
      minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center',
      background: 'linear-gradient(135deg, #0F0C29 0%, #302B63 50%, #24243e 100%)',
      position: 'relative', overflow: 'hidden',
    }}>
      {/* Blobs */}
      <Box sx={{ position: 'absolute', top: '-15%', left: '-10%', width: 500, height: 500, borderRadius: '50%', background: 'radial-gradient(circle, rgba(99,102,241,0.25) 0%, transparent 70%)', pointerEvents: 'none' }} />
      <Box sx={{ position: 'absolute', bottom: '-15%', right: '-10%', width: 600, height: 600, borderRadius: '50%', background: 'radial-gradient(circle, rgba(79,70,229,0.2) 0%, transparent 70%)', pointerEvents: 'none' }} />

      <Paper elevation={0} sx={{
        width: '100%', maxWidth: 480, mx: 2, p: { xs: 4, sm: 5 },
        borderRadius: 4,
        background: 'rgba(255,255,255,0.05)',
        backdropFilter: 'blur(20px)',
        border: '1px solid rgba(255,255,255,0.12)',
        boxShadow: '0 24px 64px rgba(0,0,0,0.5)',
      }}>
        {/* Logo */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 4 }}>
          <Box sx={{
            width: 42, height: 42, borderRadius: 2.5, display: 'flex', alignItems: 'center', justifyContent: 'center',
            background: 'linear-gradient(135deg, #6366F1 0%, #4F46E5 100%)',
            boxShadow: '0 4px 16px rgba(99,102,241,0.5)',
          }}>
            <LinkIcon sx={{ color: '#fff', fontSize: 22 }} />
          </Box>
          <Typography variant="h5" sx={{ color: '#fff', fontWeight: 800, letterSpacing: '-0.5px' }}>
            LinkSwift
          </Typography>
        </Box>

        {/* Tabs */}
        <Tabs
          value={tab} onChange={handleTabChange}
          sx={{
            mb: 3,
            '& .MuiTabs-indicator': { background: 'linear-gradient(90deg, #6366F1, #818CF8)', height: 3, borderRadius: 2 },
            '& .MuiTab-root': { color: 'rgba(255,255,255,0.45)', fontWeight: 600, textTransform: 'none', fontSize: '1rem' },
            '& .Mui-selected': { color: '#fff !important' },
          }}
        >
          <Tab label="Sign In" value="signin" />
          <Tab label="Create Account" value="signup" />
        </Tabs>

        <Typography variant="body2" sx={{ color: 'rgba(255,255,255,0.45)', mb: 3 }}>
          {tab === 'signin'
            ? 'Welcome back! Sign in to your account.'
            : 'Create your free account and start shortening links.'}
        </Typography>

        {error && <Alert severity="error" sx={{ mb: 2.5, borderRadius: 2 }}>{error}</Alert>}

        <Box component="form" onSubmit={handleSubmit} noValidate>
          {tab === 'signup' && (
            <TextField
              fullWidth label="Full Name" variant="outlined"
              value={name} onChange={(e) => setName(e.target.value)}
              autoFocus autoComplete="name"
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <PersonOutlineIcon sx={{ color: 'rgba(255,255,255,0.4)' }} />
                  </InputAdornment>
                ),
              }}
              sx={inputSx}
            />
          )}

          <TextField
            fullWidth label="Email Address" type="email" variant="outlined"
            value={email} onChange={(e) => setEmail(e.target.value)}
            autoFocus={tab === 'signin'} autoComplete="email"
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <EmailOutlinedIcon sx={{ color: 'rgba(255,255,255,0.4)' }} />
                </InputAdornment>
              ),
            }}
            sx={inputSx}
          />

          {passwordField}

          <Button
            type="submit" fullWidth variant="contained" size="large"
            disabled={loading}
            endIcon={loading ? <CircularProgress size={18} color="inherit" /> : <ArrowForwardIcon />}
            sx={{
              py: 1.75, borderRadius: 2.5, fontSize: '1rem', fontWeight: 700, textTransform: 'none',
              background: 'linear-gradient(135deg, #6366F1 0%, #4F46E5 100%)',
              boxShadow: '0 8px 24px rgba(99,102,241,0.4)',
              '&:hover': {
                background: 'linear-gradient(135deg, #818CF8 0%, #6366F1 100%)',
                boxShadow: '0 12px 32px rgba(99,102,241,0.5)',
                transform: 'translateY(-1px)',
              },
              transition: 'all 0.2s',
            }}
          >
            {loading ? (tab === 'signup' ? 'Creating account…' : 'Signing in…') : (tab === 'signup' ? 'Create Account' : 'Sign In')}
          </Button>
        </Box>

        <Divider sx={{ my: 3, borderColor: 'rgba(255,255,255,0.12)', '&::before,&::after': { borderColor: 'rgba(255,255,255,0.12)' } }}>
          <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.4)', px: 1 }}>or</Typography>
        </Divider>

        <Button
          fullWidth variant="outlined" size="large" disabled={guestLoading}
          startIcon={guestLoading ? <CircularProgress size={18} color="inherit" /> : <PersonIcon />}
          onClick={handleGuest}
          sx={{
            py: 1.6, borderRadius: 2.5, fontSize: '0.95rem', fontWeight: 600, textTransform: 'none',
            color: 'rgba(255,255,255,0.75)', borderColor: 'rgba(255,255,255,0.2)',
            '&:hover': { borderColor: 'rgba(255,255,255,0.45)', bgcolor: 'rgba(255,255,255,0.06)' },
            transition: 'all 0.2s',
          }}
        >
          Continue as Guest
        </Button>

        <Typography variant="caption" sx={{ display: 'block', mt: 3, textAlign: 'center', color: 'rgba(255,255,255,0.3)' }}>
          Guests can shorten links without an account.
        </Typography>
      </Paper>
    </Box>
  );
}
