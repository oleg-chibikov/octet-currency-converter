export const addHoursToCurrent = function (hours: number): Date {
  const date = new Date();
  date.setTime(date.getTime() + hours * 60 * 60 * 1000);
  return date;
};

export const addMinutesToCurrent = function (minutes: number): Date {
  const date = new Date();
  date.setTime(date.getTime() + minutes * 60 * 1000);
  return date;
};
