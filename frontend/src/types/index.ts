/* ─── Enums as const objects (erasableSyntaxOnly compatible) ── */

export const ProjectStatus = {
  Active: 'Active',
  Completed: 'Completed',
  Archived: 'Archived',
} as const;
export type ProjectStatus = (typeof ProjectStatus)[keyof typeof ProjectStatus];

export const TaskPriority = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
  Critical: 'Critical',
} as const;
export type TaskPriority = (typeof TaskPriority)[keyof typeof TaskPriority];

export const TaskItemStatus = {
  Todo: 'Todo',
  InProgress: 'InProgress',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
} as const;
export type TaskItemStatus = (typeof TaskItemStatus)[keyof typeof TaskItemStatus];

export const SessionStatus = {
  Active: 'Active',
  Paused: 'Paused',
  Completed: 'Completed',
} as const;
export type SessionStatus = (typeof SessionStatus)[keyof typeof SessionStatus];

/* ─── Domain Models ─────────────────────────────────────── */

export interface UserProfile {
  id: string;
  identityUserId: string;
  displayName: string;
  email: string;
  bio?: string;
  avatarUrl?: string;
  gitHubUrl?: string;
  linkedInUrl?: string;
  createdAt: string;
  updatedAt: string;
  projects: Project[];
}

export interface Project {
  id: string;
  userProfileId: string;
  name: string;
  description?: string;
  status: ProjectStatus;
  technologyStack?: string;
  repositoryUrl?: string;
  liveUrl?: string;
  imageUrl?: string;
  createdAt: string;
  updatedAt: string;
  completedAt?: string;
  tasks: TaskItem[];
}

export interface TaskItem {
  id: string;
  projectId: string;
  title: string;
  description?: string;
  priority: TaskPriority;
  status: TaskItemStatus;
  createdAt: string;
  updatedAt: string;
  completedAt?: string;
  dueDate?: string;
  workSessions: WorkSession[];
}

export interface WorkSession {
  id: string;
  taskItemId: string;
  startTime: string;
  endTime?: string;
  status: SessionStatus;
  notes?: string;
}

/* ─── DTOs ──────────────────────────────────────────────── */

export interface UserProfileDto {
  id: string;
  displayName: string;
  email: string;
  bio?: string;
  avatarUrl?: string;
  gitHubUrl?: string;
  linkedInUrl?: string;
  createdAt: string;
  projectCount: number;
}

export interface TaskItemDto {
  id: string;
  projectId: string;
  projectName: string;
  title: string;
  description?: string;
  priority: TaskPriority;
  status: TaskItemStatus;
  createdAt: string;
  completedAt?: string;
  dueDate?: string;
  totalHoursLogged: number;
  sessionCount: number;
  hasActiveSession: boolean;
}

export interface WorkSessionDto {
  id: string;
  taskItemId: string;
  taskTitle: string;
  startTime: string;
  endTime?: string;
  status: SessionStatus;
  notes?: string;
  durationHours: number;
}

export interface TaskReportGroup {
  status: TaskItemStatus;
  statusLabel: string;
  taskCount: number;
  totalHours: number;
  tasks: TaskItemDto[];
}

export interface ProductivityReport {
  userProfileId: string;
  userDisplayName: string;
  startDate: string;
  endDate: string;
  generatedAt: string;
  totalHoursLogged: number;
  totalTasksCount: number;
  completedTasksCount: number;
  groups: TaskReportGroup[];
}

/* ─── Auth ──────────────────────────────────────────────── */

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  displayName: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiration: string;
  userProfile: UserProfileDto;
}

export interface RefreshTokenRequest {
  token: string;
  refreshToken: string;
}
