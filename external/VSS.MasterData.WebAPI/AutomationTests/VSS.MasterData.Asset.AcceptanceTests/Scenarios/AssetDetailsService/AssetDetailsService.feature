Feature: AssetDetailsService

	References : A. Contract Document - None
	User Story 31220 : Support APP: AssetDetailAPI to return Assets and its related information for a Support User
#___________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@AssetSearchService @US31220
Scenario: AssetDetailService_HappyPathAssetUID
	Given AssetDetailService Is Ready To Verify 'AssetDetailService_HappyPathAssetUID'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Post Valid CustomerServiceCreate Request
	And DeviceAssetAssociation Request Is Setup With Default Values
	When I Post Valid DeviceAssetAssociation Request
	And AssetCustomerAssociation Request Is Setup With Default Values
	When I Post Valid AssetCustomerAssociation Request
	And AssetSubscription Request Is Setup With Default Values
	When I Post Valid AssetSubscription Request
	And I Get the AssetDetails For AssetUID
	Then AssetDetails Response With All information Should Be Returned

@Automated @Sanity @Positive
@AssetSearchService @US31220
Scenario: AssetDetailService_HappyPathDeviceUID
	Given AssetDetailService Is Ready To Verify 'AssetDetailService_HappyPathDeviceUID'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Post Valid CustomerServiceCreate Request
	And DeviceAssetAssociation Request Is Setup With Default Values
	When I Post Valid DeviceAssetAssociation Request
	And AssetCustomerAssociation Request Is Setup With Default Values
	When I Post Valid AssetCustomerAssociation Request
	And AssetSubscription Request Is Setup With Default Values
	When I Post Valid AssetSubscription Request
	And I Get the AssetDetails For DeviceUID
	Then AssetDetails Response With All information Should Be Returned

@Automated @Sanity @Positive
@AssetSearchService @US31220
Scenario: AssetDetailService_HappyPathAssetUIDDeviceUID
	Given AssetDetailService Is Ready To Verify 'AssetDetailService_HappyPathAssetUIDDeviceUID'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Post Valid CustomerServiceCreate Request
	And DeviceAssetAssociation Request Is Setup With Default Values
	When I Post Valid DeviceAssetAssociation Request
	And AssetCustomerAssociation Request Is Setup With Default Values
	When I Post Valid AssetCustomerAssociation Request
	And AssetSubscription Request Is Setup With Default Values
	When I Post Valid AssetSubscription Request
	And I Get the AssetDetails For AssetUID And DeviceUID
	Then AssetDetails Response With All information Should Be Returned

@Automated @Sanity @Positive
@AssetSearchService @US31220
Scenario: AssetDetailService_WithoutAccountAndSubscription
	Given AssetDetailService Is Ready To Verify 'AssetDetailService_WithoutAccountAndSubscription'
	And DeviceAssetAssociation Request Is Setup With Default Values
	When I Post Valid DeviceAssetAssociation Request
	And I Get the AssetDetails For AssetUID
	Then AssetDetails Response With Only Asset And Device Information Should Be Returned

@Automated @Sanity @Positive
@AssetSearchService @US31220
Scenario: AssetDetailService_WithoutSubscription
	Given AssetDetailService Is Ready To Verify 'AssetDetailService_WithoutSubscription'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Post Valid CustomerServiceCreate Request
	And DeviceAssetAssociation Request Is Setup With Default Values
	When I Post Valid DeviceAssetAssociation Request
	And AssetCustomerAssociation Request Is Setup With Default Values
	When I Post Valid AssetCustomerAssociation Request
	And I Get the AssetDetails For AssetUID
	Then AssetDetails Response With Asset, Device And Account Information Should Be Returned

@Automated @Sanity @Positive
@AssetSearchService @US31220
Scenario Outline: AssetDetailService_AccountInformation
	Given AssetDetailService Is Ready To Verify 'AssetDetailService_AccountInformation_<Description>'
	And CustomerServiceCreate Request Is Setup for '<Association>'
	When I Post Valid CustomerServiceCreate Request
	And DeviceAssetAssociation Request Is Setup for '<Association>'
	When I Post Valid DeviceAssetAssociation Request
	And AssetCustomerAssociation Request Is Setup for Asset With '<Association>'
	When I Post Valid AssetCustomerAssociation Request
	And I Get the AssetDetails For '<Association>'
	Then AssetDetails Response With Account information '<Result>' Should Be Returned
Examples: 
| Description                       | Association            | Result                 |
| DealerAssociation                 | Dealer                 | Dealer                 |
| CustomerAssociation               | Customer               | Customer               |
| CustomerDealerAssociation         | CustomerDealer         | CustomerDealer         |
| CustomerAssociationWithSameDealer | CustomerWithSameDealer | CustomerWithSameDealer |

@Automated @Sanity @Positive
@AssetSearchService @US31220
Scenario Outline: AssetDetailService_Subscription
	Given AssetDetailService Is Ready To Verify 'AssetDetailService_Subscription_<Description>'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Post Valid CustomerServiceCreate Request
	And DeviceAssetAssociation Request Is Setup With Default Values
	When I Post Valid DeviceAssetAssociation Request
	And AssetCustomerAssociation Request Is Setup With Default Values
	When I Post Valid AssetCustomerAssociation Request
	And AssetSubscription Request Is Setup For '<Description>', '<Subscription>', '<ActiveState>' And '<CustomerType>'
	When I Post Valid AssetSubscription Request
	And I Get the AssetDetails For AssetUID
	Then AssetDetails Response With All Information '<Result>' Should Be Returned
Examples: 
| Description          | Subscription          | ActiveState     | CustomerType    | Result               |
| OneActive_Customer   | Essentials            | Active          | Customer        | OneActive_Customer   |
| OneActive_Dealer     | Essentials            | Active          | Dealer          | OneActive_Dealer     |
| OneInactive_Customer | Essentials            | Inactive        | Customer        | OneInactive_Customer |
| OneInactive_Dealer   | Essentials            | Inactive        | Dealer          | OneInactive_Dealer   |
| TwoActive            | Essentials,CAT Health | Active          | Customer,Dealer | TwoActive            |
| TwoInactive          | Essentials,CAT Health | Inactive        | Customer,Dealer | TwoInactive          |
| OneActiveOneInactive | Essentials,CAT Health | Active,Inactive | Customer,Dealer | OneActiveOneInactive |

@Automated @Sanity @Positive
@AssetSearchService @US31220
Scenario: AssetDetailService_WrongAssetAndDevice
	Given AssetDetailService Is Ready To Verify 'AssetDetailService_WrongAssetAndDevice'
	And CustomerServiceCreate Request Is Setup For AssetAndDevice Verification
	When I Post Valid CustomerServiceCreate Request
	And DeviceAssetAssociation Request Is Setup For AssetAndDevice Verification
	When I Post Valid DeviceAssetAssociation Request
	And AssetCustomerAssociation Request Is Setup For AssetAndDevice Verification
	When I Post Valid AssetCustomerAssociation Request
	And AssetSubscription Request Is Setup For AssetAndDevice Verification
	When I Post Valid AssetSubscription Request
	And I Get the AssetDetails For 'AssetAndDevice'
	Then AssetDetails Response With All Information With AssetAndDevice Should Be Returned

@Automated @Sanity @Negative
@AssetSearchService @US31220
Scenario Outline: AssetDetailService_Invalid
	Given AssetDetailService Is Ready To Verify 'AssetDetailService_Invalid_<Description>'
	And I Get the AssetDetails For '<AssetUID>' And '<DeviceUID>'
	Then AssetDetails Response With '<Result>' Should Be Returned
Examples:
| Description       | AssetUID | DeviceUID | Result                     |
| NoParameters      | EMPTY    | EMPTY     | ERR_Message_EMPTY          |
| DeviceUID         | EMPTY    | Invalid   | ERR_Message_DeviceUID      |
| AssetUID          | Invalid  | EMPTY     | ERR_Message_AssetUID       |
| AssetUIDDeviceUID | Invalid  | Invalid   | ERR_Message_AssetDeviceUID |