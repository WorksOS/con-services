using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyApiUserProvisioning.UserManagement.Tests
{
    public abstract class TestsBase : IDisposable
    {
        private static readonly Random Random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        protected VSS.Hosted.VLCommon.User RandomUser 
        {
            get
            {
                var name = RandomString(6);
                
                var user = new VSS.Hosted.VLCommon.User
                {
                    fk_CustomerID = Random.Next(1000),
                    ID = Random.Next(1000),
                    Name = name,
                    EmailContact = name + "@" + RandomString(10) +".com",
                    FirstName = RandomString(10),
                    LastName = RandomString(10),
                    UserUID = Guid.NewGuid().ToString(),
                    Active = true
                };
                return user;
            }
        }

        protected VSS.Hosted.VLCommon.Customer RandomCustomer
        {
            get
            {
                var customer = new VSS.Hosted.VLCommon.Customer
                {
                    ID = Random.Next(1000),
                    Name = RandomString(10),
                    CustomerUID = Guid.NewGuid(),
                };
                return customer;
            }
        }

        protected VSS.Hosted.VLCommon.UserFeature RandomUserFeature
        {

            get
            {
                var userFeature = new VSS.Hosted.VLCommon.UserFeature
                {
                    ID = Random.Next(1000),
                    fk_Feature = Random.Next(9000),
                    fk_User = Random.Next(1000)
                };
                return userFeature;
            }
        }

        protected TestsBase()
        {
        }

        public void Dispose()
        {
           
        }
    }
}
