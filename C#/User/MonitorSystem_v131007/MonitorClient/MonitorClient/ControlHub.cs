/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/4
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

namespace MonitorClient
{
    /// <summary>
    /// 控制枢纽模块
    /// </summary>
    class ControlHub : Processor
    {
        /// <summary>
        /// 收到探针加载完毕的回复
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件内容</param>
        private void ReveiveProbleBuildedConfirmEventHandle(Processor sender, EventArgs e)
        {
            SendEvent(sender, new PerformanceCollectEvent(DateTime.Now));   //通知采集模块开始采集
        }

        /// <summary>
        /// 转发服务器事件给对应处理器
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">服务器发送的事件包</param>
        private void ServerEventTransponder(Processor sender, EventArgs e)
        {
            ServerEvent se = (ServerEvent)e;
            EventArgs serverEvent = se.serverEvent; //事件内容
            if (serverEvent is ChangeUploadPeriodEvent) //目前服务器只发送上传周期表
            {
                SendEvent(Global.upload, serverEvent);  //发送至对应处理器
            }
        }

        /// <summary>
        /// 关闭客户端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseClient(Processor sender, EventArgs e)
        {
            TimeSpan waitTime=new TimeSpan(0,0,1);
            
            Global.performanceCollector.TryToStop(waitTime);
            Global.upload.TryToStop(waitTime);
            Global.download.TryToStop(waitTime);
            Global.userConfig.TryToStop(waitTime);

            this.Stop();
        }

        /// <summary>
        /// 启动同时激活各个模块
        /// </summary>
        protected override void StartHandle()
        {
            SendEvent(Global.upload, new ClientStartNotificationEvent());
            SendEvent(Global.download, new StartListenEvent());
            SendEvent(Global.performanceCollector, new ProbeBuildingEvent());
            SendEvent(Global.userConfig, new LoadUserConfigFormEvent());
        }

        public ControlHub()
        {
            eventHandlePeriod = new TimeSpan(0, 0, 0, 0, 100);
            eventHandleTable.Add(typeof(ProbleBuildedConfirmEvent), this.ReveiveProbleBuildedConfirmEventHandle);
            eventHandleTable.Add(typeof(ServerEvent), this.ServerEventTransponder);
            eventHandleTable.Add(typeof(CloseClientEvent), this.CloseClient);
        }
    }
}
