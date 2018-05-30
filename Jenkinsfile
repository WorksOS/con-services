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
	    docker.build(container, "-f Dockerfile .") .push()
	    docker.build(testContainer, "-f Dockerfile.tests .").push()

	}
	
	stage ('Run acceptance tests') {
		dir ("yaml") {
			def testingEnvVars = readFile("testingvars.env")
				testingEnvVars.each { String line ->
				def (key, value) = line..split('=')
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
				}
			}		
		}
	}
}
