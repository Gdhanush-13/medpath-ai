import { Navigate, Route, Routes } from 'react-router-dom';
import { useAuth } from './auth/AuthContext';
import { AppShell } from './layouts/AppShell';
import { CoursePage } from './pages/CoursePage';
import { CoursesPage } from './pages/CoursesPage';
import { DashboardPage } from './pages/DashboardPage';
import { LoginPage } from './pages/LoginPage';
import { UsersPage } from './pages/UsersPage';
import { CourseBuilderPage } from './pages/CourseBuilderPage';
import { AuditPage } from './pages/AuditPage';
function Protected() { const { user, loading } = useAuth(); if (loading) return <div className="loading">Restoring your session…</div>; return user ? <AppShell/> : <Navigate to="/login" replace/>; }
export default function App() { return <Routes><Route path="/login" element={<LoginPage/>}/><Route element={<Protected/>}><Route path="/dashboard" element={<DashboardPage/>}/><Route path="/courses" element={<CoursesPage/>}/><Route path="/courses/new" element={<CourseBuilderPage/>}/><Route path="/courses/:id" element={<CoursePage/>}/><Route path="/users" element={<UsersPage/>}/><Route path="/audit" element={<AuditPage/>}/></Route><Route path="*" element={<Navigate to="/dashboard" replace/>}/></Routes>; }
