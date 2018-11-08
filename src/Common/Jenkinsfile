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
 if (prjname.contains("Nuget"))
 {
  node('Jenkins-Win2016-Raptor') {
    stage 'Checkout'
    checkout scm
    stage 'Coverage'
    bat "./coverage.bat"
    step([$class: 'CoberturaPublisher', autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/outputCobertura.xml', failUnhealthy: true, failUnstable: false, maxNumberOfBuilds: 0, onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false])
    publishHTML(target:[allowMissing: false, alwaysLinkToLastBuild: true, keepAll: true, reportDir: './CoverageReport', reportFiles: '*', reportName: 'OpenCover Report'])

  }
 }
 else
 {
   if (!(branchc.contains("release")||branchc.contains("master"))) 
    {
     load './MockProjectWebApi/src/Jenkinsfile_win'
    }
  }
}