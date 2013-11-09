namespace MonitorServer
{
    partial class UserConfigForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserConfigForm));
            this.categoryComboBox = new System.Windows.Forms.ComboBox();
            this.categoryLabel = new System.Windows.Forms.Label();
            this.hours = new System.Windows.Forms.NumericUpDown();
            this.minutes = new System.Windows.Forms.NumericUpDown();
            this.seconds = new System.Windows.Forms.NumericUpDown();
            this.periodLabel = new System.Windows.Forms.Label();
            this.hourLabel = new System.Windows.Forms.Label();
            this.minuteLabel = new System.Windows.Forms.Label();
            this.secondLabel = new System.Windows.Forms.Label();
            this.OK = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyIconMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.GraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.machineComboBox = new System.Windows.Forms.ComboBox();
            this.machineLabel = new System.Windows.Forms.Label();
            this.machineState = new System.Windows.Forms.TextBox();
            this.stateLabel = new System.Windows.Forms.Label();
            this.machineRefresh = new System.Windows.Forms.Timer(this.components);
            this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.hours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.seconds)).BeginInit();
            this.notifyIconMenuStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // categoryComboBox
            // 
            this.categoryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.categoryComboBox.FormattingEnabled = true;
            this.categoryComboBox.Location = new System.Drawing.Point(8, 90);
            this.categoryComboBox.Name = "categoryComboBox";
            this.categoryComboBox.Size = new System.Drawing.Size(170, 20);
            this.categoryComboBox.TabIndex = 0;
            this.categoryComboBox.SelectedIndexChanged += new System.EventHandler(this.categoryComboBox_SelectedIndexChanged);
            // 
            // categoryLabel
            // 
            this.categoryLabel.AutoSize = true;
            this.categoryLabel.Location = new System.Drawing.Point(6, 75);
            this.categoryLabel.Name = "categoryLabel";
            this.categoryLabel.Size = new System.Drawing.Size(53, 12);
            this.categoryLabel.TabIndex = 1;
            this.categoryLabel.Text = "监视项目";
            // 
            // hours
            // 
            this.hours.Location = new System.Drawing.Point(0, 33);
            this.hours.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.hours.Name = "hours";
            this.hours.Size = new System.Drawing.Size(34, 21);
            this.hours.TabIndex = 2;
            // 
            // minutes
            // 
            this.minutes.Location = new System.Drawing.Point(87, 33);
            this.minutes.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.minutes.Name = "minutes";
            this.minutes.Size = new System.Drawing.Size(34, 21);
            this.minutes.TabIndex = 3;
            // 
            // seconds
            // 
            this.seconds.Location = new System.Drawing.Point(174, 33);
            this.seconds.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.seconds.Name = "seconds";
            this.seconds.Size = new System.Drawing.Size(34, 21);
            this.seconds.TabIndex = 4;
            this.seconds.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // periodLabel
            // 
            this.periodLabel.AutoSize = true;
            this.periodLabel.Location = new System.Drawing.Point(-2, 18);
            this.periodLabel.Name = "periodLabel";
            this.periodLabel.Size = new System.Drawing.Size(53, 12);
            this.periodLabel.TabIndex = 5;
            this.periodLabel.Text = "上传周期";
            // 
            // hourLabel
            // 
            this.hourLabel.AutoSize = true;
            this.hourLabel.Location = new System.Drawing.Point(40, 37);
            this.hourLabel.Name = "hourLabel";
            this.hourLabel.Size = new System.Drawing.Size(29, 12);
            this.hourLabel.TabIndex = 6;
            this.hourLabel.Text = "小时";
            // 
            // minuteLabel
            // 
            this.minuteLabel.AutoSize = true;
            this.minuteLabel.Location = new System.Drawing.Point(127, 37);
            this.minuteLabel.Name = "minuteLabel";
            this.minuteLabel.Size = new System.Drawing.Size(29, 12);
            this.minuteLabel.TabIndex = 7;
            this.minuteLabel.Text = "分钟";
            // 
            // secondLabel
            // 
            this.secondLabel.AutoSize = true;
            this.secondLabel.Location = new System.Drawing.Point(214, 37);
            this.secondLabel.Name = "secondLabel";
            this.secondLabel.Size = new System.Drawing.Size(17, 12);
            this.secondLabel.TabIndex = 8;
            this.secondLabel.Text = "秒";
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(182, 228);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(76, 22);
            this.OK.TabIndex = 9;
            this.OK.Text = "更改";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "配置文件";
            this.notifyIcon.Visible = true;
            // 
            // notifyIconMenuStrip
            // 
            this.notifyIconMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GraphToolStripMenuItem,
            this.ConfigToolStripMenuItem,
            this.ExitToolStripMenuItem});
            this.notifyIconMenuStrip.Name = "notifyIconMenuStrip";
            this.notifyIconMenuStrip.Size = new System.Drawing.Size(153, 92);
            // 
            // GraphToolStripMenuItem
            // 
            this.GraphToolStripMenuItem.Name = "GraphToolStripMenuItem";
            this.GraphToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.GraphToolStripMenuItem.Text = "图像";
            this.GraphToolStripMenuItem.Click += new System.EventHandler(this.GraphToolStripMenuItem_Click);
            // 
            // ConfigToolStripMenuItem
            // 
            this.ConfigToolStripMenuItem.Name = "ConfigToolStripMenuItem";
            this.ConfigToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.ConfigToolStripMenuItem.Text = "配置";
            this.ConfigToolStripMenuItem.Click += new System.EventHandler(this.ConfigToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.secondLabel);
            this.panel1.Controls.Add(this.minuteLabel);
            this.panel1.Controls.Add(this.hourLabel);
            this.panel1.Controls.Add(this.periodLabel);
            this.panel1.Controls.Add(this.seconds);
            this.panel1.Controls.Add(this.minutes);
            this.panel1.Controls.Add(this.hours);
            this.panel1.Location = new System.Drawing.Point(8, 116);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(250, 64);
            this.panel1.TabIndex = 10;
            // 
            // machineComboBox
            // 
            this.machineComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.machineComboBox.FormattingEnabled = true;
            this.machineComboBox.Location = new System.Drawing.Point(8, 33);
            this.machineComboBox.Name = "machineComboBox";
            this.machineComboBox.Size = new System.Drawing.Size(170, 20);
            this.machineComboBox.TabIndex = 11;
            this.machineComboBox.SelectedIndexChanged += new System.EventHandler(this.machineComboBox_SelectedIndexChanged);
            // 
            // machineLabel
            // 
            this.machineLabel.AutoSize = true;
            this.machineLabel.Location = new System.Drawing.Point(6, 18);
            this.machineLabel.Name = "machineLabel";
            this.machineLabel.Size = new System.Drawing.Size(41, 12);
            this.machineLabel.TabIndex = 12;
            this.machineLabel.Text = "机器名";
            // 
            // machineState
            // 
            this.machineState.Location = new System.Drawing.Point(65, 186);
            this.machineState.Name = "machineState";
            this.machineState.ReadOnly = true;
            this.machineState.Size = new System.Drawing.Size(52, 21);
            this.machineState.TabIndex = 13;
            // 
            // stateLabel
            // 
            this.stateLabel.AutoSize = true;
            this.stateLabel.Location = new System.Drawing.Point(6, 189);
            this.stateLabel.Name = "stateLabel";
            this.stateLabel.Size = new System.Drawing.Size(53, 12);
            this.stateLabel.TabIndex = 14;
            this.stateLabel.Text = "机器状态";
            // 
            // machineRefresh
            // 
            this.machineRefresh.Enabled = true;
            this.machineRefresh.Interval = 1000;
            this.machineRefresh.Tick += new System.EventHandler(this.machineRefresh_Tick);
            // 
            // ExitToolStripMenuItem
            // 
            this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
            this.ExitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.ExitToolStripMenuItem.Text = "退出";
            this.ExitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // UserConfigForm
            // 
            this.AcceptButton = this.OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.stateLabel);
            this.Controls.Add(this.machineState);
            this.Controls.Add(this.machineLabel);
            this.Controls.Add(this.machineComboBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.categoryLabel);
            this.Controls.Add(this.categoryComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "UserConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "配置";
            ((System.ComponentModel.ISupportInitialize)(this.hours)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.seconds)).EndInit();
            this.notifyIconMenuStrip.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox categoryComboBox;
        private System.Windows.Forms.Label categoryLabel;
        private System.Windows.Forms.NumericUpDown hours;
        private System.Windows.Forms.NumericUpDown minutes;
        private System.Windows.Forms.NumericUpDown seconds;
        private System.Windows.Forms.Label periodLabel;
        private System.Windows.Forms.Label hourLabel;
        private System.Windows.Forms.Label minuteLabel;
        private System.Windows.Forms.Label secondLabel;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip notifyIconMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem GraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ConfigToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox machineComboBox;
        private System.Windows.Forms.Label machineLabel;
        private System.Windows.Forms.TextBox machineState;
        private System.Windows.Forms.Label stateLabel;
        private System.Windows.Forms.Timer machineRefresh;
        private System.Windows.Forms.ToolStripMenuItem ExitToolStripMenuItem;
    }
}

