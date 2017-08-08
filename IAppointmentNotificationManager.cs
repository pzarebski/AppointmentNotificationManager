using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentNotificationManager
{
    public interface IAppointmentNotificationManager
    {
        Appointment ReadAppintment(IAppointmentReader reader, object data);
        string GetICS(Appointment appointment, NotificationMethodType method);
    }
}
