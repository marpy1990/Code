/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/1
 * Description: 包含所有可能的事件数据类型，处理器由事件驱动，通过事件进行通信。
 *              每一种事件数据类型对应一个处理方法，处理方法保存在在各个Processor
 *              的事件映射表中。
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MonitorEvent
{
    #region 主控模块
    /// <summary>
    /// 通知服务器本客户端已经启动
    /// </summary>
    class ClientStartNotificationEvent : EventArgs
    {
        /// <summary>
        /// 客户端机器名
        /// </summary>
        public string machineName;
        public ClientStartNotificationEvent()
        {
            this.machineName = Environment.MachineName;
        }
    }

    /// <summary>
    /// 通知服务器客户端已经关闭
    /// </summary>
    class ClientStoppedNotificationEvent : EventArgs
    {
        /// <summary>
        /// 客户端机器名
        /// </summary>
        public string machineName;
        public ClientStoppedNotificationEvent()
        {
            this.machineName = Environment.MachineName;
        }
    }

    /// <summary>
    /// 关闭客户端
    /// </summary>
    class CloseServerEvent : EventArgs
    {
    }
    #endregion

    #region 接收模块
    /// <summary>
    /// 客户端发送给服务器的消息，消息保存在clientEvent
    /// </summary>
    class ClientEvent : EventArgs
    {
        /// <summary>
        /// 客户端ip
        /// </summary>
        public string ip;
        public EventArgs clientEvent;
        public ClientEvent(string ip, EventArgs clientEvent)
        {
            this.ip = ip;
            this.clientEvent = clientEvent;
        }
    }

    /// <summary>
    /// 启动服务器监听
    /// </summary>
    class StartListenEvent : EventArgs
    {
    }

    /// <summary>
    /// 停止监听
    /// </summary>
    class StopListenEvent : EventArgs
    {
    }
    #endregion

    #region 发送模块
    /// <summary>
    /// 更改客户端上传项目时间周期
    /// </summary>
    class ChangeClientUploadPeriodEvent : EventArgs
    {
        /// <summary>
        /// <上传项目名，上传周期>键值对
        /// </summary>
        public Dictionary<string, TimeSpan> periodTable;

        /// <summary>
        /// 客户端ip
        /// </summary>
        public string ip;

        public ChangeClientUploadPeriodEvent(string ip, Dictionary<string, TimeSpan> periodTable)
        {
            this.ip = ip;
            this.periodTable = periodTable;
        }
    }
    #endregion

    #region 用户设置模块
    /// <summary>
    /// 载入配置界面事件，通知模块加载界面
    /// </summary>
    class LoadUserConfigFormEvent : EventArgs
    {
    }
    #endregion

    #region 数据库模块
    #endregion

    #region 通信部分
    /// <summary>
    /// 更改上传项目时间周期，用于通信
    /// </summary>
    class ChangeUploadPeriodEvent : EventArgs
    {
        /// <summary>
        /// <上传项目名，上传周期>键值对
        /// </summary>
        public Dictionary<string, TimeSpan> periodTable;

        public ChangeUploadPeriodEvent(Dictionary<string, TimeSpan> periodTable)
        {
            this.periodTable = periodTable;
        }
    }

    /// <summary>
    /// 性能数据，用于通信
    /// </summary>
    class PerformanceData : EventArgs
    {
        /// <summary>
        /// 机器名
        /// </summary>
        public string machineName;

        /// <summary>
        /// 监视项目名
        /// </summary>
        public string categoryName;

        /// <summary>
        /// 上传日期
        /// </summary>
        public DateTime date;

        /// <summary>
        /// 实例名/值对
        /// </summary>
        public Dictionary<string, object> instanceTable;

        public PerformanceData(string machineName, string categoryName, DateTime date, Dictionary<string, object> instanceTable)
        {
            this.machineName = machineName;
            this.categoryName = categoryName;
            this.date = date;
            this.instanceTable = instanceTable;
        }
    }
    #endregion

}
