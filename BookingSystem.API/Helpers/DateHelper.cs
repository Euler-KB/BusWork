using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Helpers
{
    public static class DateHelper
    {

        public static bool IsBetween(DateTime start, DateTime end, DateTime cmp, bool inclusive = true)
        {
            if (inclusive)
                return cmp >= start && cmp <= end;
            else
                return cmp > start && cmp < end;
        }

        /// <summary>
        /// Checks whether the given date is within today's time range
        /// </summary>
        /// <param name="date">The date to compare</param>
        /// <param name="strict">If set to true, will ensure that the date is strictly today else will allow dates after today</param>
        /// <returns></returns>
        public static bool IsToday(DateTime date, bool strict = true)
        {
            var now = DateTime.Now;
            var startofDay = new DateTime(now.Year, now.Month, now.Day);
            return date >= startofDay && (strict ? date < startofDay.AddDays(1) : true);
        }

        public static bool IsYesterday(DateTime date, bool strict = true)
        {
            var now = DateTime.Now;
            var startOfDay = new DateTime(now.Year, now.Month, now.Day);
            return date >= startOfDay.AddDays(-1) && (strict ? date < startOfDay : true);
        }

        public static bool IsThisWeek(DateTime date, bool strict = true)
        {
            //
            var now = DateTime.Now;
            var startOfWeek = new DateTime(now.Year, now.Month, now.Day);
            startOfWeek = startOfWeek.AddDays(-(int)startOfWeek.DayOfWeek);

            if (!strict)
                return date >= startOfWeek;
            //
            var endOfWeek = new DateTime(now.Year, now.Month, now.Day);
            endOfWeek = endOfWeek.AddDays(7 - (int)endOfWeek.DayOfWeek);


            return date >= startOfWeek && date < endOfWeek;
        }

        public static bool IsThisMonth(DateTime date, bool strict = true)
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            if (!strict)
                return date >= startOfMonth;


            return date >= startOfMonth && date < startOfMonth.AddMonths(1);
        }
    }
}