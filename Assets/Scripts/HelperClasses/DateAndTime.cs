using System;

namespace VARLab.PublicHealth
{
    public static class DateAndTime
    {
        private static DateTime _dateTime;
        private static string _date;
        private static string _timeStamp;

        private static void GetDateTime()
        {
            _dateTime = DateTime.Now;
        }

        public static string GetDateString()
        {
            GetDateTime();
            _date = _dateTime.ToString("MMM dd, yyyy");
            return _date;
        }

        public static string GetTimeStamp()
        {
            GetDateTime();
            _timeStamp = _dateTime.ToString("ddd MMM dd H:mm:ss");
            return _timeStamp;
        }
    }
}
