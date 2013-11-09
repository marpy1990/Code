/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/4
 * Description: 数据下载模块，从服务器下载事件
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using MonitorEvent;

namespace MonitorClient
{
    /// <summary>
    /// 下载处理器，负责下载服务器数据/事件
    /// </summary>
    class Download : Processor
    {
        private TcpListener tcplisten = new TcpListener(new IPEndPoint(IPAddress.Any, Global.downloadPort));

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
                s.Receive(buffer);
                string receiveText = System.Text.Encoding.Default.GetString(buffer);
                ServerEvent serverEvent = new ServerEvent(DecodeText(receiveText)); //解码
                this.SendEvent(Global.hub, serverEvent);   //向主控枢纽发送接收到的消息
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
            /*********************************************************************
             * 将服务器发送的text转化为本地对应事件，写入返回值。
             * 不过本次程序接收的事件只有 ChangeUploadPeriodEvent这一种，所以只要
             * 无脑将text转化为ChangeUploadPeriodEvent即可
             *********************************************************************/
            //转化text 为 Dictionary<string,TimeSpan> 的键值对dict
            //return new ChangeUploadPeriodEvent(dict);

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

        public Download()
        {
            eventHandlePeriod = new TimeSpan(0, 0, 0, 0, 100);
            eventHandleTable.Add(typeof(StartListenEvent), this.Listen);     //绑定监听方法
            eventHandleTable.Add(typeof(StopListenEvent), this.StopListen);  //绑定停止监听方法
        }
    }
}
