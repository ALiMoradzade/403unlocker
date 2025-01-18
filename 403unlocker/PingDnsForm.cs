﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace _403unlocker
{
    public partial class PingDnsForm : Form
    {
        internal BindingList<DnsPing> dnsPingBinding = new BindingList<DnsPing>();

        public PingDnsForm()
        {
            InitializeComponent();
        }

        private void PingDnsForm_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = dnsPingBinding;
            dataGridView1.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["DNS"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["URL"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns["Status"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridView1.Columns["Latency"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        private async void pcPingButton_Click(object sender, EventArgs e)
        {
            foreach (DnsPing dnsPing in dnsPingBinding)
            {
                await dnsPing.GetPing();
            }
            dataGridView1.Invalidate();
        }

        private async void getPingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string selectedRowDns = dataGridView1.SelectedRows[0].Cells["DNS"].Value.ToString();
                DnsPing foundRecord = dnsPingBinding.First(dnsPing => dnsPing.DNS == selectedRowDns);
                await foundRecord.GetPing();
                dataGridView1.Invalidate();
            }
            else
            {
                MessageBox.Show("Please select a row", "Can't Get Ping!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }


        private void sortButton_Click(object sender, EventArgs e)
        {
            // sort by status
            List<DnsPing> sortedDnsPing = dnsPingBinding.OrderBy(dnsPing => dnsPing.Status)
                                                            // then sort by ping
                                                            .ThenBy(dnsPing => dnsPing.Latency)
                                                            .ToList();
            dnsPingBinding = new BindingList<DnsPing>(sortedDnsPing);
            dataGridView1.DataSource = dnsPingBinding;
        }

        private async void sitePingButton_Click(object sender, EventArgs e)
        {
            foreach (DnsPing dnsPing in dnsPingBinding)
            {
                await dnsPing.GetPing(urlTextBox.Text);
            }
            dataGridView1.Invalidate();
        }

        private void urlTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r') e.Handled = true;
        }

    }
}
