package org.ccss.build;

import com.atlassian.bamboo.specs.api.BambooSpec;
import com.atlassian.bamboo.specs.api.builders.Variable;

import com.atlassian.bamboo.specs.api.builders.deployment.Deployment;
import com.atlassian.bamboo.specs.api.builders.deployment.Environment;
import com.atlassian.bamboo.specs.api.builders.deployment.ReleaseNaming;
import com.atlassian.bamboo.specs.api.builders.notification.EmptyNotificationsList;
import com.atlassian.bamboo.specs.api.builders.permission.*;
import com.atlassian.bamboo.specs.api.builders.plan.Plan;
import com.atlassian.bamboo.specs.api.builders.plan.PlanIdentifier;
import com.atlassian.bamboo.specs.api.builders.plan.branches.BranchCleanup;
import com.atlassian.bamboo.specs.api.builders.plan.branches.PlanBranchManagement;
import com.atlassian.bamboo.specs.api.builders.project.Project;
import com.atlassian.bamboo.specs.builders.task.*;
import com.atlassian.bamboo.specs.builders.trigger.AfterSuccessfulBuildPlanTrigger;
import com.atlassian.bamboo.specs.util.BambooServer;
import com.atlassian.bamboo.specs.api.builders.plan.Job;
import com.atlassian.bamboo.specs.api.builders.plan.Stage;
import com.atlassian.bamboo.specs.api.builders.plan.artifact.Artifact;


/**
 * Plan configuration for Bamboo.
 *
 * @see <a href="https://confluence.atlassian.com/display/BAMBOO/Bamboo+Specs">Bamboo Specs</a>
 */
@BambooSpec
public class PlanSpec {

    /**
     * Run 'main' to publish your plan.
     */
    public static void main(String[] args) throws Exception {
        // by default credentials are read from the '.credentials' file
        BambooServer bambooServer = new BambooServer("http://localhost:8085");

        new PlanSpec().publishPlans(bambooServer);
    }

    static PlanPermissions createPlanPermission(PlanIdentifier planIdentifier) {
        Permissions permissions = new Permissions()
                .userPermissions("Stephen_Post@Trimble.com", PermissionType.ADMIN)
                .loggedInUserPermissions(PermissionType.BUILD)
                .anonymousUserPermissionView();

        return new PlanPermissions(planIdentifier)
                .permissions(permissions);
    }

    static DeploymentPermissions createDeploymentPermissions(Deployment deployment) {
        return new DeploymentPermissions(deployment.getName())
                .permissions(new Permissions()
                        .userPermissions("Stephen_Post@Trimble.com", PermissionType.BUILD)
                        .userPermissions("David_Glassenbury@Trimble.com", PermissionType.BUILD)
                        .loggedInUserPermissions(PermissionType.VIEW));

    }

    static EnvironmentPermissions createEnvironmentPermissions(Deployment deployment, String env) {
        return new EnvironmentPermissions(deployment.getName())
                .environmentName(env)
                .permissions(new Permissions()
                        .loggedInUserPermissions(PermissionType.VIEW)
                        .userPermissions("Stephen_Post@Trimble.com", PermissionType.BUILD)
                        .userPermissions("David_Glassenbury@Trimble.com", PermissionType.BUILD));

    }

    Project project() {
        return new Project()
                .name("CIVIL 3D")
                .key("CIV3D");
    }


    void publishPlans(BambooServer bambooServer) {
        // Names must match what bamboo/jenkins has
        // Asset Management Service
        createPlan(bambooServer,"Asset Management Service", "ASSET",
                "asset-management-webapi",
                "src/service/3dAssetMgmt",
                "assetmgmt3d-service",
                false,false,true);

        // File Access Service
        createPlan(bambooServer,"File Access Service", "FILEACCESS",
                "file-access-webapi",
                "src/service/FileAccess",
                "file-service-service",
                false, true,true);

        // 3DP TRex Service
        createPlan(bambooServer,"3DP TRex Service", "PRODTREX",
                "productivity3d-trex-webapi",
                "src/service/3DP",
                "productivity3d-service",
                false,false,true);

        // Filter Service
        createPlan(bambooServer,"Filter Service", "FILTER",
                "filter-webapi",
                "src/service/Filter",
                "filter-service",
                true,true,true);

        // Project Service
        createPlan(bambooServer,"Project Service", "PROJECT",
                "project-webapi",
                "src/service/Project",
                "project-service",
                true,true,true);

        // Push Service
        createPlan(bambooServer,"Push Service", "PUSH",
                "push-webapi",
                "src/service/Push",
                "push-service",
                false,false,true);

        // Scheduler Service
        createPlan(bambooServer,"Scheduler Service", "SCHEDULER",
                "scheduler-webapi",
                "src/service/Scheduler",
                "scheduler-service",
                true,true,true);

        // TagFileAuth Service
        createPlan(bambooServer,"TagFileAuth Service", "TFA",
                "tag-file-auth-webapi",
                "src/service/TagFileAuth",
                "tagfileauth-service",
                false,true,true);

        // Tile Service
        createPlan(bambooServer,"Tile Service", "TILE",
                "tile-webapi",
                "src/service/TileService",
                "tile-service",
                false,true,true);

        // TRex Service
        createPlan(bambooServer,"TRex Service", "TREX",
                "trex",
                "src/service/TRex",
                "trex-service",
                false,true,true);
   }

    void createPlan(BambooServer bambooServer, String name, String key, String service, String servicePath,
                    String serviceDiscoveryName, boolean buildDb, boolean runAcceptanceTest, boolean runUnitTests) {
        Plan plan = new Plan(project(), name, key)
                .linkedRepositories("con-services")
                .description("Plan created from Bamboo Java Specs")
                .notifications(new EmptyNotificationsList())
                .variables(
                        new Variable("jenkins_username", "jnimmo2"),
                        new Variable("jenkins_token", "1123a83c8b6925f1567370f72074adf77f"),
                        new Variable("jenkins_url", "http://ci.eks.ccss.cloud"),
                        new Variable("service", service),
                        new Variable("service_path", servicePath),
                        new Variable("service_discovery_name", serviceDiscoveryName),
                        new Variable("image_tag", "None"),
                        new Variable("build_db", buildDb ? "true" : "false"),
                        new Variable("run_acceptance_tests", runAcceptanceTest ? "true" : "false"),
                        new Variable("run_unit_tests", runUnitTests ? "true" : "false")
                        )
                .planBranchManagement(new PlanBranchManagement()
                        .createForVcsBranchMatching("feature/.*")
                        .notificationForCommitters()
//                        .branchIntegration(new BranchIntegration()
//                                .integrationBranchKey("MASTER")
//                                .gatekeeper(true)
//                                .pushOnSuccessfulBuild(true))
                        .triggerBuildsLikeParentPlan()
                        .delete(new BranchCleanup()
                                .whenInactiveInRepositoryAfterDays(30)));


        addPlanStages(plan, runAcceptanceTest);

        PlanPermissions planPermission = PlanSpec.createPlanPermission(plan.getIdentifier());
        bambooServer.publish(planPermission);

        // These deployments need to be created by Support, we don't have the ability to create new deployments currently
        Deployment deployment = createDeployment(bambooServer, key, "Deployment for Civil 3D - " + name);

        bambooServer.publish(deployment);
        bambooServer.publish(plan);
    }

    Deployment createDeployment(BambooServer bambooServer, String key, String name)
    {
        Deployment deployment = new Deployment(new PlanIdentifier("CIV3D", key), name)
                .releaseNaming(new ReleaseNaming("0.${bamboo.planRepository.branchName}.${bamboo.buildNumber}"));

        addDeploymentEnvironment(bambooServer, deployment, "dev");
        addDeploymentEnvironment(bambooServer, deployment, "alpha");
        addDeploymentEnvironment(bambooServer, deployment, "prod");

        DeploymentPermissions deployPermission = PlanSpec.createDeploymentPermissions(deployment);
        bambooServer.publish(deployPermission);

        bambooServer.publish(deployment);

        return deployment;
    }

    Deployment addDeploymentEnvironment(BambooServer bambooServer, Deployment deployment, String name) {

        String cleanupCommand = "STATUS=$(helm ls --namespace ${bamboo.deploy.environment} | grep ${bamboo.deploy.environment}-${bamboo.service} | awk '{ print $8 }')\n"
                + "if [ ! -z \"$STATUS\"  ] && [ \"$STATUS\" = 'failed' ]; then\n"
                + "helm delete --namespace ${bamboo.deploy.environment} ${bamboo.deploy.environment}-${bamboo.service}\n"
                + "else\n"
                + "echo Good Release\n"
                + "fi";


        String deployCommand =
                // Remove the prefix from a branch name
                // i.e feature/test -> test
                "IMAGETAG=$(echo ${bamboo.planRepository.branchName} | sed -e 's/.\\///')\n"+
                "helm upgrade " +
                "--install " +
                "${bamboo.deploy.environment}-${bamboo.service} " +
                "--namespace ${bamboo.deploy.environment} " +
                "--reset-values " +
                "--values ./chart/values.yaml " +
                "--set image.tag=$IMAGETAG-${bamboo.buildNumber}," +
                "environment=${bamboo.deploy.environment}," +
                "globalConfig=3dapp-${bamboo.deploy.environment}," +
                "rootDomain=eks.ccss.cloud," +
                "serviceName=${bamboo.service_discovery_name} " +
                "--wait " +
                "--force " +
                "./chart/";

        Environment env = new Environment(name);
        env.tasks(new CleanWorkingDirectoryTask());
        env.tasks(new ArtifactDownloaderTask()
                .artifacts(new DownloadItem()
                        .path("chart")
                        .allArtifacts(true)));

        env.tasks(new ScriptTask()
                .inlineBody("echo ${bamboo.planRepository.branchName}-${bamboo.buildNumber}")
                .interpreterBinSh());

        env.tasks(new ScriptTask()
                .inlineBody(cleanupCommand)
                .interpreterBinSh());

        env.tasks(new ScriptTask()
                .inlineBody("echo \"" + deployCommand + "\"")
                .interpreterBinSh());

        env.tasks(new ScriptTask()
                .inlineBody(deployCommand)
                .interpreterBinSh());

        if(name == "alpha") {
            env.triggers(new AfterSuccessfulBuildPlanTrigger().triggerByMasterBranch());
        }

        EnvironmentPermissions envPermission = PlanSpec.createEnvironmentPermissions(deployment, name);
        bambooServer.publish(envPermission);

        deployment.environments(env);

        return deployment;
    }

    /// Add the stages required for the build, disabling test coverage if not tests
    Plan addPlanStages(Plan plan,  boolean runAcceptanceTest) {
        Job buildJob = new Job("Build", "JOB1")
                .tasks(cleanTask(), checkoutCodeTask(), chmodScript(), runJenkinsScript())
                .artifacts(artifact());

        if(runAcceptanceTest)
            buildJob.finalTasks(testCoverageTask());

        Stage jenkinsStage = new Stage("Build Jenkins")
                .jobs(buildJob);

        plan.stages(jenkinsStage);

        return plan;
    }

    VcsCheckoutTask checkoutCodeTask() {
        return new VcsCheckoutTask().addCheckoutOfDefaultRepository();
    }

    CleanWorkingDirectoryTask cleanTask() {
        return new CleanWorkingDirectoryTask();
    }

    ScriptTask chmodScript() {
        return new ScriptTask()
                .inlineBody("chmod +x ./bamboo-specs/build-via-jenkins.py")
                .interpreterBinSh();
    }

    ScriptTask runJenkinsScript() {
        return new ScriptTask()
                .inlineBody("./bamboo-specs/build-via-jenkins.py --url ${bamboo.jenkins_url} --username ${bamboo.jenkins_username} --token ${bamboo.jenkins_token} --branch ${bamboo.planRepository.branchName} --buildid ${bamboo.buildNumber} --servicepath ${bamboo.service_path} --build ${bamboo.service} --imagetag ${bamboo.image_tag} --builddb ${bamboo.build_db} --runacctests ${bamboo.run_acceptance_tests} --rununittests ${bamboo.run_unit_tests}")
                .interpreterBinSh();
    }

    Artifact artifact() {
        return new Artifact("Deployment Chart")
                .location("artifacts/archive/deploy")
                .copyPattern("**/*")
                .shared(true);
    }

    TestParserTask testCoverageTask() {
        return TestParserTask.createNUnitParserTask()
                .resultDirectories("artifacts/archive/TestResults/UnitTests/**/*.xml",
                        "artifacts/archive/TestResults/*/*/**/*.xml",
                        "artifacts/archive/AcceptanceTests/tests/**/TestResults/*.xml");

    }
}
