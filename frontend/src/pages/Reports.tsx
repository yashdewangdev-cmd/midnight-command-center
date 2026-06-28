import { ReportGenerator } from '../components/features/ReportGenerator';
import { useAuth } from '../hooks/useAuth';
import './Reports.css';

/**
 * Reports page for generating and exporting productivity reports.
 */
export function Reports() {
  const { user } = useAuth();

  return (
    <div className="reports">
      <header className="reports__header animate-fade-in">
        <h1 className="reports__title">Reports</h1>
        <p className="reports__subtitle">
          Generate structured productivity reports and export as PDF
        </p>
      </header>

      <div className="reports__content animate-slide-up">
        {user?.id ? (
          <ReportGenerator userProfileId={user.id} id="report-gen-main" />
        ) : (
          <div className="reports__placeholder glass-card">
            <p>Please log in to generate reports.</p>
          </div>
        )}

        <div className="reports__info glass-card">
          <h3>📋 What's Included</h3>
          <ul className="reports__info-list">
            <li>Tasks grouped by status (Completed, In Progress, To Do, Cancelled)</li>
            <li>Total hours logged per task and per status group</li>
            <li>Project attribution for every task entry</li>
            <li>Priority indicators and completion dates</li>
            <li>Summary statistics: total hours, task counts, completion rate</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
