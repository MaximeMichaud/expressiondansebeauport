import {isCookieEnabled} from "@/services/cookiePreferencesService";

export function applyCookieControlledScripts(): void {
  if (isCookieEnabled('analytics')) {
    // Load or enable analytics scripts here when they are added to the project.
  }

  if (isCookieEnabled('marketing')) {
    // Load or enable marketing scripts here when they are added to the project.
  }
}
