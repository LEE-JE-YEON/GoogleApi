using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Data
{
    class CalendarEvent
    {
        public DateTime? Date { set; get; }
        public string Title { set; get; }
        public int Dday { set; get; }
    }
}
