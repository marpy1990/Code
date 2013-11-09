using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using MonitorSystem;
//using Forecast;
using ZedGraph;

namespace MonitorSystemApplication
{
    public partial class Form1 : Form
    {
        List<CurveInfo> curList = new List<CurveInfo>();

        int rowNum = 60;

        double timeStart = 0;

        const double oneSecond = 1.0 / 24 / 3600;

        const double timeRange = 50;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            zedGraphControl1_Load(sender, e);
            this.FormClosing += form1_FormClosing;
            //this.Resize+=Form1_Resize;
            this.SizeChanged += Form1_SizeChanged;
            this.listView1.MouseUp += listView1_MouseUp;
            PerformanceCounterCategory pcc = new PerformanceCounterCategory(@"Processor", Environment.MachineName);
            pcc.GetInstanceNames();
        }

        private void form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                //e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                //this.Hide();
                this.ShowInTaskbar = false;
                this.notifyIcon1.Visible = true;
                /*
                TaskIcon.BalloonTipTitle = "分诊系统";
                TaskIcon.BalloonTipText = "程序还在继续运行......" + "\r\n" + "单击托盘图标还原窗口.";
                TaskIcon.ShowBalloonTip(2000);
                 * */
            }
        }

        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {

        }

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {
            this.zedGraphControl1.GraphPane.Title.Text = "Monitor";
            this.zedGraphControl1.GraphPane.XAxis.Title.Text = "Time/Second";
            this.zedGraphControl1.GraphPane.YAxis.Title.Text = "Value";
            this.zedGraphControl1.GraphPane.XAxis.Type = ZedGraph.AxisType.Date;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.ShowDialog();
            f2.Focus();
            foreach (MonitorInfo mtInfo in f2.mtInfoList)
            {
                string machineName = mtInfo.machineName;
                string categoryName = mtInfo.categoryName;
                string instanceName = mtInfo.instanceName;
                Monitor mt = MonitorEnviroment.CreateMonitor(machineName, categoryName, instanceName);
                PointPairList list = new PointPairList();
                
                if (timeStart == 0)
                {
                    timeStart = (double)new XDate(DateTime.Now);
                    this.zedGraphControl1.GraphPane.XAxis.Scale.Max = timeStart + timeRange * oneSecond;
                    this.zedGraphControl1.GraphPane.XAxis.Scale.Min = timeStart;
                }
                /*
                for (int i = 0; i < rowNum; ++i)
                {
                    double x = (double)new XDate(DateTime.Now.AddSeconds(-((rowNum - i)*this.timer1.Interval/1000.0)));
                    double y = 0;
                    list.Add(x, y);
                }
                */
                Color color=ColorLibrary.NextColor();
                LineItem curve = zedGraphControl1.GraphPane.AddCurve(instanceName + "@" + categoryName, list, color, SymbolType.None);
                CurveInfo curInfo = new CurveInfo(mt, list, curve, color);
                curList.Add(curInfo);

                ListViewItem item=new ListViewItem();
                item.UseItemStyleForSubItems = false;
                item.Text="_______";
                item.ForeColor=color;
                item.SubItems.Add(categoryName);
                item.SubItems.Add(instanceName);
                item.SubItems.Add(machineName);
                item.Checked = true;
                item.Tag = curInfo;
                this.listView1.Items.Add(item);
            }
            if (!timer1.Enabled)
                this.timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //zedGraphControl1.GraphPane.XAxis.Scale.MaxAuto = true;

            foreach (ListViewItem item in this.listView1.Items)
            {
                CurveInfo curInfo = (CurveInfo)item.Tag;
                if (!(item.Checked))
                {
                    curInfo.curve.Color = Color.Empty;
                }
                else
                {
                    curInfo.curve.Color = curInfo.color;
                }
            }
            
            foreach (CurveInfo curInfo in curList)
            {
                curInfo.remainSeconds -= timer1.Interval;
                double x = (double)new XDate(DateTime.Now);
                double y = 0;
                double value = 0;

                if (!curInfo.isPredict)
                {
                    y = curInfo.monitor.GetValue();
                    curInfo.AddValue(y);
                }

                if (curInfo.remainSeconds <= 0)
                {
                    if (!curInfo.isPredict)
                    {
                        value = curInfo.PopMeanValue();
                        curInfo.list.Add(x, value);
                    }
                    else
                    {
                        curInfo.pd.Add(curInfo.trainList[curInfo.trainList.Count - 1].Y);
                        y=curInfo.pd.Next(curInfo.nSteps);
                        double dt = curInfo.trainList[curInfo.trainList.Count - 1].X - curInfo.trainList[curInfo.trainList.Count - 2].X;
                        //curInfo.list.Add(curInfo.trainList[curInfo.trainList.Count - 1].X + 4 * curInfo.intrvalSeconds.value / 1000 * oneSecond, y);
                        curInfo.list.Add(curInfo.trainList[curInfo.trainList.Count - 1].X + curInfo.nSteps*dt, y);
                        //curInfo.list.Add(x,curInfo.pd.Next());
                        /*
                        if (curInfo.list.Count > rowNum / curInfo.intrvalSeconds)
                        {
                            curInfo.list.RemoveAt(0);
                        }*/
                    }
                    curInfo.remainSeconds = curInfo.intrvalSeconds.value;
                }
                
            }

            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                CurveInfo curInfo = (CurveInfo)item.Tag;
                if (curInfo.list.Count > 0)
                {
                    double y = curInfo.list.ElementAt(curInfo.list.Count - 1).Y;
                    textBox1.Text = y.ToString();
                    //textBox1.Text = this.zedGraphControl1.GraphPane.XAxis.Scale.Max.ToString();
                    //textBox3.Text = this.zedGraphControl1.GraphPane.XAxis.Scale.Min.ToString();

                    
                    if (curInfo.isPredict)
                    {
                        //textBox2.Text = curInfo.pd.ppd.pd.GetType().ToString()+curInfo.pd.ppd.score.ToString();
                        textBox3.Text = curInfo.pd.MRE().ToString();
                    }
                }
            }

            double time = (double)new XDate(DateTime.Now); 
      
            if (time > this.zedGraphControl1.GraphPane.XAxis.Scale.Max+oneSecond)
            {
                this.zedGraphControl1.GraphPane.XAxis.Scale.Max = time+oneSecond;
                this.zedGraphControl1.GraphPane.XAxis.Scale.Min = time - (timeRange-1) * oneSecond;
            }
            this.zedGraphControl1.AxisChange();
            this.zedGraphControl1.Refresh();
            //this.zedGraphControl1.GraphPane.XAxis.Scale.MaxGrace = (double)new XDate(DateTime.Now.AddSeconds(10));
            //this.zedGraphControl1.GraphPane.XAxis.Scale.MinGrace = (double)new XDate(DateTime.Now.AddSeconds(-10));

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            RemoveCurve();
        }

        private void RemoveCurve()
        {
            foreach (int index in listView1.SelectedIndices)
            {
                CurveInfo curInfo = curList[index];
                curInfo.curve.Clear();
                //curInfo.list.Clear();
                listView1.Items.RemoveAt(index);
                curList.RemoveAt(index);
                this.textBox1.Text = "";
                this.zedGraphControl1.GraphPane.CurveList.RemoveAt(index);
                this.zedGraphControl1.Refresh();
            }
        }

        private void predictToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                CurveInfo curveInf = (CurveInfo)item.Tag;
                List<double> lst=new List<double>();
                foreach(PointPair p in curveInf.list)
                {
                    lst.Add(p.Y);
                }

                Predictor pd = new Predictor(5, lst);

                PointPairList PL = new PointPairList();
                /*
                for (int i = 0; i < curveInf.list.Count; ++i)
                {
                    double x = curveInf.list[i].X;
                    double y = curveInf.list[i].Y;
                    PL.Add(x,y);
                }*/
                Color color=ColorLibrary.NextColor();
                LineItem curve = zedGraphControl1.GraphPane.AddCurve(curveInf.monitor.InstanceName() + "@" + curveInf.monitor.CategoryName()+"预测", PL, color, SymbolType.None);
                CurveInfo pdCurve = new CurveInfo(curveInf.monitor, PL, curve, color);
                pdCurve.curve.Line.Style = DashStyle.Dot;
                pdCurve.isPredict = true;
                pdCurve.intrvalSeconds = curveInf.intrvalSeconds;
                pdCurve.pd = pd;
                pdCurve.remainSeconds = curveInf.remainSeconds;
                pdCurve.intrvalSeconds = curveInf.intrvalSeconds;
                pdCurve.trainList = curveInf.list;
                curList.Add(pdCurve);

                ListViewItem item2 = new ListViewItem();
                item2.UseItemStyleForSubItems = false;
                item2.Text = "_______";
                item2.ForeColor = color;
                item2.SubItems.Add("预测"+curveInf.monitor.CategoryName());
                item2.SubItems.Add(curveInf.monitor.InstanceName());
                item2.SubItems.Add(curveInf.monitor.MachineName());
                item2.Checked = true;
                item2.Tag = pdCurve;
                this.listView1.Items.Add(item2);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveCurve();
        }

        private void propertyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                CurveInfo curveInf = (CurveInfo)item.Tag;
                if (!curveInf.isPredict)
                {
                    Form3 f3 = new Form3(curveInf.intrvalSeconds.value, curveInf.monitor.MachineName(), curveInf.monitor.CategoryName(), curveInf.monitor.InstanceName());
                    f3.ShowDialog();
                    f3.Focus();
                    if (f3.OK)
                    {
                        curveInf.intrvalSeconds.value = f3.value;
                    }
                }
                else
                {
                    Form4 f4 = new Form4(curveInf.pd, curveInf.monitor.MachineName(), curveInf.monitor.CategoryName(), curveInf.monitor.InstanceName(),curveInf.nSteps);
                    f4.ShowDialog();
                    f4.Focus();
                    if (f4.OK)
                    {
                        ((KNN)curveInf.pd.units[0]).dim = f4.k;
                        ((ES)curveInf.pd.units[1]).p = f4.p;
                        ((ES)curveInf.pd.units[1]).s = f4.s;
                        curveInf.nSteps = f4.steps;
                        if (f4.select != -1)
                        {
                            curveInf.pd.selector = f4.select;
                            curveInf.pd.auto = false;
                        }
                        else
                            curveInf.pd.auto = true;
                    }
                }
            }
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                listView1.ContextMenuStrip = null;

                foreach (int index in listView1.SelectedIndices)
                {
                    contextMenuStrip1.Show(listView1, new Point(e.X, e.Y));
                }
            }
            listView1.Refresh();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            bool a=false;
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                CurveInfo cur = (CurveInfo)item.Tag;
                a = cur.isPredict;
                //MessageBox.Show(a.ToString());
            }

            if (a)
            {
                this.contextMenuStrip1.Items[0].Enabled = false;
            }
            else
            {
                this.contextMenuStrip1.Items[0].Enabled = true;
            }
            listView1.Refresh();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true; //显示窗体

            WindowState = FormWindowState.Normal;  //恢复窗体默认大小
            this.ShowInTaskbar = true; 
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            /*
            if (this.WindowState == FormWindowState.Minimized)
            {

                this.Visible = false;   //隐藏窗体
                notifyIcon1.Visible = true; //任务栏显示图标
                notifyIcon1.ShowBalloonTip(3500, "提示", "双击恢复窗口", ToolTipIcon.Info); //出显汽泡提示，可以不用
                this.ShowInTaskbar = false; //从状态栏中隐藏
            }
            */
        }
    }
}
