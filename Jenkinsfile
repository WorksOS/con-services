node ('master')
{
checkout scm
def branchc = env.BRANCH_NAME

load './MasterDataConsumers/Jenkinsfile'
if (!branchc.contains("release")) {
load './MockProjectWebApi/src/Jenkinsfile'
}
}