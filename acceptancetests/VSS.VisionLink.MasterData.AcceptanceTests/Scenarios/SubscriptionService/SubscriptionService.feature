Feature: SubscriptionService

   References : A. Contract Document - None
                           
   Dependencies:  Internal -  Kafka Topic

   User Story 8346:Group Service (Master Data Management)
#______________________________________________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_CreateHappyPath
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CreateHappyPath'
		And SubscriptionServiceCreate Request Is Setup With Default Values
	  When I Post Valid SubscriptionServiceCreate Request  
	  Then The Processed SubscriptionServiceCreate Message must be available in Kafka topic

@Automated @Sanity @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_UpdateHappyPath
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateHappyPath'
		And SubscriptionServiceUpdate Request Is Setup With Default Values
	  When I Post Valid SubscriptionServiceUpdate Request  
      Then The Processed SubscriptionServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_CreateValidAssetUID_Empty
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CreateValidAssetUID_Empty'
		And SubscriptionServiceCreate Request Is Setup With Default Values
	  When I Set SubscriptionServiceCreate AssetUID To 'NULL_NULL'
	    And I Post Valid SubscriptionServiceCreate Request
      Then The Processed SubscriptionServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CreateValidSubscriptionTypeID 
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CreateValidSubscriptionTypeID'
		And SubscriptionServiceCreate Request Is Setup With Default Values
	  When I Set SubscriptionServiceCreate SubscriptionTypeID To '<SubscriptionTypeID>'
	    And I Post Valid SubscriptionServiceCreate Request
      Then The Processed SubscriptionServiceCreate Message must be available in Kafka topic
	Examples:
	| Description                 | SubscriptionTypeID          |
	| Unknown                     | Unknown                     |
	| Essentials                  | Essentials                  |
	| ManualMaintenanceLog        | ManualMaintenanceLog        |
	| CATHealth                   | CATHealth                   |
	| StandardHealth              | StandardHealth              |
	| CATUtilization              | CATUtilization              |
	| StandardUtilization         | StandardUtilization         |
	| CATMAINT                    | CATMAINT                    |
	| VLMAINT                     | VLMAINT                     |
	| RealTimeDigitalSwitchAlerts | RealTimeDigitalSwitchAlerts |
	| e1minuteUpdateRateUpgrade   | e1minuteUpdateRateUpgrade   |
	| ConnectedSiteGateway        | ConnectedSiteGateway        |
	| e2DProjectMonitoring        | e2DProjectMonitoring        |
	| e3DProjectMonitoring        | e3DProjectMonitoring        |
	| VisionLinkRFID              | VisionLinkRFID              |
	| Manual3DProjectMonitoring   | Manual3DProjectMonitoring   |
	| VehicleConnect              | VehicleConnect              |
	| UnifiedFleet                | UnifiedFleet                |
	| AdvancedProductivity        | AdvancedProductivity        |
	| Landfill                    | Landfill                    |
	| ProjectMonitoring           | ProjectMonitoring           |

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_UpdateValidAssetUID_Empty
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateValidAssetUID_Empty'
		And SubscriptionServiceUpdate Request Is Setup With Default Values
	  When I Set SubscriptionServiceUpdate AssetUID To 'NULL_NULL'
	    And I Post Valid SubscriptionServiceUpdate Request
      Then The Processed SubscriptionServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_UpdateValidSubscriptionTypeID 
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateValidSubscriptionTypeID'
		And SubscriptionServiceUpdate Request Is Setup With Default Values
	  When I Set SubscriptionServiceUpdate SubscriptionTypeID To '<SubscriptionTypeID>'
	    And I Post Valid SubscriptionServiceUpdate Request
      Then The Processed SubscriptionServiceUpdate Message must be available in Kafka topic
	Examples:
	| Description                 | SubscriptionTypeID          |
	| Unknown                     | Unknown                     |
	| Essentials                  | Essentials                  |
	| ManualMaintenanceLog        | ManualMaintenanceLog        |
	| CATHealth                   | CATHealth                   |
	| StandardHealth              | StandardHealth              |
	| CATUtilization              | CATUtilization              |
	| StandardUtilization         | StandardUtilization         |
	| CATMAINT                    | CATMAINT                    |
	| VLMAINT                     | VLMAINT                     |
	| RealTimeDigitalSwitchAlerts | RealTimeDigitalSwitchAlerts |
	| e1minuteUpdateRateUpgrade   | e1minuteUpdateRateUpgrade   |
	| ConnectedSiteGateway        | ConnectedSiteGateway        |
	| e2DProjectMonitoring        | e2DProjectMonitoring        |
	| e3DProjectMonitoring        | e3DProjectMonitoring        |
	| VisionLinkRFID              | VisionLinkRFID              |
	| Manual3DProjectMonitoring   | Manual3DProjectMonitoring   |
	| VehicleConnect              | VehicleConnect              |
	| UnifiedFleet                | UnifiedFleet                |
	| AdvancedProductivity        | AdvancedProductivity        |
	| Landfill                    | Landfill                    |
	| ProjectMonitoring           | ProjectMonitoring           |
	| NULL                        | NULL_NULL                   |

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_UpdateValidStartDate_Empty
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateValidStartDate_Empty'
		And SubscriptionServiceUpdate Request Is Setup With Default Values
	  When I Set SubscriptionServiceUpdate StartDate  To 'NULL_NULL'
	    And I Post Valid SubscriptionServiceUpdate Request
      Then The Processed SubscriptionServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario: SubscriptionService_UpdateValidEndDate_Empty
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateValidEndDate_Empty'
		And SubscriptionServiceUpdate Request Is Setup With Default Values
	  When I Set SubscriptionServiceUpdate EndDate To 'NULL_NULL'
	    And I Post Valid SubscriptionServiceUpdate Request
      Then The Processed SubscriptionServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_UpdateValidOptionalFields
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateValidOptionalFields'
		And SubscriptionServiceUpdate Request Is Setup With Default Values
	  When I Set SubscriptionServiceUpdate AssetUID To '<AssetUID>'
		And I Set SubscriptionServiceUpdate SubscriptionTypeID To '<SubscriptionTypeID>'
		And I Set SubscriptionServiceUpdate StartDate  To '<StartDate>'
		And I Set SubscriptionServiceUpdate EndDate To '<EndDate>'
	    And I Post Valid SubscriptionServiceUpdate Request
      Then The Processed SubscriptionServiceUpdate Message must be available in Kafka topic
	  Examples:
	| Description        | AssetUID                             | SubscriptionTypeID   | StartDate           | EndDate             |
	| AssetUID           | 6CEC6135-89C8-11E5-9797-005056886D0D | NULL_NULL            | NULL_NULL           | NULL_NULL           |
	| SubscriptionTypeID | NULL_NULL                            | AdvancedProductivity | NULL_NULL           | NULL_NULL           |
	| StartDate          | NULL_NULL                            | NULL_NULL            | 2015:09:28T07:14:49 | NULL_NULL           |
	| EndDate            | NULL_NULL                            | NULL_NULL            | NULL_NULL           | 2015:09:28T07:14:49 |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CreateInvalidSubscriptionUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CreateInvalidSubscriptionUID'
		And SubscriptionServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceCreate SubscriptionUID  To '<SubscriptionUID>'
	    And I Post Invalid SubscriptionServiceCreate Request
      Then SubscriptionServiceCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | SubscriptionUID                      | ErrorMessage               |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionUIDInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionUIDInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CreateInvalidCustomerUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CreateInvalidCustomerUID'
		And SubscriptionServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceCreate CustomerUID To '<CustomerUID>'
	    And I Post Invalid SubscriptionServiceCreate Request
      Then SubscriptionServiceCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CreateInvalidAssetUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CreateInvalidAssetUID'
		And SubscriptionServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceCreate AssetUID To '<AssetUID>'
	    And I Post Invalid SubscriptionServiceCreate Request
      Then SubscriptionServiceCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_AssetUIDInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CreateInvalidSubscriptionTypeID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CreateInvalidSubscriptionTypeID'
		And SubscriptionServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceCreate SubscriptionTypeID To '<SubscriptionTypeID>'
	    And I Post Invalid SubscriptionServiceCreate Request
      Then SubscriptionServiceCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description         | SubscriptionTypeID | ErrorMessage                  |
	| InvalidSubscription | abcd               | ERR_SubscriptionTypeIDInvalid |
	| EMPTY               | EMPTY_EMPTY        | ERR_SubscriptionTypeIDInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CreateInvalidStartDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CreateInvalidStartDate'
		And SubscriptionServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceCreate StartDate To '<StartDate>'
	    And I Post Invalid SubscriptionServiceCreate Request
      Then SubscriptionServiceCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | StartDate             | ErrorMessage         |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_StartDateInvalid |
	| ContainsBackSlash | 2015/09/28T07:14:49   | ERR_StartDateInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_StartDateInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_StartDateInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_StartDateInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_StartDateInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_StartDateInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_StartDateInvalid |
	| NULL              | EMPTY_EMPTY           | ERR_StartDateInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CreateInvalidEndDate_Empty
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_CreateInvalidEndDate'
		And SubscriptionServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceCreate EndDate To '<EndDate>'
	    And I Post Invalid SubscriptionServiceCreate Request
      Then SubscriptionServiceCreate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | EndDate               | ErrorMessage       |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_EndDateInvalid |
	| ContainsBackSlash | 2015/09/28T07:14:49   | ERR_EndDateInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_EndDateInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_EndDateInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_EndDateInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_EndDateInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_EndDateInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_EndDateInvalid |
	| NULL              | EMPTY_EMPTY           | ERR_EndDateInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_CreateInvalidActionUTC
	  Given SubscriptionService Is Ready To Verify '<Description>'
		And SubscriptionServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceCreate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid SubscriptionServiceCreate Request  
      Then SubscriptionServiceCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description       | ActionUTC             | ErrorMessage         |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_ActionUTCInvalid |
	| ContainsBackSlash | 2015/09/28T07:14:49   | ERR_ActionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_ActionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_ActionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_ActionUTCInvalid |
	| NULL              | EMPTY_EMPTY           | ERR_ActionUTCInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_UpdateInvalidSubscriptionUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateInvalidSubscriptionUID'
		And SubscriptionServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceUpdate SubscriptionUID  To '<SubscriptionUID>'
	    And I Post Invalid SubscriptionServiceUpdate Request
      Then SubscriptionServiceUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | SubscriptionUID                      | ErrorMessage               |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_SubscriptionUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_SubscriptionUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_SubscriptionUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_SubscriptionUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_SubscriptionUIDInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_SubscriptionUIDInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_UpdateInvalidCustomerUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateInvalidCustomerUID'
		And SubscriptionServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceUpdate CustomerUID To '<CustomerUID>'
	    And I Post Invalid SubscriptionServiceUpdate Request
      Then SubscriptionServiceUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_UpdateInvalidAssetUID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateInvalidAssetUID'
		And SubscriptionServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceUpdate AssetUID To '<AssetUID>'
	    And I Post Invalid SubscriptionServiceUpdate Request
      Then SubscriptionServiceUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | CustomerUID                          | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_AssetUIDInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_UpdateInvalidSubscriptionTypeID
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateInvalidSubscriptionTypeID'
		And SubscriptionServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceUpdate SubscriptionTypeID To '<SubscriptionTypeID>'
	    And I Post Invalid SubscriptionServiceUpdate Request
      Then SubscriptionServiceUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description         | SubscriptionTypeID | ErrorMessage                  |
	| InvalidSubscription | abcd               | ERR_SubscriptionTypeIDInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_UpdateInvalidStartDate
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateInvalidStartDate'
		And SubscriptionServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceUpdate StartDate To '<StartDate>'
	    And I Post Invalid SubscriptionServiceUpdate Request
      Then SubscriptionServiceUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | StartDate             | ErrorMessage         |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_StartDateInvalid |
	| ContainsBackSlash | 2015/09/28T07:14:49   | ERR_StartDateInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_StartDateInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_StartDateInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_StartDateInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_StartDateInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_StartDateInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_StartDateInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_UpdateInvalidEndDate_Empty
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateInvalidEndDate'
		And SubscriptionServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceUpdate EndDate To '<EndDate>'
	    And I Post Invalid SubscriptionServiceUpdate Request
      Then SubscriptionServiceUpdate Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description       | EndDate               | ErrorMessage       |
	| ContainsColon     | 2015:09::28T07:14:49  | ERR_EndDateInvalid |
	| ContainsBackSlash | 2015/09/28T07:14:49   | ERR_EndDateInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_EndDateInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_EndDateInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_EndDateInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_EndDateInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_EndDateInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_EndDateInvalid |

@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario: SubscriptionService_UpdateInvalidAllOptionalFields_Empty
	  Given SubscriptionService Is Ready To Verify 'SubscriptionService_UpdateInvalidAllOptionalFields_Empty'
		And SubscriptionServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceUpdate AssetUID To 'NULL_NULL'
		And I Set Invalid SubscriptionServiceUpdate SubscriptionTypeID To 'NULL_NULL'
		And I Set Invalid SubscriptionServiceUpdate StartDate To 'NULL_NULL'
		And I Set Invalid SubscriptionServiceUpdate EndDate To 'NULL_NULL'
	    And I Post Invalid SubscriptionServiceUpdate Request
      Then SubscriptionServiceUpdate Response With '<ERR_SubscriptionUpdateInvalid>' Should Be Returned


@Automated @Regression @Negative
@SubscriptionService @US9655
Scenario Outline: SubscriptionService_UpdateInvalidActionUTC
	  Given SubscriptionService Is Ready To Verify '<Description>'
		And SubscriptionServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid SubscriptionServiceUpdate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid SubscriptionServiceUpdate Request  
      Then SubscriptionServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description       | ActionUTC             | ErrorMessage         |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_ActionUTCInvalid |
	| ContainsBackSlash | 2015/09/28T07:14:49   | ERR_ActionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_ActionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_ActionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_ActionUTCInvalid |
	| NULL              | EMPTY_EMPTY           | ERR_ActionUTCInvalid |
