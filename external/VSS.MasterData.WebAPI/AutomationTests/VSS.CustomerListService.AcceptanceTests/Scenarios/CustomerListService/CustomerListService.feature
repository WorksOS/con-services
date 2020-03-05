Feature: CustomerListService
References:
			-https://docs.google.com/document/d/1Y_30fqzm9Okxlbm1w5GSsDopq_dvstjM7P0K6y-ktpE/edit

Dependencies:  Internal -  Kafka Topic
   					   VSS DB	- VSS-Customer
		  	       Table - Customer, UserCustomer	 
User Story: 
		11456 - Create windows servcie to read the user customer data from Kafka
    10229 - Implement the API end-point to get customer list for the user

@Automated @Sanity @Positive
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_CreateCustomerEvent_HappyPath
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_CreateCustomerEvent_HappyPath'
		    And CustomerListConsumerService CreateCustomerEvent Request Is Setup With Default Values        
	  When I Post Valid CustomerListConsumerService CreateCustomerEvent Request  
	  Then The CreateCustomerEvent Details Are Stored In VSS DB

@Automated @Regression @Positive
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_CreateCustomerEvent_UpdateName
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_CreateCustomerEvent_UpdateName'
		    And CustomerListConsumerService CreateCustomerEvent Request Is Setup With Default Values
	  When I Post Valid CustomerListConsumerService CreateCustomerEvent Request  
        And Update The CustomerListConsumerService CreateCustomerEvent Request With Different Customer Name
        And I Post Valid CustomerListConsumerService CreateCustomerEvent Request  
	  Then The CreateCustomerEvent Details Are Updated In VSS DB

@Automated @Regression @Positive
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_CreateCustomerEvent_UpdateTypeId
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_CreateCustomerEvent_UpdateTypeId'
		    And CustomerListConsumerService CreateCustomerEvent Request Is Setup With Default Values
	  When I Post Valid CustomerListConsumerService CreateCustomerEvent Request  
        And Update The CustomerListConsumerService CreateCustomerEvent Request With Different Customer TypeId
        And I Post Valid CustomerListConsumerService CreateCustomerEvent Request  
	  Then The CreateCustomerEvent Details Are NOT Updated In VSS DB        

@Automated @Sanity @Positive
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_UpdateCustomerEvent_HappyPath
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_UpdateCustomerEvent_HappyPath'
        And CustomerListConsumerService CreateCustomerEvent Request Is Setup With Default Values		    
	  When I Post Valid CustomerListConsumerService CreateCustomerEvent Request   
        And CustomerListConsumerService UpdateCustomerEvent Request Is Setup With Default Values
        And I Post Valid CustomerListConsumerService UpdateCustomerEvent Request  
	  Then The UpdateCustomerEvent Details Are Updated In VSS DB
       
@Automated @Regression @Positive
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_DeleteCustomerEvent_HappyPath
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_DeleteCustomerEvent_HappyPath'
        And CustomerListConsumerService CreateCustomerEvent Request Is Setup With Default Values		    
	  When I Post Valid CustomerListConsumerService CreateCustomerEvent Request   
        And CustomerListConsumerService DeleteCustomerEvent Request Is Setup With Default Values
        And I Post Valid CustomerListConsumerService DeleteCustomerEvent Request  
	  Then The DeleteCustomerEvent Details Are Removed In VSS DB

@Automated @Sanity @Positive
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_AssociateCustomerUserEvent_HappyPath
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_AssociateCustomerUserEvent_HappyPath'
		    And CustomerListConsumerService AssociateCustomerUserEvent Request Is Setup With Default Values
	  When I Post Valid CustomerListConsumerService AssociateCustomerUserEvent Request  
	  Then The AssociateCustomerUserEvent Details Are Stored In VSS DB

@Automated @Regression @Positive
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_AssociateMultipleCustomerUserEvent_HappyPath
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_AssociateMultipleCustomerUserEvent_HappyPath'
		    And CustomerListConsumerService AssociateCustomerUserEvent Request Is Setup With Multiple Customer Default Values
	  When I Post Valid CustomerListConsumerService AssociateCustomerUserEvent Request For Multiple Customers 
	  Then The AssociateCustomerUserEvent Details For All Customers Are Stored In VSS DB

@Automated @Regression @Negative
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_AssociateCustomerUserEvent_InvalidCustomerUID
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_AssociateCustomerUserEvent_InvalidCustomerUID'		    
	  When I Set CustomerListConsumerService AssociateCustomerUserEvent CustomerUID Which DoesNotExist
        And I Post Valid CustomerListConsumerService AssociateCustomerUserEvent Request  
	  Then The AssociateCustomerUserEvent Details Are NOT Stored In VSS DB
        
@Automated @Sanity @Positive
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_DissociateCustomerUserEvent_HappyPath
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_DissociateCustomerUserEvent_HappyPath'
		    And CustomerListConsumerService DissociateCustomerUserEvent Request Is Setup With Default Values
	  When I Post Valid CustomerListConsumerService DissociateCustomerUserEvent Request  
	  Then The DissociateCustomerUserEvent Details Are Removed In VSS DB

@Automated @Regression @Negative
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_DissociateCustomerUserEvent_InvalidCustomerUID
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_DissociateCustomerUserEvent_InvalidCustomerUID'
		    And CustomerListConsumerService DissociateCustomerUserEvent Request Is Setup With Default Values
	  When I Set CustomerListConsumerService DissociateCustomerUserEvent CustomerUID Which DoesNotExist
        And I Post Valid CustomerListConsumerService DissociateCustomerUserEvent Request  
	  Then The DissociateCustomerUserEvent Details For Invalid Customer NOT Removed In VSS DB

@Automated @Regression @Negative
@CustomerListConsumerService @US11456
Scenario: CustomerListConsumerService_DissociateCustomerUserEvent_InvalidUserUID
	  Given CustomerListConsumerService Is Ready To Verify 'CustomerListConsumerService_DissociateCustomerUserEvent_InvalidUserUID'
		    And CustomerListConsumerService DissociateCustomerUserEvent Request Is Setup With Default Values
	  When I Set CustomerListConsumerService DissociateCustomerUserEvent UserUID Which DoesNotExist
        And I Post Valid CustomerListConsumerService DissociateCustomerUserEvent Request  
	  Then The DissociateCustomerUserEvent Details Are Invalid User NOT Removed In VSS DB

@Automated @Sanity @Positive
@CustomerListWebApi @US9992
Scenario: CustomerListWebApi_GetUserCustomerList_HappyPath
	  Given CustomerListWebApi Is Ready To Verify 'CustomerListWebApi_GetUserCustomerList_HappyPath'
        And User Created And Associated With Single Customer		    
	  When I Post Valid CustomerListWebApi GetUserCustomerList Request  
	  Then The GetUserCustomerList Response Should Return Customer Details

@Automated @Regression @Positive
@CustomerListWebApi @US9992
Scenario: CustomerListWebApi_GetUserCustomerList_Multiple
	  Given CustomerListWebApi Is Ready To Verify 'CustomerListWebApi_GetUserCustomerList_HappyPath'
        And User Created And Associated With Multiple Customers		    
	  When I Post Valid CustomerListWebApi GetUserMultipleCustomerList Request
	  Then The GetUserCustomerList Response Should Return Multiple Customers Details
    
@Automated @Regression @Negative
@CustomerListWebApi @US9992
Scenario Outline: CustomerListWebApi_GetUserCustomerList_InvalidAccessToken
	  Given CustomerListWebApi Is Ready To Verify '<Description>'
        And User Created And Associated With Single Customer		            
	  When I Post InValid CustomerListWebApi GetUserCustomerList Request With '<AccessToken>'
	  Then The GetUserCustomerList Response Should Return '<ErrorMessage>'
    Examples:
	| Description | AccessToken                      | ErrorMessage     |
	| Invalid     | e041692df491461821ceb691fd9f351a | ERR_InvalidToken |
	| Empty       | EMPTY_EMPTY                      | ERR_EmptyToken   |
	| Expired     | d041692df491461821ceb691fd9f351a | ERR_ExpiredToken |
	