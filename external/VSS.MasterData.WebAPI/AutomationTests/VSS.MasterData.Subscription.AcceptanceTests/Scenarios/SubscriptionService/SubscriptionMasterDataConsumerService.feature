Feature: Subscription MasterData ConsumerService

   References : https://docs.google.com/document/d/18zWcH34qTGne3rmqbDgqQ8Oh3ochUFJkrpRJfJ1D6vQ/edit
                           
   Dependencies:  Internal -  Kafka Topic
							  VSS DB	 - VSS-MasterData-Subscription
							      Tables - Asset Subscription
										   Project Subscription
										   Customer Subscription

   User Story 11721:Subscription MasterData Consumer Service (Master Data Management)
#______________________________________________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US11721
Scenario: SubscriptionMasterDataConsumerService_AssetSubscriptionCreateHappyPath
      Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_AssetSubscriptionCreateHappyPath'
         And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values
      When I Post Valid SubscriptionService AssetSubscriptionCreate Request 
      Then The SubscriptionService AssetSubscriptionCreated Details Are Stored in AssetSubscription and CustomerSubscription tables                                                           

@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US11721
Scenario: SubscriptionMasterDataConsumerService_AssetSubscriptionUpdateHappyPath
      Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_AssetSubscriptionUpdateHappyPath'
         And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values                       
      When I Post Valid SubscriptionService AssetSubscriptionUpdate Request 
      Then The SubscriptionService AssetSubscriptionUpdated Details Are Stored in AssetSubscription and CustomerSubscription tables                                    

@Automated @Regression @Positive
@SubscriptionService @US13356
Scenario Outline: SubscriptionMasterDataConsumerService_AssetSubscriptionUpdateValidOptionalFields
      Given SubscriptionService Is Ready To Verify 'SubscriptionMasterDataConsumerService_AssetSubscriptionUpdateValidOptionalFields'
         And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values                       
      When I Set SubscriptionService AssetSubscriptionUpdate CustomerUID To '<CustomerUID>'            
        And I Set SubscriptionService AssetSubscriptionUpdate AssetUID To '<AssetUID>'            
        And I Set SubscriptionService AssetSubscriptionUpdate SubscriptionType To '<SubscriptionType>'                       
        And I Set SubscriptionService AssetSubscriptionUpdate StartDate  To '<StartDate>'                       
        And I Set SubscriptionService AssetSubscriptionUpdate EndDate To '<EndDate>'                        
        And I Set SubscriptionService AssetSubscriptionUpdate DeviceUID To '<DeviceUID>'                        
        And I Post Valid SubscriptionService AssetSubscriptionUpdate Request            
      Then The SubscriptionService AssetSubscriptionUpdated Details Are Stored in AssetSubscription and CustomerSubscription tables      
	  Examples:       
    | Description      | CustomerUID                          | AssetUID                             | SubscriptionType     | StartDate             | EndDate               | DeviceUID                            |
    | CustomerUID      | 6CEC6135-89C8-11E5-9797-005056886D0D | NULL_NULL                            | NULL_NULL            | NULL_NULL             | NULL_NULL             | NULL_NULL                            |
    | AssetUID         | NULL_NULL                            | 6CEC6135-89C8-11E5-9797-005056886D0D | NULL_NULL            | NULL_NULL             | NULL_NULL             | NULL_NULL                            |
    | SubscriptionType | NULL_NULL                            | NULL_NULL                            | AdvancedProductivity | NULL_NULL             | NULL_NULL             | NULL_NULL                            |
    | StartDate        | NULL_NULL                            | NULL_NULL                            | NULL_NULL            | 11/22/2015 2:29:55 PM | NULL_NULL             | NULL_NULL                            |
    | EndDate          | NULL_NULL                            | NULL_NULL                            | NULL_NULL            | NULL_NULL             | 11/22/2015 2:29:55 PM | NULL_NULL                            |
    | DeviceUID        | NULL_NULL                            | NULL_NULL                            | NULL_NULL            | NULL_NULL             | NULL_NULL             | 6CEC6135-89C8-11E5-9797-005056886D0D |


@Automated @Regression @Negative
@SubscriptionMasterDataConsumerService @US11721
Scenario: SubscriptionMasterDataConsumerService_CreateInvalid_Duplicate
      Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_CreateInvalid_Duplicate'
         And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values                       
      When I Post Valid SubscriptionService AssetSubscriptionCreate Request             
        And I Post Valid SubscriptionService AssetSubscriptionCreate Request             
      Then The SubscriptionService AssetSubscriptionCreated Details Are Stored in VSS DB             

@Automated @Sanity @Negative
@SubscriptionMasterDataConsumerService @US11721
Scenario: SubscriptionMasterDataConsumerService_UpdateInvalid_WithoutCreate
      Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_UpdateInvalid_WithoutCreate'
         And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values WithoutCreate                       
      When I Post Valid SubscriptionService AssetSubscriptionUpdate Request             
      Then The SubscriptionService AssetSubscriptionUpdated Details Must Not be Stored in VSS DB            

@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US11721
Scenario: SubscriptionMasterDataConsumerService_AssetSubscription_SameAssetDifferentSubscriptionTypes
      Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_AssetSubscription_SameAssetDifferentSubscriptionTypes'   
         And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values
       When I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionType To 'Essentials'
         And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
                                      And I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionType To 'ManualMaintenancelog'
                                                  And I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionUID To Unique UID
              And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
         Then The SubscriptionService AssetSubscriptionCreated Details Are Stored in 'AssetSubscription' Table
           And The CustomerSubscription Table Should Contain SubscriptionType with StartDate and EndDate

@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US11721
Scenario: SubscriptionMasterDataConsumerService_AssetSubscription_DifferentAssetsSameSubscriptionType
         Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_AssetSubscriptionCreateHappyPath'
                                    And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values
         When I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To Min Date
              And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
              And I Set Valid SubscriptionService AssetSubscriptionCreate Request AssetUID To Second AssetUID
                                                  And I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To Current Date
              And I Set Valid SubscriptionService AssetSubscriptionCreate Request EndDate To Max Date
                                                  And I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionUID To Unique UID
              And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
       #  Then The SubscriptionService AssetSubscriptionCreated Details Are Stored in 'AssetSubscription' Table
        #   And The 'CustomerSubscription' Table Should Contain SubscriptionType with StartDate and EndDate

                                #default
                                #start date current
                                #second asset start date min date
                                #updated to current

@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US11721
Scenario: SubscriptionMasterDataConsumerService_AssetSubscriptionSC4
                  Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_AssetSubscriptionCreateHappyPath'
                                And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values
                  When I Post Valid SubscriptionService AssetSubscriptionCreate Request 
                                And I Set Valid SubscriptionService AssetSubscriptionCreate Request AssetUID To Second AssetUID
                                And I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To Min Date 
                                And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
                                                                And I Set Valid SubscriptionService AssetSubscriptionUpdate Request StartDate To Min Date 
                                And I Set Valid SubscriptionService AssetSubscriptionUpdate Request StartDate To '01/02/2018 2:29:55 PM'  
                                And I Set Valid SubscriptionService AssetSubscriptionUpdate Request EndDate To '01/30/2028 2:29:55 PM' 
                                And I Post Valid SubscriptionService AssetSubscriptionUpdate Request 
                                And I Set Valid SubscriptionService AssetSubscriptionCreate Request AssetUID To a Different UID
                                And I Set Valid SubscriptionService AssetSubscriptionCreate Request SubscriptionType To 'Essentials'
                                And I Set Valid SubscriptionService AssetSubscriptionCreate Request StartDate To '12/01/2016 2:29:55 PM'  
                                And I Set Valid SubscriptionService AssetSubscriptionCreate Request EndDate To '12/29/2026 2:29:55 PM' 
                                And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
                  Then The SubscriptionService AssetSubscriptionCreated Details Are Stored in 'AssetSubscription' Table 
                    And The 'CustomerSubscription' Table Should Contain SubscriptionType as 'StandardHealth' with StartDate as '12/28/2015' and EndDate as '10/24/2025'
                                And The 'CustomerSubscription' Table Should Contain SubscriptionType as 'Essentials' with StartDate as '12/01/2016' and EndDate as '01/30/2028'



@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US12870
Scenario Outline: SubscriptionMasterDataConsumerService_CustomerSubscriptionCreateHappyPath
	  Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_CustomerSubscriptionCreateHappyPath'
		And SubscriptionService CustomerSubscriptionCreate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService CustomerSubscriptionCreate Request 
	  Then The SubscriptionService CustomerSubscriptionCreated Details Are Stored in CustomerSubscription table
	Examples:
	| Description                   | SubscriptionType              |
	| Manual 3D Project Monitoring  | Manual 3D Project Monitoring  |
	| Operator Id/ Manage Operators | Operator Id/ Manage Operators |

@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US12870
Scenario Outline: SubscriptionMasterDataConsumerService_CustomerSubscriptionUpdateHappyPath
	  Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_CustomerSubscriptionUpdateHappyPath'
		And SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService CustomerSubscriptionUpdate Request 
	  Then The SubscriptionService CustomerSubscriptionUpdated Details Are Stored in CustomerSubscription table
	Examples:
	| Description                   | SubscriptionType              |
	| Manual 3D Project Monitoring  | Manual 3D Project Monitoring  |
	| Operator Id/ Manage Operators | Operator Id/ Manage Operators |

@Automated @Regression @Positive
@SubscriptionMasterDataConsumerService @US12870
Scenario Outline: SubscriptionMasterDataConsumerService_CustomerSubscriptionpdateValidOptionalFields
	  Given SubscriptionService Is Ready To Verify 'SubscriptionMasterDataConsumerService_CustomerSubscriptionUpdateValidOptionalFields'
		And SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService CustomerSubscriptionUpdate StartDate  To '<StartDate>'
		And I Set SubscriptionService CustomerSubscriptionUpdate EndDate To '<EndDate>'
	    And I Post Valid SubscriptionService CustomerSubscriptionUpdate Request
	  Then The SubscriptionService CustomerSubscriptionUpdated Details Are Stored in CustomerSubscription table
	  Examples:
	| Description | StartDate             | EndDate               |
	| StartDate   | 11/22/2015 2:29:55 PM | NULL_NULL             |
	| EndDate     | NULL_NULL             | 11/22/2015 2:29:55 PM |

@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US12870
Scenario Outline: SubscriptionMasterDataConsumerService_ProjectSubscriptionCreateHappyPath
	  Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_ProjectSubscriptionCreateHappyPath'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Default Values
	  When I set SubscriptionService ProjectSubscriptionCreate SubscriptionType To '<SubscriptionType>'
	    And I Post Valid SubscriptionService ProjectSubscriptionCreate Request 
	  Then SubscriptionService ProjectSubscriptionCreated Details Are Stored in ProjectSubscription and CustomerSubscription tables
	  Examples:
	| Description        | SubscriptionType   |
	| Landfill           | Landfill           |
	| Project Monitoring | Project Monitoring |

@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US12870
Scenario Outline: SubscriptionMasterDataConsumerService_ProjectSubscriptionUpdateHappyPath
	  Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_ProjectSubscriptionUpdateHappyPath'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Default Values
	  When I set SubscriptionService ProjectSubscriptionUpdate SubscriptionType To '<SubscriptionType>'
		And I Post Valid SubscriptionService ProjectSubscriptionUpdate Request 
	  Then SubscriptionService ProjectSubscriptionUpdated Details Are Stored in ProjectSubscription and CustomerSubscription tables
	Examples:
	| Description        | SubscriptionType   |
	| Landfill           | Landfill           |
	| Project Monitoring | Project Monitoring |

@Automated @Regression @Positive
@SubscriptionMasterDataConsumerService @US13356
Scenario Outline: SubscriptionMasterDataConsumerService_ProjectSubscriptionpdateValidOptionalFields
	  Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionReadService_ProjectSubscriptionpdateValidOptionalFields'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService ProjectSubscriptionUpdate StartDate  To '<StartDate>'
		And I Set SubscriptionService ProjectSubscriptionUpdate EndDate To '<EndDate>'
	    And I Post Valid SubscriptionService ProjectSubscriptionUpdate Request
	  Then SubscriptionService ProjectSubscriptionUpdated Details Are Stored in ProjectSubscription and CustomerSubscription tables
	  Examples:
	| Description | StartDate             | EndDate               |
	| StartDate   | 11/22/2015 2:29:55 PM | NULL_NULL             |
	| EndDate     | NULL_NULL             | 11/22/2015 2:29:55 PM |

@Automated @Regression @Positive
@SubscriptionMasterDataConsumerService @US12870
Scenario: SubscriptionMasterDataConsumerService_ProjectSubscription_MultipleProjectsForSameCustomer
	  Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_ProjectSubscription_MultipleProjectsForSameCustomer'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Default Values
	  When I set SubscriptionService ProjectSubscriptionCreate StartDate To Min Date
	    And I Post Valid SubscriptionService ProjectSubscriptionCreate Request 
		And I set SubscriptionService ProjectSubscriptionCreate StartDate To Current Date
		And I Post Valid SubscriptionService ProjectSubscriptionCreate Request
	  Then SubscriptionService ProjectSubscriptionCreated Details Are Stored in ProjectSubscription table 
	    And The CustomerSubscription table should have the StartDate as Min Date and End Date as Max Date


@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US12870
Scenario: SubscriptionMasterDataConsumerService_ProjectSubscriptionAssociateProjectSubscriptionHappyPath
	  Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_ProjectSubscriptionAssociateProjectSubscriptionHappyPath'
		And SubscriptionService AssociateProjectSubscription Request Is Setup With Default Values
	  When I Post Valid SubscriptionService AssociateProjectSubscription Request 
	  Then SubscriptionService ProjectSubscriptionAssociated Details Are Stored in ProjectSubscription table

@Automated @Sanity @Positive
@SubscriptionMasterDataConsumerService @US12870
Scenario: SubscriptionMasterDataConsumerService_ProjectSubscriptionDissociateProjectSubscriptionHappyPath
	  Given SubscriptionMasterDataConsumerService Is Ready To Verify 'SubscriptionMasterDataConsumerService_ProjectSubscriptionDissociateProjectSubscriptionHappyPath'
		And SubscriptionService DissociateProjectSubscription Request Is Setup With Default Values
	  When I Post Valid SubscriptionService DissociateProjectSubscription Request 
	  Then SubscriptionService ProjectSubscriptionAssociated Details Are Stored in ProjectSubscription table


















