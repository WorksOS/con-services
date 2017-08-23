properties([disableConcurrentBuilds(), pipelineTriggers([])])

node('Ubuntu_Slave') {
    //Apply version number
    //We will later use it to tag images

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
    def workspacePath =""
    currentBuild.displayName = versionNumber + suffix

    stage ('Checkout') {
        checkout scm
    }
    stage ('Restore packages') {
        sh "dotnet restore --no-cache"
    }
    stage ('Build solution') {
        sh "bash ./build.sh"
    }
    stage ('Prepare Acceptance tests') {
        sh "(cd ./AcceptanceTests/scripts && bash ./deploy_linux.sh)"
    }
    stage ('Compose containers') {
        sh "bash ./start_containers.sh"
    }
    stage ('Wait for containers to finish') {
        sh "bash ./wait_container.sh testcontainers"
    }
    stage ('Bring containers down and archive the logs') {
        sh "(mkdir -p ./logs && docker-compose logs > ./logs/logs.txt)" 
        sh "docker-compose down"
    }

    //Here we need to find test results and decide if the build successfull
    stage ('Publish test results and logs') {
        workspacePath = pwd()
        currentBuild.result = 'SUCCESS'
        step([$class: 'JUnitResultArchiver', testResults: '**/testresults/*.xml'])
        publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './logs', reportFiles: 'logs.txt', reportName: 'Build logs'])
        
        echo "Build result is ${currentBuild.result}"
        if (currentBuild.result=='SUCCESS') {
            //Rebuild Image, tag & push to AWS Docker Repo
            stage ('Get ecr login, push image to Repo') {
                sh "bash ./awslogin.sh"
            //       sh '''eval '$(aws ecr get-login --region us-west-2 --profile vss-grant)' '''
            }

            if (branch.contains("release"))	{
                stage ('Build Release Images') {
                    sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-fileaccess:latest-release-${fullVersion} ./artifacts/WebApi"
                    sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-fileaccess:latest-release-${fullVersion}"
                    sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-fileaccess:latest-release-${fullVersion}"
                }		   
	    stage ('Tag repository') {
                sh 'git rev-parse HEAD > GIT_COMMIT'
                def gitCommit=readFile('GIT_COMMIT').trim()
                def tagParameters = [
                  new StringParameterValue("REPO_NAME", "VSS.Productivity3D.FileAccess.Service"),
                  new StringParameterValue("COMMIT_ISH", gitCommit),
                  new StringParameterValue("TAG", fullVersion)
                ]
                build job: "tag-vso-commit", parameters: tagParameters
	    }

            } else {
                stage ('Build Development Images') {	   
                    sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-fileaccess:latest ./artifacts/WebApi" 
                    sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-fileaccess"
                    sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-fileaccess:latest"
                }	   
            }
        }
    }

}

node ('Jenkins-Win2016-Raptor')
{
	if (branch.contains("master")) {
        if (result=='SUCCESS') {
            currentBuild.displayName = versionNumber + suffix  
            stage ('Checkout') {
                checkout scm
            }

            stage ('Build') {
                bat "build47.bat"
            }
          
            archiveArtifacts artifacts: 'FileAccessWebApiNet47.zip', fingerprint: true

	    stage ('Tag repository') {
                bat 'git rev-parse HEAD > GIT_COMMIT'
                def gitCommit=readFile('GIT_COMMIT').trim()
                def tagParameters = [
                  new StringParameterValue("REPO_NAME", "VSS.Productivity3D.FileAccess.Service"),
                  new StringParameterValue("COMMIT_ISH", gitCommit),
                  new StringParameterValue("TAG", fullVersion+"-master")
                ]
                build job: "tag-vso-commit", parameters: tagParameters
	    }
        }
    } else {
//        currentBuild.displayName = versionNumber + suffix
//        stage ('Checkout') {
//            checkout scm
//        }
//        stage ('Coverage') {
//            bat "coverage.bat"
//        }
//
//	    step([$class: 'CoberturaPublisher', autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/outputCobertura.xml', failUnhealthy: true, failUnstable: false, maxNumberOfBuilds: 0, onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false])
//	    publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './CoverageReport', reportFiles: '*', reportName: 'OpenCover Report'])
	}
}