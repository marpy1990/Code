/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/5
 * Description: 性能监视系统的服务器程序所需要的所有全局属性。
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorServer
{
    static class Global
    {
        #region 处理器
        /// <summary>
        /// 主控处理器
        /// </summary>
        public static ControlHub hub = new ControlHub();

        /// <summary>
        /// 接收处理器
        /// </summary>
        public static Receive receive = new Receive();

        /// <summary>
        /// 发送处理器
        /// </summary>
        public static Send send = new Send();

        /// <summary>
        /// 用户设置处理器
        /// </summary>
        public static UserConfig userConfig = new UserConfig();

        /// <summary>
        /// 数据库处理器，负责与数据库交互
        /// </summary>
        public static SQLTransport sqlTrans = new SQLTransport();
        #endregion

        #region 通信地址
        /// <summary>
        /// 接收端口
        /// </summary>
        public const Int32 receivePort = 4321;

        /// <summary>
        /// 发送端口
        /// </summary>
        public const Int32 sendPort = 1234;
        #endregion

        #region 客户端列表

        public const string clientInfoBootFile = "clientsBoot.ini";

        /// <summary>
        /// 客户端列表
        /// </summary>
        public static Dictionary<string, ClientInfo> clientsTable;

        /// <summary>
        /// 客户端实时值，用于显示
        /// </summary>
        public static Dictionary<string, Dictionary<string, Dictionary<string, ValueBind>>> realtimeValueTable
            = new Dictionary<string, Dictionary<string, Dictionary<string, ValueBind>>>();

        /// <summary>
        /// 默认的上传时间列表
        /// </summary>
        public static Dictionary<string, TimeSpan> defaultUploadPeriodTable = new Dictionary<string, TimeSpan>
        {
            {   "CPU总使用率百分比(百分数)"                 ,  new TimeSpan(1,0,10)    },
            {   "处理器总中断时间百分比(百分数)"            ,  new TimeSpan(1,0,10)    },
            {   "处理器处理时间总数百分比(百分数)"            ,  new TimeSpan(1,0,10)    },
            {   "处理器总DPC时间百分比(百分数)"                  ,  new TimeSpan(1,0,10)    },
            {   "逻辑磁盘可用空间(MB)"    ,  new TimeSpan(1,0,10)    },
            {   "逻辑磁盘可用空间百分比(百分数)" , new TimeSpan(1,0,10)},
            {   "逻辑磁盘每次传输的平均秒数(秒)",  new TimeSpan(1,0,10)    },
            {   "逻辑磁盘每次读取的平均秒数(秒)"    ,  new TimeSpan(1,0,10)    },
            {   "逻辑磁盘每次写入的平均秒数(秒)"   ,  new TimeSpan(1,0,10)    },
            {   "实体磁盘每次传输的平均秒数(秒)",  new TimeSpan(1,0,10)    },
            {   "实体磁盘每次读取的平均秒数(秒)"    ,  new TimeSpan(1,0,10)    },
            {   "实体磁盘每次写入的平均秒数(秒)"   ,  new TimeSpan(1,0,10)    },
            {   "内存可用空间(MB)"    ,  new TimeSpan(1,0,10)    },
            {   "内存总量(GB)"               ,  new TimeSpan(1,0,10)},
            {   "数据库AutoClose Flag(布尔型)"            ,  new TimeSpan(1,0,10)    },
			{	"数据库AutoCreateStatistics Flag(布尔型)",  new TimeSpan(1,0,10)    },
			{	"数据库AutoShrink Flag(布尔型)"           ,  new TimeSpan(1,0,10)    },
			{	"数据库Chaining Flag(布尔型)"           ,  new TimeSpan(1,0,10)    },
			{	"数据库AutoUpdate Flag(布尔型)"           ,  new TimeSpan(1,0,10)    },
			{	"数据库剩余空间百分比(百分数)"             ,  new TimeSpan(1,0,1)     },
            {   "数据库使用空间的改变量(百分数/秒)"       ,  new TimeSpan(0,0,6)     },
            {   "数据库事务日志剩余空间百分比(百分数)%",  new TimeSpan(0,0,10)    },
            {   "数据库版本号"                ,  new TimeSpan(0,0,10)    },
            {   "SQL 2008 Agent状态"             ,  new TimeSpan(0,0,10)    },
            {   "SQL Server Analysis Services状态",  new TimeSpan(0,0,10)    },
            {   "SQL Server状态"             ,  new TimeSpan(0,0,10)    },
            {   "SQL 2005 Integration Services状态",  new TimeSpan(0,0,10)    },
            {   "SQL Server Reporting Services状态",  new TimeSpan(0,0,10)    },
            {   "SQL Server Full Text Search Service状态",  new TimeSpan(0,0,10)    },
            {   "应用程序集区可用性"         ,  new TimeSpan(0,0,10)    },
            {   "WindowsNT服务可用性"        ,  new TimeSpan(0,0,10)    },
            {   "网站可用性"                 ,  new TimeSpan(0,0,10)    },
        };
        #endregion

        #region 数据库
        /// <summary>
        /// 保存登陆数据库的指令
        /// </summary>
        public const string sqlConnectFile = "sqlConnectText.ini";
        #endregion

        #region 版本
        public static string version = "1.0";
        #endregion
    }
}
