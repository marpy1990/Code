using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace MonitorServer
{
    /// <summary>
    /// 客户端信息
    /// </summary>
    class ClientInfo
    {
        public enum State
        {
            Running,
            Stopped
        }

        /// <summary>
        /// 状态
        /// </summary>
        public State state;

        /// <summary>
        /// 客户端ip
        /// </summary>
        public string ip;

        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime date;

        /// <summary>
        /// 上传周期
        /// </summary>
        public Dictionary<string, TimeSpan> uploadPeriodTable = new Dictionary<string, TimeSpan>();

        public ClientInfo(State state, string ip, DateTime date, Dictionary<string, TimeSpan> uploadPeriodTable)
        {
            this.state = state;
            this.ip = ip;
            this.date = date;
            this.uploadPeriodTable = uploadPeriodTable;
        }
    }
}
