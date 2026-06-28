import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { WarningMessage } from '../components/common/WarningMessage';
import './Login.css';

/**
 * Login / Register page with glassmorphism form.
 */
export function Login() {
  const [isRegister, setIsRegister] = useState(false);
  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const { login, register } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      if (isRegister) {
        await register({ displayName, email, password });
      } else {
        await login({ email, password });
      }
      navigate('/dashboard');
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } };
        setError(axiosErr.response?.data?.message || 'Authentication failed.');
      } else {
        setError('An unexpected error occurred.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-card glass-card animate-slide-up">
        <div className="login-card__header">
          <h1 className="login-card__title">
            {isRegister ? 'Create Account' : 'Welcome Back'}
          </h1>
          <p className="login-card__subtitle">
            {isRegister
              ? 'Join the Midnight Command Center'
              : 'Sign in to your productivity suite'}
          </p>
        </div>

        {error && (
          <WarningMessage type="error" id="login-error">
            {error}
          </WarningMessage>
        )}

        <form onSubmit={handleSubmit} className="login-card__form">
          {isRegister && (
            <div className="login-card__field">
              <label className="input-label" htmlFor="login-name">Display Name</label>
              <input
                id="login-name"
                type="text"
                className="input"
                placeholder="Your name"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                required
                minLength={2}
              />
            </div>
          )}

          <div className="login-card__field">
            <label className="input-label" htmlFor="login-email">Email</label>
            <input
              id="login-email"
              type="email"
              className="input"
              placeholder="you@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className="login-card__field">
            <label className="input-label" htmlFor="login-password">Password</label>
            <input
              id="login-password"
              type="password"
              className="input"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              minLength={6}
            />
          </div>

          <button
            type="submit"
            className="btn btn-primary btn-lg login-card__submit"
            disabled={loading}
          >
            {loading ? 'Processing...' : isRegister ? 'Create Account' : 'Sign In'}
          </button>
        </form>

        <div className="login-card__toggle">
          <span className="login-card__toggle-text">
            {isRegister ? 'Already have an account?' : "Don't have an account?"}
          </span>
          <button
            type="button"
            className="login-card__toggle-btn"
            onClick={() => { setIsRegister(!isRegister); setError(null); }}
          >
            {isRegister ? 'Sign In' : 'Register'}
          </button>
        </div>
      </div>
    </div>
  );
}
