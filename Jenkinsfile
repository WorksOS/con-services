node ('jenkinsslave-pod') {
    def branch = env.BRANCH_NAME
    def buildNumber = env.BUILD_NUMBER
    def versionPrefix = ""
    def suffix = ""
    def branchName = ""
	def build_container = ""

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
	
	//The runtimeContainerTag will need to updated when we want to push this to ecr
	def buildContainerTag = "vss.trex_build:${fullVersion}"
	def runtimeContainerTag = vss.trex_build:${fullVersion}"
	
    try {
		//Tests are done here because host volume mounts cannot be specified in the dockerfile
		stage('Build Container') {
			checkout scm
			build_container = docker.build(buildContainerTag, "-f DockerfileBuild .")
		}
		
        stage('Test Solution') {
			//Create results directory in workspace
			dir("/TestResults") {}
			
			// Currently we need to execute the tests like this, because the pipeline docker plugin being aware of DIND, and attempting to map
			// the volume to the bare metal host
			
			//Do not modify this unless you know the difference between ' and " in bash
			// (https://www.gnu.org/software/bash/manual/html_node/Quoting.html#Quoting) see (https://gist.github.com/fuhbar/d00d11297a48b892684da34360e4135a) for Jenkinsfile 
			// specific escaping examples. One day we might be able to test solutions (and have the results go to a specific directory) rather than specific projects, negating the need for such a complex command.
			def testCommand = $/docker run -v ${env.WORKSPACE}/TestResults:/TestResults ${build_container.id} bash -c 'cd /build && ls tests/*/*/*netcore*.csproj | xargs -I@ -t dotnet test  --test-adapter-path:. --logger:"xunit;LogFilePath=/TestResults/@.xml" @'/$
			
			//Run the test command generated above
			sh(script: testCommand)
			
			//List the test results - We should have some
			sh "ls ${env.WORKSPACE}/TestResults"
        }
		
		stage('Build Runtime Container') {
			def runtime_container = docker.build(runtimeContainerTag, "-f DockerfileRuntime --build-arg BUILD_CONTAINER=${buildContainerTag} .")
			
			//This is where we would push at the moment just list available images to verify we have built containers
			sh "docker images"
		}
    }
    finally {
        //See https://jenkins.io/doc/pipeline/steps/xunit/#xunit-publish-xunit-test-result-report for DSL Guide
        stage('Publish Results'){
            step([$class: 'XUnitBuilder',
                thresholds: [[$class: 'FailedThreshold', unstableThreshold: '10']],
                tools: [[$class: 'XUnitDotNetTestType', pattern: 'TestResults/*/*/**/*']]])
        
        //http://javadoc.jenkins-ci.org/tfs/index.html?hudson/plugins/tfs/model/TeamResultType.html
        //Details of the agent -> https://docs.microsoft.com/en-us/vsts/build-release/task
        //Agent Variables -> https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/build/variables?view=vsts&tabs=batch
        step([$class: 'TeamCollectResultsPostBuildAction', 
            requestedResults: [
                [includes: 'TestResults/*/*/**/*.xml', teamResultType: 'XUNIT']
            ]
        ])
        }
    }
}