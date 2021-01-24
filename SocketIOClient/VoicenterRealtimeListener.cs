using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Script.Serialization;
using SocketIOClient;
using System.Threading;
using Timer = System.Timers.Timer;

namespace VoicenterRealtimeAPI
{
    
    
    public enum ListenerStatus { Connected, Disconnected, Reconnecting }



    public class VoicenterRealtimeListener:IDisposable
    {
        public event EventHandler<VoicenterRealtimeResponseArgs> OnEvent;

        /// <summary>
        /// Monitor login token
        /// </summary>
        private string Token {  get; set; }
        /// <summary>
        /// Monitor listener - depricated
        /// </summary>
        private static Uri monitorURI = new Uri(Properties.Settings.Default.monitorURI);


        /// <summary>
        /// Monitor listener
        /// </summary>
        private Queue<Uri> monitorURIList = new Queue<Uri>();
        private Queue<Uri> monitorURIListByDefault = new Queue<Uri>();
        private Uri currentMonitorURI = new Uri("https://monitor5.voicenter.co.il");

        /// <summary>
        /// Monitor socket
        /// </summary>
        private SocketIO socket;


        /// <summary>
        /// (bool)
        /// </summary>
        public bool Reconnection { get; set; }

        /// <summary>
        /// Max Retries to current monitor
        /// </summary>
        public int ReconnectionAttempts { get; set; }

        /// <summary>
        /// (in ms)
        /// </summary>
        public int ReconnectionDelay { get; set; }

        /// <summary>
        /// (in ms)
        /// </summary>
        public int KeepAliveInterval { get; set; }

        public DateTime LastKeepAliveEvent { get; private set; }


        private int currentRetries { get; set; }

        public ListenerStatus ListenerStatus { get; private set; }

        /// <summary>
        /// Current monitor
        /// </summary>
        public Uri SocketServerUri;

        public bool IsTokenNotValid;

        public VoicenterRealtimeListener()
        {

        }
        public VoicenterRealtimeListener(List<Uri> uriList = null, string token = "")
        {
            this.Token = token; 
            this.ListenerStatus = ListenerStatus.Disconnected;
            this.Reconnection = Properties.Settings.Default.Reconnection;
            this.ReconnectionAttempts = Properties.Settings.Default.ReconnectionAttempts;
            this.ReconnectionDelay = Properties.Settings.Default.ReconnectionDelay;
            this.currentRetries = 0;
            this.KeepAliveInterval = Properties.Settings.Default.KeepAliveInterval;
            this.UpdateMonitorUriList(uriList);

        }
        public void RefreshToken(string token, List<Uri> uriList)
        {
            if(!String.IsNullOrEmpty(token))
            {
                this.Token = token;
            }
            this.UpdateMonitorUriList(uriList);
            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
            res.Name = "TokenRefreshed";
            res.Data = this.Token;
            OnEvent(this, res);
            Reconnect();
        }

        public void UpdateMonitorUriList(List<Uri> uriList = null)
        {
            if (uriList != null && uriList.Count > 0)
            {
                monitorURIList = new Queue<Uri>(uriList.ToArray());

                monitorURIListByDefault = new Queue<Uri>(uriList.ToArray());
                currentMonitorURI = monitorURIList.Dequeue();
                monitorURIList.Enqueue(currentMonitorURI);
            }
        }


       

        private Uri _firstPrioritymonitor;

        public void ChangeMonitorPriority()
        {
            try
            {
                currentMonitorURI = this.monitorURIList.Dequeue();
                this.monitorURIList.Enqueue(currentMonitorURI);

            }
            catch (Exception ex)
            {
                VoicenterRealtimeLogger logger = new VoicenterRealtimeLogger();
                logger.ex = ex;
                logger.message = $"in changeMonitorPriority() method, socket {socket?.ServerUri}";
                logger.level = LogLevel.Error;
                Logger.log(this, logger);
            }
        }

        protected void _InitKeepAlive()
        {

            if (keepAliveTimer != null)
            {
                keepAliveTimer.Stop();
                keepAliveTimer.Elapsed -= OnTimedEvent;
                keepAliveTimer = null;
            }
            keepAliveTimer = new Timer(this.KeepAliveInterval);
            _firstPrioritymonitor = monitorURIListByDefault.Peek();
            keepAliveTimer.Elapsed += OnTimedEvent;
            keepAliveTimer.AutoReset = true;
            keepAliveTimer.Enabled = true;
        }


        protected DateTime firstPrioritymonitorConnected = DateTime.UtcNow;

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    var UtcNow = DateTime.UtcNow;
                    var LastKeepAliveEventMilliseconds = (int)UtcNow.Subtract(this.LastKeepAliveEvent).TotalMilliseconds;
                    var LastfirstPriorityMilliseconds = (int)UtcNow.Subtract(this.firstPrioritymonitorConnected).TotalMilliseconds;

                    if (socket.Connected && socket.ServerUri != _firstPrioritymonitor && LastfirstPriorityMilliseconds > this.KeepAliveInterval * 6)
                    {
                        monitorURIList = this.monitorURIListByDefault;
                        this.currentMonitorURI = _firstPrioritymonitor;
                        this.firstPrioritymonitorConnected = DateTime.UtcNow;
                        this.Reconnect();
                    }
                    else if(socket.Connected && LastKeepAliveEventMilliseconds > this.KeepAliveInterval * 2) {
                        
                        
                        this.ChangeMonitorPriority();
                        this.Reconnect();
                        

                    }else
                    {
                        await socket.EmitAsync("keepalive");
                    }

                }
                catch (Exception ex)
                {
                    VoicenterRealtimeLogger logger = new VoicenterRealtimeLogger();
                    logger.ex = ex;
                    logger.message = $"in OnTimedEvent() method, socket {socket?.ServerUri}";
                    logger.level = LogLevel.Error;
                    Logger.log(this, logger);
                }
            });

        }

        public void Listen()
        {
            

            try
            {
                var options = new SocketIOOptions();

                Dictionary<String, String> query = new Dictionary<String, String>();
                query.Add("token", this.Token);
                options.Query = query;


                options.Reconnection = false;
                
                if (monitorURIList.Count == 0)
                    monitorURIList.Enqueue(VoicenterRealtimeListener.monitorURI);
                socket = new SocketIO(this.currentMonitorURI, options);
                //await socket.ConnectAsync();
                socket.OnConnected += Socket_OnConnected;
                socket.OnDisconnected += Socket_OnDisconnected;


                socket.On("loginStatus", async (data) =>
                {
                    await Task.Run(() =>
                    {
                        if (this.OnEvent != null)
                        {
                            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
                            res.Name = "loginStatus";
                            res.Data = data.GetValue<Object>();
                            OnEvent(this, res);
                        }
                    });
                });
                
                socket.On("AllExtensionsStatus", async (data) =>
                {
                    await Task.Run(() =>
                    {
                        if (this.OnEvent != null)
                        {
                            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
                            res.Name = "AllExtensionsStatus";
                            res.Data = data.GetValue<Object>();
                            OnEvent(this, res);
                        }
                    });
                });

                socket.On("ExtensionEvent", async (data) =>
                {
                    await Task.Run(() =>
                    {

                        if (this.OnEvent != null)
                        {
                            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
                            res.Name = "ExtensionEvent";
                            res.Data = data.GetValue<Object>();
                            OnEvent(this, res);
                        }
                    });
                });

                socket.On("loginSuccess", async (data) =>
                {
                    await Task.Run(() =>
                    {

                        if (this.OnEvent != null)
                        {
                            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
                            res.Name = "loginSuccess";
                            res.Data = data.GetValue<Object>();
                            OnEvent(this, res);
                        }
                    });
                });

                socket.On("QueueEvent", async (data) =>
                {
                    await Task.Run(() =>
                    {
                        if (this.OnEvent != null)
                        {
                            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
                            res.Name = "QueueEvent";
                            res.Data = data.GetValue<Object>();
                            OnEvent(this, res);
                        }
                    });
                });


                socket.On("ExtensionsUpdated", async (data) =>
                {
                    await Task.Run(() =>
                    {

                        if (this.OnEvent != null)
                        {
                            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
                            res.Name = "ExtensionsUpdated";
                            res.Data = data.GetValue<Object>();
                            this.Resync(false);
                            OnEvent(this, res);
                        }
                    });
                });

                socket.On("QueuesUpdated", async (data) =>
                {
                    await Task.Run(() =>
                    {
                        if (this.OnEvent != null)
                        {
                            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
                            res.Name = "QueuesUpdated";
                            res.Data = data.GetValue<Object>();
                            this.Resync(false);
                            OnEvent(this, res);
                        }
                    });
                });

                socket.On("keepaliveResponse", async (data) =>
                {
                    await Task.Run(() =>
                    {
                        if (this.OnEvent != null)
                        {
                            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
                            res.Name = "keepaliveResponse";
                            this.LastKeepAliveEvent = DateTime.UtcNow;
                            res.Data = data.GetValue<Object>();
                            OnEvent(this, res);
                        }
                    });
                });

                this.Connect();
            }
            catch (Exception ex)
            {
                VoicenterRealtimeLogger logger = new VoicenterRealtimeLogger();
                logger.ex = ex;
                logger.message = $"in Listen() method, socket {socket?.ServerUri}";
                logger.level = LogLevel.Error;
                Logger.log(this, logger);

                IsTokenNotValid = true;

            }
        }

        private async void Socket_OnConnected(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (OnEvent != null)
                    {
                        VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
                        res.Name = "CONNECTED";
                        res.Id = socket.Id;
                        SocketServerUri = socket.ServerUri;
                        this.LastKeepAliveEvent = DateTime.UtcNow;
                        this._InitKeepAlive();
                        this.ListenerStatus = ListenerStatus.Connected;
                        OnEvent(this, res);

                    }
                });
            }
            catch (Exception ex) {

                VoicenterRealtimeLogger logger = new VoicenterRealtimeLogger();
                logger.ex = ex;
                logger.message = $"in Socket_OnConnected() method, socket {socket?.ServerUri}";
                logger.level = LogLevel.Error;
                Logger.log(this, logger);
            }
        }
        private async void Socket_OnDisconnected(dynamic sender, string e)
        {
            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
            try
            {
                await Task.Run(async () =>
                {
                    this.ListenerStatus = ListenerStatus.Disconnected;
                    if (this.currentRetries < this.ReconnectionAttempts)
                    {
                        this.currentRetries++;

                    }else
                    {
                        this.currentRetries = 0;
                        this.ChangeMonitorPriority();
                        
                        
                    }
                    res.Name = "DISCONNECTED";

                    SocketServerUri = socket.ServerUri;
                    OnEvent(this, res);
                    this.Reconnect();
                });
            }
            catch (Exception ex)
            {

                VoicenterRealtimeLogger logger = new VoicenterRealtimeLogger();
                logger.ex = ex;
                logger.message = $"in Socket_OnDisconnected() method, socket {socket?.ServerUri}";
                logger.level = LogLevel.Error;
                Logger.log(this, logger);

            }
        }

        public async void Reconnect()
        {
            VoicenterRealtimeResponseArgs res = new VoicenterRealtimeResponseArgs();
            if (this.Reconnection)
            {
                res.Name = "RECONNECTING";
                this.ListenerStatus = ListenerStatus.Reconnecting;
                await this.StopListenning();
                SocketServerUri = this.currentMonitorURI;
                OnEvent(this, res);
                await Task.Delay(this.ReconnectionDelay);
                this.Listen();
            }
        }

        public Int16 UpdateExtensions(string commaSeparatedList)
        {
            var obj = new JObject();
            obj["extensionsString"] = commaSeparatedList;
            if (socket != null)
            {
                socket.EmitAsync("updateMonitoredExtensions", obj);
                return 1;
            }
            return 0;
        }

        public bool Resync(bool cache)
        {
            if (socket != null)
            {
                socket.EmitAsync("resync", new { cache = cache});
                return true;
            }
            return false;
        }

        protected Timer keepAliveTimer;

        public async Task StopListenning()
        {
            try
            {
                if (socket != null)
                {
                    this.ListenerStatus = ListenerStatus.Disconnected;
                    await socket.DisconnectAsync();
                }
                
            }
            catch (Exception ex)
            {

                VoicenterRealtimeLogger logger = new VoicenterRealtimeLogger();
                logger.ex = ex;
                logger.message = $" in StopListenning() method, socket {socket?.ServerUri}";
                logger.level = LogLevel.Error;
                Logger.log(this, logger);
            }
        }



        public async void Connect()
        {


            try
            {
                await Task.Run(async () =>
                {
                    var task = socket.ConnectAsync();
                    if (await Task.WhenAny(task, Task.Delay(this.ReconnectionDelay)) == task)
                    {
                        if(task.IsFaulted)
                        {
                            throw task.Exception;
                        }
                    }
                    else
                    {
                        ChangeMonitorPriority();
                        this.Reconnect();
                    }
                });
            }
            catch (Exception ex)
            {
                ChangeMonitorPriority();
                this.Reconnect();

                VoicenterRealtimeLogger logger = new VoicenterRealtimeLogger();
                logger.ex = ex;
                logger.message = $" in Connect() method, socket {socket?.ServerUri}";
                logger.level = LogLevel.Error;
                Logger.log(this, logger);
            }
        }



        public  void Dispose()
        {
            try
            {
                if (keepAliveTimer != null)
                {
                    keepAliveTimer.Elapsed -= OnTimedEvent;
                    keepAliveTimer = null;
                }
                //await socket.DisconnectAsync();
                if (socket != null)
                {
                    monitorURIList.Clear();
                    OnEvent = null;
                    socket.OnConnected -= Socket_OnConnected;
                    socket.OnDisconnected -= Socket_OnDisconnected;
                    socket = null;

                }
            }
            catch (Exception ex)
            {
                VoicenterRealtimeLogger logger = new VoicenterRealtimeLogger();
                logger.ex = ex;
                logger.message = $" in Dispose() method, socket {socket?.ServerUri}";
                logger.level = LogLevel.Error;
                Logger.log(this, logger);
            }
        }
    }
}
