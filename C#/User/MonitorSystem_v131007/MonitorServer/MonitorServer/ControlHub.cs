/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/6
 * Description: 控制枢纽模块，负责个处理器模块的同步。
 *              控制枢纽模块本身不执行具体操作，他只是通过向各个模块发送/接收事件
 *              来完成模块间同步。
 *              这种设计主要是为了便于我们有效管理整个系统的运行流程。
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorEvent;
using Newtonsoft.Json;
using System.IO;

namespace MonitorServer
{
    /// <summary>
    /// 主控模块
    /// </summary>
    class ControlHub : Processor
    {
        /// <summary>
        /// 转发客户端事件给对应处理器
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">客户端发送的事件包</param>
        private void ClientEventTransponder(Processor sender, EventArgs e)
        {
            ClientEvent se = (ClientEvent)e;
            EventArgs clientEvent = se.clientEvent; //事件内容

            if (clientEvent is ClientStartNotificationEvent) //客户端启动事件
            {
                ClientStartNotification(clientEvent, se.ip);
            }
            else if (clientEvent is ClientStoppedNotificationEvent)  //客户端关闭事件
            {
                ClientStoppedNotification(clientEvent);
            }
            else if (clientEvent is PerformanceData)
            {
                PerformanceDataHandle(clientEvent);
            }

        }

        /// <summary>
        /// 上传的数据保存
        /// </summary>
        /// <param name="e"></param>
        private void PerformanceDataHandle(EventArgs e)
        {
            PerformanceData pd = (PerformanceData)e;
            if (!Global.realtimeValueTable.ContainsKey(pd.machineName))
                Global.realtimeValueTable.Add(pd.machineName, new Dictionary<string, Dictionary<string, ValueBind>>());
            if (!Global.realtimeValueTable[pd.machineName].ContainsKey(pd.categoryName))
                Global.realtimeValueTable[pd.machineName].Add(pd.categoryName, new Dictionary<string, ValueBind>());
            foreach (string ins in pd.instanceTable.Keys)
            {
                Dictionary<string, ValueBind> insTable = Global.realtimeValueTable[pd.machineName][pd.categoryName];
                if (!insTable.ContainsKey(ins))
                    insTable.Add(ins, new ValueBind(pd.instanceTable[ins], pd.date));
                else
                    insTable[ins] = new ValueBind(pd.instanceTable[ins], pd.date);
            }
            SendEvent(Global.sqlTrans, pd);
        }

        /// <summary>
        /// 客户端启动事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientStartNotification(EventArgs clientEvent, string ip)
        {
            ClientStartNotificationEvent ce = (ClientStartNotificationEvent)clientEvent;
            ClientInfo clientInfo;
            if (!Global.clientsTable.ContainsKey(ce.machineName))   //新的客户端
            {
                Dictionary<string, TimeSpan> periodTable = Global.defaultUploadPeriodTable;
                clientInfo = new ClientInfo(ClientInfo.State.Running, ip, DateTime.Now, periodTable);
                Global.clientsTable.Add(ce.machineName, clientInfo);
            }
            else
            {
                clientInfo = Global.clientsTable[ce.machineName];
                clientInfo.state = ClientInfo.State.Running;
                clientInfo.ip = ip;
                clientInfo.date = DateTime.Now;
            }
            SendEvent(Global.send, new ChangeClientUploadPeriodEvent(ip, clientInfo.uploadPeriodTable));  //发送至对应处理器
        }

        /// <summary>
        /// 客户端离开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientStoppedNotification(EventArgs e)
        {
            ClientStoppedNotificationEvent ce = (ClientStoppedNotificationEvent)e;
            Global.clientsTable[ce.machineName].state = ClientInfo.State.Stopped;
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseServer(Processor sender, EventArgs e)
        {
            TimeSpan waitTime = new TimeSpan(0, 0, 1);
            SaveClientInfo();
            Global.receive.TryToStop(waitTime);
            Global.send.TryToStop(waitTime);
            Global.userConfig.TryToStop(waitTime);

            this.Stop();
        }

        /// <summary>
        /// 保存客户机信息
        /// </summary>
        private void SaveClientInfo()
        {
            //写入客户信息，使用Json
            FileStream fs = new FileStream(Global.clientInfoBootFile, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            string clientText = JsonConvert.SerializeObject(Global.clientsTable);
            sw.Write(clientText);
            sw.Close();
            fs.Close();
        }

        /// <summary>
        /// 载入客户机信息，使用Json
        /// </summary>
        private void LoadClientInfo()
        {
            //写入客户信息，使用Json
            FileStream fs = new FileStream(Global.clientInfoBootFile, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string clientText = sr.ReadToEnd();
            if (clientText != "null")
            {
                Global.clientsTable = JsonConvert.DeserializeObject<Dictionary<string, ClientInfo>>(clientText);
            }
            else
            {
                Global.clientsTable = new Dictionary<string, ClientInfo>();
            }
            sr.Close();
            fs.Close();
        }

        protected override void StartHandle()
        {
            SendEvent(Global.receive, new StartListenEvent());
            SendEvent(Global.userConfig, new LoadUserConfigFormEvent());
        }

        public ControlHub()
        {
            eventHandlePeriod = new TimeSpan(0, 0, 0, 0, 100);
            eventHandleTable.Add(typeof(ClientEvent), this.ClientEventTransponder);
            eventHandleTable.Add(typeof(CloseServerEvent), this.CloseServer);
            LoadClientInfo();   //载入客户端列表
        }
    }
}
