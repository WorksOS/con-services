Feature: Customer Service

   References : A. Contract Document - None
                           
   Dependencies:  Internal -  Kafka Topic

   User Story 8587:Customer Service (Master Data Management)
#______________________________________________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateHappyPath
	  Given CustomerService Is Ready To Verify 'CustomerService_CreateHappyPath'
		And CustomerServiceCreate Request Is Setup With Default Values
	  When I Post Valid CustomerServiceCreate Request  
	  Then The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Sanity @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateHappyPath
	  Given CustomerService Is Ready To Verify 'CustomerService_UpdateHappyPath'
		And CustomerServiceUpdate Request Is Setup With Default Values
	  When I Post Valid CustomerServiceUpdate Request  
      Then The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Sanity @Positive
@CustomerService @US8587
Scenario: CustomerService_DeleteHappyPath
	  Given CustomerService Is Ready To Verify 'CustomerService_DeleteHappyPath'
		And CustomerServiceDelete Request Is Setup With Default Values
	  When I Post Valid CustomerServiceDelete Request     
      Then The Processed CustomerServiceDelete Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateValidNetworkDealerCode_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_CreateValidNetworkDealerCode_Empty'
		And CustomerServiceCreate Request Is Setup With Default Values
	  When I Set CustomerServiceCreate NetworkDealerCode To 'NULL_NULL'
		And I Post Valid CustomerServiceCreate Request 
	  Then The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateValidNetworkCustomerCode_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_CreateValidNetworkCustomerCode_Empty'
		And CustomerServiceCreate Request Is Setup With Default Values
	  When I Set CustomerServiceCreate NetworkCustomerCode To 'NULL_NULL'
		And I Post Valid CustomerServiceCreate Request 
      Then The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateValidDealerAccountCode_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_CreateValidDealerAccountCode_Empty'
		And CustomerServiceCreate Request Is Setup With Default Values
	  When I Set CustomerServiceCreate DealerAccountCode To 'NULL_NULL'
	    And I Post Valid CustomerServiceCreate Request
      Then The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateValidAllOptionalAttributes_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_CreateValidAllOptionalAttributes_Empty'
		And CustomerServiceCreate Request Is Setup With Default Values
  	  When I Set CustomerServiceCreate NetworkDealerCode To 'NULL_NULL'
	    And I Set CustomerServiceCreate NetworkCustomerCode To 'NULL_NULL'
 	    And I Set CustomerServiceCreate DealerAccountCode To 'NULL_NULL'
		And I Post Valid CustomerServiceCreate Request
      Then The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario Outline: CustomerService_CreateValidCustomerType
	  Given CustomerService Is Ready To Verify 'CustomerService_CreateValidCustomerType'
		And CustomerServiceCreate Request Is Setup With Default Values
	  When I Set CustomerServiceCreate CustomerType To '<CustomerType>'
		And I Post Valid CustomerServiceCreate Request  
      Then The Processed CustomerServiceCreate Message must be available in Kafka topic
	Examples:
	| Description | CustomerType |
	| Dealer      | Dealer       |
	| Customer    | Customer     |
	| Operations  | Operations   |
	| Corporate   | Corporate    |

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidCustomerName_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidCustomerName_Empty'
		And CustomerServiceUpdate Request Is Setup With Default Values
	  When I Set CustomerServiceUpdate CustomerName To 'NULL_NULL'
	    And I Post Valid CustomerServiceUpdate Request  
      Then The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidDealerNetwork_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidDealerNetwork_Empty'
		And CustomerServiceUpdate Request Is Setup With Default Values
	  When I Set CustomerServiceUpdate DealerNetwork To 'NULL_NULL'
	    And I Post Valid CustomerServiceUpdate Request  
      Then The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidNetworkDealerCode_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidNetworkDealerCode_Empty'
		And CustomerServiceUpdate Request Is Setup With Default Values
	  When I Set CustomerServiceUpdate NetworkDealerCode To 'NULL_NULL'
	    And I Post Valid CustomerServiceUpdate Request          
     Then The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidNetworkCustomerCode_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidNetworkCustomerCode_Empty'
		And CustomerServiceUpdate Request Is Setup With Default Values
	  When I Set CustomerServiceUpdate NetworkCustomerCode To 'NULL_NULL'
	    And I Post Valid CustomerServiceUpdate Request   
      Then The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidDealerAccountCode_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidDealerAccountCode_Empty'
		And CustomerServiceUpdate Request Is Setup With Default Values
	  When I Set CustomerServiceUpdate DealerAccountCode To 'NULL_NULL'
	    And I Post Valid CustomerServiceUpdate Request 
     Then The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario Outline: CustomerService_UpdateValidOptionalAttributes
	  Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidOptionalAttributes'
		And CustomerServiceUpdate Request Is Setup With Default Values
	  When I Set CustomerServiceUpdate CustomerName To '<CustomerName>'
	  	And I Set CustomerServiceUpdate DealerNetwork To '<DealerNetwork>'
		And I Set CustomerServiceUpdate NetworkDealerCode To '<NetworkDealerCode>'
		And I Set CustomerServiceUpdate NetworkCustomerCode To '<NetworkCustomerCode>'
		And I Set CustomerServiceUpdate DealerAccountCode To '<DealerAccountCode>'
     Then The Processed CustomerServiceUpdate Message must be available in Kafka topic
	Examples:
	| Description         | CustomerName | DealerNetwork | NetworkDealerCode | NetworkCustomerCode | DealerAccountCode |
	| CustomerName        | TestCustomer | NULL_NULL     | NULL_NULL         | NULL_NULL           | NULL_NULL         |
	| DealerNetwork       | NULL_NULL    | SITECH        | NULL_NULL         | NULL_NULL           | NULL_NULL         |
	| NetworkDealerCode   | NULL_NULL    | NULL_NULL     | NDTest            | NULL_NULL           | NULL_NULL         |
	| NetworkCustomerCode | NULL_NULL    | NULL_NULL     | NULL_NULL         | NC50                | NULL_NULL         |
	| DealerAccountCode   | NULL_NULL    | NULL_NULL     | NULL_NULL         | NULL_NULL           | DA50              |

@Automated @Regression @Negative
@CustomerService @US8587
Scenario: CustomerService_CreateInvalidCustomerName_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_CreateInvalidCustomerName_Empty'
		And CustomerServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceCreate CustomerName To 'NULL_NULL'
		And I Post Invalid CustomerServiceCreate Request 
      Then CustomerServiceCreate Response With 'ERR_CustomerNameInvalid' Should Be Returned	

@Automated @Regression @Negative
@CustomerService @US8587
Scenario Outline: CustomerService_CreateInvalidCustomerType
	  Given CustomerService Is Ready To Verify 'CustomerService_CreateInvalidCustomerType'
		And CustomerServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceCreate CustomerType To '<CustomerType>'
		And I Post Invalid CustomerServiceCreate Request  
      Then CustomerServiceCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description | CustomerType | ErrorMessage            |
	| string      | abcd         | ERR_CustomerTypeInvalid |
	| NULL        | NULL_NULL    | ERR_CustomerTypeInvalid |  

@Automated @Regression @Negative
@CustomerService @US8587
Scenario: CustomerService_CreateInvalidBSSID_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_CreateInvalidBSSID_Empty'
		And CustomerServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceCreate BSSID To 'NULL_NULL'
		And I Post Invalid CustomerServiceCreate Request   
      Then CustomerServiceCreate Response With 'ERR_BSSIDInvalid' Should Be Returned

@Automated @Regression @Negative
@CustomerService @US8587
Scenario: CustomerService_CreateInvalidDealerNetwork_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_CreateInvalidDealerNetwork_Empty'
		And CustomerServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceCreate DealerNetwork To 'NULL_NULL'
		And I Post Invalid CustomerServiceCreate Request  
      Then CustomerServiceCreate Response With 'ERR_DealerNetworkInvalid' Should Be Returned	

@Automated @Regression @Negative
@CustomerService @US8587
Scenario Outline: CustomerService_CreateInvalidCustomerUID
	  Given CustomerService Is Ready To Verify '<Description>'
		And CustomerServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceCreate CustomerUID To '<CustomerUID>'
 	    And I Post Invalid CustomerServiceCreate Request  
      Then CustomerServiceCreate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |

@Automated @Regression @Negative
@CustomerService @US8587
Scenario Outline: CustomerService_CreateInvalidActionUTC
	  Given CustomerService Is Ready To Verify '<Description>'
		And CustomerServiceCreate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceCreate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid CustomerServiceCreate Request  
      Then CustomerServiceCreate Response With '<ErrorMessage>' Should Be Returned	
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
@CustomerService @US8587
Scenario Outline: CustomerService_UpdateInvalidCustomerUID
	  Given CustomerService Is Ready To Verify '<Description>'
		And CustomerServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceUpdate CustomerUID To '<CustomerUID>'
 	    And I Post Invalid CustomerServiceUpdate Request  
      Then CustomerServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |


@Automated @Regression @Negative
@CustomerService @US8587
Scenario Outline: CustomerService_UpdateInvalidActionUTC
	  Given CustomerService Is Ready To Verify '<Description>'
		And CustomerServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceUpdate ActionUTC To '<ActionUTC>'
 	    And I Post Invalid CustomerServiceUpdate Request  
      Then CustomerServiceUpdate Response With '<ErrorMessage>' Should Be Returned	
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
@CustomerService @US8587
Scenario: CustomerService_UpdateInvalidAllOptionalAttributes_Empty
	  Given CustomerService Is Ready To Verify 'CustomerService_UpdateInvalidAllOptionalAttributes_Empty'
		And CustomerServiceUpdate Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceUpdate CustomerName To '<CustomerName>'
	  	And I Set Invalid CustomerServiceUpdate DealerNetwork To '<DealerNetwork>'
		And I Set Invalid CustomerServiceUpdate NetworkDealerCode To '<NetworkDealerCode>'
		And I Set Invalid CustomerServiceUpdate NetworkCustomerCode To '<NetworkCustomerCode>'
		And I Set Invalid CustomerServiceUpdate DealerAccountCode To '<DealerAccountCode>'
		And I Post Invalid CustomerServiceUpdate Request 
      Then CustomerServiceUpdate Response With '<ERR_Invalid>' Should Be Returned	

@Automated @Regression @Negative
@CustomerService @US8587
Scenario Outline: CustomerService_DeleteInvalidCustomerUID
	  Given CustomerService Is Ready To Verify '<Description>'
		And CustomerServiceDelete Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceDelete CustomerUID To '<CustomerUID>'
 	    And I Post Invalid CustomerServiceDelete Request  
      Then CustomerServiceDelete Response With '<ErrorMessage>' Should Be Returned	
	Examples:
	| Description        | CustomerUID                          | ErrorMessage           |
	| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_CustomerUIDInvalid |
	| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_CustomerUIDInvalid |
	| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_CustomerUIDInvalid |
	| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_CustomerUIDInvalid |
	| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_CustomerUIDInvalid |
	| NULL               | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |


@Automated @Regression @Negative
@CustomerService @US8587
Scenario Outline: CustomerService_DeleteInvalidActionUTC
	  Given CustomerService Is Ready To Verify '<Description>'
		And CustomerServiceDelete Request Is Setup With Invalid Default Values
	  When I Set Invalid CustomerServiceDelete ActionUTC To '<ActionUTC>'
 	    And I Post Invalid CustomerServiceDelete Request  
      Then CustomerServiceDelete Response With '<ErrorMessage>' Should Be Returned	
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

