import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AppProvider } from './context/AppContext';
import AppLayout from './components/layout/AppLayout';
import GuestLayout from './components/layout/GuestLayout';
import LoginPage from './pages/LoginPage';
import ShortenPage from './pages/ShortenPage';
import LinksPage from './pages/LinksPage';
import AnalyticsPage from './pages/AnalyticsPage';
import SettingsPage from './pages/SettingsPage';
import PricingPage from './pages/PricingPage';
import './App.css';

function App() {
  return (
    <AppProvider>
      <BrowserRouter>
        <Routes>
          {/* Authenticated routes (sidebar layout) */}
          <Route element={<AppLayout />}>
            <Route path="/" element={<ShortenPage />} />
            <Route path="/links" element={<LinksPage />} />
            <Route path="/analytics" element={<AnalyticsPage />} />
            <Route path="/analytics/:shortCode" element={<AnalyticsPage />} />
            <Route path="/settings" element={<SettingsPage />} />
            <Route path="/pricing" element={<PricingPage />} />
          </Route>

          {/* Guest / public routes (no sidebar) */}
          <Route element={<GuestLayout />}>
            <Route path="/login" element={<LoginPage />} />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AppProvider>
  );
}

export default App;
