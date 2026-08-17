import type { AuthResponse, Course, CourseDetails, Dashboard, User } from '../types';

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5076';
let accessToken = localStorage.getItem('medpath_access_token');
export const setAccessToken = (token: string | null) => { accessToken = token; token ? localStorage.setItem('medpath_access_token', token) : localStorage.removeItem('medpath_access_token'); };
export const hasAccessToken = () => Boolean(accessToken);
async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, { ...init, headers: { 'Content-Type': 'application/json', ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}), ...init.headers } });
  if (!response.ok) { const body = await response.json().catch(() => ({})); throw new Error(body.message ?? body.title ?? `Request failed (${response.status})`); }
  return response.status === 204 ? (undefined as T) : response.json();
}
export const api = {
  login: (body: { email: string; password: string }) => request<AuthResponse>('/api/auth/login', { method: 'POST', body: JSON.stringify(body) }),
  me: () => request<User>('/api/auth/me'),
  courses: () => request<Course[]>('/api/courses'),
  course: (id: string) => request<CourseDetails>(`/api/courses/${id}`),
  dashboard: () => request<Dashboard>('/api/learning/dashboard'),
  completeLesson: (id: string) => request<void>(`/api/learning/lessons/${id}/complete`, { method: 'POST' }),
  aiStudy: (body: { action: string; lessonTitle: string; lessonContent: string }) => request<{ answer: string }>('/api/ai-study', { method: 'POST', body: JSON.stringify(body) }),
  users: () => request<User[]>('/api/users'),
  createCourse: (body: { title: string; description: string }) => request<Course>('/api/courses', { method: 'POST', body: JSON.stringify(body) }),
  auditLogs: () => request<{ id: string; actorEmail: string; action: string; targetType: string; targetId: string; createdAtUtc: string }[]>('/api/audit-logs'),
};
