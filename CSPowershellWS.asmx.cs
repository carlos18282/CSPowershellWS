using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Serialization;
using System.Windows.Documents;

namespace CSPowershellWS
{
    /// <summary>
    /// Description résumée de WebService1
    /// </summary>
    [WebService(Namespace = "http://cspowershellws.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Pour autoriser l'appel de ce service Web depuis un script à l'aide d'ASP.NET AJAX, supprimez les marques de commentaire de la ligne suivante. 
    // [System.Web.Script.Services.ScriptService]

    public class WebService1 : System.Web.Services.WebService
    {
        public HeaderSoapAccess HeaderSoapAccess;
            


        [WebMethod]
        [SoapHeader("HeaderSoapAccess")]
        public string RunPowershell(string Body)
        {
            string user;
            string password;
            bool Access;

            if (HeaderSoapAccess == null || string.IsNullOrEmpty(HeaderSoapAccess?.User) || string.IsNullOrEmpty(HeaderSoapAccess?.Password))
            {
                throw new SoapException("L'accès non autorisé", SoapException.ClientFaultCode,
                    new Exception(@"Le nom d'utilisateur et le mot de passe sont obligatoires dans l'en-tête de la demande."));
            }
            else
            {
                user = HeaderSoapAccess.User;
                password = HeaderSoapAccess.Password;
            }


            Access = HeaderSoapAccess.UserVerification(user, password);


            //Username and Password are correct
            if (Access == true)
            {
                if (Body != null)
                {
                    
                    return runPowershellRemotely(Body);
                }
                else {
                    throw new SoapException("Manque de paramètres:", SoapException.ClientFaultCode,
                       new Exception(@""));
                }
            }
            else 
            {
                throw new SoapException("L'accès non autorisé.", SoapException.ClientFaultCode,
                   new Exception(@"L'utilisateur et/ou le mot de passe ne sont pas valides."));

            }


            
        }





        private string runPowershellRemotely(string Body)
        {

            var Parameters = Body.Split('"')
                     .Select((element, index) => index % 2 == 0  // If even index
                                           ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)  // Split the item
                                           : new string[] { element })  // Keep the entire item
                     .SelectMany(element => element).ToList();

            try
            {
                string shellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
                string userName = "csdecou\\userlibreservice";
                string password = "CeP1cDe1$pN1fP2f1C74";
                SecureString securePassword = new SecureString();

                foreach (char c in password) { securePassword.AppendChar(c); }
                PSCredential credential = new PSCredential(userName, securePassword);

                WSManConnectionInfo connectionInfo = new WSManConnectionInfo(false, "10.4.234.129", 5985, "/wsman", shellUri, credential);
                connectionInfo.OperationTimeout = 40 * 60 * 1000; // 40 minutes.

                connectionInfo.OpenTimeout = 39 * 60 * 1000;

                connectionInfo.IdleTimeout = 38 * 60 * 1000;
                try
                {
                    using (Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo))
                    {

                        if (Parameters != null)
                        {
                            runspace.Open();

                            Pipeline pipeline = runspace.CreatePipeline();

                            Command scriptCommand = new Command(Parameters[0]);

                            if (Parameters.Count >= 2)
                            {
                                Collection<CommandParameter> commandParameters = new Collection<CommandParameter>();
                                for (int i = 1; i < Parameters.Count; i++)
                                {
                                    CommandParameter commandParm = new CommandParameter(null, Parameters[i]);
                                    commandParameters.Add(commandParm);
                                    scriptCommand.Parameters.Add(commandParm);
                                }
                            }



                            pipeline.Commands.Add(scriptCommand);

                            //pipeline.Commands.Add("Out-String");

                            Collection<PSObject> results = pipeline.Invoke();

                            StringBuilder stringBuilder = new StringBuilder();

                            foreach (PSObject obj in results) { stringBuilder.AppendLine(obj.ToString()); }

                            runspace.Close();

                            return stringBuilder.ToString();

                        }
                        else
                        {
                            throw new SoapException("Fichier Powershell et paramètres manquants", SoapException.ClientFaultCode,
                            new Exception(@"Le chemin du fichier PowerShell n'est pas valide"));

                        }


                    } //using (Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo))
                }
                catch (Exception ex) { return ex.ToString(); }
            }
            catch (Exception e) { return e.ToString(); }

        } // End  private string runPowershellRemotely




    }
}
