/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/4
 * Description: 数据上传模块，向服务器传输数据/事件。
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using MonitorEvent;
using System.IO;

namespace MonitorClient
{
    /// <summary>
    /// 上传处理器，负责向服务器上传数据/事件
    /// </summary>
    class Upload : Processor
    {
        #region 启动项目模块
        /// <summary>
        /// 通知服务器本客户端已经启动，主控方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientStartNotify(Processor sender, EventArgs e)
        {
            string sendText = ClientStartNotifyText();
            try
            {
                ConnectServerToSendText(sendText);
            }
            catch
            {
                SendEvent(Global.upload, new ClientStartNotificationEvent());
            }
        }

        private string ClientStartNotifyText()
        {
            /********************************************************************
             * 告诉服务器本客户端已经启动，内容就是这个函数的返回值。
             * 服务器收到这个消息后，必须给本客户端发送一个<监视项目,上传周期>表。
             * 在Download模块会接收这张表。
             ********************************************************************/

            return EventArgsConvert.SerializeObject(new ClientStartNotificationEvent());
        }
        #endregion

        #region 性能上传模块
        /// <summary>
        /// 连接服务器并上传Text，之后关闭与服务器的连接
        /// </summary>
        /// <param name="Text">上传内容</param>
        void ConnectServerToSendText(string sendText)
        {
            try
            {
                IPAddress serverIP = IPAddress.Parse(Global.ip);
                IPEndPoint iep = new IPEndPoint(serverIP, Global.uploadPort);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //EndPoint ep = (EndPoint)iep;
                socket.Connect(iep);
                byte[] buffersend = System.Text.Encoding.Default.GetBytes(sendText);
                socket.Send(buffersend);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            finally
            {
                ConnectServerToSendTextError();
            }
        }

        /// <summary>
        /// 连接服务器异常处理，可选
        /// </summary>
        void ConnectServerToSendTextError()
        {
            //可选
        }

        /// <summary>
        /// 数据上传的主控方法
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">上传数据</param>
        private void PerformanceDataUpload(Processor sender, EventArgs e)
        {
            try
            {
                PerformanceDataUploadEvent pe = (PerformanceDataUploadEvent)e;
                InstanceValues instances = Global.categoryValues[pe.categoryName];
                Dictionary<string, object> instanceTable = new Dictionary<string, object>();
                foreach (string instanceName in instances.instanceNames)
                {
                    instanceTable.Add(instanceName, instances.GetValue(instanceName));
                }
                PerformanceData data = new PerformanceData(Global.machineName, pe.categoryName, DateTime.Now, instanceTable);
                string sendText = TransDataListToText(data);
                ConnectServerToSendText(sendText);
            }
            catch
            {
                PerformanceDataUploadError();
            }
        }

        private string TransDataListToText(PerformanceData data)
        {
            /********************************************************************
             * 将一串UpLoadData转化为字符串，内容就是这个函数的返回值。
             * 服务器收到字符串后，会将其反转为对应项目信息值。
             * 注意UpLoadData字段有个DateTime，不知道Java有没有。
             *********************************************************************/

            return EventArgsConvert.SerializeObject(data);
        }

        /// <summary>
        /// 上传失败处理方法，可选
        /// </summary>
        private void PerformanceDataUploadError()
        {
            //可选
        }

        /// <summary>
        /// 预处理，这是为了实现上传的消息循环
        /// </summary>
        /// <param name="e">事件信息</param>
        /// <returns>是否跳过处理方法</returns>
        protected override bool PretreatToSkip(EventArgs e)
        {
            if (e is PerformanceDataUploadEvent)
            {
                DateTime Now = DateTime.Now;
                PerformanceDataUploadEvent pe = (PerformanceDataUploadEvent)e;
                if (!Global.uploadPeriodTable.ContainsKey(pe.categoryName)) return true;
                try
                {
                    if (pe.sendDate.Add(Global.uploadPeriodTable[pe.categoryName]) <= Now)
                    {
                        pe.sendDate = Now;  //到达上传时间，预约下一次采样
                        SendEvent(this, pe);
                        return false;
                    }
                    else
                    {
                        SendEvent(this, pe);
                        return true;        //未达到上传时间，跳过处理线程
                    }
                }
                catch
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 更改上传周期
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">内含上传周期表</param>
        private void ChangeUploadPeriod(Processor sender, EventArgs e)
        {
            try
            {
                ChangeUploadPeriodEvent ce = (ChangeUploadPeriodEvent)e;
                Dictionary<string, TimeSpan> upTable = ce.periodTable;
                foreach (string categoryName in upTable.Keys)
                {
                    if (Global.uploadPeriodTable.ContainsKey(categoryName))
                    {
                        Global.uploadPeriodTable[categoryName] = upTable[categoryName];
                    }
                    else
                    {
                        SendEvent(this, new PerformanceDataUploadEvent(categoryName, DateTime.Now));    //上传对应内容
                        Global.uploadPeriodTable.Add(categoryName, upTable[categoryName]);
                    }
                }
            }
            catch
            {
                ChangeUploadPeriodError();
            }
        }

        /// <summary>
        /// 更改失败，可选
        /// </summary>
        private void ChangeUploadPeriodError()
        {
            //可选
        }
        #endregion

        #region 初始化模块
        /// <summary>
        /// 关闭前通知服务器
        /// </summary>
        protected override void PrepareToStop()
        {
            string sendText = ClientStopNotifyText();
            try
            {
                ConnectServerToSendText(sendText);
            }
            catch
            {
                SendEvent(Global.upload, new ClientStoppedNotificationEvent());
            }
        }

        private string ClientStopNotifyText()
        {
            return EventArgsConvert.SerializeObject(new ClientStoppedNotificationEvent());
        }

        /// <summary>
        /// 启动同时获取主机ip地址
        /// </summary>
        protected override void StartHandle()
        {
            try
            {
                FileStream fs = new FileStream(Global.serverIPFile, FileMode.Open);
                StreamReader m_streamReader = new StreamReader(fs);
                Global.ip = m_streamReader.ReadToEnd();
                m_streamReader.Close();
                fs.Close();
            }
            catch (FileNotFoundException)
            {
                LoadServerIPFileError();
            }
        }

        /// <summary>
        /// 读取ip文件失败处理，可选
        /// </summary>
        private void LoadServerIPFileError()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 初始化并绑定事件处理方法
        /// </summary>
        public Upload()
        {
            eventHandlePeriod = new TimeSpan(0, 0, 0, 0, 100); //事件驱动事件为100ms
            eventHandleTable.Add(typeof(ClientStartNotificationEvent), this.ClientStartNotify);
            eventHandleTable.Add(typeof(PerformanceDataUploadEvent), this.PerformanceDataUpload);
            eventHandleTable.Add(typeof(ChangeUploadPeriodEvent), this.ChangeUploadPeriod);
        }
        #endregion

    }
}
