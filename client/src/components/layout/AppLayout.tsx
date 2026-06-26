import { Box, IconButton } from '@mui/material';
import { Outlet } from 'react-router-dom';
import { useState } from 'react';
import MenuIcon from '@mui/icons-material/Menu';
import Sidebar from './Sidebar';

/**
 * AppLayout — shown for all users (logged in and guest).
 * Sidebar is always present; it adapts based on login state.
 */
export default function AppLayout() {
  const [sidebarVisible, setSidebarVisible] = useState(true);

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      {/* Sidebar always visible */}
      {sidebarVisible && (
        <Sidebar onCollapse={() => setSidebarVisible(false)} />
      )}

      {/* Floating menu button when sidebar is hidden */}
      {!sidebarVisible && (
        <IconButton
          onClick={() => setSidebarVisible(true)}
          sx={{
            position: 'fixed',
            left: 16,
            top: 16,
            zIndex: 1100,
            bgcolor: 'background.paper',
            border: '1px solid',
            borderColor: 'divider',
            boxShadow: '0 1px 3px rgba(0,0,0,0.1)',
            '&:hover': { bgcolor: 'action.hover' }
          }}
        >
          <MenuIcon />
        </IconButton>
      )}

      {/* Main content — no top header, no padding offset issues */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: { xs: 2, md: 4 },
          minHeight: '100vh',
          bgcolor: 'background.default',
          pt: !sidebarVisible ? 8 : { xs: 2, md: 4 },
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
}
