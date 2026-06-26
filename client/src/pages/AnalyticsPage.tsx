import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Box, Typography, Paper, Button, Breadcrumbs, Link, LinearProgress,
  ToggleButton, ToggleButtonGroup, IconButton, Skeleton
} from '@mui/material';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import LinkIcon from '@mui/icons-material/Link';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import BoltIcon from '@mui/icons-material/Bolt';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import PublicIcon from '@mui/icons-material/Public';
import ShareIcon from '@mui/icons-material/Share';
import DownloadIcon from '@mui/icons-material/Download';
import NotificationsNoneIcon from '@mui/icons-material/NotificationsNone';
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts';
import { analyticsApi, urlApi } from '../api/client';
import type {
  GeoItem, LinkAnalyticsDetailResponse, ReferrerItem, ClickTimePoint, UrlDetailsResponse,
} from '../api/types';
import { useApp } from '../context/AppContext';

const referrerIcons: Record<string, React.ReactNode> = {
  'Direct / Unknown': <PublicIcon />,
  'google.com': <TrendingUpIcon />,
  'twitter.com': <Typography sx={{ fontWeight: 700, fontSize: 14 }}>@</Typography>,
  'facebook.com': <ShareIcon />,
};

export default function AnalyticsPage() {
  const { shortCode } = useParams<{ shortCode?: string }>();
  const { userId, showSnackbar } = useApp();
  const navigate = useNavigate();
  const [links, setLinks] = useState<UrlDetailsResponse[]>([]);
  const [detail, setDetail] = useState<LinkAnalyticsDetailResponse | null>(null);
  const [clicks, setClicks] = useState<ClickTimePoint[]>([]);
  const [referrers, setReferrers] = useState<ReferrerItem[]>([]);
  const [geo, setGeo] = useState<GeoItem[]>([]);
  const [period, setPeriod] = useState<'daily' | 'weekly'>('daily');
  const [initialLoading, setInitialLoading] = useState(true);
  const activeCode = shortCode ?? links[0]?.shortCode;

  useEffect(() => {
    if (!userId) return;
    urlApi.getRecent(userId, 20).then((res) => setLinks(res.data)).catch(console.error);
  }, [userId]);

  useEffect(() => {
    if (!activeCode) return;
    Promise.all([
      analyticsApi.getDetail(activeCode),
      analyticsApi.getClicks(activeCode, period),
      analyticsApi.getReferrers(activeCode),
      analyticsApi.getGeo(activeCode),
    ]).then(([d, c, r, g]) => {
      setDetail(d.data);
      setClicks(c.data.points);
      setReferrers(r.data.referrers);
      setGeo(g.data.countries);
    }).catch(console.error)
    .finally(() => setInitialLoading(false));
  }, [activeCode, period]);

  if (!activeCode && links.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Typography variant="h5" gutterBottom>No links to analyze</Typography>
        <Typography color="text.secondary" sx={{ mb: 3 }}>Create a shortened link first to view analytics.</Typography>
        <Button variant="contained" onClick={() => navigate('/')}>Shorten a Link</Button>
      </Box>
    );
  }

  if (initialLoading) {
    return (
      <Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 4, flexWrap: 'wrap', gap: 2 }}>
          <Box>
            <Skeleton variant="text" width={250} height={40} />
            <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', mt: 1 }}>
              <Skeleton variant="text" width={100} height={20} />
              <Skeleton variant="text" width={100} height={20} />
              <Skeleton variant="text" width={100} height={20} />
            </Box>
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Skeleton variant="rounded" width={80} height={36} />
            <Skeleton variant="rounded" width={120} height={36} />
          </Box>
        </Box>
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr 1fr' }, gap: 2, mb: 3 }}>
          {Array.from(new Array(3)).map((_, idx) => (
            <Paper key={idx} sx={{ p: 3, borderRadius: 3 }}>
              <Skeleton variant="text" width={100} height={20} />
              <Skeleton variant="text" width={60} height={40} sx={{ my: 1 }} />
              <Skeleton variant="text" width={120} height={20} />
            </Paper>
          ))}
        </Box>
        <Paper sx={{ p: 3, borderRadius: 3, mb: 3 }}>
          <Skeleton variant="text" width={150} height={32} />
          <Skeleton variant="rectangular" width="100%" height={250} sx={{ mt: 2 }} />
        </Paper>
      </Box>
    );
  }

  if (!detail) return null;

  const handleCopy = async () => {
    await navigator.clipboard.writeText(detail.shortUrl);
    showSnackbar('Link copied!');
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 3 }}>
        <Breadcrumbs sx={{ fontSize: '0.75rem', letterSpacing: '0.05em' }}>
          <Link underline="hover" color="text.secondary" sx={{ cursor: 'pointer' }} onClick={() => navigate('/analytics')}>ANALYTICS</Link>
          <Typography color="text.primary" fontWeight={600} fontSize="0.75rem">
            {activeCode?.toUpperCase()}
          </Typography>
        </Breadcrumbs>
        <IconButton><NotificationsNoneIcon /></IconButton>
      </Box>

      {!shortCode && links.length > 1 && (
        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 3 }}>
          {links.map((l) => (
            <Button
              key={l.shortCode}
              size="small"
              variant={l.shortCode === activeCode ? 'contained' : 'outlined'}
              onClick={() => navigate(`/analytics/${l.shortCode}`)}
            >
              {l.shortCode}
            </Button>
          ))}
        </Box>
      )}

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 4, flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h4" fontWeight={800} sx={{ mb: 1 }}>
            {detail.shortUrl.replace(/^https?:\/\//, '')}
          </Typography>
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <LinkIcon sx={{ fontSize: 16, color: 'text.secondary' }} />
              <Typography variant="body2" color="text.secondary">{detail.longUrl}</Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <CalendarTodayIcon sx={{ fontSize: 16, color: 'text.secondary' }} />
              <Typography variant="body2" color="text.secondary">
                Created {new Date(detail.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: detail.isActive ? 'primary.main' : 'error.main' }} />
              <Typography variant="body2" color="text.secondary">{detail.isActive ? 'Active' : 'Expired'}</Typography>
            </Box>
          </Box>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" startIcon={<EditOutlinedIcon />}>Edit</Button>
          <Button variant="contained" startIcon={<ContentCopyIcon />} onClick={handleCopy}>Copy Link</Button>
        </Box>
      </Box>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr 1fr' }, gap: 2, mb: 3 }}>
        <Paper sx={{ p: 3, borderRadius: 3 }}>
          <Typography variant="body2" color="text.secondary">Total Engagement</Typography>
          <Typography variant="h3" fontWeight={800}>{detail.totalEngagement.toLocaleString()}</Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 1 }}>
            <TrendingUpIcon sx={{ fontSize: 14, color: 'success.main' }} />
            <Typography variant="caption" color="success.main" fontWeight={600}>
              +{detail.engagementGrowthPercentage}% from last month
            </Typography>
          </Box>
        </Paper>
        <Paper sx={{ p: 3, borderRadius: 3 }}>
          <Typography variant="body2" color="text.secondary">Conversion Rate</Typography>
          <Typography variant="h3" fontWeight={800}>{detail.conversionRate}%</Typography>
          <Typography variant="caption" color="text.secondary">{detail.conversions} conversions</Typography>
          <LinearProgress variant="determinate" value={detail.conversionRate * 10} sx={{ mt: 2, height: 4, borderRadius: 2 }} />
        </Paper>
        <Paper sx={{ p: 3, borderRadius: 3, display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box sx={{ width: 48, height: 48, borderRadius: 2, bgcolor: 'primary.main', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <BoltIcon sx={{ color: '#fff' }} />
          </Box>
          <Box>
            <Typography variant="subtitle2" fontWeight={700}>Instant Redirect</Typography>
            <Typography variant="caption" color="text.secondary">Global edge latency &lt; 20ms</Typography>
          </Box>
        </Paper>
      </Box>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '2fr 1fr' }, gap: 3, mb: 3 }}>
        <Paper sx={{ p: 3, borderRadius: 3 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
            <Box>
              <Typography variant="h6">Clicks Over Time</Typography>
              <Typography variant="caption" color="text.secondary">Last 30 days performance</Typography>
            </Box>
            <ToggleButtonGroup size="small" value={period} exclusive onChange={(_, v) => v && setPeriod(v)}>
              <ToggleButton value="daily">Daily</ToggleButton>
              <ToggleButton value="weekly">Weekly</ToggleButton>
            </ToggleButtonGroup>
          </Box>
          <ResponsiveContainer width="100%" height={220}>
            <AreaChart data={clicks}>
              <defs>
                <linearGradient id="clickGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#4F46E5" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="#4F46E5" stopOpacity={0} />
                </linearGradient>
              </defs>
              <XAxis dataKey="label" tick={{ fontSize: 11 }} axisLine={false} tickLine={false} />
              <YAxis hide />
              <Tooltip />
              <Area type="monotone" dataKey="clicks" stroke="#4F46E5" fill="url(#clickGrad)" strokeWidth={2} />
            </AreaChart>
          </ResponsiveContainer>
        </Paper>

        <Paper sx={{ p: 3, borderRadius: 3 }}>
          <Typography variant="h6" sx={{ mb: 2 }}>Top Referrers</Typography>
          {referrers.map((ref) => (
            <Box key={ref.name} sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 2 }}>
              <Box sx={{
                width: 36, height: 36, borderRadius: 2, bgcolor: 'secondary.main',
                display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'primary.main',
              }}>
                {referrerIcons[ref.name] ?? <PublicIcon />}
              </Box>
              <Box sx={{ flex: 1 }}>
                <Typography variant="body2" fontWeight={600}>{ref.name}</Typography>
                <Typography variant="caption" color="text.secondary">{ref.percentage}%</Typography>
              </Box>
              <Typography variant="body2" fontWeight={700}>{ref.clicks.toLocaleString()}</Typography>
            </Box>
          ))}
          <Button size="small" sx={{ mt: 1 }}>View All Referrers</Button>
        </Paper>
      </Box>

      <Paper sx={{ p: 3, borderRadius: 3, position: 'relative' }}>
        <Typography variant="h6">Audience Distribution</Typography>
        <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 3 }}>
          Traffic source by geographical location
        </Typography>
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 3 }}>
          <Box>
            {geo.map((country) => (
              <Box key={country.country} sx={{ mb: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                  <Typography variant="body2" fontWeight={600}>
                    {country.countryCode} {country.country}
                  </Typography>
                  <Typography variant="body2" fontWeight={700}>
                    {country.clicks.toLocaleString()} ({country.percentage}%)
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={country.percentage}
                  sx={{ height: 4, borderRadius: 2, bgcolor: 'secondary.main' }}
                />
              </Box>
            ))}
          </Box>
          <Box sx={{
            bgcolor: 'action.hover', borderRadius: 3, minHeight: 200,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
          }}>
            <Button variant="outlined">Map View</Button>
          </Box>
        </Box>
        <Button
          variant="contained"
          startIcon={<DownloadIcon />}
          sx={{ position: 'absolute', bottom: 24, right: 24 }}
        >
          Export Report
        </Button>
      </Paper>
    </Box>
  );
}
