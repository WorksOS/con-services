Feature: CustomerService
	References : A. Contract Document - None
	Dependencies:  Internal -  Kafka Topic
	User Story 8587:Customer Micro Service (Master Data Management)
#_______________________________________________________________________________________

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateHappyPath
	Given CustomerService Is Ready To Verify 'CustomerService_CreateHappyPath'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Post Valid CustomerServiceCreate Request  
	Then The CreateCustomerEvent Details Are Stored In VSS DB
	#And The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Sanity @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateHappyPath
	Given CustomerService Is Ready To Verify 'CustomerService_UpdateHappyPath'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Post Valid CustomerServiceUpdate Request  
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Sanity @Positive
@CustomerService @US8587
Scenario: CustomerService_DeleteHappyPath
	Given CustomerService Is Ready To Verify 'CustomerService_DeleteHappyPath'
	And CustomerServiceDelete Request Is Setup With Default Values
	When I Post Valid CustomerServiceDelete Request     
    Then The DeleteCustomerEvent Details Are Removed In VSS DB
	And The Processed CustomerServiceDelete Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateValidBSSID_Empty
	Given CustomerService Is Ready To Verify 'CustomerService_CreateValidBSSID_Empty'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Set CustomerServiceCreate BSSID To 'NULL_NULL'
	And I Post Valid CustomerServiceCreate Request   
	Then The CreateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateValidNetworkDealerCode_NULL
	Given CustomerService Is Ready To Verify 'CustomerService_CreateValidNetworkDealerCode_NULL'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Set CustomerServiceCreate NetworkDealerCode To 'NULL_NULL'
	And I Post Valid CustomerServiceCreate Request 
	Then The CreateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateValidNetworkCustomerCode_NULL
	Given CustomerService Is Ready To Verify 'CustomerService_CreateValidNetworkCustomerCode_NULL'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Set CustomerServiceCreate NetworkCustomerCode To 'NULL_NULL'
	And I Post Valid CustomerServiceCreate Request 
	Then The CreateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateValidDealerAccountCode_NULL
	Given CustomerService Is Ready To Verify 'CustomerService_CreateValidDealerAccountCode_NULL'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Set CustomerServiceCreate DealerAccountCode To 'NULL_NULL'
	And I Post Valid CustomerServiceCreate Request
	Then The CreateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US33087
Scenario Outline: CustomerService_CreateValidPrimaryContactEmail
	Given CustomerService Is Ready To Verify '<Description>'
    And CustomerServiceCreate Request Is Setup With Default Values
	When I Set CustomerServiceCreate PrimaryContactEmail To '<PrimaryContactEmail>'
	And I Post Valid CustomerServiceCreate Request
	Then The CreateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceCreate Message must be available in Kafka topic
Examples:
	| Description | PrimaryContactEmail |
	| NULL        | NULL_NULL           |
	| EMPTY       | EMPTY_EMPTY         |

@Automated @Regression @Positive
@CustomerService @US33087
Scenario Outline: CustomerService_CreateValidFirstName
	Given CustomerService Is Ready To Verify '<Description>'
    And CustomerServiceCreate Request Is Setup With Default Values
	When I Set CustomerServiceCreate FirstName To '<FirstName>'
	And I Post Valid CustomerServiceCreate Request
	Then The CreateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceCreate Message must be available in Kafka topic
Examples:
	| Description | FirstName   |
	| NULL        | NULL_NULL   |
	| EMPTY       | EMPTY_EMPTY |

@Automated @Regression @Positive
@CustomerService @US33087
Scenario Outline: CustomerService_CreateValidLastName
	Given CustomerService Is Ready To Verify '<Description>'
    And CustomerServiceCreate Request Is Setup With Default Values
	When I Set CustomerServiceCreate LastName To '<LastName>'
	And I Post Valid CustomerServiceCreate Request
	Then The CreateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceCreate Message must be available in Kafka topic
Examples:
	| Description | LastName    |
	| NULL        | NULL_NULL   |
	| EMPTY       | EMPTY_EMPTY |

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_CreateValidAllOptionalAttributes_NULL
	Given CustomerService Is Ready To Verify 'CustomerService_CreateValidAllOptionalAttributes_NULL'
	And CustomerServiceCreate Request Is Setup With Default Values
  	When I Set CustomerServiceCreate NetworkDealerCode To 'NULL_NULL'
	And I Set CustomerServiceCreate BSSID To 'NULL_NULL'
	And I Set CustomerServiceCreate NetworkCustomerCode To 'NULL_NULL'
 	And I Set CustomerServiceCreate DealerAccountCode To 'NULL_NULL'
	And I Set CustomerServiceCreate PrimaryContactEmail To 'NULL_NULL'
	And I Set CustomerServiceCreate FirstName To 'NULL_NULL'
	And I Set CustomerServiceCreate LastName To 'NULL_NULL'
	And I Post Valid CustomerServiceCreate Request
	Then The CreateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario Outline: CustomerService_CreateValidCustomerType
	Given CustomerService Is Ready To Verify 'CustomerService_CreateValidCustomerType'
	And CustomerServiceCreate Request Is Setup With Default Values
	When I Set CustomerServiceCreate CustomerType To '<CustomerType>'
	And I Post Valid CustomerServiceCreate Request  
	Then The CreateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceCreate Message must be available in Kafka topic
Examples:
	| Description | CustomerType |
	| Dealer      | Dealer       |
	| Customer    | Customer     |
	| Operations  | Operations   |
	| Corporate   | Corporate    |

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidCustomerName_NULL
	Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidCustomerName_NULL'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Set CustomerServiceUpdate CustomerName To 'NULL_NULL'
	And I Post Valid CustomerServiceUpdate Request  
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidBSSID_NULL
	Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidBSSID_NULL'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Set CustomerServiceUpdate BSSID To 'NULL_NULL'
	And I Post Valid CustomerServiceUpdate Request  
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidDealerNetwork_NULL
	Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidDealerNetwork_NULL'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Set CustomerServiceUpdate DealerNetwork To 'NULL_NULL'
	And I Post Valid CustomerServiceUpdate Request  
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidNetworkDealerCode_NULL
	Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidNetworkDealerCode_NULL'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Set CustomerServiceUpdate NetworkDealerCode To 'NULL_NULL'
	And I Post Valid CustomerServiceUpdate Request          
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidNetworkCustomerCode_NULL
	Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidNetworkCustomerCode_NULL'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Set CustomerServiceUpdate NetworkCustomerCode To 'NULL_NULL'
	And I Post Valid CustomerServiceUpdate Request   
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US8587
Scenario: CustomerService_UpdateValidDealerAccountCode_NULL
	Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidDealerAccountCode_NULL'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Set CustomerServiceUpdate DealerAccountCode To 'NULL_NULL'
	And I Post Valid CustomerServiceUpdate Request 
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@CustomerService @US33087
Scenario Outline: CustomerService_UpdateValidPrimaryContactEmail
	Given CustomerService Is Ready To Verify '<Description>'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Set CustomerServiceUpdate PrimaryContactEmail To '<PrimaryContactEmail>'
	And I Post Valid CustomerServiceUpdate Request 
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic
Examples:
	| Description | PrimaryContactEmail |
	| NULL        | NULL_NULL           |
	| EMPTY       | EMPTY_EMPTY         |

@Automated @Regression @Positive
@CustomerService @US33087
Scenario Outline: CustomerService_UpdateValidFirstName
	Given CustomerService Is Ready To Verify '<Description>'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Set CustomerServiceUpdate FirstName To '<FirstName>'
	And I Post Valid CustomerServiceUpdate Request 
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic
Examples:
	| Description | FirstName   |
	| NULL        | NULL_NULL   |
	| EMPTY       | EMPTY_EMPTY |

@Automated @Regression @Positive
@CustomerService @US33087
Scenario Outline: CustomerService_UpdateValidLastName
	Given CustomerService Is Ready To Verify '<Description>'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Set CustomerServiceUpdate LastName To '<LastName>'
	And I Post Valid CustomerServiceUpdate Request 
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic
Examples:
	| Description | LastName    |
	| NULL        | NULL_NULL   |
	| EMPTY       | EMPTY_EMPTY |

@Automated @Regression @Positive
@CustomerService @US8587
Scenario Outline: CustomerService_UpdateValidOptionalAttributes
	Given CustomerService Is Ready To Verify 'CustomerService_UpdateValidOptionalAttributes'
	And CustomerServiceUpdate Request Is Setup With Default Values
	When I Set CustomerServiceUpdate CustomerName To '<CustomerName>'
	And I Set CustomerServiceUpdate BSSID To '<BSSID>'
	And I Set CustomerServiceUpdate DealerNetwork To '<DealerNetwork>'
	And I Set CustomerServiceUpdate NetworkDealerCode To '<NetworkDealerCode>'
	And I Set CustomerServiceUpdate NetworkCustomerCode To '<NetworkCustomerCode>'
	And I Set CustomerServiceUpdate DealerAccountCode To '<DealerAccountCode>'
	And I Set CustomerServiceUpdate PrimaryContactEmail To '<PrimaryContactEmail>'
	And I Set CustomerServiceUpdate FirstName To '<FirstName>'
	And I Set CustomerServiceUpdate LastName To '<LastName>'
	And I Post Valid CustomerServiceUpdate Request 
    Then The UpdateCustomerEvent Details Are Stored In VSS DB
	And The Processed CustomerServiceUpdate Message must be available in Kafka topic
Examples:
	| Description         | CustomerName    | BSSID                | DealerNetwork | NetworkDealerCode | NetworkCustomerCode | DealerAccountCode | PrimaryContactEmail     | FirstName         | LastName         |
	| BSSID               | NULL_NULL       | -8002120150629121003 | NULL_NULL     | NULL_NULL         | NULL_NULL           | NULL_NULL         | NULL_NULL               | NULL_NULL         | NULL_NULL        |
	| DealerNetwork       | NULL_NULL       | NULL_NULL            | SITECH        | NULL_NULL         | NULL_NULL           | NULL_NULL         | NULL_NULL               | NULL_NULL         | NULL_NULL        |
	| NetworkDealerCode   | NULL_NULL       | NULL_NULL            | NULL_NULL     | NDTest            | NULL_NULL           | NULL_NULL         | NULL_NULL               | NULL_NULL         | NULL_NULL        |
	| NetworkCustomerCode | NULL_NULL       | NULL_NULL            | NULL_NULL     | NULL_NULL         | NC50                | NULL_NULL         | NULL_NULL               | NULL_NULL         | NULL_NULL        |
	| DealerAccountCode   | NULL_NULL       | NULL_NULL            | NULL_NULL     | NULL_NULL         | NULL_NULL           | DA50              | NULL_NULL               | NULL_NULL         | NULL_NULL        |
	| PrimaryContactEmail | NewAutoCustomer | NULL_NULL            | NULL_NULL     | NULL_NULL         | NULL_NULL           | NULL_NULL         | AutoTest123@trimble.com | NULL_NULL         | NULL_NULL        |
	| FirstName           | NewAutoCustomer | NULL_NULL            | NULL_NULL     | NULL_NULL         | NULL_NULL           | NULL_NULL         | NULL_NULL               | AutoTestFirstName | NULL_NULL        |
	| LastName            | NewAutoCustomer | NULL_NULL            | NULL_NULL     | NULL_NULL         | NULL_NULL           | NULL_NULL         | NULL_NULL               | NULL_NULL         | AutoTestLastName |

@Automated @Regression @Negative
@CustomerService @US8587
Scenario: CustomerService_CreateInvalid_Duplicate
	Given CustomerService Is Ready To Verify 'CustomerService_CreateInvalid_Duplicate'
	And CustomerServiceCreate Request Is Setup With Invalid Default Values  
	When I Set Duplicate CustomerUID Value To '<CustomerUID>'
	And I Post Invalid CustomerServiceCreate Request 
    Then CustomerServiceCreate Response With 'ERR_Duplicate' Should Be Returned	

@Automated @Regression @Negative
@CustomerService @US8587
Scenario: CustomerService_UpdateInvalid_NonExistingCustomer
	Given CustomerService Is Ready To Verify 'CustomerService_UpdateInvalid_NonExistingCustomer'
	And CustomerServiceUpdate Request Is Setup With Invalid Default Values
	When I Post Invalid CustomerServiceUpdate Request 
    Then CustomerServiceCreate Response With 'ERR_NonExistingCustomer' Should Be Returned	

@Automated @Regression @Negative
@CustomerService @US8587
Scenario: CustomerService_DeleteInvalid_NonExistingCustomer
	Given CustomerService Is Ready To Verify 'CustomerService_DeleteInvalid_NonExistingCustomer'
	And CustomerServiceDelete Request Is Setup With Invalid Default Values  
	When I Post Invalid CustomerServiceDelete Request 
	Then CustomerServiceCreate Response With 'ERR_NonExistingCustomer' Should Be Returned	

@Automated @Regression @Negative
@CustomerService @US8587
Scenario Outline: CustomerService_CreateInvalidCustomerName
	Given CustomerService Is Ready To Verify 'CustomerService_CreateInvalidCustomerName_Empty'
	And CustomerServiceCreate Request Is Setup With Invalid Default Values
	When I Set Invalid CustomerServiceCreate CustomerName To '<CustomerName>'
	And I Post Invalid CustomerServiceCreate Request 
    Then CustomerServiceCreate Response With '<ErrorMessage>' Should Be Returned	
Examples:
	| Description | CustomerName | ErrorMessage            |
	| NULL        | NULL_NULL    | ERR_CustomerNameInvalid |
	| EMPTY       | EMPTY_EMPTY  | ERR_CustomerNameInvalid |

@Automated @Regression @Negative
@CustomerService @US8587
Scenario Outline: CustomerService_CreateInvalidCustomerType
	Given CustomerService Is Ready To Verify 'CustomerService_CreateInvalidCustomerType'
	And CustomerServiceCreate Request Is Setup With Invalid Default Values
	When I Set Invalid CustomerServiceCreate CustomerType To '<CustomerType>'
	And I Post Invalid CustomerServiceCreate Request  
    Then CustomerServiceCreate Response With '<ErrorMessage>' Should Be Returned	
Examples:
	| Description | CustomerType | ErrorMessage             |
	| string      | abcd         | ERR_CustomerTypeInvalid  |
	| NULL        | NULL_NULL    | ERR_CustomerTypeRequired |
	| EMPTY       | EMPTY_EMPTY  | ERR_CustomerTypeRequired |

@Automated @Regression @Negative
@CustomerService @US8587
Scenario Outline: CustomerService_CreateInvalidDealerNetwork
	Given CustomerService Is Ready To Verify 'CustomerService_CreateInvalidDealerNetwork'
	And CustomerServiceCreate Request Is Setup With Invalid Default Values
	When I Set Invalid CustomerServiceCreate DealerNetwork To '<DealerNetwork>'
	And I Post Invalid CustomerServiceCreate Request  
    Then CustomerServiceCreate Response With '<ErrorMessage>' Should Be Returned	
Examples:
	| Description | DealerNetwork | ErrorMessage             |
	| NULL        | NULL_NULL     | ERR_DealerNetworkInvalid |
	| EMPTY       | EMPTY_EMPTY   | ERR_DealerNetworkInvalid |
	
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
	| EMPTY              | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |

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
	| EMPTY             | EMPTY_EMPTY           | ERR_ActionUTCInvalid |
	
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
	| EMPTY              | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |

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
	| EMPTY             | EMPTY_EMPTY           | ERR_ActionUTCInvalid |

@Automated @Regression @Negative
@CustomerService @US8587
Scenario: CustomerService_UpdateInvalidAllOptionalAttributes_Empty
	Given CustomerService Is Ready To Verify 'CustomerService_UpdateInvalidAllOptionalAttributes_Empty'
	And CustomerServiceUpdate Request Is Setup With Invalid Default Values
	When I Set Invalid CustomerServiceUpdate CustomerName To 'EMPTY_EMPTY'
	And I Set Invalid CustomerServiceUpdate BSSID To 'EMPTY_EMPTY'
	And I Set Invalid CustomerServiceUpdate DealerNetwork To 'EMPTY_EMPTY'
	And I Set Invalid CustomerServiceUpdate NetworkDealerCode To 'EMPTY_EMPTY'
	And I Set Invalid CustomerServiceUpdate NetworkCustomerCode To 'EMPTY_EMPTY'
	And I Set Invalid CustomerServiceUpdate DealerAccountCode To 'EMPTY_EMPTY'
	And I Set Invalid CustomerServiceUpdate PrimaryContactEmail To 'EMPTY_EMPTY'
	And I Set Invalid CustomerServiceUpdate FirstName To 'EMPTY_EMPTY'
	And I Set Invalid CustomerServiceUpdate LastName To 'EMPTY_EMPTY'
	And I Post Invalid CustomerServiceUpdate Request 
    Then CustomerServiceUpdate Response With 'ERR_UpdateInvalid' Should Be Returned	

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
	| EMPTY              | EMPTY_EMPTY                          | ERR_CustomerUIDInvalid |

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
	| EMPTY             | EMPTY_EMPTY           | ERR_ActionUTCInvalid |	