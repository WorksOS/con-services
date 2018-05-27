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
podTemplate(label: label, namespace: "testing", yaml: """
apiVersion: v1
kind: Pod
metadata:
  name: projectservice-testing
spec:

  containers:

  - name: mysql-container
    image: mysql/mysql-server:5.7.15
    ports:
    - containerPort: 3306
    livenessProbe:
      tcpSocket:
        port: 3306
    readinessProbe:
      tcpSocket:
        port: 3306
    envFrom:
    - configMapRef:
        name: projectservice-testing

  - name: zookeeper-container
    image: wurstmeister/zookeeper:3.4.6
    - containerPort: 2181
    livenessProbe:
      tcpSocket:
        port: 2181
    readinessProbe:
      tcpSocket:
        port: 2181

  - name: kafka-container
    image: wurstmeister/kafka:0.11.0.1
    - containerPort: 9092
    livenessProbe:
      tcpSocket:
        port: 9092
    readinessProbe:
      tcpSocket:
        port: 9092
    envFrom:
    - configMapRef:
        name: projectservice-testing

  - name: mockapi-container
    image: 276986344560.dkr.ecr.us-west-2.amazonaws.com/vss-mockproject-webapi:latest-linux
    - containerPort: 5001
    livenessProbe:
      tcpSocket:
        port: 5001
    readinessProbe:
      tcpSocket:
        port: 5001
    tty: true
    envFrom:
    - configMapRef:
        name: projectservice-testing

  - name: service-container
    image: ${container}
    tty: true
    envFrom:
    - configMapRef:
        name: projectservice-testing

""", containers: [containerTemplate(name: "jnlp", image: testContainer, ttyEnabled: true,  envVars: [
            envVar(key: 'MYSQL_ALLOW_EMPTY_PASSWORD', value: 'true'),
        ])]
) {
	node (label) {
dir ("/app") {
sh "/bin/sh runtests.sh"
}
  	}
  }
		
		
		
		}
}