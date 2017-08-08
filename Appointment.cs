using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentNotificationManager
{
    [Serializable]
    public class Appointment
    {
        // custom        
        public const string SEQUENCE = "EventSequence";

        // standard
        public const string ID = "UID";
        public const string SUBJECT = "Title";
        public const string ALL_DAY_EVENT = "All Day Event";
        public const string RECURRENCE = "Recurrence";
        public const string START_DATE = "EventDate";
        public const string END_DATE = "EndDate";
        public const string DESCRIPTION = "Description";
        public const string LOCATION = "Location";
        public const string RECURRENCE_DATA = "RecurrenceData";
        public const string EVENT_TYPE = "EventType";
        public const string ATTENDEES = "Attendees";

        public static readonly DaysAbbr[] AllDays = { DaysAbbr.MO, DaysAbbr.TU, DaysAbbr.WE, DaysAbbr.TH, DaysAbbr.FR, DaysAbbr.SA, DaysAbbr.SU };

        public string Id { get; set; }
        public string Subject { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StampDate { get; set; }
        public int Sequence { get; set; }
        public AppointmentUser Organizer { get; set; }
        public AppointmentRecurrence RecurrenceData { get; set; }
        public List<AppointmentUser> Attendees { get; set; }
        public bool AllDayEvent { get; set; }
        public int EventType { get; set; }
        public bool Recurrence
        {
            get
            {
                return RecurrenceData != null;
            }
        }

        public Appointment()
        {
            Sequence = 0;
            Id = Guid.NewGuid().ToString("N");
            Attendees = new List<AppointmentUser>();
        }
    }
}