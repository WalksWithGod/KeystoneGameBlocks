/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 2/26/2015
 * Time: 9:20 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Keystone.Utilities
{
	/// <summary>
	/// Description of TimeDateHelper.
	/// </summary>
	public class TimeDateHelper
	{
		public static long ToJulian(DateTime dateTime)
        {
            int day = dateTime.Day;
            int month = dateTime.Month;
            int year = dateTime.Year;

            if (month < 3)
            {
                month = month + 12;
                year = year - 1;
            }

            return day + (153 * month - 457) / 5 + 365 * year + (year / 4) - (year / 100) + (year / 400) + 1721119;
        } 

        public static string FromJulian(long julianDate, string format) 
        { 
            return FromJulian(julianDate).ToString(format); 
        }

        public static DateTime FromJulian(long julianDate)
        {
            long L = julianDate + 68569;
            long N = (long)((4 * L) / 146097);
            L = L - ((long)((146097 * N + 3) / 4));

            long I = (long)((4000 * (L + 1) / 1461001));
            L = L - (long)((1461 * I) / 4) + 31;

            long J = (long)((80 * L) / 2447);
            int Day = (int)(L - (long)((2447 * J) / 80));

            L = (long)(J / 11);
            int Month = (int)(J + 2 - 12 * L);
            int Year = (int)(100 * (N - 49) + I + L);

            // example format "dd/MM/yyyy"
            return new DateTime(Year, Month, Day); 
        }
	}
}
