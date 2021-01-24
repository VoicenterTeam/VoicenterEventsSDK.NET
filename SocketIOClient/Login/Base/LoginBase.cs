using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Script.Serialization;

namespace VoicenterRealtimeAPI.Login.Base
{
    public  enum Identity { Account = 1, User, Token };
    public enum LoginError { OK = 0, RequestFailed, WrongLoginDetails, NoMonitor }
    public abstract class LoginBase
    {

        public Identity identity;

        /// <summary>
        /// Get monitor token from here
        /// </summary>
        protected Uri Url { get; set; }
        protected string request { get; set; }
        public string LoginErrorDescription { get; private set; }
        public string IdentityCode { get; set; }
        private Socket SocketData { get; set; }
        private Timer keepAliveTimer;
        private VoicenterRealtimeListener RealtimeListener;
        /// <summary>
        /// Monitor listener
        /// </summary>
        private List<Uri> monitorURIList = new List<Uri>();

        class Socket
        {
            public string Token { get; set; }
            public int PersonID { get; set; }
            public string RefreshToken { get; set; }
            public string Url { get; set; }
            public List<string> URLList { get; set; }
            public DateTime TokenExpiry { get; set; }
        }
        class LoginData
        {
            public Socket Socket { get; set; }
        }
        class MonitorDetailsResult
        {
            public HttpStatusCode StatusCode { get; set; }
            public string Status { get; set; }
            public LoginData Data { get; set; }
            public string URL { get; set; }
            public List<string> URLList { get; set; }
        }
        public LoginBase SetMonitorList(List<string> urlList)
        {
            if (!(urlList is null))
            {
                foreach (var uri in urlList)
                {
                    monitorURIList.Add(new Uri(uri));
                }
                monitorURIList = monitorURIList.Select(_ => _).ToList();
                
            }
            return this;

        }
        public VoicenterRealtimeListener Init()
        {
            if ((monitorURIList.Count > 0) && (!string.IsNullOrEmpty(this.IdentityCode))) return this._Init();
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers["System"] = Assembly.GetCallingAssembly().GetName().Name;
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    byte[] response = client.UploadData(this.Url, Encoding.UTF8.GetBytes(this.request));
                    var monitorData = new JavaScriptSerializer().Deserialize<MonitorDetailsResult>(Encoding.UTF8.GetString(response));
                    this.LoginErrorDescription = monitorData.Status;
                    SocketData = monitorData.Data.Socket;
                    this.IdentityCode = SocketData.Token;



                    if (SocketData.URLList != null && SocketData.URLList.Count != 0 && (monitorURIList.Count == 0))
                    {
                        monitorURIList = SocketData.URLList.Select(xa => new Uri(xa)).ToList();
                    }
                    return this._Init();
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = (HttpWebResponse)e.Response;
                    throw new Exception(LoginError.WrongLoginDetails.ToString());
                }
                throw e;
            }

            
        }

        private VoicenterRealtimeListener _Init(string token = "")
        {
            if (!(string.IsNullOrEmpty(token))) this.IdentityCode = token;
            if (typeof(Object).IsInstanceOfType(SocketData) && !string.IsNullOrEmpty(SocketData.RefreshToken)) _InitRefreshToken();

            RealtimeListener =  new VoicenterRealtimeListener(this.monitorURIList,this.IdentityCode);

            return RealtimeListener;
        }
        public void RefreshToken()
        {
            if(!(string.IsNullOrEmpty(this.SocketData.RefreshToken)))
            {
                Uri RefreshTokenUrl = new Uri(Properties.Settings.Default.RefreshTokenUrl);

                try
                {

                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(RefreshTokenUrl);
                    request.Method = "GET";
                    request.Headers["Authorization"] = "Bearer " + this.SocketData.RefreshToken;
                    request.Headers["System"] = Assembly.GetCallingAssembly().GetName().Name;
                    String data = String.Empty;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        Stream dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        data = reader.ReadToEnd();
                        reader.Close();
                        dataStream.Close();

                        var monitorData = new JavaScriptSerializer().Deserialize<MonitorDetailsResult>(data);
                        this.LoginErrorDescription = monitorData.Status;
                        SocketData = monitorData.Data.Socket;
                        this.IdentityCode = SocketData.Token;


                        if (SocketData.URLList != null && SocketData.URLList.Count != 0 && (monitorURIList.Count == 0))
                        {
                            monitorURIList = SocketData.URLList.Select(xa => new Uri(xa)).ToList();
                        }
                        RealtimeListener.RefreshToken(IdentityCode, monitorURIList);

                        if (!string.IsNullOrEmpty(SocketData.RefreshToken)) _InitRefreshToken();

                    }
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse response = (HttpWebResponse)e.Response;
                        throw new Exception(LoginError.WrongLoginDetails.ToString());
                    }
                    throw e;
                }

            }

        }



        protected void _InitRefreshToken()
        {

            if (keepAliveTimer != null)
            {
                keepAliveTimer.Stop();
                keepAliveTimer.Elapsed -= OnTimedEvent;
                keepAliveTimer = null;
            }
            var timer = Math.Max(2000, (SocketData.TokenExpiry.Subtract(DateTime.Now).TotalMilliseconds) - 5000);

            keepAliveTimer = new Timer(timer);
            keepAliveTimer.Elapsed += (Object source, ElapsedEventArgs e) => RefreshToken();
            keepAliveTimer.AutoReset = false;
            keepAliveTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            
        }
    }
    
}
