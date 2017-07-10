To run acceptance tests on local machine.

 - run build.bat in repository root directory
 - run AcceptanceTests\scripts\runLocal.ps1
 
 
 ////////////////////////////// FOR RUNNING TESTS FROM VISUAL STUDIO //////////////////////
 
 after runLocal has finished run docker inspect on the newly built *webapi* container to find the ipaddress it has, update AcceptanceTests\scripts\setEnvironmentVariables.ps1 to reflect this new ip address.
 
 - run setEnvironmentVariables.ps1 (from an elevated prompt)
 
 open visual studio project and run tests
 
 
 ////////////////////////////// RUNNING ALL TESTS FROM BAT FILE //////////////////////
 after runLocal has finished, update/create container.txt in root of project repository to contain the newly created container id.
 
 - run runacceptancetests.bat
 