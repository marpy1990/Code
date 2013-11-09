using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorServer
{
    static class MainEntry
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Global.hub.Start();
            Global.receive.Start();
            Global.send.Start();
            Global.userConfig.Start();
            Global.sqlTrans.Start();

            Global.hub.Join();
        }
    }
}
