using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonitorEvent;

namespace MonitorServer
{
    public partial class UserConfigForm : Form
    {
        #region 初始化
        public UserConfigForm()
        {
            InitializeComponent();
            this.Load += ConfigForm_Load;
            this.SizeChanged += ConfigForm_SizeChanged;
        }

        void ConfigForm_Load(object sender, EventArgs e)
        {
            this.notifyIcon.MouseClick += notifyIcon_MouseClick;
            MachineComboBox_Load();
            this.notifyIcon.ContextMenuStrip = this.notifyIconMenuStrip;
            this.WindowState = FormWindowState.Normal;     //默认正常窗口
        }

        #endregion

        #region 载入机器名
        private void MachineComboBox_Load()
        {
            //this.machineComboBox.Items.Clear();
            try
            {
                foreach (string machineName in Global.clientsTable.Keys)
                {
                    if (!this.machineComboBox.Items.Contains(machineName))
                        this.machineComboBox.Items.Add(machineName);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 每隔一段时间刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void machineRefresh_Tick(object sender, EventArgs e)
        {
            string machineName = (string)this.machineComboBox.SelectedItem;
            MachineComboBox_Load();
            if(this.machineComboBox.SelectedIndex!=-1)
                this.machineState.Text = Global.clientsTable[machineName].state.ToString();
        }

        private void machineComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string machineName = (string)this.machineComboBox.SelectedItem;
            CategoryComboBox_Load(machineName);
            this.machineState.Text = Global.clientsTable[machineName].state.ToString();
        }

        #endregion

        #region 监视项目列表
        /// <summary>
        /// 载入上传对象名
        /// </summary>
        void CategoryComboBox_Load(string  machineName)
        {
            this.categoryComboBox.Items.Clear();
            foreach (string categoryName in Global.clientsTable[machineName].uploadPeriodTable.Keys)
            {
                this.categoryComboBox.Items.Add(categoryName);
            }
        }

        /// <summary>
        /// 选中项目表对象时
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件数据</param>
        private void categoryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string machineName = (string)this.machineComboBox.SelectedItem;
            string categoryName = (string)this.categoryComboBox.SelectedItem;

            ShowPeriodTime(machineName, categoryName);
        }
        #endregion

        #region 项目上传周期
        /// <summary>
        /// 显示上传周期
        /// </summary>
        /// <param name="categoryName">监视项目名称</param>
        void ShowPeriodTime(string machineName, string categoryName)
        {
            try
            {
                TimeSpan period = Global.clientsTable[machineName].uploadPeriodTable[categoryName];
                hours.Value = period.Hours;
                minutes.Value = period.Minutes;
                seconds.Value = period.Seconds;
            }
            catch
            {
                ShowPeriodTimeError();
            }
        }

        /// <summary>
        /// 显示上传周期错误处理，可选
        /// </summary>
        void ShowPeriodTimeError()
        {   
            //可选
        }

        /// <summary>
        /// 确认更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, EventArgs e)
        {
            string machineName = (string)this.machineComboBox.SelectedItem;
            string categoryName = (string)this.categoryComboBox.SelectedItem;
            int hours = (int)this.hours.Value;
            int minutes = (int)this.minutes.Value;
            int seconds = (int)this.seconds.Value;

            try
            {
                ///上传周期不能为0
                if (hours == 0 && minutes == 0 && seconds == 0)
                {
                    MessageBox.Show("上传时间周期不能0！");
                    this.seconds.Value = 1;
                }
                else
                {
                    ClientInfo client = Global.clientsTable[machineName];
                    Dictionary<string, TimeSpan> periodTable = new Dictionary<string, TimeSpan>();
                    periodTable.Add(categoryName, new TimeSpan(hours, minutes, seconds));
                    client.uploadPeriodTable[categoryName] = new TimeSpan(hours, minutes, seconds);
                    Global.hub.SendEvent(Global.send, new ChangeClientUploadPeriodEvent(client.ip, periodTable));
                    notifyIcon.ShowBalloonTip(3500, "提示", "监视项目"+categoryName+"上传时间已更新", ToolTipIcon.Info); //出显汽泡提示，可以不用
                }
            }
            catch
            {
                PeriodChangeError();
            }
        }

        void PeriodChangeError()
        {
            //可选
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
            notifyIcon.ShowBalloonTip(3500, "提示", "单击恢复窗口", ToolTipIcon.Info); //出显汽泡提示，可以不用
            this.ShowInTaskbar = false; //从状态栏中隐藏
        }

        /// <summary>
        /// 恢复窗口
        /// </summary>
        void NormalizeForm()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Visible = true; //显示窗体
                WindowState = FormWindowState.Normal;  //恢复窗体默认大小
                this.ShowInTaskbar = true;
            }
            else
            {
                this.Activate();    //赋予焦点
            }
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
            if (e.Button == MouseButtons.Left)
            {
                NormalizeForm();
            }
        }

        /// <summary>
        /// 显示高级图像窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MonitorSystemApplication.Form1 f1 = new MonitorSystemApplication.Form1();
            f1.Show();
        }

        /// <summary>
        /// 单击配置键，恢复窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NormalizeForm();
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
