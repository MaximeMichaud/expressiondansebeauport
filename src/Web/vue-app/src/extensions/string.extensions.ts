import i18n from "@/i18n";

export {};

declare global {
  interface String {
    makeFirstLetterLowercase(): string;

    toCamelCase(): string;

    truncateString(maxLength: number): string;

    extractFileName(): string;

    toLocalDateTimeString(): string;
  }
}

String.prototype.makeFirstLetterLowercase = function (): string {
  return this.substring(0, 1).toLowerCase() + this.substring(1);
};

String.prototype.toCamelCase = function (): string {
  const words = this.split("_");
  const camelCasedWords = [];
  for (let i = 0; i < words.length; i++) {
    const word = words[i];
    if (i == 0) {
      camelCasedWords.push(word.toLowerCase());
      continue;
    }
    if (word.length > 1)
      camelCasedWords.push(
        `${word[0].toUpperCase()}${word.substring(1).toLowerCase()}`
      );
    else camelCasedWords.push(word.toUpperCase());
  }
  return camelCasedWords.join("");
};

String.prototype.truncateString = function truncateString(maxLength: number): string {
  if (this.length > maxLength) {
    return this.substr(0, maxLength) + "...";
  }
  return this.toString();
};

String.prototype.extractFileName = function (): string {
  const regex = /-(.+)\.[^.]+$/;
  const match = decodeURI(this.toString()).match(regex);
  return match ? match[1] : decodeURI(this.toString())
};

String.prototype.toLocalDateTimeString = function toLocalDateTimeString(): string {
  const localDateTime = new Date(this as string)
  const locale = `${i18n.getLocale()}-CA`
  const time = localDateTime.toLocaleTimeString(locale, {hour: "2-digit", minute: "2-digit", hour12: false})
  return `${localDateTime.toLocaleDateString(locale)} ${i18n.t('global.at')} ${time}`
};