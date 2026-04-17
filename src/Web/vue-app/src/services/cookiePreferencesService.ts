export type CookiePreferenceType = 'necessary' | 'analytics' | 'marketing';

export interface CookiePreferences {
  necessary: true;
  analytics: boolean;
  marketing: boolean;
}

const STORAGE_KEY = 'cookiePreferences';

const defaultCookiePreferences: CookiePreferences = {
  necessary: true,
  analytics: false,
  marketing: false,
};

let cookiePreferencesCache: CookiePreferences | null | undefined;

function isBrowserEnvironment(): boolean {
  return typeof window !== 'undefined' && typeof window.localStorage !== 'undefined';
}

function normalizeCookiePreferences(value: unknown): CookiePreferences | null {
  if (!value || typeof value !== 'object') {
    return null;
  }

  const preferences = value as Partial<Record<CookiePreferenceType, unknown>>;

  return {
    necessary: true,
    analytics: preferences.analytics === true,
    marketing: preferences.marketing === true,
  };
}

function readCookiePreferencesFromStorage(): CookiePreferences | null {
  if (!isBrowserEnvironment()) {
    return null;
  }

  const rawValue = window.localStorage.getItem(STORAGE_KEY);
  if (!rawValue) {
    return null;
  }

  try {
    return normalizeCookiePreferences(JSON.parse(rawValue));
  } catch {
    window.localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

function writeCookiePreferencesToStorage(preferences: CookiePreferences): void {
  if (!isBrowserEnvironment()) {
    return;
  }

  window.localStorage.setItem(STORAGE_KEY, JSON.stringify(preferences));
}

export function getDefaultCookiePreferences(): CookiePreferences {
  return { ...defaultCookiePreferences };
}

export function getCookiePreferences(): CookiePreferences | null {
  if (cookiePreferencesCache !== undefined) {
    return cookiePreferencesCache ? { ...cookiePreferencesCache } : null;
  }

  cookiePreferencesCache = readCookiePreferencesFromStorage();
  return cookiePreferencesCache ? { ...cookiePreferencesCache } : null;
}

export function setCookiePreferences(preferences: Partial<CookiePreferences>): CookiePreferences {
  const nextPreferences: CookiePreferences = {
    necessary: true,
    analytics: preferences.analytics === true,
    marketing: preferences.marketing === true,
  };

  writeCookiePreferencesToStorage(nextPreferences);
  cookiePreferencesCache = nextPreferences;

  return { ...nextPreferences };
}

export function isCookieEnabled(type: CookiePreferenceType): boolean {
  const preferences = getCookiePreferences();

  if (type === 'necessary') {
    return true;
  }

  return preferences?.[type] === true;
}

export function clearCookiePreferences(): void {
  if (isBrowserEnvironment()) {
    window.localStorage.removeItem(STORAGE_KEY);
  }

  cookiePreferencesCache = null;
}

export function hasUserMadeChoice(): boolean {
  return getCookiePreferences() !== null;
}
