import { Outlet } from 'react-router-dom';
import { Box } from '@mui/material';

/**
 * GuestLayout — bare wrapper with no sidebar/header.
 * Used for public routes like /login.
 */
export default function GuestLayout() {
  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <Outlet />
    </Box>
  );
}
