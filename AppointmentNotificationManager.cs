using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppointmentNotificationManager
{
    public class AppointmentNotificationManager : IAppointmentNotificationManager
    {
        const string ICS_DATE_TIME_FORMAT = "yyyyMMddTHHmmssZ";
        const string ICS_DATE_FORMAT = "yyyyMMdd";

        public AppointmentNotificationManager()
        {

        }

        public Appointment ReadAppintment(IAppointmentReader reader, object data)
        {
            return reader.Read(data);
        }

        public string GetICS(Appointment appointment, NotificationMethodType method)
        {
            string methodIcs = "REQUEST";
            int sequence = appointment.Sequence;
            switch (method)
            {
                case NotificationMethodType.Create:
                    break;
                case NotificationMethodType.Update:
                    sequence = appointment.Sequence + 1;
                    break;
                case NotificationMethodType.Delete:
                    methodIcs = "CANCEL";
                    break;
                default:
                    break;
            }
            List<string> contents = new List<string>();



            contents.AddRange(new string[] {
                                  "BEGIN:VCALENDAR",
                                  "PRODID:-//Custom//CustomAppointmentSender//EN",
                                  "VERSION:2.0",
                                  "CALSCALE:GREGORIAN",
                                  "METHOD:" + methodIcs,
                                  "BEGIN:VTIMEZONE",
                                  "TZID:" + TimeZone.CurrentTimeZone.StandardName,
                                  "BEGIN:STANDARD",
                                  "DTSTART:16010101T030000",
                                  "TZOFFSETFROM:+0200",
                                  "TZOFFSETTO:+0100;",
                                  "RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=-1SU;BYMONTH=10",
                                  "END:STANDARD",
                                  "BEGIN:DAYLIGHT",
                                  "DTSTART:16010101T020000",
                                  "TZOFFSETFROM:+0100",
                                  "TZOFFSETTO:+0200",
                                  "RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=-1SU;BYMONTH=3",
                                  "END:DAYLIGHT",
                                  "END:VTIMEZONE",
                                  "BEGIN:VEVENT" });
            if (appointment.AllDayEvent)
            {
                contents.AddRange(new string[] {
                                  "DTSTART;VALUE=DATE:" + appointment.StartDate.ToString(ICS_DATE_FORMAT),
                                  "DTEND;VALUE=DATE:" + appointment.EndDate.ToString(ICS_DATE_FORMAT) });
            }
            else
            {
                contents.AddRange(new string[] {
                                  "DTSTART:" + appointment.StartDate.ToString(ICS_DATE_TIME_FORMAT),
                                  "DTEND:" + appointment.EndDate.ToString(ICS_DATE_TIME_FORMAT) });
            }

            contents.AddRange(new string[] {
                                  "UID:" + appointment.Id,
                                  "PRIORITY:3",
                                  "DTSTAM:" + appointment.StampDate.ToString(ICS_DATE_TIME_FORMAT),
                                  "LOCATION:" + appointment.Location,
                                  "DESCRIPTION;ENCODING=QUOTED-PRINTABLE:" + XmlHelper.StripXml(appointment.Description),
                                  "X-ALT-DESC;FMTTYPE=text/html:" + XmlHelper.GetHtmlDocument(appointment.Description)});

            if (appointment.Organizer != null)
            {
                contents.Add(string.Format("ORGANIZER;CN={0}:MAILTO:{1}", appointment.Organizer.DisplayName, appointment.Organizer.Email));
            }

            foreach (AppointmentUser a in appointment.Attendees)
            {
                contents.Add(string.Format("ATTENDEE;CUTYPE=INDIVIDUAL;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=FALSE;CN={0};X-NUM-GUESTS=0:mailto:{1}", a.DisplayName, a.Email));
            }

            if (appointment.Recurrence)
                contents.Add(GetRRULE(appointment.RecurrenceData));


            contents.AddRange(new string[] {
                                  "SEQUENCE:" + sequence.ToString(),
                                  "X-MICROSOFT-CDO-APPT-SEQUENCE:" + sequence.ToString(),
                                  "X-MICROSOFT-CDO-BUSYSTATUS:TENTATIVE",
                                  "X-MICROSOFT-CDO-INTENDEDSTATUS:BUSY",
                                  "X-MICROSOFT-CDO-ALLDAYEVENT:" + appointment.AllDayEvent.ToString().ToUpper(),
                                  "X-MICROSOFT-CDO-IMPORTANCE:1",
                                  "X-MICROSOFT-CDO-INSTTYPE:0",
                                  "X-MICROSOFT-DISALLOW-COUNTER:FALSE",
                                  "SUMMARY:" + appointment.Subject,
                                  "END:VEVENT",
                                  "END:VCALENDAR" });

            return string.Join("\r\n", contents.ToArray());
        }

        private string GetRRULE(AppointmentRecurrence recurrence)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("RRULE:");
            sb.Append("FREQ=");
            sb.Append(recurrence.Type.ToString());
            if (recurrence.UntilDate != null)
            {
                sb.Append(";UNTIL=");
                sb.Append(recurrence.UntilDate.Value.ToString(ICS_DATE_TIME_FORMAT));
            }

            if (recurrence.RepeatInstances > 0)
            {
                sb.Append(";COUNT=");
                sb.Append(recurrence.RepeatInstances.ToString());
            }

            if (recurrence.Frequency > 0)
            {
                sb.Append(";INTERVAL=");
                sb.Append(recurrence.Frequency);
            }

            if (recurrence.MonthDay > 0)
            {
                sb.Append(";BYMONTHDAY=");
                sb.Append(recurrence.MonthDay.ToString());
            }

            if (recurrence.Month > 0)
            {
                sb.Append(";BYMONTH=");
                sb.Append(recurrence.Month.ToString());
            }

            switch (recurrence.Type)
            {
                case RecuringType.DAILY:
                    break;
                case RecuringType.WEEKLY:
                    if (recurrence.Days.Count > 0)
                    {
                        sb.Append(";BYDAY=");
                        sb.Append(string.Join(",", recurrence.Days.Select(d => d.ToString()).ToArray()));
                    }
                    break;
                case RecuringType.MONTHLY:
                    break;
                case RecuringType.YEARLY:
                    break;
                default:
                    break;
            }

            sb.Append(";WKST=");
            sb.Append(recurrence.WeekStart.ToString());

            return sb.ToString();
        }
    }
}