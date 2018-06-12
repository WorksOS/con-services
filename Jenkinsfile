node ('jenkinsslave-pod') {
    def branch = env.BRANCH_NAME
    def buildNumber = env.BUILD_NUMBER
    def versionPrefix = ""
    def suffix = ""
    def branchName = ""
	def prjname = env.JOB_NAME.toLowerCase() 

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
	
	def vars = []
	def file
	def yaml
	
    stage('Build Solution') {
        checkout scm
		// TODO use dockerfile with ENTRYPOINT instead of CMD as it is insecure
	    def building = docker.build(container, "-f Dockerfile.build .")

        // Currently we need to execute the tests like this, because the pipeline docker plugin being aware of DIND, and attempting to map
        // the volume to the bare metal host        
        sh "docker run -v ${env.WORKSPACE}/TestResults:/build/TestResults ${building.id} /bin/sh /build/unittests.sh"
        sh "ls ${env.WORKSPACE}/TestResults"
		
		 //See https://jenkins.io/doc/pipeline/steps/xunit/#xunit-publish-xunit-test-result-report for DSL Guide
        step([$class: 'XUnitBuilder',
                thresholds: [[$class: 'FailedThreshold', unstableThreshold: '10']],
                tools: [[$class: 'XUnitDotNetTestType', pattern: 'TestResults/*']]])

		cobertura autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/*.xml', conditionalCoverageTargets: '70, 0, 0', failUnhealthy: false, failUnstable: false, lineCoverageTargets: '80, 0, 0', maxNumberOfBuilds: 0, methodCoverageTargets: '80, 0, 0', onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false
				
		//http://javadoc.jenkins-ci.org/tfs/index.html?hudson/plugins/tfs/model/TeamResultType.html
        //Details of the agent -> https://docs.microsoft.com/en-us/vsts/build-release/task
        //Agent Variables -> https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/build/variables?view=vsts&tabs=batch
        step([$class: 'TeamCollectResultsPostBuildAction', 
            requestedResults: [
                [includes: 'TestResults/*.xml', teamResultType: 'XUNIT'],
				[includes: 'TestCoverage/*.xml', teamResultType: 'COBERTURA']
            ]
        ])
	}
	
	stage('Prepairing runtime image') {
		docker.build(container, "-f Dockerfile .").push()
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

					//step([$class: 'JUnitResultArchiver', testResults: 'AcceptanceTests/tests/**/TestResults/*.xml'])
			        step([$class: 'TeamCollectResultsPostBuildAction', 
						requestedResults: [[includes: 'AcceptanceTests/tests/**/TestResults/*.xml', teamResultType: 'XUNIT']]])
				}
			}		
		}
	}
}
