namespace AppointmentNotificationManager
{
    public interface IAppointmentNotificationManager
    {
        Appointment ReadAppointment(IAppointmentReader reader, object data);
        string GetICS(Appointment appointment, NotificationMethodType method);
    }
}
