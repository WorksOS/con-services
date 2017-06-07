properties([disableConcurrentBuilds(), pipelineTriggers([])])

    def result = ''

    def branch = env.BRANCH_NAME
    def buildNumber = env.BUILD_NUMBER
    def versionPrefix = ""
    def suffix = ""
    def branchName = ""

    if (branch.contains("release")) {
       versionPrefix = "1.0."
       branchName = "Release"
       } else if (branch.contains("Dev")) {
       versionPrefix = "0.99."
       branchName = "Dev"
       } else {
       branchName = branch.substring(branch.lastIndexOf("/") + 1)
       suffix = "-" + branchName
       versionPrefix = "0.98."
       }
    
    def versionNumber = versionPrefix + buildNumber
    def fullVersion = versionNumber + suffix


node('Ubuntu_Slave') {
    //Apply version number
    //We will later use it to tag images

    def workspacePath =""
    currentBuild.displayName = versionNumber + suffix

    stage 'Checkout'
    checkout scm
    stage 'Restore packages'
    sh "dotnet restore --no-cache"
    stage 'Build solution'
    sh "bash ./build.sh"
/*    stage 'Run unit tests'
    sh "bash ./unittests.sh" */
    stage 'Prepare Acceptance tests'
    sh "(cd ./AcceptanceTests/scripts && bash ./deploy_linux.sh)"
    stage 'Compose containers'
    sh "bash ./start_containers.sh"
    stage 'Wait for containers to finish'
    sh "bash ./wait_container.sh testcontainers"
    stage 'Bring containers down and archive the logs'
    sh "(mkdir -p ./logs && docker-compose logs > ./logs/logs.txt)" 
    sh "docker-compose down"

//Here we need to find test results and decide if the build successfull
    stage 'Publish test results and logs'
    workspacePath = pwd()
    currentBuild.result = 'SUCCESS'
    step([$class: 'JUnitResultArchiver', testResults: '**/testresults/*.xml'])
    publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './logs', reportFiles: 'logs.txt', reportName: 'Build logs'])
    
    echo "Build result is ${currentBuild.result}"
    result = currentBuild.result

    if (currentBuild.result=='SUCCESS') {
       //Rebuild Image, tag & push to AWS Docker Repo
       stage 'Get ecr login, push image to Repo'
       sh '''eval '$(aws ecr get-login --region us-west-2 --profile vss-grant)' '''

	if (branch.contains("release"))
	{
	       stage 'Build Release Images'

	       sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:latest-release-${fullVersion} ./artifacts/ProjectWebApi"
 
	       sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:latest-release-${fullVersion}"

	       sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:latest-release-${fullVersion}"
		   
		   sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-db:latest-release-${fullVersion} ./database"
 
	       sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-db:latest-release-${fullVersion}"

	       sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-db:latest-release-${fullVersion}"

	}
	else
	{
	stage 'Build Development Images'

	   
       sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:${fullVersion}-${branch} ./artifacts/ProjectWebApi"
 
       sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:latest ./artifacts/ProjectWebApi"
 
       sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:${fullVersion}-${branch}"

       sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi"

       sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:${fullVersion}-${branch}"
       sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:latest"
	   
	   sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-db:${fullVersion}-${branch} ./database"
 
       sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-db:latest ./database"
 
       sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-db:${fullVersion}-${branch}"

       sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-db"

       sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-db:${fullVersion}-${branch}"
       sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-db:latest"
	}

    }
}

node ('Jenkins-Win2016-Raptor')
{
	if (branch.contains("master"))
	{
         if (result=='SUCCESS')
          {
           currentBuild.displayName = versionNumber + suffix
  
           stage 'Checkout'
           checkout scm

           stage 'Build'
           bat "build47.bat"

         }

        }
}