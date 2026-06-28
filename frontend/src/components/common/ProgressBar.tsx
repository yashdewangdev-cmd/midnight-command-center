import './ProgressBar.css';

interface ProgressBarProps {
  progress: number; // 0-100
  label?: string;
  variant?: 'blue' | 'emerald' | 'amber';
  animated?: boolean;
  id?: string;
}

/**
 * Animated progress bar with percentage label.
 * Used for PDF generation progress tracking.
 */
export function ProgressBar({
  progress,
  label,
  variant = 'blue',
  animated = true,
  id,
}: ProgressBarProps) {
  const clampedProgress = Math.min(100, Math.max(0, progress));

  return (
    <div id={id} className="progress-bar-container">
      {(label || clampedProgress > 0) && (
        <div className="progress-bar-header">
          {label && <span className="progress-bar-label">{label}</span>}
          <span className="progress-bar-percentage">{Math.round(clampedProgress)}%</span>
        </div>
      )}
      <div className="progress-bar-track">
        <div
          className={`progress-bar-fill progress-bar-fill--${variant} ${animated ? 'progress-bar-fill--animated' : ''}`}
          style={{ width: `${clampedProgress}%` }}
          role="progressbar"
          aria-valuenow={clampedProgress}
          aria-valuemin={0}
          aria-valuemax={100}
        >
          {clampedProgress === 100 && (
            <span className="progress-bar-complete-icon">✓</span>
          )}
        </div>
      </div>
    </div>
  );
}
