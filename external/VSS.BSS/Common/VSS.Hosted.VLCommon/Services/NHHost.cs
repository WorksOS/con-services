using log4net;
using Microsoft.ServiceModel.Web;
using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace VSS.Hosted.VLCommon
{
    public class NHHost<T> : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        protected ServiceHost _host = null;

        public virtual void StartService()
        {
            if (_host == null)
            {
                _host = SpringServiceHostFactory.BuildServiceHost(typeof(T));
                InitializeHost(_host);
            }
        }

        public virtual void StartService(Uri address)
        {
            if (_host == null)
            {
                _host = new ServiceHost(typeof(T), address);
                InitializeHost(_host);
            }
        }

        protected void InitializeHost(ServiceHost instantiatedHost)
        {
            _host.Open();
            _host.Faulted += new EventHandler(_host_Faulted);
            _host.UnknownMessageReceived += new EventHandler<UnknownMessageReceivedEventArgs>(_host_UnknownMessageReceived);
        }

        protected void _host_UnknownMessageReceived(object sender, UnknownMessageReceivedEventArgs e)
        {
            log.IfWarnFormat("ServiceHost<{0}> has discarded unknown message {1}.", typeof(T), e.Message.ToString());
        }

        protected void _host_Faulted(object sender, EventArgs e)
        {
            log.FatalFormat("ServiceHost<{0}> has faulted. Data has been dropped.", typeof(T));
            StopService();
            StartService();
        }

        public void StopService()
        {
            if (_host != null && _host.State == CommunicationState.Opened)
            {
                try
                {
                    _host.Close();
                }
                catch (Exception e)
                {
                    log.IfWarnFormat("An error attempting to close service host<{0}>:  {1}", GetType().FullName, e.ToString());
                }

            }
            _host = null;
        }

        public void Dispose()
        {
            StopService();
        }
    }

    public class NHWebHost<T> : NHHost<T>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        //current this only supports two URI Addresses
        public void StartHTTPService(List<Uri> addresses, Type endPointContractType, UserNamePasswordValidator validator, IList<RequestInterceptor> interceptors = null)
        {
            if (_host == null)
            {
                _host = new WebServiceHost2(typeof(T), true, addresses.ToArray());
                if (interceptors != null)
                {
                    foreach (RequestInterceptor requestInterceptor in interceptors)
                    {
                        ((WebServiceHost2)_host).Interceptors.Add(requestInterceptor);
                        log.DebugFormat("RequestInterceptor added: {0}", requestInterceptor.GetType().Name);
                    }
                }

                SetupServiceHost(addresses, endPointContractType, validator, null);

                InitializeHost(_host);
            }
        }

        public void StartHTTPService(List<Uri> addresses, Type endPointContractType, UserNamePasswordValidator validator, IAuthorizationPolicy authorizationPolicy, IServiceBehavior behavior, RequestInterceptor interceptor = null)
        {
            if (_host == null)
            {
                _host = new WebServiceHost2(typeof(T), true, addresses.ToArray());
                if (interceptor != null)
                {
                    ((WebServiceHost2)_host).Interceptors.Add(interceptor);
                }

                SetupServiceHost(addresses, endPointContractType, validator, behavior);

                AddAuthorizationPolicy(authorizationPolicy);

                InitializeHost(_host);
            }
        }

        private void AddAuthorizationPolicy(IAuthorizationPolicy authorizationPolicy)
        {
            log.InfoFormat("Setting AuthorizationPolicy of type: {0}", authorizationPolicy.GetType());
            _host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            List<IAuthorizationPolicy> authPolicies = new List<IAuthorizationPolicy>();
            authPolicies.Add(authorizationPolicy);
            _host.Authorization.ExternalAuthorizationPolicies = authPolicies.AsReadOnly();
        }

        public void StartBasicHTTPService(List<Uri> addresses, Type endPointContractType, Type serviceType, UserNamePasswordValidator validator)
        {
            if (_host == null)
            {
                _host = new ServiceHost(serviceType);

                foreach (Uri address in addresses)
                {
                    ServiceMetadataBehavior metadataBehavior = _host.Description.Behaviors.Find<ServiceMetadataBehavior>();

                    if (metadataBehavior == null)
                    {
                        metadataBehavior = new ServiceMetadataBehavior();

                        _host.Description.Behaviors.Add(metadataBehavior);
                    }
                    if (address.Scheme == "http")
                    {
                        metadataBehavior.HttpGetEnabled = true;
                        metadataBehavior.HttpGetUrl = new Uri(string.Format("{0}", address));
                    }
                    else
                    {
                        metadataBehavior.HttpsGetEnabled = true;
                        metadataBehavior.HttpsGetUrl = new Uri(string.Format("{0}", address));
                    }

                    if (address.ToString().Contains("https:"))
                    {
                        //set up https binding with basic authentication
                        BasicHttpBinding binding = new BasicHttpBinding();
                        binding.Security.Mode = BasicHttpSecurityMode.Transport;
                        //binding.TransferMode = TransferMode.Streamed;
                        //binding.MaxReceivedMessageSize = 67108864;
                        //binding.MaxBufferSize = 65536;
                        binding.CloseTimeout = TimeSpan.FromMinutes(1);
                        binding.OpenTimeout = TimeSpan.FromMinutes(1);
                        binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                        binding.SendTimeout = TimeSpan.FromMinutes(10);
                        binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                        binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
                        binding.AllowCookies = false;

                        //plug in CustomUserNamePasswordValidation see 
                        _host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
                        _host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = validator;

                        _host.AddServiceEndpoint(endPointContractType, binding, address);
                    }
                    else
                    {
                        _host.AddServiceEndpoint(endPointContractType, new BasicHttpBinding(), address);
                    }
                }
                InitializeHost(_host);
            }
        }

        private void SetupServiceHost(List<Uri> addresses, Type endPointContractType, UserNamePasswordValidator validator, IServiceBehavior behavior)
        {
            foreach (Uri address in addresses)
            {
                ////Commenting this condition to support security recommendations for US90246
                ////if (address.ToString().Contains("https:"))
                ////{
                //prevents being able to browse the wsdl
                // set the bools to true to enable this
                ServiceMetadataBehavior metadataBehavior = _host.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (metadataBehavior == null)
                    metadataBehavior = new ServiceMetadataBehavior();
                metadataBehavior.HttpGetEnabled = false;
                metadataBehavior.HttpsGetEnabled = false;
                _host.Description.Behaviors.Add(metadataBehavior);
                if (behavior != null) _host.Description.Behaviors.Add(behavior);

                //set up https binding with basic authentication
                WebHttpBinding binding = new WebHttpBinding();
                binding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
                //binding.TransferMode = TransferMode.Streamed;
                //binding.MaxReceivedMessageSize = 67108864;
                //binding.MaxBufferSize = 65536;
                binding.CloseTimeout = TimeSpan.FromMinutes(1);
                binding.OpenTimeout = TimeSpan.FromMinutes(1);
                binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                binding.SendTimeout = TimeSpan.FromMinutes(10);
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                binding.AllowCookies = false;

                //plug in CustomUserNamePasswordValidation see 
                _host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
                _host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = validator;

                _host.AddServiceEndpoint(endPointContractType, binding, string.Empty);
                //}
            }
        }

    }

}
