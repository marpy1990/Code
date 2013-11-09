/********************************************************************************
 * Author:  marpy
 * Date:    2013/9/28
 * Description: 性能监视系统的客户端程序，从本地采集数据并向服务器上传。
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonitorEvent;

namespace MonitorClient
{
    static class MainEntry
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Global.hub.Start();                     //启动主控模块
            Global.download.Start();                //启动下载模块
            Global.upload.Start();                  //启动上传模块
            Global.performanceCollector.Start();    //启动性能检测模块
            Global.userConfig.Start();              //启动用户界面模块

            Global.hub.Join();

            //System.Environment.Exit(0);
        }
    }
}
