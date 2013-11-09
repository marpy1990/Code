/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/5
 * Description: 接收模块，接收客户端上传的事件
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using MonitorEvent;

namespace MonitorServer
{
    /// <summary>
    /// 接收处理器，负责接收客户端的数据/事件
    /// </summary>
    class Receive : Processor
    {
        private TcpListener tcplisten = new TcpListener(new IPEndPoint(IPAddress.Any, Global.receivePort));

        /// <summary>
        /// 长时间监听服务器，接收从服务器传来的事件，并发送给主控枢纽解析
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Listen(Processor sender, EventArgs e)
        {
            tcplisten.Start();
            byte[] buffer = new byte[10000];

            try
            {
                Socket s = tcplisten.AcceptSocket();
                IPEndPoint clientipe = (IPEndPoint)s.RemoteEndPoint;
                string ip = clientipe.Address.ToString();
                s.Receive(buffer);
                string receiveText = System.Text.Encoding.Default.GetString(buffer);
                ClientEvent clientEvent = new ClientEvent(ip, DecodeText(receiveText)); //解码
                this.SendEvent(Global.hub, clientEvent);   //向主控枢纽发送接收到的消息
                s.Close();
            }
            catch
            {
                ListenAcceptInterrupt();
            }
            finally
            {
                tcplisten.Stop();
            }
            SendEvent(this, new StartListenEvent());
        }

        /// <summary>
        /// 监听被打断，可能是处理器关闭操作，可选
        /// </summary>
        void ListenAcceptInterrupt()
        {
            //可选
        }

        /// <summary>
        /// 将服务器消息转化成本地对应的事件
        /// </summary>
        /// <param name="text">消息文本</param>
        /// <returns>事件数据</returns>
        private EventArgs DecodeText(string text)
        {
            return EventArgsConvert.DeserializeObject(text);
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopListen(Processor sender, EventArgs e)
        {
            tcplisten.Stop();
        }

        /// <summary>
        /// 在停止前先停止监听
        /// </summary>
        protected override void PrepareToStop()
        {
            StopListen(null, null);
        }

        public Receive()
        {
            eventHandlePeriod = new TimeSpan(0, 0, 0, 0, 100);
            eventHandleTable.Add(typeof(StartListenEvent), this.Listen);     //绑定监听方法
            eventHandleTable.Add(typeof(StopListenEvent), this.StopListen);  //绑定停止监听方法
        }
    }
}
