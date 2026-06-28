import { useState, useEffect, useCallback, useRef } from 'react';

interface TimerState {
  elapsed: number; // seconds
  isRunning: boolean;
  formattedTime: string;
}

/**
 * Custom hook for real-time work session timer.
 * Updates every second when running.
 */
export function useTimer(initialElapsed = 0): TimerState & {
  start: () => void;
  pause: () => void;
  reset: () => void;
} {
  const [elapsed, setElapsed] = useState(initialElapsed);
  const [isRunning, setIsRunning] = useState(false);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const formatTime = useCallback((totalSeconds: number): string => {
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    const pad = (n: number) => n.toString().padStart(2, '0');

    if (hours > 0) {
      return `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`;
    }
    return `${pad(minutes)}:${pad(seconds)}`;
  }, []);

  useEffect(() => {
    if (isRunning) {
      intervalRef.current = setInterval(() => {
        setElapsed((prev) => prev + 1);
      }, 1000);
    }

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, [isRunning]);

  const start = useCallback(() => setIsRunning(true), []);
  const pause = useCallback(() => setIsRunning(false), []);
  const reset = useCallback(() => {
    setIsRunning(false);
    setElapsed(0);
  }, []);

  return {
    elapsed,
    isRunning,
    formattedTime: formatTime(elapsed),
    start,
    pause,
    reset,
  };
}
