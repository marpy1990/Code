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
    class MonitorServer
    {
        private ServerConnection sc;
        public Database db;
    }

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

            memoryAvailableMBytes
        };
    }

    class DatabaseMonitorInfo
    {
        public string name;
    }

    class DatabaseCategoryEnum
    {
        private static DatabaseMonitorInfo autoClose = new DatabaseMonitorInfo
        {
            name = @"Auto Close Flag"
        };

        private static DatabaseMonitorInfo autoCreateStatisticsEnabled = new DatabaseMonitorInfo
        {
            name = @"Auto Create Statistics Flag"
        };

        private static DatabaseMonitorInfo autoShrink = new DatabaseMonitorInfo
        {
            name = @"Auto Shrink Flag"
        };

        private static DatabaseMonitorInfo databaseOwnershipChaining = new DatabaseMonitorInfo
        {
            name = @"DB Chaining Flag"
        };

        private static DatabaseMonitorInfo autoUpdateStatisticsAsync = new DatabaseMonitorInfo
        {
            name = @"Auto Update Flag"
        };

        public static List<DatabaseMonitorInfo> databaseMonitoers = new List<DatabaseMonitorInfo>
        {
            autoClose,
            autoCreateStatisticsEnabled,
            autoShrink,
            databaseOwnershipChaining,
            autoUpdateStatisticsAsync
        };
    }

    public class Machine
    {
        protected string machineName;

        public Machine(string name)
        {
            machineName = name;
        }

        public string MachineName()
        {
            return machineName;
        }

        public virtual List<Category> Categorys()
        {
            List<Category> lst = new List<Category>();
            
            //load SystemCategory
            foreach (SystemMonitorInfo sysInfo in SystemCategoryEnum.windowsServiceMonitors)
            {
                SystemCategory sysObj = new SystemCategory(this, sysInfo.name, sysInfo.categoryName, sysInfo.counterName);
                lst.Add(sysObj);
            }
            
            //load DatabaseCategory
            foreach (DatabaseMonitorInfo dbInfo in DatabaseCategoryEnum.databaseMonitoers)
            {
                DatabaseCategory dbObj = new DatabaseCategory(this, dbInfo.name);
                lst.Add(dbObj);
            }
            return lst;
        }
        
    }

    public class Category
    {
        protected Machine machine;

        protected string categoryName;

        public Category(Machine machine, string categoryName)
        {
            this.machine=machine;
            this.categoryName=categoryName;
        }

        public string MachineName()
        {
            return machine.MachineName();
        }

        public string CategoryName()
        {
            return categoryName;
        }

        public virtual List<Instance> Instances()
        {
            return null;
        }
    }

    class SystemCategory : Category
    {
        protected string objectName;
        protected string counterName;

        public SystemCategory(Machine machine, string categoryName, string objectName, string counterName)
            : base(machine, categoryName)
        {
            this.objectName = objectName;
            this.counterName = counterName;
        }

        public string ObjectName()
        {
            return objectName;
        }

        public string CounterName()
        {
            return counterName;
        }

        public override List<Instance> Instances()
        {
            List<Instance> lst = new List<Instance>();
            PerformanceCounterCategory pcc = new PerformanceCounterCategory(objectName, MachineName());
            foreach (string instanceName in pcc.GetInstanceNames().DefaultIfEmpty(""))
            {
                SystemInstance instacnce = new SystemInstance(this, instanceName);
                lst.Add(instacnce);
            }
            return lst;
        }
    }

    class DatabaseCategory : Category
    {
        protected Server server;

        public DatabaseCategory(Machine machine, string categoryName)
            : base(machine, categoryName)
        {
            server = new Server(MachineName());
        }

        public override List<Instance> Instances()
        {
            List<Instance> lst = new List<Instance>();
            foreach (Database db in server.Databases)
            {
                DatabaseInstance dbIns = new DatabaseInstance(this, db);
                lst.Add(dbIns);
            } 
            return lst;
        }
    }

    public class Instance
    {
        protected Category category;

        protected string instanceName;

        public Instance(Category category, string instanceName)
        {
            this.category = category;
            this.instanceName = instanceName;
        }

        public string MachineName()
        {
            return category.MachineName();
        }

        public string CategoryName()
        {
            return category.CategoryName();
        }

        public string InstanceName()
        {
            return instanceName;
        }

        public virtual Monitor Monitor()
        {
            return null;
        }
    }

    class SystemInstance : Instance
    {
        public SystemInstance(SystemCategory category, string instanceName)
            : base(category, instanceName)
        {
        }

        public string ObjectName()
        {
            return ((SystemCategory)category).ObjectName();
        }

        public string CounterName()
        {
            return ((SystemCategory)category).CounterName();
        }

        public override Monitor Monitor()
        {
            SystemMonitor sysMonitor = new SystemMonitor(this);
            return sysMonitor;
        }
    }

    class DatabaseInstance : Instance
    {
        protected Database database;

        public DatabaseInstance(DatabaseCategory category, Database db)
            : base(category, db.Name)
        {
            database = db;
        }

        public Database Database()
        {
            return database;
        }

        public override Monitor Monitor()
        {
            Monitor monitor;
            switch (CategoryName())
            {
                case @"Auto Close Flag":
                    monitor = new DbAutoCloseMonitor(this);
                    break;
                case @"Auto Create Statistics Flag":
                    monitor = new DbAutoCreateStatic(this);
                    break;
                case @"Auto Shrink Flag":
                    monitor = new DbAutoShrink(this);
                    break;
                case @"DB Chaining Flag":
                    monitor = new DbChainingFlag(this);
                    break;
                case @"Auto Update Flag":
                    monitor = new DbAutoUpdate(this);
                    break;
                default:
                    monitor = null;
                    break;
            }
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

        public virtual string GetStatus()
        {
            return null;
        }
    }

    class SystemMonitor : Monitor
    {
        protected PerformanceCounter pc;

        public SystemMonitor(SystemInstance sysIns)
            : base(sysIns.MachineName(), sysIns.CategoryName(), sysIns.InstanceName())
        {
            pc = new PerformanceCounter(sysIns.ObjectName(), sysIns.CounterName(), sysIns.InstanceName(), sysIns.MachineName());
            pc.NextValue();
            //Thread.Sleep(1000);
        }

        public override double GetValue()
        {
            return pc.NextValue();
        }
    }

    class DatabaseMonitor : Monitor
    {
        protected Database database;

        public DatabaseMonitor(DatabaseInstance dbIns)
            : base(dbIns.MachineName(), dbIns.CategoryName(), dbIns.InstanceName())
        {
            database = dbIns.Database();
        }
    }

    class DbAutoCloseMonitor : DatabaseMonitor
    {
        public DbAutoCloseMonitor(DatabaseInstance dbIns)
            : base(dbIns)
        {
        }

        public override double GetValue()
        {
            if(database.AutoClose)
                return 1;
            else
                return 0;
        }
    }

    class DbAutoCreateStatic : DatabaseMonitor
    {
        public DbAutoCreateStatic(DatabaseInstance dbIns)
            : base(dbIns)
        {
        }

        public override double GetValue()
        {
            if (database.AutoCreateStatisticsEnabled)
                return 1;
            else
                return 0;
        }
    }

    class DbAutoShrink : DatabaseMonitor
    {
        public DbAutoShrink(DatabaseInstance dbIns)
            : base(dbIns)
        {
        }

        public override double GetValue()
        {
            if (database.AutoShrink)
                return 1;
            else
                return 0;
        }
    }

    class DbChainingFlag : DatabaseMonitor
    {
        public DbChainingFlag(DatabaseInstance dbIns)
            : base(dbIns)
        {
        }

        public override double GetValue()
        {
            if (database.DatabaseOwnershipChaining)
                return 1;
            else
                return 0;
        }
    }

    class DbAutoUpdate : DatabaseMonitor
    {
        public DbAutoUpdate(DatabaseInstance dbIns)
            : base(dbIns)
        {
        }

        public override double GetValue()
        {
            if (database.AutoUpdateStatisticsEnabled)
                return 1;
            else
                return 0;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Machine m = new Machine(Environment.MachineName);
            foreach (Category ctg in m.Categorys())
            {
                foreach (Instance ins in ctg.Instances())
                {
                    Monitor monitor=ins.Monitor();
                    Console.WriteLine("{0} , {1} , {2}", ins.CategoryName(), ins.InstanceName(),monitor.GetValue());
                }
            }
        }
    }
}
