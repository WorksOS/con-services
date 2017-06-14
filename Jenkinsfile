node ('master')
{
checkout scm
def branchc = env.BRANCH_NAME

load './MasterDataConsumers/Jenkinsfile'
if (!(branchc.contains("release")||branchc.contains("master"))) 
{
load './MockProjectWebApi/src/Jenkinsfile'
}
}