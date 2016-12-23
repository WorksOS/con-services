using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using KafkaConsumer;
using VSS.UnifiedProductivity.Service.Repositories;
using VSS.UnifiedProductivity.Service.Utils;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Customer.Data
{
    public class CustomerRepository : RepositoryBase, IRepository<ICustomerEvent>
    {
    //    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public CustomerRepository(IConfigurationStore _connectionString) : base(_connectionString)
        {
        }

        public async Task<int> StoreEvent(ICustomerEvent evt)
        {
            var upsertedCount = 0;

            if (evt is CreateCustomerEvent)
            {
                var customerEvent = (CreateCustomerEvent)evt;
                var customer = new Models.Customer();
                customer.CustomerName = customerEvent.CustomerName;
                customer.CustomerUID = customerEvent.CustomerUID.ToString();
                customer.CustomerType = (CustomerType)Enum.Parse(typeof(CustomerType), customerEvent.CustomerType, true);
                customer.LastActionedUTC = customerEvent.ActionUTC;
                upsertedCount = await UpsertCustomerDetail(customer, "CreateCustomerEvent");
            }
            else if (evt is UpdateCustomerEvent)
            {
                var customerEvent = (UpdateCustomerEvent)evt;
                var customer = new Models.Customer();
                customer.CustomerName = customerEvent.CustomerName;
                customer.CustomerUID = customerEvent.CustomerUID.ToString();
                customer.LastActionedUTC = customerEvent.ActionUTC;
                upsertedCount = await UpsertCustomerDetail(customer, "UpdateCustomerEvent");
            }
            else if (evt is DeleteCustomerEvent)
            {
                var customerEvent = (DeleteCustomerEvent)evt;
                var customer = new Models.Customer();
                customer.CustomerUID = customerEvent.CustomerUID.ToString();
                customer.LastActionedUTC = customerEvent.ActionUTC;
                upsertedCount = await UpsertCustomerDetail(customer, "DeleteCustomerEvent");
            }
            else if (evt is AssociateCustomerUserEvent)
            {
                var customerEvent = (AssociateCustomerUserEvent)evt;
                var customerUser = new Models.CustomerUser();
                customerUser.CustomerUID = customerEvent.CustomerUID.ToString();
                customerUser.UserUID = customerEvent.UserUID.ToString();
                customerUser.LastActionedUTC = customerEvent.ActionUTC;
                upsertedCount = await UpsertCustomerUserDetail(customerUser, "AssociateCustomerUserEvent");
            }
            else if (evt is DissociateCustomerUserEvent)
            {
                var customerEvent = (DissociateCustomerUserEvent)evt;
                var customerUser = new Models.CustomerUser();
                customerUser.CustomerUID = customerEvent.CustomerUID.ToString();
                customerUser.UserUID = customerEvent.UserUID.ToString();
                customerUser.LastActionedUTC = customerEvent.ActionUTC;
                upsertedCount = await UpsertCustomerUserDetail(customerUser, "DissociateCustomerUserEvent");
            }

            return upsertedCount;
        }

        /// <summary>
        /// All Customer detail-related columns can be inserted, 
        /// but only certain columns can be updated.
        /// On the deletion, a corresponded entry will be deleted.
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private async Task<int> UpsertCustomerDetail(Models.Customer customer, string eventType)
        {
            int upsertedCount = 0;

            await PerhapsOpenConnection();

            var existing = Connection.Query<Models.Customer>
              (@"SELECT 
                  CustomerUID, CustomerName, fk_CustomerTypeID AS CustomerType, LastActionedUTC
              FROM Customer
              WHERE CustomerUID = @CustomerUid", new { CustomerUid = customer.CustomerUID }).FirstOrDefault();

            if (eventType == "CreateCustomerEvent")
            {
                upsertedCount = CreateCustomer(customer, existing);
            }

            if (eventType == "UpdateCustomerEvent")
            {
                upsertedCount = UpdateCustomer(customer, existing);
            }

            if (eventType == "DeleteCustomerEvent")
            {
                upsertedCount = DeleteCustomer(customer, existing);
            }

          //  Log.DebugFormat("CustomerRepository: upserted {0} rows", upsertedCount);

            PerhapsCloseConnection();

            return upsertedCount;
        }

        private int CreateCustomer(Models.Customer customer, Models.Customer existing)
        {
            if (existing == null)
            {
               // Log.DebugFormat("CustomerRepository: going to create customer={0}", JsonConvert.SerializeObject(customer));

                const string insert =
                  @"INSERT Customer
              (CustomerUID, CustomerName, fk_CustomerTypeID, LastActionedUTC)
              VALUES
              (@CustomerUID, @CustomerName, @CustomerType, @LastActionedUTC)";

                return Connection.Execute(insert, customer);
            }

         //   Log.DebugFormat("CustomerRepository: can't create as already exists newActionedUTC={0}", customer.LastActionedUTC);

            return 0;
        }

        private int UpdateCustomer(Models.Customer customer, Models.Customer existing)
        {
            if (existing != null)
            {
                if (customer.LastActionedUTC >= existing.LastActionedUTC)
                {
                    const string update =
                      @"UPDATE Customer                
                SET CustomerName = @CustomerName,
                    LastActionedUTC = @LastActionedUTC
                WHERE CustomerUID = @CustomerUID";
                    return Connection.Execute(update, customer);
                }

           //     Log.DebugFormat("CustomerRepository: old update event ignored currentActionedUTC={0} newActionedUTC={1}",
        //          existing.LastActionedUTC, customer.LastActionedUTC);
            }
            else
            {
       //         Log.DebugFormat("CustomerRepository: can't update as none existing newActionedUTC={0}",
  //                customer.LastActionedUTC);
            }
            return 0;
        }

        private int DeleteCustomer(Models.Customer customer, Models.Customer existing)
        {
            if (existing != null)
            {
                if (customer.LastActionedUTC >= existing.LastActionedUTC)
                {
                    const string delete =
                      @"DELETE 
              FROM Customer                
              WHERE CustomerUID = @CustomerUID";
                    return Connection.Execute(delete, customer);
                }

      //          Log.DebugFormat("CustomerRepository: old delete event ignored currentActionedUTC={0} newActionedUTC={1}",
   //               existing.LastActionedUTC, customer.LastActionedUTC);
            }
            else
            {
     //           Log.DebugFormat("CustomerRepository: can't delete as none existing newActionedUT={0}",
  //                customer.LastActionedUTC);
            }
            return 0;
        }

        public async Task<Models.Customer> GetAssociatedCustomerbyUserUid(System.Guid userUid)
        {
            await PerhapsOpenConnection();

            var customer = Connection.Query<Models.Customer>
                (@"SELECT c.* 
            FROM Customer c JOIN CustomerUser cu ON cu.fk_CustomerUID = c.CustomerUID 
            WHERE cu.fk_UserUID = @userUid", new { userUid = userUid.ToString() }).FirstOrDefault();

            PerhapsCloseConnection();

            return customer;
        }

        public async Task<Models.Customer> GetCustomer(System.Guid customerUid)
        {
            await PerhapsOpenConnection();

            var customer = Connection.Query<Models.Customer>
                (@"SELECT * 
             FROM Customer 
             WHERE CustomerUID = @customerUid", new { customerUid = customerUid.ToString() }).FirstOrDefault();

            PerhapsCloseConnection();

            return customer;
        }

        /// <summary>
        /// All CustomerUser detail-related columns can be inserted, 
        /// but only certain columns can be updated.
        /// On the deletion, a corresponded entry will be deleted.
        /// </summary>
        /// <param name="customerUser"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private async Task<int> UpsertCustomerUserDetail(Models.CustomerUser customerUser, string eventType)
        {
            int upsertedCount = 0;

            await PerhapsOpenConnection();

       //     Log.DebugFormat("CustomerRepository: Upserting eventType={0} CustomerUid={1}, UserUid={2}",
           //   eventType, customerUser.CustomerUID, customerUser.UserUID);

            var existing = Connection.Query<Models.CustomerUser>
              (@"SELECT 
            fk_UserUID AS UserUID, fk_CustomerUID AS CustomerUID, LastActionedUTC
              FROM CustomerUser
              WHERE fk_CustomerUID = @customerUID AND fk_UserUID = @userUID", new { customerUID = customerUser.CustomerUID, userUID = customerUser.UserUID }).FirstOrDefault();

            if (eventType == "AssociateCustomerUserEvent")
            {
                upsertedCount = AssociateCustomerUser(customerUser, existing);
            }

            if (eventType == "DissociateCustomerUserEvent")
            {
                upsertedCount = DissociateCustomerUser(customerUser, existing);
            }

         //   Log.DebugFormat("CustomerRepository: upserted {0} rows", upsertedCount);

            PerhapsCloseConnection();

            return upsertedCount;
        }

        private int AssociateCustomerUser(Models.CustomerUser customerUser, Models.CustomerUser existing)
        {
            if (existing == null)
            {
                const string insert =
                  @"INSERT CustomerUser
            (fk_UserUID, fk_CustomerUID, LastActionedUTC)
            VALUES
            (@UserUID, @CustomerUID, @LastActionedUTC)";

                return Connection.Execute(insert, customerUser);
            }

      //      Log.DebugFormat("CustomerRepository: can't create as already exists newActionedUTC={0}", customerUser.LastActionedUTC);
            return 0;
        }

        private int DissociateCustomerUser(Models.CustomerUser customerUser, Models.CustomerUser existing)
        {
            if (existing != null)
            {
                if (customerUser.LastActionedUTC >= existing.LastActionedUTC)
                {
                    const string delete =
                      @"DELETE 
              FROM CustomerUser               
              WHERE fk_CustomerUID = @CustomerUID AND fk_UserUID = @UserUID";
                    return Connection.Execute(delete, customerUser);
                }

          //      Log.DebugFormat("CustomerRepository: old delete event ignored currentActionedUTC{0} newActionedUTC{1}",
      //            existing.LastActionedUTC, customerUser.LastActionedUTC);
            }
            else
            {
     //           Log.DebugFormat("CustomerRepository: can't delete as none existing newActionedUTC {0}",
  //                customerUser.LastActionedUTC);
            }
            return 0;
        }


    }
}
