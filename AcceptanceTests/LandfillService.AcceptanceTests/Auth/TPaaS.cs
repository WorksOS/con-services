using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationCore.API.Framework.Common.Features.TPaaS;

namespace LandfillService.AcceptanceTests.Auth
{
    public static class TPaaS
    {
        private static string bearerToken;

        public static string BearerToken
        {
            get
            {
                if (string.IsNullOrEmpty(bearerToken))
                {
                    bearerToken = TokenService.GetAccessToken();
                }
                return bearerToken;
            }
        }
    }
}
