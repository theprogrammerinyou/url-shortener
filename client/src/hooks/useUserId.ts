import { useEffect, useState } from 'react';

const hashString = async (value: string) => {
  const encoder = new TextEncoder();
  const data = encoder.encode(value);
  const hashBuffer = await crypto.subtle.digest('SHA-256', data);
  return Array.from(new Uint8Array(hashBuffer))
    .map((b) => b.toString(16).padStart(2, '0'))
    .join('')
    .slice(0, 16);
};

export function useUserId() {
  const [userId, setUserId] = useState('');

  useEffect(() => {
    const initialize = async () => {
      const stored = window.localStorage.getItem('url-shortener-user-id');
      if (stored) {
        setUserId(stored);
        return;
      }

      try {
        const ipResponse = await fetch('https://api.ipify.org?format=json');
        const ipData = await ipResponse.json();
        const fingerprint = `${navigator.userAgent}|${navigator.language}|${ipData.ip}`;
        const hashed = await hashString(fingerprint);
        window.localStorage.setItem('url-shortener-user-id', hashed);
        setUserId(hashed);
      } catch {
        const fallback = await hashString(`${navigator.userAgent}|${navigator.language}|${Date.now()}`);
        window.localStorage.setItem('url-shortener-user-id', fallback);
        setUserId(fallback);
      }
    };

    initialize();
  }, []);

  return userId;
}
