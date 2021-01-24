using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoicenterRealtimeAPI.Login;

namespace VoicenterRealtimeAPI
{
    public class VoicenterRealtime 
    {
        private LogLevel logLevel = LogLevel.Info;
        public VoicenterRealtime(LogLevel logLevel = LogLevel.Info)
        {
            this.logLevel = logLevel;
            Logger.logLevel = this.logLevel;
        }
        public Account Account(string Username, string Password)
        {
            return new Account(Username, Password);
        }
        public User User(string Email, string Password)
        {
            return new User(Email, Password);
        }
        public Token Token(string Token)
        {
            return new Token(Token);
        }

    }
}
