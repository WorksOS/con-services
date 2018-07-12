node ('jenkinsslave-pod') {

    // adds job parameters
    properties([
        parameters([
            string(
                defaultValue: null,
                description: 'The build number supplied by VSTS perhaps fail build if this is nothing to prevent unrequested builds during multibranch scan',
                name: 'VSTS_BUILD_NUMBER'
            ),
        ])
    ])

    // We may need to rename the branch to conform to DNS name spec
    def branchName = env.BRANCH_NAME.substring(env.BRANCH_NAME.lastIndexOf("/") + 1)
    def jobnameparts = JOB_NAME.tokenize('/') as String[]
    def project_name = jobnameparts[0].toLowerCase()     
    def versionNumber = branchName + "-" + params.VSTS_BUILD_NUMBER
    def container = "registry.k8s.vspengg.com:80/${project_name}:${versionNumber}"
    def testContainer = "registry.k8s.vspengg.com:80/${project_name}.tests:${versionNumber}"
    def finalImage = "276986344560.dkr.ecr.us-west-2.amazonaws.com/${project_name}:${versionNumber}"
    
    def vars = []
    def acceptance_testing_yaml
    def runtimeImage 
    def build_container
    //Set the build name so it is consistant with VSTS
    currentBuild.displayName = versionNumber

    stage("Prebuild Checks") {
        if (params.VSTS_BUILD_NUMBER == null) {
            currentBuild.result = 'ABORTED'
            error("Build stopping, no valid build number supplied")
        }
    }
    
    stage('Build Solution') {
        checkout scm
        
        build_container = docker.build(container, "-f Dockerfile.build .")

        // Currently we need to execute the tests like this, because the pipeline docker plugin being aware of DIND, and attempting to map
        // the volume to the bare metal host        

    }
    
    stage('Prepairing runtime image') {
        runtimeImage = docker.build(container, "-f Dockerfile --build-arg BUILD_CONTAINER=${container} .")
        runtimeImage.push()
    }

    stage ('Publish results') {
        sh "docker tag ${container} ${finalImage}"
        sh "eval \$(aws ecr get-login --region us-west-2 --no-include-email)"
        sh "docker push ${finalImage}"
        sh "ls -la chart/"
        archiveArtifacts artifacts: 'chart/**/*.*', fingerprint: true
    }
}
