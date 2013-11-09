/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/2
 * Description: 性能监视系统的客户端程序所需要的所有全局属性。
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
    /// 所有可访问的全局变量
    /// </summary>
    static class Global
    {
        #region 处理器
        /// <summary>
        /// 主控枢纽处理器
        /// </summary>
        public static ControlHub hub = new ControlHub();

        /// <summary>
        /// 性能探测处理器
        /// </summary>
        public static PerformanceCollector performanceCollector = new PerformanceCollector();

        /// <summary>
        /// 用户配置处理器
        /// </summary>
        public static UserConfig userConfig = new UserConfig();

        /// <summary>
        /// 上传处理器
        /// </summary>
        public static Upload upload = new Upload();

        /// <summary>
        /// 下载处理器
        /// </summary>
        public static Download download = new Download();

        /// <summary>
        /// 测试处理器，仅供测试用
        /// </summary>
        public static Test test = new Test();

        #endregion

        #region 全局数据
        /// <summary>
        /// 项目性能值
        /// </summary>
        public static CategoryValues categoryValues = new CategoryValues();

        /// <summary>
        /// 对应项目性能上传周期
        /// </summary>
        public static Dictionary<string, TimeSpan> uploadPeriodTable = new Dictionary<string, TimeSpan>();
        #endregion

        #region 文件路径
        /// <summary>
        /// 用于加载dll探针库的引导文件
        /// </summary>
        public const string probeBootFile = "ProbeBoot.ini";
        /// <summary>
        /// 用于加载服务器ip
        /// </summary>
        public const string serverIPFile = "ServerIp.ini";
        #endregion  

        #region 通信地址
        /// <summary>
        /// 服务器ip
        /// </summary>
        public static string ip;

        /// <summary>
        /// 下载端口，可更改，与服务器统一
        /// </summary>
        public const Int32 downloadPort = 1234;

        /// <summary>
        /// 上传端口，可更改，与服务器统一
        /// </summary>
        public const Int32 uploadPort = 4321;
        #endregion

        #region 本地信息
        /// <summary>
        /// 本机名称
        /// </summary>
        public static string machineName = Environment.MachineName;
        #endregion
    }
}
