import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import { Card, EmptyState, ErrorState } from '../components/Ui';
export function CoursesPage() { const query = useQuery({ queryKey: ['courses'], queryFn: api.courses }); if (query.isLoading) return <div className="loading">Loading courses…</div>; if (query.isError) return <ErrorState error={query.error}/>; return <Card title="Course catalog"><div className="course-grid">{query.data?.length ? query.data.map(course => <Link className="course-card" to={`/courses/${course.id}`} key={course.id}><span className="course-kicker">{course.status}</span><h3>{course.title}</h3><p>{course.description}</p><small>{course.moduleCount} modules · {course.enrollmentCount} learners</small></Link>) : <EmptyState title="No courses published" detail="Educators can publish learning content from the course builder."/>}</div></Card>; }
