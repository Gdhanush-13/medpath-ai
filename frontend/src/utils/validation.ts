export function isValidEmail(value: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim());
}

export function firstName(displayName: string): string {
  return displayName.trim().split(/\s+/)[0] || 'there';
}
