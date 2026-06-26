import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Typography, Paper, TextField, Button, InputAdornment,
  Tabs, Tab, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  IconButton, Pagination, LinearProgress, Skeleton
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import AddIcon from '@mui/icons-material/Add';
import FilterListIcon from '@mui/icons-material/FilterList';
import BarChartIcon from '@mui/icons-material/BarChart';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import EmojiEventsIcon from '@mui/icons-material/EmojiEvents';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import { dashboardApi, urlApi } from '../api/client';
import type { DashboardStatsResponse, LinkVelocityResponse, PagedLinksResponse } from '../api/types';
import StatCard from '../components/common/StatCard';
import StatusChip from '../components/common/StatusChip';
import { useApp } from '../context/AppContext';

function formatNumber(n: number) {
  if (n >= 1000) return `${(n / 1000).toFixed(1).replace(/\.0$/, '')}k`;
  return n.toString();
}

function CopyableShortUrl({ shortUrl }: { shortUrl: string }) {
  const [copied, setCopied] = useState(false);
  const handle = () => {
    navigator.clipboard.writeText(shortUrl).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    });
  };
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
      <Typography 
        variant="body2" 
        color="primary.main" 
        fontWeight={700} 
        noWrap 
        sx={{ 
          maxWidth: 200, 
          cursor: 'pointer',
          '&:hover': { textDecoration: 'underline' }
        }}
        onClick={() => window.open(shortUrl, '_blank', 'noopener,noreferrer')}
      >
        {shortUrl.replace(/^https?:\/\//, '')}
      </Typography>
      <IconButton size="small" onClick={handle} sx={{ p: 0.25, opacity: 0.5, '&:hover': { opacity: 1 } }}>
        {copied
          ? <ContentCopyIcon sx={{ fontSize: 13, color: 'success.main' }} />
          : <ContentCopyIcon sx={{ fontSize: 13 }} />}
      </IconButton>
    </Box>
  );
}




export default function LinksPage() {
  const { userId } = useApp();
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('all');
  const [page, setPage] = useState(1);
  const [stats, setStats] = useState<DashboardStatsResponse | null>(null);
  const [velocity, setVelocity] = useState<LinkVelocityResponse | null>(null);
  const [links, setLinks] = useState<PagedLinksResponse | null>(null);
  const [initialLoading, setInitialLoading] = useState(true);

  const load = async () => {
    if (!userId) return;
    try {
      const [statsRes, velocityRes, linksRes] = await Promise.all([
        dashboardApi.getStats(userId),
        dashboardApi.getVelocity(userId),
        urlApi.getPaged({ userId, search: search || undefined, status: status === 'all' ? undefined : status, page, pageSize: 10 }),
      ]);
      setStats(statsRes.data);
      setVelocity(velocityRes.data);
      setLinks(linksRes.data);
    } finally {
      setInitialLoading(false);
    }
  };

  useEffect(() => {
    load().catch(console.error);
  }, [userId, search, status, page]);

  const maxVelocity = Math.max(...(velocity?.points.map((p) => p.clicks) ?? [1]));

  return (
    <Box>
      <Box sx={{ display: 'flex', gap: 2, mb: 4, flexWrap: 'wrap', alignItems: 'center' }}>
        <TextField
          placeholder="Search your shortened links..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          sx={{ flex: 1, minWidth: 280 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon color="action" />
              </InputAdornment>
            ),
            sx: { bgcolor: 'background.paper', borderRadius: 2 },
          }}
        />
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => navigate('/')}>
          Create New
        </Button>
        <IconButton sx={{ border: '1px solid', borderColor: 'divider', borderRadius: 2 }}>
          <FilterListIcon />
        </IconButton>
      </Box>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr 1fr' }, gap: 2, mb: 4 }}>
        {initialLoading ? (
          Array.from(new Array(3)).map((_, idx) => (
            <Paper key={idx} sx={{ p: 3, borderRadius: 3, border: '1px solid', borderColor: 'divider' }}>
              <Skeleton variant="text" width={100} height={20} />
              <Skeleton variant="text" width={80} height={50} sx={{ my: 0.5 }} />
              <Skeleton variant="text" width={120} height={20} />
            </Paper>
          ))
        ) : (
          <>
            <StatCard
              title="Total Clicks"
              value={formatNumber(stats?.totalClicks ?? 0)}
              caption={`+${stats?.clickGrowthPercentage ?? 0}% from last month`}
            />
            <Paper sx={{ p: 2.5, borderRadius: 3 }}>
              <Typography variant="body2" color="text.secondary" fontWeight={500}>Active Links</Typography>
              <Typography variant="h4" fontWeight={800} sx={{ mt: 1 }}>
                {stats?.activeLinks ?? 0} / {stats?.linkLimit ?? 200} Limit
              </Typography>
              <LinearProgress
                variant="determinate"
                value={((stats?.activeLinks ?? 0) / (stats?.linkLimit ?? 200)) * 100}
                sx={{ mt: 2, height: 6, borderRadius: 3, bgcolor: 'secondary.main' }}
              />
            </Paper>
            <StatCard
              title="Top Referral"
              value={stats?.topReferral ?? 'Direct'}
              caption={`${stats?.topReferralPercentage ?? 0}% Traffic`}
              icon={<TrendingUpIcon color="primary" />}
            />
          </>
        )}
      </Box>

      <Paper sx={{ borderRadius: 3, mb: 4, overflow: 'hidden' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', px: 3, pt: 2 }}>
          <Typography variant="h6">Recent Links</Typography>
          <Tabs
            value={status}
            onChange={(_, v) => { setStatus(v); setPage(1); }}
            sx={{ minHeight: 36, '& .MuiTab-root': { minHeight: 36, py: 0.5 } }}
          >
            <Tab label="All" value="all" />
            <Tab label="Active" value="active" />
            <Tab label="Expired" value="expired" />
          </Tabs>
        </Box>

        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 700, color: 'text.secondary', fontSize: '0.75rem' }}>LINK INFO</TableCell>
                <TableCell sx={{ fontWeight: 700, color: 'text.secondary', fontSize: '0.75rem' }}>STATUS</TableCell>
                <TableCell sx={{ fontWeight: 700, color: 'text.secondary', fontSize: '0.75rem' }}>CLICKS</TableCell>
                <TableCell sx={{ fontWeight: 700, color: 'text.secondary', fontSize: '0.75rem' }}>DATE</TableCell>
                <TableCell sx={{ fontWeight: 700, color: 'text.secondary', fontSize: '0.75rem' }}>ACTIONS</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {initialLoading ? (
                Array.from(new Array(5)).map((_, idx) => (
                  <TableRow key={idx}>
                    <TableCell><Skeleton variant="text" width={200} /><Skeleton variant="text" width={150} /></TableCell>
                    <TableCell><Skeleton variant="rounded" width={80} height={24} /></TableCell>
                    <TableCell><Skeleton variant="text" width={40} /><Skeleton variant="text" width={60} /></TableCell>
                    <TableCell><Skeleton variant="text" width={80} /></TableCell>
                    <TableCell><Skeleton variant="circular" width={24} height={24} /></TableCell>
                  </TableRow>
                ))
              ) : (
                <>
                  {(links?.items ?? []).map((link) => (
                    <TableRow key={link.shortCode} hover>
                      <TableCell>
                        <CopyableShortUrl shortUrl={link.shortUrl} />
                        <Typography variant="caption" color="text.secondary" noWrap display="block" sx={{ maxWidth: 280 }}>
                          {link.longUrl}
                        </Typography>
                      </TableCell>
                      <TableCell><StatusChip status={link.status} /></TableCell>
                      <TableCell>
                        <Typography variant="body2" fontWeight={700}>{link.clickCount}</Typography>
                        {link.clicksToday > 0 ? (
                          <Typography variant="caption" color="success.main">+{link.clicksToday} today</Typography>
                        ) : link.isExpired ? (
                          <Typography variant="caption" color="text.secondary">
                            Ended {new Date(link.expiresAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}
                          </Typography>
                        ) : null}
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {new Date(link.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <IconButton size="small" title="View analytics" onClick={() => navigate(`/analytics/${link.shortCode}`)}>
                          <BarChartIcon fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                  {(links?.items ?? []).length === 0 && (
                    <TableRow>
                      <TableCell colSpan={5} align="center" sx={{ py: 4 }}>
                        <Typography color="text.secondary">No links found</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </>
              )}
            </TableBody>
          </Table>
        </TableContainer>

        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', px: 3, py: 2 }}>
          <Typography variant="caption" color="text.secondary">
            Showing {links ? Math.min((page - 1) * links.pageSize + 1, links.totalCount) : 0} to{' '}
            {links ? Math.min(page * links.pageSize, links.totalCount) : 0} of {links?.totalCount ?? 0} links
          </Typography>
          <Pagination
            count={links ? Math.ceil(links.totalCount / links.pageSize) : 1}
            page={page}
            onChange={(_, p) => setPage(p)}
            size="small"
          />
        </Box>
      </Paper>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 3 }}>
        <Paper sx={{
          p: 3, borderRadius: 3,
          background: 'linear-gradient(135deg, #EEF2FF 0%, #E0E7FF 100%)',
          color: '#1E1B4B',
        }}>
          <EmojiEventsIcon sx={{ color: 'primary.main', fontSize: 32, mb: 1 }} />
          <Typography variant="h6" fontWeight={700}>Unlimited Potential with Swft Pro</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ my: 1.5 }}>
            Get custom domains, team collaboration, and advanced analytics.
          </Typography>
          <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
            <Button variant="contained" size="small" onClick={() => navigate('/pricing')}>Start 14-day Trial</Button>
            <Button variant="outlined" size="small" sx={{ bgcolor: 'background.paper' }} onClick={() => navigate('/pricing')}>View Pricing</Button>
          </Box>
        </Paper>

        <Paper sx={{ p: 3, borderRadius: 3 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
            <Typography variant="h6">Link Velocity</Typography>
            <Typography variant="caption" color="text.secondary">Last 7 Days</Typography>
          </Box>
          <ResponsiveContainer width="100%" height={160}>
            <BarChart data={velocity?.points ?? []}>
              <XAxis dataKey="label" tick={{ fontSize: 12 }} axisLine={false} tickLine={false} />
              <YAxis hide />
              <Tooltip />
              <Bar dataKey="clicks" radius={[6, 6, 0, 0]}>
                {(velocity?.points ?? []).map((entry, index) => (
                  <Cell
                    key={entry.label}
                    fill={entry.clicks === maxVelocity ? '#4F46E5' : '#C7D2FE'}
                  />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </Paper>
      </Box>

      <Box sx={{ mt: 4, display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1 }}>
        <Typography variant="caption" color="text.secondary">© 2024 LinkSwift Inc.</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          {['Privacy Policy', 'Terms of Service', 'API Documentation', 'Status'].map((t) => (
            <Typography key={t} variant="caption" color="text.secondary" sx={{ cursor: 'pointer', '&:hover': { color: 'primary.main' } }}>
              {t}
            </Typography>
          ))}
        </Box>
      </Box>
    </Box>
  );
}
