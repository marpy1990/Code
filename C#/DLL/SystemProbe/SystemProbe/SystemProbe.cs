using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;


namespace SystemProbe
{
    public class SystemProbe:Probe.Probe
    {
        #region Version1.0
        /*
        private class CounterBind
        {
            public PerformanceCounter pc;
            public string categoryName;
            public string instanceName;
            public CounterBind(PerformanceCounter pc, string categoryName, string instanceName)
            {
                this.pc=pc;
                this.categoryName=categoryName;
                this.instanceName=instanceName;
            }
        }

        List<CounterBind> counterList = new List<CounterBind>();

        public SystemProbe()
        {
            string machineName = Environment.MachineName;
            foreach (SysInfo pcObj in SysMonitor.WindowsServiceMonitors)
            {
                PerformanceCounterCategory pcc = new PerformanceCounterCategory(pcObj.objectName);
                foreach (string instanceName in pcc.GetInstanceNames().DefaultIfEmpty(""))
                {
                    PerformanceCounter pc = new PerformanceCounter(pcObj.objectName, pcObj.counterName, instanceName, machineName);
                    CounterBind bind=new CounterBind(pc,pcObj.Name,instanceName);
                    counterList.Add(bind);
                }
            }
        }

        ICollection<Probe.DetectedData> Probe.Probe.GetValues()
        {
            
            List<Probe.DetectedData> lst = new List<Probe.DetectedData>();

            foreach (CounterBind bind in counterList)
            {
                Probe.DetectedData data = new Probe.DetectedData();
                data.categoryName = bind.categoryName;
                data.instanceName = bind.instanceName;
                data.value = bind.pc.NextValue();
                lst.Add(data);
            }
            return lst;
        }
        */
        #endregion
       
        #region Version2.0
        /*
        private Dictionary<string, Dictionary<string, PerformanceCounter>> categoryTable = new Dictionary<string, Dictionary<string, PerformanceCounter>>();

        private Semaphore categoryTableSem = new Semaphore(1, 1);

        ICollection<Probe.DetectedData> Probe.Probe.GetValues()
        {
            List<Probe.DetectedData> lst = new List<Probe.DetectedData>();
            string machineName = Environment.MachineName;
            foreach (SysInfo pcObj in SysMonitor.WindowsServiceMonitors)
            {
                PerformanceCounterCategory pcc = new PerformanceCounterCategory(pcObj.objectName);
                foreach (string instanceName in pcc.GetInstanceNames().DefaultIfEmpty(""))
                {
                    categoryTableSem.WaitOne();
                    Probe.DetectedData data = new Probe.DetectedData();
                    if (!categoryTable[pcObj.Name].ContainsKey(instanceName))
                    {
                        PerformanceCounter pc = new PerformanceCounter(pcObj.objectName, pcObj.counterName, instanceName, machineName);
                        categoryTable[pcObj.Name].Add(instanceName, pc);
                    }
                    data.categoryName = pcObj.Name;
                    data.instanceName = instanceName;
                    data.value = categoryTable[pcObj.Name][instanceName].NextValue();
                    categoryTableSem.Release();
                    lst.Add(data);
                }
            }
            return lst;
        }

        public SystemProbe()
        {
            foreach (SysInfo pcObj in SysMonitor.WindowsServiceMonitors)
            {
                categoryTable.Add(pcObj.Name, new Dictionary<string, PerformanceCounter>());
            }
        }*/
        #endregion

        #region Version3.0
        private Dictionary<string, HashSet<string>> categoryTable = new Dictionary<string, HashSet<string>>();

        private Semaphore categoryTableSem = new Semaphore(1, 1);

        private Timer checkInstanceTimer;

        private void CheckInstance()
        {
            categoryTableSem.WaitOne();
            string machineName = Environment.MachineName;
            foreach (SysInfo pcObj in SysMonitor.WindowsServiceMonitors)
            {
                PerformanceCounterCategory pcc = new PerformanceCounterCategory(pcObj.objectName);
                foreach (string instanceName in pcc.GetInstanceNames().DefaultIfEmpty(""))
                {
                    if (!categoryTable[pcObj.Name].Contains(instanceName))
                    {
                        PerformanceCounter pc = new PerformanceCounter(pcObj.objectName, pcObj.counterName, instanceName, machineName);
                        CounterBind bind = new CounterBind(pc, pcObj.Name, instanceName);
                        counterList.Add(bind);
                        categoryTable[pcObj.Name].Add(instanceName);
                    }
                }
            }
            categoryTableSem.Release();
        }

        private class CounterBind
        {
            public PerformanceCounter pc;
            public string categoryName;
            public string instanceName;
            public CounterBind(PerformanceCounter pc, string categoryName, string instanceName)
            {
                this.pc = pc;
                this.categoryName = categoryName;
                this.instanceName = instanceName;
            }
        }

        List<CounterBind> counterList = new List<CounterBind>();

        public SystemProbe()
        {
            string machineName = Environment.MachineName;
            foreach (SysInfo pcObj in SysMonitor.WindowsServiceMonitors)
            {
                categoryTable.Add(pcObj.Name, new HashSet<string>());
                PerformanceCounterCategory pcc = new PerformanceCounterCategory(pcObj.objectName);
                foreach (string instanceName in pcc.GetInstanceNames().DefaultIfEmpty(""))
                {
                    PerformanceCounter pc = new PerformanceCounter(pcObj.objectName, pcObj.counterName, instanceName, machineName);
                    CounterBind bind = new CounterBind(pc, pcObj.Name, instanceName);
                    counterList.Add(bind);
                    categoryTable[pcObj.Name].Add(instanceName);
                }
            }
            checkInstanceTimer = new Timer((object state) => this.CheckInstance());
            checkInstanceTimer.Change(1000, 1000);
        }

        ICollection<Probe.DetectedData> Probe.Probe.GetValues()
        {

            List<Probe.DetectedData> lst = new List<Probe.DetectedData>();

            foreach (CounterBind bind in counterList)
            {
                try
                {
                    Probe.DetectedData data = new Probe.DetectedData();
                    data.categoryName = bind.categoryName;
                    data.instanceName = bind.instanceName;
                    data.value = bind.pc.NextValue();
                    lst.Add(data);
                }
                catch
                {
                }
            }
            return lst;
        }
        #endregion
         
    }

    class SysInfo
    {
        public string Name;
        public string objectName;
        public string counterName;
    }

    class SysMonitor
    {
        private static SysInfo CpuUsage = new SysInfo
        {
            Name = @"%CPU Usage",
            objectName = @"Processor",
            counterName = @"% Processor Time"
        };

        private static SysInfo InterruptTime = new SysInfo
        {
            Name = @"%Interrupt Time",
            objectName = @"Processor",
            counterName = @"% Interrupt Time"
        };

        private static SysInfo ProcessorTime = new SysInfo
        {
            Name = @"%Processor Time",
            objectName = @"Processor",
            counterName = @"% User Time"
        };

        private static SysInfo DPCTime = new SysInfo
        {
            Name = @"%DPC Time",
            objectName = @"Processor",
            counterName = @"% DPC Time"
        };

        private static SysInfo LogicalDiscFreeSpace = new SysInfo
        {
            Name = @"Logical Disc Free Space",
            objectName = @"LogicalDisk",
            counterName = @"Free Megabytes"
        };

        private static SysInfo AvgLogDiskSecTransfer = new SysInfo
        {
            Name = @"Avg. Log. Disk sec/Transfer",
            objectName = @"LogicalDisk",
            counterName = @"Avg. Disk sec/Transfer"
        };

        private static SysInfo AvgLogDiskSecRead = new SysInfo
        {
            Name = @"Avg. Log. Disk sec/Read",
            objectName = @"LogicalDisk",
            counterName = @"Avg. Disk sec/Read"
        };

        private static SysInfo AvgLogDiskSecWrite = new SysInfo
        {
            Name = @"Avg. Log. Disk sec/Write",
            objectName = @"LogicalDisk",
            counterName = @"Avg. Disk sec/Write"
        };

        private static SysInfo AvgPhsDiskSecTransfer = new SysInfo
        {
            Name = @"Avg. Phs. Disk sec/Transfer",
            objectName = @"PhysicalDisk",
            counterName = @"Avg. Disk sec/Transfer"
        };

        private static SysInfo AvgPhsDiskSecRead = new SysInfo
        {
            Name = @"Avg. Phs. Disk sec/Read",
            objectName = @"PhysicalDisk",
            counterName = @"Avg. Disk sec/Read"
        };

        private static SysInfo AvgPhsDiskSecWrite = new SysInfo
        {
            Name = @"Avg. Phs. Disk sec/Write",
            objectName = @"PhysicalDisk",
            counterName = @"Avg. Disk sec/Write"
        };

        private static SysInfo MemoryAvailableMBytes = new SysInfo
        {
            Name = @"Memory Available MBytes",
            objectName = @"Memory",
            counterName = @"Available MBytes"
        };

        public static List<SysInfo> WindowsServiceMonitors = new List<SysInfo> 
        {
            CpuUsage,
            InterruptTime,
            ProcessorTime,
            DPCTime,
            
            LogicalDiscFreeSpace,
            AvgLogDiskSecTransfer,
            AvgLogDiskSecRead,
            AvgLogDiskSecWrite,
            
            AvgPhsDiskSecTransfer,
            AvgPhsDiskSecRead,
            AvgPhsDiskSecWrite,
            
            MemoryAvailableMBytes
        };
    }
}
