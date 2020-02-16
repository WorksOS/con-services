using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using LegacyApiUserProvisioning.CustomerData;
using LegacyApiUserProvisioning.CustomerData.Interfaces;
using LegacyApiUserProvisioning.UserManagement;
using LegacyApiUserProvisioning.UserManagement.Interfaces;
using VSS.Hosted.VLCommon;

namespace LegacyApiUserProvisioning.WebApi
{
    public class IocContainer
    {
        private static IContainer Container { get; set; }

        public static void RegisterItems()
        {

            var builder = new ContainerBuilder();
            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerRequest();
            builder.RegisterType<UserManager>().As<IUserManager>().InstancePerRequest();
            

            builder.Register(c => ObjectContextFactory.NewNHContext<INH_OP>()).As<INH_OP>().InstancePerDependency();

            builder.Register(c=>LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)).As<ILog>().InstancePerDependency();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).Where(
                t => !t.IsAbstract && typeof(ApiController)
                         .IsAssignableFrom(t)).InstancePerMatchingLifetimeScope().InstancePerRequest();

            Container = builder.Build();

            // Set the dependency resolver for Web API.
            var webApiResolver = new AutofacWebApiDependencyResolver(Container);
            GlobalConfiguration.Configuration.DependencyResolver = webApiResolver;
        }
    }
}