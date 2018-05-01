using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BookingSystem.Android.Helpers
{
    public static class DateHelper
    {
        public static void SelectDate(this Context context, bool includeTime, Action<DateTime?> onSelected, DateTime? current = null)
        {
            current = current ?? DateTime.Now;

            new DatePickerDialog(context, new EventHandler<DatePickerDialog.DateSetEventArgs>((s, e) =>
            {
                if (!includeTime)
                {
                    onSelected?.Invoke(e.Date);
                }
                else
                {
                    new TimePickerDialog(context, new EventHandler<TimePickerDialog.TimeSetEventArgs>((sender, evt) =>
                    {
                        var selectedDate = new DateTime(e.Year, e.Month, e.DayOfMonth, evt.HourOfDay, evt.Minute, 0);
                        onSelected?.Invoke(selectedDate);

                    }), current.Value.Hour, current.Value.Minute, false).Show();
                }

            }), current.Value.Year, current.Value.Month, current.Value.Day).Show();


        }

        public static string FormatDifference(DateTime start, DateTime end)
        {
            var diff = end - start;
            if (diff.Days != 0 && diff.Hours != 0)
            {
                return $"{diff.Days} day(s) , {diff.Hours} hour(s)";
            }
            else if (diff.Days == 0 && diff.Hours != 0)
            {
                return $"{diff.Hours} hour(s)";
            }
            else if (diff.Hours == 0 && diff.Minutes != 0)
            {
                return $"{diff.Minutes} minute (s), {diff.Seconds} sec(s)";
            }

            var totalDays = diff.TotalDays;
            return $"{  (totalDays - (int)totalDays > 0.5 ? Math.Ceiling(totalDays) : Math.Floor(totalDays))} day(s)";
        }
    }
}