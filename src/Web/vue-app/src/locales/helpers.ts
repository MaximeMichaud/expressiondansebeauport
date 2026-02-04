import i18n from "@/i18n";
import { LOCALES } from "@/locales/index";

export function getLocalizedRoutes(key:string) {
  const localizedRoutes:string[] = [];

  LOCALES.forEach((locale) => {
    const route =  getValueWithDottedKey(i18n.messages[locale.value], key);

    if(route && route !== "") localizedRoutes.push(route);
  })

  return localizedRoutes
}

function getValueWithDottedKey(obj: Record<string, any>, key:string):any {
  return key
      .split('.')
      .reduce((acc, part) => acc && acc[part], obj);
}