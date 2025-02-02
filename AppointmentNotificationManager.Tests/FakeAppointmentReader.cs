namespace AppointmentNotificationManager.Tests
{
    public class FakeAppointmentReader : IAppointmentReader
    {
        public Appointment Read(object data)
        {
            return new Appointment
            {
                Id = "fake-id",
                Subject = "Fake Subject",
                StartDate = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 1, 1, 11, 0, 0, DateTimeKind.Utc),
                StampDate = new DateTime(2025, 1, 1, 9, 0, 0, DateTimeKind.Utc),
                Location = "Fake Location",
                Description = "Fake Description",
                Sequence = 0,
                AllDayEvent = false,
                Attendees = new List<AppointmentUser>(),
                Organizer = null,
                RecurrenceData = null
            };
        }

        public AppointmentRecurrence ReadRecurrenceData(string recurrenceXml)
        {
            return null;
        }
    }
}