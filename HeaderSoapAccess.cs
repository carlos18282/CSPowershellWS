using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSPowershellWS
{
    public class HeaderSoapAccess : System.Web.Services.Protocols.SoapHeader
    {

        public string User { get; set; }
        public string Password { get; set; }

        public bool UserVerification(string user, string password)
        {
            if (user == "pfXk6v6wPUdvF6gG" && password == "ATRwH9Xb5G9yMNTQ")
            {
                 return true;
            }
            else
            {
                // SI NO ES VÁLIDO EL USUARIO RETORNAMOS NULL.
                return false;
            }
        }

    }
}