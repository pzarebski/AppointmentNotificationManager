using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentNotificationManager
{
    [Serializable]
    public class AppointmentRecurrence
    {
        public AppointmentRecurrence()
        {
            Days = new List<DaysAbbr>();
            UntilDate = null;
            RepeatInstances = -1;
            Frequency = -1;
        }

        public RecuringType Type { get; set; }
        public List<DaysAbbr> Days { get; set; }
        public DateTime? UntilDate { get; set; }
        public int RepeatInstances { get; set; }
        public bool RepeatForever { get; set; }
        public DaysAbbr WeekStart { get; set; }
        public int Frequency { get; set; }
        public int Month { get; set; }
        public int MonthDay { get; set; }
    }
}