import { useTimer } from '../../hooks/useTimer';
import './WorkTimer.css';

interface WorkTimerProps {
  taskTitle?: string;
  initialElapsed?: number;
  onStart?: () => void;
  onPause?: () => void;
  onStop?: () => void;
  id?: string;
}

/**
 * Real-time elapsed work timer with start/pause/stop controls.
 * Updates every second with a pulsing active indicator.
 */
export function WorkTimer({
  taskTitle,
  initialElapsed = 0,
  onStart,
  onPause,
  onStop,
  id,
}: WorkTimerProps) {
  const { elapsed, isRunning, formattedTime, start, pause, reset } = useTimer(initialElapsed);

  const handleStart = () => {
    start();
    onStart?.();
  };

  const handlePause = () => {
    pause();
    onPause?.();
  };

  const handleStop = () => {
    pause();
    onStop?.();
    reset();
  };

  return (
    <div id={id} className={`work-timer glass-card ${isRunning ? 'work-timer--active' : ''}`}>
      <div className="work-timer__display">
        {isRunning && <span className="work-timer__pulse" />}
        <span className="work-timer__time">{formattedTime}</span>
        {taskTitle && (
          <span className="work-timer__task">{taskTitle}</span>
        )}
      </div>

      <div className="work-timer__controls">
        {!isRunning ? (
          <button className="btn btn-success" onClick={handleStart}>
            ▶ Start
          </button>
        ) : (
          <button className="btn btn-secondary" onClick={handlePause}>
            ⏸ Pause
          </button>
        )}
        <button
          className="btn btn-danger"
          onClick={handleStop}
          disabled={elapsed === 0}
        >
          ⏹ Stop
        </button>
      </div>

      {elapsed > 0 && (
        <div className="work-timer__stats">
          <span className="work-timer__stat">
            {(elapsed / 3600).toFixed(2)}h logged
          </span>
        </div>
      )}
    </div>
  );
}
