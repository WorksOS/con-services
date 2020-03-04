Feature: SubscriptionService

   References : https://docs.google.com/document/d/18zWcH34qTGne3rmqbDgqQ8Oh3ochUFJkrpRJfJ1D6vQ/edit
                           
   Dependencies:  Internal -  Kafka Topic

   User Story 9655:Subscription Service (Master Data Management)
#______________________________________________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_AssetSubscriptionCreateHappyPath
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreateHappyPath'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService AssetSubscriptionCreate Request 
	  Then The Processed SubscriptionService AssetSubscriptionCreate Message must be available in Kafka topic
	  And The CreateSubscription Details must be stored in MySql DB

@Automated @Sanity @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_AssetSubscriptionUpdateHappyPath
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateHappyPath'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService AssetSubscriptionUpdate Request
      Then The Processed SubscriptionService AssetSubscriptionUpdate Message must be available in Kafka topic
	  And The UpdateSubscription Details must be stored in MySql DB

@Automated @Sanity @Positive
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionCreate_Source
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreate_Source_<Description>'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values
		Then I Set SubscriptionService AssetSubscriptionCreate Source To '<Source>'
	  When I Post Valid SubscriptionService AssetSubscriptionCreate Request 
	  Then The Processed SubscriptionService AssetSubscriptionCreate Message must be available in Kafka topic
	  And The CreateSubscription Details must be stored in MySql DB
Examples: 
	  | Description | Source    |
	  | Store       | Store     |
	  | SAV         | SAV       |
	  | NULL        | NULL_NULL |

@Automated @Sanity @Positive
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionUpdate_Source
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdate_Source_<Description>'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values
	  Then I Set SubscriptionService AssetSubscriptionUpdate Source To '<Source>'
	  When I Post Valid SubscriptionService AssetSubscriptionUpdate Request
      Then The Processed SubscriptionService AssetSubscriptionUpdate Message must be available in Kafka topic
	  And The UpdateSubscription Details must be stored in MySql DB
Examples: 
	  | Description | Source    |
	  | Store       | Store     |
	  | SAV         | SAV       |
	  | NULL        | NULL_NULL |

@Automated @Sanity @Positive
@SubscriptionService @US13356
Scenario: SubscriptionService_AssetSubscriptionCreateDeviceUID_NULL
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreateDeviceUID_NULL'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Default Values
	  When I Set SubscriptionService AssetSubscriptionCreate DeviceUID To 'NULL_NULL'
	    And I Post Valid SubscriptionService AssetSubscriptionCreate Request 
	  Then The Processed SubscriptionService AssetSubscriptionCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_AssetSubscriptionUpdateValidCustomerUID_NULL
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateValidCustomerUID_NULL'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService AssetSubscriptionUpdate CustomerUID To 'NULL_NULL'
	    And I Post Valid SubscriptionService AssetSubscriptionUpdate Request
      Then The Processed SubscriptionService AssetSubscriptionUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_AssetSubscriptionUpdateValidAssetUID_NULL
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateValidAssetUID_NULL'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService AssetSubscriptionUpdate AssetUID To 'NULL_NULL'
	    And I Post Valid SubscriptionService AssetSubscriptionUpdate Request
      Then The Processed SubscriptionService AssetSubscriptionUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_AssetSubscriptionUpdateValidSubscriptionType_NULL
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateValidSubscriptionType_NULL'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService AssetSubscriptionUpdate SubscriptionType To 'NULL_NULL'
	    And I Post Valid SubscriptionService AssetSubscriptionUpdate Request
      Then The Processed SubscriptionService AssetSubscriptionUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_AssetSubscriptionUpdateValidStartDate_NULL
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateValidStartDate_NULL'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService AssetSubscriptionUpdate StartDate  To 'NULL_NULL'
	    And I Post Valid SubscriptionService AssetSubscriptionUpdate Request
      Then The Processed SubscriptionService AssetSubscriptionUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_AssetSubscriptionUpdateValidEndDate_NULL
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateValidEndDate_NULL'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService AssetSubscriptionUpdate EndDate To 'NULL_NULL'
	    And I Post Valid SubscriptionService AssetSubscriptionUpdate Request
      Then The Processed SubscriptionService AssetSubscriptionUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US13356
Scenario: SubscriptionService_AssetSubscriptionUpdateValidDeviceUID_NULL
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateValidDeviceUID_NULL'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService AssetSubscriptionUpdate DeviceUID To 'NULL_NULL'
	    And I Post Valid SubscriptionService AssetSubscriptionUpdate Request
      Then The Processed SubscriptionService AssetSubscriptionUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US13356
Scenario Outline: SubscriptionService_AssetSubscriptionUpdateValidOptionalFields
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateValidOptionalFields'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Default Values
	  When I Set SubscriptionService AssetSubscriptionUpdate CustomerUID To '<CustomerUID>'
	    And I Set SubscriptionService AssetSubscriptionUpdate AssetUID To '<AssetUID>'
		And I Set SubscriptionService AssetSubscriptionUpdate SubscriptionType To '<SubscriptionType>'
		And I Set SubscriptionService AssetSubscriptionUpdate StartDate  To '<StartDate>'
		And I Set SubscriptionService AssetSubscriptionUpdate EndDate To '<EndDate>'
		And I Set SubscriptionService AssetSubscriptionUpdate DeviceUID To '<DeviceUID>'
	    And I Post Valid SubscriptionService AssetSubscriptionUpdate Request
      Then The Processed SubscriptionService AssetSubscriptionUpdate Message must be available in Kafka topic
	  Examples:
	| Description      | CustomerUID                          | AssetUID                             | SubscriptionType     | StartDate             | EndDate               | DeviceUID                            |
	| CustomerUID      | 6CEC6135-89C8-11E5-9797-005056886D0D | NULL_NULL                            | NULL_NULL            | NULL_NULL             | NULL_NULL             | NULL_NULL                            |
	| AssetUID         | NULL_NULL                            | 6CEC6135-89C8-11E5-9797-005056886D0D | NULL_NULL            | NULL_NULL             | NULL_NULL             | NULL_NULL                            |
	| SubscriptionType | NULL_NULL                            | NULL_NULL                            | AdvancedProductivity | NULL_NULL             | NULL_NULL             | NULL_NULL                            |
	| StartDate        | NULL_NULL                            | NULL_NULL                            | NULL_NULL            | 11/22/2015 2:29:55 PM | NULL_NULL             | NULL_NULL                            |
	| EndDate          | NULL_NULL                            | NULL_NULL                            | NULL_NULL            | NULL_NULL             | 11/22/2015 2:29:55 PM | NULL_NULL                            |
	| DeviceUID        | NULL_NULL                            | NULL_NULL                            | NULL_NULL            | NULL_NULL             | NULL_NULL             | 6CEC6135-89C8-11E5-9797-005056886D0D |



@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionCreateInvalidSubscriptionUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreateInvalidSubscriptionUID'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionCreate SubscriptionUID  To '<SubscriptionUID>'
	    And I Post Invalid SubscriptionService AssetSubscriptionCreate Request
      Then SubscriptionService AssetSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | SubscriptionUID                      | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US13356
Scenario Outline: SubscriptionService_AssetSubscriptionCreateInvalidDeviceUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreateInvalidDeviceUID'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionCreate DeviceUID  To '<DeviceUID>'
	    And I Post Invalid SubscriptionService AssetSubscriptionCreate Request
      Then SubscriptionService AssetSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | DeviceUID                            | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionCreateInvalidCustomerUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreateInvalidCustomerUID'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionCreate CustomerUID To '<CustomerUID>'
	    And I Post Invalid SubscriptionService AssetSubscriptionCreate Request
      Then SubscriptionService AssetSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionCreateInvalidAssetUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreateInvalidAssetUID'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionCreate AssetUID To '<AssetUID>'
	    And I Post Invalid SubscriptionService AssetSubscriptionCreate Request
      Then SubscriptionService AssetSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | AssetUID                             | ErrorMessage                    |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionAssetUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionAssetUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionAssetUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionAssetUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionAssetUIDInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionAssetUIDInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionAssetUIDInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionCreateInvalidSubscriptionType
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreateInvalidSubscriptionType'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionCreate SubscriptionType To '<SubscriptionType>'
	    And I Post Invalid SubscriptionService AssetSubscriptionCreate Request
      Then SubscriptionService AssetSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description | SubscriptionType | ErrorMessage                |
	| EMPTY       | EMPTY_EMPTY      | ERR_SubscriptionTypeInvalid |
	| NULL        | NULL_NULL        | ERR_SubscriptionTypeInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionCreateInvalidStartDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreateInvalidStartDate'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionCreate StartDate To '<StartDate>'
	    And I Post Invalid SubscriptionService AssetSubscriptionCreate Request
      Then SubscriptionService AssetSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | StartDate             | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionCreateInvalidEndDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreateInvalidEndDate'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionCreate EndDate To '<EndDate>'
	    And I Post Invalid SubscriptionService AssetSubscriptionCreate Request
      Then SubscriptionService AssetSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | EndDate               | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionCreateInvalidActionUTC
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionCreateInvalidActionUTC'
		And SubscriptionService AssetSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionCreate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid SubscriptionService AssetSubscriptionCreate Request  
      Then SubscriptionService AssetSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description       | ActionUTC             | ErrorMessage               |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionUpdateInvalidSubscriptionUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateInvalidSubscriptionUID'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionUpdate SubscriptionUID  To '<SubscriptionUID>'
	    And I Post Invalid SubscriptionService AssetSubscriptionUpdate Request
      Then SubscriptionService AssetSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | SubscriptionUID                      | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionUpdateInvalidCustomerUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateInvalidCustomerUID'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionUpdate CustomerUID To '<CustomerUID>'
	    And I Post Invalid SubscriptionService AssetSubscriptionUpdate Request
      Then SubscriptionService AssetSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionUpdateInvalidAssetUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateInvalidAssetUID'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionUpdate AssetUID To '<AssetUID>'
	    And I Post Invalid SubscriptionService AssetSubscriptionUpdate Request
      Then SubscriptionService AssetSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | AssetUID                             | ErrorMessage                    |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionAssetUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionAssetUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionAssetUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionAssetUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionAssetUIDInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionAssetUIDInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario: SubscriptionService_AssetSubscriptionUpdateInvalidSubscriptionType_Empty
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateInvalidSubscriptionType_Empty'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Invalid Default Values
	 When I Set Invalid SubscriptionService AssetSubscriptionUpdate CustomerUID To 'NULL_NULL'
	    And I Set Invalid SubscriptionService AssetSubscriptionUpdate AssetUID To 'NULL_NULL'
		And I Set Invalid SubscriptionService AssetSubscriptionUpdate SubscriptionType To 'EMPTY_EMPTY'
		And I Set Invalid SubscriptionService AssetSubscriptionUpdate StartDate To 'NULL_NULL'
		And I Set Invalid SubscriptionService AssetSubscriptionUpdate EndDate To 'NULL_NULL'
	    And I Post Invalid SubscriptionService AssetSubscriptionUpdate Request
      Then SubscriptionService AssetSubscriptionUpdate Response With 'ERR_AssetSubscriptionUpdateInvalid' Should Be Returned

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionUpdateInvalidStartDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateInvalidStartDate'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionUpdate StartDate To '<StartDate>'
	    And I Post Invalid SubscriptionService AssetSubscriptionUpdate Request
      Then SubscriptionService AssetSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | StartDate             | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionUpdateInvalidEndDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateInvalidEndDate'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionUpdate EndDate To '<EndDate>'
	    And I Post Invalid SubscriptionService AssetSubscriptionUpdate Request
      Then SubscriptionService AssetSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | EndDate               | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
		
@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionUpdateInvalidActionUTC
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateInvalidActionUTC'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionUpdate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid SubscriptionService AssetSubscriptionUpdate Request  
      Then SubscriptionService AssetSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description       | ActionUTC             | ErrorMessage               |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssetSubscriptionUpdateInvalidAllOptionalFields
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssetSubscriptionUpdateInvalidAllOptionalFields'
		And SubscriptionService AssetSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssetSubscriptionUpdate AssetUID To '<AssetUID>'
	  	And I Set Invalid SubscriptionService AssetSubscriptionUpdate CustomerUID To '<CustomerUID>'
		And I Set Invalid SubscriptionService AssetSubscriptionUpdate SubscriptionType To '<SubscriptionType>'
		And I Set Invalid SubscriptionService AssetSubscriptionUpdate StartDate To '<StartDate>'
		And I Set Invalid SubscriptionService AssetSubscriptionUpdate EndDate To '<EndDate>'
	    And I Post Invalid SubscriptionService AssetSubscriptionUpdate Request
      Then SubscriptionService AssetSubscriptionUpdate Response With 'ERR_AssetSubscriptionUpdateInvalid' Should Be Returned
	Examples:
	| Description | AssetUID    | CustomerUID | SubscriptionType | StartDate   | EndDate     |
	| NULL        | NULL_NULL   | NULL_NULL   | NULL_NULL        | NULL_NULL   | NULL_NULL   |
	| EMPTY       | EMPTY_EMPTY | EMPTY_EMPTY | EMPTY_EMPTY      | EMPTY_EMPTY | EMPTY_EMPTY |


@Automated @Sanity @Positive
@SubscriptionService @US12665
Scenario: SubscriptionService_CustomerSubscriptionCreateHappyPath
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionCreateHappyPath'
		And SubscriptionService CustomerSubscriptionCreate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService CustomerSubscriptionCreate Request 
	  Then The Processed SubscriptionService CustomerSubscriptionCreate Message must be available in Kafka topic

@Automated @Sanity @Positive
@SubscriptionService @US12665
Scenario: SubscriptionService_CustomerSubscriptionUpdateHappyPath
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionUpdateHappyPath'
		And SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService CustomerSubscriptionUpdate Request
      Then The Processed SubscriptionService CustomerSubscriptionUpdate Message must be available in Kafka topic

@Automated @Sanity @Positive
@SubscriptionService @US12665
Scenario: SubscriptionService_ProjectSubscriptionCreateHappyPath
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionCreateHappyPath'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService ProjectSubscriptionCreate Request 
	  Then The Processed SubscriptionService ProjectSubscriptionCreate Message must be available in Kafka topic

@Automated @Sanity @Positive
@SubscriptionService @US12665
Scenario: SubscriptionService_ProjectSubscriptionUpdateHappyPath
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionUpdateHappyPath'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Default Values
	  When I Post Valid SubscriptionService ProjectSubscriptionUpdate Request
      Then The Processed SubscriptionService ProjectSubscriptionUpdate Message must be available in Kafka topic

@Automated @Sanity @Positive
@SubscriptionService @US12665
Scenario: SubscriptionService_AssociateProjectSubscriptionHappyPath
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssociateProjectSubscriptionHappyPath'
		And SubscriptionService AssociateProjectSubscription Request Is Setup With Default Values
	  When I Post Valid SubscriptionService AssociateProjectSubscription Request 
	  Then The Processed SubscriptionService AssociateProjectSubscription Message must be available in Kafka topic

@Automated @Sanity @Positive
@SubscriptionService @US12665
Scenario: SubscriptionService_DissociateProjectSubscriptionHappyPath
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_DissociateProjectSubscriptionHappyPath'
		And SubscriptionService DissociateProjectSubscription Request Is Setup With Default Values
	  When I Post Valid SubscriptionService DissociateProjectSubscription Request
      Then The Processed SubscriptionService DissociateProjectSubscription Message must be available in Kafka topic


@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CustomerSubscriptionCreateInvalidSubscriptionUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionCreateInvalidSubscriptionUID'
		And SubscriptionService CustomerSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService CustomerSubscriptionCreate SubscriptionUID  To '<SubscriptionUID>'
	    And I Post Invalid SubscriptionService CustomerSubscriptionCreate Request
      Then SubscriptionService CustomerSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | SubscriptionUID                      | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CustomerSubscriptionCreateInvalidCustomerUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionCreateInvalidCustomerUID'
		And SubscriptionService CustomerSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService CustomerSubscriptionCreate CustomerUID To '<CustomerUID>'
	    And I Post Invalid SubscriptionService CustomerSubscriptionCreate Request
      Then SubscriptionService CustomerSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CustomerSubscriptionCreateInvalidSubscriptionType
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionCreateInvalidSubscriptionType'
		And SubscriptionService CustomerSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService CustomerSubscriptionCreate SubscriptionType To '<SubscriptionType>'
	    And I Post Invalid SubscriptionService CustomerSubscriptionCreate Request
      Then SubscriptionService CustomerSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description | SubscriptionType | ErrorMessage                |
	| EMPTY       | EMPTY_EMPTY      | ERR_SubscriptionTypeInvalid |
	| NULL        | NULL_NULL        | ERR_SubscriptionTypeInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CustomerSubscriptionCreateInvalidStartDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionCreateInvalidStartDate'
		And SubscriptionService CustomerSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService CustomerSubscriptionCreate StartDate To '<StartDate>'
	    And I Post Invalid SubscriptionService CustomerSubscriptionCreate Request
      Then SubscriptionService CustomerSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | StartDate             | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CustomerSubscriptionCreateInvalidEndDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionCreateInvalidEndDate'
		And SubscriptionService CustomerSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService CustomerSubscriptionCreate EndDate To '<EndDate>'
	    And I Post Invalid SubscriptionService CustomerSubscriptionCreate Request
      Then SubscriptionService CustomerSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | EndDate               | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CustomerSubscriptionCreateInvalidActionUTC
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionCreateInvalidActionUTC'
		And SubscriptionService CustomerSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService CustomerSubscriptionCreate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid SubscriptionService CustomerSubscriptionCreate Request  
      Then SubscriptionService CustomerSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description       | ActionUTC             | ErrorMessage               |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CustomerSubscriptionUpdateInvalidSubscriptionUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionUpdateInvalidSubscriptionUID'
		And SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService CustomerSubscriptionUpdate SubscriptionUID  To '<SubscriptionUID>'
	    And I Post Invalid SubscriptionService CustomerSubscriptionUpdate Request
      Then SubscriptionService CustomerSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | SubscriptionUID                      | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionInvalid |


@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CustomerSubscriptionUpdateInvalidStartDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionUpdateInvalidStartDate'
		And SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService CustomerSubscriptionUpdate StartDate To '<StartDate>'
	    And I Post Invalid SubscriptionService CustomerSubscriptionUpdate Request
      Then SubscriptionService CustomerSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | StartDate             | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CustomerSubscriptionUpdateInvalidEndDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionUpdateInvalidEndDate'
		And SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService CustomerSubscriptionUpdate EndDate To '<EndDate>'
	    And I Post Invalid SubscriptionService CustomerSubscriptionUpdate Request
      Then SubscriptionService CustomerSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | EndDate               | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CustomerSubscriptionUpdateInvalidActionUTC
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CustomerSubscriptionUpdateInvalidActionUTC'
		And SubscriptionService CustomerSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService CustomerSubscriptionUpdate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid SubscriptionService CustomerSubscriptionUpdate Request  
      Then SubscriptionService CustomerSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description       | ActionUTC             | ErrorMessage               |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionCreateInvalidSubscriptionUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionCreateInvalidSubscriptionUID'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionCreate SubscriptionUID  To '<SubscriptionUID>'
	    And I Post Invalid SubscriptionService ProjectSubscriptionCreate Request
      Then SubscriptionService ProjectSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | SubscriptionUID                      | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionCreateCreateInvalidCustomerUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionCreateCreateInvalidCustomerUID'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionCreate CustomerUID To '<CustomerUID>'
	    And I Post Invalid SubscriptionService ProjectSubscriptionCreate Request
      Then SubscriptionService ProjectSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionCreateInvalidSubscriptionType
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionCreateInvalidSubscriptionType'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionCreate SubscriptionType To '<SubscriptionType>'
	    And I Post Invalid SubscriptionService ProjectSubscriptionCreate Request
      Then SubscriptionService ProjectSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description | SubscriptionType | ErrorMessage                |
	| EMPTY       | EMPTY_EMPTY      | ERR_SubscriptionTypeInvalid |
	| NULL        | NULL_NULL        | ERR_SubscriptionTypeInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionCreateInvalidStartDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionCreateInvalidStartDate'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionCreate StartDate To '<StartDate>'
	    And I Post Invalid SubscriptionService ProjectSubscriptionCreate Request
      Then SubscriptionService ProjectSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | StartDate             | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionCreateInvalidEndDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionCreateInvalidEndDate'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionCreate EndDate To '<EndDate>'
	    And I Post Invalid SubscriptionService ProjectSubscriptionCreate Request
      Then SubscriptionService ProjectSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | EndDate               | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionCreateInvalidActionUTC
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionCreateInvalidActionUTC'
		And SubscriptionService ProjectSubscriptionCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionCreate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid SubscriptionService ProjectSubscriptionCreate Request  
      Then SubscriptionService ProjectSubscriptionCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description       | ActionUTC             | ErrorMessage               |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionUpdateInvalidSubscriptionUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionUpdateInvalidSubscriptionUID'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionUpdate SubscriptionUID  To '<SubscriptionUID>'
	    And I Post Invalid SubscriptionService ProjectSubscriptionUpdate Request
      Then SubscriptionService ProjectSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | SubscriptionUID                      | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionUpdateInvalidCustomerUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionUpdateInvalidCustomerUID'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionUpdate CustomerUID To '<CustomerUID>'
	    And I Post Invalid SubscriptionService ProjectSubscriptionUpdate Request
      Then SubscriptionService ProjectSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |
	| NULL               | NULL_NULL                            | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionUpdateInvalidSubscriptionType
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionUpdateInvalidSubscriptionType'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionUpdate SubscriptionType To '<SubscriptionType>'
	    And I Post Invalid SubscriptionService ProjectSubscriptionUpdate Request
      Then SubscriptionService ProjectSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description | SubscriptionType | ErrorMessage                |
	| EMPTY       | EMPTY_EMPTY      | ERR_SubscriptionTypeInvalid |
	| NULL        | NULL_NULL        | ERR_SubscriptionTypeInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionUpdateInvalidStartDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionUpdateInvalidStartDate'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionUpdate StartDate To '<StartDate>'
	    And I Post Invalid SubscriptionService ProjectSubscriptionUpdate Request
      Then SubscriptionService ProjectSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | StartDate             | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionUpdateInvalidEndDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionUpdateInvalidEndDate'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionUpdate EndDate To '<EndDate>'
	    And I Post Invalid SubscriptionService ProjectSubscriptionUpdate Request
      Then SubscriptionService ProjectSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | EndDate               | ErrorMessage               |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_ProjectSubscriptionUpdateInvalidActionUTC
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_ProjectSubscriptionUpdateInvalidActionUTC'
		And SubscriptionService ProjectSubscriptionUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService ProjectSubscriptionUpdate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid SubscriptionService ProjectSubscriptionUpdate Request  
      Then SubscriptionService ProjectSubscriptionUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description       | ActionUTC             | ErrorMessage               |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |
	| NULL              | NULL_NULL             | ERR_SubscriptionUTCInvalid |


@Automated @Regression @Negative
@SubscriptionService @US12665
Scenario Outline: SubscriptionService_AssociateProjectSubscriptionInvalidActionUTC
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssociateProjectSubscriptionInvalidActionUTC'
		And SubscriptionService AssociateProjectSubscription Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssociateProjectSubscription ActionUTC To '<ActionUTC>'
		And I Post Invalid SubscriptionService AssociateProjectSubscription Request
      Then SubscriptionService AssociateProjectSubscription Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | ActionUTC             | ErrorMessage               |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssociateProjectSubscriptionInvalidProjectUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssociateProjectSubscriptionInvalidProjectUID'
		And SubscriptionService AssociateProjectSubscription Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssociateProjectSubscription ProjectUID To '<ProjectUID>'
	    And I Post Invalid SubscriptionService AssociateProjectSubscription Request
      Then SubscriptionService AssociateProjectSubscription Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US12665
Scenario Outline: SubscriptionService_AssociateProjectSubscriptionInvalidEffectiveDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssociateProjectSubscriptionInvalidEffectiveDate'
		And SubscriptionService AssociateProjectSubscription Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssociateProjectSubscription EffectiveDate To '<EffectiveDate>'
		And I Post Invalid SubscriptionService AssociateProjectSubscription Request
      Then SubscriptionService AssociateProjectSubscription Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | ActionUTC             | ErrorMessage               |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_AssociateProjectSubscriptionInvalidSubscriptionUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_AssociateProjectSubscriptionInvalidSubscriptionUID'
		And SubscriptionService AssociateProjectSubscription Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService AssociateProjectSubscription SubscriptionUID To '<SubscriptionUID>'
	    And I Post Invalid SubscriptionService AssociateProjectSubscription Request
      Then SubscriptionService AssociateProjectSubscription Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US12665
Scenario Outline: SubscriptionService_DissociateProjectSubscriptionInvalidActionUTC
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_DissociateProjectSubscriptionInvalidActionUTC'
		And SubscriptionService DissociateProjectSubscription Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService DissociateProjectSubscription ActionUTC To '<ActionUTC>'
		And I Post Invalid SubscriptionService DissociateProjectSubscription Request
      Then SubscriptionService DissociateProjectSubscription Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | ActionUTC             | ErrorMessage               |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US12665
Scenario Outline: SubscriptionService_DissociateProjectSubscriptionInvalidProjectUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_DissociateProjectSubscriptionInvalidProjectUID'
		And SubscriptionService DissociateProjectSubscription Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService DissociateProjectSubscription ProjectUID To '<ProjectUID>'
	    And I Post Invalid SubscriptionService DissociateProjectSubscription Request
      Then SubscriptionService DissociateProjectSubscription Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |

@Automated @Regression @Negative
@SubscriptionService @US12665
Scenario Outline: SubscriptionService_DissociateProjectSubscriptionInvalidEffectiveDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_DissociateProjectSubscriptionInvalidEffectiveDate'
		And SubscriptionService DissociateProjectSubscription Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService DissociateProjectSubscription EffectiveDate To '<EffectiveDate>'
		And I Post Invalid SubscriptionService DissociateProjectSubscription Request
      Then SubscriptionService DissociateProjectSubscription Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | ActionUTC             | ErrorMessage               |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_SubscriptionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_SubscriptionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_SubscriptionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_SubscriptionUTCInvalid |
	| EMPTY             | EMPTY_EMPTY           | ERR_SubscriptionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_DissociateProjectSubscriptionInvalidSubscriptionUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_DissociateProjectSubscriptionInvalidSubscriptionUID'
		And SubscriptionService DissociateProjectSubscription Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionService DissociateProjectSubscription SubscriptionUID To '<SubscriptionUID>'
	    And I Post Invalid SubscriptionService DissociateProjectSubscription Request
      Then SubscriptionService DissociateProjectSubscription Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage            |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionInvalid |

























































