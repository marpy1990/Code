/********************************************************************************
 * Author:  marpy
 * Date:    2013/10/3
 * Description: 用户配置界面模块，向用户提供管理界面管理客户端系统
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonitorEvent;

namespace MonitorServer
{
    /// <summary>
    /// 用户配置界面模块，提供管理界面
    /// </summary>
    class UserConfig : Processor
    {
        /// <summary>
        /// 载入配置界面的主控方法
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件数据</param>
        private void LoadUserConfigForm(Processor sender, EventArgs e)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UserConfigForm());
            SendEvent(Global.hub, new CloseServerEvent());  //关闭窗口，结束程序
        }

        /// <summary>
        /// 初始化，绑定事件处理方法
        /// </summary>
        public UserConfig()
        {
            eventHandlePeriod = new TimeSpan(0, 0, 1);
            this.eventHandleTable.Add(typeof(LoadUserConfigFormEvent), this.LoadUserConfigForm); //绑定界面处理方法
        }
    }
}
