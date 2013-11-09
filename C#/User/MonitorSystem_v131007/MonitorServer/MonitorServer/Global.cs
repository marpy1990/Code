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
            {   "%CPU Usage"                 ,  new TimeSpan(1,0,10)    },
            {   "%Interrupt Time"            ,  new TimeSpan(1,0,10)    },
            {   "%Processor Time"            ,  new TimeSpan(1,0,10)    },
            {   "%DPC Time"                  ,  new TimeSpan(1,0,10)    },
            {   "Logical Disc Free Space"    ,  new TimeSpan(1,0,10)    },
            {   "Avg. Log. Disk sec/Transfer",  new TimeSpan(1,0,10)    },
            {   "Avg. Log. Disk sec/Read"    ,  new TimeSpan(1,0,10)    },
            {   "Avg. Log. Disk sec/Write"   ,  new TimeSpan(1,0,10)    },
            {   "Avg. Phs. Disk sec/Transfer",  new TimeSpan(1,0,10)    },
            {   "Avg. Phs. Disk sec/Read"    ,  new TimeSpan(1,0,10)    },
            {   "Avg. Phs. Disk sec/Write"   ,  new TimeSpan(1,0,10)    },
            {   "Memory Available MBytes"    ,  new TimeSpan(1,0,10)    },
            {   "Auto Close Flag"            ,  new TimeSpan(1,0,10)    },
			{	"Auto Create Statistics Flag",  new TimeSpan(1,0,10)    },
			{	"Auto Shrink Flag"           ,  new TimeSpan(1,0,10)    },
			{	"DB Chaining Flag"           ,  new TimeSpan(1,0,10)    },
			{	"Auto Update Flag"           ,  new TimeSpan(1,0,10)    },
			{	"DB Space Free%"             ,  new TimeSpan(1,0,1)     },
            {   "DB UsedSpace% Change"       ,  new TimeSpan(0,0,6)     },
            {   "Transaction Log Space Free%",  new TimeSpan(0,0,10)    },
            {   "SQL Version"                ,  new TimeSpan(0,0,10)    },
            {   "SQL 2008 Agent"             ,  new TimeSpan(0,0,10)    },
            {   "SQL Server Analysis Services",  new TimeSpan(0,0,10)    },
            {   "SQL SQL Server"             ,  new TimeSpan(0,0,10)    },
            {   "SQL 2005Integration Services",  new TimeSpan(0,0,10)    },
            {   "SQL Server Reporting Services",  new TimeSpan(0,0,10)    },
            {   "SQL Server Full Text Search Service",  new TimeSpan(0,0,10)    },
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
