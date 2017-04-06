using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security;
using System.Security.Cryptography;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO;

namespace Beanfun
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 存取beanfun網頁的遊戲資訊
        /// </summary>
        public class GameService
        {
            public string name { get; set; }
            public string service_code { get; set; }
            public string service_region { get; set; }

            public GameService(string name, string service_code, string service_region)
            {
                this.name = name;
                this.service_code = service_code;
                this.service_region = service_region;
            }
        }

        BeanfunClient beanfunClient;

        public List<GameService> gameList;

        string service_code;
        string service_region;

        public Form1()
        {
            gameList = new List<GameService>();

            InitializeComponent();

            bool initResult = initSetting();

            if(!initResult)
            {
                showError("初始化失敗，請檢查網路或重新執行程式!", true);
            }
        }

        public bool initSetting()
        {
            try
            {
                if (Properties.Settings.Default.remember == true)
                {
                    account_textBox.Text = Properties.Settings.Default.account;

                    remember_checkBox.Checked = true;

                    if (File.Exists("UserData.dat"))
                    {
                        try
                        {
                            Byte[] cipher = File.ReadAllBytes("UserData.dat"); //讀檔
                            string entropy = Properties.Settings.Default.entropy; //讀取存在Settings的key
                            byte[] plaintext = ProtectedData.Unprotect(cipher, Encoding.UTF8.GetBytes(entropy), DataProtectionScope.CurrentUser); //解密
                            password_textBox.Text = System.Text.Encoding.UTF8.GetString(plaintext);
                        }
                        catch(Exception error)
                        {
                            File.Delete("UserData.dat"); //發生錯誤就刪除
                        }
                    }
                }

                WebClient webClient = new WebClient();

                string response = Encoding.UTF8.GetString(webClient.DownloadData("http://tw.beanfun.com/game_zone/"));

                Regex regex = new Regex("Services.ServiceList = (.*);");

                if (regex.IsMatch(response))
                {
                    string json = regex.Match(response).Groups[1].Value;
                    JObject jObject = JObject.Parse(json);

                    foreach (var game in jObject["Rows"])
                    {
                        comboBox1.Items.Add((string)game["ServiceFamilyName"]);

                        gameList.Add(new GameService((string)game["ServiceFamilyName"], (string)game["ServiceCode"], (string)game["ServiceRegion"]));
                    }
                }

                comboBox1.SelectedIndex = Properties.Settings.Default.loginGame; //讀取當初選擇的目錄

                return true;
            }
            catch(Exception error)
            {
                return false;
            }
        }

        private void showError(string message, bool isExit = false)
        {
            switch(message)
            {
                case "LoginNoAkey":
                    message = "登入失敗，帳號或密碼錯誤。";
                    break;
                default:
                    break;
            }

            MessageBox.Show(message);

            if(isExit)
            {
                Environment.Exit(Environment.ExitCode);
            }
            else
            {
                beanfunClient.errorMessage = null;
            }
        }

        /// <summary>
        /// 加密 使用ProtectedData.Protect
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <returns>byte[] result</returns>
        private byte[] ciphertext(string plaintext, string key)
        {
            byte[] plainByte = Encoding.UTF8.GetBytes(plaintext);
            byte[] entropy = Encoding.UTF8.GetBytes(key);
            return ProtectedData.Protect(plainByte, entropy, DataProtectionScope.CurrentUser);
        }

        private void login_button_Click(object sender, EventArgs e)
        {
            //如果記住帳密被打勾就執行這邊
            if(remember_checkBox.Checked == true)
            {
                Properties.Settings.Default.account = account_textBox.Text; //將帳號儲存至設定

                //創造一個二進制檔案並將加密後的密碼寫入
                using (BinaryWriter writer = new BinaryWriter(File.Open("UserData.dat", FileMode.Create)))
                {
                    //創造隨機且可重複的8個字元當作key
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    string entropy = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());

                    Properties.Settings.Default.entropy = entropy; //將key儲存至設定
                    writer.Write(ciphertext(password_textBox.Text, entropy)); //寫入加密後的密碼
                }
            }
            else //如果記住密碼沒有被打勾
            {
                Properties.Settings.Default.entropy = ""; //將key從設定清除
                File.Delete("UserData.dat"); //刪除二進制檔案
            }

            Properties.Settings.Default.Save(); //儲存到目前為止的設定

            login_button.Text = "請稍後...";
            login_button.Enabled = false; 
            
            this.UseWaitCursor = true; //讓視窗的Cursor變成轉圈圈
            
            this.loginWorker.RunWorkerAsync(); //loginWorker啟動
        }

        private void loginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("loginWorker starting"); //將這句話顯示在Debug視窗

            beanfunClient = new BeanfunClient(); //創建BeanfunClient實例

            //執行登入方法
            beanfunClient.Login(account_textBox.Text, password_textBox.Text, service_code, service_region); 
        }

        private void loginWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("loginWorker end");

            login_button.Text = "登入";
            login_button.Enabled = true;

            this.UseWaitCursor = false; //讓視窗的Cursor恢復成箭頭

            if (beanfunClient.errorMessage != null) //如果beanfunClient有錯誤訊息則執行下面片段
            {
                showError(beanfunClient.errorMessage);
            }
            else //沒有發生錯誤則執行下片面段
            {
                Form2 form2 = new Form2(this, beanfunClient, comboBox1.Text, account_textBox.Text, service_code, service_region);
                form2.Show(); //顯示帳號清單視窗
                this.Hide(); //隱藏該視窗
            }   
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //將選擇哪款遊戲的索引值記錄到設定之裡
            Properties.Settings.Default.loginGame = this.comboBox1.SelectedIndex; 

            try
            {
                service_code = gameList[this.comboBox1.SelectedIndex].service_code; //更新目前的service_code
                service_region = gameList[this.comboBox1.SelectedIndex].service_region; //更新目前的service_region
            }
            catch
            {

            }
        }

        private void remember_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if(remember_checkBox.Checked == true)
            {
                Properties.Settings.Default.remember = true; //將記住密碼的設定 = true
            }
            else
            {
                Properties.Settings.Default.remember = false; //將記住密碼的設定 = false
            }
        }
    }
}
