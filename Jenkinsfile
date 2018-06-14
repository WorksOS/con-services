node ('jenkinsslave-pod') {

	// adds job parameters
	properties([
		parameters([
			string(
				defaultValue: "nothing",
				description: 'isFoo should be false',
				name: 'VSTS_BUILD_NUMBER'
			),
		])
	])

    def branch = env.BRANCH_NAME
    def buildNumber = params.VSTS_BUILD_NUMBER
    def versionPrefix = ""
    def suffix = ""
    def branchName = ""
	def jobnameparts = JOB_NAME.tokenize('/') as String[]
	def prjname = jobnameparts[0].toLowerCase() 	

    if (branch.contains("release")) {
        versionPrefix = "1.0."
        branchName = ""
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
	
    def container = "registry.k8s.vspengg.com:80/${prjname}:${fullVersion}"
    def testContainer = "registry.k8s.vspengg.com:80/${prjname}.tests:${fullVersion}"
    def finalImage = "276986344560.dkr.ecr.us-west-2.amazonaws.com/${prjname}:${versionNumber}"
	
    def vars = []
    def file
    def yaml
	def runtimeImage
	
    stage('Build Solution') {
        checkout scm
		// TODO use dockerfile with ENTRYPOINT instead of CMD as it is insecure
	    def build_container = docker.build(container, "-f Dockerfile.build .")

        // Currently we need to execute the tests like this, because the pipeline docker plugin being aware of DIND, and attempting to map
        // the volume to the bare metal host        
        //sh "docker run -v ${env.WORKSPACE}/TestResults:/TestResults ${building.id} /bin/sh /build/unittests.sh"

		//Create results directory in workspace
		dir("TestResults") {}
		
		// Currently we need to execute the tests like this, because the pipeline docker plugin being aware of DIND, and attempting to map
		// the volume to the bare metal host
		
		//Do not modify this unless you know the difference between ' and " in bash
		// (https://www.gnu.org/software/bash/manual/html_node/Quoting.html#Quoting) see (https://gist.github.com/fuhbar/d00d11297a48b892684da34360e4135a) for Jenkinsfile 
		// specific escaping examples. One day we might be able to test solutions (and have the results go to a specific directory) rather than specific projects, negating the need for such a complex command.
		def testCommand = $/docker run -v ${env.WORKSPACE}/TestResults:/TestResults ${build_container.id} bash -c 'cd /build/test && ls UnitTests/**/*Tests.csproj | xargs -I@ -t dotnet test @ /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutputDirectory=/TestResults/TestCoverage/ --test-adapter-path:. --logger:"xunit;LogFilePath=/TestResults/@.xml"'/$
		
		//Run the test command generated above
		sh(script: testCommand)

        sh "ls ${env.WORKSPACE}/TestResults"
		
		 //See https://jenkins.io/doc/pipeline/steps/xunit/#xunit-publish-xunit-test-result-report for DSL Guide
        step([$class: 'XUnitBuilder',
                thresholds: [[$class: 'FailedThreshold', unstableThreshold: '10']],
                tools: [[$class: 'XUnitDotNetTestType', pattern: 'TestResults/UnitTests/**/*.xml']]])

		cobertura autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: 'TestResults/TestCoverage/*.xml', conditionalCoverageTargets: '70, 0, 0', failUnhealthy: false, failUnstable: false, lineCoverageTargets: '80, 0, 0', maxNumberOfBuilds: 0, methodCoverageTargets: '80, 0, 0', onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false

		//Stash results for publication to vsts after acceptance/integration tests have run
		dir("TestResults") {
			stash name: "build-test-results"
		} 
		

	}
	
	stage('Prepairing runtime image') {
		runtimeImage = docker.build(container, "-f Dockerfile .")
		runtimeImage.push()
	}
	
    stage('Build Acceptance tests') {	
	    docker.build(testContainer, "-f Dockerfile.tests .").push()
	}

	
	
	stage ('Run acceptance tests') {
		dir ("yaml") {
			def testingEnvVars = readFile("testingvars.env").split("\n")
				testingEnvVars.each { String line ->
				def (key, value) = line.split('=')
				vars.add(envVar(key: key, value: value))
				}
			file = readFile("pod.yaml")
			yaml = file.replace('!container!', "${container}")
		}
			def label = "testingpod-${UUID.randomUUID().toString()}"
		podTemplate(label: label, namespace: "testing", yaml: yaml, containers: [containerTemplate(name: "jnlp", image: testContainer, ttyEnabled: true,  envVars: vars)])
		{
			node (label) {
				dir ("/app") {
					sh "/bin/sh runtests.sh"

					step([$class: 'XUnitBuilder',
							thresholds: [[$class: 'FailedThreshold', unstableThreshold: '100']],
							tools: [[$class: 'XUnitDotNetTestType', pattern: 'AcceptanceTests/tests/**/TestResults/*.xml']]])


					//Unstash test results from earlier for publishing to vsts as we can only do this once
					dir("TestResults") {
						unstash "build-test-results"
						//check that we got something
						sh "ls -la"
					}

					//http://javadoc.jenkins-ci.org/tfs/index.html?hudson/plugins/tfs/model/TeamResultType.html
					//Details of the agent -> https://docs.microsoft.com/en-us/vsts/build-release/task
					//Agent Variables -> https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/build/variables?view=vsts&tabs=batch
					step([$class: 'TeamCollectResultsPostBuildAction', 
						requestedResults: [
							[includes: 'TestResults/UnitTests/**/*.xml', teamResultType: 'XUNIT'],
							[includes: 'TestResults/TestCoverage/*.xml', teamResultType: 'COBERTURA'],
							[includes: 'AcceptanceTests/tests/**/TestResults/*.xml', teamResultType: 'XUNIT']
						]
					])



				}
			}		
		}
	}
	
	stage ('Publish results') {
		sh "docker tag ${container} ${finalImage}"
		sh "eval \$(aws ecr get-login --region us-west-2 --no-include-email)"
		sh "docker push ${finalImage}"
		sh "echo ${env.versionNumber} >> chart/build.sbt"
		sh "ls -la chart/"
        archiveArtifacts artifacts: 'chart/**/*.*', fingerprint: true 
	}
}
