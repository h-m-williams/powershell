﻿using Microsoft.SharePoint.Client;
using PnP.Framework;
using PnP.PowerShell.Commands.Attributes;
using PnP.PowerShell.Commands.Base.PipeBinds;
using PnP.PowerShell.Commands.Provider;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Security;
using File = System.IO.File;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Tokens.Jwt;
using PnP.PowerShell.Commands.Enums;
using PnP.PowerShell.Commands.Model;
using Resources = PnP.PowerShell.Commands.Properties.Resources;
using System.Collections.Generic;
using PnP.Framework.Utilities;
using System.Reflection;
using System.Threading;
using PnP.PowerShell.Commands.Utilities;
using System.Threading.Tasks;

namespace PnP.PowerShell.Commands.Base
{
    [Cmdlet(VerbsCommunications.Connect, "PnPOnline", DefaultParameterSetName = ParameterSet_MAIN)]
    public class ConnectOnline : BasePSCmdlet
    {
        private CancellationTokenSource cancellationTokenSource;
        private const string ParameterSet_MAIN = "Main";
        private const string ParameterSet_TOKEN = "Token";
        private const string ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL = "App-Only using a clientId and clientSecret and an URL";
        private const string ParameterSet_APPONLYCLIENTIDCLIENTSECRETAADDOMAIN = "App-Only using a clientId and clientSecret and an AAD Domain";
        private const string ParameterSet_ADFSCERT = "ADFS with client Certificate";
        private const string ParameterSet_ADFSCREDENTIALS = "ADFS with user credentials";
        // private const string ParameterSet_NATIVEAAD = "Azure Active Directory";
        private const string ParameterSet_APPONLYAAD = "App-Only with Azure Active Directory";
        private const string ParameterSet_APPONLYAADPEM = "App-Only with Azure Active Directory using certificate as PEM strings";
        private const string ParameterSet_APPONLYAADCER = "App-Only with Azure Active Directory using X502 certificates";
        private const string ParameterSet_APPONLYAADThumb = "App-Only with Azure Active Directory using certificate from certificate store by thumbprint";
        private const string ParameterSet_SPOMANAGEMENT = "SPO Management Shell Credentials";
        private const string ParameterSet_DEVICELOGIN = "PnP Management Shell / DeviceLogin";
        private const string ParameterSet_GRAPHDEVICELOGIN = "PnP Management Shell to the Microsoft Graph";
        private const string ParameterSet_AADWITHSCOPE = "Azure Active Directory using Scopes";
        private const string ParameterSet_ACCESSTOKEN = "Access Token";
        private const string ParameterSet_WEBLOGIN = "Web Login";
        // private const string ParameterSet_GRAPHWITHAAD = "Microsoft Graph using Azure Active Directory";
        private const string SPOManagementClientId = "9bc3ab49-b65d-410a-85ad-de819febfddc";
        private const string SPOManagementRedirectUri = "https://oauth.spops.microsoft.com/";



        //private static readonly Uri GraphAADLogin = new Uri("https://login.microsoftonline.com/");
        //private static readonly string[] GraphDefaultScope = { "https://graph.microsoft.com/.default" };

        private const string ParameterSet_MANAGEDIDENTITY = "Managed Identity";

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_MAIN, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TOKEN, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETAADDOMAIN, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCERT, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCREDENTIALS, ValueFromPipeline = true)]
        // [Parameter(Mandatory = false, ParameterSetName = ParameterSet_NATIVEAAD, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAAD, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADPEM, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADThumb, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADCER, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_SPOMANAGEMENT, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ACCESSTOKEN, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_DEVICELOGIN, ValueFromPipeline = true)]
        public SwitchParameter ReturnConnection;

        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_MAIN, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_TOKEN, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_ADFSCERT, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_ADFSCREDENTIALS, ValueFromPipeline = true)]
        // [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_NATIVEAAD, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_APPONLYAAD, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_APPONLYAADPEM, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_APPONLYAADCER, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_APPONLYAADThumb, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_SPOMANAGEMENT, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSet_ACCESSTOKEN, ValueFromPipeline = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet_DEVICELOGIN, ValueFromPipeline = true)]
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSet_WEBLOGIN, ValueFromPipeline = true)]
        public string Url;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_MAIN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCREDENTIALS)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_AADWITHSCOPE)]
        public CredentialPipeBind Credentials;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_MAIN)]
        public SwitchParameter CurrentCredentials;

        //[Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCREDENTIALS)]
        //public SwitchParameter UseAdfs;

        //[Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCERT)]
        //public SwitchParameter UseAdfsCert;

        //[Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCERT)]
        //public X509Certificate2 ClientCertificate;

        //[Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCREDENTIALS)]
        //public SwitchParameter Kerberos;

        //[Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCERT)]
        //[Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCREDENTIALS)]
        //public string LoginProviderName;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TOKEN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETAADDOMAIN)]
        public string Realm;

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_TOKEN)]

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETAADDOMAIN)]
        public string ClientSecret;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_MAIN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TOKEN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETAADDOMAIN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCERT)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCREDENTIALS)]
        // [Parameter(Mandatory = false, ParameterSetName = ParameterSet_NATIVEAAD)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAAD)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADPEM)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADThumb)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADCER)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_SPOMANAGEMENT)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ACCESSTOKEN)]
        public SwitchParameter CreateDrive;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_MAIN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TOKEN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETAADDOMAIN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCERT)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCREDENTIALS)]
        // [Parameter(Mandatory = false, ParameterSetName = ParameterSet_NATIVEAAD)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAAD)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADPEM)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADThumb)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADCER)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_SPOMANAGEMENT)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ACCESSTOKEN)]
        public string DriveName = "SPO";

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_SPOMANAGEMENT)]
        public SwitchParameter SPOManagementShell;


        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_DEVICELOGIN)]
        public SwitchParameter PnPManagementShell;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_DEVICELOGIN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_GRAPHDEVICELOGIN)]
        public SwitchParameter LaunchBrowser;

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_GRAPHDEVICELOGIN)]
        public SwitchParameter Graph;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_MAIN)]
        // [Parameter(Mandatory = true, ParameterSetName = ParameterSet_NATIVEAAD)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAAD)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAADPEM)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAADThumb)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAADCER)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETAADDOMAIN)]
        public string ClientId;

        //[Parameter(Mandatory = true, ParameterSetName = ParameterSet_NATIVEAAD)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_MAIN)]
        public string RedirectUri;

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAADPEM)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAADThumb)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAAD)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAADCER)]
        public string Tenant;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAAD)]
        public string CertificatePath;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAAD)]
        public string CertificateBase64Encoded;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAAD)]
        public System.Security.Cryptography.X509Certificates.X509Certificate2 Certificate;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAAD)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADPEM)]
        public SecureString CertificatePassword;

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAADPEM)]
        public string PEMCertificate;

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAADPEM)]
        public string PEMPrivateKey;

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYAADThumb)]
        public string Thumbprint;

        // [Parameter(Mandatory = false, ParameterSetName = ParameterSet_NATIVEAAD)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAAD)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADPEM)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADThumb)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADCER)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TOKEN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_AADWITHSCOPE)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_DEVICELOGIN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_GRAPHDEVICELOGIN)]
        public AzureEnvironment AzureEnvironment = AzureEnvironment.Production;

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_AADWITHSCOPE)]
        public string[] Scopes;

        //[Parameter(Mandatory = true, ParameterSetName = ParameterSet_GRAPHWITHAAD)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETAADDOMAIN)]
        public string AADDomain;

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_ACCESSTOKEN)]
        public string AccessToken;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_MAIN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_TOKEN)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCERT)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_ADFSCREDENTIALS)]
        // [Parameter(Mandatory = false, ParameterSetName = ParameterSet_NATIVEAAD)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAAD)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADPEM)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADThumb)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_APPONLYAADCER)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_SPOMANAGEMENT)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_WEBLOGIN)]
        public string TenantAdminUrl;

        [Parameter(Mandatory = false)]
        [Obsolete("Set the environment variable 'PNPPOWERSHELL_DISABLETELEMETRY' to 'true' instead of using this switch.")]
        public SwitchParameter NoTelemetry;

        [Parameter(Mandatory = false)]
        [Obsolete("Set the environment variable 'PNPPOWERSHELL_UPDATECHECK' to 'false' instead of using this switch.")]
        public SwitchParameter NoVersionCheck;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_MANAGEDIDENTITY)]
        public SwitchParameter ManagedIdentity;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_MAIN)]
        public SwitchParameter TransformationOnPrem;

        [Parameter(Mandatory = true, ParameterSetName = ParameterSet_WEBLOGIN)]
        public SwitchParameter UseWebLogin;

        [Parameter(Mandatory = false, ParameterSetName = ParameterSet_WEBLOGIN)]
        public SwitchParameter ForceAuthentication;

        protected override void ProcessRecord()
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

#pragma warning disable CS0618 // NoTelemetry and NoVersionCheck needs to be set through environment variables now
            if (NoTelemetry)
            {
                Environment.SetEnvironmentVariable("PNPPOWERSHELL_DISABLETELEMETRY", "true");
            }
            if (NoVersionCheck)
            {
                Environment.SetEnvironmentVariable("PNPPOWERSHELL_UPDATECHECK", "false");
            }
#pragma warning restore CS6018            

            VersionChecker.CheckVersion(this);
            try
            {
                Connect(ref token);
            }
            catch (Exception ex)
            {
                ex.Data["TimeStampUtc"] = DateTime.UtcNow;
                throw ex;
            }
        }

        /// <summary>
        /// Sets up the connection using the information provided through the cmdlet arguments
        /// </summary>
        protected void Connect(ref CancellationToken cancellationToken)
        {

            if (!string.IsNullOrEmpty(Url) && Url.EndsWith("/"))
            {
                Url = Url.TrimEnd('/');
            }

            PnPConnection connection = null;

            PSCredential credentials = null;
            if (Credentials != null)
            {
                credentials = Credentials.Credential;
            }

            // Connect using the used set parameters
            switch (ParameterSetName)
            {
                case ParameterSet_SPOMANAGEMENT:
                    connection = ConnectSpoManagement();
                    break;

                case ParameterSet_DEVICELOGIN:
                    connection = ConnectDeviceLogin(cancellationToken);
                    break;

                case ParameterSet_GRAPHDEVICELOGIN:
                    connection = ConnectGraphDeviceLogin(cancellationToken);
                    break;
                case ParameterSet_APPONLYAAD:
                    connection = ConnectAppOnlyAad();
                    break;

                case ParameterSet_APPONLYAADPEM:
                    connection = ConnectAppOnlyAadPem();
                    break;

                case ParameterSet_APPONLYAADThumb:
                    connection = ConnectAppOnlyAadThumb();
                    break;

                case ParameterSet_APPONLYAADCER:
                    connection = ConnectAppOnlyAadCer();
                    break;

                case ParameterSet_AADWITHSCOPE:
                    connection = ConnectAadWithScope(credentials, AzureEnvironment, cancellationToken);
                    break;
                case ParameterSet_ACCESSTOKEN:
                    connection = ConnectAccessToken();
                    break;
                case ParameterSet_TOKEN:
                    connection = ConnectToken();
                    break;

                case ParameterSet_APPONLYCLIENTIDCLIENTSECRETURL:
                    connection = ConnectAppOnlyClientIdCClientSecretUrl();
                    break;

                case ParameterSet_APPONLYCLIENTIDCLIENTSECRETAADDOMAIN:
                    connection = ConnectAppOnlyClientIdCClientSecretAadDomain();
                    break;

                case ParameterSet_MAIN:
                    connection = ConnectCredentials(credentials);
                    break;

                case ParameterSet_MANAGEDIDENTITY:
                    connection = ConnectManagedIdentity();
                    break;
                case ParameterSet_WEBLOGIN:
                    connection = ConnectWebLogin();
                    break;
            }

            // Ensure a connection instance has been created by now
            if (connection == null)
            {
                // No connection instance was created
                throw new PSInvalidOperationException("Unable to connect using provided arguments");
            }

            // Connection has been established
#if !NETFRAMEWORK
            WriteVerbose($"PnP PowerShell Cmdlets ({new SemanticVersion(Assembly.GetExecutingAssembly().GetName().Version)}): Connected to {Url}");
#else
            WriteVerbose($"PnP PowerShell Cmdlets ({Assembly.GetExecutingAssembly().GetName().Version}): Connected to {Url}");
#endif
            PnPConnection.CurrentConnection = connection;
            if (CreateDrive && PnPConnection.CurrentConnection.Context != null)
            {
                var provider = SessionState.Provider.GetAll().FirstOrDefault(p => p.Name.Equals(SPOProvider.PSProviderName, StringComparison.InvariantCultureIgnoreCase));
                if (provider != null)
                {
                    if (provider.Drives.Any(d => d.Name.Equals(DriveName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        SessionState.Drive.Remove(DriveName, true, "Global");
                    }

                    var drive = new PSDriveInfo(DriveName, provider, string.Empty, Url, null);
                    SessionState.Drive.New(drive, "Global");
                }
            }

            if (PnPConnection.CurrentConnection.Url != null)
            {
                var hostUri = new Uri(PnPConnection.CurrentConnection.Url);
                Environment.SetEnvironmentVariable("PNPPSHOST", hostUri.Host);
                Environment.SetEnvironmentVariable("PNPPSSITE", hostUri.LocalPath);
            }
            else
            {
                Environment.SetEnvironmentVariable("PNPPSHOST", "GRAPH");
                Environment.SetEnvironmentVariable("PNPPSSITE", "GRAPH");
            }
         
            if (ReturnConnection)
            {
                WriteObject(connection);
            }

        }

        #region Connect Types

        /// <summary>
        /// Connect using the paramater set TOKEN
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectToken()
        {
            return PnPConnectionHelper.InstantiateSPOnlineConnection(new Uri(Url), AADDomain, ClientId, ClientSecret, TenantAdminUrl, false, AzureEnvironment);
        }


        /// <summary>
        /// Connect using the parameter set APPONLYCLIENTIDCLIENTSECRETURL
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectAppOnlyClientIdCClientSecretUrl()
        {
            return PnPConnectionHelper.InstantiateSPOnlineConnection(new Uri(Url), AADDomain, ClientId, ClientSecret, TenantAdminUrl, false, AzureEnvironment);
        }

        /// <summary>
        /// Connect using the parameter set APPONLYCLIENTIDCLIENTSECRETAADDOMAIN
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectAppOnlyClientIdCClientSecretAadDomain()
        {
            return PnPConnection.GetConnectionWithClientIdAndClientSecret(ClientId, ClientSecret, InitializationType.ClientIDSecret, Url, AADDomain);
        }

        /// <summary>
        /// Connect using the parameter set SPOMANAGEMENT
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectSpoManagement()
        {
            ClientId = SPOManagementClientId;
            RedirectUri = SPOManagementRedirectUri;

            return ConnectCredentials(Credentials?.Credential, InitializationType.SPOManagementShell);
        }

        /// <summary>
        /// Connect using the parameter set DEVICELOGIN
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectDeviceLogin(CancellationToken cancellationToken)
        {
            var messageWriter = new CmdletMessageWriter(this);
            PnPConnection connection = null;
            var uri = new Uri(Url);
            if ($"https://{uri.Host}".Equals(Url.ToLower()))
            {
                Url += "/";
            }
            var task = Task.Factory.StartNew(() =>
            {
                var returnedConnection = PnPConnectionHelper.InstantiateDeviceLoginConnection(Url, LaunchBrowser, TenantAdminUrl, messageWriter, AzureEnvironment, cancellationToken);
                connection = returnedConnection;
                messageWriter.Finished = true;
            }, cancellationToken);
            messageWriter.Start();
            return connection;
        }

        /// <summary>
        /// Connect using the parameter set GRAPHDEVICELOGIN
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectGraphDeviceLogin(CancellationToken cancellationToken)
        {
            PnPConnection connection = null;
            var messageWriter = new CmdletMessageWriter(this);
            Task.Factory.StartNew(() =>
            {
                connection = PnPConnectionHelper.InstantiateGraphDeviceLoginConnection(LaunchBrowser, this, messageWriter, AzureEnvironment, cancellationToken);
                messageWriter.Stop();
            });
            messageWriter.Start();
            return connection;
        }

        /// <summary>
        /// Connect using the parameter set APPONLYAAD
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectAppOnlyAad()
        {
            if (ParameterSpecified(nameof(CertificatePath)))
            {
                if (!Path.IsPathRooted(CertificatePath))
                {
                    CertificatePath = System.IO.Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path,
                               CertificatePath);
                }
                //WriteWarning(@"Your certificate is copied by the operating system to c:\ProgramData\Microsoft\Crypto\RSA\MachineKeys. Over time this folder may increase heavily in size. Use Disconnect-PnPOnline in your scripts remove the certificate from this folder to clean up. Consider using -Thumbprint instead of -CertificatePath.");
                return PnPConnectionHelper.InstantiateConnectionWithCertPath(new Uri(Url), ClientId, Tenant, CertificatePath, CertificatePassword, TenantAdminUrl, AzureEnvironment);
            }
            else if (ParameterSpecified(nameof(Certificate)))
            {
                return PnPConnectionHelper.InstantiateConnectionWithCert(new Uri(Url), ClientId, Tenant, TenantAdminUrl, AzureEnvironment, Certificate);
            }
            else if (ParameterSpecified(nameof(CertificateBase64Encoded)))
            {
                return PnPConnectionHelper.InstantiateConnectionWithCert(new Uri(Url), ClientId, Tenant, TenantAdminUrl, AzureEnvironment, CertificateBase64Encoded);
            }
            else
            {
                throw new ArgumentException("You must either provide CertificatePath, Certificate or CertificateBase64Encoded when connecting using an Azure Active Directory registered application");
            }
        }

        /// <summary>
        /// Connect using the parameter set APPONLYAADPEM
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectAppOnlyAadPem()
        {
            //return PnPConnectionHelper.InitiateAzureADAppOnlyConnection(new Uri(Url), ClientId, Tenant, PEMCertificate, PEMPrivateKey, CertificatePassword, TenantAdminUrl, Host, NoTelemetry, AzureEnvironment);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Connect using the parameter set APPONLYAADThumb
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectAppOnlyAadThumb()
        {
            return PnPConnectionHelper.InstantiateConnectionWithCertThumbprint(new Uri(Url), ClientId, Tenant, Thumbprint, TenantAdminUrl, AzureEnvironment);
        }

        /// <summary>
        /// Connect using the parameter set APPONLYAADCER
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectAppOnlyAadCer()
        {
            //return PnPConnectionHelper.InitiateAzureADAppOnlyConnection(new Uri(Url), ClientId, Tenant, Certificate, TenantAdminUrl, Host, NoTelemetry, AzureEnvironment);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Connect using the parameter set AADWITHSCOPE
        /// </summary>
        /// <param name="credentials">Credentials to authenticate with for delegated access or NULL for application permissions</param>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectAadWithScope(PSCredential credentials, AzureEnvironment azureEnvironment, CancellationToken cancellationToken)
        {
            var messageWriter = new CmdletMessageWriter(this);
            // Filter out the scopes for the Microsoft Office 365 Management API
            var officeManagementApiScopes = Enum.GetNames(typeof(OfficeManagementApiPermission)).Select(s => s.Replace("_", ".")).Intersect(Scopes).ToArray();

            // Take the remaining scopes and try requesting them from the Microsoft Graph API
            var graphScopes = Scopes.Except(officeManagementApiScopes).ToArray();

            PnPConnection connection = null;

            Task.Factory.StartNew(() =>
            {
                // If we have Office 365 scopes, get a token for those first
                if (officeManagementApiScopes.Length > 0)
                {
                    var officeManagementApiToken = credentials == null ?
                        OfficeManagementApiToken.AcquireApplicationTokenDeviceLoginAsync(PnPConnection.PnPManagementShellClientId, officeManagementApiScopes, PnPConnection.DeviceLoginCallback(messageWriter, true), azureEnvironment, cancellationToken).GetAwaiter().GetResult() :
                        OfficeManagementApiToken.AcquireDelegatedTokenWithCredentialsAsync(PnPConnection.PnPManagementShellClientId, officeManagementApiScopes, credentials.UserName, credentials.Password).GetAwaiter().GetResult();
                    connection = PnPConnection.GetConnectionWithToken(officeManagementApiToken, TokenAudience.OfficeManagementApi, InitializationType.DeviceLogin, credentials);
                }

                // If we have Graph scopes, get a token for it
                if (graphScopes.Length > 0)
                {
                    var graphToken = credentials == null ? GraphToken.AcquireApplicationTokenDeviceLoginAsync(PnPConnection.PnPManagementShellClientId, graphScopes, PnPConnection.DeviceLoginCallback(messageWriter, true), azureEnvironment, cancellationToken).GetAwaiter().GetResult() : GraphToken.AcquireDelegatedTokenWithCredentialsAsync(PnPConnection.PnPManagementShellClientId, graphScopes, credentials.UserName, credentials.Password, AzureEnvironment).GetAwaiter().GetResult();
                    // If there's a connection already, add the AAD token to it, otherwise set up a new connection with it
                    if (connection != null)
                    {
                        //connection.AddToken(TokenAudience.MicrosoftGraph, graphToken);
                    }
                    else
                    {
                        connection = PnPConnection.GetConnectionWithToken(graphToken, TokenAudience.MicrosoftGraph, InitializationType.GraphDeviceLogin, credentials);
                    }
                }
                messageWriter.Stop();
            }, cancellationToken);
            messageWriter.Start();
            connection.Scopes = Scopes;
            return connection;
        }

        /// <summary>
        /// Connect using the parameter set ACCESSTOKEN
        /// </summary>
        /// <returns>PnPConnection based on the parameters provided in the parameter set</returns>
        private PnPConnection ConnectAccessToken()
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(AccessToken);
            var aud = jwtToken.Audiences.FirstOrDefault();
            var url = Url ?? aud ?? throw new PSArgumentException(Resources.AccessTokenConnectFailed);

            switch (url.ToLower())
            {
                case GraphToken.ResourceIdentifier:
                    return PnPConnection.GetConnectionWithToken(new GraphToken(AccessToken), TokenAudience.MicrosoftGraph, InitializationType.Token, null);

                case OfficeManagementApiToken.ResourceIdentifier:
                    return PnPConnection.GetConnectionWithToken(new OfficeManagementApiToken(AccessToken), TokenAudience.OfficeManagementApi, InitializationType.Token, null);

                default:
                    {
                        ClientContext clientContext = null;
                        if (ParameterSpecified(nameof(Url)))
                        {
                            clientContext = new ClientContext(Url);
                        }
                        return PnPConnection.GetConnectionWithToken(new SharePointToken(AccessToken), TokenAudience.SharePointOnline, InitializationType.Token, null, Url, clientContext: clientContext);
                    }
            }
        }

        /// <summary>
        /// Connect using provided credentials or the current credentials
        /// </summary>
        /// <returns>PnPConnection based on credentials authentication</returns>
        private PnPConnection ConnectCredentials(PSCredential credentials, InitializationType initializationType = InitializationType.Credentials)
        {
            if (!CurrentCredentials && credentials == null)
            {
                credentials = GetCredentials();
                if (credentials == null)
                {
                    credentials = Host.UI.PromptForCredential(Resources.EnterYourCredentials, "", "", "");

                    // Ensure credentials have been entered
                    if (credentials == null)
                    {
                        // No credentials have been provided
                        return null;
                    }
                }
            }

            return PnPConnectionHelper.InstantiateConnectionWithCredentials(new Uri(Url),
                                                               credentials,
                                                               TenantAdminUrl,
                                                               AzureEnvironment,
                                                               ClientId,
                                                               RedirectUri, TransformationOnPrem, initializationType);
        }

        private PnPConnection ConnectManagedIdentity()
        {
            WriteVerbose("Connecting to the Graph with the current Managed Identity");
            var connection = new PnPConnection(null, InitializationType.Graph);
            return connection;
        }

        private PnPConnection ConnectWebLogin()
        {
            if (Utilities.OperatingSystem.IsWindows())
            {
                WriteWarning("THIS IS AN EXPERIMENTAL FEATURE:\n\nDue to the way of authentication we are not able to retrieve on demand access tokens which are required for some cmdlets to work. None of the cmdlets that require the Microsoft Graph (Teams and Planner cmdlets) will work and some of the handlers in the PnP Provisioning Engine (Get-PnPSiteTemplate, Invoke-PnPSiteTemplate, etc.) will fail too. Consider moving to other types of authentication.");
                return PnPConnectionHelper.InstantiateWebloginConnection(new Uri(Url.ToLower()), TenantAdminUrl, ForceAuthentication);
            }
            else
            {
                throw new PSArgumentException("-UseWebLogin only works when running on Microsoft Windows due to the requirement to show a login window.");
            }
        }
        #endregion

        #region Helper methods
        private PSCredential GetCredentials()
        {
            var connectionUri = new Uri(Url);

            // Try to get the credentials by full url
            PSCredential credentials = Utilities.CredentialManager.GetCredential(Url);
            if (credentials == null)
            {
                // Try to get the credentials by splitting up the path
                var pathString = $"{connectionUri.Scheme}://{(connectionUri.IsDefaultPort ? connectionUri.Host : $"{connectionUri.Host}:{connectionUri.Port}")}";
                var path = connectionUri.AbsolutePath;
                while (path.IndexOf('/') != -1)
                {
                    path = path.Substring(0, path.LastIndexOf('/'));
                    if (!string.IsNullOrEmpty(path))
                    {
                        var pathUrl = $"{pathString}{path}";
                        credentials = Utilities.CredentialManager.GetCredential(pathUrl);
                        if (credentials != null)
                        {
                            break;
                        }
                    }
                }

                if (credentials == null)
                {
                    // Try to find the credentials by schema and hostname
                    credentials = Utilities.CredentialManager.GetCredential(connectionUri.Scheme + "://" + connectionUri.Host);

                    if (credentials == null)
                    {
                        // Maybe added with an extra slash?
                        credentials = Utilities.CredentialManager.GetCredential(connectionUri.Scheme + "://" + connectionUri.Host + "/");

                        if (credentials == null)
                        {
                            // try to find the credentials by hostname
                            credentials = Utilities.CredentialManager.GetCredential(connectionUri.Host);
                        }
                    }
                }

            }

            return credentials;
        }



        protected override void StopProcessing()
        {
            cancellationTokenSource.Cancel();
        }

        #endregion
    }
}
