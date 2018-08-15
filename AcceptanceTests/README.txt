To run acceptance tests on local machine.
 - run build.bat in repository root directory
 - run AcceptanceTests\scripts\runLocal.ps1
 
 
 ////////////////////////////// FOR RUNNING TESTS FROM VISUAL STUDIO //////////////////////
 
After runLocal has finished it displays an IP Address e.g

Creating vssproductivity3dservice_mockprojectwebapi_1
Creating vssproductivity3dservice_mockprojectwebapi_1 ... done
Docker started successfully (Running in Detached mode)

  Container Name: vssproductivity3dservice_webapi
  Container ID: ceb5f335e5f9
  IP Address: 172.17.187.96


Update set-environment-variables.ps1 to reflect this new ip address.
 
Open visual studio project and run tests (note VS should not be running when setting environment variables.
 
 
 ////////////////////////////// RUNNING ALL TESTS FROM BAT FILE //////////////////////
 after runLocal has finished, update/create container.txt in root of project repository to contain the newly created container id.
 
 - run runacceptancetests.bat
 
 
*****************************************************************************
*	Environment variables for Jenkins are set in runacceptancetests.bat		*
*****************************************************************************