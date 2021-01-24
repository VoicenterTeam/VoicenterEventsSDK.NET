# Voicenter Events SDK
![Image of Voicenter SDK](https://voicentercdn-voicenterltd.netdna-ssl.com/cdn/images/external/SDK_red.png)<br/>
Voicenter Events SDK aims to manage API and socket communication with Voicenter APIs and backends.
You can use the SDK to send and receive real-time data from and to voicenter servers.


### Getting Started

The Events SDK should be used to communicate with Voicenter servers in order to receive real-time data via sockets. Underneath, the events SDK uses socket.io to send and receive events.

## Table of contents

1. [Instalation](#instalation)
2. [Usage](#usage)
3. [Public methods](#use-the-public-methods-to-send-and-receive-events)
4. [Other Methods](#other-methods)
    1. [Set token](#set-token)
    2. [Resync](#resync)
    3. [Set monitor url](#set-monitor-url)
    4. [Login with user](#login-with-user)
    5. [Login with account](#login-with-account)
    6. [Login with code](#login-with-code)
    7. [Disconnect](#disconnect)
5. [Logs](#Logger-event)
8. [Events you can subscribe to](#events-you-can-subscribe-to)

## Instalation
1. Direct Download  NUGET:<br/>

    Download out nuget package:
    ```html
    https://www.nuget.org/packages/VoicenterEventsSDK.NET/1.0.1
    ```
2. paket:
    ```sh
    > paket add VoicenterEventsSDK.NET --version 1.0.1
    ```    
3. Package Manager:
    ```sh
    PM> Install-Package VoicenterEventsSDK.NET -Version 1.0.1
    ```       
    
 
## Usage 
You can initialize and use it to send and receive events to and from our servers.
- Initialize the constructor with a monitorCode
```C#
    VoicenterRealtime voicenterRealtime = new VoicenterRealtime(LogLevel.Info);
    VoicenterRealtimeListener socket = voicenterRealtime.Token("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx").Init();
```

This step is important as we have an algorithm that retrieves multiple servers and has a failover mechanism behind it. Skipping this step, will throw error(s) in the subsequent steps.

## Use the public methods to send and receive events 
- Subscribe to all events
```C#
    socket.OnEvent += (object sender, VoicenterRealtimeResponseArgs e) => { Console.WriteLine(e.Name); };
    socket.Listen();
```
- Subscribe to specific events
```C#
    socket.OnEvent += (object sender, VoicenterRealtimeResponseArgs e) => { 
    if(e.Name === EventTypes.AllExtensionsStatus)
    {
       Console.WriteLine(e.Name);
    } 
    };
```
- Emit events
```C#
   socket.UpdateExtensions("1111,2222");
```

Servers List format
```C#
       new List<String>() { "https://monitor1.voicenter.co/" }
```

##### `Resync`
Emits resync event to resync data.
```C#
  socket.Resync();
```

##### `Set monitor url`
Sets new monitor url. This will trigger an http call to get the socket servers based on the monitor url. Please not that providing an invalid url, will try to revert to the old values. In case that fails, you will have to init the SDK again.
```C#
    voicenterRealtime.Token("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx").SetMonitorList(new List<String>() { "https://monitor1.voicenter.co/" }).Init();
```

##### `Login with user`
Log in based on user credentials.
```C#
    voicenterRealtime.User("Email", "Password").Init();
```

##### `Login with account`
Log in based on user account credentials.
```C#
        voicenterRealtime.Account("Username", "Password").Init();
```

##### `Disconnect`
Forcefully Dispose the socket.
```C#
    socket.Dispose();
```



## Logger event
```C#
      Logger.onLog += (object sender, VoicenterRealtimeLogger e) =>
       {
            Console.WriteLine(e.message);
       };
```
## EventTypes you can subscribe to 
 - `CONNECTED` 
 - `DISCONNECTED`
 - `RECONNECTING` 
 - `CONNECT_ERROR`
 - `loginSuccess`
 - `loginStatus`
 - `AllExtensionsStatus`
 - `ExtensionEvent`
 - `QueueEvent` 
 - `ExtensionsUpdated`
 - `QueuesUpdated`
 - `keepaliveResponse`
 - `TokenRefreshed`
