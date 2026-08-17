import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import { Button, Card } from '../components/Ui';
import '../course-builder.css';

export function CourseBuilderPage() {
  const navigate = useNavigate();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const mutation = useMutation({
    mutationFn: () => api.createCourse({ title, description }),
    onSuccess: course => navigate(`/courses/${course.id}`),
  });

  return (
    <Card
      title="Create a course"
      subtitle="Start with a clear learning outcome. Modules and assessments can be added through the API as the content workflow grows."
    >
      <form className="form-stack course-form" onSubmit={event => { event.preventDefault(); mutation.mutate(); }}>
        <div className="form-fields">
          <label className="form-field" htmlFor="course-title">
            <span>Course title</span>
            <input
              id="course-title"
              value={title}
              onChange={event => setTitle(event.target.value)}
              required
              minLength={3}
              placeholder="e.g. Patient Safety Essentials"
            />
            <small className="field-hint">Use a clear, learner-facing title.</small>
          </label>
          <label className="form-field" htmlFor="course-description">
            <span>Description</span>
            <textarea
              id="course-description"
              value={description}
              onChange={event => setDescription(event.target.value)}
              required
              rows={5}
              placeholder="What will learners be able to do?"
            />
            <small className="field-hint">Describe the practical outcome learners should achieve.</small>
          </label>
        </div>
        {mutation.isError && <p className="form-error" role="alert">{mutation.error.message}</p>}
        <div className="form-actions">
          <Button type="submit" disabled={mutation.isPending}>{mutation.isPending ? 'Creating...' : 'Create course'}</Button>
        </div>
      </form>
    </Card>
  );
}
