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
        public static string BearerToken
        {
            get
            {
                return TokenService.GetAccessToken();
            }
        }
    }
}
