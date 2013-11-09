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
    public partial class Form3 : Form
    {
        public double value;

        public bool OK = false;

        public Form3(double value, string machineName, string categoryName, string instanceName)
        {
            InitializeComponent();
            this.numericUpDown1.Value = (decimal)value;
            this.textBox1.Text = machineName;
            this.textBox2.Text = categoryName;
            this.textBox3.Text = instanceName;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            value = (double)this.numericUpDown1.Value;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            value = (double)this.numericUpDown1.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            value = (double)this.numericUpDown1.Value;
            OK = true;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OK = false;
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
