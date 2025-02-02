namespace AppointmentNotificationManager.Tests
{
    public class AppointmentNotificationManagerTests
    {
        private readonly AppointmentNotificationManager _manager = new AppointmentNotificationManager();

        [Fact]
        public void ReadAppintment_ReturnsAppointmentFromReader()
        {
            IAppointmentReader reader = new FakeAppointmentReader();
            var dummyData = new object();

            var appointment = _manager.ReadAppointment(reader, dummyData);

            Assert.NotNull(appointment);
            Assert.Equal("fake-id", appointment.Id);
        }

        [Fact]
        public void GetICS_CreateMethod_NonAllDayEvent_ReturnsCorrectFormat()
        {
            var appointment = new Appointment
            {
                Id = "12345",
                Subject = "Test Meeting",
                StartDate = new DateTime(2025, 05, 01, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 05, 01, 11, 0, 0, DateTimeKind.Utc),
                StampDate = new DateTime(2025, 04, 30, 12, 0, 0, DateTimeKind.Utc),
                Location = "Conference Room",
                Description = "Meeting Description",
                Sequence = 5,
                AllDayEvent = false,
                Attendees = new List<AppointmentUser>(),
                Organizer = null,
                RecurrenceData = null
            };

            var ics = _manager.GetICS(appointment, NotificationMethodType.Create);

            // basic ICS elements
            Assert.Contains("BEGIN:VCALENDAR", ics);
            Assert.Contains("METHOD:REQUEST", ics);
            Assert.Contains("UID:12345", ics);
            Assert.Contains("DTSTART:" + appointment.StartDate.ToString("yyyyMMddTHHmmssZ"), ics);
            Assert.Contains("DTEND:" + appointment.EndDate.ToString("yyyyMMddTHHmmssZ"), ics);
            Assert.Contains("SUMMARY:Test Meeting", ics);
            // for method Create sequence is not incrementd
            Assert.Contains("SEQUENCE:" + appointment.Sequence, ics);
        }

        [Fact]
        public void GetICS_UpdateMethod_IncrementsSequence()
        {
            var appointment = new Appointment
            {
                Id = "12345",
                Subject = "Updated Meeting",
                StartDate = new DateTime(2025, 05, 01, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 05, 01, 11, 0, 0, DateTimeKind.Utc),
                StampDate = new DateTime(2025, 04, 30, 12, 0, 0, DateTimeKind.Utc),
                Location = "Conference Room",
                Description = "Updated Description",
                Sequence = 5,
                AllDayEvent = false,
                Attendees = new List<AppointmentUser>(),
                Organizer = null,
                RecurrenceData = null
            };

            var ics = _manager.GetICS(appointment, NotificationMethodType.Update);

            // for Update method sequence should be incremented
            Assert.Contains("SEQUENCE:" + (appointment.Sequence + 1), ics);
            Assert.Contains("X-MICROSOFT-CDO-APPT-SEQUENCE:" + (appointment.Sequence + 1), ics);
        }

        [Fact]
        public void GetICS_DeleteMethod_SetsMethodToCancel()
        {
            var appointment = new Appointment
            {
                Id = "12345",
                Subject = "Canceled Meeting",
                StartDate = new DateTime(2025, 05, 01, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 05, 01, 11, 0, 0, DateTimeKind.Utc),
                StampDate = new DateTime(2025, 04, 30, 12, 0, 0, DateTimeKind.Utc),
                Location = "Conference Room",
                Description = "Canceled Description",
                Sequence = 5,
                AllDayEvent = false,
                Attendees = new List<AppointmentUser>(),
                Organizer = null,
                RecurrenceData = null
            };

            var ics = _manager.GetICS(appointment, NotificationMethodType.Delete);

            // for Delete method the ICS METHOD value should be set to CANCEL
            Assert.Contains("METHOD:CANCEL", ics);
        }

        [Fact]
        public void GetICS_AllDayEvent_UsesDateOnlyFormat()
        {
            var appointment = new Appointment
            {
                Id = "all-day-1",
                Subject = "All Day Event",
                StartDate = new DateTime(2025, 05, 01, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 05, 02, 0, 0, 0, DateTimeKind.Utc),
                StampDate = new DateTime(2025, 04, 30, 12, 0, 0, DateTimeKind.Utc),
                Location = "Office",
                Description = "All day event description",
                Sequence = 1,
                AllDayEvent = true,
                Attendees = new List<AppointmentUser>(),
                Organizer = null,
                RecurrenceData = null
            };

            var ics = _manager.GetICS(appointment, NotificationMethodType.Create);

            // for all day event date format should be set without time part
            Assert.Contains("DTSTART;VALUE=DATE:" + appointment.StartDate.ToString("yyyyMMdd"), ics);
            Assert.Contains("DTEND;VALUE=DATE:" + appointment.EndDate.ToString("yyyyMMdd"), ics);
            // it should not contain full date and time
            Assert.DoesNotContain("DTSTART:" + appointment.StartDate.ToString("yyyyMMddTHHmmssZ"), ics);
        }

        [Fact]
        public void GetICS_IncludesOrganizerAndAttendees()
        {
            var organizer = new AppointmentUser { DisplayName = "Organizer", Email = "organizer@test.com" };
            var attendee = new AppointmentUser { DisplayName = "Attendee", Email = "attendee@test.com" };

            var appointment = new Appointment
            {
                Id = "org-att-1",
                Subject = "Meeting with Attendees",
                StartDate = new DateTime(2025, 06, 01, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 06, 01, 11, 0, 0, DateTimeKind.Utc),
                StampDate = new DateTime(2025, 05, 31, 9, 0, 0, DateTimeKind.Utc),
                Location = "Main Office",
                Description = "Meeting description",
                Sequence = 2,
                AllDayEvent = false,
                Organizer = organizer,
                Attendees = new List<AppointmentUser> { attendee },
                RecurrenceData = null
            };

            var ics = _manager.GetICS(appointment, NotificationMethodType.Create);

            // verify if generated ICS contains organizer and participants
            Assert.Contains($"ORGANIZER;CN={organizer.DisplayName}:MAILTO:{organizer.Email}", ics);
            Assert.Contains($"ATTENDEE;CUTYPE=INDIVIDUAL;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=FALSE;CN={attendee.DisplayName};X-NUM-GUESTS=0:mailto:{attendee.Email}", ics);
        }

        [Fact]
        public void GetICS_IncludesRecurrenceRule_WhenRecurrenceIsSet()
        {
            var recurrence = new AppointmentRecurrence
            {
                Type = RecuringType.WEEKLY,
                UntilDate = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                RepeatInstances = 10,
                Frequency = 1,
                MonthDay = 0,
                Month = 0,
                Days = new List<DaysAbbr> { DaysAbbr.MO, DaysAbbr.WE, DaysAbbr.FR },
                WeekStart = DaysAbbr.MO
            };

            var appointment = new Appointment
            {
                Id = "recurr-1",
                Subject = "Recurring Meeting",
                StartDate = new DateTime(2025, 07, 01, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2025, 07, 01, 11, 0, 0, DateTimeKind.Utc),
                StampDate = new DateTime(2025, 06, 30, 9, 0, 0, DateTimeKind.Utc),
                Location = "Conference Hall",
                Description = "Recurring event description",
                Sequence = 3,
                AllDayEvent = false,
                Attendees = new List<AppointmentUser>(),
                Organizer = null,
                RecurrenceData = recurrence
            };

            var ics = _manager.GetICS(appointment, NotificationMethodType.Create);

            // verify if generated ICS contains proper RRULE
            Assert.Contains("RRULE:FREQ=WEEKLY", ics);
            Assert.Contains("UNTIL=" + recurrence.UntilDate.Value.ToString("yyyyMMddTHHmmssZ"), ics);
            Assert.Contains("COUNT=10", ics);
            Assert.Contains("INTERVAL=1", ics);
            Assert.Contains("BYDAY=MO,WE,FR", ics);
            Assert.Contains("WKST=MO", ics);
        }
    }
}