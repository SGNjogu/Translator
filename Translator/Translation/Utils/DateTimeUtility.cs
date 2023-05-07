using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Translation.Utils
{
    public static class DateTimeUtility
    {

        /// <summary>
        /// Enums for different date formats
        /// </summary>
        public enum Format
        {
            DayDateMonthHour,
            Day,
            DayHour,
            Month,
            Year,
            DayMonth,
            Time,
            Time24Hour,
            FullDate,
            FullDateGMT,
            MonthYear,
            DayDateMonth,
            DayMonthYear,
            FullDayMonthYear
        }

        static readonly string DayDateMonthHour = "ddd, dd MMM h:mm tt";
        static readonly string Day = "ddd";
        static readonly string DayHour = "ddd, hh:mm tt";
        static readonly string Year = "yyyy";
        static readonly string FullDate = "dd MMMM yyyy h:mm tt";
        static readonly string DayMonth = "dd MMMM";
        static readonly string Time = "hh:mm tt";
        static readonly string Time24Hour = "HH:mm";
        static readonly string FullDateGMT = "ddd, dd MMM yyy HH':'mm':'ss 'GMT'";
        static readonly string MonthYear = "MMM yyyy";
        static readonly string DayDateMonth = "ddd, dd MMM";
        static readonly string DayMonthYear = "dd, MMM yyyy";
        static readonly string FullDayMonthYear = "dddd, dd MMMM yyyy";

        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Helper function to return current date in long
        /// </summary>
        /// <returns>Current Date in long</returns>
        public static long ReturnCurrentTimeInLong()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Takes a date in long, adds days and return the new 
        /// date in long
        /// </summary>
        /// <param name="date">date in long</param>
        /// <param name="days">days in int</param>
        /// <returns>Return Added Days in long</returns>
        public static long ReturnAddedDaysInLongFromLong(long date, int days)
        {
            DateTimeOffset oDate = DateTimeOffset.FromUnixTimeMilliseconds(date);
            return oDate.AddDays(days).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Takes in a Date string and returns the date as a long
        /// </summary>
        /// <param name="date">Takes in Date as a string</param>
        /// <returns>Returns Date string as long</returns>
        public static long ReturnDateLongFromString(string date)
        {
            DateTime oDate = Convert.ToDateTime(date);
            return ((DateTimeOffset)oDate).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Takes in DateTime and returns the date as a long
        /// </summary>
        /// <param name="date">Takes in a DateTime</param>
        /// <returns>Returns DatetTime as a long</returns>
        public static long ReturnLongFromDateTime(DateTime date)
        {
            return (long)(date - UnixEpoch).TotalMilliseconds;
        }

        /// <summary>
        /// Takes in a Date as DateTime and returns the date as a long
        /// </summary>
        /// <param name="date">Takes in a long</param>
        /// <returns>Returns DateTime from a long</returns>
        public static DateTime ReturnDateTimeFromlongMilliseconds(long date)
        {
            return UnixEpoch.AddMilliseconds(date);
        }

        /// <summary>
        /// Takes in a Date as DateTime and returns the date as a long
        /// </summary>
        /// <param name="date">Takes in a long</param>
        /// <returns>Returns DateTime from a long</returns>
        public static DateTime ReturnDateTimeFromlongSeconds(long date)
        {
            return UnixEpoch.AddSeconds(date);
        }

        /// <summary>
        /// Takes in TimesSpan to be added, adds the TimeSpan and returns 
        /// the new date as a long
        /// </summary>
        /// <param name="time">Takes in TimeSpan</param>
        /// <param name="date">Takes in a long</param>
        /// <returns>Adds TimeSpan to date in long and returns long</returns>
        public static long ReturnLongAfterAddingTimeSpan(TimeSpan time, long date)
        {
            var datetime = ReturnDateTimeFromlongMilliseconds(date);
            var newdatetime = datetime.Add(time);
            return ReturnLongFromDateTime(newdatetime);
        }

        /// <summary>
        /// Takes in enum Format, and returns a Date string formatted
        /// as the enum Format intended
        /// </summary>
        /// <param name="format">Takes in enum Format</param>
        /// <param name="date">Takes in a Date as long</param>
        /// <returns>Returns a formatted Date string</returns>
        public static string ReturnDateStringFromLong(Format format, long date)
        {
            DateTime datetime = UnixEpoch.AddMilliseconds(date).ToLocalTime();
            string dateString = "";

            switch (format)
            {
                case Format.DayDateMonthHour:
                    dateString = datetime.ToString(DayDateMonthHour);
                    break;
                case Format.Day:
                    dateString = datetime.ToString(Day);
                    break;
                case Format.DayHour:
                    dateString = datetime.ToString(DayHour);
                    break;
                case Format.Year:
                    dateString = datetime.ToString(Year);
                    break;
                case Format.FullDate:
                    dateString = datetime.ToString(FullDate);
                    break;
                case Format.DayMonth:
                    dateString = datetime.ToString(DayMonth);
                    break;
                case Format.Time:
                    dateString = datetime.ToString(Time);
                    break;
                case Format.Time24Hour:
                    dateString = datetime.ToString(Time24Hour);
                    break;
                case Format.FullDateGMT:
                    dateString = datetime.ToString(FullDateGMT);
                    break;
                case Format.MonthYear:
                    dateString = datetime.ToString(MonthYear);
                    break;
                case Format.DayDateMonth:
                    dateString = datetime.ToString(DayDateMonth);
                    break;
                case Format.DayMonthYear:
                    dateString = datetime.ToString(DayMonthYear);
                    break;
                case Format.FullDayMonthYear:
                    dateString = datetime.ToString(FullDayMonthYear);
                    break;
            }

            return dateString;
        }

        /// <summary>
        /// Takes in time as a long and returns a Date string
        /// Formatted for chats
        /// </summary>
        /// <param name="startDate">Takes in StartDate as a long</param>
        /// <returns>Returns a chat formatted Date string</returns>
        public static string ReturnChatFormatedDate(long startDate)
        {
            string date;

            var difference = ReturnDateTimeFromlongMilliseconds(ReturnCurrentTimeInLong()) - ReturnDateTimeFromlongMilliseconds(startDate);

            if (difference.TotalSeconds < 120)
            {
                date = $"seconds ago";
            }
            else if (difference.TotalMinutes < 5)
            {
                date = $"{(int)Math.Ceiling(difference.TotalMinutes)} mins ago";
            }
            else if (difference.TotalDays < 1)
            {
                date = ReturnDateStringFromLong(Format.Time24Hour, startDate);
            }
            else if (difference.TotalDays < 7)
            {
                date = ReturnDateStringFromLong(Format.DayHour, startDate);
            }
            else if (difference.TotalDays < 365)
            {
                date = ReturnDateStringFromLong(Format.DayDateMonthHour, startDate);
            }
            else
            {
                date = ReturnDateStringFromLong(Format.DayMonthYear, startDate);
            }

            return date;
        }

        /// <summary>
        /// Takes in time as a long and returns a Date string
        /// Formatted for Feed (Such as Social Feed or News Feed)
        /// </summary>
        /// <param name="startDate">Takes in StartDate as a long</param>
        /// <returns>Returns a feed formatted Date string</returns>
        public static string ReturnFeedFormatedDate(long startDate)
        {
            string date;

            var difference = ReturnDateTimeFromlongMilliseconds(ReturnCurrentTimeInLong()) - ReturnDateTimeFromlongMilliseconds(startDate);

            if (difference.TotalSeconds < 120)
            {
                date = $"seconds ago";
            }
            else if (difference.TotalMinutes < 5)
            {
                date = $"{(int)Math.Ceiling(difference.TotalMinutes)} mins ago";
            }
            else if (difference.TotalDays < 1)
            {
                date = $"{(int)Math.Ceiling(difference.TotalHours)}h";
            }
            else if (difference.TotalDays < 7)
            {
                date = ReturnDateStringFromLong(Format.DayHour, startDate);
            }
            else if (difference.TotalDays < 31)
            {
                date = $"{(int)Math.Ceiling(difference.TotalDays)}d";
            }
            else if (difference.TotalDays < 365)
            {
                date = ReturnDateStringFromLong(Format.DayMonth, startDate);
            }
            else
            {
                date = ReturnDateStringFromLong(Format.DayMonthYear, startDate);
            }

            return date;
        }

        /// <summary>
        /// Method to ParseExact date
        /// </summary>
        /// <param name="date">Takes in the date string e.g 20200515072845 representing 2020/05/15/07:28:45</param>
        /// <param name="returnFormat">Takes in the desired return format</param>
        /// <param name="currentFormat">Takes in the current format, default is yyyyMMddHHmmss</param>
        /// <returns>Formated Date String</returns>
        public static string ReturnExactlyParsedDate(string date, Format returnFormat, string currentFormat = "yyyyMMddHHmmss")
        {
            switch (returnFormat)
            {
                case Format.DayDateMonthHour:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(DayDateMonthHour);
                case Format.Day:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(Day);
                case Format.DayHour:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(DayHour);
                case Format.Year:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(Year);
                case Format.FullDate:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(FullDate);
                case Format.DayMonth:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(DayMonth);
                case Format.Time:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(Time);
                case Format.Time24Hour:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(Time24Hour);
                case Format.FullDateGMT:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(FullDateGMT);
                case Format.MonthYear:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(MonthYear);
                case Format.DayDateMonth:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(DayDateMonth);
                case Format.DayMonthYear:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(DayMonthYear);
                default:
                    return DateTime.ParseExact(date, currentFormat, CultureInfo.InvariantCulture).ToString(FullDate);
            }
        }

        /// <summary>
        /// Returns duration in minutes
        /// </summary>
        /// <param name="startTime">Takes in start time in long</param>
        /// <param name="endTime">Takes in end time in long</param>
        /// <returns>Returne the duration</returns>
        public static double ReturnDurationInMinutesFromLongSeconds(long startTime, long endTime)
        {
            var start = ReturnDateTimeFromlongSeconds(startTime);
            var end = ReturnDateTimeFromlongSeconds(endTime);

            return (end - start).TotalMinutes;
        }

        /// <summary>
        /// Returns duration in minutes
        /// </summary>
        /// <param name="startTime">Takes in start time in long</param>
        /// <param name="endTime">Takes in end time in long</param>
        /// <returns>Returne the duration</returns>
        public static double ReturnDurationInMinutesFromMilliseconds(long startTime, long endTime)
        {
            var start = ReturnDateTimeFromlongMilliseconds(startTime);
            var end = ReturnDateTimeFromlongMilliseconds(endTime);

            return (end - start).TotalMinutes;
        }

        public static DateRange GetDateRange(DateTime startDate, DateTime endDate)
        {
            return new DateRange
            {
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public static bool IsInDateRange(DateTime dateToCheck, DateRange targetRange)
        {
            //An argument can be made to use non-encompassing comparisons for both checks
            //depending on your requirements
            return dateToCheck.Date >= targetRange.StartDate.Date && dateToCheck.Date <= targetRange.EndDate.Date;
        }
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateRange() { }
        public DateRange(DateTime start, DateTime end)
        {
            this.StartDate = start;
            this.EndDate = end;
        }
    }
}
