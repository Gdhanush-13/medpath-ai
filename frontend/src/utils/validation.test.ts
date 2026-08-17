import { describe, expect, it } from 'vitest';
import { firstName, isValidEmail } from './validation';

describe('validation helpers', () => {
  it('accepts normal email addresses and rejects malformed values', () => {
    expect(isValidEmail('student@medpath.local')).toBe(true);
    expect(isValidEmail('student@')).toBe(false);
  });

  it('returns a stable greeting name', () => {
    expect(firstName('Ada Lovelace')).toBe('Ada');
    expect(firstName('')).toBe('there');
  });
});
