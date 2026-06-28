import { useState, useCallback, useRef } from 'react';
import api from '../services/api';

interface ReportProgressState {
  isGenerating: boolean;
  progress: number; // 0-100
  error: string | null;
}

/**
 * Hook for tracking PDF report generation progress.
 * Simulates progress while awaiting the backend response.
 */
export function useReportProgress() {
  const [state, setState] = useState<ReportProgressState>({
    isGenerating: false,
    progress: 0,
    error: null,
  });
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const startProgressSimulation = useCallback(() => {
    setState({ isGenerating: true, progress: 0, error: null });

    // Simulate progress increments while waiting for backend
    intervalRef.current = setInterval(() => {
      setState((prev) => {
        // Progress slows down as it approaches 90%
        const increment = prev.progress < 50 ? 8 : prev.progress < 80 ? 3 : 1;
        const nextProgress = Math.min(prev.progress + increment, 90);
        return { ...prev, progress: nextProgress };
      });
    }, 300);
  }, []);

  const stopProgressSimulation = useCallback((success: boolean, errorMsg?: string) => {
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }

    if (success) {
      setState({ isGenerating: false, progress: 100, error: null });
      // Reset after brief display
      setTimeout(() => {
        setState({ isGenerating: false, progress: 0, error: null });
      }, 2000);
    } else {
      setState({ isGenerating: false, progress: 0, error: errorMsg || 'Generation failed' });
    }
  }, []);

  const generateReport = useCallback(
    async (userProfileId: string, startDate: string, endDate: string) => {
      startProgressSimulation();

      try {
        const response = await api.get('/api/reports/productivity/pdf', {
          params: { userProfileId, startDate, endDate },
          responseType: 'blob',
        });

        // Create download link
        const blob = new Blob([response.data], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `productivity-report-${startDate}-to-${endDate}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        stopProgressSimulation(true);
      } catch (err: unknown) {
        const message =
          err instanceof Error ? err.message : 'Failed to generate report';
        stopProgressSimulation(false, message);
      }
    },
    [startProgressSimulation, stopProgressSimulation]
  );

  return {
    ...state,
    generateReport,
  };
}
