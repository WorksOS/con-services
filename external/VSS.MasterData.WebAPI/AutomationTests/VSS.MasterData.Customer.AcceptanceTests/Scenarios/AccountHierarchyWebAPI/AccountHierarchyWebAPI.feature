Feature: AccountHierarchyWebAPI
References:
			-https://docs.google.com/document/d/1Y_30fqzm9Okxlbm1w5GSsDopq_dvstjM7P0K6y-ktpE/edit	 
User Story: 
		66257 - Enhance AccountHierarchy endpoint to provide account hierarchy info along with UCID and DC Information.

@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario Outline: AccountHierarchyWebAPI_GetHierarchyForUser_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForUser_HappyPath'
And User Is Created And Associated With '<CustomerCount>' '<CustomerType>' With '<HasValidCustomerAsset>'
When I Perform GetAccountHierarchyByUserUID
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy

Examples: 
| Description                  | CustomerCount | CustomerType | HasValidCustomerAsset |
| SingleDealerValidAsset       | 1             | Dealer       | True                  |
| MultipleDealerValidAsset     | 3             | Dealer       | True                  |
| SingleCustomerValidAsset     | 1             | Customer     | True                  |
| MultipleCustomerValidAsset   | 3             | Customer     | True                  |
| SingleDealerInvalidAsset     | 1             | Dealer       | False                 |
| MultipleDealerInvalidAsset   | 3             | Dealer       | False                 |
| SingleCustomerInvalidAsset   | 1             | Customer     | False                 |
| MultipleCustomerInvalidAsset | 3             | Customer     | False                 |






@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario: AccountHierarchyWebAPI_GetHierarchyForDealerWithSingleCustomer_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForDealerWithSingleCustomer_HappyPath'
And 'Customer' 'Customer1' Is Associated With 'Dealer' 'Dealer1' 
And User Is Created And Associated To 'Dealer' 'Dealer1'
When I Perform GetAccountHierarchyByUserUID 
And I Perform GetAccountHierarchyByCustomerUID
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy
#Then The GetAccountHierarchyByCustomerUID Response Should Return The Customer AccountHierarchy

@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario Outline: AccountHierarchyWebAPI_GetHierarchyForDealerWithCustomerHavingMultipleAccounts_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForDealerWithCustomerHavingMultipleAccounts_HappyPath'
And 'Customer1' Has '2' Accounts With '<UCID>'
And 'Customer' 'Customer1' Is Associated With 'Dealer' 'Dealer1'
And User Is Created And Associated To 'Dealer' 'Dealer1'
When I Perform GetAccountHierarchyByUserUID 
And I Perform GetAccountHierarchyByCustomerUID
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy
#Then The GetAccountHierarchyByCustomerUID Response Should Return The Customer AccountHierarchy
Examples: 
| Description    | UCID          |
| Same UCID      | SameUCID      |
| Different UCID | DifferentUCID |

@Automated @Sanity @Positive
@Account @HierarchyWebAPI @US66257
Scenario Outline: AccountHierarchyWebAPI_GetHierarchyForDealerWithSubDealerAndCustomer_HappyPath
Given AccountHierarchyWebApi Is Ready To Verify 'AccountHierarchyWebAPI_GetHierarchyForDealerWithSubDealerAndCustomer_HappyPath'
And 'Dealer' 'Dealer1' Is Associated With 'Dealer' 'Dealer2'
And 'Customer1' Has '2' Accounts With '<UCID>'
And 'Customer' 'Customer1' Is Associated With 'Dealer' 'Dealer2'
And User Is Created And Associated To 'Dealer' 'Dealer1'
#When I Perform GetAccountHierarchyByUserUID 
And I Perform GetAccountHierarchyByCustomerUID
Then The GetAccountHierarchyByUserUID Response Should Return The User AccountHierarchy
#Then The GetAccountHierarchyByCustomerUID Response Should Return The Customer AccountHierarchy
Examples: 
| Description    | UCID          |
| Same UCID      | SameUCID      |
| Different UCID | DifferentUCID |