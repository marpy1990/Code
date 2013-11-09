using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;
using System.DirectoryServices;
using Microsoft.Web.Administration;
using System.Net;

using Probe;

namespace IIProbe
{
    public class IISProbe:Probe.Probe
    {
        ICollection<DetectedData> Probe.Probe.GetValues()
        {
            List<DetectedData> lst = new List<DetectedData>();
            //IIS服务器监控类使用示例
            IISmonitor IM = new IISmonitor(Environment.MachineName);


            //获取应用程序集区可用性列表
            List<Item> It = IM.listAppPool();
            foreach (Item i in It)
            {
                Probe.DetectedData data = new DetectedData();
                data.categoryName = i.monitorName;
                data.instanceName = i.caseName;
                data.value = i.State;
                
                lst.Add(data);
            }

            It = IM.listFTP();
            foreach (Item i in It)
            {
                Probe.DetectedData data = new DetectedData();
                data.categoryName = i.monitorName;
                data.instanceName = i.caseName;
                data.value = i.State;

                lst.Add(data);
            }

            It = IM.listServices();
            foreach (Item i in It)
            {
                Probe.DetectedData data = new DetectedData();
                data.categoryName = i.monitorName;
                data.instanceName = i.caseName;
                data.value = i.State;

                lst.Add(data);
            }

            It = IM.listWeb();
            foreach (Item i in It)
            {
                Probe.DetectedData data = new DetectedData();
                data.categoryName = i.monitorName;
                data.instanceName = i.caseName;
                data.value = i.State;

                lst.Add(data);
            }

            return lst;
        }
    }

    //监控对象
    struct Item
    {
        //机器名称
        public string machineName;
        //监控项目名称
        public string monitorName;
        //实例名称
        public string caseName;
        //监控状态
        public string State;
    }

    //IIS服务器监控类
    class IISmonitor
    {
        public string machineName;

        private static List<DateTime> AlreadyHaveLogs = new List<DateTime>();

        //IIS组件监控
        //public List<Item> listAppPoolList();  //应用程序集区可用性
        //public List<Item> listFTP();  //FTP站点可用性


        //private bool checkFTP(string url, string port);

        //构造函数
        public IISmonitor(string machineName)
        {
            this.machineName = machineName;
        }


        //IIS组件监控
        //应用程序集区可用性
        public List<Item> listAppPool()
        {
            List<Item> AppPoolList = new List<Item>();
            
            ServerManager iisManager = ServerManager.OpenRemote(machineName);
            foreach (ApplicationPool appPool in iisManager.ApplicationPools)
            {
                Item item;
                item.machineName = machineName;
                item.monitorName = "应用程序集区可用性";
                item.caseName = appPool.Name;
                item.State = appPool.State.ToString();
                AppPoolList.Add(item);
            }

            return AppPoolList;
        }

        //FTP站点可用性
        public List<Item> listFTP()
        {
            List<Item> FTPlist = new List<Item>();
            ServerManager manager = ServerManager.OpenRemote(machineName);
            foreach (Site site in manager.Sites)
            {
                bool isFTP = false;
                string[] url = {"", ""};
                foreach (Microsoft.Web.Administration.Binding binding in site.Bindings)
                {
                    if (binding.Protocol == "ftp")
                    {
                        isFTP = true;
                        string BI = binding.BindingInformation;
                        url = BI.Split(':');
                    }
                }
                if (isFTP)
                {
                    Item item;
                    item.machineName = machineName;
                    item.monitorName = "FTP站点可用性";
                    item.caseName = site.Name;

                    if (checkFTP(url[0], url[1]))
                        item.State = "available";
                    else
                        item.State = "disavailable";

                    FTPlist.Add(item);
                }
            }

            return FTPlist;
        }

        //网站可用性
        public List<Item> listWeb()
        {
            List<Item> WebList = new List<Item>();

            ServerManager manager = ServerManager.OpenRemote(machineName);
            foreach (Site site in manager.Sites)
            {
                bool isFTP = false;

                foreach (Microsoft.Web.Administration.Binding binding in site.Bindings)
                {
                    if (binding.Protocol == "ftp")
                        isFTP = true;
                }

                if (!isFTP)
                {
                    Item item;
                    item.machineName = machineName;
                    item.monitorName = "网站可用性";
                    item.caseName = site.Name;
                    item.State = site.State.ToString();
                    WebList.Add(item);
                }
            }

            return WebList;
        }


        //WindowsNT服务监控
        //表格里所列出的所有服务
        public List<Item> listServices()
        {
            List<Item> ServiceList = new List<Item>();

            //表格里要求的7项服务名称
            string[] serviceCollection = { "MSFTPSVC", "FTPSVC", "IISADMIN", "WMSVC", "WAS", "W3SVC", "SMTPSVC" };
            string[] serviceName = {"MSFTP服务可用性", "FTP服务可用性", "IISAdmin服务可用性", "Web Management Service 可用性", "Windows 进程启用服务可用性",
                                   "World Wide Web Publishing 服务可用性", "SMTP 服务可用性"};
            for (int i = 0; i < serviceCollection.Length; i++)
            {
                Item item;
                item.machineName = machineName;
                item.monitorName = "WindowsNT服务可用性";
                item.caseName = serviceName[i];
                try
                {
                    ServiceController sc = new ServiceController(serviceCollection[i], machineName);
                    item.State = sc.Status.ToString();
                }
                catch
                {
                    item.State = "未找到该服务";
                }
                ServiceList.Add(item);
            }

            return ServiceList;
        }


        //事件记录监视
        //运行监测时不作任何输出, 若有异常事件写入则会弹出提示
        public void startLogMonitor()
        {
            EventLog ev = new EventLog();
            ev.Log = "Application";
            EventLogEntryCollection elec = ev.Entries;
            foreach (EventLogEntry test in elec)
            {
                var time = test.TimeGenerated;
                if (!AlreadyHaveLogs.Contains(time))
                {
                    AlreadyHaveLogs.Add(time);
                }
            }
            EventLog ev2 = new EventLog();
            ev2.Log = "System";
            EventLogEntryCollection elec2 = ev2.Entries;
            foreach (EventLogEntry test2 in elec2)
            {
                var time2 = test2.TimeGenerated;
                if (!AlreadyHaveLogs.Contains(time2))
                {
                    AlreadyHaveLogs.Add(time2);
                }
            }
            Thread thread = new Thread(ShowLog);
            thread.Start();
        }




        private bool checkFTP(string url, string port)
        {
            if (url == "*")
                url = "localhost";
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create("ftp://" + url + ":" + port);
            request.Credentials = new NetworkCredential("", "");
            try
            {
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                //Console.WriteLine("available");
                return true;
            }
            catch
            {
                //Console.WriteLine("disavailable");
                return false;
            }
        }

        private void ShowLog()
        {
            EventLog ev = new EventLog();
            ev.MachineName = machineName;
            ev.Log = "Application";
            EventLogEntryCollection elec = ev.Entries;
            foreach (EventLogEntry test in elec)
            {
                var time = test.TimeGenerated;
                if (!AlreadyHaveLogs.Contains(time))
                {
                    AlreadyHaveLogs.Add(time);
                    //是警告或错误
                    if (test.EntryType == EventLogEntryType.Error)
                    {
                    }
                }
            }
            EventLog ev2 = new EventLog();
            ev2.Log = "System";
            EventLogEntryCollection elec2 = ev2.Entries;
            foreach (EventLogEntry test2 in elec2)
            {
                var time2 = test2.TimeGenerated;
                if (!AlreadyHaveLogs.Contains(time2))
                {
                    AlreadyHaveLogs.Add(time2);
                    //是警告或错误
                    if (test2.EntryType == EventLogEntryType.Error)
                    {
                        //todo 显示log
                        if (test2.EventID == 5144 || test2.EventID == 5002 || test2.EventID == 5059 || test2.EventID == 5021 || test2.EventID == 5057
                            || test2.EventID == 5190 || test2.EventID == 5150 || test2.EventID == 1131
                            || test2.EventID == 5056 || test2.EventID == 5161 || test2.EventID == 1029 || test2.EventID == 1004 || test2.EventID == 1172 || test2.EventID == 1040
                            || test2.EventID == 1003 || test2.EventID == 1129 || test2.EventID == 1130 || test2.EventID == 1007 || test2.EventID == 5055
                            || test2.EventID == 5102 || test2.EventID == 5143)
                        {


                        }
                    }
                    if (test2.EntryType == EventLogEntryType.Warning)
                    {
                    }
                    if (test2.EventID == 5117 || test2.EventID == 5077)
                    {
                    }
                }
            }
            //暂停5秒
            Thread.Sleep(5000);
            //5秒后继续
            ShowLog();
        }


    }

}
