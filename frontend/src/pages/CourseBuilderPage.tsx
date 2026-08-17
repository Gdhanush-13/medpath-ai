import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import { Button, Card } from '../components/Ui';

export function CourseBuilderPage() {
  const navigate = useNavigate();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const mutation = useMutation({ mutationFn: () => api.createCourse({ title, description }), onSuccess: course => navigate(`/courses/${course.id}`) });
  return <Card title="Create a course" subtitle="Start with a clear learning outcome. Modules and assessments can be added through the API as the content workflow grows.">
    <form className="form-stack" onSubmit={event => { event.preventDefault(); mutation.mutate(); }}>
      <label>Course title<input value={title} onChange={event => setTitle(event.target.value)} required minLength={3} placeholder="e.g. Patient Safety Essentials" /></label>
      <label>Description<textarea value={description} onChange={event => setDescription(event.target.value)} required rows={5} placeholder="What will learners be able to do?" /></label>
      {mutation.isError && <p className="form-error">{mutation.error.message}</p>}
      <Button type="submit" disabled={mutation.isPending}>{mutation.isPending ? 'Creating…' : 'Create course'}</Button>
    </form>
  </Card>;
}
