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
} else if (branch.contains("master")) {
    versionPrefix = "1.0."
    branchName = "master"
} else {
    branchName = branch.substring(branch.lastIndexOf("/") + 1)
    suffix = "-" + branchName
    versionPrefix = "0.98."
}

def versionNumber = versionPrefix + buildNumber
def fullVersion = versionNumber + suffix

node('Ubuntu_Slave') {
    def workspacePath =""
    currentBuild.displayName = versionNumber + suffix

	try
	{
		stage ('Checkout') {
			checkout scm
		}
		stage ('Restore packages') {
			sh "dotnet restore --no-cache VSS.Productivity3D.Scheduler.sln"
		}
		stage ('Build solution') {
			sh "bash ./build.sh"
		}
		stage ('Run unit tests') {
			sh "bash ./unittests.sh" 
		}
		stage ('Prepare Acceptance tests') {
			sh "(cp ./AcceptanceTests/DockerfileJenkins ./AcceptanceTests/Dockerfile)"
			sh "(cp ./AcceptanceTests/scripts/runtestsjenkins.sh ./AcceptanceTests/scripts/runtests.sh)"
			sh "(cd ./AcceptanceTests/scripts && bash ./deploy_linux.sh)"
		}
		stage ('Compose containers') {
			sh "bash ./awslogin.sh"
			sh "bash ./start_containers.sh"
		}
		stage ('Wait for containers to finish') {
			sh "bash ./wait_container.sh testcontainers"
		}
		stage ('Bring containers down and archive the logs') {
			sh "(mkdir -p ./logs && docker-compose logs > ./logs/logs.txt)" 
			sh "docker-compose down"
		}
	}
	catch (error)
	{
		echo "An error occurred during execution of packaging - ${error.getMessage()}."
		sendBuildFailureMessage()
		// re-throw error to maintain logic flow
		throw error
	}

    // Here we need to find test results and decide if the build successfull
    stage ('Publish test results and logs') {
        workspacePath = pwd()
        currentBuild.result = 'SUCCESS'
        step([$class: 'JUnitResultArchiver', testResults: '**/testresults/*.xml'])
        publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './logs', reportFiles: 'logs.txt', reportName: 'Build logs'])
    
        echo "Build result is ${currentBuild.result}"
        result = currentBuild.result
    }

    if (currentBuild.result=='SUCCESS') {
		if (!branch.contains("master")) {
			//Rebuild Image, tag & push to AWS Docker Repo
			stage ('Get ecr login, push image to Repo') {
				sh "bash ./awslogin.sh"
			}
		}

        if (branch.contains("release")) {
            stage ('Build Release Images') {
                sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-webapi:latest-release-${fullVersion} ./artifacts/VSS.Productivity3D.Scheduler.WebApi"
                sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-webapi:latest-release-${fullVersion}"
                sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-webapi:latest-release-${fullVersion}"
                sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-db:latest-release-${fullVersion} ./database"
                sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-db:latest-release-${fullVersion}"
                sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-db:latest-release-${fullVersion}"
            }

            stage ('Tag repository') {
                sh 'git rev-parse HEAD > GIT_COMMIT'
                def gitCommit=readFile('GIT_COMMIT').trim()
                def tagParameters = [
                    new StringParameterValue("REPO_NAME", "VSS.Productivity3D.Scheduler"),
                    new StringParameterValue("COMMIT_ISH", gitCommit),
                    new StringParameterValue("TAG", fullVersion)]

                build job: "tag-vso-commit", parameters: tagParameters
            }
        } 
		else {
            if (branch.contains("Dev")) {
                stage ('Build Development Images') {
                    sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-webapi:latest ./artifacts/VSS.Productivity3D.Scheduler.WebApi"
                    sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-webapi"
                    sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-webapi:latest"
                    sh "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-db:latest ./database"
                    sh "docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-db"
                    sh "docker rmi -f 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-productivity3d-scheduler-db:latest"
                }
            }
        }
    }
	else {
		sendBuildFailureMessage()
	}
}

node ('Jenkins-Win2016-Raptor') {
    if (branch.contains("master")) {
        if (result=='SUCCESS') {
            currentBuild.displayName = versionNumber + suffix

            stage ('Checkout') {
                checkout scm
            }

            stage ('Build') {
                bat  "PowerShell.exe -ExecutionPolicy Bypass -Command .\\build471.ps1 -uploadArtifact -versionNumber ${versionNumber}"
            }

            archiveArtifacts artifacts: 'VSS.Productivity3D.Scheduler.WebApiNet471.zip', fingerprint: true 

            stage ('Tag repository') {
                bat 'git rev-parse HEAD > GIT_COMMIT'
                def gitCommit=readFile('GIT_COMMIT').trim()
                def tagParameters = [
                    new StringParameterValue("REPO_NAME", "VSS.Productivity3D.Scheduler"),
                    new StringParameterValue("COMMIT_ISH", gitCommit),
                    new StringParameterValue("TAG", fullVersion+"-master")]

                build job: "tag-vso-commit", parameters: tagParameters
            }
        }
    } else {
        currentBuild.displayName = versionNumber + suffix

        stage ('Checkout') {
            checkout scm
            stage 'Coverage'
            bat "coverage.bat"
        }

        step([$class: 'CoberturaPublisher', autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/outputCobertura.xml', failUnhealthy: true, failUnstable: false, maxNumberOfBuilds: 0, onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false])
        publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './CoverageReport', reportFiles: '*', reportName: 'OpenCover Report'])
    }
}

/*	Send build failure message     */
def sendBuildFailureMessage() {
	echo "Sending failure email..."
	try
	{
		commitVals = getChangeLogs().split(',')
		committer = commitVals[0]
		committerEmail = commitVals[1]
		commitId = commitVals[2]
	}
	catch (error)
	{
		echo "Unable to determine committer for failure email: ${error.getMessage()}"
	}
	
	def body = "${env.JOB_NAME} - build failed"	
	body = "${body}\nBuild #: ${env.BUILD_ID}"
	body = "${body}\nCommitters: ${committer}"
	body = "${body}\nCommit SHA: ${commitId}"
	body = "${body}\nSee console log at ${env.BUILD_URL}console for details"

	retry(2)
	{
		mail body: "${body}",
		from: 'jenkins_noreply@vspengg.com',
		subject: "${env.BUILD_URL} Failed",
		cc: (env.BRANCH_NAME == 'master' ? 'VSSTeamMerino@trimble.com' : ''),
		to: committerEmail
	}
}	

/*  Get the changelogs for the commit which triggered this build.   */
@NonCPS
def getChangeLogs() {
    def changeLogSets = currentBuild.changeSets
    def commitVals
    for (int i = 0; i < changeLogSets.size(); i++)
    {
        def entries = changeLogSets[i].items
        for (int j = 0; j < entries.length; j++)
        {
            def entry = entries[j]
            def email = entry.author.getProperty(hudson.tasks.Mailer.UserProperty.class).getAddress()
            commitVals = "${entry.author},${email},${entry.commitId}"
        }
    }
    return commitVals
}