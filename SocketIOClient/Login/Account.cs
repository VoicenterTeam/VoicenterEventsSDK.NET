using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using VoicenterRealtimeAPI.Login.Base;

namespace VoicenterRealtimeAPI.Login
{
    public class Account : LoginBase
    {
        protected string Username { get; set; }
        protected string Password { get; set; }
        

        public Account(string Username, string Password)
            {
                this.Username = Username;
                this.Password = Password;

                this.request = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    username = this.Username,
                    password = this.Password
                });

                this.identity = Identity.Account;
                this.Url = new Uri(Properties.Settings.Default.AccountUrl);


        }

    }
}
