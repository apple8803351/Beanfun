using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Beanfun
{
    public partial class Form2 : Form
    {
        private BeanfunClient beanfunClient;

        private string service_code;
        private string service_region;
        private string otp;

        private Form main;

        public Form2()
        {
            InitializeComponent();
        }

        public Form2(Form main, BeanfunClient beanfunClient,string game ,string accout, string service_code, string service_region)
        {
            this.main = main;

            this.beanfunClient = beanfunClient;

            this.service_code = service_code;
            this.service_region = service_region;

            InitializeComponent();

            this.Text = game + "-" + accout; //更改視窗名稱

            pingWorker.RunWorkerAsync(); //執行pingWorker

            //將該遊戲的所有帳號放到listView1中
            foreach (var account in this.beanfunClient.accountList)
            {
                string[] row = { WebUtility.HtmlDecode(account.sname), account.sacc };
                var listViewItem = new ListViewItem(row);
                listView1.Items.Add(listViewItem);
            }

            //如果有選項 就用程式選取第一個選項
            if (listView1.Items.Count > 0)
            {
                listView1.Items[0].Selected = true;
                listView1.Select();
            }
        }

        private void otp_button_Click(object sender, EventArgs e)
        {
            //如果pingWorer正在運作 則停止pingWorker
            if (this.pingWorker.IsBusy)
            {
                this.pingWorker.CancelAsync();
            }

            otp_textBox.Text = "獲取密碼中...";
            listView1.Enabled = false;
            otp_button.Enabled = false;

            getOtpWorker.RunWorkerAsync(listView1.SelectedItems[0].Index);
        }

        //登出被按下的事件
        private void SignOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(getOtpWorker.IsBusy)
            {
                getOtpWorker.CancelAsync();
            }

            if (pingWorker.IsBusy)
            {
                pingWorker.CancelAsync();
            }

            this.Close();
        }

        private void pingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("pingWorker start");

            const int waitSeconds = 60;

            //每60秒 ping beanfun網站一次
            while (true)
            {
                if (pingWorker.CancellationPending)
                {
                    Debug.WriteLine("break duo to cancel");
                    break;
                }

                if (beanfunClient != null)
                {
                    beanfunClient.Ping();
                }

                for (int i = 0; i < waitSeconds; i++)
                {
                    if (pingWorker.CancellationPending)
                    {
                        break;
                    }

                    Thread.Sleep(1000);
                }
            }
        }

        private void pingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("pingWoker end");
        }

        private void getOtpWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (this.pingWorker.IsBusy)
            {
                Thread.Sleep(150);
            }

            Debug.WriteLine("getOtpWorker start");

            int index = (int)e.Argument;

            if (this.beanfunClient.accountList.Count <= index)
            {
                return;
            }

            Debug.WriteLine("call GetOTP");
            otp = beanfunClient.getOTP(this.beanfunClient.accountList[index], this.service_code, this.service_region); //拿取開啟遊戲的密碼
            Debug.WriteLine("call GetOTP done");
        }

        private void getOtpWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("getOtpWorker end");

            otp_textBox.Text = this.otp;

            listView1.Enabled = true;
            otp_button.Enabled = true;
            
            this.pingWorker.RunWorkerAsync(); //拿完繼續ping
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            //當選項被點兩下 就複製該項目的值
            Clipboard.SetText(this.beanfunClient.accountList[this.listView1.SelectedItems[0].Index].sacc);
            MessageBox.Show("成功將帳號複製到剪貼簿!");
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            main.Show(); //當子視窗要被關閉時 開啟主視窗
        }
    }
}
