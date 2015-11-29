Feature: Projects

	Dependencies:	Internal  - AutomationCoreAPI
					External  - Kafka queue, Landfill WebApi, mySql database, project monitoring and Foreman Web Api's

Background: 
Given login goodCredentials
And Get Project data for 'Casella-Stanley Landfill'


@Manual @Sanity @Positive
@Projects