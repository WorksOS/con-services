node('Jenkins-Win2016-Raptor') {
    // This limits build concurrency to 1 per branch
    properties([disableConcurrentBuilds()])

    def branch = env.BRANCH_NAME
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

    // Apply version number and use it later to tag images
    def versionNumber = versionPrefix + env.BUILD_NUMBER
    def fullVersion = versionNumber + suffix
    currentBuild.displayName = versionNumber + suffix

    try
    {
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
            // Presence of the containers indicate another is building. Abort immediately.
            bat "PowerShell.exe -ExecutionPolicy Bypass -Command .\\check-container-state.ps1"

            try {
                stage ('Compose containers') {
                    bat  "PowerShell.exe -ExecutionPolicy Bypass -Command .\\start_containers.ps1 -branch ${branchName}"
                }
                stage ('Run Acceptance Tests') {
                    bat  "PowerShell.exe -ExecutionPolicy Bypass -Command .\\runacceptancetests.ps1"
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
    }
    catch (error)
    {
        echo "An error occurred during execution of packaging - ${error.getMessage()}."
        SendBuildFailureMessage()
        // re-throw error to maintain logic flow
        throw error
    }

    echo "Build result is ${currentBuild.result}"
    if (currentBuild.result=='SUCCESS') {
        stage ('Coverage') {
       //     bat "./coverage.bat"
       //     step([$class: 'CoberturaPublisher', autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/outputCobertura.xml', failUnhealthy: true, failUnstable: false, maxNumberOfBuilds: 0, onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false])
      //      publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './CoverageReport', reportFiles: '*', reportName: 'OpenCover Report'])
        }

        if (branch.contains("release")) {
            stage ('Build Release Images') {
                bat "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:latest-release-${fullVersion} ./Artifacts/WebApi"
                
                stage ('Tag repository') {
                    bat 'git rev-parse HEAD > GIT_COMMIT'
                    def gitCommit=readFile('GIT_COMMIT').trim()
                    def tagParameters = [
                        new StringParameterValue("REPO_NAME", "VSS.Productivity3D.Service"),
                        new StringParameterValue("COMMIT_ISH", gitCommit),
                        new StringParameterValue("TAG", fullVersion)]
                        
                    build job: "tag-vso-commit", parameters: tagParameters
                }
            }

            //Publish to AWS Repo
            stage ('Get ecr login, push image to Repo') {
                bat "PowerShell.exe -ExecutionPolicy Bypass -Command .\\PushImages.ps1 -fullVersion latest-release-${fullVersion}"
            }
        } else if (branch.contains("master")) {
            def buildArtifact = '3DPRaptorWebApi.zip'
            archiveArtifacts artifacts: buildArtifact, fingerprint: true

            // This stage is only necessarly so long as the Prod deployments are not containerised.
            stage ('Upload artifact to S3 deployment repository') {
                bat "powershell.exe -ExecutionPolicy Bypass -Command aws s3 cp ${buildArtifact} s3://vss-merino/Productivity3D/Releases/${buildArtifact} --acl public-read --profile vss-grant"
            }

            stage ('Tag repository') {
                bat 'git rev-parse HEAD > GIT_COMMIT'
                def gitCommit=readFile('GIT_COMMIT').trim()
                def tagParameters = [
                    new StringParameterValue("REPO_NAME", "VSS.Productivity3D.Service"),
                    new StringParameterValue("COMMIT_ISH", gitCommit),
                    new StringParameterValue("TAG", fullVersion+"-master")]

                build job: "tag-vso-commit", parameters: tagParameters
            }
        } else {
            //Rebuild Image, tag & push to AWS Docker Repo
            stage ('Build Development Images') {
                bat "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:${fullVersion}-${branchName} ./Artifacts/WebApi"
                bat "docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-raptor-webapi:latest ./Artifacts/WebApi"
            }
            //Publish to AWS Repo
            stage ('Get ecr login, push image to Repo') {
                bat "PowerShell.exe -ExecutionPolicy Bypass -Command .\\PushImages.ps1 -fullVersion ${fullVersion}-${branchName}"
            }
        }
    }
    else {
        SendBuildFailureMessage()
    }
}

/* Send build failure message */
def SendBuildFailureMessage() {
    echo "Sending failure email..."
    try
    {
        def changelogs = GetChangeLogs()

        // Build may have failed after being manually restarted with no new commits and so no 'committer' to email.
        if (changelogs == null) {
            return
        }

        commitVals = changelogs.split(',')
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

/* Get the changelogs for the commit which triggered this build. */
@NonCPS
def GetChangeLogs() {
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