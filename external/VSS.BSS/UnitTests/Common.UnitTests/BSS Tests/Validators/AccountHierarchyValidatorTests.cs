using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
    [TestClass]
    public class AccountHierarchyValidatorTests : BssUnitTestBase
    {
        protected AccountHierarchyValidator Validator;

        [TestInitialize]
        public void AccountHierarchyValidatorTests_Init()
        {
            Validator = new AccountHierarchyValidator();
        }

        [TestCleanup]
        public void AccountHierarchyMessageValidatorTests_Init()
        {
            if (Validator == null) return;

            Console.WriteLine(Validator.Warnings.ToFormattedString());
            Console.WriteLine();
            Console.WriteLine(Validator.Errors.Select(x => x.Item2).ToFormattedString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Validate_NullMessage_InvalidOperationException()
        {
            Validator.Validate(null);
        }

        /*
        * When Dealer CustomerType message is valid
        * Then no Warnings And no Errors
        */
        [TestMethod]
        public void Validate_Dealer_MessageIsValid_NoWarningsAndNoErrors()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
        }

        /*
        * When Customer message is valid
        * Then no Warnings And no Errors
        */
        [TestMethod]
        public void Validate_Customer_MessageIsValid_NoWarningsAndNoErrors()
        {
            var message = BSS.AHCreated.ForCustomer().ContactDefined().Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
        }

        /*
        * When Account message is valid
        * Then no Warnings And no Errors
        */
        [TestMethod]
        public void Validate_Account_MessageIsValid_NoWarningsAndNoErrors()
        {
            var message = BSS.AHCreated.ForAccount().Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
        }

        #region General Validations

        /*
		*  SequenceNumber is not defined
		*/
        [TestMethod]
        public void Validate_SequenceNumberNotDefined_ErrorMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().SequenceNumber(0).Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.SEQUENCE_NUMBER_NOT_DEFINED, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.SequenceNumberNotDefined, Validator.FirstFailureCode());
        }

        /*
            * ControlNumber is not defined
            */
        [TestMethod]
        public void Validate_ControlNumberNotDefined_ErrorMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().ControlNumber(string.Empty).Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.CONTROL_NUMBER_NOT_DEFINED, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.ControlNumberNotDefined, Validator.FirstFailureCode());
        }

        #endregion

        #region PrimaryContact Validations

        /*
           When PrimaryContact is not defined for Create      
            */
        [TestMethod]
        public void Validate_PrimaryContactNotDefinedForCreate_ErrorMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactNotDefined().Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.PRIMARY_CONTACT_NOT_DEFINED, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.PrimaryContactInvalid, Validator.FirstFailureCode());
        }

        /*
           When PrimaryContact is not defined for Update      
            */
        [TestMethod]
        public void Validate_PrimaryContactNotDefinedForUpdate_NoWarningsAndNoErrors()
        {
            var message = BSS.AHUpdated.ForDealer().ContactNotDefined().Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
        }

        /*
           When PrimaryContact is defined for Update      
            */
        [TestMethod]
        public void Validate_ValidPrimaryContactDefinedForUpdate_NoWarningsAndNoErrors()
        {
            var message = BSS.AHUpdated.ForDealer().ContactDefined().Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
        }

        /*
    * When PrimaryContact is defined
    * And is FirstName is not defined 
    */
        [TestMethod]
        public void Validate_PrimaryContactDefined_FirstNameNotDefined_ErrorMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().FirstName("").Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.PRIMARY_CONTACT_FIRST_NAME_NOT_DEFINED, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.PrimaryContactInvalid, Validator.FirstFailureCode());
        }

        /*
            * When PrimaryContact is defined
        * And is LastName is not defined 
            */
        [TestMethod]
        public void Validate_PrimaryContactDefined_LastNameNotDefined_ErrorMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().LastName("").Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.PRIMARY_CONTACT_LAST_NAME_NOT_DEFINED, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.PrimaryContactInvalid, Validator.FirstFailureCode());
        }

        /*
            * When PrimaryContact is defined
        * And is Email is not defined 
            */
        [TestMethod]
        public void Validate_PrimaryContactDefined_EmailNotDefined_ErrorMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().Email("").Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.PRIMARY_CONTACT_EMAIL_NOT_DEFINED, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.PrimaryContactInvalid, Validator.FirstFailureCode());
        }

        /*
        * When PrimaryContact is defined
      * And Email with domain lesser than Minimum length(2) is defined 
        */
        [TestMethod]
        public void Validate_PrimaryContactDefined_EmailDomainSectionLesserThanMinimumLength_ErrorMessage()
        {
            string email = "abcd123@xyz.c";
            var message = BSS.AHCreated.ForDealer().ContactDefined().Email(email).Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(string.Format(BssConstants.Hierarchy.PRIMARY_CONTACT_EMAIL_INVALID, email), Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.PrimaryContactInvalid, Validator.FirstFailureCode());
        }

        /*
         * When PrimaryContact is defined
     * And Email with lesser than Minimum length(6) is defined 
         */
        [TestMethod]
        public void Validate_PrimaryContactDefined_EmailLesserThanMinimumLength_ErrorMessage()
        {
            string email = "a@a.c";
            var message = BSS.AHCreated.ForDealer().ContactDefined().Email(email).Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(string.Format(BssConstants.Hierarchy.PRIMARY_CONTACT_EMAIL_INVALID, email), Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.PrimaryContactInvalid, Validator.FirstFailureCode());
        }
      
        /*
          * When PrimaryContact is defined
      * And Email with invalid special characters is defined 
          */
        [TestMethod]
        public void Validate_PrimaryContactDefined_EmailDefinedWithInvalidSpecialCharacter_ErrorMessage()
        {
            string email = "abc*d@domain.com";
            var message = BSS.AHCreated.ForDealer().ContactDefined().Email(email).Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(string.Format(BssConstants.Hierarchy.PRIMARY_CONTACT_EMAIL_INVALID, email), Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.PrimaryContactInvalid, Validator.FirstFailureCode());
        }

        /*
               * When PrimaryContact is defined
        * And Email with valid special characters is defined 
            */
        [TestMethod]
        public void Validate_ValidPrimaryContactDefined_EmailDefinedWithValidSpecialCharacter_NoWarningsAndNoErrors()
        {
            string email = "a_bc'd@domain.com";
            var message = BSS.AHCreated.ForDealer().ContactDefined().Email(email).Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
        }

        #endregion


        #region AccountHierarchy Validations

        /*
		* When Action is invalid
    * Note: This is really implemented in the InvalidActionWorkflow
    * but reporduced here for good measure
		*/
        [TestMethod]
        public void Validate_ActionInvalid_ErrorMessage()
        {
            var message = BSS.AH(ActionEnum.Swapped).ForDealer().Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(string.Format(BssConstants.ACTION_INVALID_FOR_MESSAGE, ActionEnum.Swapped, typeof(AccountHierarchy).Name), Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.ActionInvalid, Validator.FirstFailureCode());
        }


        /*
            * When CustomerName not is defined
            */
        [TestMethod]
        public void Validate_CustomerNameNotDefined_ErrorMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().Name("").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.CUSTOMER_NAME_NOT_DEFINED, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.CustomerNameNotDefined, Validator.FirstFailureCode());
        }

        /*
             * When BSSID is not defined
             */
        [TestMethod]
        public void Validate_BssIdNotDefined_ErrorMessage()
        {
            var message = BSS.AHCreated.ContactDefined().ForDealer().BssId(null).Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.BSSID_NOT_DEFINED, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.BssIdNotDefined, Validator.FirstFailureCode());
        }

        /*
         * When ParentBSSID is defined
         * And Relationship is not defined
         */
        [TestMethod]
        public void Validate_ParentBssIdDefined_RelationshipIdNotDefined_ErrorMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined()
              .ParentBssId(IdGen.GetId().ToString())
              .RelationshipId(null)
              .Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.RELATIONSHIPID_NOT_DEFINED, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.RelationshipIdNotDefined, Validator.FirstFailureCode());
        }

        /*
            * When Relationship is defined
        * And ParentBSSID is not defined
            */
        [TestMethod]
        public void Validate_RelationshipIdDefined_ParentBssIdNotDefined_ErrorMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined()
              .ParentBssId(null)
              .RelationshipId(IdGen.GetId().ToString())
              .Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.PARENT_BSSID_NOT_DEFINED, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.ParentBssIdNotDefined, Validator.FirstFailureCode());

        }

        /*
             * CustomerType is invalid
         * Currently this is an enum so ignored.
             */
        [Ignore]
        [TestMethod]
        public void Validate_CustomerTypeIsInvalid_ErrorMessage()
        {
            AccountHierarchy message = null;
            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.CUSTOMER_TYPE_INVALID, Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.CustomerTypeInvalid, Validator.FirstFailureCode());
        }

        #endregion


        #region Dealer CustomerType Validations

        /*
		* When CustomerType is Dealer
    * And HierarchyType is not TCS Dealer
		*/
        [TestMethod]
        public void Validate_Dealer_HierarchyTypeNotTcsDealer_ErrorMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().HierarchyType("NOT_VALID").Build();

            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(string.Format(BssConstants.Hierarchy.HIERARCHY_TYPE_INVALID, "NOT_VALID"), Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.HierarchyTypeInvalid, Validator.FirstFailureCode());
        }

        /*
            * When CustomerType is Dealer 
        * And DealerNetwork is not defined
         * Warning
            */
        [TestMethod]
        public void Validate_Dealer_DealerNetworkNotDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().DealerNetwork("").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.DEALER_NETWORK_NOT_DEFINED, Validator.Warnings[0]);
        }

        /*
            * When CustomerType is Dealer 
        * And DealerNetwork is invalid
            */
        [Ignore]
        [TestMethod]
        public void Validate_Dealer_DealerNetworkInvalid_ErrorMessage()
        {
            throw new NotImplementedException();
        }

        /*
            * When CustomerType is Dealer 
        * And NetworkDealerCode is not defined
        * Warning
            */
        [TestMethod]
        public void Validate_Dealer_NetworkDealerCodeNotDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().NetworkDealerCode("").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(BssConstants.Hierarchy.NETWORK_DEALER_CODE_NOT_DEFINED, Validator.Warnings[0]);
        }

        /*
            * When CustomerType is Dealer
        * And NetworkCustomerCode is defined
         * Warning
            */
        [TestMethod]
        public void Validate_Dealer_NetworkCustomerCodeDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().NetworkCustomerCode("IS_DEFINED").Build();

            Validator.Validate(message);

            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.NETWORK_CUSTOMER_CODE_DEFINED, Validator.Warnings[0]);
        }

        /*
            * When CustomerType is Dealer 
        * And  DealerAccountCode is not defined
         * Warning
            */
        [TestMethod]
        public void Validate_Dealer_DealerAccountCodeDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForDealer().ContactDefined().DealerAccountCode("IS_DEFINED").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.DEALER_ACCOUNT_CODE_DEFINED, Validator.Warnings[0]);
        }

        #endregion


        #region Customer CustomerType Validations

        /*
		* When CustomerType is Customer
    * And ParentBSSID is defined
		*/
        [TestMethod]
        public void Validate_Customer_ParentBssIdDefined_NoErrorMessage()
        {
            var message = BSS.AHCreated.ForCustomer().ContactDefined().ParentDefined().Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            //Assert.AreEqual(1, Validator.Errors.Count);
            //Assert.AreEqual(BssConstants.Hierarchy.PARENT_BSSID_DEFINED, Validator.Errors[0].Item2);
            //Assert.AreEqual(BssFailureCode.ParentBssIdDefined, Validator.FirstFailureCode());
        }

        /*
        * When CustomerType is Customer
        * And HierarchyType is not TCS Customer
        */
        [TestMethod]
        public void Validate_Customer_HierarchyTypeNotTcsCustomer_ErrorMessage()
        {
            var message = BSS.AHCreated.ForCustomer().ContactDefined().HierarchyType("NOT_VALID").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(string.Format(BssConstants.Hierarchy.HIERARCHY_TYPE_INVALID, "NOT_VALID"), Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.HierarchyTypeInvalid, Validator.FirstFailureCode());
        }

        /*
            * When CustomerType is Customer
        * And DealerNetwork is defined
         * Warning
            */
        [TestMethod]
        public void Validate_Customer_DealerNetworkDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForCustomer().ContactDefined().DealerNetwork("IS_DEFINED").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.DEALER_NETWORK_DEFINED, Validator.Warnings[0]);
        }

        /*
            * When CustomerType is Customer
        * And NetworkDealerCode is defined
         * Warning
            */
        [TestMethod]
        public void Validate_Customer_NetworkDealerCodeDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForCustomer().ContactDefined().NetworkDealerCode("IS_DEFINED").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.NETWORK_DEALER_CODE_DEFINED, Validator.Warnings[0]);
        }

        /*
            * When CustomerType is Customer
        * And NetworkCustomerCode is defined
         * Warning
            */
        [TestMethod]
        public void Validate_Customer_NetworkCustomerCodeDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForCustomer().ContactDefined().NetworkCustomerCode("IS_DEFINED").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.NETWORK_CUSTOMER_CODE_DEFINED, Validator.Warnings[0]);
        }

        /*
            * When CustomerType is Customer
        * And DealerAccountCode is defined
         * Warning
            */
        [TestMethod]
        public void Validate_Customer_DealerAccountCodeDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForCustomer().ContactDefined().DealerAccountCode("IS_DEFINED").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.DEALER_ACCOUNT_CODE_DEFINED, Validator.Warnings[0]);
        }

        #endregion


        #region Account CustomerType Validations

        /*
	 * When CustomerType is Account
     * And HierarchyType is not TCS Dealer or TCS Customer
	 */
        [TestMethod]
        public void Validate_Account_HierarchyTypeInvalid_ErrorMessage()
        {
            var message = BSS.AHCreated.ForAccount().HierarchyType("NOT_VALID").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Warnings.Count);
            Assert.AreEqual(1, Validator.Errors.Count);
            Assert.AreEqual(string.Format(BssConstants.Hierarchy.HIERARCHY_TYPE_INVALID, "NOT_VALID"), Validator.Errors[0].Item2);
            Assert.AreEqual(BssFailureCode.HierarchyTypeInvalid, Validator.FirstFailureCode());
        }

        /*
            * When CustomerType is Account 
        * And DealerNetwork is defined
         * Warning
            */
        [TestMethod]
        public void Validate_Account_DealerNetworkDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForAccount().DealerNetwork("IS_DEFINED").Build();

            Validator.Validate(message);

            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.DEALER_NETWORK_DEFINED, Validator.Warnings[0]);
        }

        /*
            * When CustomerType is Account
        * And NetworkDealerCode is defined
         * Warning
            */
        [TestMethod]
        public void Validate_Account_NetworkDealerCodeDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForAccount().NetworkDealerCode("IS_DEFINED").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(BssConstants.Hierarchy.NETWORK_DEALER_CODE_DEFINED, Validator.Warnings[0]);
        }

        /*
            * When CustomerType is Account
        * And NetworkCustomerCode is not defined
        * Warning
            */
        [TestMethod]
        public void Validate_Account_NetworkCustomerCodeNotDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForAccount().NetworkCustomerCode("").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(BssConstants.Hierarchy.NETWORK_CUSTOMER_CODE_NOT_DEFINED, Validator.Warnings[0]);
        }

        /*
            * When CustomerType is Account
        * And DealerAccountCode is not defined
        * Warning
            */
        [TestMethod]
        public void Validate_Account_DealerAccountCodeNotDefined_WarningMessage()
        {
            var message = BSS.AHCreated.ForAccount().DealerAccountCode("").Build();

            Validator = new AccountHierarchyValidator();
            Validator.Validate(message);

            Assert.AreEqual(0, Validator.Errors.Count);
            Assert.AreEqual(1, Validator.Warnings.Count);
            Assert.AreEqual(BssConstants.Hierarchy.DEALER_ACCOUNT_CODE_NOT_DEFINED, Validator.Warnings[0]);
        }

        #endregion

    }
}
