Feature: Login
	I should be able to login, or not.

Background: 
	Given /api/v1/users/login

#@ignore
Scenario: Login with good credentials
	When login 'akorban' 'Bullshit1!'
	Then match response SessionId

#@ignore
Scenario: Login with bad credentials
	When login 'akorban' 'badpassword'
	Then match response (Error 401)


