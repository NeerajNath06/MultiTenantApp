/**
 * Format date/time from API for display in IST (India Standard Time).
 * API stores/returns UTC; we display in IST so user sees correct local time.
 */

const IST_TIMEZONE = 'Asia/Kolkata';

/**
 * Parse API datetime string as UTC if it has no timezone (e.g. "2025-02-09T04:47:00" from server = UTC).
 * If string already has Z or offset, use as-is.
 */
function parseAsUtcIfNeeded(d: Date | string | null | undefined): Date | null {
  if (d == null) return null;
  if (typeof d !== 'string') return d;
  const s = d.trim();
  if (!s) return null;
  if (s.endsWith('Z') || /[+-]\d{2}:?\d{2}$/.test(s)) return new Date(s);
  return new Date(s + 'Z');
}

/**
 * Format time in IST (e.g. "10:10 AM").
 */
export function formatTimeIST(d: Date | string | null | undefined): string {
  const date = parseAsUtcIfNeeded(d);
  if (!date || isNaN(date.getTime())) return '-';
  return date.toLocaleTimeString('en-IN', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true,
    timeZone: IST_TIMEZONE,
  });
}

/**
 * Format short date in IST (e.g. "Feb 9").
 */
export function formatDateShortIST(d: Date | string | null | undefined): string {
  const date = parseAsUtcIfNeeded(d);
  if (!date || isNaN(date.getTime())) return '-';
  return date.toLocaleDateString('en-IN', {
    month: 'short',
    day: 'numeric',
    timeZone: IST_TIMEZONE,
  });
}

/**
 * Format day of week in IST (e.g. "Mon").
 */
export function formatDayIST(d: Date | string | null | undefined): string {
  const date = parseAsUtcIfNeeded(d);
  if (!date || isNaN(date.getTime())) return '-';
  return date.toLocaleDateString('en-IN', {
    weekday: 'short',
    timeZone: IST_TIMEZONE,
  });
}

/**
 * Format full date in IST (e.g. "9 Feb 2025").
 */
export function formatFullDateIST(d: Date | string | null | undefined): string {
  const date = parseAsUtcIfNeeded(d);
  if (!date || isNaN(date.getTime())) return '-';
  return date.toLocaleDateString('en-IN', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    timeZone: IST_TIMEZONE,
  });
}
