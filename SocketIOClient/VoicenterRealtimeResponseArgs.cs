﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoicenterRealtimeAPI
{
    public class VoicenterRealtimeResponseArgs : EventArgs
    {
        public EventTypes Name { get; set; }
        public object Data { get; set; }
        public string Id { get; set; }

        public VoicenterRealtimeResponseArgs()
        {

        }
    }
}
