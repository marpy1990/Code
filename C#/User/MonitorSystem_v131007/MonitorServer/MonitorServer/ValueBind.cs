using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorServer
{
    public class ValueBind
    {
        public object value;
        public DateTime date;
        public ValueBind(object value, DateTime date)
        {
            this.value = value;
            this.date = date;
        }
    }
}
