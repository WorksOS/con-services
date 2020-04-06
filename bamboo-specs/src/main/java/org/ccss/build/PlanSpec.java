package org.ccss.build;

import com.atlassian.bamboo.specs.api.BambooSpec;
import com.atlassian.bamboo.specs.api.builders.Variable;

import com.atlassian.bamboo.specs.api.builders.applink.ApplicationLink;
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
import com.atlassian.bamboo.specs.api.builders.repository.VcsChangeDetection;
import com.atlassian.bamboo.specs.api.builders.requirement.Requirement;
import com.atlassian.bamboo.specs.builders.repository.bitbucket.cloud.BitbucketCloudRepository;
import com.atlassian.bamboo.specs.builders.repository.bitbucket.server.BitbucketServerRepository;
import com.atlassian.bamboo.specs.builders.task.*;
import com.atlassian.bamboo.specs.builders.trigger.AfterSuccessfulBuildPlanTrigger;
import com.atlassian.bamboo.specs.builders.trigger.RepositoryPollingTrigger;
import com.atlassian.bamboo.specs.util.BambooServer;
import com.atlassian.bamboo.specs.api.builders.plan.Job;
import com.atlassian.bamboo.specs.api.builders.plan.Stage;
import com.atlassian.bamboo.specs.api.builders.plan.artifact.Artifact;

import java.time.LocalTime;
import java.util.concurrent.TimeUnit;


/**
 * Plan configuration for Bamboo.
 *
 * @see <a href="https://confluence.atlassian.com/display/BAMBOO/Bamboo+Specs">Bamboo Specs</a>
 */
@BambooSpec
public class PlanSpec {

    // Encrypted via Bitbucket
    private static String SshPrivateKey = "BAMSCRT@0@0@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";
    private static String SshPublicKey = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQC3XbV9df9/4/By1NeThiPAh8yF83gHNQx374iJ3XUS2blNNxXaGiimpMFrV2vmRXul35PY8645fJfiYDzHNPo8LzyMpkXNu0K5FZJoPixhShyj5nM7RUtvXmVXzteJnVhbqkIS2VmRCzJRda9krB/rgKf+z5Dk2xasTg6FU6rmO3D3qXPoplMYSqUTklHK5ghFjrrUcfHjwOBmPQ/Pk6zG1VZt6JrooFRdETfpr1fLpp+yZ+yun4wFACzuI6sSFD4edgX+hw+19W65mw9/Og9LXn/OrNlrAeI7cVFxD/oYB75He9hFTO+xDXBARsC/jvifWy3PcbsbmQQpNIJRyvCZ bitbucket-builds\n";
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

        // Sanity build - no deployment
//        createSanityPlan(bambooServer);


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

        String regex = "^.*" + servicePath.replace("/", "\\/") + "\\/.*";
        Plan plan = new Plan(project(), name, key)
                .planRepositories(new BitbucketServerRepository()
                        .name("code")
                        .server(new ApplicationLink()
                                .name("E-Tools Bitbucket"))
                        .projectKey("CIV")
                        .repositorySlug("con-services")
                        .branch("master")
                        .sshCloneUrl("ssh://git@bitbucket.trimble.tools/civ/con-services.git")
                        .sshPublicKey(SshPublicKey)
                        .sshPrivateKey(SshPrivateKey)
                        .shallowClonesEnabled(true)
                        .fetchWholeRepository(false)
                        .changeDetection(new VcsChangeDetection()
                            .quietPeriodEnabled(true)
                            .filterFilePatternOption(VcsChangeDetection.FileFilteringOption.INCLUDE_ONLY)
                            .filterFilePatternRegex(regex)))
//                .linkedRepositories("con-services")

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
                .triggers(new RepositoryPollingTrigger()
                        .pollEvery(10, TimeUnit.MINUTES))
                .planBranchManagement(new PlanBranchManagement()
                        .createForVcsBranchMatching("feature/.*") // Don't monitor feature branches
                        .notificationForCommitters()
//                        .branchIntegration(new BranchIntegration()
//                                .integrationBranchKey("MASTER")
//                                .gatekeeper(true)
//                                .pushOnSuccessfulBuild(true))
                        .delete(new BranchCleanup()
                                .whenRemovedFromRepository(true)
                                .whenInactiveInRepositoryAfterDays(7)));


        addPlanStages(plan, runAcceptanceTest);

        PlanPermissions planPermission = PlanSpec.createPlanPermission(plan.getIdentifier());
        bambooServer.publish(planPermission);

        // These deployments need to be created by Support, we don't have the ability to create new deployments currently
        Deployment deployment = createDeployment(bambooServer, key, "Deployment for Civil 3D - " + name);

        bambooServer.publish(deployment);
        bambooServer.publish(plan);
    }

    void createSanityPlan(BambooServer bambooServer) {
        Plan plan = new Plan(project(), "Services Sanity Build", "SANITY")
                .planRepositories(new BitbucketServerRepository()
                        .name("code")
                        .server(new ApplicationLink()
                                .name("E-Tools Bitbucket"))
                        .projectKey("CIV")
                        .repositorySlug("con-services")
                        .branch("master")
                        .sshCloneUrl("ssh://git@bitbucket.trimble.tools/civ/con-services.git")
                        .sshPublicKey(SshPublicKey)
                        .sshPrivateKey(SshPrivateKey)
                        .shallowClonesEnabled(true)
                        .fetchWholeRepository(false)
                        .changeDetection(new VcsChangeDetection()
                                .quietPeriodEnabled(true)))
                .description("Plan created from Bamboo Java Specs")
                .notifications(new EmptyNotificationsList())
                .triggers(new RepositoryPollingTrigger()
                        .pollEvery(1, TimeUnit.MINUTES))
                .planBranchManagement(new PlanBranchManagement()
                        .createForVcsBranchMatching("feature/.*")
                        .notificationForCommitters()
                        .delete(new BranchCleanup()
                                .whenRemovedFromRepository(true)
                                .whenInactiveInRepositoryAfterDays(7)));

        Stage stage = new Stage("Sanity Builds");
//        createSanityJob(stage, "COMMON", "Common", "src/Common/", "Common.sln", "");
        createSanityJob(stage, "ASSETMGMT", "Asset Management", "src/service/3dAssetMgmt/", "VSS.Productivity3D.3DAssetMgmt.sln", "test");
//        createSanityJob(stage, "3DNOW", "3D Now", "src/service/3dNow/VSS.Productivity3D.3DNow.sln", "test");
        createSanityJob(stage, "3DP", "3DP", "src/service/3DP/", "VSS.Productivity3D.Service.sln", "test");
        createSanityJob(stage, "FILEACCESS", "File Access", "src/service/FileAccess/","VSS.Productivity3D.FileAccess.Service.sln", "test");
        createSanityJob(stage, "FILTER", "Filter", "src/service/Filter/","VSS.Productivity3D.Filter.sln", "test");
//        // Has no tests
//        createSanityJob(stage, "MOCKAPI", "Mock Web API", "src/service/MockProjectWebApi/MockProjectWebApi.sln", "test");
        createSanityJob(stage, "PROJECT", "Project", "src/service/Project/","VSS.Visionlink.Project.sln", "test");
        createSanityJob(stage, "PUSH", "Push", "src/service/Push/","VSS.Productivity3D.Push.sln", "test");
        createSanityJob(stage, "SCHEDULER", "Scheduler", "src/service/Scheduler/","VSS.Productivity3D.Scheduler.sln", "test");
        createSanityJob(stage, "TFA", "Tag File Auth", "src/service/TagFileAuth/","VSS.TagFileAuth.Service.sln", "test");
        createSanityJob(stage, "TILE", "Tile", "src/service/TileService/","VSS.Tile.Service.sln", "test");
        createSanityJob(stage, "TREX", "TRex", "src/service/TRex/","TRex.netstandard.sln", "tests");

        plan.stages(stage);

        PlanPermissions planPermission = PlanSpec.createPlanPermission(plan.getIdentifier());
        bambooServer.publish(planPermission);
        bambooServer.publish(plan);
    }

    void createSanityJob(Stage stage, String key, String serviceName, String serviceFolder, String solutionName, String testFolder) {
        String solutionPath = serviceFolder + solutionName;
        String dotnetParms = "-r linux-x64 -p:AllowUnsafeBlocks=true";

        // Command we are trying to run
        // find src/Common/ -type f -name "*.csproj" | xargs -I@ sh -c "dotnet test -r linux-x64 -p:AllowUnsafeBlocks=true --test-adapter-path:. --logger:\"nunit;LogFilePath=\$(pwd)\TestResults/\$(basename @).xml\" @"
        String testCommand = String.format("find %s%s -type f -name \"*.csproj\" | xargs -I@ sh -c \"echo @ && dotnet test -r linux-x64 -p:AllowUnsafeBlocks=true --test-adapter-path:. --logger:\\\"nunit;LogFilePath=$(pwd)/TestResults/\\$(basename @).xml\\\" @ \"",
                serviceFolder,
                testFolder);

        ScriptTask buildScript = new ScriptTask()
                .inlineBody("dotnet build " + dotnetParms + " " + solutionPath)
                .interpreterBinSh();

        ScriptTask testScript = new ScriptTask()
                .inlineBody(testCommand)
                .interpreterBinSh();

        TestParserTask testParserTask = TestParserTask.createNUnitParserTask()
                .resultDirectories("TestResults/*.xml");

        Job buildJob = new Job(serviceName, "SAN"+key)
                .tasks(cleanTask(), checkoutCodeTask(), buildScript, testScript)
                .artifacts(new Artifact(serviceName + "Test Results")
                        .location("TestResults/")
                        .copyPattern("*.xml")
                        .shared(true))
                .finalTasks(testParserTask);
//                .requirements(new Requirement("team")
//                        .matchType(Requirement.MatchType.EQUALS)
//                        .matchValue("merino"));

        stage.jobs(buildJob);
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
//                .requirements(new Requirement("team")
//                        .matchType(Requirement.MatchType.EQUALS)
//                        .matchValue("merino"))
                .artifacts(artifact());

        if(runAcceptanceTest)
            buildJob.tasks(testCoverageTask());

        buildJob.finalTasks(cleanTask());

        Stage jenkinsStage = new Stage("Build Jenkins")
                .jobs(buildJob);

        plan.stages(jenkinsStage);

        return plan;
    }



    VcsCheckoutTask checkoutCodeTask() {
        return new VcsCheckoutTask().addCheckoutOfDefaultRepository().cleanCheckout(true);
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
