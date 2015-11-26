Feature: Customer Asset Service

   References : A. Contract Document - None
                           
   Dependencies:  Internal -  Kafka Topic

   User Story 8587:Customer Asset Service (Master Data Management)
#______________________________________________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@CustomerAssetService @US8587
Scenario: CustomerAssetService_AssociateHappyPath
	  Given CustomerAssetService Is Ready To Verify 'CustomerAssetService_AssociateHappyPath'
		And CustomerAssetServiceAssociate Request Is Setup With Default Values
	  When I Post Valid CustomerAssetServiceAssociate Request  
	  Then The Processed CustomerAssetServiceAssociate Message must be available in Kafka topic

@Automated @Sanity @Positive
@CustomerAssetService @US8587
Scenario: CustomerAssetService_DissociateHappyPath
	  Given CustomerAssetService Is Ready To Verify 'CustomerAssetService_AssociateHappyPath'
		And CustomerAssetServiceDissociate Request Is Setup With Default Values
	  When I Post Valid CustomerAssetServiceDissociate Request  
	  Then The Processed CustomerAssetServiceDissociate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerAssetService @US8587
Scenario Outline: CustomerAssetService_AssociateValidRelationType
	  Given CustomerAssetService Is Ready To Verify 'CustomerAssetService_AssociateValidRelationType'
		And CustomerAssetServiceAssociate Request Is Setup With Default Values
	  When I Set CustomerAssetServiceAssociate RelationType To '<RelationType>'
 	    And I Post Valid CustomerAssetServiceAssociate Request 
	  Then The Processed CustomerAssetServiceAssociate Message must be available in Kafka topic
	Examples:
	| Description | RelationType |
	| Owner       | Owner        |
	| Customer    | Customer     |
	| Dealer      | Dealer       |
	| Operations  | Operations   |
	| Corporate   | Corporate    |
	| SharedOwner | SharedOwner  |

@Automated @Regression @Negative
@CustomerAssetService @US8587
Scenario Outline: CustomerAssetService_AssociateInvalidRelationType
	  Given CustomerAssetService Is Ready To Verify '<Description>'
		And CustomerAssetServiceAssociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerAssetServiceAssociate RelationType To '<RelationType>'
 	    And I Post Invalid CustomerAssetServiceAssociate Request  
      Then CustomerAssetServiceAssociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | RelationType | ErrorMessage            |
	| string             | abc          | ERR_RelationTypeInvalid |
	| ContainsSpace      | deal er      | ERR_RelationTypeInvalid |
	| ContainsUnderScore | custom_er    | ERR_RelationTypeInvalid |
	| SplChar            | corpor*ate   | ERR_RelationTypeInvalid |
	| NULL               | NULL_NULL    | ERR_RelationTypeInvalid |

@Automated @Regression @Negative
@CustomerAssetService @US8587
Scenario Outline: CustomerAssetService_AssociateInvalidCustomerUID
	  Given CustomerAssetService Is Ready To Verify '<Description>'
		And CustomerAssetServiceAssociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerAssetServiceAssociate CustomerUID To '<CustomerUID>'
 	    And I Post Invalid CustomerAssetServiceAssociate Request  
      Then CustomerAssetServiceAssociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |


@Automated @Regression @Negative
@CustomerAssetService @US8587
Scenario Outline: CustomerAssetService_AssociateInvalidAssetUID
	  Given CustomerAssetService Is Ready To Verify '<Description>'
		And CustomerAssetServiceAssociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerAssetServiceAssociate AssetUID To '<AssetUID>'
 	    And I Post Invalid CustomerAssetServiceAssociate Request 
      Then CustomerAssetServiceAssociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | AssetUID                             | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |


@Automated @Regression @Negative
@CustomerAssetService @US8587
Scenario Outline: CustomerAssetService_AssociateInvalidActionUTC
	  Given CustomerAssetService Is Ready To Verify '<Description>'
		And CustomerAssetServiceAssociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerAssetServiceAssociate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid CustomerAssetServiceAssociate Request   
      Then CustomerAssetServiceAssociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description       | ActionUTC             | ErrorMessage         |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_ActionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_ActionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_ActionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_ActionUTCInvalid |
	| NULL              | EMPTY_EMPTY           | ERR_ActionUTCInvalid |

@Automated @Regression @Negative
@CustomerAssetService @US8587
Scenario Outline: CustomerAssetService_DissociateInvalidCustomerUID
	  Given CustomerAssetService Is Ready To Verify '<Description>'
		And CustomerAssetServiceDissociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceDissociate CustomerUID To '<CustomerUID>'
 	    And I Post Invalid CustomerAssetServiceDissociate Request  
      Then CustomerAssetServiceDissociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |


@Automated @Regression @Negative
@CustomerAssetService @US8587
Scenario Outline: CustomerAssetService_DissociateInvalidAssetUID
	  Given CustomerAssetService Is Ready To Verify '<Description>'
		And CustomerAssetServiceDissociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceDissociate AssetUID To '<AssetUID>'
 	    And I Post Invalid CustomerAssetServiceDissociate Request  
      Then CustomerAssetServiceDissociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | AssetUID                             | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |


@Automated @Regression @Negative
@CustomerAssetService @US8587
Scenario Outline: CustomerAssetService_DissociateInvalidActionUTC
	  Given CustomerAssetService Is Ready To Verify '<Description>'
		And CustomerAssetServiceDissociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceDissociate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid CustomerAssetServiceDissociate Request    
      Then CustomerAssetServiceDissociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description       | ActionUTC             | ErrorMessage         |
	| ContainsColon     | 2015:09:28T07:14:49   | ERR_ActionUTCInvalid |
	| ContainsBackSlash | 2015//09/28T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidYear       | 20155:09:28T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidMonth      | 2015:099:28T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidDate       | 2015:09:288T07:14:49  | ERR_ActionUTCInvalid |
	| InvalidHour       | 2015:09:28T070:14:49  | ERR_ActionUTCInvalid |
	| InvalidMinutes    | 2015:09:288T07:144:49 | ERR_ActionUTCInvalid |
	| InvalidSeconds    | 2015:09:288T07:14:499 | ERR_ActionUTCInvalid |
	| NULL              | EMPTY_EMPTY           | ERR_ActionUTCInvalid |

