﻿using System;
using VoicenterRealtimeAPI;
using Newtonsoft.Json.Linq;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            
            VoicenterRealtime voicenterRealtime = new VoicenterRealtime();
            //Logger
            Logger.onLog += (object sender, VoicenterRealtimeLogger e) =>
            {
                Console.WriteLine(e.message);
            };
            //

            //Listener
            VoicenterRealtimeListener socket = voicenterRealtime.Token("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx").Init();
            socket.OnEvent += OnEventHandler;
            socket.Listen();

            Console.ReadLine();
        }
        private static void OnEventHandler(object sender, VoicenterRealtimeResponseArgs e)
        {
            var voicenterRealtimeListener = (sender as VoicenterRealtimeListener);
            switch (e.Name)
            {
                // Connected to monitor
                case EventTypes.CONNECTED:
                    Console.WriteLine(e.Name + " to " + voicenterRealtimeListener?.SocketServerUri?.ToString());
                    //Console.WriteLine(e.Data);
                    Console.WriteLine("---------------------------");
                    break;
                // Disconnected from monitor
                case EventTypes.DISCONNECTED:
                    Console.WriteLine(e.Name + " to " + voicenterRealtimeListener?.SocketServerUri?.ToString());
                    break;
                case EventTypes.RECONNECTING:
                    Console.WriteLine(e.Name + " to " + voicenterRealtimeListener?.SocketServerUri?.ToString());
                    break;
                // Failed connecting to monitor
                case EventTypes.CONNECT_ERROR:
                    break;
                // When first connected, received current status of all queues
                case EventTypes.loginSuccess:
                    var z = ((JObject)e.Data).ToObject(typeof(AllQueueEvents));

                    Console.WriteLine(e.Name);
                    Console.WriteLine(e.Data);
                    Console.WriteLine("---------------------------");
                    break;
                case EventTypes.loginStatus:
                    var a = ((JObject)e.Data).ToObject(typeof(AllQueueEvents));

                    Console.WriteLine(e.Name);
                    Console.WriteLine(e.Data);
                    Console.WriteLine("---------------------------");
                    break;
                // When first connected, received current status of all extensions
                case EventTypes.AllExtensionsStatus:
                    var b = ((JObject)e.Data).ToObject(typeof(AllExtensionEvents));
                    Console.WriteLine(e.Name);
                    Console.WriteLine(e.Data);
                    Console.WriteLine("---------------------------");
                    break;
                // An update received (for example: new call, or hangup)
                case EventTypes.ExtensionEvent:
                    var c = ((JObject)e.Data).ToObject(typeof(ExtensionEvent));
                    Console.WriteLine(e.Name);
                    Console.WriteLine(e.Data);
                    Console.WriteLine("---------------------------");
                    break;
                // An update received (for example: new call in queue, or call exited queue)
                case EventTypes.QueueEvent:
                    var d = ((JObject)e.Data).ToObject(typeof(QueueEvent));
                    Console.WriteLine(e.Name);
                    Console.WriteLine(e.Data);
                    Console.WriteLine("---------------------------");
                    break;
                case EventTypes.ExtensionsUpdated:
                    Console.WriteLine(e.Name);
                    //Console.WriteLine(e.Data);
                    Console.WriteLine("---------------------------");
                    break;
                case EventTypes.QueuesUpdated:
                    Console.WriteLine(e.Name);
                    //Console.WriteLine(e.Data);
                    Console.WriteLine("---------------------------");
                    break;
                case EventTypes.keepaliveResponse:
                    Console.WriteLine(e.Name);
                    Console.WriteLine(e.Data);
                    Console.WriteLine("---------------------------");
                    break;
                case EventTypes.TokenRefreshed:
                    Console.WriteLine(e.Name);
                    Console.WriteLine(e.Data);
                    Console.WriteLine("---------------------------");
                    break;


            }

        }


        /// <summary>
        /// Received when first connecting, receive the current status of all relevant extensions based on
        /// login authorizaation
        /// </summary>
        class AllExtensionEvents
        {
            public ExtensionObject[] extensions;
            /// <summary>
            /// server epoch time in seconds
            /// </summary>
            public int servertime;
        }

        /// <summary>
        /// Received on extension change (for example: new call, user online/offline)
        /// </summary>
        class ExtensionEvent
        {
            /// <summary>
            /// Reason for receiving updated extension info
            /// Options: ANSWER / HANGUP/ NEWCALL / userStatusUpdate
            /// </summary>
            public string reason;
            /// <summary>
            /// Extension data
            /// </summary>
            public ExtensionObject data;
            /// <summary>
            /// server epoch time in seconds
            /// </summary>
            public int servertime;
        }

        /// <summary>
        /// Extension data
        /// </summary>
        class ExtensionObject
        {
            /// <summary>
            /// Current extension calls
            /// </summary>
            public ExtensionCall[] calls;
            /// <summary>
            /// Extension's user ID
            /// </summary>
            public int userID;
            /// <summary>
            /// Full user name 
            /// </summary>
            public string userName;
            /// <summary>
            /// Unique extension identifier
            /// </summary>
            public string extenUser;
            /// <summary>
            /// 1 = online 
            /// 2 = offline
            /// 3,5,7,9,11,12,13 = break types;
            /// </summary>
            public int representativeStatus;
        }

        /// <summary>
        /// call object for an extension
        /// </summary>
        class ExtensionCall
        {
            /// <summary>
            /// In case of a c2c call
            /// 1 = first call, usually to representative
            /// 2 = second call
            /// </summary>
            public string c2cdirection;
            /// <summary>
            /// Call start time, epoch time in seconds
            /// </summary>
            public int callStarted;
            public string callername;
            public string callerphone;
            /// <summary>
            /// status of call:
            /// Talking = active call
            /// Ringing 
            /// Dialing
            /// Hold
            /// </summary>
            public string callstatus;
            /// <summary>
            /// Only relevant when custom variables are passed, for example in c2c calls
            /// </summary>
            public object customdata;
            /// <summary>
            /// Incoming / Outgoing / Click2Call
            /// </summary>
            public string direction;
            /// <summary>
            /// Unique identifier
            /// </summary>
            public string ivrid;
        }

        /// <summary>
        /// Received when first connecting, receive the current status of all relevant queues based on
        /// login authorizaation
        /// </summary>
        class AllQueueEvents
        {
            public QueueObject[] queues;
            /// <summary>
            /// Server epoch time in seconds
            /// </summary>
            public int servertime;
        }

        /// <summary>
        /// Received when a call is added or removed from a queue
        /// </summary>
        class QueueEvent
        {
            public QueueObject data;
            /// <summary>
            /// Server epoch time in seconds
            /// </summary>
            public int servertime;
        }

        /// <summary>
        /// Queue data
        /// </summary>
        class QueueObject
        {
            public int QueueID;
            public QueueCall[] Calls;
            public string QueueName;
        }

        /// <summary>
        /// call object for a queue
        /// </summary>
        class QueueCall
        {
            public string CallerID;
            public string CallerName;
            /// <summary>
            /// Unique identifier
            /// </summary>
            public string ivrid;
            /// <summary>
            /// Queue join epoch time in seconds
            /// </summary>
            public int JoinTimeStamp;
        }
    }
}
