using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorClient
{
    public partial class UserConfigForm : Form
    {
        #region 初始化
        public UserConfigForm()
        {
            InitializeComponent();
            this.Load += ConfigForm_Load;
            this.SizeChanged += ConfigForm_SizeChanged;
            this.notifyIconMenuStrip.Items[0].Enabled = false;
        }

        void ConfigForm_Load(object sender, EventArgs e)
        {
            this.notifyIcon.MouseClick += notifyIcon_MouseClick;
            this.notifyIcon.ContextMenuStrip = this.notifyIconMenuStrip;
            this.WindowState = FormWindowState.Minimized;     //默认最小化窗口
        }
        #endregion

 

        #region 最小化/恢复窗口
        /// <summary>
        /// 最小化按钮触发最小化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ConfigForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.MinimizeForm();
            }
        }

        /// <summary>
        /// 最小化窗口
        /// </summary>
        void MinimizeForm()
        {
            this.Visible = false;   //隐藏窗体
            notifyIcon.Visible = true; //任务栏显示图标
            notifyIcon.ShowBalloonTip(3500, "提示", "客户端已经运行", ToolTipIcon.Info); //出显汽泡提示，可以不用
            this.ShowInTaskbar = false; //从状态栏中隐藏
        }

        #endregion

        #region 任务栏菜单
        /// <summary>
        /// 左击恢复窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// 单击配置键，恢复窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// 单击退出键，退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
