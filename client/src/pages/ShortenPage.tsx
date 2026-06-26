import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Typography, Paper, TextField, Button, Checkbox, FormControlLabel,
  IconButton, Alert, Dialog, DialogTitle, DialogContent, DialogActions,
  FormControl, InputLabel, Select, MenuItem, Skeleton
} from '@mui/material';
import LinkIcon from '@mui/icons-material/Link';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import BarChartIcon from '@mui/icons-material/BarChart';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import RocketLaunchIcon from '@mui/icons-material/RocketLaunch';
import BoltIcon from '@mui/icons-material/Bolt';
import QrCode2Icon from '@mui/icons-material/QrCode2';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import PublicIcon from '@mui/icons-material/Public';
import CampaignIcon from '@mui/icons-material/Campaign';
import QRCode from 'react-qr-code';
import { dashboardApi, urlApi } from '../api/client';
import type { DashboardSummaryResponse, UrlDetailsResponse } from '../api/types';
import { useApp } from '../context/AppContext';

function formatClicks(n: number) {
  if (n >= 1000) return `${(n / 1000).toFixed(1).replace(/\.0$/, '')}k`;
  return n.toString();
}

export default function ShortenPage() {
  const { userId, showSnackbar } = useApp();
  const navigate = useNavigate();
  const [longUrl, setLongUrl] = useState('');
  const [customAlias, setCustomAlias] = useState('');
  const [useCustomAlias, setUseCustomAlias] = useState(false);
  const [generateQr, setGenerateQr] = useState(false);
  const [expiryOption, setExpiryOption] = useState('never');
  const [customExpiryDate, setCustomExpiryDate] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [recent, setRecent] = useState<UrlDetailsResponse[]>([]);
  const [summary, setSummary] = useState<DashboardSummaryResponse | null>(null);

  // Success Dialog State
  const [successOpen, setSuccessOpen] = useState(false);
  const [newUrl, setNewUrl] = useState('');
  const [newShortCode, setNewShortCode] = useState('');

  // Selected QR Code Dialog State (for view from list)
  const [qrViewOpen, setQrViewOpen] = useState(false);
  const [qrViewUrl, setQrViewUrl] = useState('');

  const loadData = async () => {
    if (!userId) return;
    try {
      const [recentRes, summaryRes] = await Promise.all([
        urlApi.getRecent(userId, 5),
        dashboardApi.getSummary(userId),
      ]);
      setRecent(recentRes.data);
      setSummary(summaryRes.data);
    } finally {
      setInitialLoading(false);
    }
  };

  useEffect(() => {
    loadData().catch(console.error);
  }, [userId]);

  const handleShorten = async () => {
    setError('');
    if (!longUrl) {
      showSnackbar('Please enter a URL');
      return;
    }

    // URL Validation
    try {
      new URL(longUrl);
    } catch {
      showSnackbar('Invalid URL format. Please include http:// or https://');
      return;
    }

    setLoading(true);

    let expiresAt: string | undefined = undefined;
    if (expiryOption !== 'never') {
      const now = new Date();
      if (expiryOption === '1h') now.setHours(now.getHours() + 1);
      else if (expiryOption === '1d') now.setDate(now.getDate() + 1);
      else if (expiryOption === '7d') now.setDate(now.getDate() + 7);
      else if (expiryOption === '30d') now.setDate(now.getDate() + 30);
      else if (expiryOption === 'custom' && customExpiryDate) {
        expiresAt = new Date(customExpiryDate).toISOString();
      }
      if (expiryOption !== 'custom') {
        expiresAt = now.toISOString();
      }
    }

    try {
      const response = await urlApi.shorten({
        longUrl,
        customAlias: useCustomAlias ? customAlias : undefined,
        userId,
        expiresAt,
        generateQrCode: generateQr,
        isPrivate: false,
      });

      const generatedShortUrl = response.data.shortUrl;
      const generatedShortCode = response.data.shortCode;

      if (generateQr) {
        setNewUrl(generatedShortUrl);
        setNewShortCode(generatedShortCode);
        setSuccessOpen(true);
      } else {
        showSnackbar('Link shortened successfully!');
      }

      setLongUrl('');
      setCustomAlias('');
      setUseCustomAlias(false);
      setExpiryOption('never');
      setCustomExpiryDate('');
      await loadData();
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Failed to shorten URL';
      setError(errorMessage);
      showSnackbar(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleCopy = async (url: string) => {
    await navigator.clipboard.writeText(url);
    showSnackbar('Link copied!');
  };

  const handleDelete = async (shortCode: string) => {
    await urlApi.delete(shortCode, userId);
    showSnackbar('Link deleted');
    await loadData();
  };

  const linkIcons = [<PublicIcon />, <CampaignIcon />];

  return (
    <Box>
      <Box sx={{ textAlign: 'center', mb: 5 }}>
        <Typography variant="h4" sx={{ mb: 1.5, fontWeight: 800 }}>
          Make every link{' '}
          <Box component="span" sx={{ color: 'primary.main' }}>count.</Box>
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 520, mx: 'auto' }}>
          Shorten, brand, and track your links with high-performance analytics and lightning-fast redirection.
        </Typography>
      </Box>

      <Paper sx={{ p: 3, borderRadius: 3, mb: 5, maxWidth: 800, mx: 'auto', border: '1px solid', borderColor: 'divider' }}>
        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <TextField
            fullWidth
            label="Paste your long URL"
            placeholder="https://example.com/very-long-url"
            value={longUrl}
            onChange={(e) => setLongUrl(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                e.preventDefault();
                handleShorten();
              }
            }}
            sx={{ flex: 1, minWidth: 240 }}
          />
          <Button
            variant="contained"
            size="large"
            startIcon={<LinkIcon />}
            onClick={handleShorten}
            disabled={loading}
            sx={{ px: 4, minHeight: 56 }}
          >
            Shorten
          </Button>
        </Box>

        <Box sx={{ mt: 3, display: 'flex', flexDirection: 'column', gap: 2 }}>
          {/* Custom Alias Config */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
            <FormControlLabel
              control={<Checkbox checked={useCustomAlias} onChange={(e) => setUseCustomAlias(e.target.checked)} size="small" />}
              label={<Typography variant="body2">Custom Alias</Typography>}
            />
            {useCustomAlias && (
              <TextField
                size="small"
                label="Alias"
                placeholder="my-custom-code"
                value={customAlias}
                onChange={(e) => setCustomAlias(e.target.value)}
                sx={{ minWidth: 200 }}
              />
            )}
          </Box>

          {/* Expiration Config */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
            <FormControlLabel
              control={<Checkbox checked={generateQr} onChange={(e) => setGenerateQr(e.target.checked)} size="small" />}
              label={<Typography variant="body2">Generate QR Code</Typography>}
            />
            
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel>Expiration</InputLabel>
              <Select
                value={expiryOption}
                label="Expiration"
                onChange={(e) => setExpiryOption(e.target.value)}
              >
                <MenuItem value="never">Never (30 days default)</MenuItem>
                <MenuItem value="1h">1 Hour</MenuItem>
                <MenuItem value="1d">1 Day</MenuItem>
                <MenuItem value="7d">7 Days</MenuItem>
                <MenuItem value="30d">30 Days</MenuItem>
                <MenuItem value="custom">Custom Date</MenuItem>
              </Select>
            </FormControl>

            {expiryOption === 'custom' && (
              <TextField
                type="datetime-local"
                label="Select Date/Time"
                size="small"
                InputLabelProps={{ shrink: true }}
                value={customExpiryDate}
                onChange={(e) => setCustomExpiryDate(e.target.value)}
                sx={{ minWidth: 200 }}
              />
            )}
          </Box>
        </Box>

        <Box sx={{ display: 'flex', justifyContent: 'flex-end', alignItems: 'center', mt: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <InfoOutlinedIcon sx={{ fontSize: 16, color: 'text.secondary' }} />
            <Typography variant="caption" color="text.secondary">
              Pro users get unlimited custom aliases
            </Typography>
          </Box>
        </Box>

        {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
      </Paper>

      <Box sx={{ mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Box>
            <Typography variant="h6" fontWeight={700}>Recent Activity</Typography>
            <Typography variant="body2" color="text.secondary">Your latest 5 shortened links</Typography>
          </Box>
          <Button endIcon={<ArrowForwardIcon />} onClick={() => navigate('/links')}>
            View all links
          </Button>
        </Box>

        <Box sx={{ display: 'grid', gap: 1.5 }}>
          {initialLoading ? (
            Array.from(new Array(3)).map((_, idx) => (
              <Paper key={idx} sx={{ p: 2.5, borderRadius: 3, display: 'flex', alignItems: 'center', gap: 2, border: '1px solid', borderColor: 'divider' }}>
                <Skeleton variant="rounded" width={48} height={48} />
                <Box sx={{ flex: 1 }}>
                  <Skeleton variant="text" width="40%" height={24} />
                  <Skeleton variant="text" width="60%" height={20} />
                </Box>
                <Box sx={{ textAlign: 'center', minWidth: 80 }}>
                  <Skeleton variant="text" width={60} height={16} sx={{ mx: 'auto' }} />
                  <Skeleton variant="text" width={40} height={32} sx={{ mx: 'auto' }} />
                </Box>
                <Skeleton variant="rounded" width={120} height={32} />
              </Paper>
            ))
          ) : recent.length === 0 ? (
            <Paper sx={{ p: 3, borderRadius: 3, textAlign: 'center', border: '1px solid', borderColor: 'divider' }}>
              <Typography color="text.secondary">No links yet. Shorten your first URL above!</Typography>
            </Paper>
          ) : (
            recent.map((link, i) => (
            <Paper key={link.shortCode} sx={{ p: 2.5, borderRadius: 3, display: 'flex', alignItems: 'center', gap: 2, border: '1px solid', borderColor: 'divider' }}>
              <Box sx={{
                width: 48, height: 48, borderRadius: 2, bgcolor: 'secondary.main',
                display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'primary.main',
              }}>
                {linkIcons[i % linkIcons.length]}
              </Box>
              <Box sx={{ flex: 1, minWidth: 0 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography 
                    variant="subtitle2" 
                    color="primary.main" 
                    fontWeight={700} 
                    noWrap
                    sx={{ 
                      cursor: 'pointer',
                      '&:hover': { textDecoration: 'underline' }
                    }}
                    onClick={() => window.open(link.shortUrl, '_blank', 'noopener,noreferrer')}
                  >
                    {link.shortUrl.replace(/^https?:\/\//, '')}
                  </Typography>
                  <IconButton size="small" onClick={() => {
                    setQrViewUrl(link.shortUrl);
                    setQrViewOpen(true);
                  }} title="QR Code" sx={{ p: 0.5 }}>
                    <QrCode2Icon sx={{ fontSize: 16 }} />
                  </IconButton>
                  <IconButton size="small" onClick={() => handleCopy(link.shortUrl)} title="Copy" sx={{ p: 0.5 }}>
                    <ContentCopyIcon sx={{ fontSize: 16 }} />
                  </IconButton>
                </Box>
                <Typography variant="caption" color="text.secondary" noWrap display="block">
                  {link.longUrl}
                </Typography>
              </Box>
              <Box sx={{ textAlign: 'center', minWidth: 80 }}>
                <Typography variant="caption" color="text.secondary" display="block">TOTAL CLICKS</Typography>
                <Typography variant="h6" fontWeight={800}>{formatClicks(link.clickCount)}</Typography>
              </Box>
              <Box sx={{ display: 'flex', gap: 0.5 }}>
                <IconButton size="small" onClick={() => navigate(`/analytics/${link.shortCode}`)} title="Analytics">
                  <BarChartIcon fontSize="small" />
                </IconButton>
                <IconButton size="small" onClick={() => handleDelete(link.shortCode)} title="Delete" color="error">
                  <DeleteOutlineIcon fontSize="small" />
                </IconButton>
              </Box>
            </Paper>
            ))
          )}
        </Box>
      </Box>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr 1fr' }, gap: 2 }}>
        {initialLoading ? (
          Array.from(new Array(3)).map((_, idx) => (
            <Paper key={idx} sx={{ p: 3, borderRadius: 3, border: '1px solid', borderColor: 'divider' }}>
              <Skeleton variant="circular" width={32} height={32} sx={{ mb: 1 }} />
              <Skeleton variant="text" width={100} height={20} />
              <Skeleton variant="text" width={80} height={50} sx={{ my: 0.5 }} />
              <Skeleton variant="text" width={120} height={20} />
            </Paper>
          ))
        ) : (
          <>
            <Paper sx={{
              p: 3, borderRadius: 3,
              background: 'linear-gradient(135deg, #4F46E5 0%, #6366F1 100%)',
              color: '#fff', position: 'relative', overflow: 'hidden',
            }}>
              <RocketLaunchIcon sx={{ mb: 1 }} />
              <Typography variant="caption" sx={{ opacity: 0.8 }}>UPGRADED</Typography>
              <Typography variant="h3" fontWeight={800} sx={{ my: 0.5 }}>
                {summary?.uptimePercentage ?? 99.9}%
              </Typography>
              <Typography variant="body2" sx={{ opacity: 0.85 }}>Link Uptime Status</Typography>
            </Paper>

            <Paper sx={{ p: 3, borderRadius: 3, bgcolor: '#EEF2FF', border: '1px solid', borderColor: '#C7D2FE' }}>
              <BoltIcon sx={{ color: 'primary.main', mb: 1 }} />
              <Typography variant="h3" fontWeight={800} color="primary.main">
                {summary?.avgRedirectLatencyMs ?? 12}ms
              </Typography>
              <Typography variant="body2" color="text.secondary">Avg. Redirect Latency</Typography>
            </Paper>

            <Paper sx={{ p: 3, borderRadius: 3, position: 'relative', overflow: 'hidden', border: '1px solid', borderColor: 'divider' }}>
              <QrCode2Icon sx={{ color: 'primary.main', mb: 1 }} />
              <Typography variant="h3" fontWeight={800}>
                {formatClicks(summary?.qrScansThisMonth ?? 0)}
              </Typography>
              <Typography variant="body2" color="text.secondary">QR Scans this month</Typography>
              <QrCode2Icon sx={{
                position: 'absolute', right: -10, bottom: -10, fontSize: 100,
                opacity: 0.06, color: 'primary.main',
              }} />
            </Paper>
          </>
        )}
      </Box>

      {/* Shorten Success dialog with QR Code */}
      <Dialog open={successOpen} onClose={() => setSuccessOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 700, textAlign: 'center' }}>Link Shortened!</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3, py: 3 }}>
          <Typography variant="body2" color="text.secondary" align="center">
            Your URL was successfully shortened and QR code has been generated.
          </Typography>
          <Box sx={{ p: 2, bgcolor: '#fff', border: '1px solid', borderColor: 'divider', borderRadius: 2 }}>
            <QRCode value={newUrl} size={180} />
          </Box>
          <Typography variant="h6" color="primary.main" fontWeight={700} align="center">
            {newUrl.replace(/^https?:\/\//, '')}
          </Typography>
        </DialogContent>
        <DialogActions sx={{ justifyContent: 'center', px: 3, pb: 3, gap: 1.5 }}>
          <Button variant="outlined" startIcon={<ContentCopyIcon />} onClick={() => handleCopy(newUrl)}>
            Copy Link
          </Button>
          <Button variant="contained" onClick={() => setSuccessOpen(false)}>
            Close
          </Button>
        </DialogActions>
      </Dialog>

      {/* View QR Code Dialog */}
      <Dialog open={qrViewOpen} onClose={() => setQrViewOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 700, textAlign: 'center' }}>QR Code</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2, py: 3 }}>
          <Box sx={{ p: 2, bgcolor: '#fff', border: '1px solid', borderColor: 'divider', borderRadius: 2 }}>
            <QRCode value={qrViewUrl} size={200} />
          </Box>
          <Typography variant="subtitle2" color="text.secondary" align="center">
            Scan to navigate to:
          </Typography>
          <Typography variant="h6" color="primary.main" fontWeight={700} align="center">
            {qrViewUrl.replace(/^https?:\/\//, '')}
          </Typography>
        </DialogContent>
        <DialogActions sx={{ justifyContent: 'center', px: 3, pb: 3 }}>
          <Button variant="contained" onClick={() => setQrViewOpen(false)}>
            Done
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
