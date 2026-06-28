import { useState } from 'react';
import { ProgressBar } from '../common/ProgressBar';
import { useReportProgress } from '../../hooks/useReportProgress';
import './ReportGenerator.css';

interface ReportGeneratorProps {
  userProfileId: string;
  id?: string;
}

/**
 * Report generation form with real-time progress tracking.
 * Triggers PDF export with visual feedback during compilation.
 */
export function ReportGenerator({ userProfileId, id }: ReportGeneratorProps) {
  const today = new Date().toISOString().split('T')[0];
  const weekAgo = new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

  const [startDate, setStartDate] = useState(weekAgo);
  const [endDate, setEndDate] = useState(today);
  const { isGenerating, progress, error, generateReport } = useReportProgress();

  const handleGenerate = () => {
    generateReport(userProfileId, startDate, endDate);
  };

  return (
    <div id={id} className="report-generator glass-card animate-fade-in">
      <div className="report-generator__header">
        <h3 className="report-generator__title">📊 Generate Report</h3>
        <p className="report-generator__subtitle">
          Export your productivity data as a formatted PDF
        </p>
      </div>

      <div className="report-generator__form">
        <div className="report-generator__date-row">
          <div className="report-generator__field">
            <label className="input-label" htmlFor="report-start-date">Start Date</label>
            <input
              id="report-start-date"
              type="date"
              className="input"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              disabled={isGenerating}
            />
          </div>
          <div className="report-generator__field">
            <label className="input-label" htmlFor="report-end-date">End Date</label>
            <input
              id="report-end-date"
              type="date"
              className="input"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              disabled={isGenerating}
            />
          </div>
        </div>

        <button
          className="btn btn-primary btn-lg report-generator__btn"
          onClick={handleGenerate}
          disabled={isGenerating || !startDate || !endDate}
        >
          {isGenerating ? (
            <>
              <span className="report-generator__spinner" />
              Generating...
            </>
          ) : (
            <>📄 Generate PDF Report</>
          )}
        </button>
      </div>

      {isGenerating && (
        <div className="report-generator__progress animate-fade-in">
          <ProgressBar
            progress={progress}
            label="Compiling report..."
            variant="blue"
            animated
          />
        </div>
      )}

      {progress === 100 && !isGenerating && (
        <div className="report-generator__success animate-fade-in">
          ✅ Report downloaded successfully!
        </div>
      )}

      {error && (
        <div className="report-generator__error animate-fade-in">
          ❌ {error}
        </div>
      )}
    </div>
  );
}
