Feature: AccountHierarchyService
References:
			-https://docs.google.com/document/d/1Y_30fqzm9Okxlbm1w5GSsDopq_dvstjM7P0K6y-ktpE/edit	 
User Story: 
		22566 - MDM: VL NG MDM API 1.0/accounthierarchy endpoint to return hierarchial information from CustomerRelationshipNode table
		66257 - Enhance AccountHierarchy endpoint to provide account hierarchy info along with UCID and DC Information.



@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario Outline: AccountHierarchyWebAPI_GetHierarchyForDealerUserWithValidAsset_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForDealerUserWithValidAsset_HappyPath'
And User Is Created And Associated With '<DealerCount>' Dealer With Valid Asset
When I Perform Valid AccountHierarchyService User GetAccountHierarchy Request
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy

Examples: 
| Description     | DealerCount |
| SingleDealer    | 1           |
| Multiple Dealer | 3           |

@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario Outline: AccountHierarchyWebAPI_GetHierarchyForDealerUserWithoutValidAsset_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForDealerUserWithValidAsset_HappyPath'
And User Is Created And Associated With '<DealerCount>' Dealer Without Valid Asset
When I Perform Valid AccountHierarchyService User GetAccountHierarchy Request
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy

Examples: 
| Description     | DealerCount |
| SingleDealer    | 1           |
| Multiple Dealer | 3           |


@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario Outline: AccountHierarchyWebAPI_GetHierarchyForCustomerUserWithValidAsset_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForCustomerUser_HappyPath'
And User Is Created And Associated With '<CustomerCount>' Customer With Valid Asset
When I Perform Valid AccountHierarchyService User GetAccountHierarchy Request
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy

Examples: 
| Description     | CustomerCount |
| SingleDealer    | 1             |
| Multiple Dealer | 3             |

@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario: AccountHierarchyWebAPI_GetHierarchyForDealerUserWithMultipleAccounts_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForDealerUserWithMultipleAccounts_HappyPath'
And Account 'A1' is Associated With Dealer 'D1'
And Account 'A2' Is Associated With Dealer 'D1'
And User 'U1' Is Created And Associated With Dealer 'D1'
When I Perform GetAccountHierarchyByUserUID 
And I Perform GetAccountHierarchyByCustomerUID
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy
Then The GetAccountHierarchyByCustomerUID Response Should Return The Customer AccountHierarchy

@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario: AccountHierarchyWebAPI_GetHierarchyForCustomerUserWithMultipleAccounts_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForCustomerUserWithMultipleAccounts_HappyPath'
And Account 'A1' is Associated With Customer 'C1'
And Account 'A2' Is Associated With Customer 'C1'
And User 'U1' Is Created And Associated With Customer 'C1'
When I Perform GetAccountHierarchyByUserUID 
And I Perform GetAccountHierarchyByCustomerUID
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy
Then The GetAccountHierarchyByCustomerUID Response Should Return The Customer AccountHierarchy


@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario Outline: AccountHierarchyWebAPI_GetCustomerUserHierarchy_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForUserAssociatedWithDealerCustomers_HappyPath'
And User Is Created And Associated With '<DealerCount>' Dealer And '<CustomerCount>' Customer
When I Perform GetAccountHierarchyByUserUID 
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy


Examples: 
| Description                    | DealerCount | CustomerCount |
| SingleDealerSingleCustomer     | 1           | 1             |
| SingleDealerMultipleCustomer   | 1           | 2             |
| MultipleDealerSingleCustomer   | 2           | 1             |
| MultipleDealerMultipleCustomer | 2           | 2             |

@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario Outline: AccountHierarchyWebAPI_GetUserAssociatedWithDealerCustomersHierarchy_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetUserAssociatedWithDealerCustomersHierarchy_HappyPath'
And User Is Created And Associated With '<DealerCount>' Dealer And '<CustomerCount>' Customer
When I Perform GetAccountHierarchyByUserUID 
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy


Examples: 
| Description                    | DealerCount | CustomerCount |
| SingleDealerSingleCustomer     | 1           | 1             |
| SingleDealerMultipleCustomer   | 1           | 2             |
| MultipleDealerSingleCustomer   | 2           | 1             |
| MultipleDealerMultipleCustomer | 2           | 2             |

@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario: AccountHierarchyWebAPI_GetHierarchyForUserAssociatedWithDealerWithSubDealers_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForUserAssociatedWithDealerWithSubDealers_HappyPath'
And Dealer 'D2' is the Child Dealer Of 'D1'
And Dealer 'D3' is the Child Dealer Of 'D1'
And User 'U1' is Created And Associated With Dealer 'D1' 
When I Perform GetAccountHierarchyByUserUID 
And I Perform GetAccountHierarchyByCustomerUID
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy
Then The GetAccountHierarchyByCustomerUID Response Should Return The Customer AccountHierarchy

@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario: AccountHierarchyWebAPI_GetHierarchyForDealerUserWithDealerAssociatedWithMultipleCustomers_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForDealerUserWithDealerAssociatedWithMultipleCustomers_HappyPath'
And Customer 'C1' is Associated With Dealer 'D1'
And Customer 'C2'is Associated With Dealer 'D1'
And User 'U1' is Created And Associated With Dealer 'D1' 
When I Perform GetAccountHierarchyByUserUID 
And I Perform GetAccountHierarchyByCustomerUID
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy
Then The GetAccountHierarchyByCustomerUID Response Should Return The Customer AccountHierarchy

@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario: AccountHierarchyWebAPI_GetHierarchyForDealerUserWithDealerAssociatedWithMultipleCustomersAndSubDealers_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForDealerUserWithDealerAssociatedWithMultipleCustomersAndSubDealers_HappyPath'
And Customer 'C1' is Associated With Dealer 'D1'
And Customer 'C2'is Associated With Dealer 'D1'
And Dealer 'D2' is the Child Dealer Of 'D1'
And User 'U1' is Created And Associated With Dealer 'D1' 
When I Perform GetAccountHierarchyByUserUID 
And I Perform GetAccountHierarchyByCustomerUID
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy
Then The GetAccountHierarchyByCustomerUID Response Should Return The Customer AccountHierarchy







##############################################################################################################################################################


@Automated @Sanity @Positive
@AccountHierarchyWebApi @US22566
Scenario: AccountHierarchyWebApi_GetDealerUserHierarchy_HappyPath
	  Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebApi_GetDealerUserHierarchy_HappyPath'
		And User Created And Associated With '1' 'Dealer' Who Has '2' 'Customer'
	  When I Post Valid AccountHierarchyService GetAccountHierarchy Request  
	  Then The GetAccountHierarchy Response Should Return The UserAccountHierarchy

@Automated @Regression @Positive
@AccountHierarchyWebApi @US22566
Scenario: AccountHierarchyWebApi_GetCustomerUserHierarchy_HappyPath
	  Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebApi_GetCustomerUserHierarchy_HappyPath'
		And User Created And Associated With '3' 'Customer'
	  When I Post Valid AccountHierarchyService GetAccountHierarchy Request  
	  Then The GetAccountHierarchy Response Should Return The UserAccountHierarchy

@Automated @Regression @Positive
@AccountHierarchyWebApi @US22566
Scenario: AccountHierarchyWebApi_UserUnderTwoDealersWithCustomers
	  Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebApi_UserUnderTwoDealersWithCustomers'
		And User Created And Associated With '2' 'Dealer' Who Has '2' 'Customer'
	  When I Post Valid AccountHierarchyService GetAccountHierarchy Request  
	  Then The GetAccountHierarchy Response Should Return The UserAccountHierarchy

@Automated @Regression @Positive
@AccountHierarchyWebApi @US22566
Scenario: AccountHierarchyWebApi_UserUnderDealerWithSubDealer
	  Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebApi_UserUnderTwoDealersWithCustomers'
		And User Is Associated With '1' 'Dealer' Who Has '2' 'Dealer'
	  When I Post Valid AccountHierarchyService GetAccountHierarchy Request  
	  Then The GetAccountHierarchy Response Should Return The UserAccountHierarchy

 Scenario: AccountHierarchyWebApi_SimpleAccountHierarchyHappyPath
# Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebApi_SimpleAccountHierarchyHappyPath'
When I Set 'Dealer1' Has 'Dealer2' And 'Customer1' As Children 
#And I Set 'Dealer2' With 'Customer2' As Child
And I Remove 'Dealer2' From 'Dealer1' As Child
#When I Post Valid AccountHierarchyService GetAccountHierarchy Request  
Then The AccountHierarchyRelationship Should Stored In VSSDB
# And The GetAccountHierarchy Response Should Return The AccountHierarchyRelationship


Scenario:AccountHierarchyWebApi_SimpleAccountHierarchyDeleteHappyPath
#Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebApi_SimpleAccountHierarchyHappyPath'
When I Set 'Dealer1' As RootNode And Has 'Dealer2' And 'Customer1' As Children 
And I Set 'Dealer2' Has 'Customer1' And 'Dealer4' As Children
And I Set 'Dealer1'  Has 'Dealer3' As Children
And I Set 'Dealer4'  Has 'Customer1' As Children
And I Remove 'Dealer2' From 'Dealer1' As Child
And I Set 'Dealer1' With Parent 'Dealer1' LeftNode As '1' And RightNode As '6'
And I Set 'Dealer2' With Parent 'Dealer2' LeftNode As '1' And RightNode As '8'
And I Set 'Dealer3' With Parent 'Dealer1' LeftNode As '4' And RightNode As '5'
And I Set 'Customer1' With Parent 'Dealer1' LeftNode As '2' And RightNode As '3'
And I Set 'Customer1' With Parent 'Dealer2' LeftNode As '2' And RightNode As '3'
And I Set 'Customer1' With Parent 'Dealer4' LeftNode As '5' And RightNode As '6'
And I Set 'Dealer4' With Parent 'Dealer2' LeftNode As '4' And RightNode As '7'
#When I Post Valid AccountHierarchyService GetAccountHierarchy Request  

Then The AccountHierarchyRelationship Should Stored In VSSDB


