node ('master')
{
checkout scm
def branchc = env.BRANCH_NAME
def prjname = env.JOB_NAME 

if (prjname.contains("MasterDataConsumers"))
{
 load './MasterDataConsumers/Jenkinsfile'
}
else
{
  if (!(branchc.contains("release")||branchc.contains("master"))) 
  {
   load './MockProjectWebApi/src/Jenkinsfile'
  }
}

}