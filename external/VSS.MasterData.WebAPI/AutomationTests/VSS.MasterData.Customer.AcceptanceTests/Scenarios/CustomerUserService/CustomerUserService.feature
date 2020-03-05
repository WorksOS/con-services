Feature: CustomerUserService

   References : A. Contract Document - None
                           
   Dependencies:  Internal -  Kafka Topic

   User Story 8587:Customer Micro Service (Master Data Management)
#______________________________________________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@CustomerUserService @US8587
Scenario: CustomerUserService_AssociateHappyPath
	  Given CustomerUserService Is Ready To Verify 'CustomerUserService_AssociateHappyPath'
		And CustomerUserServiceAssociate Request Is Setup With Default Values
	  When I Post Valid CustomerUserServiceAssociate Request  
	  Then The AssociateCustomerUserEvent Details Are Stored In VSS DB
	    And The Processed CustomerUserServiceAssociate Message must be available in Kafka topic

@Automated @Sanity @Positive
@CustomerUserService @US8587
Scenario: CustomerUserService_DissociateHappyPath
	  Given CustomerUserService Is Ready To Verify 'CustomerUserService_DissociateHappyPath'
		And CustomerUserServiceDissociate Request Is Setup With Default Values
	  When I Post Valid CustomerUserServiceDissociate Request  
	  Then The DissociateCustomerUserEvent Details Are Removed In VSS DB
	    And The Processed CustomerUserServiceDissociate Message must be available in Kafka topic

@Automated @Regression @Negative
@CustomerUserService @US8587
Scenario: CustomerUserService_AssociateInvalid_Duplicate
	  Given CustomerUserService Is Ready To Verify 'CustomerAssetService_AssociateInvalid_Duplicate'
		And CustomerUserServiceAssociate Request Is Setup With Invalid Default Values  
	  When I Set Duplicate AssociateUserCustomer
		And I Post Invalid CustomerUserServiceAssociate Request 
      Then CustomerUserServiceAssociate Response With 'ERR_DuplicateCustomerUser' Should Be Returned	

@Automated @Regression @Negative
@CustomerUserService @US8587
Scenario: CustomerUserService_AssociateInvalid_NonExistingCustomer
	  Given CustomerUserService Is Ready To Verify 'CustomerAssetService_AssociateInvalid_NonExistingCustomer'
		And CustomerUserServiceAssociate Request Is Setup With Invalid Default Values  
	  When I Post Invalid CustomerUserServiceAssociate Request 
      Then CustomerUserServiceAssociate Response With 'ERR_NonExistingCustomer' Should Be Returned	

@Automated @Regression @Negative
@CustomerUserService @US8587
Scenario: CustomerUserService_DissociateInvalid_NonExistingCustomer
	  Given CustomerUserService Is Ready To Verify 'CustomerAssetService_DissociateInvalid_NonExistingCustomer'
		And CustomerUserServiceDissociate Request Is Setup With Invalid Default Values  
	  When I Post Invalid CustomerUserServiceDissociate Request 
      Then CustomerUserServiceDissociate Response With 'ERR_NonExistingCustomerUser' Should Be Returned	

@Automated @Regression @Negative
@CustomerUserService @US8587
Scenario: CustomerUserService_DissociateInvalid_NonExistingCustomerUser
	  Given CustomerUserService Is Ready To Verify 'CustomerUserService_DissociateInvalid_NonExistingCustomerUser'
		And CustomerUserServiceDissociate Request Is Setup With Invalid Default Values  
	  When I Set DissociateCustomerUser to a non existing user
	    And I Post Invalid CustomerUserServiceDissociate Request 
      Then CustomerUserServiceDissociate Response With 'ERR_NonExistingCustomerUser' Should Be Returned	

@Automated @Regression @Negative
@CustomerUserService @US8587
Scenario Outline: CustomerUserService_AssociateInvalidCustomerUID
	  Given CustomerUserService Is Ready To Verify '<Description>'
		And CustomerUserServiceAssociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerUserServiceAssociate CustomerUID To '<CustomerUID>'
 	    And I Post Invalid CustomerUserServiceAssociate Request  
      Then CustomerUserServiceAssociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |


@Automated @Regression @Negative
@CustomerUserService @US8587
Scenario Outline: CustomerUserService_AssociateInvalidUserUID
	  Given CustomerUserService Is Ready To Verify '<Description>'
		And CustomerUserServiceAssociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerUserServiceAssociate UserUID To '<UserUID>'
 	    And I Post Invalid CustomerUserServiceAssociate Request  
      Then CustomerUserServiceAssociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | UserUID                              | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |


@Automated @Regression @Negative
@CustomerUserService @US8587
Scenario Outline: CustomerUserService_AssociateInvalidActionUTC
	  Given CustomerUserService Is Ready To Verify '<Description>'
		And CustomerUserServiceAssociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerUserServiceAssociate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid CustomerUserServiceAssociate Request  
      Then CustomerUserServiceAssociate Response With '<ErrorMessage>' Should Be Returned	
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
@CustomerUserService @US8587
Scenario Outline: CustomerUserService_DissociateInvalidCustomerUID
	  Given CustomerUserService Is Ready To Verify '<Description>'
		And CustomerUserServiceDissociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerUserServiceDissociate CustomerUID To '<CustomerUID>'
 	    And I Post Invalid CustomerUserServiceDissociate Request  
      Then CustomerUserServiceDissociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |


@Automated @Regression @Negative
@CustomerUserService @US8587
Scenario Outline: CustomerUserService_DissociateInvalidUserUID
	  Given CustomerUserService Is Ready To Verify '<Description>'
		And CustomerUserServiceDissociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerUserServiceDissociate UserUID To '<UserUID>'
 	    And I Post Invalid CustomerUserServiceDissociate Request  
      Then CustomerUserServiceDissociate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | UserUID                             | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |


@Automated @Regression @Negative
@CustomerUserService @US8587
Scenario Outline: CustomerUserService_DissociateInvalidActionUTC
	  Given CustomerUserService Is Ready To Verify '<Description>'
		And CustomerUserServiceDissociate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerUserServiceDissociate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid CustomerUserServiceDissociate Request  
      Then CustomerUserServiceDissociate Response With '<ErrorMessage>' Should Be Returned	
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

