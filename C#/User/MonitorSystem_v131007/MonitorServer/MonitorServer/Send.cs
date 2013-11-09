using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonitorEvent;
using System.Net;
using System.Net.Sockets;

namespace MonitorServer
{
    /// <summary>
    /// 发送处理器
    /// </summary>
    class Send:Processor
    {
        /// <summary>
        /// 改变客户端上传周期的主控方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ChangeClientUploadPeriod(Processor sender, EventArgs e)
        {
            ChangeClientUploadPeriodEvent ce = (ChangeClientUploadPeriodEvent)e;
            string sendText = EventArgsConvert.SerializeObject(new ChangeUploadPeriodEvent(ce.periodTable));
            ConnectClientToSendText(ce.ip, sendText);
        }

        /// <summary>
        /// 连接客户端并上传Text，之后关闭与服务器的连接
        /// </summary>
        /// <param name="Text">上传内容</param>
        void ConnectClientToSendText(string ip, string sendText)
        {
            try
            {
                IPAddress serverIP = IPAddress.Parse(ip);
                IPEndPoint iep = new IPEndPoint(serverIP, Global.sendPort);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                socket.Connect(iep);
                byte[] buffersend = System.Text.Encoding.Default.GetBytes(sendText);
                socket.Send(buffersend);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch
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

        public Send()
        {
            eventHandlePeriod = new TimeSpan(0, 0, 0, 0, 100);
            eventHandleTable.Add(typeof(ChangeClientUploadPeriodEvent), this.ChangeClientUploadPeriod);
        }
    }
}
