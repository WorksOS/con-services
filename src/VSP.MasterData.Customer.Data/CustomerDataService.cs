using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using VSP.MasterData.Customer.Data.Models;
using System;

namespace VSP.MasterData.Customer.Data
{
    public class CustomerDataService : ICustomerDataService
    {
        private readonly string _connectionString;

        public CustomerDataService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MySql.Customer"].ConnectionString;
        }

        public void CreateCustomer(CreateCustomerEvent createCustomerEvent)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute("Insert into Customer(CustomerUID,CustomerName,fk_CustomerTypeID,LastCustomerUTC) values(@CustomerUid,@Name,@fk_CustTypeId,@lastCustomerUTC);", new
                            {
                                CustomerUid = createCustomerEvent.CustomerUID,
                                Name = createCustomerEvent.CustomerName,
                                fk_CustTypeId = createCustomerEvent.CustomerType,
                                lastCustomerUTC = DateTime.UtcNow
                            },
                            commandType: CommandType.Text);
            }
        }

        public int UpdateCustomer(UpdateCustomerEvent updateCustomerEvent)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Execute("Update Customer set CustomerName =@Name , LastCustomerUTC = @lastCustomerUTC where CustomerUID=@CustomerUid;", new
                            {
                                Name = updateCustomerEvent.CustomerName,
                                lastCustomerUTC = DateTime.UtcNow,
                                CustomerUid = updateCustomerEvent.CustomerUID
                            },
                            commandType: CommandType.Text);
            }
        }

        public void DeleteCustomer(DeleteCustomerEvent deleteCustomerEvent)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute("delete from Customer where CustomerUID = @CustomerUid;", new
                            {
                                CustomerUid = deleteCustomerEvent.CustomerUID
                            },
                            commandType: CommandType.Text);
            }
        }

        public bool AssociateCustomerUser(AssociateCustomerUserEvent associateCustomerUserEvent)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var customer = connection.Query<Models.Customer>("select * from Customer where CustomerUID = @customerUid", new { customerUid = associateCustomerUserEvent.CustomerUID }, commandType: CommandType.Text).FirstOrDefault();
                if(customer != null && customer.CustomerID >0)
                {
                    return connection.Execute("Insert into UserCustomer(fk_CustomerUID,fk_UserUID,fk_CustomerID,LastUserUTC) values(@CustomerUid,@UserUid,@fk_customerID,@lastUserUTC);", new
                                {
                                    CustomerUid = associateCustomerUserEvent.CustomerUID,
                                    UserUid = associateCustomerUserEvent.UserUID,
                                    fk_customerID = customer.CustomerID,
                                    lastUserUTC = DateTime.UtcNow
                                },
                                commandType: CommandType.Text) == 1;
                } 
                return false;
            }
        }

        public void DissociateCustomerUser(DissociateCustomerUserEvent dissociateCustomerUserEvent)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                connection.Execute("delete from UserCustomer where fk_CustomerUID=@CustomerUid and fk_UserUID = @UserUid;", new
                            {
                                CustomerUid = dissociateCustomerUserEvent.CustomerUID,
                                UserUid = dissociateCustomerUserEvent.UserUID,
                            },
                           commandType: CommandType.Text);
            }
        }


        public List<Models.Customer> GetAssociatedCustomerbyUserUid(System.Guid UserUID)
        {
            List<Models.Customer> customerList = new List<Models.Customer>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                customerList = connection.Query<Models.Customer>("select c.* from Customer c join UserCustomer uc on uc.fk_CustomerUID = c.CustomerUID where uc.fk_UserUID = @userUid",
                    new { userUid = UserUID }, commandType: CommandType.Text).AsList();
            }
            return customerList;
        }

        public Models.Customer GetCustomer(System.Guid CustomerUID)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<Models.Customer>("select * from Customer where CustomerUID = @customerUid", new { customerUid = CustomerUID }, commandType: CommandType.Text).FirstOrDefault();
            }
        }
    }

}
