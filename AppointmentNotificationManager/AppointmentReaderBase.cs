using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppointmentNotificationManager
{
    public abstract class AppointmentReaderBase : IAppointmentReader
    {
        public abstract Appointment Read(object data);

        public virtual AppointmentRecurrence ReadRecurrenceData(string recurrenceXml)
        {
            if (string.IsNullOrEmpty(recurrenceXml)) return null;

            XDocument xDoc = XDocument.Parse(recurrenceXml);
            AppointmentRecurrence ar = new AppointmentRecurrence();

            XElement rule = xDoc.Element("recurrence").Element("rule");
            XElement firstDayOfWeek = rule.Element("firstDayOfWeek");
            XElement repeatForever = rule.Element("repeatForever");
            XElement repeat = rule.Element("repeat");

            if (firstDayOfWeek != null)
            {
                var value = firstDayOfWeek.Value;
                ar.WeekStart = (DaysAbbr)Enum.Parse(typeof(DaysAbbr), value.ToUpper());
            }
            else
            {
                ar.WeekStart = DaysAbbr.MO;
            }
            if (repeatForever != null)
            {
                var value = repeatForever.Value;
                ar.RepeatForever = Convert.ToBoolean(value);
            }
            else
            {
                ar.RepeatForever = false;
            }

            var windowEnd = rule.Element("windowEnd");
            if (windowEnd != null)
            {
                ar.UntilDate = DateTime.ParseExact(windowEnd.Value, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.CurrentCulture);
            }

            var repeatInstances = rule.Element("repeatInstances");
            if (repeatInstances != null)
            {
                ar.RepeatInstances = int.Parse(repeatInstances.Value);
            }

            var daily = repeat.Element("daily");
            var weekly = repeat.Element("weekly");
            var monthly = repeat.Element("monthly");
            var yearly = repeat.Element("yearly");

            if (daily != null)
            {
                ar.Type = RecuringType.DAILY;
                var dayFrequency = daily.Attribute("dayFrequency");
                if (dayFrequency != null)
                {
                    ar.Frequency = int.Parse(dayFrequency.Value);
                }


            }
            else if (weekly != null)
            {
                ar.Type = RecuringType.WEEKLY;
                var weekFrequency = weekly.Attribute("weekFrequency");
                if (weekFrequency != null)
                {
                    ar.Frequency = int.Parse(weekFrequency.Value);
                }

                var names = Appointment.AllDays.Select(day => day.ToString());
                foreach (XAttribute attr in weekly.Attributes())
                {
                    if (names.Contains(attr.Name.LocalName.ToUpper()) && Convert.ToBoolean(attr.Value) == true)
                    {
                        ar.Days.Add((DaysAbbr)Enum.Parse(typeof(DaysAbbr), attr.Name.LocalName.ToUpper()));
                    }
                }
            }
            else if (monthly != null)
            {
                ar.Type = RecuringType.MONTHLY;
                var monthFrequency = monthly.Attribute("monthFrequency");
                if (monthFrequency != null)
                {
                    ar.Frequency = int.Parse(monthFrequency.Value);
                }
                var day = monthly.Attribute("day");
                if (day != null)
                {
                    ar.MonthDay = int.Parse(day.Value);
                }
            }
            else if (yearly != null)
            {
                ar.Type = RecuringType.YEARLY;
                var yearFrequency = yearly.Attribute("yearFrequency");
                if (yearFrequency != null)
                {
                    ar.Frequency = int.Parse(yearFrequency.Value);
                }
                var day = yearly.Attribute("day");
                if (day != null)
                {
                    ar.MonthDay = int.Parse(day.Value);
                }
                var month = yearly.Attribute("month");
                {
                    ar.Month = int.Parse(month.Value);
                }
            }

            return ar;
        }
    }
}
