import type { ReactNode } from 'react';
export function Button({ children, ...props }: React.ButtonHTMLAttributes<HTMLButtonElement>) { return <button className="button" {...props}>{children}</button>; }
export function Card({ children, title, subtitle, action }: { children: ReactNode; title?: string; subtitle?: string; action?: ReactNode }) { return <section className="card"><div className="card-head">{(title || subtitle) && <div>{title && <h2>{title}</h2>}{subtitle && <p className="card-subtitle">{subtitle}</p>}</div>}{action}</div>{children}</section>; }
export function EmptyState({ title, detail }: { title: string; detail: string }) { return <div className="empty"><strong>{title}</strong><span>{detail}</span></div>; }
export function ErrorState({ error }: { error: unknown }) { return <div className="error-state">{error instanceof Error ? error.message : 'Something went wrong. Please try again.'}</div>; }
