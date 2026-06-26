import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';

// ── Suppress the harmless ResizeObserver "loop" notification ──────────────
// This is a known benign browser notification (not a real error) triggered by
// MUI charts/animations when they resize faster than the observer can deliver.
// It pollutes the dev overlay and console but has no functional impact.
const _originalError = window.onerror;
window.onerror = (message, ...rest) => {
  if (typeof message === 'string' && message.includes('ResizeObserver loop')) {
    return true; // suppress — returning true prevents default error handling
  }
  return _originalError ? _originalError(message, ...rest) : false;
};

// Also suppress it from the React dev overlay's unhandledrejection path
const _originalConsoleError = console.error;
console.error = (...args: unknown[]) => {
  if (
    typeof args[0] === 'string' &&
    args[0].includes('ResizeObserver loop')
  ) return;
  _originalConsoleError(...args);
};

const rootElement = document.getElementById('root');
if (!rootElement) throw new Error('Root element not found');

const root = ReactDOM.createRoot(rootElement);
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
