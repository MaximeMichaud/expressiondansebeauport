import i18n from "@/i18n";

export {};

declare global {
  interface Date {
    getTimestamp(): number;

    hoursBetweenToday(): number;

    formatToDateString(): string;

    formatToTimeString(): string;
  }
}

/*
  Extends the Date object to get number of seconds since UNIX epoch
  instead of milliseconds as with .getTime()
*/
Date.prototype.getTimestamp = function (): number {
  return Math.round(this.getTime() / 1000);
};

/*
  Extends the Date object to calculate the number of hours
  between .getTime() and today's date
*/
Date.prototype.hoursBetweenToday = function () {
  // Copy the date to avoid modifying the original object
  const endDate = new Date(this.getTime());
  const startDate = new Date();
  const timeDifference = endDate.getTime() - startDate.getTime();

  // Convert milliseconds to hours
  return Math.ceil(timeDifference / (1000 * 3600));
};


// set locale for dates to be formatted correctly, but time"s already in local time so say it's in UTC so its not converted
Date.prototype.formatToDateString = function toDateString(): string {
  return this.toLocaleDateString(`${i18n.getLocale()}-CA`, {timeZone: "UTC"})
};

// set locale for dates to be formatted correctly, but time"s already in local time so say it's in UTC so its not converted
Date.prototype.formatToTimeString = function toTimeString(): string {
  const locale = `${i18n.getLocale()}-CA`
  return this.toLocaleTimeString(locale, {hour: "2-digit", minute: "2-digit", hour12: false, timeZone: "UTC"})
};
