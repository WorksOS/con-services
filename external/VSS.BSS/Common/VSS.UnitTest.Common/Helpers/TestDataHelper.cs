using System;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common.Contexts;
namespace VSS.UnitTest.Common
{
  /// <summary>
  /// Helper class to configure entities with known values in
  /// for unit tests.
  /// </summary>
  public class TestDataHelper 
  {
    #region CONSTANTS

    public const long TRIMBLE_OPS_CUSTOMER_ID = 1;

    public const string EN_US = "en-US";
    public const string fr_FR = "fr-FR";
    public const string ru_RU = "ru-RU";

    public const string CORP_ADMIN_USERNAME = "CORP_ADMIN_USERNAME";
    public const string CORP_ADMIN_PASSWORD = "CORP_ADMIN_PASSWORD";

    public const string DEALER_ADMIN_USERNAME = "DEALER_ADMIN_USERNAME";
    public const string DEALER_ADMIN_PASSWORD = "DEALER_ADMIN_PASSWORD";

    public const string DEALER_USER_USERNAME = "DEALER_USER_USERNAME";
    public const string DEALER_USER_PASSWORD = "DEALER_USER_PASSWORD";

    public const string CUSTOMER_ADMIN_USERNAME = "CUSTOMER_ADMIN_USERNAME";
    public const string CUSTOMER_ADMIN_PASSWORD = "CUSTOMER_ADMIN_PASSWORD";

    public const string CUSTOMER_PASSWORD_CHANGE_USERNAME = "CUSTOMER_PASSWORD_CHANGE_USERNAME";
    public const string CUSTOMER_PASSWORD_CHANGE_PASSWORD = "CUSTOMER_PASSWORD_CHANGE_PASSWORD";

    public const string CUSTOMER_USER_USERNAME = "CUSTOMER_USER_USERNAME";
    public const string CUSTOMER_USER_PASSWORD = "CUSTOMER_USER_PASSWORD";
    public const string CUSTOMER_EMAIL = "TestCustomer@testDomain.com";

    public const string CUSTOMER_ALERT_ADMIN_USERNAME = "CUSTOMER_ALERT_ADMIN_USERNAME";
    public const string CUSTOMER_ALERT_ADMIN_PASSWORD = "CUSTOMER_ALERT_ADMIN_PASSWORD";

    public const string TRIMBLE_USER_USERNAME = "TRIMBLE_USER_USERNAME";
    public const string TRiMBLE_USER_PASSWORD = "TRIMBLE_USER_PASSWORD";

    public const string TESTDEALER1_BELONGTOCAT_USER_USERNAME = "TESTDEALER1_BELONGTOCAT_USER_USERNAME";
    public const string TESTDEALER1_BELONGTOCAT_USER_PASSWORD = "TESTDEALER1_BELONGTOCAT_USER_PASSWORD";

    public const string TESTPARENTOFDEALER1_USER_USERNAME = "TESTPARENTOFDEALER1_USER_USERNAME";
    public const string TESTPARENTOFDEALER1_USER_PASSWORD = "TESTPARENTOFDEALER1_USER_PASSWORD";    

    #endregion

    #region LANGUAGES

    private Language _english;
    private Language _french;
    private Language _russian;

    public Language English
    {
      get
      {
        if (_english == null)
          _english = GetEnglish();
        return _english;
      }
    }

    public Language French
    {
      get
      {
        if (_french == null)
          _french = GetFrench();
        return _french;
      }
    }

    public Language Russian
    {
      get
      {
        if (_russian == null)
          _russian = GetRussian();
        return _russian;
      }
    }

    private Language GetEnglish()
    {
      Language enUS = ContextContainer.Current.OpContext.Language.SingleOrDefault(x => x.ISOName == EN_US);
      if (enUS == null)
      {
        enUS = new Language { ISOName = EN_US, ID = 1 };
        ContextContainer.Current.OpContext.Language.AddObject(enUS);
        ContextContainer.Current.OpContext.SaveChanges();
      }
      return enUS;
    }

    private Language GetFrench()
    {
      Language frFR = ContextContainer.Current.OpContext.Language.SingleOrDefault(x => x.ISOName == fr_FR);
      if (frFR == null)
      {
        frFR = new Language { ISOName = fr_FR, ID = 2 };
        ContextContainer.Current.OpContext.Language.AddObject(frFR);
        ContextContainer.Current.OpContext.SaveChanges();
      }
      return frFR;
    }

    private Language GetRussian()
    {
      Language ruRU = ContextContainer.Current.OpContext.Language.SingleOrDefault(x => x.ISOName == ru_RU);
      if (ruRU == null)
      {
        ruRU = new Language { ISOName = ru_RU, ID = 4 };
        ContextContainer.Current.OpContext.Language.AddObject(ruRU);
        ContextContainer.Current.OpContext.SaveChanges();
      }
      return ruRU;
    }
    #endregion

    #region CUSTOMERS

    private Customer _testCorporate;
    private Customer _testDealer;
    private Customer _testCustomer;
    private Customer _testAccount;
    private Customer _testTrimbleOps;

    public Customer TestCorporate
    {
      get
      {
        if (_testCorporate == null)
          _testCorporate = Entity.Customer.Corporate.Name("TESTDATA_CORPORATE").SyncWithRpt().Save();
        return _testCorporate;
      }
    }
    public Customer TestDealer
    {
      get
      {
        if(_testDealer == null)
          _testDealer = Entity.Customer.Dealer.Name("TESTDATA_DEALER").SyncWithRpt().Save();
        return _testDealer;
      }
    }
    public Customer TestCustomer 
    {
      get 
      {
        if (_testCustomer == null)
          _testCustomer = Entity.Customer.EndCustomer.Name("TESTDATA_ENDCUSTOMER").SyncWithRpt().Save();
        return _testCustomer;
      }
    }
    public Customer TestAccount 
    {
      get 
      {
        if (_testAccount == null)
        {
          _testAccount = Entity.Customer.Account.Name("TESTDATA_ACCOUNT").SyncWithRpt().Save();
 //         var dummy = TestDealerHierarchy;
   //       dummy = TestCustomerHierarchy;
        }
        return _testAccount;
      }
    }
    public Customer TestTrimbleOps
    {
      get
      {
        if (_testTrimbleOps == null)
          _testTrimbleOps = GetTrimbleOpsCustomer();
        return _testTrimbleOps;
      }
    }

    private Customer GetTrimbleOpsCustomer()
    {
      Customer customer = ContextContainer.Current.OpContext.Customer.FirstOrDefault(x => x.ID == TRIMBLE_OPS_CUSTOMER_ID);
      if (customer == null)
      {
        customer = Entity.Customer.Administrator.Id(1).Name("Trimble Operations").SyncWithRpt().Save();
      }
      return customer;
    }

    #endregion
    
    #region CUSTOMERS_DEALERS_ACCOUNTS

    private Customer _D1;
    private Customer _D2;
    private Customer _D3;
    
    private Customer _ParentOfD1;
    private Customer _ParentOfD2;

    private Customer _C0;
    private Customer _C1;
    private Customer _C2;
    private Customer _C3;

    private Customer _A0C0;
    private Customer _A1D1C1;
    private Customer _A2D1C1;
    private Customer _A3D1C2;
    private Customer _A4D2C1;
    private Customer _A5D2C2;
    private Customer _A6C3;

    public Customer TestDealer1BelongToCAT
    {
      get
      {
        if (_D1 == null)
        {
          _D1 = Entity.Customer.Dealer.Name("TESTDATA_DEALER1").DealerNetwork(DealerNetworkEnum.CAT).SyncWithRpt().Save();
          Entity.CustomerRelationship.Relate(TestParentOfDealer1, _D1).Save();
        }
        return _D1;
      }
    }

    public Customer TestParentOfDealer1
    {
      get
      {
        if (_ParentOfD1 == null)
          _ParentOfD1 = Entity.Customer.Dealer.Name("TESTDATA_DEALER1Parent").DealerNetwork(DealerNetworkEnum.CAT).SyncWithRpt().Save();
        return _ParentOfD1;
      }
    }

    public Customer TestParentOfDealer2
    {
      get
      {
        if (_ParentOfD2 == null)
          _ParentOfD2 = Entity.Customer.Dealer.Name("TESTDATA_DEALER2Parent").DealerNetwork(DealerNetworkEnum.CAT).SyncWithRpt().Save();
        return _ParentOfD2;
      }
    }

    public Customer TestDealer2BelongToCAT
    {
      get
      {
        if (_D2 == null)
        {
          _D2 = Entity.Customer.Dealer.Name("TESTDATA_DEALER2").DealerNetwork(DealerNetworkEnum.CAT).SyncWithRpt().Save();
          Entity.CustomerRelationship.Relate(TestParentOfDealer2, _D2).Save();
        }
        return _D2;
      }
    }

    public Customer TestDealer3BelongToTRMB
    {
      get
      {
        if (_D3 == null)
          _D3 = Entity.Customer.Dealer.Name("TESTDATA_DEALER3").DealerNetwork(DealerNetworkEnum.TRMB).SyncWithRpt().Save();
        return _D3;
      }
    }

    public Customer TestCustomer0
    {
      get
      {
        if (_C0 == null)
          _C0 = Entity.Customer.EndCustomer.Name("TESTDATACUSTOMER0").SyncWithRpt().Save();
        return _C0;
      }
    }

    public Customer TestCustomer1
    {
      get
      {
        if (_C1 == null)
        {
          _C1 = Entity.Customer.EndCustomer.Name("TESTDATACUSTOMER1").SyncWithRpt().Save();
          
        }
        return _C1;
      }
    }

    public Customer TestCustomer2
    {
      get
      {
        if (_C2 == null)
        {
          _C2 = Entity.Customer.EndCustomer.Name("TESTDATACUSTOMER2").SyncWithRpt().Save();
        }
        return _C2;
      }
    }

    public Customer TestCustomer3
    {
      get
      {
        if (_C3 == null)
        {
          _C3 = Entity.Customer.EndCustomer.Name("TESTDATACUSTOMER2").SyncWithRpt().Save();
        }
        return _C3;
      }
    }

    public Customer TestAccount0RelatedToC0
    {
      get
      {
        if (_A0C0 == null)
        {
          _A0C0 = Entity.Customer.Account.Name("TESTDATA_ACCOUNT0").SyncWithRpt().Save();
          Entity.CustomerRelationship.Relate(TestCustomer0,_A0C0).Save();
        }
        return _A0C0;
      }
    }

    public Customer TestAccount6RelatedToC3
    {
      get
      {
        if (_A6C3 == null)
        {
          _A6C3 = Entity.Customer.Account.Name("TESTDATA_ACCOUNT6").SyncWithRpt().Save();
          Entity.CustomerRelationship.Relate(TestCustomer3, _A6C3).Save();
        }
        return _A6C3;
      }
    }

    public Customer TestAccount1RelatedToD1C1
    {
      get
      {
        if (_A1D1C1 == null)
        {
          _A1D1C1 = Entity.Customer.Account.Name("TESTDATA_ACCOUNT1").SyncWithRpt().Save();
          Entity.CustomerRelationship.Relate(TestCustomer1, _A1D1C1).Save();
          Entity.CustomerRelationship.Relate(TestDealer1BelongToCAT, _A1D1C1).Save();
        }
        return _A1D1C1;
      }
    }

    public Customer TestAccount2RelatedToD1C1
    {
      get
      {
        if (_A2D1C1 == null)
        {
          _A2D1C1 = Entity.Customer.Account.Name("TESTDATA_ACCOUNT2").SyncWithRpt().Save();
          Entity.CustomerRelationship.Relate(TestCustomer1,_A2D1C1).Save();
          Entity.CustomerRelationship.Relate(TestDealer1BelongToCAT,_A2D1C1).Save();
        }
        return _A2D1C1;
      }
    }

    public Customer TestAccount3RelatedToD1C2
    {
      get
      {
        if (_A3D1C2 == null)
        {
          _A3D1C2 = Entity.Customer.Account.Name("TESTDATA_ACCOUNT3").SyncWithRpt().Save();
          Entity.CustomerRelationship.Relate(TestDealer1BelongToCAT, _A3D1C2).Save();
          Entity.CustomerRelationship.Relate(TestCustomer2, _A3D1C2).Save();
        }
        return _A3D1C2;
      }
    }

    public Customer TestAccount4RelatedToD2C1
    {
      get
      {
        if (_A4D2C1 == null)
        {
          _A4D2C1 = Entity.Customer.Account.Name("TESTDATA_ACCOUNT4").SyncWithRpt().Save();
          Entity.CustomerRelationship.Relate(TestDealer2BelongToCAT, _A4D2C1).Save();
          Entity.CustomerRelationship.Relate(TestCustomer1, _A4D2C1).Save();
        }
        return _A4D2C1;
      }
    }

    public Customer TestAccount5RelatedToD2C2
    {
      get
      {
        if (_A5D2C2 == null)
        {
          _A5D2C2 = Entity.Customer.Account.Name("TESTDATA_ACCOUNT5").SyncWithRpt().Save();
          Entity.CustomerRelationship.Relate(TestDealer2BelongToCAT, _A5D2C2).Save();
          Entity.CustomerRelationship.Relate(TestCustomer2, _A5D2C2).Save();
        }
        return _A5D2C2;
      }
    }

    #endregion
  
    #region RELATIONSHIPS

    private CustomerRelationship _testDealerHierarchy;
    private CustomerRelationship _testCustomerHierarchy;

    public CustomerRelationship TestDealerHierarchy 
    {
      get 
      {
        if (_testDealerHierarchy == null)
          _testDealerHierarchy = Entity.CustomerRelationship.Relate(TestDealer, TestAccount).Save();
        return _testDealerHierarchy;
      }
    }
    public CustomerRelationship TestCustomerHierarchy 
    {
      get 
      {
        if (_testCustomerHierarchy == null)
          _testCustomerHierarchy = Entity.CustomerRelationship.Relate(TestCustomer, TestAccount).Save();
        return _testCustomerHierarchy;
      }
    }

    #endregion

    #region USERS

    private User _testCorporateAdmin;
    private User _testDealerAdmin;
    private User _testDealerUser;
    private User _testCustomerAdmin;
    private User _testPasswordChange;
    private User _testCustomerUser;
    private User _testApiUser;
    private User _testCustomerWithDetails;
    private User _testPasswordExpieryCustomer;
    private User _testFirstTimeLoginCustomer;
    private User _testCustomerAdminUser;
    private User _testCustomerAlertAdminUser;
    private User _testTrimbleOpsAdmin;
    private User _testDealer1BelongToCATUser;
    private User _testParentOfDealer1User;
	  private readonly string TestUserUID = Guid.NewGuid().ToString();
    public User TestApiUser
    {
      get
      {
        if (_testApiUser == null)
        {
          _testApiUser = Entity.User.ForCustomer(TestCustomer).WithLanguage(English)
                                          .Username(CUSTOMER_USER_USERNAME).Password(CUSTOMER_USER_PASSWORD)
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.DataServices)
                                            .Access(FeatureAccessEnum.View)).Save();
        }
        return _testApiUser;
      }
    }

    public User TestCustomerWithDetails
    {
      get
      {
        if (_testCustomerWithDetails == null)
        {
          _testCustomerWithDetails = Entity.User.ForCustomer(TestCustomer).WithLanguage(English)
                                          .Username(CUSTOMER_USER_USERNAME).Password(CUSTOMER_USER_PASSWORD).FirstName("be").LastName("back").Address("future").PhoneNumber("9999").JobTitle("terminator").UserUID(TestUserUID).Email(CUSTOMER_EMAIL)
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.NHWeb)
                                            .Feature(FeatureEnum.NHWebAdmin)
                                            .Access(FeatureAccessEnum.View)).Save();
        }
        return _testCustomerWithDetails;
      }
    }

    public User TestCorporateAdmin
    {
      get
      {
        if (_testCorporateAdmin == null)
        {
          _testCorporateAdmin = Entity.User.ForCustomer(TestCorporate).WithLanguage(English)
                                        .Username(CORP_ADMIN_USERNAME).Password(CORP_ADMIN_PASSWORD)
                                        .WithFeature(x => x
                                          .App(FeatureAppEnum.NHWeb)
                                          .Feature(FeatureEnum.NHWebAdmin)
                                          .Access(FeatureAccessEnum.Full)).Save();
        }
        return _testCorporateAdmin;
      }
    }
    public User TestDealerAdmin
    {
      get
      {
        if(_testDealerAdmin == null)
        {
          _testDealerAdmin = Entity.User.ForCustomer(TestDealer).WithLanguage(English)
                                        .Username(DEALER_ADMIN_USERNAME).Password(DEALER_ADMIN_PASSWORD).Email("DEALER_ADMIN_USERNAME@unittest.com")
                                        .WithFeature(x => x
                                          .App(FeatureAppEnum.NHWeb)
                                          .Feature(FeatureEnum.NHWebAdmin)
                                          .Access(FeatureAccessEnum.Full)).Save();
        }
        return _testDealerAdmin;
      }
    }
    public User TestDealerUser
    {
      get
      {
        if(_testDealerUser == null)
        {
          _testDealerUser = Entity.User.ForCustomer(TestDealer).WithLanguage(English)
                                       .Username(DEALER_USER_USERNAME).Password(DEALER_USER_PASSWORD)
                                       .WithFeature(x => x
                                         .App(FeatureAppEnum.NHWeb)
                                         .Access(FeatureAccessEnum.View)).Save();
        }
        return _testDealerUser;
      }
    }
    public User TestCustomerAdmin
    {
      get
      {
        if(_testCustomerAdmin == null)
        {
          _testCustomerAdmin = Entity.User.ForCustomer(TestCustomer).WithLanguage(English)
                                          .Username(CUSTOMER_ADMIN_USERNAME).Password(CUSTOMER_ADMIN_PASSWORD).Timezone("Eastern Standard Time")
                                          .Email(CUSTOMER_EMAIL)
																					.UserUID(TestUserUID)
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.NHWeb)
                                            .Feature(FeatureEnum.NHWebAdmin)
                                            .Access(FeatureAccessEnum.View)).FirstName("VSS").LastName("Admin").Save();
        }
        return _testCustomerAdmin;
      }
    }

    public User TestPasswordChange
    {
      get
      {
        if (_testPasswordChange == null)
        {
          _testPasswordChange = Entity.User.ForCustomer(TestCustomer).WithLanguage(English)
                                          .Username(CUSTOMER_PASSWORD_CHANGE_USERNAME).Password(CUSTOMER_PASSWORD_CHANGE_PASSWORD)
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.NHWeb)
                                            .Feature(FeatureEnum.NHWebAdmin)
                                            .Access(FeatureAccessEnum.View)).FirstName("VSS").LastName("Admin").Save();
        }
        return _testPasswordChange;
      }
    }

    public User TestCustomerUser
    {
      get
      {
        if(_testCustomerUser == null)
        {
          _testCustomerUser = Entity.User.ForCustomer(TestCustomer).WithLanguage(English)
                                          .Username(CUSTOMER_USER_USERNAME).Password(CUSTOMER_USER_PASSWORD)
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.NHWeb)
                                            .Access(FeatureAccessEnum.View)).Save();
        }
        return _testCustomerUser;
      }
    }

    public User TestPasswordExpieryCustomer
    {
      get
      {
        if (_testPasswordExpieryCustomer == null)
        {
          _testPasswordExpieryCustomer = Entity.User.ForCustomer(TestCustomer).WithLanguage(English).PasswordExpirationUtc(DateTime.UtcNow.AddMonths(-1))
                                          .Username(CUSTOMER_USER_USERNAME).Password(CUSTOMER_USER_PASSWORD)
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.NHWeb)
                                            .Access(FeatureAccessEnum.View)).Save();
        }
        return _testPasswordExpieryCustomer;
      }
    }

    public User FirstTimeLoginCustomer
    {
      get
      {
        if (_testFirstTimeLoginCustomer == null)
        {
          _testFirstTimeLoginCustomer = Entity.User.ForCustomer(TestCustomer).WithLanguage(English).PasswordExpirationUtc(null)
                                          .Username(CUSTOMER_USER_USERNAME).Password(CUSTOMER_USER_PASSWORD)
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.NHWeb)
                                            .Access(FeatureAccessEnum.View)).Save();
        }
        return _testFirstTimeLoginCustomer;
      }
    }

    public User TestCustomerAdminUser
    {
      get
      {
        if (_testCustomerAdminUser == null)
        {
          _testCustomerAdminUser = Entity.User.ForCustomer(TestCustomer).WithLanguage(English)
                                          .Username(CUSTOMER_USER_USERNAME).Password(CUSTOMER_USER_PASSWORD).Email("CUSTOMER_USER_USERNAME@untitest.com")
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.NHWeb)
                                            .Access(FeatureAccessEnum.Full)).Save();
        }
        return _testCustomerAdminUser;
      }
    }
    public User TestCustomerAlertAdminUser
    {
      get
      {
        if(_testCustomerAlertAdminUser == null)
        {
          _testCustomerAlertAdminUser = Entity.User.ForCustomer(TestCustomer).WithLanguage(English)
                                                    .Username(CUSTOMER_ALERT_ADMIN_USERNAME).Password(CUSTOMER_ALERT_ADMIN_PASSWORD)
                                                    .WithFeature(
                                                      x =>
                                                      x.App(FeatureAppEnum.NHWeb).Access(FeatureAccessEnum.View).Child(FeatureChildEnum.Alerts).Access(
                                                        FeatureAccessEnum.Full)).Save();

        }
        return _testCustomerAlertAdminUser;
      }
    }
    public User TestTrimbleOpsAdmin
    {
      get
      {
        if(_testTrimbleOpsAdmin == null)
        {
          _testTrimbleOpsAdmin = Entity.User.ForCustomer(TestTrimbleOps).WithLanguage(English)
                                          .Username(TRIMBLE_USER_USERNAME).Password(TRiMBLE_USER_PASSWORD)
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.NHWeb)
                                            .Feature(FeatureEnum.NHWebAdmin)
                                            .Access(FeatureAccessEnum.Full)).Save();
        }
        return _testTrimbleOpsAdmin;
      }
    }
    public User TestDealer1BelongToCATUser
    {
      get
      {
        if (_testDealer1BelongToCATUser == null)
        {
          _testDealer1BelongToCATUser = Entity.User.ForCustomer(TestDealer1BelongToCAT).WithLanguage(English)
                                          .Username(TESTDEALER1_BELONGTOCAT_USER_USERNAME).Password(TESTDEALER1_BELONGTOCAT_USER_USERNAME)
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.NHWeb)
                                            .Feature(FeatureEnum.NHWebAdmin)
                                            .Access(FeatureAccessEnum.Full)).Save();
        }
        return _testDealer1BelongToCATUser;
      }
    }
    public User TestParentOfDealer1User
    {
      get
      {
        if (_testParentOfDealer1User == null)
        {
          _testParentOfDealer1User = Entity.User.ForCustomer(TestParentOfDealer1).WithLanguage(English)
                                          .Username(TESTPARENTOFDEALER1_USER_USERNAME).Password(TESTPARENTOFDEALER1_USER_USERNAME)
                                          .WithFeature(x => x
                                            .App(FeatureAppEnum.NHWeb)
                                            .Feature(FeatureEnum.NHWebAdmin)
                                            .Access(FeatureAccessEnum.Full)).Save();
        }
        return _testParentOfDealer1User;
      }
    }  
    #endregion
    
    #region ACTIVEUSER

    private ActiveUser _testActiveCustomerWithDetails;
    private ActiveUser _corporateActiveUser;
    private ActiveUser _dealerAdminActiveUser;
    private ActiveUser _dealerUserActiveUser;
    private ActiveUser _customerAdminActiveUser;
    private ActiveUser _customerPasswordChangeUser;
    private ActiveUser _customerUserActiveUser;
    private ActiveUser _trimbleOpsActiveUser;
    private ActiveUser _testDealer1BelongToCATActiveUser;
    private ActiveUser _testParentOfDealer1ActiveUser;

    public ActiveUser CorporateActiveUser
    {
      get
      {
        if (_corporateActiveUser == null)
          _corporateActiveUser = Entity.ActiveUser.ForUser(TestCorporateAdmin).Save();
        return _corporateActiveUser;
      }
      set
      {
        _corporateActiveUser = value;
      }
    }

    public ActiveUser TestActiveCustomerWithDetails
    {
      get {
        return _testActiveCustomerWithDetails ??
               (_testActiveCustomerWithDetails = Entity.ActiveUser.ForUser(TestCustomerWithDetails).Save());
      }
      set { _testActiveCustomerWithDetails = value; }

    }

    public ActiveUser DealerAdminActiveUser
    {
      get
      {
        if (_dealerAdminActiveUser == null)
          _dealerAdminActiveUser = Entity.ActiveUser.ForUser(TestDealerAdmin).Save();
        return _dealerAdminActiveUser;
      }
      set
      {
        _dealerAdminActiveUser = value;
      }
    }
    public ActiveUser DealerUserActiveUser
    {
      get
      {
        if (_dealerUserActiveUser == null)
          _dealerUserActiveUser = Entity.ActiveUser.ForUser(TestDealerUser).Save();
        return _dealerUserActiveUser;
      }
      set
      {
        _dealerUserActiveUser = value;
      }
    }
    public ActiveUser CustomerAdminActiveUser
    {
      get
      {
        if (_customerAdminActiveUser == null)
          _customerAdminActiveUser = Entity.ActiveUser.ForUser(TestCustomerAdmin).WithLastActivity(DateTime.UtcNow).Save();
        return _customerAdminActiveUser;
      }
      set
      {
        _customerAdminActiveUser = value;
      }
    }

    public ActiveUser CustomerPasswordChangeActiveUser
    {
      get
      {
        if (_customerPasswordChangeUser == null)
          _customerPasswordChangeUser = Entity.ActiveUser.ForUser(TestPasswordChange).WithLastActivity(DateTime.UtcNow).Save();
        return _customerPasswordChangeUser;
      }
      set
      {
        _customerPasswordChangeUser = value;
      }
    }
    public ActiveUser CustomerUserActiveUser
    {
      get
      {
        if (_customerUserActiveUser == null)
          _customerUserActiveUser = Entity.ActiveUser.ForUser(TestCustomerUser).Save();
        return _customerUserActiveUser;
      }
      set
      {
        _customerUserActiveUser = value;
      }
    }
    public ActiveUser TrimbleOpsActiveUser
    {
      get
      {
        if (_trimbleOpsActiveUser == null)
          _trimbleOpsActiveUser = Entity.ActiveUser.ForUser(TestTrimbleOpsAdmin).WithLastActivity(DateTime.UtcNow).Save();
        return _trimbleOpsActiveUser;
      }
      set
      {
        _trimbleOpsActiveUser = value;
      }
    }
    public ActiveUser TestDealer1BelongToCATActiveUser
    {
      get
      {
        if (_testDealer1BelongToCATActiveUser == null)
          _testDealer1BelongToCATActiveUser = Entity.ActiveUser.ForUser(TestDealer1BelongToCATUser).Save();
        return _testDealer1BelongToCATActiveUser;
      }
      set
      {
        _testDealer1BelongToCATActiveUser = value;
      }
    }
    public ActiveUser TestParentOfDealer1ActiveUser
    {
      get
      {
        if (_testParentOfDealer1ActiveUser == null)
          _testParentOfDealer1ActiveUser = Entity.ActiveUser.ForUser(TestParentOfDealer1User).Save();
        return _testParentOfDealer1ActiveUser;
      }
      set
      {
        _testParentOfDealer1ActiveUser = value;
      }
    }

    #endregion

    #region DEVICES

    private Device _testNoDevice;
    private Device _testPL121;
    private Device _testPL321;
    private Device _testPL421;
    private Device _testPL431;
    private Device _testSNM451;
    private Device _testPL420;
    private Device _testMTS521;
    private Device _testMTS522;
    private Device _testMTS523;
    private Device _testTrimTrac;
    private Device _testCrossCheck;
    private Device _testSNM940;
    private Device _testTAP66;
    private Device _testPL641;
    private Device _testPLE641;
    private Device _testDCM300;
    private Device _testPL631;
    private Device _testPLE631;
    private Device _testPLE41_PL631;
    private Device _testPL131;
    private Device _testPL141;
    private Device _testPL440;
    private Device _testPL240;
    private Device _testPL161;



    public Device TestNoDevice
    {
      get
      {
        if (_testNoDevice == null)
          _testNoDevice = Entity.Device.NoDevice.OwnerBssId(TestDealer.BSSID).Save();
        return _testNoDevice;
      }
    }
    public Device TestPL121
    {
      get
      {
        if (_testPL121 == null)
          _testPL121 = Entity.Device.PL121.OwnerBssId(TestDealer.BSSID).Save();
        return _testPL121;
      }
    }
    public Device TestPL321
    {
      get
      {
        if (_testPL321 == null)
          _testPL321 = Entity.Device.PL321.OwnerBssId(TestDealer.BSSID).Save();
        return _testPL321;
      }
    }
    
    public Device TestMTS521
    {
      get
      {
        if (_testMTS521 == null)
          _testMTS521 = Entity.Device.MTS521.OwnerBssId(TestAccount.BSSID).Save();
        return _testMTS521;
      }
    }
    public Device TestMTS522
    {
      get
      {
        if (_testMTS522 == null)
          _testMTS522 = Entity.Device.MTS522.OwnerBssId(TestAccount.BSSID).Save();
        return _testMTS522;
      }
    }

    public Device TestMTS521Subscribed
    {
      get
      {
        if (_testMTS521 == null)
          _testMTS521 = Entity.Device.MTS521.OwnerBssId(TestAccount.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
        return _testMTS521;
      }
    }
    public Device TestMTS522Subscribed
    {
      get
      {
        if (_testMTS522 == null)
          _testMTS522 = Entity.Device.MTS522.OwnerBssId(TestAccount.BSSID).DeviceState(DeviceStateEnum.Subscribed).Save();
        return _testMTS522;
      }
    }

    public Device TestMTS523
    {
      get
      {
        if (_testMTS523 == null)
          _testMTS523 = Entity.Device.MTS523.OwnerBssId(TestAccount.BSSID).Save();
        return _testMTS523;
      }
    }
    public Device TestSNM940 
    { 
      get
      {
        if (_testSNM940 == null)
          _testSNM940 = Entity.Device.SNM940.OwnerBssId(TestAccount.BSSID).Save();
        return _testSNM940;
      }
    }
    public Device TestPL420
    {
      get
      {
        if (_testPL420 == null)
          _testPL420 = Entity.Device.PL420.OwnerBssId(TestAccount.BSSID).Save();
        return _testPL420;
      }
    }
    public Device TestPL421
    {
      get
      {
        if (_testPL421 == null)
          _testPL421 = Entity.Device.PL421.OwnerBssId(TestAccount.BSSID).Save();
        return _testPL421;
      }
    }
    public Device TestPL431
    {
      get
      {
        if (_testPL431 == null)
          _testPL431 = Entity.Device.PL431.OwnerBssId(TestAccount.BSSID).Save();
        return _testPL431;
      }
    }
    public Device TestSNM451
    {
      get
      {
        if (_testSNM451 == null)
          _testSNM451 = Entity.Device.SNM451.OwnerBssId(TestAccount.BSSID).Save();
        return _testSNM451;
      }
    }

    public Device TestTrimTrac
    {
      get
      {
        if (_testTrimTrac == null)
          _testTrimTrac = Entity.Device.TrimTrac.OwnerBssId(TestAccount.BSSID).Save();
        return _testTrimTrac;
      }
    }

    public Device TestCrossCheck
    {
      get
      {
        if (_testCrossCheck == null)
          _testCrossCheck = Entity.Device.CrossCheck.OwnerBssId(TestAccount.BSSID).Save();
        return _testCrossCheck;
      }
    }

    public Device TestTAP66
    {
      get
      {
        if (_testTAP66 == null)
          _testTAP66 = Entity.Device.TAP66.OwnerBssId(TestAccount.BSSID).Save();
        return _testTAP66;
      }
    }

    public Device TestPL641
    {
      get
      {
        if (_testPL641 == null)
          _testPL641 = Entity.Device.PL641.OwnerBssId(TestAccount.BSSID).Save();
        return _testPL641;
      }
    }

    public Device TestPLE641
    {
      get
      {
        if (_testPLE641 == null)
          _testPLE641 = Entity.Device.PLE641.OwnerBssId(TestAccount.BSSID).Save();
        return _testPLE641;
      }
    }

    public Device TestDCM300
    {
      get
      {
        if (_testDCM300 == null)
          _testDCM300 = Entity.Device.DCM300.OwnerBssId(TestAccount.BSSID).Save();
        return _testDCM300;
      }
    }

    public Device TestPL631
    {
      get
      {
        if (_testPL631 == null)
          _testPL631 = Entity.Device.PL631.OwnerBssId(TestAccount.BSSID).Save();
        return _testPL631;
      }
    }

    public Device TestPLE631
    {
      get
      {
        if (_testPLE631 == null)
          _testPLE631 = Entity.Device.PLE631.OwnerBssId(TestAccount.BSSID).Save();
        return _testPLE631;
      }
    }

    public Device TestPLE641_PL631
    {
      get
      {
        if (_testPLE41_PL631 == null)
            _testPLE41_PL631 = Entity.Device.PLE641PLUSPL631.OwnerBssId(TestAccount.BSSID).Save();
        return _testPLE41_PL631;
      }
    }

    public Device TestPL131
    {
      get
      {
        if (_testPL131 == null)
          _testPL131 = Entity.Device.PL131.OwnerBssId(TestAccount.BSSID).Save();
        return _testPL131;
      }
    }

    public Device TestPL141
    {
      get
      {
        if (_testPL141 == null)
          _testPL141 = Entity.Device.PL141.OwnerBssId(TestAccount.BSSID).Save();
        return _testPL141;
      }
    }
    public Device TestPL161
    {
        get
        {
            if (_testPL161 == null)
                _testPL161 = Entity.Device.PL161.OwnerBssId(TestAccount.BSSID).Save();
            return _testPL161;
        }
    }

    public Device TestPL440
    {
      get
      {
        if (_testPL440 == null)
          _testPL440 = Entity.Device.PL440.OwnerBssId(TestAccount.BSSID).Save();
        return _testPL440;
      }
    }

    public Device TestPL240
    {
        get
        {
            if (_testPL240 == null)
                _testPL240 = Entity.Device.PL440.OwnerBssId(TestAccount.BSSID).Save();
            return _testPL240;
        }
    }

    #endregion

    #region ASSETS

    private Asset _testAssetNoDevice;
    private Asset _testAssetPL121;
    private Asset _testAssetPL321;
    private Asset _testAssetPL421;
    private Asset _testAssetPL420;
    private Asset _testAssetSNM451;
    private Asset _testAssetMTS521;
    private Asset _testAssetMTS522;
    private Asset _testAssetMTS523;
    private Asset _testAssetTAP66;
    private Asset _testAssetSNM940;
    private Asset _testAssetTrimTrack;
    private Asset _testAssetCrossCheck;
    private Asset _testAssetPLE641;

    public Asset TestAssetNoDevice
    {
      get
      {
        if (_testAssetNoDevice == null)
          _testAssetNoDevice = Entity.Asset.WithDevice(TestNoDevice).SerialNumberVin("TESTDATA_ASSET_NODEVICE").SyncWithRpt().Save();
        return _testAssetNoDevice;
      }
    }
    public Asset TestAssetPL121 
    {
      get 
      {
        if (_testAssetPL121 == null)
          _testAssetPL121 = Entity.Asset.WithDevice(TestPL121).SerialNumberVin("TESTDATA_ASSET_PL121").MakeCode("CAT").SyncWithRpt().Save();
        return _testAssetPL121;
      }
    }
    public Asset TestAssetPL321 
    {
      get 
      {
        if (_testAssetPL321 == null)
          _testAssetPL321 = Entity.Asset.WithDevice(TestPL321).SerialNumberVin("TESTDATA_ASSET_PL321").MakeCode("CAT").SyncWithRpt().Save();
        return _testAssetPL321;
      }
    }
    public Asset TestAssetPL421
    {
      get
      {
        if (_testAssetPL421 == null)
          _testAssetPL421 = Entity.Asset.WithDevice(TestPL421).SerialNumberVin("TESTDATA_ASSET_PL421").SyncWithRpt().Save();
        return _testAssetPL421;
      }
    }
    public Asset TestAssetPL420
    {
      get
      {
        if (_testAssetPL420 == null)
          _testAssetPL420 = Entity.Asset.WithDevice(TestPL420).SerialNumberVin("TESTDATA_ASSET_PL420").SyncWithRpt().Save();
        return _testAssetPL420;
      }
    }
    public Asset TestAssetSNM451
    {
      get
      {
        if (_testAssetSNM451 == null)
          _testAssetSNM451 = Entity.Asset.WithDevice(TestSNM451).SerialNumberVin("TESTDATA_ASSET_SNM451").SyncWithRpt().Save();
        return _testAssetSNM451;
      }
    }
    public Asset TestAssetMTS521 
    {
      get 
      {
        if (_testAssetMTS521 == null)
          _testAssetMTS521 = Entity.Asset.WithDevice(TestMTS521).SerialNumberVin("TESTDATA_ASSET_MTS521").SyncWithRpt().Save();
        return _testAssetMTS521;
      }
    }
    public Asset TestAssetMTS522 
    {
      get 
      {
        if (_testAssetMTS522 == null)
          _testAssetMTS522 = Entity.Asset.WithDevice(TestMTS522).SerialNumberVin("TESTDATA_ASSET_MTS522").SyncWithRpt().Save();
        return _testAssetMTS522;
      }
    }
    public Asset TestAssetMTS523 
    {
      get 
      {
        if (_testAssetMTS523 == null)
          _testAssetMTS523 = Entity.Asset.WithDevice(TestMTS523).SerialNumberVin("TESTDATA_ASSET_MTS523").SyncWithRpt().Save();
        return _testAssetMTS523;
      }
    }
    public Asset TestAssetTAP66
    {
      get
      {
        if (_testAssetTAP66 == null)
          _testAssetTAP66 = Entity.Asset.WithDevice(TestTAP66).SerialNumberVin("TESTDATA_ASSET_TAP66").SyncWithRpt().Save();
        return _testAssetTAP66;
      }
    }

    public Asset TestAssetSNM940 
    {
      get 
      {
        if (_testAssetSNM940 == null)
          _testAssetSNM940 = Entity.Asset.WithDevice(TestSNM940).SerialNumberVin("TESTDATA_ASSET_SNM940").SyncWithRpt().Save();
        return _testAssetSNM940;
      }
    }
    public Asset TestAssetTrimTrac 
    {
      get 
      {
        if (_testAssetTrimTrack == null)
          _testAssetTrimTrack = Entity.Asset.WithDevice(TestTrimTrac).SerialNumberVin("TESTDATA_ASSET_TRIMTRAC").SyncWithRpt().Save();
        return _testAssetTrimTrack;
      }
    }
    public Asset TestAssetCrossCheck
    {
      get
      {
        if (_testAssetCrossCheck == null)
          _testAssetCrossCheck = Entity.Asset.WithDevice(TestCrossCheck).SerialNumberVin("CrossCheckAsset").SyncWithRpt().Save();
        return _testAssetCrossCheck;
      }
    }

    public Asset TestAssetPLE641
    {
      get
      {
        if (_testAssetPLE641 == null)
          _testAssetPLE641 = Entity.Asset.WithDevice(TestPLE641).SerialNumberVin("PLE641Asset").SyncWithRpt().Save();
        return _testAssetPLE641;
      }
    }

    #endregion

    #region ASSET EXPECTED HOURS PROJECTED

    private AssetExpectedRuntimeHoursProjected _assetExpectedRuntimeHoursProjectedForTestAssetPL121;
    private AssetExpectedRuntimeHoursProjected _assetExpectedRuntimeHoursProjectedForTestAssetPL321;
    private AssetExpectedRuntimeHoursProjected _assetExpectedRuntimeHoursProjectedForTestAssetMTS521;
    private AssetExpectedRuntimeHoursProjected _assetExpectedRuntimeHoursProjectedForTestAssetMTS522;

    public AssetExpectedRuntimeHoursProjected AssetExpectedRuntimeHoursProjectedForTestAssetPL121
    {
      get
      {
        if (_assetExpectedRuntimeHoursProjectedForTestAssetPL121 == null)
        {
          _assetExpectedRuntimeHoursProjectedForTestAssetPL121 = Entity.AssetExpectedRuntimeHoursProjected.ForAsset(TestAssetPL121).Save();
        }
        return _assetExpectedRuntimeHoursProjectedForTestAssetPL121;
      }
    }
    public AssetExpectedRuntimeHoursProjected AssetExpectedRuntimeHoursProjectedForTestAssetPL321
    {
      get
      {
        if (_assetExpectedRuntimeHoursProjectedForTestAssetPL321 == null)
        {
          _assetExpectedRuntimeHoursProjectedForTestAssetPL321 = Entity.AssetExpectedRuntimeHoursProjected.ForAsset(TestAssetPL321).Save();
        }
        return _assetExpectedRuntimeHoursProjectedForTestAssetPL321;
      }
    }
    public AssetExpectedRuntimeHoursProjected AssetExpectedRuntimeHoursProjectedForTestAssetMTS521
    {
      get
      {
        if (_assetExpectedRuntimeHoursProjectedForTestAssetMTS521 == null)
        {
          _assetExpectedRuntimeHoursProjectedForTestAssetMTS521 = Entity.AssetExpectedRuntimeHoursProjected.ForAsset(TestAssetMTS521).Save();
        } 
        return _assetExpectedRuntimeHoursProjectedForTestAssetMTS521;
      }
    }
    public AssetExpectedRuntimeHoursProjected AssetExpectedRuntimeHoursProjectedForTestAssetMTS522
    {
      get
      {
        if (_assetExpectedRuntimeHoursProjectedForTestAssetMTS522 == null)
        {
          _assetExpectedRuntimeHoursProjectedForTestAssetMTS522 = Entity.AssetExpectedRuntimeHoursProjected.ForAsset(TestAssetMTS522).Save();
        }
        return _assetExpectedRuntimeHoursProjectedForTestAssetMTS522;
      }
    }

    #endregion

    #region ASSET BURN RATES

    private AssetBurnRates _assetBurnRatesForTestAssetPL121;
    private AssetBurnRates _assetBurnRatesForTestAssetPL321;
    private AssetBurnRates _assetBurnRatesForTestAssetMTS521;
    private AssetBurnRates _assetBurnRatesForTestAssetMTS522;

    public AssetBurnRates AssetBurnRatesForTestAssetPL121
    {
      get
      {
        if (_assetBurnRatesForTestAssetPL121 == null)
        {
          _assetBurnRatesForTestAssetPL121 = Entity.AssetBurnRates.ForAsset(TestAssetPL121).SyncWithRpt().Save();
        }
        return _assetBurnRatesForTestAssetPL121;
      }
    }
    public AssetBurnRates AssetBurnRatesForTestAssetPL321
    {
      get
      {
        if (_assetBurnRatesForTestAssetPL321 == null)
        {
          _assetBurnRatesForTestAssetPL321 = Entity.AssetBurnRates.ForAsset(TestAssetPL321).SyncWithRpt().Save();
        }
        return _assetBurnRatesForTestAssetPL321;
      }
    }
    public AssetBurnRates AssetBurnRatesForTestAssetMTS521
    {
      get
      {
        if (_assetBurnRatesForTestAssetMTS521 == null)
        {
          _assetBurnRatesForTestAssetMTS521 = Entity.AssetBurnRates.ForAsset(TestAssetMTS521).SyncWithRpt().Save();
        }
        return _assetBurnRatesForTestAssetMTS521;
      }
    }
    public AssetBurnRates AssetBurnRatesForTestAssetMTS522
    {
      get
      {
        if (_assetBurnRatesForTestAssetMTS522 == null)
        {
          _assetBurnRatesForTestAssetMTS522 = Entity.AssetBurnRates.ForAsset(TestAssetMTS522).SyncWithRpt().Save();
        }
        return _assetBurnRatesForTestAssetMTS522;
      }
    }

    #endregion

    #region ASSET EXPECTED HOURS PROJECTED

    private AssetWorkingDefinition _assetWorkingDefinitionForTestAssetPL121;
    private AssetWorkingDefinition _assetWorkingDefinitionForTestAssetPL321;
    private AssetWorkingDefinition _assetWorkingDefinitionForTestAssetMTS521;
    private AssetWorkingDefinition _assetWorkingDefinitionForTestAssetMTS522;

    public AssetWorkingDefinition AssetWorkingDefinitionForTestAssetPL121
    {
      get
      {
        if (_assetWorkingDefinitionForTestAssetPL121 == null)
        {
          _assetWorkingDefinitionForTestAssetPL121 = Entity.AssetWorkingDefinition.ForAsset(TestAssetPL121).SyncWithRpt().Save();
        }
        return _assetWorkingDefinitionForTestAssetPL121;
      }
    }
    public AssetWorkingDefinition AssetWorkingDefinitionForTestAssetPL321
    {
      get
      {
        if (_assetWorkingDefinitionForTestAssetPL321 == null)
        {
          _assetWorkingDefinitionForTestAssetPL321 = Entity.AssetWorkingDefinition.ForAsset(TestAssetPL321).SyncWithRpt().Save();
        }
        return _assetWorkingDefinitionForTestAssetPL321;
      }
    }
    public AssetWorkingDefinition AssetWorkingDefinitionForTestAssetMTS521
    {
      get
      {
        if (_assetWorkingDefinitionForTestAssetMTS521 == null)
        {
          _assetWorkingDefinitionForTestAssetMTS521 = Entity.AssetWorkingDefinition.ForAsset(TestAssetMTS521).SyncWithRpt().Save();
        }
        return _assetWorkingDefinitionForTestAssetMTS521;
      }
    }
    public AssetWorkingDefinition AssetWorkingDefinitionForTestAssetMTS522
    {
      get
      {
        if (_assetWorkingDefinitionForTestAssetMTS522 == null)
        {
          _assetWorkingDefinitionForTestAssetMTS522 = Entity.AssetWorkingDefinition.ForAsset(TestAssetMTS522).SyncWithRpt().Save();
        }
        return _assetWorkingDefinitionForTestAssetMTS522;
      }
    }

    #endregion

    #region SERVICES / SERVICE VIEWS

    #region MANUAL MAINTENANCE

    private Service _manualWatchNoDevice;

    /*
     *  Manual Watch service for No Device
     *  with service view for customer test dealer on Asset with No Device
     *  Add it to NH_RPT
     */

    public Service ManualWatchNoDevice
    {
      get 
      { 
        if(_manualWatchNoDevice == null)
        {
          _manualWatchNoDevice = Entity.Service.ManualWatch.ForDevice(TestNoDevice)
                                   .WithView(view => view.ForAsset(TestAssetNoDevice).ForCustomer(TestDealer))
                                   .SyncWithRpt().Save();
        }
        return _manualWatchNoDevice;
      }
    }

    #endregion

    #region ESSENTIALS

    private Service _essentialsPL121;
    private Service _essentialsPL321;
    private Service _essentialsMTS521;
    private Service _essentialsMTS522;
    private Service _essentialsMTS523;
    private Service _essentialsTAP66;
    private Service _essentialsSNM940;
    private Service _essentialsPL420;
    private Service _essentialsTrimTrac;
    private Service _essentialsPL421;
    private Service _essentialsPLE641;

    /* Essential service for PLE641 */

    public Service EssentialsPLE641
    {
      get
      {
        if (_essentialsPLE641 == null)
        {
          _essentialsPLE641 = Entity.Service.Essentials.ForDevice(TestPLE641)
                                .WithView(view => view.ForAsset(TestAssetPLE641).ForCustomer(TestDealer))                                
                                .SyncWithRpt().Save();
        }
        return _essentialsPLE641;
      }
    }
    
    /*
     *  Essential service for PL121 
     *  with service view for customer test dealer on Asset with PL121
     *  Add it to NH_RPT
     */

    public Service EssentialsPL121
    {
      get 
      {
        if (_essentialsPL121 == null)
        {
          _essentialsPL121 = Entity.Service.Essentials.ForDevice(TestPL121)
                               .WithView(view => view.ForAsset(TestAssetPL121).ForCustomer(TestDealer))
                               .SyncWithRpt().Save();
        }
        return _essentialsPL121;
      }
    }

    /*
     *  Essential service for PL321 
     *  with service view for customer test dealer on Asset with PL321
     *  Add it to NH_RPT
     */

    public Service EssentialsPL321
    {
      get 
      {
        if (_essentialsPL321 == null)
        {
          _essentialsPL321 = Entity.Service.Essentials.ForDevice(TestPL321)
                               .WithView(view => view.ForAsset(TestAssetPL321).ForCustomer(TestDealer))
                               .SyncWithRpt().Save();
        }
        return _essentialsPL321;
      }
    }

    /*
     *  Essential service for MTS521 
     *  with service view for Test Dealer on Asset with MTS521
     *  with service view for Test Customer on Asset with MTS521
     *  Add it to NH_RPT
     */

    public Service EssentialsMTS521 
    {
      get 
      {
        if (_essentialsMTS521 == null) 
        {
          _essentialsMTS521 = Entity.Service.Essentials.ForDevice(TestMTS521)
                                .WithView(view => view.ForAsset(TestAssetMTS521).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetMTS521).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _essentialsMTS521;
      }
    }

    /*
     *  Essential service for MTS522 
     *  with service view for Test Dealer on Asset with MTS522 
     *  with service view for Test Customer on Asset with MTS522 
     *  Add it to NH_RPT
     */

    public Service EssentialsMTS522 
    {
      get 
      {
        if (_essentialsMTS522 == null) 
        {
          _essentialsMTS522 = Entity.Service.Essentials.ForDevice(TestMTS522)
                                .WithView(view => view.ForAsset(TestAssetMTS522).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetMTS522).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _essentialsMTS522;
      }
    }

    /*
     *  Essential service for MTS523 
     *  with service view for Test Dealer on Asset with MTS523 
     *  with service view for Test Customer on Asset with MTS523 
     *  Add it to NH_RPT
     */

    public Service EssentialsMTS523
    {
      get 
      {
        if (_essentialsMTS523 == null) 
        {
          _essentialsMTS523 = Entity.Service.Essentials.ForDevice(TestMTS523)
                                .WithView(view => view.ForAsset(TestAssetMTS523).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetMTS523).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _essentialsMTS523;
      }
    }

    /*
     *  Essential service for TAP66
     *  with service view for Test Dealer on Asset with TAP66
     *  with service view for Test Customer on Asset with TAP66
     *  Add it to NH_RPT
     */

    public Service EssentialsTAP66
    {
      get
      {
        if (_essentialsTAP66 == null)
        {
          _essentialsTAP66 = Entity.Service.Essentials.ForDevice(TestTAP66)
                                .WithView(view => view.ForAsset(TestAssetTAP66).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetTAP66).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _essentialsTAP66;
      }
    }

    /*
 *  Essential service for SNM940 
 *  with service view for Test Dealer on Asset with MTS523 
 *  with service view for Test Customer on Asset with MTS523 
 *  Add it to NH_RPT
 */

    public Service EssentialsSNM940
    {
      get
      {
        if (_essentialsSNM940 == null)
        {
          _essentialsSNM940 = Entity.Service.Essentials.ForDevice(TestSNM940)
                                .WithView(view => view.ForAsset(TestAssetSNM940).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetSNM940).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _essentialsSNM940;
      }
    }

    /*
*  Essential service for PL420 
*  with service view for Test Dealer on Asset with PL420
*  with service view for Test Customer on Asset with PL420
*  Add it to NH_RPT
*/

    public Service EssentialsPL420
    {
      get
      {
        if (_essentialsPL420 == null)
        {
          _essentialsPL420 = Entity.Service.Essentials.ForDevice(TestPL420)
                                .WithView(view => view.ForAsset(TestAssetPL420).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetPL420).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _essentialsPL420;
      }
    }

    public Service EssentialsTrimTrac
    {
      get
      {
        if (_essentialsTrimTrac == null)
        {
          _essentialsTrimTrac = Entity.Service.Essentials.ForDevice(TestTrimTrac)
                                .WithView(view => view.ForAsset(TestAssetTrimTrac).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetTrimTrac).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _essentialsTrimTrac;
      }
    }

    public Service EssentialsPL421
    {
      get
      {
        if (_essentialsPL421 == null)
        {
          _essentialsPL421 = Entity.Service.Essentials.ForDevice(TestPL421)
                                .WithView(view => view.ForAsset(TestAssetPL421).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetPL421).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _essentialsPL421;
      }
    }
    #endregion
    
    #region HEALTH

    private Service _healthPL121;
    private Service _healthPL321;

    /*
     *  Healt service for PL121
     *  with service view for Test Dealer on Asset with PL121
     *  with service view for Test Customer on Asset with PL121
     *  Add it to NH_RPT
     */

    public Service HealthPL121
    {
      get 
      {
        if (_healthPL121 == null) 
        {
          _healthPL121 = Entity.Service.Essentials.ForDevice(TestPL121)
                                .WithView(view => view.ForAsset(TestAssetPL121).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetPL121).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _healthPL121;
      }
    }

    /*
     *  Health service for PL321
     *  with service view for TestDealer on Asset with PL321
     *  with service view for TestCustomer on Asset with PL321
     *  Add it to NH_RPT
     */

    public Service HealthPL321
    {
      get 
      {
        if (_healthPL321 == null)  

        {
          _healthPL321 = Entity.Service.Essentials.ForDevice(TestPL321)
                                .WithView(view => view.ForAsset(TestAssetPL321).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetPL321).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _healthPL321;
      }
    }

    #endregion

    #region MAINTENANCE

    private Service _maintenancePL321;

    public Service MaintenancePL321
    {
      get
      {
        if (_maintenancePL321 == null)
        {
          _maintenancePL321 = Entity.Service.Maintenance.ForDevice(TestPL321)
                                .WithView(view => view.ForAsset(TestAssetPL321).ForCustomer(TestDealer))
                                .WithView(view => view.ForAsset(TestAssetPL321).ForCustomer(TestCustomer).StartsOn(DateTime.UtcNow.AddMonths(-1)))
                                .SyncWithRpt().Save();
        }
        return _maintenancePL321;
      }
    }

    #endregion

    public void InitializeService()
    {
      // This is just for assignment...
      Service dummy; 

      dummy = ManualWatchNoDevice;

      dummy = EssentialsPL121;
      dummy = EssentialsPL321;
      dummy = EssentialsMTS521;
      dummy = EssentialsMTS522;
      dummy = EssentialsMTS523;
      dummy = EssentialsSNM940;

      dummy = HealthPL121;
      dummy = HealthPL321;

      dummy = MaintenancePL321;
    }

    #endregion

    #region SITES

    private Site _wholeWorldSite;
    private Site _broomfieldSite;

    public Site WholeWorldSite
    {
      get
      {
        if(_wholeWorldSite == null)
        {
          _wholeWorldSite = Entity.Site.ForCustomer(TestDealer).CreatedByUser(TestDealerUser)
                                .WithPoint(new Point(-90, -180))
                                .WithPoint(new Point(-90, 180))
                                .WithPoint(new Point(90, 180))
                                .WithPoint(new Point(90, -180)).Save();
        }
        return _wholeWorldSite;
      }
    }
    public Site BroomfieldSite 
    {
      get 
      {
        if (_broomfieldSite == null) 
        {
          _broomfieldSite = Entity.Site.ForCustomer(TestDealer).CreatedByUser(TestDealerUser)
                                  .WithPoint(new Point(39.9370510922951, -105.152618966406))
                                  .WithPoint(new Point(39.8959728974208, -105.153992257422))
                                  .WithPoint(new Point(39.8791131774735, -105.105927071875))
                                  .WithPoint(new Point(39.9149351259622, -105.079834542578)).Save();
        }
        return _broomfieldSite;
      }
    }
    
    #endregion

  }
}
