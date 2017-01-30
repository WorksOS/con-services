properties([disableConcurrentBuilds(), pipelineTriggers([])])

node ('master')
{
    def branch = env.BRANCH_NAME
    def buildNumber = env.BUILD_NUMBER
    def versionPrefix = ""
    def suffix = ""
    def branchName = ""
    def openCoverLocation = ""
    def reportGeneratorLocation = ""
    def workspacePath =""
    def openCoverfilters ="-filter:\"+[*]* -[*]*.*Tests -[*]VSS.VisionLink.Interfaces.* -[*]MockClasses.*\"" 


    if (branch.contains("master")) {
       versionPrefix = "1.0."
       branchName = "Release"
       } else if (branch.contains("develop")) {
       versionPrefix = "0.99."
       branchName = "Dev"
       } else {
       branchName = branch.substring(branch.lastIndexOf("/") + 1)
       suffix = "-" + branchName
       versionPrefix = "0.98."
       }
    
    def versionNumber = versionPrefix + buildNumber
    def fullVersion = versionNumber + suffix

    //openCoverLocation = tool name: 'default OpenCover', type: 'com.cloudbees.jenkins.plugins.customtools.CustomTool'
    openCoverLocation = pwd()
    workspacePath = pwd()
    openCoverLocation = openCoverLocation + "\\tools\\OpenCover.Console.exe"
   //reportGeneratorLocation = tool name: 'default ReportGenerator', type: 'com.cloudbees.jenkins.plugins.customtools.CustomTool'
    reportGeneratorLocation = pwd()
    reportGeneratorLocation = reportGeneratorLocation + "\\tools\\ReportGenerator.exe"

    stage 'Checkout'
    checkout scm
    stage 'Restore packages'
    bat "dotnet restore"
    stage 'Build solution'
    bat "./build.bat"
    stage 'Run tests and get coverage results'
    bat "del /q ${workspacePath}\\*.xml"

    bat "cd ${workspacePath}\\test\\UnitTests\\MasterDataConsumerTests\\bin\\Debug\\net451\\win7-x64 & \"${openCoverLocation}\" \"-register:user\" -target:\"dotnet-test-mstest.exe\" -targetargs:\"MasterDataConsumerTests.dll\" -output:\"${workspacePath}\\coverageDF.xml\" ${openCoverfilters}"
    bat "cd \"${workspacePath}\" & \"${reportGeneratorLocation}\" \"-reports:${workspacePath}\\*.xml\" \"-reporttypes:Html\" \"-targetdir:${workspacePath}\\Coverage\" "

    def reportDir = "${workspacePath}\\Coverage"
    def reportName = "OpenCover Results"
    publishHTML(target:[allowMissing: true, alwaysLinkToLastBuild: true, keepAll: true, reportDir: reportDir, reportFiles: 'index.htm', reportName: reportName])
}

node('Ubuntu_Slave') {
    //Apply version number
    //We will later use it to tag images

    def branch = env.BRANCH_NAME
    def buildNumber = env.BUILD_NUMBER
    def versionPrefix = ""
    def suffix = ""
    def branchName = ""

    if (branch.contains("QA")) {
       versionPrefix = "1.0."
       branchName = "QA"
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
    def workspacePath =""
    currentBuild.displayName = versionNumber + suffix

    stage 'Checkout'
    checkout scm
    stage 'Restore packages'
    sh "dotnet restore"
    stage 'Build solution'
    sh "bash ./build.sh"
    stage 'Run unit tests'
    sh "bash ./unittests.sh"
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
    step([$class: 'JUnitResultArchiver', testResults: '**/testresults/*.xml'])
    publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './logs', reportFiles: 'logs.txt', reportName: 'Build logs'])
 
//    mail body: 'Project MDM ${branch} ${fullVersion} build successful',
//         from: 'david_glassenbury@trimble.com',
//         replyTo: 'david_glassenbury@trimble.com',
//         subject: 'Project MDM ${branch} ${fullVersion} build successful',
//         to: 'david_glassenbury@trimble.com'
 
    if (branch == "Dev") {
       //Rebuild Image, tag & push to AWS Docker Repo
       stage 'Build Images'
	   
       sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:${fullVersion} ./artifacts/WebApi"
       sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-vup-masterdataconsumer:${fullVersion} ./artifacts/MasterDataConsumer"
 
       sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:latest ./artifacts/WebApi"
       sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-vup-masterdataconsumer:latest ./artifacts/MasterDataConsumer"
 
       //Publish to AWS Repo
       stage 'Get ecr login, push image to Repo'
       sh '''eval '$(aws ecr get-login --region us-west-2 --profile vss-grant)' '''
       sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:${fullVersion}"
       sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-vup-masterdataconsumer:${fullVersion}"

       sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi"
       sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-vup-masterdataconsumer"

       sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-vup-masterdataconsumer:${fullVersion}"
       sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:${fullVersion}"
       sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-vup-masterdataconsumer:latest"
       sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-project-webapi:latest"
    }
}
