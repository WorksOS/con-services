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
	    docker.build("registry.k8s.vspengg.com:80/vss.projectservice:${versionNumber}", "-f Dockerfile .") .push()
	    docker.build("registry.k8s.vspengg.com:80/vss.projectservice.tests:${versionNumber}", "-f Dockerfile.tests .").push()
	    def containerName = "registry.k8s.vspengg.com:80/vss.projectservice:${versionNumber}"
	    def testContainerName = "registry.k8s.vspengg.com:80/vss.projectservice.tests:${versionNumber}"

	    def nestedLabel = "projectservice-${UUID.randomUUID().toString()}"
	    def label = "projectservice-${UUID.randomUUID().toString()}"
            def template = readFile "yaml/testing-pod.yaml"
 	    
		podTemplate(label: nestedLabel, yaml: template, namespace: "testing") {
                podTemplate(label: label, containers: [containerTemplate(name: containerName, image: containerName, ttyEnabled: true), 
						       containerTemplate(name: testContainerName, image: testContainerName, ttyEnabled: true)]) {
		  node (label) {
			container (testContainerName)
			{
			 sh '/app/runtests.sh'
			}
		  }
		 }
		}
		
    }
}