# Ignite Web Console for TRex POC

This contains all the file requrired to build and deploy the ignite webconsole for TRex.
The console is being evaulated to determine if it is worth including as part of a trex deploy.

## Components
The webconle consists of 3 components
- Web front end
- Web back end
- Web agent

See https://apacheignite-tools.readme.io/docs/kubernetes-installation for full details.

## Build
The ignite supplied images are broken for the backend and agent build and deploy using following commands (from this directory). Dont forget to login to ecr:

```
docker build -t 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-trex:console-webagent -f .\Dockerfile.webagent .

docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-trex:console-webagent

docker build 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-trex:console-backend -f .\Dockerfile.backend .

docker push 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-trex:console-backend

```

Trex needs to be built with rest client support enabled to do this you will need to modify the Dockerfile.build file and add the following line and copy the ignite-rest-http-2.7.0.jar from this directory to the /$SERVICE_PATH/build/ directory (e.g. src/service/TRex/build)


```
COPY ./$SERVICE_PATH/build/ignite-rest-http-2.7.0.jar /trex/libs/
```

You then need to build and deploy trex as normal.

## Deployment
From this directory run:

```
kubectl apply -f .\mongodb-deployment.yaml

kubectl apply -f .\webconsole-deployment.yaml

kubectl apply -f .\webagent-deployment.yaml

```
If you are deploying from scratch see the https://apacheignite-tools.readme.io/docs/kubernetes-installation for how to setup the token prior to deploying the web agent.

## Removal
To remove to ignite webconsole undo the changes done to the Trex Dockerfile.build, and remove ignite-rest-http-2.7.0.jar from the build directory. Build and deploy as usual.

To remove the webconsole from the cluster:

```
kubectl delete -f .\mongodb-deployment.yaml

kubectl delete -f .\webconsole-deployment.yaml

kubectl delete -f .\webagent-deployment.yaml
```

