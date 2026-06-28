import { TaskItemStatus, TaskPriority, SessionStatus } from '../../types';
import './StatusBadge.css';

type BadgeVariant = TaskItemStatus | TaskPriority | SessionStatus | 'FINISHED';

interface StatusBadgeProps {
  status: BadgeVariant;
  /** Context helps disambiguate shared status strings (e.g., "Completed") */
  context?: 'task' | 'session' | 'priority';
  label?: string;
  id?: string;
}

interface BadgeConfig {
  className: string;
  defaultLabel: string;
}

/**
 * Resolves badge styling based on status value and optional context.
 */
function getConfig(status: BadgeVariant, context?: string): BadgeConfig {
  // Explicit FINISHED variant
  if (status === 'FINISHED') return { className: 'badge--finished', defaultLabel: 'FINISHED' };

  // Priority values (unique strings)
  if (status === TaskPriority.Low) return { className: 'badge--low', defaultLabel: 'Low' };
  if (status === TaskPriority.Medium) return { className: 'badge--medium', defaultLabel: 'Medium' };
  if (status === TaskPriority.High) return { className: 'badge--high', defaultLabel: 'High' };
  if (status === TaskPriority.Critical) return { className: 'badge--critical', defaultLabel: 'Critical' };

  // Task statuses
  if (status === TaskItemStatus.Todo) return { className: 'badge--todo', defaultLabel: 'To Do' };
  if (status === TaskItemStatus.InProgress) return { className: 'badge--in-progress', defaultLabel: 'In Progress' };
  if (status === TaskItemStatus.Cancelled) return { className: 'badge--cancelled', defaultLabel: 'Cancelled' };

  // "Completed" — shared between TaskItemStatus and SessionStatus
  if (status === 'Completed') return { className: 'badge--completed', defaultLabel: 'Completed' };

  // Session-only statuses
  if (status === SessionStatus.Active && context === 'session') return { className: 'badge--active', defaultLabel: 'Active' };
  if (status === SessionStatus.Paused) return { className: 'badge--paused', defaultLabel: 'Paused' };

  // "Active" without session context maps to task Active (same as session Active style)
  if (status === 'Active') return { className: 'badge--active', defaultLabel: 'Active' };

  return { className: 'badge--default', defaultLabel: String(status) };
}

/**
 * Color-coded status badge with persistent visibility.
 * Uses :where() selector strategy to prevent grid selection
 * styles from overriding badge appearance.
 */
export function StatusBadge({ status, context, label, id }: StatusBadgeProps) {
  const config = getConfig(status, context);

  return (
    <span
      id={id}
      className={`status-badge ${config.className}`}
      data-status={status}
    >
      <span className="status-badge__dot" />
      {label || config.defaultLabel}
    </span>
  );
}
