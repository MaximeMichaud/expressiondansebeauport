import {TranslatableString} from "@/types";
import i18n from "@/i18n";

export async function getFileFromUrl(url: string, name: string, defaultType = 'image/jpeg'): Promise<File> {
  const response = await fetch(url);
  const data = await response.blob();
  return new File([data], name, {
    type: data.type || defaultType,
  });
}

export function getValueForLocale(translatableString?: TranslatableString): string {
  if (i18n.getLocale() === "fr")
    return translatableString?.fr ?? "";
  return translatableString?.en ?? "";
}