import i18n from "@/i18n";

export class TranslatableString {
  fr?: string
  en?: string

  constructor(fr?: string, en?: string) {
    this.fr = fr
    this.en = en
  }

  get getValueForLocale() {
    if (i18n.getLocale() === "fr")
      return this.fr ?? "";
    return this.en ?? "";
  }
}