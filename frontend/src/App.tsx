import { BrowserRouter, Routes, Route, NavLink, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './hooks/useAuth';
import { Dashboard } from './pages/Dashboard';
import { Portfolio } from './pages/Portfolio';
import { Reports } from './pages/Reports';
import { Login } from './pages/Login';
import type { ReactNode } from 'react';
import './App.css';

function ProtectedRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div className="app-loading">
        <span className="app-loading__spinner" />
        <span>Loading...</span>
      </div>
    );
  }

  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}

function AppNav() {
  const { isAuthenticated, user, logout } = useAuth();

  return (
    <nav className="app-nav glass-card" id="main-nav">
      <div className="app-nav__brand">
        <span className="app-nav__logo">◈</span>
        <span className="app-nav__title">Midnight Command</span>
      </div>

      <div className="app-nav__links">
        <NavLink to="/portfolio" className="app-nav__link" id="nav-portfolio">
          Portfolio
        </NavLink>

        {isAuthenticated && (
          <>
            <NavLink to="/dashboard" className="app-nav__link" id="nav-dashboard">
              Dashboard
            </NavLink>
            <NavLink to="/reports" className="app-nav__link" id="nav-reports">
              Reports
            </NavLink>
          </>
        )}
      </div>

      <div className="app-nav__right">
        {isAuthenticated ? (
          <div className="app-nav__user">
            <span className="app-nav__user-name">{user?.displayName}</span>
            <button className="btn btn-secondary btn-sm" onClick={logout} id="nav-logout">
              Sign Out
            </button>
          </div>
        ) : (
          <NavLink to="/login" className="btn btn-primary btn-sm" id="nav-login">
            Sign In
          </NavLink>
        )}
      </div>
    </nav>
  );
}

function AppRoutes() {
  return (
    <>
      <AppNav />
      <main className="app-main">
        <Routes>
          <Route path="/" element={<Navigate to="/portfolio" replace />} />
          <Route path="/portfolio" element={<Portfolio />} />
          <Route path="/login" element={<Login />} />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/reports"
            element={
              <ProtectedRoute>
                <Reports />
              </ProtectedRoute>
            }
          />
        </Routes>
      </main>
    </>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  );
}
