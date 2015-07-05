Feature: Login
	I should be able to login, or not.

#Background: 
#	Given uri users/login

Scenario: Login with good credentials
	When login goodCredentials
	Then match response SessionId

Scenario: Login with invalid username
	When login invalidUsername
	Then match response (Error 401)

Scenario: Login with bad credentials
	When login badCredentials
	Then match response (Error 403)

Scenario: Login with no credentials
	When login noCredentials
	Then match response (Error 401)

Scenario: Logout
	When login goodCredentials
	And logout
	Then match response (Ok 200)

Scenario: Logout then request
	When login goodCredentials
	And logout
	And get list of projects
	Then match response (Error 401)
	And not null response 

Scenario: Request with bad session
	When login noCredentials
	And use badSession
	And get list of projects
	Then not null response 
