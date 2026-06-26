import { Paper, Typography, Box } from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';

interface StatCardProps {
  title: string;
  value: string;
  caption?: string;
  icon?: React.ReactNode;
  accent?: boolean;
}

export default function StatCard({ title, value, caption, icon, accent }: StatCardProps) {
  return (
    <Paper sx={{ p: 2.5, borderRadius: 3, height: '100%' }}>
      <Typography variant="body2" color="text.secondary" fontWeight={500}>
        {title}
      </Typography>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}>
        {icon}
        <Typography variant="h4" fontWeight={800} color={accent ? 'primary.main' : 'text.primary'}>
          {value}
        </Typography>
      </Box>
      {caption && (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 1 }}>
          <TrendingUpIcon sx={{ fontSize: 14, color: 'success.main' }} />
          <Typography variant="caption" color="success.main" fontWeight={600}>
            {caption}
          </Typography>
        </Box>
      )}
    </Paper>
  );
}
