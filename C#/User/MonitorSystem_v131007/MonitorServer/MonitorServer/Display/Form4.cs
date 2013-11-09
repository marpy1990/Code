using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorSystemApplication
{
    public partial class Form4 : Form
    {
        public int k;
        public int p;
        public double s;
        Predictor pd;
        public bool OK = false;
        public int select;
        public int steps;

        public Form4(Predictor pd, string machineName, string categoryName, string instanceName,int steps)
        {
            InitializeComponent();
            this.pd = pd;
            this.numericUpDown1.Value = ((KNN)pd.units[0]).dim;
            this.numericUpDown2.Value = ((ES)pd.units[1]).p;
            this.numericUpDown3.Value = (decimal)((ES)pd.units[1]).s;
            k = ((KNN)pd.units[0]).dim;
            p = ((ES)pd.units[1]).p;
            s = ((ES)pd.units[1]).s;
            this.radioButton1.Checked = false;
            this.radioButton2.Checked = false;
            this.radioButton3.Checked = false;
            select = pd.selector;
            if (!pd.auto)
            {
                if (pd.selector == 0)
                    this.radioButton1.Checked = true;
                else if (pd.selector == 1)
                    this.radioButton2.Checked = true;
            }
            else
            {
                this.radioButton3.Checked = true;
            }
            this.textBox5.Text = machineName;
            this.textBox4.Text = categoryName;
            this.textBox3.Text = instanceName;
            this.steps = steps;
            this.numericUpDown4.Value = steps;
        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton1.Checked)
            {
                this.radioButton2.Checked = false;
                this.radioButton3.Checked = false;
                select = 0;
            }
            
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton2.Checked)
            {
                this.radioButton1.Checked = false;
                this.radioButton3.Checked = false;
                select = 1;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.textBox1.Text = pd.units[0].MRE().ToString();
            this.textBox2.Text = pd.units[1].MRE().ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            k = (int)this.numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            p = (int)this.numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            s = (double)this.numericUpDown3.Value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OK = true;
            this.Close();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton3.Checked)
            {
                this.radioButton1.Checked = false;
                this.radioButton2.Checked = false;
                select = -1;
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            steps = (int)this.numericUpDown4.Value;
        }
    }
}
