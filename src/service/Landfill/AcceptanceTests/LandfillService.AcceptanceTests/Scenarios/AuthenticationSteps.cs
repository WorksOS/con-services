using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using System.Text;
using System.Threading.Tasks;
using LandfillService.AcceptanceTests.Auth;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class AuthenticationSteps
    {
        [Given("I am logged in with good credentials")]
        public void GivenIAmLoggedInWithGoodCredentials()
        {
            Assert.IsFalse(string.IsNullOrEmpty(TPaaS.BearerToken), "Unable to get token!");
        }
    }
}
