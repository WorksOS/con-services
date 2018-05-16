def result = ''
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


node ('slave') {
    checkout scm
    docker.build("vss.trex:${fullVersion}")
    

}



