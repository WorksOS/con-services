Feature: MasterData

Scenario: Create customer
	Given I inject 'CreateCustomerEvent' into Kafka
	Then a new 'Customer' is created

Scenario: Update customer
	Given I inject 'CreateCustomerEvent' into Kafka
		And a new 'Customer' is created
	When I inject 'UpdateCustomerEvent' into Kafka
	Then the new 'Customer' is updated

Scenario: Delete customer
	Given I inject 'CreateCustomerEvent' into Kafka
		And a new 'Customer' is created
	When I inject 'DeleteCustomerEvent' into Kafka
	Then the new 'Customer' is deleted

Scenario: Associate user customer
	Given I inject 'CreateCustomerEvent' into Kafka
		And I inject 'AssociateCustomerUserEvent' into Kafka
	Then user and customer are associated

Scenario: Associate project subscription
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is in the list
		And the number of days to subscription expiry is correct

Scenario: Associate project subscription out of order
	Given I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'CreateProjectEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is in the list
		And the number of days to subscription expiry is correct

Scenario: Update project subscription
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I inject 'UpdateProjectSubscriptionEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is in the list
		And the updated number of days to subscription expiry is correct

Scenario: Update project details
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'UpdateProjectEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the project details are updated

Scenario: Delete project
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'DeleteProjectEvent' into Kafka
	When I make a Web API request for a list of projects
	Then the created project is not in the list

Scenario: Create geofence - geofence list
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'CreateProjectGeofenceEvent' into Kafka
		And I inject 'AssociateProjectGeofenceEvent' into Kafka
	When I make a Web API request for a list of geofences
	Then the created geofence is in the list

Scenario: Create geofence - geofence boundary
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'CreateProjectGeofenceEvent' into Kafka
		And I inject 'AssociateProjectGeofenceEvent' into Kafka
		And I make a Web API request for a list of geofences
		And the created geofence is in the list
	When I make a Web API request for the boundary of the geofence
	Then the geofence boundary points are correct

Scenario: Update geofence
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'CreateProjectGeofenceEvent' into Kafka
		And I inject 'AssociateProjectGeofenceEvent' into Kafka
		And I make a Web API request for a list of geofences
		And the created geofence is in the list
		And I inject 'UpdateProjectGeofenceEvent' into Kafka
	When I make a Web API request for a list of geofences
	Then the geofence details are updated

Scenario: Delete geofence
	Given I inject 'CreateProjectEvent' into Kafka
		And I inject 'CreateProjectSubscriptionEvent' into Kafka
		And I inject 'AssociateProjectCustomer' into Kafka
		And I inject 'AssociateProjectSubscriptionEvent' into Kafka
		And I make a Web API request for a list of projects
		And the created project is in the list
		And I inject 'CreateProjectGeofenceEvent' into Kafka
		And I inject 'AssociateProjectGeofenceEvent' into Kafka
		And I make a Web API request for a list of geofences
		And the created geofence is in the list
		And I inject 'DeleteProjectGeofenceEvent' into Kafka
	When I make a Web API request for a list of geofences
	Then the created geofence is not in the list

Scenario: Add landfill geofence
	Given I set up a project for customer 'Middleton'
	When I add landfill site 'Marylands' to the project of customer 'Middleton'
	Then the landfill site is in the geofence list of the project of customer 'Middleton'
