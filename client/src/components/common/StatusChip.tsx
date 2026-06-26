import { Chip } from '@mui/material';
import LockIcon from '@mui/icons-material/Lock';

interface StatusChipProps {
  status: string;
}

export default function StatusChip({ status }: StatusChipProps) {
  const normalized = status.toLowerCase();

  if (normalized === 'active') {
    return (
      <Chip
        label="Active"
        size="small"
        sx={{ bgcolor: '#ECFDF5', color: '#059669', fontWeight: 600, fontSize: '0.75rem' }}
      />
    );
  }

  if (normalized === 'private') {
    return (
      <Chip
        icon={<LockIcon sx={{ fontSize: '14px !important' }} />}
        label="Private"
        size="small"
        sx={{ bgcolor: '#EEF2FF', color: '#4F46E5', fontWeight: 600, fontSize: '0.75rem' }}
      />
    );
  }

  return (
    <Chip
      label="Expired"
      size="small"
      sx={{ bgcolor: '#FEF2F2', color: '#DC2626', fontWeight: 600, fontSize: '0.75rem' }}
    />
  );
}
