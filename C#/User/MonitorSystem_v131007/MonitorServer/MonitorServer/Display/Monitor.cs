using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Timers;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using MonitorServer;

namespace MonitorSystem
{
    public class MonitorEnviroment
    {
        public static List<string> MachineNames()
        {
            List<string> lst = new List<string>();
            foreach (string machineName in Global.realtimeValueTable.Keys)
            {
                lst.Add(machineName);
            }
            return lst;
        }

        public static List<string> CategoryNames()
        {
            List<string> lst = new List<string>(); 
            foreach (string categoryNames in Global.defaultUploadPeriodTable.Keys)
            {
                lst.Add(categoryNames);
            }
            return lst;
        }

        public static List<string> InstanceNames(string machineName, string categoryName)
        {
            List<string> lst=new List<string>();
            if (Global.realtimeValueTable[machineName].ContainsKey(categoryName))
            {
                foreach (string ins in Global.realtimeValueTable[machineName][categoryName].Keys)
                {
                    lst.Add(ins);
                }
            }
            return lst;
        }

        public static Monitor CreateMonitor(string machineName, string categoryName, string instanceName)
        {
            Monitor mt = new Monitor(machineName, categoryName, instanceName);
            return mt;
        }
    }

   
    public class Monitor
    {
        protected string machineName;

        protected string categoryName;

        protected string instanceName;

        public Monitor(string machineName, string categoryName, string instanceName)
        {
            this.machineName = machineName;
            this.categoryName = categoryName;
            this.instanceName = instanceName;
        }

        public virtual string MachineName()
        {
            return machineName;
        }

        public virtual string CategoryName()
        {
            return categoryName;
        }

        public virtual string InstanceName()
        {
            return instanceName;
        }

        public virtual ValueBind GetValue()
        {
            ValueBind vb= Global.realtimeValueTable[machineName][categoryName][instanceName];
            lastDate = vb.date;
            return vb;
        }

        private DateTime lastDate;

        public bool isChanged
        {
            get
            {
                return Global.realtimeValueTable[machineName][categoryName][instanceName].date != lastDate;
            }
        }
    }
}
