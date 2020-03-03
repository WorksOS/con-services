Feature: CustomerRelationshipService

   References : A. Contract Document - None
                           
   Dependencies:  Internal -  Kafka Topic

   User Story 22562: MDM: VL NG MDM API endpoint is modified to accept CreateCustomerRelationship and DeleteCustomerRelationship events from CG
#______________________________________________________________________________________________________________________________________________________

@Automated @Sanity @Positive
@CustomerRelationshipService @US22562
Scenario: CustomerRelationshipService_CreateCustomerRelationshipHappyPath
	  Given CustomerRelationshipService Is Ready To Verify 'CustomerService_CreateCustomerRelationshipHappyPath'
		And CustomerService CreateCustomerRelationship Request Is Setup With Default Values
	  When I Post Valid CustomerService CreateCustomerRelationship Request  
	  Then The CreateCustomerRelationshipEvent Details Are Stored In VSS DB
	    And The CreateCustomerRelationshipEvent Message must be available in Kafka topic

@Automated @Sanity @Positive
@CustomerRelationshipService @US22562
Scenario: CustomerRelationshipService_DeleteCustomerRelationshipHappyPath
	  Given CustomerRelationshipService Is Ready To Verify 'CustomerRelationshipService_DeleteCustomerRelationshipHappyPath'
		And CustomerService DeleteCustomerRelationship Request Is Setup With Default Values
	  When I Post Valid CustomerService DeleteCustomerRelationship Request  
	  Then The DeleteCustomerRelationshipEvent Details Are Stored In VSS DB
	    And The DeleteCustomerRelationshipEvent Message must be available in Kafka topic