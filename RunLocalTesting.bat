call build.bat
cd AcceptanceTests\scripts
call deploy_win.bat

cd ..
docker-compose -f docker-compose-local.yml rm -f

aws ecr get-login --region us-west-2 --profile vss-grant > temp.cmd
call temp.cmd
del temp.cmd

docker-compose -f docker-compose-local.yml up --build > c:\temp\output.txt