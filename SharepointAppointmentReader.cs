using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentNotificationManager
{
    public class SharepointAppointmentReader : AppointmentReaderBase
    {
        public SharepointAppointmentReader()
        {
        }

        public override Appointment Read(object data)
        {
            ListItem item = (ListItem)data;
            Appointment appointment = new Appointment();
            appointment.AllDayEvent = Convert.ToBoolean(item[Appointment.ALL_DAY_EVENT] ?? "");

            var userValueCollection = item[Appointment.ATTENDEES] as FieldUserValue[];
            if (userValueCollection != null && userValueCollection.Length > 0)
            {
                foreach (FieldUserValue uservalue in userValueCollection)
                {
                    if (uservalue != null && string.IsNullOrEmpty(uservalue.Email) == false)
                    {
                        AppointmentUser a = new AppointmentUser();
                        a.DisplayName = uservalue.LookupValue;
                        a.Email = uservalue.Email;
                        appointment.Attendees.Add(a);
                    }
                }
            }

            appointment.Subject = (item[Appointment.SUBJECT] ?? "").ToString();
            appointment.Description = (item[Appointment.DESCRIPTION] ?? "").ToString();
            appointment.EndDate = Convert.ToDateTime(item[Appointment.END_DATE].ToString());
            appointment.EventType = (int)item[Appointment.EVENT_TYPE];
            appointment.Id = (item[Appointment.ID] ?? "").ToString();
            appointment.Location = (string)item[Appointment.LOCATION];

            var userValue = item["Author"] as FieldUserValue;
            if (userValue != null && string.IsNullOrEmpty(userValue.Email) == false)
            {
                appointment.Organizer = new AppointmentUser() { DisplayName = userValue.LookupValue, Email = userValue.Email };
            }
            bool recurrence = Convert.ToBoolean(item[Appointment.RECURRENCE]);
            // read recurrence data
            if (recurrence)
            {
                appointment.RecurrenceData = ReadRecurrenceData((item[Appointment.RECURRENCE_DATA] ?? "").ToString());
            }
            appointment.Sequence = (int)double.Parse((item[Appointment.SEQUENCE] ?? 0).ToString());
            appointment.StampDate = DateTime.Now;
            appointment.StartDate = Convert.ToDateTime(item[Appointment.START_DATE].ToString());

            return appointment;
        }
    }
}
