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

Date.prototype.formatToDateString = function toDateString(): string {
  return this.toLocaleDateString("fr-CA", {timeZone: "UTC"})
};

Date.prototype.formatToTimeString = function toTimeString(): string {
  return this.toLocaleTimeString("fr-CA", {hour: "2-digit", minute: "2-digit", hour12: false, timeZone: "UTC"})
};
