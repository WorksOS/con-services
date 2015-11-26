Feature: GroupService

   References : A. Contract Document - None
                           
   Dependencies:  Internal -  Kafka Topic

   User Story 8346:Group Service (Master Data Management)
#______________________________________________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@GroupService @US8346
Scenario: GroupService_CreateHappyPath
	  Given GroupService Is Ready To Verify 'GroupService_CreateHappyPath'
		And GroupServiceCreate Request Is Setup With Default Values
	  When I Post Valid GroupServiceCreate Request  
	  Then The Processed GroupServiceCreate Message must be available in Kafka topic

@Automated @Sanity @Positive
@GroupService @US8346
Scenario: GroupService_UpdateHappyPath
	  Given GroupService Is Ready To Verify 'GroupService_UpdateHappyPath'
		And GroupServiceUpdate Request Is Setup With Default Values
	  When I Post Valid GroupServiceUpdate Request  
      Then The Processed GroupServiceUpdate Message must be available in Kafka topic

@Automated @Sanity @Positive
@GroupService @US8346
Scenario: GroupService_DeleteHappyPath
	  Given GroupService Is Ready To Verify 'GroupService_DeleteHappyPath'
		And GroupServiceDelete Request Is Setup With Default Values
	   When I Post Valid GroupServiceDelete Request     
      Then The Processed GroupServiceDelete Message must be available in Kafka topic

@Automated @Regression @Positive
@GroupService @US8346
Scenario: GroupService_UpdateValidGroupName_Empty
	  Given GroupService Is Ready To Verify 'GroupService_UpdateValidGroupName_Empty'
		And GroupServiceUpdate Request Is Setup With Default Values
	  When I Set GroupServiceUpdate GroupName To 'NULL_NULL'	
		And I Post Valid GroupServiceUpdate Request
     Then The Processed GroupServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@GroupService @US8346
Scenario: GroupService_UpdateValidAssociatedAssetUID_Empty
	  Given GroupService Is Ready To Verify 'GroupService_UpdateValidAssociatedAssetUID_Empty'
		And GroupServiceUpdate Request Is Setup With Default Values
	  When I Set GroupServiceUpdate AssociatedAssetUID To 'NULL_NULL'	
		And I Post Valid GroupServiceUpdate Request
     Then The Processed GroupServiceUpdate Message must be available in Kafka topic
	 		
@Automated @Regression @Positive
@GroupService @US8346
Scenario: GroupService_UpdateValidDisssociatedAssetUID_Empty
	  Given GroupService Is Ready To Verify 'GroupService_UpdateValidDissociatedAssetUID_Empty'
		And GroupServiceUpdate Request Is Setup With Default Values
	  When I Set GroupServiceUpdate DissociatedAssetUID To 'NULL_NULL'	
		And I Post Valid GroupServiceUpdate Request
     Then The Processed GroupServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@GroupService @US8346
Scenario Outline: GroupService_UpdateValidOptionalAttributes
	  Given GroupService Is Ready To Verify 'GroupService_UpdateValidOptionalAttributes'
		And GroupServiceUpdate Request Is Setup With Default Values
	  When I Set GroupServiceUpdate GroupName To '<GroupName>'
	  	And I Set GroupServiceUpdate AssociatedAssetUID To '<AssociatedAssetUID>'
        And I Set GroupServiceUpdate DissociatedAssetUID To '<DissociatedAssetUID>'	
		And I Post Valid GroupServiceUpdate Request
     Then The Processed GroupServiceUpdate Message must be available in Kafka topic
	 Examples:
	| Description         | GroupName | AssociatedAssetUID                   | DissociatedAssetUID                  |
	| GroupName           | TestGroup | NULL_NULL                            | NULL_NULL                            |
	| AssociatedAssetUID  | NULL_NULL | 6CEC6135-89C8-11E5-9797-005056886D0D | NULL_NULL                            |
	| DissociatedAssetUID | NULL_NULL | NULL_NULL                            | 6CEC6135-89C8-11E5-9797-005056886D0D |

@Automated @Regression @Negative
@GroupService @US8346 @Work
Scenario: GroupService_CreateInvalidGroupName_Empty
	  Given GroupService Is Ready To Verify 'GroupService_CreateInvalidGroupName_Empty'
		And GroupServiceCreate Request  Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceCreate GroupName To 'EMPTY_EMPTY '
		And I Post Invalid GroupServiceCreate Request  
      Then GroupServiceCreate Response With 'ERR_GroupNameInvalid' Should Be Returned	

@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_CreateInvalidCustomerUID
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceCreate Request  Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceCreate CustomerUID To '<CustomerUID>'
 	    And I Post Invalid GroupServiceCreate Request  
      Then GroupServiceCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |

@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_CreateInvalidUserUID
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceCreate Request  Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceCreate UserUID To '<UserUID>'
 	    And I Post Invalid GroupServiceCreate Request  
      Then GroupServiceCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | UserUID                              | ErrorMessage       |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_UserUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_UserUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_UserUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_UserUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_UserUIDInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_UserUIDInvalid |

@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_CreateInvalidAssetUID
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceCreate Request  Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceCreate AssetUID To '<AssetUID>'
 	    And I Post Invalid GroupServiceCreate Request  
      Then GroupServiceCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | AssetUID                             | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_AssetUIDInvalid |
	| EMPTY              | EMPTY_EMPTY                          | ERR_AssetUIDInvalid |

@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_CreateInvalidGroupUID
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceCreate Request  Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceCreate GroupUID To '<GroupUID>'
 	    And I Post Invalid GroupServiceCreate Request  
      Then GroupServiceCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | GroupUID                             | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_GroupUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_GroupUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_GroupUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_GroupUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_GroupUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_GroupUIDInvalid |

@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_CreateInvalidActionUTC
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceCreate Request  Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceCreate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid GroupServiceCreate Request  
      Then GroupServiceCreate Response With '<ErrorMessage>' Should Be Returned	
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
@GroupService @US8346
Scenario Outline: GroupService_UpdateInvalidUserUID
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceUpdate UserUID To '<UserUID>'
 	    And I Post Invalid GroupServiceUpdate Request  
      Then GroupServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | UserUID                              | ErrorMessage       |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_UserUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_UserUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_UserUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_UserUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_UserUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_UserUIDInvalid |

@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_UpdateInvalidAssociatedAssetUID
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceUpdate AssociatedAssetUID To '<AssociatedAssetUID>'
 	    And I Post Invalid GroupServiceUpdate Request  
      Then GroupServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | AssociatedAssetUID                   | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_AssetUIDInvalid |


@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_UpdateInvalidDissociatedAssetUID
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceUpdate DissociatedAssetUID To '<DissociatedAssetUID>'
 	    And I Post Invalid GroupServiceUpdate Request  
      Then GroupServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | DissociatedAssetUID                  | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_AssetUIDInvalid |


@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_UpdateInvalidGroupUID
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceUpdate GroupUID To '<GroupUID>'
 	    And I Post Invalid GroupServiceUpdate Request  
      Then GroupServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | GroupUID                             | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_GroupUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_GroupUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_GroupUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_GroupUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_GroupUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_GroupUIDInvalid |


@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_UpdateInvalidActionUTC
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceUpdate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid GroupServiceUpdate Request  
      Then GroupServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
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
@GroupService @US8346
Scenario: GroupService_UpdateInvalidAllOptionalAttributes_Empty
	  Given GroupService Is Ready To Verify 'GroupService_UpdateInvalidAllOptionalAttributes_Empty'
		And GroupServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceUpdate GroupName To 'EMPTY_EMPTY'
	  	And I Set Invalid GroupServiceUpdate AssociatedAssetUID To 'EMPTY_EMPTY'
        And I Set Invalid GroupServiceUpdate DissociatedAssetUID To 'EMPTY_EMPTY'	
		And I Post Invalid GroupServiceUpdate Request
      Then GroupServiceDelete Response With '<ERR_UserUIDInvalid>' Should Be Returned	

@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_DeleteInvalidUserUID
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceDelete Request Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceDelete UserUID To '<UserUID>'
 	    And I Post Invalid GroupServiceDelete Request  
      Then GroupServiceDelete Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | UserUID                              | ErrorMessage       |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_UserUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_UserUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_UserUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_UserUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_UserUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_UserUIDInvalid |

@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_DeleteInvalidGroupUID
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceDelete Request Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceDelete GroupUID To '<GroupUID>'
 	    And I Post Invalid GroupServiceDelete Request  
      Then GroupServiceDelete Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | GroupUID                             | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_GroupUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_GroupUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_GroupUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_GroupUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_GroupUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_GroupUIDInvalid |

@Automated @Regression @Negative
@GroupService @US8346
Scenario Outline: GroupService_DeleteInvalidActionUTC
	  Given GroupService Is Ready To Verify '<Description>'
		And GroupServiceDelete Request Is Setup With Invalid Default Values
	  When I Set Invalid GroupServiceDelete ActionUTC To '<ActionUTC>'
 	    And I Post Invalid GroupServiceDelete Request  
      Then GroupServiceDelete Response With '<ErrorMessage>' Should Be Returned	
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







