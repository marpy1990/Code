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
    #region 主控模块事件
    /// <summary>
    /// 探针已经加载完毕
    /// </summary>
    class ProbleBuildedConfirmEvent : EventArgs
    {
    }

    /// <summary>
    /// 关闭客户端
    /// </summary>
    class CloseClientEvent : EventArgs
    {
    }
    #endregion

    #region 性能采集器事件
    /// <summary>
    /// 探针构建事件，通知处理器构建所需探针
    /// </summary>
    class ProbeBuildingEvent : EventArgs
    {
    }

    /// <summary>
    /// 性能采集事件，通知探针进行一次性能采集，并更新性能数据表
    /// </summary>
    class PerformanceCollectEvent : EventArgs
    {
        /// <summary>
        /// 采集事件的发送时间
        /// </summary>
        public DateTime sendDate;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sendDate">采集事件的发送时间</param>
        public PerformanceCollectEvent(DateTime sendDate)
        {
            this.sendDate = sendDate;
        }
    }

    #endregion

    #region 用户配置界面模块事件
    /// <summary>
    /// 载入配置界面事件，通知模块加载界面
    /// </summary>
    class LoadUserConfigFormEvent : EventArgs
    {
    }
    #endregion

    #region 上传模块事件
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
    /// 上传对应项目名称的性能数据
    /// </summary>
    class PerformanceDataUploadEvent : EventArgs
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string categoryName;

        /// <summary>
        /// 发送日期
        /// </summary>
        public DateTime sendDate;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="categoryName">项目名称</param>
        /// <param name="sendDate">发送日期</param>
        public PerformanceDataUploadEvent(string categoryName, DateTime sendDate)
        {
            this.categoryName = categoryName;
            this.sendDate = sendDate;
        }
    }

    /// <summary>
    /// 更改上传项目时间周期
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
    #endregion

    #region 下载模块事件
    /// <summary>
    /// 服务器发送给客户端的消息，消息保存在serverEvent
    /// </summary>
    class ServerEvent : EventArgs
    {
        public EventArgs serverEvent;
        public ServerEvent(EventArgs serverEvent)
        {
            this.serverEvent = serverEvent;
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

    #region 与服务器通信事件
    /// <summary>
    /// 探测到的性能数据
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

    #region 测试模块事件，仅供测试用
    class CreatTestFormEvent : EventArgs
    {
    }
    #endregion

}
