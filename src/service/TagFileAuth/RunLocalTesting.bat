call build.bat
cd AcceptanceTests\scripts
call deploy_win.bat

cd ..
docker-compose -f docker-compose-local.yml up --build > c:\temp\output.log