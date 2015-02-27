Feature: Login
	I should be able to login, or not.

Background: 
	Given users/login

#@ignore
Scenario: Login with good credentials
	When login goodCredentials
	Then match response SessionId

#@ignore
Scenario: Login with bad credentials
	When login badCredentials
	Then match response (Error 401)

#@ignore
Scenario: Logout
	When login goodCredentials
	And logout
	Then match response (Ok 200)

#@ignore
Scenario: Logout then request
	When login goodCredentials
	And logout
	And getProjects
	Then match response (Error 401)
	And not $ null response 

Scenario: Request with bad session
	When use badSession
	And getProjects
	Then match response (Error 401)
