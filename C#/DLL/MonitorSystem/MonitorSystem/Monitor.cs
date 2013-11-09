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
using System.ServiceProcess;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.Win32;

namespace MonitorSystem
{
    class SystemMonitorInfo
    {
        public string name;
        public string categoryName;
        public string counterName;
    }

    class SystemCategoryEnum
    {
        private static SystemMonitorInfo cpuUsage = new SystemMonitorInfo
        {
            name = @"%CPU Usage",
            categoryName = @"Processor",
            counterName = @"% Processor Time"
        };

        private static SystemMonitorInfo interruptTime = new SystemMonitorInfo
        {
            name = @"%Interrupt Time",
            categoryName = @"Processor",
            counterName = @"% Interrupt Time"
        };

        private static SystemMonitorInfo processorTime = new SystemMonitorInfo
        {
            name = @"%Processor Time",
            categoryName = @"Processor",
            counterName = @"% User Time"
        };

        private static SystemMonitorInfo dpcTime = new SystemMonitorInfo
        {
            name = @"%DPC Time",
            categoryName = @"Processor",
            counterName = @"% DPC Time"
        };

        private static SystemMonitorInfo logicalDiscFreeSpace = new SystemMonitorInfo
        {
            name = @"Logical Disc Free Space",
            categoryName = @"LogicalDisk",
            counterName = @"Free Megabytes"
        };

        private static SystemMonitorInfo avgLogDiskSecTransfer = new SystemMonitorInfo
        {
            name = @"Avg. Log. Disk sec/Transfer",
            categoryName = @"LogicalDisk",
            counterName = @"Avg. Disk sec/Transfer"
        };

        private static SystemMonitorInfo avgLogDiskSecRead = new SystemMonitorInfo
        {
            name = @"Avg. Log. Disk sec/Read",
            categoryName = @"LogicalDisk",
            counterName = @"Avg. Disk sec/Read"
        };

        private static SystemMonitorInfo avgLogDiskSecWrite = new SystemMonitorInfo
        {
            name = @"Avg. Log. Disk sec/Write",
            categoryName = @"LogicalDisk",
            counterName = @"Avg. Disk sec/Write"
        };

        private static SystemMonitorInfo avgPhsDiskSecTransfer = new SystemMonitorInfo
        {
            name = @"Avg. Phs. Disk sec/Transfer",
            categoryName = @"PhysicalDisk",
            counterName = @"Avg. Disk sec/Transfer"
        };

        private static SystemMonitorInfo avgPhsDiskSecRead = new SystemMonitorInfo
        {
            name = @"Avg. Phs. Disk sec/Read",
            categoryName = @"PhysicalDisk",
            counterName = @"Avg. Disk sec/Read"
        };

        private static SystemMonitorInfo avgPhsDiskSecWrite = new SystemMonitorInfo
        {
            name = @"Avg. Phs. Disk sec/Write",
            categoryName = @"PhysicalDisk",
            counterName = @"Avg. Disk sec/Write"
        };

        private static SystemMonitorInfo memoryAvailableMBytes = new SystemMonitorInfo
        {
            name = @"Memory Available MBytes",
            categoryName = @"Memory",
            counterName = @"Available MBytes"
        };

        private static SystemMonitorInfo DBLogSpaceFree = new SystemMonitorInfo
        {
            name = @"Transaction Log Space Free%",
            categoryName = @"SQLServer:Databases",
            counterName = @"Percent Log Used"
        };

        public static List<SystemMonitorInfo> windowsServiceMonitors = new List<SystemMonitorInfo> 
        {
            cpuUsage,
            interruptTime,
            processorTime,
            dpcTime,

            logicalDiscFreeSpace,
            avgLogDiskSecTransfer,
            avgLogDiskSecRead,
            avgLogDiskSecWrite,

            avgPhsDiskSecTransfer,
            avgPhsDiskSecRead,
            avgPhsDiskSecWrite,

            memoryAvailableMBytes,
            
            DBLogSpaceFree
        };
    }

    public class MonitorEnviroment
    {
        private static Dictionary<string, PartialMonitorEnviroment> mtEnvDict=new Dictionary<string,PartialMonitorEnviroment>();

        private static List<PartialMonitorEnviroment> envList = new List<PartialMonitorEnviroment>
        {
            new SystemMonitorEnviroment(),
            new DatabaseMonitorEnviroment()
        };

        static MonitorEnviroment()
        {
            foreach (PartialMonitorEnviroment plMtEnv in MonitorEnviroment.envList)
            {
                foreach (string categoryName in plMtEnv.CategoryNames())
                {
                    mtEnvDict.Add(categoryName, plMtEnv);
                }
            }
        }

        public static List<string> MachineNames()
        {
            List<string> lst = new List<string>();
            lst.Add(Environment.MachineName);
            return lst;
        }

        public static List<string> CategoryNames()
        {
            List<string> categoryNames = new List<string>(); 
            foreach (PartialMonitorEnviroment plMtEnv in envList)
            {
                categoryNames.AddRange(plMtEnv.CategoryNames());
            }
            return categoryNames;
        }

        public static List<string> InstanceNames(string machineName, string categoryName)
        {
            PartialMonitorEnviroment plMtEnv = mtEnvDict[categoryName];
            return plMtEnv.InstanceNames(machineName, categoryName);
        }

        public static Monitor CreateMonitor(string machineName, string categoryName, string instanceName)
        {
            PartialMonitorEnviroment plMtEnv = mtEnvDict[categoryName];
            Monitor mt = plMtEnv.CreateMonitor(machineName, categoryName, instanceName);
            return mt;
        }
    }

    class PartialMonitorEnviroment
    {
        public virtual List<string> CategoryNames()
        {
            return null;
        }

        public virtual List<string> InstanceNames(string machineName, string categoryName)
        {
            return null;
        }

        public virtual Monitor CreateMonitor(string machineName, string categoryName, string instanceName)
        {
            return null;
        }
    }

    class SystemMonitorEnviroment : PartialMonitorEnviroment
    {
        private static List<string> categoryNames = new List<string>();

        private static Dictionary<string, string> objectNameDict = new Dictionary<string, string>();

        private static Dictionary<string, string> counterNameDict = new Dictionary<string, string>();

        static SystemMonitorEnviroment()
        {
            foreach (SystemMonitorInfo sysInfo in SystemCategoryEnum.windowsServiceMonitors)
            {
                categoryNames.Add(sysInfo.name);
                objectNameDict.Add(sysInfo.name, sysInfo.categoryName);
                counterNameDict.Add(sysInfo.name, sysInfo.counterName);
            }
        }

        public override List<string> CategoryNames()
        {
            return categoryNames;
        }

        public override List<string> InstanceNames(string machineName, string categoryName)
        {
            List<string> lst=new List<string>();
            try
            {
                PerformanceCounterCategory pcc = new PerformanceCounterCategory(objectNameDict[categoryName], machineName);
                foreach (string name in pcc.GetInstanceNames().DefaultIfEmpty(""))
                {
                    lst.Add(name);
                }
                return lst;
            }
            catch
            {
                return null;
            }
        }

        public override Monitor CreateMonitor(string machineName, string categoryName, string instanceName)
        {
            string objectName = objectNameDict[categoryName];
            string counterName = counterNameDict[categoryName];
            try
            {
                SystemMonitor monitor = new SystemMonitor(machineName, categoryName, instanceName, objectName, counterName);
                return monitor;
            }
            catch
            {
                return null;
            }
        }
    }

    class DatabaseMonitorEnviroment : PartialMonitorEnviroment
    {
        public override List<string> CategoryNames()
        {
            return new List<string> { "DB space free%", "Per. Change in DB % Used Space" };
        }

        public override List<string> InstanceNames(string machineName, string categoryName)
        {
            List<string> lst = new List<string>();
            Server server = new Server(machineName);
            foreach (Database db in server.Databases)
            {
                lst.Add(db.Name);
            }
            return lst;
        }

        public override Monitor CreateMonitor(string machineName, string categoryName, string instanceName)
        {
            Monitor monitor=new DatabaseMonitor(machineName, categoryName, instanceName);
            return monitor;
        }
    }

    public class Monitor
    {
        protected string machineName;

        protected string categoryName;

        protected string instanceName;

        protected Monitor(string machineName, string categoryName, string instanceName)
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

        public virtual double GetValue()
        {
            return 0;
        }
    }

    class SystemMonitor : Monitor
    {
        protected PerformanceCounter pc;

        public SystemMonitor(string machineName, string categoryName, string instanceName, string objectName, string counterName)
            : base(machineName, categoryName, instanceName)
        {
            pc = new PerformanceCounter(objectName, counterName, instanceName, machineName);
            pc.NextValue();
        }

        public override double GetValue()
        {
            if (categoryName == @"Transaction Log Space Free%")
                return 100 - pc.NextValue();
            else
                return pc.NextValue();
        }
 
    }

    class DatabaseMonitor : Monitor
    {
        ServerConnection sc;
        int startTick;
        double lastUsage;

        public DatabaseMonitor(string machineName, string categoryName, string instanceName)
            : base(machineName, categoryName, instanceName)
        {
            sc = new ServerConnection(machineName);  
            startTick=Environment.TickCount;
            sc.Connect();
            Server sv = new Server(sc);
            lastUsage = sv.Databases[instanceName].DataSpaceUsage;
        }

        public override double GetValue()
        {

            sc.Connect();
            Server sv = new Server(sc);
            double size = sv.Databases[instanceName].Size;
            double avi = sv.Databases[instanceName].SpaceAvailable;
            double free= avi / (size + avi) * 100;

            if (categoryName == "DB space free%")
            {
                return free;
            }
            else if (categoryName == "Per. Change in DB % Used Space")
            {
                double tmp=sv.Databases[instanceName].DataSpaceUsage;
                double c = Math.Abs(tmp - lastUsage) / lastUsage;
                lastUsage = tmp;
                return c;
            }
            else
            {
                return 0;
            }
        }
    }
}
