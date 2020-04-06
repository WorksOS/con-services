#!/bin/bash


declare -A SERVICE_PATHS
# Service path from root, run unit tests, run db build, run acceptance test, test folder reative to serivce path
SERVICE_PATHS["assetmgmt"]="src/service/3dAssetMgmt:true:false:false:test"
SERVICE_PATHS["3dp"]="src/service/3DP:true:false:false:test"
SERVICE_PATHS["fileaccess"]="src/service/FileAccess:true:false:false:test"
SERVICE_PATHS["filter"]="src/service/Filter:true:true:false:test"
SERVICE_PATHS["project"]="src/service/Project:true:true:false:test"
SERVICE_PATHS["push"]="src/service/Push:true:true:false:test"
SERVICE_PATHS["scheduler"]="src/service/Scheduler:true:true:false:test"
SERVICE_PATHS["tfa"]="src/service/TagFileAuth:true:true:false:test"
SERVICE_PATHS["tile"]="src/service/TileService:true:true:false:test"
SERVICE_PATHS["trex"]="src/service/TRex:true:false:false:tests"

# Check required applications
command -v aws >/dev/null 2>&1 || { echo >&2 "aws required but it's not installed.  Aborting."; exit 1; }
command -v docker >/dev/null 2>&1 || { echo >&2 "docker required but it's not installed.  Aborting."; exit 1; }
# command -v kubectl >/dev/null 2>&1 || { echo >&2 "helm required but it's not installed.  Aborting."; exit 1; }

# Relative to this script
SCRIPT_PATH_TO_ROOT=../../
# Change to root of repoistory, as all docker commands run from there
cd $SCRIPT_PATH_TO_ROOT

RUN_TESTS=false
RUN_AT=false
RUN_DB_BUILD=false
TEST_FOLDER=test
DEBUG=false
AWS_PROFILE=""
SERVICE=""
HOSTNAME=$(hostname | tr '[:upper:]' '[:lower:]') # make lower case
CONTEXT=""

function print_help {
    echo -e "usage $0 -s <service> -p <aws-profle>" >&2
    echo -e "\r\t-s <service> \t\tBuild a service (Required)" >&2
    echo -e "\r\t-p <aws-profile> \tAWS PROFILE used for AWS Commands (ECR LOGIN) (Required for local builds)" >&2
    echo -e "\r\t-c <k8s context> \tKubernetes context to use for AT (current conext will be used if not defined)" >&2
    echo -e "Services that can be built:" >&2
    for key in ${!SERVICE_PATHS[@]}; do echo -e "\t$key" >&2; done
}

PREFIX="rpd-ccss-"
CONTAINER_NAME="940327799086.dkr.ecr.us-west-2.amazonaws.com/${PREFIX}jenkins-sandbox:$HOSTNAME"
CONTAINER_DB_NAME="940327799086.dkr.ecr.us-west-2.amazonaws.com/${PREFIX}jenkins-sandbox:$HOSTNAME.db"
CONTAINER_TEST_NAME="940327799086.dkr.ecr.us-west-2.amazonaws.com/${PREFIX}jenkins-sandbox:$HOSTNAME.tests"

GET_DOCKER_IMAGE_COMMAND="docker images | awk '{print \$3}' | awk ''NR==2''"

while getopts ":s:p:c:d" opts; do
    case ${opts} in
        s) SERVICE=${OPTARG} ;;
        p) AWS_PROFILE=${OPTARG} ;;
        d) DEBUG=true ;;
        c) CONTEXT=${OPTARG} ;;
        \?) print_help ;;
    esac
done

SERVICE_PATH=$(echo ${SERVICE_PATHS[$SERVICE]} | cut -f1 -d:)
RUN_TESTS=$(echo ${SERVICE_PATHS[$SERVICE]} | cut -f2 -d:)
RUN_DB_BUILD=$(echo ${SERVICE_PATHS[$SERVICE]} | cut -f3 -d:)
RUN_AT=$(echo ${SERVICE_PATHS[$SERVICE]} | cut -f4 -d:)
TEST_FOLDER=$(echo ${SERVICE_PATHS[$SERVICE]} | cut -f5 -d:)

# Does the service exist? And it's path?
if [ -z "$SERVICE" ] || [ -z "$SERVICE_PATH" ]; then
    echo >&2 "No service defined $SERVICE"
    print_help
    exit 1
fi

BUILD_DOCKER_FILE=$(pwd)/$SERVICE_PATH/build/Dockerfile.build
RUNTIME_DOCKER_FILE=$(pwd)/$SERVICE_PATH/build/Dockerfile
TEST_DOCKER_FILE=$(pwd)/$SERVICE_PATH/build/Dockerfile.tests

echo -e "Running Serivce $SERVICE ($SERVICE_PATH). \n \
    Run Unit Tests: $RUN_TESTS.\n \
    Run DB Build: $RUN_DB_BUILD.\n \
    Run Acceptance Tests: $RUN_AT.\n \
    Unit Test Folder: $TEST_FOLDER.\n \
    AWS Profile: $AWS_PROFILE.\n \
    K8S Context: $CONTEXT.\n \
    Debug: $DEBUG\n"

if [ ! -f $BUILD_DOCKER_FILE ]; then
    echo -e "Cannot find docker file at $BUILD_DOCKER_FILE - Exiting\n" 1>&2
    exit 1
fi

if [ ! -f $RUNTIME_DOCKER_FILE ]; then 
    echo -e "Cannot find docker file at $RUNTIME_DOCKER_FILE - Exiting\n" 1>&2
    exit 1 
fi

if [ ! -f $TEST_DOCKER_FILE ] && [ $RUN_AT = true ]; then 
    echo -e "Cannot find docker file at $TEST_DOCKER_FILE - Exiting\n" 1>&2
    exit 1 
fi

echo -e "Logging into docker"
# This is just the command to run, if this returns 0 we will still need to execute the command
if [ ! -z "$AWS_PROFILE" ]; then
    AWS_LOGIN_COMMAND="aws ecr get-login-password --profile $AWS_PROFILE --region us-west-2"
else
    AWS_LOGIN_COMMAND="aws ecr get-login-password --region us-west-2"
fi

echo $AWS_LOGIN_COMMAND

docker login --username AWS --password `$AWS_LOGIN_COMMAND` 940327799086.dkr.ecr.us-west-2.amazonaws.com > /dev/null 2>&1

STATUS=$?
if [ $STATUS -ne 0 ]; then
    echo >&2 "Failed to login to docker. Check AWS"
    exit 1
fi
# Actually execute the login command bassed from AWS CLI
eval "$AWS_LOGIN_COMMAND" 2> /dev/null

# Need to change kubenetes context
if [ ! -z "$CONTEXT" ]; then
    echo "Attempting to switch to Kubernetes context $CONTEXT"
    KUBECTL_CONTEXT_SWITCH="kubectl config use-context $CONTEXT"
    $KUBECTL_CONTEXT_SWITCH
fi

if [ "$CONTEXT" = "docker-desktop" ]; then 
    echo "Running in docker desktop envrionment, need to create docker registry secret"

    TOKEN=`aws ecr --region=us-west-2 get-authorization-token --profile $AWS_PROFILE --output text --query authorizationData[].authorizationToken | base64 -d | cut -d: -f2`

    kubectl delete secret --ignore-not-found regcred
    kubectl create secret docker-registry regcred \
    --docker-server=https://940327799086.dkr.ecr.us-west-2.amazonaws.com \
    --docker-username=AWS \
    --docker-password="${TOKEN}" \
    --docker-email="none@none.com"
fi

# echo "Building docker build image"
DOCKER_BUILD_COMMAND="docker build -f $BUILD_DOCKER_FILE . --build-arg SERVICE_PATH=$SERVICE_PATH"
[ $DEBUG = true ] && echo "Docker command: $DOCKER_BUILD_COMMAND"

# # Build the build container
$DOCKER_BUILD_COMMAND
STATUS=$?

if [ $STATUS -ne 0 ]; then
    echo >&2 "Failed to build the build container"
    exit 1
fi

BUILD_IMAGE_ID=$(eval $GET_DOCKER_IMAGE_COMMAND)
echo "Got Image ID: $BUILD_IMAGE_ID"

if [ $RUN_TESTS = true ]; then
    echo "Running tests"

    echo "Test Results: $(pwd)/TestResults"
    mkdir -p $(pwd)/TestResults/UnitTests/
    rm -Rf $(pwd)/TestResults/UnitTest/*


    RUN_TEST_IN_CONTAINER_COMMAND="cd /build && find $SERVICE_PATH/$TEST_FOLDER -type f -name \"*.csproj\" | xargs -I@ sh -c \"echo @ && dotnet test -r linux-x64 -p:AllowUnsafeBlocks=true --test-adapter-path:. --logger:\\\"nunit;LogFilePath=/TestResults/\$(basename @).xml\\\" @ \""
    [ $DEBUG = true ] && echo "Test command running inside docker: $RUN_TEST_IN_CONTAINER_COMMAND"

    TEST_COMMAND="docker run -v$(pwd)/TestResults/UnitTests/:/TestResults $BUILD_IMAGE_ID bash -c '$RUN_TEST_IN_CONTAINER_COMMAND'"
    [ $DEBUG = true ] && echo "Test command: $TEST_COMMAND"
    echo "Running Unit Tests inside Docker"
    echo "--------"
    eval $TEST_COMMAND
    STATUS=$?
    

    if [ $STATUS -ne 0 ]; then
        echo >&2 "Unit Tests failed to run, see results at $(pwd)/TestResults"
        exit 1
    fi 
fi


if [ "$SERVICE" = "trex" ]; then
    echo "Building TRex Runtime containers"

    for COMPONENT in ApplicationServer ConnectedSiteGateway DesignElevation MutableData PSNode QMesh TileRendering TINSurfaceExport Reports Gateway MutableGateway Webtools Utils 
    do
        echo "Build TRex: $COMPONENT"
        TAG=$CONTAINER_NAME.$COMPONENT
        DOCKER_RUNTIME_COMMAND="docker build -f $RUNTIME_DOCKER_FILE . --build-arg SERVICE_PATH=$SERVICE_PATH -t $TAG --build-arg BUILD_CONTAINER=$BUILD_IMAGE_ID --build-arg COMPONENT=$COMPONENT"
        [ $DEBUG = true ] && echo "Docker command: $DOCKER_RUNTIME_COMMAND"

        # # Build the runtime container
        $DOCKER_RUNTIME_COMMAND
        STATUS=$?

        if [ $STATUS -ne 0 ]; then
            echo >&2 "Failed to build the runtime container"
            exit 1
        fi
        #docker push $TAG
    done

else
    # Now build the actual runtime container
    echo "Building docker runtime image"
    DOCKER_RUNTIME_COMMAND="docker build -f $RUNTIME_DOCKER_FILE . --build-arg SERVICE_PATH=$SERVICE_PATH -t $CONTAINER_NAME"
    [ $DEBUG = true ] && echo "Docker command: $DOCKER_RUNTIME_COMMAND"

    # # Build the runtime container
    $DOCKER_RUNTIME_COMMAND
    STATUS=$?

    if [ $STATUS -ne 0 ]; then
        echo >&2 "Failed to build the runtime container"
        exit 1
    fi
    #docker push $CONTAINER_NAME
fi

if [ $RUN_AT = true ]; then 
    # echo "Building Acceptance tests"
    YAML_FOLDER=$(pwd)/$SERVICE_PATH/build/yaml
    ENV_VAR_FILE=$YAML_FOLDER/testingvars.env
    DOCKER_TEST_COMMAND="docker build -f $TEST_DOCKER_FILE . --build-arg SERVICE_PATH=$SERVICE_PATH -t $CONTAINER_TEST_NAME"
    [ $DEBUG = true ] && echo "Docker command: $DOCKER_TEST_COMMAND"
    # Build the test container
    $DOCKER_TEST_COMMAND
    STATUS=$?

    if [ $STATUS -ne 0 ]; then
        echo >&2 "Failed to build the test container"
        exit 1
    fi

    #docker push $CONTAINER_TEST_NAME

    # setup args
    # while IFS="=" read line val
    # do 
    #     echo $line : $val; 
    # done < $ENV_VAR_FILE


    # echo "Applying testing config map"
    # KUBECTL_TESTMAP_APPLY_COMMAND="kubectl apply -f $YAML_FOLDER/testing-configmap.yaml -n testing"
    # [ $DEBUG = true ] && echo "Config map apply command: $KUBECTL_TESTMAP_APPLY_COMMAND"
    # $KUBECTL_TESTMAP_APPLY_COMMAND

    # replace container images in yaml
    sed -e "s#!container!#$CONTAINER_NAME#g; s#!db-container!#$CONTAINER_DB_NAME#g" $YAML_FOLDER/pod.yaml > temp.yaml

    POD_UNIQUE_NAME=$SERVICE-${HOSTNAME}-$(uuidgen)
    echo "new podname $POD_UNIQUE_NAME"

    # set a new container name
    # Also add a image pull secret, if running local

    AT_CONTAINER_NAME="at-tests"

    CONFIGMAP_NAME=$(yq r $SERVICE_PATH/build/yaml/testing-configmap.yaml "metadata.name")
    echo "Found configmap $CONFIGMAP_NAME"

    sed -e "s#!container!#$CONTAINER_NAME#g; s#!db-container!#$CONTAINER_DB_NAME#g" $YAML_FOLDER/pod.yaml | \
        yq w - metadata.name $POD_UNIQUE_NAME | \
        yq w - "spec.imagePullSecrets[+].name" "regcred" | \
        yq w - "spec.containers[+].name" "$AT_CONTAINER_NAME" | \
        yq w - "spec.containers.(name==$AT_CONTAINER_NAME).image" $CONTAINER_TEST_NAME | \
        yq w - "spec.containers.(name==$AT_CONTAINER_NAME).command[+]" "/bin/sh" | \
        # yq w - "spec.containers.(name==$AT_CONTAINER_NAME).args[]" "-c" | \
        yq w - "spec.containers.(name==$AT_CONTAINER_NAME).args[+]"  --tag '!!str' -- -c | \
        yq w - "spec.containers.(name==$AT_CONTAINER_NAME).args[+]"  --tag '!!str' 'while true; do sleep 10; done' | \
        yq w - "spec.containers.(name==$AT_CONTAINER_NAME).envFrom[+].configMapRef.name" "$CONFIGMAP_NAME" | \
        yq w - "spec.containers.(name==$AT_CONTAINER_NAME).tty" true > temp.yaml

    cat temp.yaml

    PODNAME=$(kubectl apply -n testing -f temp.yaml  --validate=false | cut -f 1 -d ' ')
    echo $PODNAME    

    kubectl delete $PODNAME

fi

