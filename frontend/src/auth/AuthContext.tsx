import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { api, hasAccessToken, setAccessToken } from '../api/client';
import type { User } from '../types';

type AuthContextValue = { user: User | null; loading: boolean; login: (email: string, password: string) => Promise<void>; logout: () => void };
const AuthContext = createContext<AuthContextValue | null>(null);
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  useEffect(() => {
    if (!hasAccessToken()) { setLoading(false); return; }
    api.me().then(setUser).catch(() => setAccessToken(null)).finally(() => setLoading(false));
  }, []);
  const value = useMemo(() => ({ user, loading, login: async (email: string, password: string) => { const response = await api.login({ email, password }); setAccessToken(response.accessToken); setUser(response.user); }, logout: () => { setAccessToken(null); setUser(null); } }), [user, loading]);
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
export const useAuth = () => { const value = useContext(AuthContext); if (!value) throw new Error('useAuth must be used inside AuthProvider'); return value; };
