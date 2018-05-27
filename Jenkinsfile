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
	    docker.build("registry.k8s.vspengg.com:80/vss.projectservice:${fullVersion}", "-f Dockerfile .") .push()
	    docker.build("registry.k8s.vspengg.com:80/vss.projectservice.tests:${fullVersion}", "-f Dockerfile.tests .").push()
	    def container = "registry.k8s.vspengg.com:80/vss.projectservice:${fullVersion}"
	    def testContainer = "registry.k8s.vspengg.com:80/vss.projectservice.tests:${fullVersion}"

def label = "mypod-${UUID.randomUUID().toString()}"
podTemplate(label: label, yaml: """
apiVersion: v1
kind: Pod
metadata:
  name: projectservice-testing
spec:

  containers:

  - name: mysql-container
    image: mysql/mysql-server:5.7.15 
    envFrom:
    - configMapRef:
        name: projectservice-testing

  - name: zookeeper-container
    image: wurstmeister/zookeeper:3.4.6

  - name: kafka-container
    image: wurstmeister/kafka:0.11.0.1
    envFrom:
    - configMapRef:
        name: projectservice-testing

  - name: mockapi-container
    image: 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-mockproject-webapi:latest-linux
    envFrom:
    - configMapRef:
        name: projectservice-testing

  - name: service-container
    image: ${container}
    envFrom:
    - configMapRef:
        name: projectservice-testing

  - name: test-container
    image: ${testContainer}
    envFrom:
    - configMapRef:
        name: projectservice-testing

"""
) {
	node (label) {
		container(test-container) {
			sh "/runtests.sh"
		}
	}
  }
		
		
		
		}
}