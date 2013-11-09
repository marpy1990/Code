using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonitorSystem;

namespace MonitorSystemApplication
{
    public partial class Form2 : Form
    {
        public List<MonitorInfo> mtInfoList=new List<MonitorInfo>();

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            mtInfoList.Clear();
            foreach(string machine in MonitorEnviroment.MachineNames())
            {
                this.comboBoxComputer.Items.Add(machine);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string machine = (string)comboBoxComputer.SelectedItem;
            comboBox2.Items.Clear();
            foreach (string category in MonitorEnviroment.CategoryNames())
            {
                comboBox2.Items.Add(category);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string machine = (string)comboBoxComputer.SelectedItem;
            string category = (string)comboBox2.SelectedItem;
            foreach (object ins in listBox1.SelectedItems)
            {
                string instance = (string)ins;
                ListViewItem item = new ListViewItem();
                item.Text = category;
                item.SubItems.Add(instance);
                item.SubItems.Add(machine);
                this.listView1.Items.Add(item);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string machine = (string)comboBoxComputer.SelectedItem;
            string category = (string)comboBox2.SelectedItem;
            listBox1.Items.Clear();
            foreach (string instance in MonitorEnviroment.InstanceNames(machine, category))
            {
                listBox1.Items.Add(instance);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.Items)
            {
                if (item.Selected)
                {
                    item.Remove();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.Items)
            {
                string category=item.SubItems[0].Text;
                string instance=item.SubItems[1].Text;
                string machine=item.SubItems[2].Text;
                MonitorInfo mtInfo = new MonitorInfo(machine, category, instance);
                mtInfoList.Add(mtInfo);
            }
            this.Close();
        }
    }
}
