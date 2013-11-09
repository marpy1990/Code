using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Probe;
using System.Management;
using System.ServiceProcess;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.IO;

namespace SQLProbe
{

    public class SQLProbe:Probe.Probe
    {
        ICollection<Probe.DetectedData> Probe.Probe.GetValues()
        {
            List<Probe.DetectedData> lst = new List<Probe.DetectedData>();
            Probe.DetectedData svdata;

            try
            {
                sv = new Server(Environment.MachineName);
                foreach (Database db in sv.Databases)
                {
                    Probe.DetectedData data = new Probe.DetectedData();
                    data.categoryName = @"Auto Close Flag";
                    data.instanceName = db.Name;
                    data.value = db.AutoClose;
                    lst.Add(data);

                    data = new Probe.DetectedData();
                    data.categoryName = @"Auto Create Statistics Flag";
                    data.instanceName = db.Name;
                    data.value = db.AutoCreateStatisticsEnabled;
                    lst.Add(data);

                    data = new Probe.DetectedData();
                    data.categoryName = @"Auto Shrink Flag";
                    data.instanceName = db.Name;
                    data.value = db.AutoShrink;
                    lst.Add(data);

                    data = new Probe.DetectedData();
                    data.categoryName = @"DB Chaining Flag";
                    data.instanceName = db.Name;
                    data.value = db.DatabaseOwnershipChaining;
                    lst.Add(data);

                    data = new Probe.DetectedData();
                    data.categoryName = @"Auto Update Flag";
                    data.instanceName = db.Name;
                    data.value = db.IsUpdateable;
                    lst.Add(data);

                    data = new Probe.DetectedData();
                    data.categoryName = @"DB Space Free%";
                    data.instanceName = db.Name;
                    data.value = db.SpaceAvailable / (db.SpaceAvailable + db.DataSpaceUsage) * 100;
                    lst.Add(data);

                    double spaceAvi = (double)data.value;
                    data = new Probe.DetectedData();
                    data.categoryName = @"DB UsedSpace% Change";
                    data.instanceName = db.Name;
                    if (lastDBUsage.ContainsKey(db.Name))
                    {
                        data.value = Math.Abs(lastDBUsage[db.Name] - spaceAvi) / (DateTime.Now - lastTime).TotalSeconds;
                        lastDBUsage[db.Name] = spaceAvi;
                        lastTime = DateTime.Now;
                    }
                    else
                    {
                        data.value = (double)0;
                        lastDBUsage.Add(db.Name, spaceAvi);
                    }
                    lst.Add(data);

                    data = new Probe.DetectedData();
                    data.categoryName = @"Transaction Log Space Free%";
                    data.instanceName = db.Name;
                    double value = 0, totalValue = 0;
                    foreach (LogFile logFile in db.LogFiles)
                    {
                        totalValue += logFile.Size;
                        value += logFile.UsedSpace;
                    }
                    data.value = (totalValue - value) / totalValue * 100;

                    lst.Add(data);
                }

                svdata = new Probe.DetectedData();
                svdata.categoryName = @"SQL Version";
                svdata.instanceName = "";
                svdata.value = sv.VersionString;
                lst.Add(svdata);
            }
            catch
            {
            }

            try
            {
                svdata = new Probe.DetectedData();
                svdata.categoryName = @"SQL Server";
                svdata.instanceName = "";
                ServiceController scServices = new ServiceController("MSSQLSERVER", Environment.MachineName);
                svdata.value = scServices.Status.ToString();
                lst.Add(svdata);

                svdata = new Probe.DetectedData();
                svdata.categoryName = @"SQL 2008 Agent";
                svdata.instanceName = "";
                scServices = new ServiceController("SQLSERVERAGENT", Environment.MachineName);
                svdata.value = scServices.Status.ToString();
                lst.Add(svdata);

                svdata = new Probe.DetectedData();
                svdata.categoryName = @"SQL Server Analysis Services";
                svdata.instanceName = "";
                scServices = new ServiceController("MSSQLServerOLAPService", Environment.MachineName);
                svdata.value = scServices.Status.ToString();
                lst.Add(svdata);

                svdata = new Probe.DetectedData();
                svdata.categoryName = @"SQL 2005 Integration Services";
                svdata.instanceName = "";
                scServices = new ServiceController("MsDtsServer100", Environment.MachineName);
                svdata.value = scServices.Status.ToString();
                lst.Add(svdata);

                svdata = new Probe.DetectedData();
                svdata.categoryName = @"SQL Server Reporting Services";
                svdata.instanceName = "";
                scServices = new ServiceController("MsDtsServer100", Environment.MachineName);
                svdata.value = scServices.Status.ToString();
                lst.Add(svdata);

                svdata = new Probe.DetectedData();
                svdata.categoryName = @"SQL Server Full Text Search Service";
                svdata.instanceName = "";
                scServices = new ServiceController("MSSQLFDLauncher", Environment.MachineName);
                svdata.value = scServices.Status.ToString();
                lst.Add(svdata);
            }
            catch
            {
            }
            return lst;
        }

        private Server sv;
        Dictionary<string, double> lastDBUsage = new Dictionary<string, double>();
        DateTime lastTime = DateTime.Now;

        public SQLProbe()
        {
            string machineName = Environment.MachineName;
            sv = new Server(machineName);
            
        }

    }
}
