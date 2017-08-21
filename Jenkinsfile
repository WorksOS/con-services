options {
    disableConcurrentBuilds()
}

node('Jenkins-Win2016-Raptor') {
    //Apply version number
    //We will later use it to tag images

    def branch = env.BRANCH_NAME
    def buildNumber = env.BUILD_NUMBER
    def versionPrefix = ""
    def suffix = ""
    def branchName = ""

    if (branch.contains("release")) {
       versionPrefix = "1.0."
       branchName = "release"
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
        bat "dotnet restore --no-cache VSS.Productivity3D.Service.sln"
    }
    stage ('Build solution') {
        bat "./build.bat"
    }
    stage ('Run unit tests') {
        bat "./unittests.bat"
    }
    stage ('Prepare Acceptance tests') {
        bat "./acceptancetests.bat"
        try {
            stage ('Compose containers') {
                bat "./start_containers.bat"
            }
            stage ('Run Acceptance Tests') {
                bat "./runacceptancetests.bat"
            }
        } finally {
            stage ('Bring containers down and archive the logs') {
                bat "./stop_containers.bat"
            }
            stage ('Publish test results and logs') {
                //Convert trx to xml for archiving
                currentBuild.result = 'SUCCESS'
                bat ".\\msxsl.exe .\\AcceptanceTests\\tests\\ProductionDataSvc.AcceptanceTests\\bin\\Debug\\testresults.trx .\\mstest-to-junit.xsl -o .\\TestResult.xml"
                step([$class: 'JUnitResultArchiver', testResults: '**/TestResult.xml'])
                publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './AcceptanceTests/tests/ProductionDataSvc.AcceptanceTests/bin/Debug', reportFiles: 'logs.txt', reportName: 'WebApi logs'])
            }
        }
    }
	
    echo "Build result is ${currentBuild.result}"
    if (currentBuild.result=='SUCCESS') {
        stage ('Coverage') {
            bat "./coverage.bat"
	        step([$class: 'CoberturaPublisher', autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/outputCobertura.xml', failUnhealthy: true, failUnstable: false, maxNumberOfBuilds: 0, onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false])
	        publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './CoverageReport', reportFiles: '*', reportName: 'OpenCover Report'])

		    if (branch.contains("release")) {
                stage ('Build Release Images') {
                    bat "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:latest-release-${fullVersion} ./Artifacts/WebApi"
                } 
                //Publish to AWS Repo
                stage ('Get ecr login, push image to Repo') {
                    bat "PowerShell.exe -ExecutionPolicy Bypass -Command .\\PushImages.ps1 -fullVersion latest-release-${fullVersion}"
		        }
            } else {
                //Rebuild Image, tag & push to AWS Docker Repo
                stage ('Build Development Images') {
                    bat "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:${fullVersion}-${branchName} ./Artifacts/WebApi"
                    bat "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:latest ./Artifacts/WebApi"
                }
            }

            //Publish to AWS Repo
            stage ('Get ecr login, push image to Repo') {
                bat "PowerShell.exe -ExecutionPolicy Bypass -Command .\\PushImages.ps1 -fullVersion ${fullVersion}-${branchName}"
            }
	   }
    }
}