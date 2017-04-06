using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Beanfun
{
    public partial class BeanfunClient : WebClient
    {
        private const string userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";

        public class Account
        {
            public string sacc;
            public string sotp;
            public string sname;
            public string screatetime;

            public Account()
            {
                this.sacc = null;
                this.sotp = null;
                this.sname = null;
                this.screatetime = null;
            }
            public Account(string sacc, string sotp, string sname, string screatetime = null)
            {
                this.sacc = sacc;
                this.sotp = sotp;
                this.sname = sname;
                this.screatetime = screatetime;
            }
        }

        private CookieContainer CookieContainer; //存Cookie的容器

        private Uri ResponseUri;
        private bool redirect; //是否自動導向
        private string webtoken; //beanfun認證的cookie

        private string service_code; //遊戲編碼
        private string service_region; //遊戲代號

        public string errorMessage; //記錄錯誤訊息

        public List<Account> accountList;

        public BeanfunClient()
        {
            this.CookieContainer = new System.Net.CookieContainer();
            this.Headers.Set("User-Agent", userAgent);

            this.ResponseUri = null;
            this.redirect = true;
            this.webtoken = null;

            this.accountList = new List<Account>();
        }

        public string GetSessionkey()
        {
            string response = this.DownloadString("https://tw.beanfun.com/beanfun_block/bflogin/default.aspx?service=999999_T0");

            response = this.ResponseUri.ToString();

            if (response == null)
            {
                this.errorMessage = "LoginNoResponse";
                return null;
            }

            Regex regex = new Regex("skey=(.*)&display");

            if (!regex.IsMatch(response))
            {
                this.errorMessage = "LoginNoSkey";
                return null;
            }

            return regex.Match(response).Groups[1].Value;
        }

        /// <summary>
        /// 登入Beanfun
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="service_code"></param>
        /// <param name="service_region"></param>
        public void Login(string account, string password, string service_code, string service_region)
        {
            try
            {
                string response = "";

                string sessionKey = this.GetSessionkey();

                if (sessionKey == null)
                {
                    return;
                }

                response = this.DownloadString("https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey=" + sessionKey);

                //拿取VIEWSTATE
                Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                string viewstate = regex.Match(response).Groups[1].Value;

                //拿取EVENTVALIDATION
                regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                string eventvalidation = regex.Match(response).Groups[1].Value;

                //拿取VIEWSTATEGENERATOR
                regex = new Regex("id=\"__VIEWSTATEGENERATOR\" value=\"(.*)\" />");
                string viewstateGenerator = regex.Match(response).Groups[1].Value;

                //拿取LBD_VCID_c_login_idpass_form_samplecaptcha
                regex = new Regex("id=\"LBD_VCID_c_login_idpass_form_samplecaptcha\" value=\"(.*)\" />");
                string samplecaptcha = regex.Match(response).Groups[1].Value;

                //類似HashMap
                NameValueCollection payload = new NameValueCollection();
                payload.Add("__EVENTTARGET", "");
                payload.Add("__EVENTARGUMENT", "");
                payload.Add("__VIEWSTATE", viewstate);
                payload.Add("__VIEWSTATEGENERATOR", viewstateGenerator);
                payload.Add("__EVENTVALIDATION", eventvalidation);
                payload.Add("t_AccountID", account);
                payload.Add("t_Password", password);
                payload.Add("CodeTextBox", "");
                payload.Add("btn_login.x", "0");
                payload.Add("btn_login.y", "0");
                payload.Add("LBD_VCID_c_login_idpass_form_samplecaptcha", samplecaptcha);

                //上傳並且拿取得回應結果
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey=" + sessionKey, payload));

                //取得akey
                regex = new Regex("akey=(.*)");
                if (!regex.IsMatch(this.ResponseUri.ToString()))
                {
                    this.errorMessage = "LoginNoAkey";
                    return;
                }

                string authKey = regex.Match(this.ResponseUri.ToString()).Groups[1].Value;

                payload = new NameValueCollection();
                payload.Add("SessionKey", sessionKey);
                payload.Add("AuthKey", authKey);

                //上傳並且拿取得回應結果
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.beanfun.com/beanfun_block/bflogin/return.aspx", payload));
                response = this.DownloadString("https://tw.beanfun.com/" + this.ResponseHeaders["Location"]);

                //拿取名稱為bfWebToken的Cookie
                this.webtoken = this.GetCookie("bfWebToken");

                if (webtoken == "")
                {
                    this.errorMessage = "LoginNoWebtoken";
                    return;
                }

                //將webtoken、service_code、service_region GET到該網站上 取得回應結果
                response = this.DownloadString("https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx%3Fservice_code_and_region%3D" + service_code + "_" + service_region + "&web_token=" + webtoken, Encoding.UTF8);

                regex = new Regex("<div id=\"(\\w+)\" sn=\"(\\d+)\" name=\"([^\"]+)\"");

                this.accountList.Clear();

                foreach (Match match in regex.Matches(response))
                {
                    if (match.Groups[1].Value == "" || match.Groups[2].Value == "" || match.Groups[3].Value == "")
                    {
                        continue;
                    }
                    accountList.Add(new Account(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
                }
            }
            catch (Exception error)
            {
                if (error is WebException)
                {
                    this.errorMessage = "網路連線錯誤，請檢查網路連線是否正常。";
                }
                else
                {
                    this.errorMessage = "LoginUnknown\n\n" + error.Message + "\n" + error.StackTrace;
                }
            }
        }

        /// <summary>
        /// 拿取Beanfun登入密碼
        /// </summary>
        /// <param name="account"></param>
        /// <param name="service_code"></param>
        /// <param name="service_region"></param>
        /// <returns>解密後的密碼</returns>
        public string getOTP(Account account, string service_code, string service_region)
        {
            try
            {
                string response;

                response = this.DownloadString("https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start_step2.aspx%3Fservice_code%3D" + service_code + "%26service_region%3D" + service_region + "%26sotp%3D" + account.sotp + "&web_token=" + this.webtoken);

                Regex regex = new Regex("GetResultByLongPolling&key=(.*)\"");

                string longPollingKey = regex.Match(response).Groups[1].Value;

                if (account.screatetime == null)
                {
                    regex = new Regex("ServiceAccountCreateTime: \"([^\"]+)\"");

                    account.screatetime = regex.Match(response).Groups[1].Value;
                }
                response = this.DownloadString("https://tw.newlogin.beanfun.com/generic_handlers/get_cookies.ashx", Encoding.UTF8);

                //拿取秘密編碼
                regex = new Regex("var m_strSecretCode = '(.*)';");

                string secretCode = regex.Match(response).Groups[1].Value;

                NameValueCollection payload = new NameValueCollection();
                payload.Add("service_code", service_code);
                payload.Add("service_region", service_region);
                payload.Add("service_account_id", account.sacc);
                payload.Add("service_sotp", account.sotp);
                payload.Add("service_display_name", account.sname);
                payload.Add("service_create_time", account.screatetime);

                //不知用意
                System.Net.ServicePointManager.Expect100Continue = false;
                response = Encoding.UTF8.GetString(this.UploadValues("https://tw.new.beanfun.com/beanfun_block/generic_handlers/record_service_start.ashx", payload));

                response = this.DownloadString("https://tw.new.beanfun.com/generic_handlers/get_result.ashx?meth=GetResultByLongPolling&key=" + longPollingKey + "&_=" + GetCurrentTime());
                response = this.DownloadString("https://tw.new.beanfun.com/beanfun_block/generic_handlers/get_webstart_otp.ashx?SN=" + longPollingKey + "&WebToken=" + this.webtoken + "&SecretCode=" + secretCode + "&ppppp=FE40250C435D81475BF8F8009348B2D7F56A5FFB163A12170AD615BBA534B932&ServiceCode=" + service_code + "&ServiceRegion=" + service_region + "&ServiceAccount=" + account.sacc + "&CreateTime=" + account.screatetime.Replace(" ", "%20"));
                response = response.Substring(2);
                string key = response.Substring(0, 8); //取得鑰匙
                string plain = response.Substring(8); //取得密碼
                string otp = DecryptDES(plain, key); //用鑰匙把密碼做DES解密

                return otp;
            }
            catch(Exception error)
            {
                this.errorMessage = "獲取密碼失敗，請嘗試重新登入。";
                return null;
            }
            
        }

        //把拿到的 OTP 和 KEY 做DES解碼
        private string DecryptDES(string hexString, string key)
        {
            try
            {
                DESCryptoServiceProvider desc = new DESCryptoServiceProvider();
                desc.Mode = CipherMode.ECB;
                desc.Padding = PaddingMode.None;
                desc.Key = Encoding.ASCII.GetBytes(key);

                byte[] s = new byte[hexString.Length / 2];

                int j = 0;
                for (int i = 0; i < hexString.Length / 2; i++)
                {
                    s[i] = Byte.Parse(hexString[j].ToString() + hexString[j + 1].ToString(), System.Globalization.NumberStyles.HexNumber);
                    j += 2;
                }

                ICryptoTransform desencrypt = desc.CreateDecryptor();
                return Encoding.ASCII.GetString(desencrypt.TransformFinalBlock(s, 0, s.Length));
            }
            catch (Exception error)
            {
                return null;
            }
        }

        public string DownloadString(string Uri, Encoding Encoding)
        {
            var response = (Encoding.GetString(base.DownloadData(Uri)));
            return response;
        }

        public string DownloadString(string Uri)
        {
            this.Headers.Set("User-Agent", userAgent);
            var response = base.DownloadString(Uri);
            return response;
        }

        /// <summary>
        /// POST 類似模擬form標籤
        /// </summary>
        /// <param name="skey"></param>
        /// <param name="payload"></param>
        /// <returns>回傳byte類型的網頁訊息</returns>
        public byte[] UploadValues(string skey, NameValueCollection payload)
        {
            this.Headers.Set("User-Agent", userAgent);
            return base.UploadValues(skey, payload);
        }

        /// <summary>
        /// 拿取Cookie
        /// </summary>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        private string GetCookie(string cookieName)
        {
            //指定拿取哪個網頁的Cookies
            foreach (Cookie cookie in this.CookieContainer.GetCookies(new Uri("https://tw.beanfun.com/")))
            {
                if (cookie.Name == cookieName)
                {
                    return cookie.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 覆寫GetWebResponse方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                WebResponse webResponse = base.GetWebResponse(request);
                this.ResponseUri = webResponse.ResponseUri; //這邊可能是怕跳轉，所以紀錄跳轉後的網址 後續可以拿skey
                return webResponse;
            }
            catch(Exception error)
            {
                return null;
            }
            
        }

        /// <summary>
        /// 覆寫GetWebRequest方法
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            webRequest.Timeout = 1000;
            HttpWebRequest httpRequest = webRequest as HttpWebRequest;

            if (httpRequest != null)
            {
                httpRequest.CookieContainer = this.CookieContainer; //紀錄Cookie
                httpRequest.AllowAutoRedirect = this.redirect; //允許跳轉
            }
            return webRequest;
        }

        private string GetCurrentTime(int method = 0)
        {
            DateTime date = DateTime.Now;

            switch (method)
            {
                case 1:
                    return (date.Year - 1900).ToString() + (date.Month - 1).ToString() + date.ToString("ddHHmmssfff");
                case 2:
                    return date.Year.ToString() + (date.Month - 1).ToString() + date.ToString("ddHHmmssfff");
                default:
                    return date.ToString("yyyyMMddHHmmss.fff");
            }
        }

        public string Ping()
        {
            try
            {
                byte[] buffer = null;

                buffer = this.DownloadData("http://tw.beanfun.com/beanfun_block/generic_handlers/echo_token.ashx?webtoken=1");
                string response = Encoding.GetString(buffer);

                Debug.WriteLine(GetCurrentTime() + " @ " + response); //顯示ping的資訊

                return "OK";
            }
            catch(Exception error)
            {
                if (error is WebException)
                {
                    this.errorMessage = "網路連線錯誤，請檢查官方網站連線是否正常。\n\n" + error.Message;
                }
                else
                {
                    this.errorMessage = "LoginUnknown\n\n" + error.Message + "\n" + error.StackTrace;
                }

                return null;
            }
        }
    }
}