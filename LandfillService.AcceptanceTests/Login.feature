Feature: Login
	I should be able to login, or not.

#Background: 
#	Given uri users/login

Scenario: Login with good credentials
	When login goodCredentials
	Then match response SessionId

Scenario: Login with bad credentials
	When login badCredentials
	Then match response (Error 401)

Scenario: Logout
	When login goodCredentials
	And logout
	Then match response (Ok 200)

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
