Feature: Assets Service

   References : A. Contract Document - None
                           
   Dependencies:  Internal -  Kafka Topic

       User Story 7349:Assets Service (Master Data Management)
#______________________________________________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@AssetService @US7349
Scenario: AssetService_CreateHappyPath
	  Given AssetService Is Ready To Verify 'AssetService_CreateHappyPath'
		And AssetServiceCreate Request Is Setup With Default Values
	  When I Post Valid AssetServiceCreate Request  
	  Then The Processed AssetServiceCreate Message must be available in Kafka topic

@Automated @Sanity @Positive
@AssetService @US7349
Scenario: AssetService_UpdateHappyPath
	  Given AssetService Is Ready To Verify 'AssetService_UpdateHappyPath'
		And AssetServiceUpdate Request Is Setup With Default Values
	  When I Post Valid AssetServiceUpdate Request  
      Then The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Sanity @Positive
@AssetService @US7349
Scenario: AssetService_DeleteHappyPath
	  Given AssetService Is Ready To Verify 'AssetService_DeleteHappyPath'
		And AssetServiceDelete Request Is Setup With Default Values
	  When I Post Valid AssetServiceDelete Request     
      Then The Processed AssetServiceDelete Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349
Scenario Outline: AssetService_CreateValidMakeCode
	  Given AssetService Is Ready To Verify 'AssetService_CreateValidMakeCode'
		And AssetServiceCreate Request Is Setup With Default Values
	  When I Set AssetServiceCreate MakeCode To '<MakeCode>'	 
		And I Post Valid AssetServiceCreate Request  
      Then The Processed AssetServiceCreate Message must be available in Kafka topic		
	Examples:
	| Description | MakeCode |
	| InLowerCase | cat      |
	| InUpperCase | CAT      |

@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_CreateValidAssetName_NULL
	  Given AssetService Is Ready To Verify 'AssetService_CreateValidAssetName_NULL'
		And AssetServiceCreate Request Is Setup With Default Values
	  When I Set AssetServiceCreate AssetName To 'NULL_NULL'	
	    And I Post Valid AssetServiceCreate Request
	  Then The Processed AssetServiceCreate Message must be available in Kafka topic		

@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_CreateValidAssetType_NULL
	  Given AssetService Is Ready To Verify 'AssetService_CreateValidAssetType_NULL'
		And AssetServiceCreate Request Is Setup With Default Values
	  When I Set AssetServiceCreate AssetType To 'NULL_NULL'	
	    And I Post Valid AssetServiceCreate Request
	  Then The Processed AssetServiceCreate Message must be available in Kafka topic			

@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_CreateValidModel_NULL
	  Given AssetService Is Ready To Verify 'AssetService_CreateValidModel_NULL'
		And AssetServiceCreate Request Is Setup With Default Values
	  When I Set AssetServiceCreate Model To 'NULL_NULL'	
	    And I Post Valid AssetServiceCreate Request
	  Then The Processed AssetServiceCreate Message must be available in Kafka topic			

@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_CreateValidModelYear_NULL
	  Given AssetService Is Ready To Verify 'AssetService_CreateValidModelYear_NULL'
		And AssetServiceCreate Request Is Setup With Default Values
	  When I Set AssetServiceCreate ModelYear To 'NULL_NULL'	
	    And I Post Valid AssetServiceCreate Request
	  Then The Processed AssetServiceCreate Message must be available in Kafka topic			

@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_CreateValidEquipmentVIN_NULL
	  Given AssetService Is Ready To Verify 'AssetService_CreateValidEquipmentVIN_NULL'
		And AssetServiceCreate Request Is Setup With Default Values
	  When I Set AssetServiceCreate EquipmentVIN To 'NULL_NULL'	
	    And I Post Valid AssetServiceCreate Request
	  Then The Processed AssetServiceCreate Message must be available in Kafka topic			

@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_CreateValidIconKey_NULL
	  Given AssetService Is Ready To Verify 'AssetService_CreateValidIconKey_NULL'
		And AssetServiceCreate Request Is Setup With Default Values
	  When I Set AssetServiceCreate IconKey To 'NULL_NULL'	
	    And I Post Valid AssetServiceCreate Request
	  Then The Processed AssetServiceCreate Message must be available in Kafka topic
	  
@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_CreateValidOptionalFields_NULL
	  Given AssetService Is Ready To Verify 'AssetService_CreateValidOptionalFields_NULL'
		And AssetServiceCreate Request Is Setup With Default Values
	  When I Post Valid AssetServiceCreate Request With The Below Values
	    | AssetName | AssetType | Model     | ModelYear | EquipmentVIN | IconKey   |
	    | NULL_NULL | NULL_NULL | NULL_NULL | NULL_NULL | NULL_NULL    | NULL_NULL |
     Then The Processed AssetServiceCreate Message must be available in Kafka topic				

@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_UpdateValidAssetName_NULL
	  Given AssetService Is Ready To Verify 'AssetService_UpdateValidAssetName_NULL'
		And AssetServiceUpdate Request Is Setup With Default Values
	  When I Post Valid AssetServiceUpdate Request With The Below Values
	 | AssetName | AssetType | Model | ModelYear | EquipmentVIN | IconKey |
	 | NULL_NULL | Loader    | A60   | 2010      | TestAsset123 | 30      |
     Then The Processed AssetServiceUpdate Message must be available in Kafka topic
		
@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_UpdateValidAssetType_NULL
	  Given AssetService Is Ready To Verify 'AssetService_UpdateValidAssetType_NULL'
		And AssetServiceUpdate Request Is Setup With Default Values
	  When I Post Valid AssetServiceUpdate Request With The Below Values
		| AssetName    | AssetType | Model | ModelYear | EquipmentVIN | IconKey |
		| TestAsset256 | NULL_NULL | B89   | 2013      | TestAsset256 | 17      |
     Then The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_UpdateValidAssetModel_NULL
	  Given AssetService Is Ready To Verify 'AssetService_UpdateValidAssetModel_NULL'
		And AssetServiceUpdate Request Is Setup With Default Values
	  When I Post Valid AssetServiceUpdate Request With The Below Values
		| AssetName    | AssetType | Model     | ModelYear | EquipmentVIN | IconKey |
		| TestAsset892 | LOADER    | NULL_NULL | 2011      | TestAsset256 | 30      |
     Then The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_UpdateValidAssetModelYear_NULL
	  Given AssetService Is Ready To Verify 'AssetService_UpdateValidAssetModelYear_NULL'
		And AssetServiceUpdate Request Is Setup With Default Values
	  When I Post Valid AssetServiceUpdate Request With The Below Values
		| AssetName    | AssetType             | Model | ModelYear | EquipmentVIN | IconKey |
		| TestAsset892 | MULTI TERRAIN LOADERS | B89   | NULL_NULL | TestAsset256 | 17      |
	  Then The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_UpdateValidEquipmentVIN_NULL
	  Given AssetService Is Ready To Verify 'AssetService_UpdateValidEquipmentVIN_NULL'
		And AssetServiceUpdate Request Is Setup With Default Values
	  When I Post Valid AssetServiceUpdate Request With The Below Values
	    | AssetName    | AssetType  | Model | ModelYear | EquipmentVIN | IconKey |
	    | TestAssetayt | PIPELAYERS | H88   | 2011      | NULL_NULL    | 30      |
     Then The Processed AssetServiceUpdate Message must be available in Kafka topic
	 	 
@Automated @Regression @Positive
@AssetService @US7349
Scenario: AssetService_UpdateValidIconKey_Blank
	  Given AssetService Is Ready To Verify 'AssetService_UpdateValidIconKey_Blank'
		And AssetServiceUpdate Request Is Setup With Default Values
	  When I Post Valid AssetServiceUpdate Request With The Below Values
	    | AssetName    | AssetType  | Model | ModelYear | EquipmentVIN | IconKey   |
	    | TestAsset145 | PIPELAYERS | K90   | 2011      | TestAsset256 | NULL_NULL |
     Then The Processed AssetServiceUpdate Message must be available in Kafka topic	  

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_CreateInvalidAssetUID
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceCreate AssetUID To '<AssetUID>'
 	    And I Post Invalid AssetServiceCreate Request  
      Then AssetService Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | AssetUID                             | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_AssetUIDInvalid |
	| NULL               | NULL_NULL                            | ERR_AssetUIDInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario: AssetService_CreateInvalidSerialNumber_NULL
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceCreate SerialNumber To 'NULL_NULL'
 	    And I Post Invalid AssetServiceCreate Request   
      Then AssetService Response With 'ERR_SerialNumberInvalid' Should Be Returned		

@Automated @Regression @Negative
@AssetService @US7349
Scenario: AssetService_CreateInvalidMakeCode_NULL
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceCreate MakeCode To 'NULL_NULL'
 	    And I Post Invalid AssetServiceCreate Request   
      Then AssetService Response With 'ERR_MakeCodeInvalid' Should Be Returned

@Automated @Regression @Negative
@AssetService @US7349	
Scenario Outline: AssetService_CreateInvalidIconKey
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceCreate IconKey To '<IconKey>'
 	    And I Post Invalid AssetServiceCreate Request  
      Then AssetService Response With '<ErrorMessage>' Should Be Returned		
	Examples:
	| Description   | IconKey                                 | ErrorMessage       |
	| String        | abc                                     | ERR_IconKeyInvalid |
	| ContainsSpace | 1 2                                     | ERR_IconKeyInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_CreateInvalidModelYear
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceCreate ModelYear To '<ModelYear>'
 	    And I Post Invalid AssetServiceCreate Request   
      Then AssetService Response With '<ErrorMessage>' Should Be Returned		
	Examples:
	| Description   | ModelYear                               | ErrorMessage         |
	| String        | abc                                     | ERR_ModelYearInvalid |
	| ContainsSpace | 1 2                                     | ERR_ModelYearInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_CreateInvalidActionUTC
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceCreate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid AssetServiceCreate Request   
      Then AssetService Response With '<ErrorMessage>' Should Be Returned		
	Examples:
	| Description   | ActionUTC          | ErrorMessage  |
	| String        | abc                | ERR_ActionUTC |
	| ContainsSpace | 1 2                | ERR_ActionUTC |
	| NotInDateTime | 2015-2-13-14-22:02 | ERR_ActionUTC |

@Automated @Regression @Negative
@AssetService @US7349
Scenario: AssetService_UpdateInvalidOptionalFields_Blank
	  Given AssetService Is Ready To Verify 'AssetService_UpdateValidOptionalFields_Blank'
		And AssetServiceUpdate Request Is Setup With Invalid Default Values
	  When I Post Invalid AssetServiceUpdate Request With The Below Values
	    | AssetName | AssetType | Model     | ModelYear | EquipmentVIN | IconKey   |
	    | NULL_NULL | NULL_NULL | NULL_NULL | NULL_NULL | NULL_NULL    | NULL_NULL |
      Then AssetService Response With 'ERR_Invalid' Should Be Returned	 

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_UpdateInvalidAssetUID
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceUpdate AssetUID To '<AssetUID>'
 	    And I Post Invalid AssetServiceUpdate Request  
      Then AssetService Response With '<ErrorMessage>' Should Be Returned
	Examples:
	| Description        | AssetUID                             | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_AssetUIDInvalid |
	| NULL               | NULL_NULL                            | ERR_AssetUIDInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_UpdateInvalidModelYear
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceUpdate ModelYear To '<ModelYear>'
	    And I Post Invalid AssetServiceUpdate Request 
      Then AssetService Response With '<ErrorMessage>' Should Be Returned				
	Examples:
	| Description   | ModelYear                               | ErrorMessage         |
	| String        | abc                                     | ERR_ModelYearInvalid |
	| ContainsSpace | 1 2                                     | ERR_ModelYearInvalid |
	| SplChar       | &^9AB056CA.,-3514-E411-8AF_24FD5231FB1F | ERR_ModelYearInvalid | 

@Automated @Regression @Negative
@AssetService @US7349	
Scenario Outline: AssetService_UpdateInvalidIconKey
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceUpdate IconKey To '<IconKey>'
 	    And I Post Invalid AssetServiceUpdate Request 
      Then AssetService Response With '<ErrorMessage>' Should Be Returned			
	Examples:
	| Description   | IconKey | ErrorMessage       |
	| String        | abc     | ERR_IconKeyInvalid |
	| ContainsSpace | 1 2     | ERR_IconKeyInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_UpdateInvalidActionUTC
	  Given AssetService Is Ready To Verify '<Description>'
  		And AssetServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceUpdate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid AssetServiceUpdate Request   
      Then AssetService Response With '<ErrorMessage>' Should Be Returned		
	Examples:
	| Description   | ActionUTC          | ErrorMessage  |
	| String        | abc                | ERR_ActionUTC |
	| ContainsSpace | 1 2                | ERR_ActionUTC |
	| NotInDateTime | 2015-2-13-14-22:02 | ERR_ActionUTC |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_DeleteInvalidAssetUID
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceDelete Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceDelete AssetUID To '<AssetUID>'
 	    And I Post Invalid AssetServiceDelete Request 
      Then AssetService Response With '<ErrorMessage>' Should Be Returned		
	Examples:
	| Description        | AssetUID                             | ErrorMessage        |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
	| NULL               | NULL_NULL                            | ERR_AssetUIDInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_DeleteInvalidActionUTC
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceDelete Request Is Setup With Invalid Default Values
	  When I Set Invalid AssetServiceDelete ActionUTC To '<ActionUTC>'
 	    And I Post Invalid AssetServiceDelete Request   
      Then AssetService Response With '<ErrorMessage>' Should Be Returned		
	Examples:
	| Description   | ActionUTC          | ErrorMessage  |
	| String        | abc                | ERR_ActionUTC |
	| ContainsSpace | 2015 13 15         | ERR_ActionUTC |
	| NotInDateTime | 2015-2-13-14-22:02 | ERR_ActionUTC |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_CreateInvalidContentType
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceCreate Request Is Setup With Invalid Default Values
	  When I Post AssetServiceCreate Request With Invalid ContentType '<Value>' 
      Then AssetService Response With '<ErrorMessage>' Should Be Returned
	Examples:
    | Description | Value | ErrorMessage           |
    | HTML        | HTML  | ERR_InvalidContentType |
    | XML         | XML   | ERR_InvalidContentType |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_UpdateInvalidContentType
	  Given AssetService Is Ready To Verify '<Description>'
		And AssetServiceUpdate Request Is Setup With Invalid Default Values
	  When I Post Invalid AssetServiceUpdate Request With Invalid ContentType '<Value>' 
      Then AssetService Response With '<ErrorMessage>' Should Be Returned
	Examples:
    | Description | Value | ErrorMessage           |
    | HTML        | HTML  | ERR_InvalidContentType |
    | XML         | XML   | ERR_InvalidContentType |







