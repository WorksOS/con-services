Feature: Subscription List WebApi

   References : https://docs.google.com/document/d/18zWcH34qTGne3rmqbDgqQ8Oh3ochUFJkrpRJfJ1D6vQ/edit
                           
   Dependencies:  Internal -  Kafka Topic
							  VSS DB	 - VSS-MasterData-Subscription
							      Tables - Asset Subscription
										   Project Subscription
										   Customer Subscription

   User Story 12099:Subscription List WebAPI (Master Data Management)
#______________________________________________________________________________________________________________________________________________________
@Automated @Sanity @Positive
@SubscriptionListWebApi @US12099
Scenario: GetSubscriptionDetailsCustomerContext_HappyPath
	  Given SubscriptionListWebApi Is Ready To Verify 'GetSubscriptionDetailsCustomerContext_HappyPath'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService AssetSubscriptionCreate Request 
	    And I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details For The Customer

@Automated @Regression @Positive
@SubscriptionListWebApi @US12099
Scenario: GetSubscriptionDetailsCustomerContext_AfterAssetSubscriptionUpdate
	  Given SubscriptionListWebApi Is Ready To Verify 'GetSubscriptionDetailsCustomerContext_AfterAssetSubscriptionUpdate'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService AssetSubscriptionUpdate Request 
	  	And I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription UpdatedDetails For The Customer

#@Automated @Regression @Positive
#@SubscriptionListWebApi @US12099
#Scenario: SubscriptionListWebApi_Customer_SameAssetDifferentSubscriptionTypes
#      Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_Customer_SameAssetDifferentSubscriptionTypes'
#         And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values
#      When I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionType To 'Essentials'
#         And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
#	     And I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionType To 'CAT Health'
#	     And I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionUID To Unique UID
#         And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
#	     And I Post Valid SubscriptionService Read Request For The Customer
#	  Then The SubscriptionServiceRead Response should return the MultipleAsset Subscription Details For The Customer With SubscriptionType as 'Essentials' and 'CAT Health'

@Automated @Regression @Positive
@SubscriptionListWebApi @US12099
Scenario: GetSubscriptionDetailsCustomerContext_MultipleSubscriptionsForAsset
      Given SubscriptionListWebApi Is Ready To Verify 'GetSubscriptionDetailsCustomerContext_MultipleSubscriptionsForAsset'
         And Multiple Subscriptions exist for an asset 
      When I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the MultipleAsset Subscription Details For The Customer

@Automated @Regression @Positive
@SubscriptionListWebApi @US12099
Scenario: GetSubscriptionDetailsCustomerContext_CustomerWithMultipleAssets
     Given SubscriptionListWebApi Is Ready To Verify 'GetSubscriptionDetailsCustomerContext_CustomerWithMultipleAssets'
		And SubscriptionType 'Essentials' has been setup for multiple assets under a customer
	 When I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To 'Min Date' For First Asset
        And I Set Valid SubscriptionService AssetSubscriptionCreate Request EndDate To 'Max Date' For Second Asset
	    And I Post Valid SubscriptionService Create Request For Multiple Asset Subscriptions
	    And I Post Valid SubscriptionService Read Request For The Customer
	 Then The GetSubscriptionDetailsCustomerContext should return the Subscription Details With Start Date as 'Min Date' and End Date as 'Max Date'

#@Automated @Regression @Positive
#@SubscriptionListWebApi @US12099
#Scenario: GetSubscriptionDetailsCustomerContext_CustomerWithMultipleAssets2
#     Given SubscriptionReadService Is Ready To Verify 'GetSubscriptionDetailsCustomerContext_CustomerWithMultipleAssets'
#		 And Multiple Subscriptions '<SubscriptionType>' has been setup for multiple assets under a customer
#	 When I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To Min Date
#        And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
#        And I Set Valid SubscriptionService AssetSubscriptionCreate Request AssetUID To Second AssetUID
#		And I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To Current Date
#        And I Set Valid SubscriptionService AssetSubscriptionCreate Request EndDate To Max Date
#		And I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionUID To Unique UID
#        And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
#	    And I Post Valid SubscriptionService Read Request For The Customer
#	 Then The SubscriptionServiceRead Response should return the Subscription Details For The Customer With Min Start Date and Max End Date	    
	 
@Automated @Regression @Positive
@SubscriptionReadService @US12099
Scenario: SubscriptionReadService_Customer_AssetSubscriptionUpdateEndDate_TerminatingAssetSubscriptionForFirstAssetWithEndDateasStartDate
     Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_Customer_AssetSubscriptionUpdateEndDate'
	    And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values
	 When I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To Min Date
        And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
        And I Set Valid SubscriptionService AssetSubscriptionCreate Request AssetUID To Second AssetUID
		And I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To Current Date
        And I Set Valid SubscriptionService AssetSubscriptionCreate Request EndDate To Max Date
		And I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionUID To Unique UID
        And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
		And I Set Valid SubscriptionService AssetSubscriptionUpdate Request SubscriptionUID To First Asset SubscriptionUID
		And I Set Valid SubscriptionService AssetSubscriptionUpdate Request ActionUTC To Current Date
		And I Set Valid SubscriptionService AssetSubscriptionUpdate Request EndDate To Min Date
		And I Post Valid SubscriptionService AssetSubscriptionUpdate Request 
	    And I Post Valid SubscriptionService Read Request For The Customer
	 Then The SubscriptionServiceRead Response should return the Subscription Details For The Customer     
	 
@Automated @Regression @Positive
@SubscriptionListWebApi @US12099
Scenario: GetSubscriptionDetailsCustomerContext_CustomerWithMultipleAssets_AfterUpdatingEndDate
     Given SubscriptionReadService Is Ready To Verify 'GetSubscriptionDetailsCustomerContext_CustomerWithMultipleAssets_AfterUpdatingEndDate'
		And SubscriptionType 'Essentials' has been setup for multiple assets under a customer
	 When I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To 'Min Date' For First Asset
        And I Set Valid SubscriptionService AssetSubscriptionCreate Request EndDate To 'Max Date' For Second Asset
	    And I Post Valid SubscriptionService Create Request For Multiple Asset Subscriptions
		And I Set Valid SubscriptionService AssetSubscriptionUpdate Request EndDate To 'Current Date' For First Asset
		And I Post Valid SubscriptionService AssetSubscriptionUpdate Request
	    And I Post Valid SubscriptionService Read Request For The Customer
	 Then The GetSubscriptionDetailsCustomerContext should return the Subscription Details With Start Date as 'Current Date' and End Date as 'Max Date'	         	       	  

@Automated @Regression @Positive
@SubscriptionListWebApi @US12099
Scenario: GetSubscriptionDetailsCustomerContext_CustomerWithMultipleAssets_AfterUpdatingStartDateAndEndDate
     Given SubscriptionReadService Is Ready To Verify 'GetSubscriptionDetailsCustomerContext_CustomerWithMultipleAssets_AfterUpdatingStartDateAndEndDate'
		And SubscriptionType 'Essentials' has been setup for multiple assets under a customer
	 When I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To 'Min Date' For First Asset
        And I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To 'Min Date' For Second Asset
	    And I Post Valid SubscriptionService Create Request For Multiple Asset Subscriptions
		And I Set Valid SubscriptionService AssetSubscriptionUpdate Request StartDate To 'Current Date' For First Asset
		And I Set Valid SubscriptionService AssetSubscriptionUpdate Request EndDate To 'Max Date' For First Asset
		And I Post Valid SubscriptionService AssetSubscriptionUpdate Request
	    And I Post Valid SubscriptionService Read Request For The Customer
	 Then The GetSubscriptionDetailsCustomerContext should return the Subscription Details With Start Date as 'Min Date' and End Date as 'Max Date'	         	       	  

  









@Automated @Sanity @Positive
@SubscriptionReadService @US12871
Scenario: SubscriptionReadService_CustomerSubscriptionCreateHappyPath1
	  Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_CustomerSubscriptionCreateHappyPath'
		And SubscriptionService CustomerSubscriptionCreate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService CustomerSubscriptionCreate Request 
	    And I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details For The Customer

@Automated @Regression @Positive
@SubscriptionReadService @US12871
Scenario: SubscriptionReadService_CustomerSubscriptionUpdateHappyPath
	  Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_CustomerSubscriptionUpdateHappyPath'
		And SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService CustomerSubscriptionUpdate Request 
	    And I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details For The Customer

@Automated @Regression @Positive
@SubscriptionReadService @US12871
Scenario Outline: SubscriptionReadService_CustomerSubscriptionpdateValidOptionalFields
	  Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_CustomerSubscriptionpdateValidOptionalFields'
		And SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService CustomerSubscriptionUpdate StartDate  To '<StartDate>'
		And I Set SubscriptionService CustomerSubscriptionUpdate EndDate To '<EndDate>'
	    And I Post Valid SubscriptionService CustomerSubscriptionUpdate Request
	    And I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details For The Customer
	  Examples:
	| Description | StartDate             | EndDate               |
	| StartDate   | 11/22/2015 2:29:55 PM | NULL_NULL             |
	| EndDate     | NULL_NULL             | 11/22/2015 2:29:55 PM |

@Automated @Sanity @Positive
@SubscriptionReadService @US12871
Scenario: SubscriptionReadService_ProjectSubscriptionCreateHappyPath
	  Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_ProjectSubscriptionCreateHappyPath'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService ProjectSubscriptionCreate Request 
	    And I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details For The Customer

@Automated @Regression @Positive
@SubscriptionReadService @US12871
Scenario Outline: SubscriptionReadService_ProjectSubscriptionUpdateHappyPath
	  Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_ProjectSubscriptionUpdateHappyPath'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Default Values
	  When I set SubscriptionService ProjectSubscriptionUpdate SubscriptionType To '<SubscriptionType>'
		And I Post Valid SubscriptionService ProjectSubscriptionUpdate Request 
	    And I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details For The Customer
	Examples:
	| Description        | SubscriptionType   |
	| Landfill           | Landfill           |
	| Project Monitoring | Project Monitoring |

@Automated @Regression @Positive
@SubscriptionReadService @US12871
Scenario Outline: SubscriptionReadService_ProjectSubscriptionpdateValidOptionalFields
	  Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_ProjectSubscriptionpdateValidOptionalFields'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService ProjectSubscriptionUpdate StartDate  To '<StartDate>'
		And I Set SubscriptionService ProjectSubscriptionUpdate EndDate To '<EndDate>'
	    And I Post Valid SubscriptionService ProjectSubscriptionUpdate Request
	    And I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details For The Customer
	  Examples:
	| Description | StartDate             | EndDate               |
	| StartDate   | 11/22/2015 2:29:55 PM | NULL_NULL             |
	| EndDate     | NULL_NULL             | 11/22/2015 2:29:55 PM |

@Automated @Sanity @Positive
@SubscriptionReadService @US12871
Scenario: SubscriptionReadService_ProjectSubscriptionAssociateProjectSubscriptionHappyPath
	  Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_ProjectSubscriptionAssociateProjectSubscriptionHappyPath'
		And SubscriptionService AssociateProjectSubscription Request Is Setup With Default Values
	  When I Post Valid SubscriptionService AssociateProjectSubscription Request 
	    And I Post Valid GetActiveProjectSubscriptions For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details SubscriptionCount as '0'

@Automated @Sanity @Positive
@SubscriptionReadService @US12871
Scenario: SubscriptionReadService_ProjectSubscriptionDissociateProjectSubscriptionHappyPath
	  Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_ProjectSubscriptionDissociateProjectSubscriptionHappyPath'
		And SubscriptionService DissociateProjectSubscription Request Is Setup With Default Values
	  When I Post Valid SubscriptionService DissociateProjectSubscription Request 
	    And I Post Valid GetActiveProjectSubscriptions For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details For The Customer




@Automated @Sanity @Negative
@SubscriptionReadService @US12871
Scenario: SubscriptionReadService_CustomerSubscriptionCreate_StartDateFutureDate
	  Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_CustomerSubscriptionCreateHappyPath'
		And SubscriptionService CustomerSubscriptionCreate Request Is Setup With Default Values
	  When I set SubscriptionService CustomerSubscriptionCreate StartDate To FutureDate
	    And I Post Valid SubscriptionService CustomerSubscriptionCreate Request 
	    And I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details SubscriptionCount as '0'

@Automated @Sanity @Negative
@SubscriptionReadService @US12871
Scenario: SubscriptionReadService_ProjectSubscriptionCreate_StartDateFutureDate
	  Given SubscriptionReadService Is Ready To Verify 'SubscriptionReadService_ProjectSubscriptionStartDate_FutureDate'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Default Values
	  When I set SubscriptionService ProjectSubscriptionCreate StartDate To FutureDate
	    And I Post Valid SubscriptionService ProjectSubscriptionCreate Request 
	    And I Post Valid SubscriptionService Read Request For The Customer
	  Then The SubscriptionServiceRead Response should return the Subscription Details SubscriptionCount as '0'











