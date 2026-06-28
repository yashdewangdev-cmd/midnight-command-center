import { useState, useCallback, createContext, useContext, useEffect, type ReactNode } from 'react';
import api from '../services/api';
import type { AuthResponse, LoginRequest, RegisterRequest, UserProfileDto } from '../types';

interface AuthContextType {
  user: UserProfileDto | null;
  token: string | null;
  isAuthenticated: boolean;
  loading: boolean;
  login: (request: LoginRequest) => Promise<void>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserProfileDto | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  // Hydrate from localStorage on mount
  useEffect(() => {
    const savedToken = localStorage.getItem('auth_token');
    const savedProfile = localStorage.getItem('user_profile');

    if (savedToken && savedProfile) {
      setToken(savedToken);
      try {
        setUser(JSON.parse(savedProfile));
      } catch {
        localStorage.removeItem('user_profile');
      }
    }
    setLoading(false);
  }, []);

  const handleAuthSuccess = useCallback((data: AuthResponse) => {
    localStorage.setItem('auth_token', data.token);
    localStorage.setItem('refresh_token', data.refreshToken);
    localStorage.setItem('user_profile', JSON.stringify(data.userProfile));
    setToken(data.token);
    setUser(data.userProfile);
  }, []);

  const login = useCallback(async (request: LoginRequest) => {
    const { data } = await api.post<AuthResponse>('/api/auth/login', request);
    handleAuthSuccess(data);
  }, [handleAuthSuccess]);

  const register = useCallback(async (request: RegisterRequest) => {
    const { data } = await api.post<AuthResponse>('/api/auth/register', request);
    handleAuthSuccess(data);
  }, [handleAuthSuccess]);

  const logout = useCallback(() => {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user_profile');
    setToken(null);
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: !!token,
        loading,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
