import { useAuth } from '../hooks/useAuth';
import { WorkTimer } from '../components/features/WorkTimer';
import { TaskGrid } from '../components/features/TaskGrid';
import { WarningMessage } from '../components/common/WarningMessage';
import { useApi } from '../hooks/useApi';
import type { TaskItemDto } from '../types';
import api from '../services/api';
import './Dashboard.css';

/**
 * Productivity dashboard with real-time timers, task grid,
 * and daily stats widgets.
 */
export function Dashboard() {
  const { user } = useAuth();
  const { data: odataResponse, loading, refetch } = useApi<{ value: TaskItemDto[] }>(
    '/odata/Tasks?$expand=WorkSessions&$orderby=CreatedAt desc&$top=50'
  );
  const tasks = odataResponse?.value ?? (Array.isArray(odataResponse) ? odataResponse as unknown as TaskItemDto[] : []);

  const totalHours = tasks.reduce((sum, t) => sum + t.totalHoursLogged, 0);
  const activeTasks = tasks.filter((t) => t.hasActiveSession).length;
  const completedToday = tasks.filter(
    (t) => t.completedAt && new Date(t.completedAt).toDateString() === new Date().toDateString()
  ).length;

  const handleStartSession = async (taskId: string) => {
    try {
      await api.post(`/odata/Tasks(${taskId})/StartSession`);
      refetch();
    } catch (err) {
      console.error('Failed to start session:', err);
    }
  };

  const handleStopSession = async (taskId: string) => {
    try {
      await api.post(`/odata/Tasks(${taskId})/StopSession`, {});
      refetch();
    } catch (err) {
      console.error('Failed to stop session:', err);
    }
  };

  return (
    <div className="dashboard">
      <header className="dashboard__header animate-fade-in">
        <div>
          <h1 className="dashboard__greeting">
            Welcome back, <span className="dashboard__name">{user?.displayName || 'Developer'}</span>
          </h1>
          <p className="dashboard__subtitle">Here's your productivity snapshot for today</p>
        </div>
      </header>

      {/* ─── Stats Cards ──────────────────────────────────── */}
      <div className="dashboard__stats animate-slide-up">
        <div className="stat-card glass-card">
          <span className="stat-card__icon">⏱️</span>
          <div className="stat-card__data">
            <span className="stat-card__value">{totalHours.toFixed(1)}h</span>
            <span className="stat-card__label">Total Hours</span>
          </div>
        </div>
        <div className="stat-card glass-card">
          <span className="stat-card__icon">🔥</span>
          <div className="stat-card__data">
            <span className="stat-card__value">{activeTasks}</span>
            <span className="stat-card__label">Active Sessions</span>
          </div>
        </div>
        <div className="stat-card glass-card">
          <span className="stat-card__icon">✅</span>
          <div className="stat-card__data">
            <span className="stat-card__value">{completedToday}</span>
            <span className="stat-card__label">Completed Today</span>
          </div>
        </div>
        <div className="stat-card glass-card">
          <span className="stat-card__icon">📋</span>
          <div className="stat-card__data">
            <span className="stat-card__value">{tasks.length}</span>
            <span className="stat-card__label">Total Tasks</span>
          </div>
        </div>
      </div>

      {/* ─── Timer & Warning ──────────────────────────────── */}
      <div className="dashboard__content">
        <div className="dashboard__left">
          <WorkTimer id="work-timer-main" taskTitle="Active work session" />

          <WarningMessage type="info" title="Tip: Multi-Session Tracking" id="dashboard-tip">
            You can run multiple work sessions across different tasks.
            The cumulative daily time is calculated across all disconnected
            sessions, even those spanning midnight boundaries.
            Start a session from the task grid to begin tracking.
          </WarningMessage>
        </div>

        <div className="dashboard__right">
          {loading ? (
            <div className="dashboard__loading">
              <span className="dashboard__spinner" />
              Loading tasks...
            </div>
          ) : (
            <TaskGrid
              id="task-grid-main"
              tasks={tasks}
              onStartSession={handleStartSession}
              onStopSession={handleStopSession}
            />
          )}
        </div>
      </div>
    </div>
  );
}
