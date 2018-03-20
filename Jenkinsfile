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

	stage ('Checkout') {
		checkout scm
	}
	stage ('Restore packages') {        
		sh "bash ./CollectCoverage.sh"
	}

     publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './coverage-html', reportFiles: '*', reportName: 'Coverage report'])
}


