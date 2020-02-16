using System;
using System.Collections.Generic;

using Spring.Aop.Framework;


namespace LegacyApiUserProvisioning.UserManagement
{
    public static class API
    {
        public static IEmailAPI Email => email ?? (email = GetProxy<EmailAPI, IEmailAPI>());

        private static K GetProxy<T, K>()
        {
            var obj = Activator.CreateInstance<T>();
            {
                var factory = new ProxyFactory(obj);

                // add spring aspect advice here...

                return (K) factory.GetProxy();
            }
        }

        private static IEmailAPI email = null;
    }
}