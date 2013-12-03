using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Management;


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

        double memoryCapacity = 0;

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

            ManagementClass cimobject1 = new ManagementClass("Win32_PhysicalMemory");
            ManagementObjectCollection moc1 = cimobject1.GetInstances();
            foreach (ManagementObject mo1 in moc1)
            {
                this.memoryCapacity += ((Math.Round(Int64.Parse(mo1.Properties["Capacity"].Value.ToString()) / 1024 / 1024 / 1024.0, 1)));
            }
            moc1.Dispose();
            cimobject1.Dispose();

            checkInstanceTimer = new Timer((object state) => this.CheckInstance());
            checkInstanceTimer.Change(1000, 1000);
        }

        ICollection<Probe.DetectedData> Probe.Probe.GetValues()
        {

            List<Probe.DetectedData> lst = new List<Probe.DetectedData>();
            Dictionary<string, double> ins2Space = new Dictionary<string, double>();
            Dictionary<string, double> ins2SpacePer = new Dictionary<string, double>();
            float memAvi = 0;

            foreach (CounterBind bind in counterList)
            {
                try
                {
                    Probe.DetectedData data = new Probe.DetectedData();
                    data.categoryName = bind.categoryName;
                    if (bind.instanceName.Length > 30)
                    {
                        data.instanceName = bind.instanceName.Substring(0, 30);
                    }
                    else
                    {
                        data.instanceName = bind.instanceName;
                    }
                    data.value = bind.pc.NextValue();
                    if (data.categoryName == SysMonitor.MemoryAvailableMBytes.Name)
                    {
                        memAvi = (float)data.value;
                    }
                    else if (data.categoryName == SysMonitor.LogicalDiscFreeSpace.Name)
                    {
                        ins2Space.Add(data.instanceName, (float)data.value);
                    }
                    else if (data.categoryName == SysMonitor.LogicalDiscFreeSpacePer.Name)
                    {
                        ins2SpacePer.Add(data.instanceName, (float)data.value);
                    }
                    lst.Add(data);
                }
                catch
                {
                }
                
            }
            Probe.DetectedData memdata = new Probe.DetectedData();
            memdata.categoryName = "内存总量(GB)";
            memdata.instanceName = "";
            memdata.value = this.memoryCapacity;
            lst.Add(memdata);

            Probe.DetectedData memdataPer = new Probe.DetectedData();
            memdataPer.categoryName = "内存使用百分比(百分数)";
            memdataPer.instanceName = "";
            memdataPer.value = (1 - memAvi / this.memoryCapacity / 1024) * 100;
            lst.Add(memdataPer);

            foreach (string instance in ins2Space.Keys)
            {
                Probe.DetectedData diskdataTotal = new Probe.DetectedData();
                diskdataTotal.categoryName = "逻辑磁盘总空间(GB)";
                diskdataTotal.instanceName = instance;
                diskdataTotal.value = ins2Space[instance] / ins2SpacePer[instance] * 100 / 1024;
                lst.Add(diskdataTotal);
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
            Name = @"CPU总使用率百分比(百分数)",
            objectName = @"Processor",
            counterName = @"% Processor Time"
        };

        private static SysInfo InterruptTime = new SysInfo
        {
            Name = @"处理器总中断时间百分比(百分数)",
            objectName = @"Processor",
            counterName = @"% Interrupt Time"
        };

        private static SysInfo ProcessorTime = new SysInfo
        {
            Name = @"处理器处理时间总数百分比(百分数)",
            objectName = @"Processor",
            counterName = @"% User Time"
        };

        private static SysInfo DPCTime = new SysInfo
        {
            Name = @"处理器总DPC时间百分比(百分数)",
            objectName = @"Processor",
            counterName = @"% DPC Time"
        };

        public static SysInfo LogicalDiscFreeSpace = new SysInfo
        {
            Name = @"逻辑磁盘可用空间(MB)",
            objectName = @"LogicalDisk",
            counterName = @"Free Megabytes"
        };

        public static SysInfo LogicalDiscFreeSpacePer = new SysInfo
        {
            Name = @"逻辑磁盘可用空间百分比(百分数)",
            objectName = @"LogicalDisk",
            counterName = @"% Free Space"
        };

        private static SysInfo AvgLogDiskSecTransfer = new SysInfo
        {
            Name = @"逻辑磁盘每次传输的平均秒数(秒)",
            objectName = @"LogicalDisk",
            counterName = @"Avg. Disk sec/Transfer"
        };

        private static SysInfo AvgLogDiskSecRead = new SysInfo
        {
            Name = @"逻辑磁盘每次读取的平均秒数(秒)",
            objectName = @"LogicalDisk",
            counterName = @"Avg. Disk sec/Read"
        };

        private static SysInfo AvgLogDiskSecWrite = new SysInfo
        {
            Name = @"逻辑磁盘每次写入的平均秒数(秒)",
            objectName = @"LogicalDisk",
            counterName = @"Avg. Disk sec/Write"
        };

        private static SysInfo LogDiskMBytesTransfer = new SysInfo
        {
            Name = @"逻辑磁盘传输速率(MB/s)",
            objectName = @"LogicalDisk",
            counterName = @"Disk Transfers/sec"
        };

        private static SysInfo LogDiskMBytesRead = new SysInfo
        {
            Name = @"逻辑磁盘读取速率(MB/s)",
            objectName = @"LogicalDisk",
            counterName = @"Disk Reads/sec"
        };

        private static SysInfo LogDiskMBytesWrite = new SysInfo
        {
            Name = @"逻辑磁盘写入速率(MB/s)",
            objectName = @"LogicalDisk",
            counterName = @"Disk Writes/sec"
        };

        private static SysInfo AvgPhsDiskSecTransfer = new SysInfo
        {
            Name = @"实体磁盘每次传输的平均秒数(秒)",
            objectName = @"PhysicalDisk",
            counterName = @"Avg. Disk sec/Transfer"
        };

        private static SysInfo AvgPhsDiskSecRead = new SysInfo
        {
            Name = @"实体磁盘每次读取的平均秒数(秒)",
            objectName = @"PhysicalDisk",
            counterName = @"Avg. Disk sec/Read"
        };

        private static SysInfo AvgPhsDiskSecWrite = new SysInfo
        {
            Name = @"实体磁盘每次写入的平均秒数(秒)",
            objectName = @"PhysicalDisk",
            counterName = @"Avg. Disk sec/Write"
        };

        private static SysInfo PhsDiskMBytesTransfer = new SysInfo
        {
            Name = @"实体磁盘传输速率(MB/s)",
            objectName = @"PhysicalDisk",
            counterName = @"Disk Transfers/sec"
        };

        private static SysInfo PhsDiskMBytesRead = new SysInfo
        {
            Name = @"实体磁盘读取速率(MB/s)",
            objectName = @"PhysicalDisk",
            counterName = @"Disk Reads/sec"
        };

        private static SysInfo PhsDiskMBytesWrite = new SysInfo
        {
            Name = @"实体磁盘写入速率(MB/s)",
            objectName = @"PhysicalDisk",
            counterName = @"Disk Writes/sec"
        };

        public static SysInfo MemoryAvailableMBytes = new SysInfo
        {
            Name = @"内存可用空间(MB)",
            objectName = @"Memory",
            counterName = @"Available MBytes"
        };

        private static SysInfo NetworkRecvBytes = new SysInfo
        {
            Name = @"网络字节接收速率(B/s)",
            objectName = @"Network Interface",
            counterName = @"Bytes Received/sec"
        };

        private static SysInfo NetworkSendBytes = new SysInfo
        {
            Name = @"网络字节发送速率(B/s)",
            objectName = @"Network Interface",
            counterName = @"Bytes Sent/sec"
        };

        private static SysInfo NetworkTotalBytes = new SysInfo
        {
            Name = @"网络字节接收发送总速率(B/s)",
            objectName = @"Network Interface",
            counterName = @"Bytes Total/sec"
        };

        public static List<SysInfo> WindowsServiceMonitors = new List<SysInfo> 
        {
            CpuUsage,
            InterruptTime,
            ProcessorTime,
            DPCTime,
            
            LogicalDiscFreeSpace,
            LogicalDiscFreeSpacePer,
            AvgLogDiskSecTransfer,
            AvgLogDiskSecRead,
            AvgLogDiskSecWrite,
            LogDiskMBytesTransfer,
            LogDiskMBytesRead,
            LogDiskMBytesWrite,
            
            AvgPhsDiskSecTransfer,
            AvgPhsDiskSecRead,
            AvgPhsDiskSecWrite,
            PhsDiskMBytesTransfer,
            PhsDiskMBytesRead,
            PhsDiskMBytesWrite,
            
            MemoryAvailableMBytes,

            NetworkRecvBytes,
            NetworkSendBytes,
            NetworkTotalBytes
        };
    }
}
