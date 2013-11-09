using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSystemApplication
{
    public class MonitorInfo
    {
        public string machineName;
        public string categoryName;
        public string instanceName;

        public MonitorInfo(string machineName, string categoryName, string instanceName)
        {
            this.machineName = machineName;
            this.categoryName = categoryName;
            this.instanceName = instanceName;
        }
    }
}
