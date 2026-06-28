import { useState } from 'react';
import { StatusBadge } from '../common/StatusBadge';
import type { TaskItemDto } from '../../types';
import { TaskItemStatus } from '../../types';
import './TaskGrid.css';

interface TaskGridProps {
  tasks: TaskItemDto[];
  onTaskSelect?: (task: TaskItemDto) => void;
  onStartSession?: (taskId: string) => void;
  onStopSession?: (taskId: string) => void;
  id?: string;
}

/**
 * Interactive data grid for tasks.
 * Status badges remain persistently visible regardless of selection state.
 */
export function TaskGrid({ tasks, onTaskSelect, onStartSession, onStopSession, id }: TaskGridProps) {
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [sortField, setSortField] = useState<keyof TaskItemDto>('createdAt');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('desc');

  const handleSort = (field: keyof TaskItemDto) => {
    if (sortField === field) {
      setSortDirection((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
  };

  const sortedTasks = [...tasks].sort((a, b) => {
    const aVal = a[sortField];
    const bVal = b[sortField];
    if (aVal == null && bVal == null) return 0;
    if (aVal == null) return 1;
    if (bVal == null) return -1;

    const comparison = String(aVal).localeCompare(String(bVal));
    return sortDirection === 'asc' ? comparison : -comparison;
  });

  const handleRowClick = (task: TaskItemDto) => {
    const newSelected = selectedId === task.id ? null : task.id;
    setSelectedId(newSelected);
    if (newSelected && onTaskSelect) {
      onTaskSelect(task);
    }
  };

  const isFinished = (status: TaskItemStatus) =>
    status === TaskItemStatus.Completed || status === TaskItemStatus.Cancelled;

  return (
    <div id={id} className="task-grid glass-card animate-fade-in">
      <div className="task-grid__header">
        <h3 className="task-grid__title">Tasks</h3>
        <span className="task-grid__count">{tasks.length} total</span>
      </div>

      <div className="task-grid__table-wrapper">
        <table className="task-grid__table">
          <thead>
            <tr>
              <th onClick={() => handleSort('title')} className="task-grid__th--sortable">
                Task {sortField === 'title' && (sortDirection === 'asc' ? '↑' : '↓')}
              </th>
              <th onClick={() => handleSort('projectName')} className="task-grid__th--sortable">
                Project {sortField === 'projectName' && (sortDirection === 'asc' ? '↑' : '↓')}
              </th>
              <th>Status</th>
              <th onClick={() => handleSort('priority')} className="task-grid__th--sortable">
                Priority {sortField === 'priority' && (sortDirection === 'asc' ? '↑' : '↓')}
              </th>
              <th onClick={() => handleSort('totalHoursLogged')} className="task-grid__th--sortable">
                Hours {sortField === 'totalHoursLogged' && (sortDirection === 'asc' ? '↑' : '↓')}
              </th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {sortedTasks.map((task) => (
              <tr
                key={task.id}
                className={`task-grid__row ${selectedId === task.id ? 'task-grid__row--selected' : ''}`}
                onClick={() => handleRowClick(task)}
              >
                <td className="task-grid__cell-title">
                  <span className="task-grid__task-name">{task.title}</span>
                  {task.dueDate && (
                    <span className="task-grid__due-date">
                      Due: {new Date(task.dueDate).toLocaleDateString()}
                    </span>
                  )}
                </td>
                <td className="task-grid__cell-secondary">{task.projectName}</td>
                <td>
                  {/* Status badge stays visible regardless of row selection */}
                  {isFinished(task.status) ? (
                    <StatusBadge status="FINISHED" />
                  ) : (
                    <StatusBadge status={task.status} />
                  )}
                </td>
                <td>
                  <StatusBadge status={task.priority} />
                </td>
                <td className="task-grid__cell-hours">
                  {task.totalHoursLogged.toFixed(1)}h
                </td>
                <td className="task-grid__cell-actions">
                  {task.hasActiveSession ? (
                    <button
                      className="btn btn-danger btn-sm"
                      onClick={(e) => {
                        e.stopPropagation();
                        onStopSession?.(task.id);
                      }}
                    >
                      ⏹ Stop
                    </button>
                  ) : (
                    !isFinished(task.status) && (
                      <button
                        className="btn btn-success btn-sm"
                        onClick={(e) => {
                          e.stopPropagation();
                          onStartSession?.(task.id);
                        }}
                      >
                        ▶ Start
                      </button>
                    )
                  )}
                </td>
              </tr>
            ))}
            {tasks.length === 0 && (
              <tr>
                <td colSpan={6} className="task-grid__empty">
                  No tasks found. Create a project and add tasks to get started.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
