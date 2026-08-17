import { useQuery } from '@tanstack/react-query';
import { api } from '../api/client';
import { Card, ErrorState } from '../components/Ui';
export function UsersPage() { const query = useQuery({ queryKey: ['users'], queryFn: api.users }); if (query.isLoading) return <div className="loading">Loading users…</div>; if (query.isError) return <ErrorState error={query.error}/>; return <Card title="User management"><div className="table-wrap"><table><thead><tr><th>Name</th><th>Email</th><th>Role</th><th>Status</th></tr></thead><tbody>{query.data?.map(user => <tr key={user.id}><td>{user.displayName}</td><td>{user.email}</td><td>{user.roles.join(', ')}</td><td><span className="status-pill">{user.status}</span></td></tr>)}</tbody></table></div></Card>; }
