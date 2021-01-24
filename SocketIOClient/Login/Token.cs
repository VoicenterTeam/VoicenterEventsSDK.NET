using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using VoicenterRealtimeAPI.Login.Base;

namespace VoicenterRealtimeAPI.Login
{
    public class Token : LoginBase
    {

        public Token(string token)
        {
            this.IdentityCode = token;

            this.request = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                token = this.IdentityCode
            });

            this.identity = Identity.Token;
            this.Url = new Uri(Properties.Settings.Default.TokenUrl);

        }
    }
}
