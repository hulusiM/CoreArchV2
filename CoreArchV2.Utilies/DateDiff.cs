using CoreArchV2.Dto.EReportDto;

namespace CoreArchV2.Utilies
{

    public static class DateDiff
    {
        //Verilen tarih aralığındaki yıl,ay,gün,dk,sn sürelerini bulur. Örn: 5 yıl 3 ay 25 gün
        public struct DateTimeSpan
        {
            public int Years { get; }
            public int Months { get; }
            public int Days { get; }
            public int Hours { get; }
            public int Minutes { get; }
            public int Seconds { get; }
            public int Milliseconds { get; }

            public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
            {
                Years = years;
                Months = months;
                Days = days;
                Hours = hours;
                Minutes = minutes;
                Seconds = seconds;
                Milliseconds = milliseconds;
            }

            enum Phase { Years, Months, Days, Done }

            public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
            {
                if (date2 < date1)
                {
                    var sub = date1;
                    date1 = date2;
                    date2 = sub;
                }

                DateTime current = date1;
                int years = 0;
                int months = 0;
                int days = 0;

                Phase phase = Phase.Years;
                DateTimeSpan span = new DateTimeSpan();
                int officialDay = current.Day;

                while (phase != Phase.Done)
                {
                    switch (phase)
                    {
                        case Phase.Years:
                            if (current.AddYears(years + 1) > date2)
                            {
                                phase = Phase.Months;
                                current = current.AddYears(years);
                            }
                            else
                            {
                                years++;
                            }
                            break;
                        case Phase.Months:
                            if (current.AddMonths(months + 1) > date2)
                            {
                                phase = Phase.Days;
                                current = current.AddMonths(months);
                                if (current.Day < officialDay && officialDay <= DateTime.DaysInMonth(current.Year, current.Month))
                                    current = current.AddDays(officialDay - current.Day);
                            }
                            else
                            {
                                months++;
                            }
                            break;
                        case Phase.Days:
                            if (current.AddDays(days + 1) > date2)
                            {
                                current = current.AddDays(days);
                                var timespan = date2 - current;
                                span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                                phase = Phase.Done;
                            }
                            else
                            {
                                days++;
                            }
                            break;
                    }
                }

                return span;
            }
        }

        /// <summary>
        /// Örnek (Ay baş-bit tarihileri verir)
        /// 01-01-2020 31-01-2020
        /// 01-02-2020 28-02-2020
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<RVehicleCostDto> TwoDateRangeMonthSplit(DateTime start, DateTime end)
        {
            DateTime monthStartDate, monthEndDate;
            List<RVehicleCostDto> listDates = new List<RVehicleCostDto>();
            monthStartDate = start.AddDays(-1 * start.Day + 1);
            monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);

            while (monthEndDate < end)
            {
                listDates.Add(new RVehicleCostDto { StartDate = start, EndDate = monthEndDate });
                monthStartDate = monthEndDate.AddDays(1);
                start = monthStartDate;
                monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);
            }

            listDates.Add(new RVehicleCostDto { StartDate = monthStartDate, EndDate = end });

            return listDates;
        }


        /// <summary>
        /// Örnek
        /// 01-01-2020 01-02-2020
        /// 01-02-2020 01-03-2020
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<RVehicleCostDto> TwoDateRangeMonthSplitAddDays_1(DateTime start, DateTime end)
        {
            DateTime monthStartDate, monthEndDate;
            List<RVehicleCostDto> listDates = new List<RVehicleCostDto>();
            monthStartDate = start.AddDays(-1 * start.Day + 1);
            monthEndDate = monthStartDate.AddMonths(1);

            while (monthEndDate < end)
            {
                listDates.Add(new RVehicleCostDto { StartDate = start, EndDate = monthEndDate, IsAllMonth = MonthLastDayControl(monthEndDate) });
                monthStartDate = monthEndDate;
                start = monthStartDate;
                monthEndDate = monthStartDate.AddMonths(1);
            }

            listDates.Add(new RVehicleCostDto { StartDate = monthStartDate, EndDate = end, IsAllMonth = MonthLastDayControl(end) });

            return listDates;
        }
        public static List<RVehicleCostDto> TwoDateRangeMonthSplitForCost(DateTime start, DateTime end)
        {
            DateTime monthStartDate, monthEndDate;
            List<RVehicleCostDto> listDates = new List<RVehicleCostDto>();
            monthStartDate = start.AddDays(-1 * start.Day + 1);
            monthEndDate = monthStartDate.AddMonths(1);

            while (monthEndDate < end)
            {
                listDates.Add(new RVehicleCostDto { StartDate = start, EndDate = monthEndDate, IsAllMonth = MonthLastDayControl(monthEndDate) });
                monthStartDate = monthEndDate;
                start = monthStartDate;
                monthEndDate = monthStartDate.AddMonths(1);
            }

            listDates.Add(new RVehicleCostDto { StartDate = start, EndDate = end, IsAllMonth = MonthLastDayControl(end) });

            return listDates;
        }
        //Ayın son günü mü
        public static bool MonthLastDayControl(DateTime endDate)
        {
            var isAllMonth = false;
            DateTime lastDayOfMonth = new DateTime(endDate.Year, endDate.Month, 1);
            if (endDate == lastDayOfMonth)
                isAllMonth = true;

            return isAllMonth;
        }

    }
}
