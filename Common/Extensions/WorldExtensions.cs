using Common.Interfaces.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class WorldExtensions
    {
        public static int GetTime(this IWorldHandler worldhandler)
        {
            DateTime now = DateTime.Now;
            int year = (now.Year - 2000) << 24;
            int month = (now.Month - 1) << 20;
            int day = (now.Day - 1) << 14;
            int dow = (int)now.DayOfWeek << 11;
            int hour = now.Hour << 6;

            return now.Minute + hour + dow + day + month + year;
        }
    }
}
