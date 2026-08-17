import { useQuery } from '@tanstack/react-query';
import { api } from '../api/client';
import { Card, ErrorState } from '../components/Ui';

export function AuditPage() {
  const query = useQuery({ queryKey: ['audit'], queryFn: api.auditLogs });
  if (query.isLoading) return <div className="loading">Loading audit history…</div>;
  if (query.isError) return <ErrorState error={query.error} />;
  return <Card title="Audit history" subtitle="Recent security-relevant actions across the workspace."><div className="table-wrap"><table><thead><tr><th>When</th><th>Actor</th><th>Action</th><th>Target</th></tr></thead><tbody>{query.data?.map(log => <tr key={log.id}><td>{new Date(log.createdAtUtc).toLocaleString()}</td><td>{log.actorEmail ?? 'System'}</td><td><span className="status-pill">{log.action}</span></td><td>{log.targetType}</td></tr>)}</tbody></table></div></Card>;
}
