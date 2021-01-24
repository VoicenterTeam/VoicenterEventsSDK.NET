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
    public class User : LoginBase
    {
        private string Email { get; set; }
        private string Password { get; set; }

        public User(string Email , string Password)
        {
            this.Email = Email;
            this.Password = Password;

            this.request = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                email = this.Email,
                pin = this.Password
            });

            this.identity = Identity.User;
            this.Url = new Uri(Properties.Settings.Default.UserUrl);

        }
    }
}
