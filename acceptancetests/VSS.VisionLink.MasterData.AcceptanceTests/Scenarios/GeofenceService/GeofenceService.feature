Feature: Geofence Service

   References : A. Contract Document - None
                           
   Dependencies:  Internal -  Kafka Topic

       User Story 8588:Geofence Service (Master Data Management)
#______________________________________________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@GeofenceService @US8588
Scenario: GeofenceService_CreateHappyPath
	  Given GeofenceService Is Ready To Verify 'GeofenceService_CreateHappyPath'
		And GeofenceServiceCreate Request Is Setup With Default Values
	  When I Post Valid GeofenceServiceCreate Request  
	  Then The Processed GeofenceServiceCreate Message must be available in Kafka topic

@Automated @Sanity @Positive
@GeofenceService @US8588
Scenario: GeofenceService_UpdateHappyPath
	  Given GeofenceService Is Ready To Verify 'GeofenceService_UpdateHappyPath'
		And GeofenceServiceUpdate Request Is Setup With Default Values
	  When I Post Valid GeofenceServiceUpdate Request  
      Then The Processed GeofenceServiceUpdate Message must be available in Kafka topic

@Automated @Sanity @Positive
@GeofenceService @US8588
Scenario: GeofenceService_DeleteHappyPath
	  Given GeofenceService Is Ready To Verify 'GeofenceService_DeleteHappyPath'
		And GeofenceServiceDelete Request Is Setup With Default Values
	   When I Post Valid GeofenceServiceDelete Request     
      Then The Processed GeofenceServiceDelete Message must be available in Kafka topic

@Automated @Regression @Positive
@GeofenceService @US8588
Scenario: GeofenceService_CreateValidDescription_Empty
	  Given GeofenceService Is Ready To Verify 'GeofenceService_CreateValidDescription_Empty'
		And GeofenceServiceCreate Request Is Setup With Default Values
	  When I Set GeofenceServiceCreate Description To 'NULL_NULL'	
	    And I Post Valid GeofenceServiceCreate Request 
      Then The Processed GeofenceServiceCreate Message must be available in Kafka topic		


@Automated @Regression @Positive
@GeofenceService @US8588
Scenario: GeofenceService_CreateValidFillColor_Empty
	  Given GeofenceService Is Ready To Verify 'GeofenceService_CreateValidFillColor_Empty'
		And GeofenceServiceCreate Request Is Setup With Default Values
		 When I Set GeofenceServiceCreate FillColor To 'NULL_NULL'	
	    And I Post Valid GeofenceServiceCreate Request 
      Then The Processed GeofenceServiceCreate Message must be available in Kafka topic		


@Automated @Regression @Positive
@GeofenceService @US8588
Scenario Outline: GeofenceService_CreateValidIsTransparent
	  Given GeofenceService Is Ready To Verify 'GeofenceService_CreateValidIsTransparent'
		And GeofenceServiceCreate Request Is Setup With Default Values
	  When I Set GeofenceServiceCreate IsTransparent To '<IsTransparent>'	 
		And I Post Valid GeofenceServiceCreate Request  
      Then The Processed GeofenceServiceCreate Message must be available in Kafka topic		
	Examples:
	| Description      | IsTransparent |
	| InLowerCasetrue  | true          |
	| InLowerCaseFalse | false         |
	| InUpperCasetrue  | TRUE          |
	| InUpperCaseFalse | FALSE         |

@Automated @Regression @Positive
@GeofenceService @US8588
Scenario: GeofenceService_UpdateValidGeofenceName_Empty
	  Given GeofenceService Is Ready To Verify 'GeofenceService_UpdateValidGeofenceName_Empty'
		And GeofenceServiceUpdate Request Is Setup With Default Values
	  When I Set GeofenceServiceUpdate GeofenceName To 'NULL_NULL'
		And I Post Valid GeofenceServiceUpdate Request 	
      Then The Processed GeofenceServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@GeofenceService @US8588
Scenario: GeofenceService_UpdateValidDescription_Empty
	  Given GeofenceService Is Ready To Verify 'GeofenceService_UpdateValidDescription_Empty'
		And GeofenceServiceUpdate Request Is Setup With Default Values
	  When I Set GeofenceServiceUpdate Description To 'NULL_NULL'
		And I Post Valid GeofenceServiceUpdate Request 	
      Then The Processed GeofenceServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@GeofenceService @US8588
Scenario: GeofenceService_UpdateValidGeofenceType_Empty
	  Given GeofenceService Is Ready To Verify 'GeofenceService_UpdateValidGeofenceType__Blank'
		And GeofenceServiceUpdate Request Is Setup With Default Values
	  When I Set GeofenceServiceUpdate GeofenceType To 'NULL_NULL'
		And I Post Valid GeofenceServiceUpdate Request 	
      Then The Processed GeofenceServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@GeofenceService @US8588
Scenario: GeofenceService_UpdateValidGeometryWKT_Empty
	  Given GeofenceService Is Ready To Verify 'GeofenceService_UpdateValidGeometryWKT_Empty'
		And GeofenceServiceUpdate Request Is Setup With Default Values
	  When I Set GeofenceServiceUpdate GeometryWKT To 'NULL_NULL'
		And I Post Valid GeofenceServiceUpdate Request 
      Then The Processed GeofenceServiceUpdate Message must be available in Kafka topic


@Automated @Regression @Positive
@GeofenceService @US8588
Scenario: GeofenceService_UpdateValidFillColor_Empty
	  Given GeofenceService Is Ready To Verify 'GeofencetService_UpdateValidFillColor_Empty'
		And GeofenceServiceUpdate Request Is Setup With Default Values
	   When I Set GeofenceServiceUpdate FillColor To 'NULL_NULL'
		And I Post Valid GeofenceServiceUpdate Request 
       Then The Processed GeofenceServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@GeofenceService @US8588
Scenario: GeofenceService_UpdateValidIsTransparent_Empty
	  Given GeofenceService Is Ready To Verify 'GeofenceService_UpdateValidIsTransparent_Empty'
		And GeofenceServiceUpdate Request Is Setup With Default Values
	  When I Set GeofenceServiceUpdate IsTransparent To 'NULL_NULL'
	    And I Post Valid GeofenceServiceUpdate Request 
      Then The Processed GeofenceServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@GeofenceService @US8588
Scenario: GeofenceService_UpdateValidAllOptionalAttributes_Empty
	  Given GeofenceService Is Ready To Verify 'GeofenceService_UpdateValidAllOptionalAttributes_Empty'
		And GeofenceServiceUpdate Request Is Setup With Default Values
	  When  I Set GeofenceServiceUpdate GeofenceName To 'NULL_NULL'
		And I Set GeofenceServiceUpdate Description To 'NULL_NULL'
		And I Set GeofenceServiceUpdate GeofenceType To 'NULL_NULL'
		And I Set GeofenceServiceUpdate GeometryWKT To 'NULL_NULL'
		And I Set GeofenceServiceUpdate FillColor To 'NULL_NULL'
		And I Set GeofenceServiceUpdate IsTransparent To 'NULL_NULL'
		And I Post Valid GeofenceServiceUpdate Request 
      Then The Processed GeofenceServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_CreateInvalidCustomerUID
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceCreate Request Is Setup With Default Values
	  When I Set Invalid GeofenceServiceCreate CustomerUID To '<CustomerUID>'
 	    And I Post Invalid GeofenceServiceCreate Request  
      Then GeofenceServiceCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| Blank              | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario: GeofenceService_CreateInvalidGeofenceName_Empty
	  Given GeofenceService Is Ready To Verify 'CreateInvalidGeofenceName_Empty'
		And GeofenceServiceCreate Request Is Setup With Default Values
	  When I Set Invalid GeofenceServiceCreate GeofenceName To 'NULL_NULL'
	    And I Post Invalid GeofenceServiceCreate Request
      Then GeofenceServiceCreate Response With 'ERR_GeofenceNameInvalid' Should Be Returned		

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario: GeofenceService_CreateInvalidGeofenceType_Empty
	  Given GeofenceService Is Ready To Verify 'CreateInvalidGeofenceType_Empty'
		And GeofenceServiceCreate Request Is Setup With Default Values
	  When I Set Invalid GeofenceServiceCreate GeofenceType To 'NULL_NULL'
	    And I Post Invalid GeofenceServiceCreate Request  
      Then GeofenceServiceCreate Response With 'ERR_GeofenceTypeInvalid' Should Be Returned		

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario: GeofenceService_CreateInvalidGeometryWKT_Empty
	  Given GeofenceService Is Ready To Verify 'CreateInvalidGeometryWKT_Empty'
		And GeofenceServiceCreate Request Is Setup With Default Values
	  When I Set Invalid GeofenceServiceCreate GeometryWKT To 'NULL_NULL'
	    And I Post Invalid GeofenceServiceCreate Request
      Then GeofenceServiceCreate Response With 'ERR_GeometryWKTInvalid' Should Be Returned		

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_CreateInvalidFillColor
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceCreate FillColor To '<FillColor>'
 	    And I Post Invalid GeofenceServiceCreate Request  
      Then GeofenceServiceCreate Response With '<ErrorMessage>' Should Be Returned		
	Examples:
	| Description    | FillColor | ErrorMessage         |
	| ContainsString | ABCD      | ERR_FillColorInvalid |
	| ContainsSpace  | 1 2       | ERR_FillColorInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_CreateInvalidIsTransparent
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceCreate IsTransparent To '<IsTransparent>'
 	    And I Post Invalid GeofenceServiceCreate Request  
      Then GeofenceServiceCreate Response With '<ErrorMessage>' Should Be Returned		
	Examples:
	| Description              | IsTransparent | ErrorMessage             |
	| ContainsString           | ABCD          | ERR_IsTransparentInvalid |
	| Containsnumber           | 12            | ERR_IsTransparentInvalid |
	| Containsspace            | tr ue         | ERR_IsTransparentInvalid |
	| Containsspecialcharacher | true*         | ERR_IsTransparentInvalid |
	| NULL                     | NULL_NULL     | ERR_IsTransparentInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_CreateInvalidGeofenceUID
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceCreate GeofenceUID To '<GeofenceUID>'
 	    And I Post Invalid GeofenceServiceCreate Request  
      Then GeofenceServiceCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | GeofenceUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_GeofenceUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_GeofenceUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_GeofenceUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_GeofenceUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_GeofenceUIDInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_CreateInvalidUserUID
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceCreate UserUID To '<UserUID>'
 	    And I Post Invalid GeofenceServiceCreate Request  
      Then GeofenceServiceCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | UserUID                              | ErrorMessage       |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_UserUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_UserUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_UserUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_UserUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_UserUIDInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_CreateInvalidActionUTC
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceCreate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid GeofenceServiceCreate Request  
      Then GeofenceServiceCreate Response With '<ErrorMessage>' Should Be Returned	
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
@GeofenceService @US8588
Scenario Outline: GeofenceService_UpdateInvalidFillColor
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceUpdate FillColor To '<FillColor>'
 	    And I Post Invalid GeofenceServiceUpdate Request  
      Then GeofenceServiceUpdate Response With '<ErrorMessage>' Should Be Returned		
	Examples:
	| Description    | FillColor | ErrorMessage         |
	| ContainsString | ABCD      | ERR_FillColorInvalid |
	| ContainsSpace  | 1 2       | ERR_FillColorInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_UpdateInvalidIsTransparent
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceUpdate IsTransparent To '<IsTransparent>'
 	    And I Post Invalid GeofenceServiceUpdate Request  
      Then GeofenceServiceUpdate Response With '<ErrorMessage>' Should Be Returned		
	Examples:
	| Description              | IsTransparent | ErrorMessage             |
	| ContainsString           | ABCD          | ERR_IsTransparentInvalid |
	| Containsnumber           | 12            | ERR_IsTransparentInvalid |
	| Containsspace            | tr ue         | ERR_IsTransparentInvalid |
	| Containsspecialcharacher | true*         | ERR_IsTransparentInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_UpdateInvalidGeofenceUID
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceUpdate GeofenceUID To '<GeofenceUID>'
 	    And I Post Invalid GeofenceServiceUpdate Request  
      Then GeofenceServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | GeofenceUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_GeofenceUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_GeofenceUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_GeofenceUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_GeofenceUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_GeofenceUIDInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_UpdateInvalidUserUID
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceUpdate UserUID To '<UserUID>'
 	    And I Post Invalid GeofenceServiceUpdate Request  
      Then GeofenceServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | UserUID                              | ErrorMessage       |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_UserUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_UserUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_UserUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_UserUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_UserUIDInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_UpdateInvalidActionUTC
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceUpdate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid GeofenceServiceUpdate Request  
      Then GeofenceServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
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
@GeofenceService @US8588
Scenario Outline: GeofenceService_DeleteInvalidGeofenceUID
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceDelete Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceDelete GeofenceUID To '<GeofenceUID>'
 	    And I Post Invalid GeofenceServiceDelete Request  
      Then GeofenceServiceDelete Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | GeofenceUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_GeofenceUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_GeofenceUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_GeofenceUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_GeofenceUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_GeofenceUIDInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_DeleteInvalidUserUID
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceDelete Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceDelete UserUID To '<UserUID>'
 	    And I Post Invalid GeofenceServiceDelete Request  
      Then GeofenceServiceDelete Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | UserUID                              | ErrorMessage       |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_UserUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_UserUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_UserUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_UserUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_UserUIDInvalid |

@Automated @Regression @Negative
@GeofenceService @US8588
Scenario Outline: GeofenceService_DeleteInvalidActionUTC
	  Given GeofenceService Is Ready To Verify '<Description>'
		And GeofenceServiceDelete Request Is Setup With Invalid Default Values
	  When I Set Invalid GeofenceServiceDelete ActionUTC To '<ActionUTC>'
 	    And I Post Invalid GeofenceServiceDelete Request  
      Then GeofenceServiceDelete Response With '<ErrorMessage>' Should Be Returned	
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