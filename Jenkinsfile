node ('jenkinsslave-pod') {
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
    stage('Build Solution') {
            checkout scm
	    docker.build("vss.projectservice:${fullVersion}", "-f Dockerfile .") 
	    docker.build("vss.projectservice.tests:${fullVersion}", "-f Dockerfile.tests .")
	    def label = "mypod-${UUID.randomUUID().toString()}"
            def template = readFile "yaml/testing-pod.yaml"
	    podTemplate(name: "vss.projectservice:${fullVersion}", label: label, yaml: template, namespace: "testing")
		{
		  node (label) {
			container ("vss.projectservice:${fullVersion}")
		  }
		} 
    }
}