#!/usr/bin/python3

import requests, zipfile, io
import sys,getopt
import jenkins
import time
import json
from functools import reduce

# Used to get nested keys in Dict, ie prop.prop2.prop3
def deep_get(dictionary, keys, default=None):
    return reduce(lambda d, key: d.get(key, default) if isinstance(d, dict) else default, keys.split("."), dictionary)

# Convert string to bool
def str2bool(v):
    return str(v).lower() in ("yes", "true", "t", "1")

def get_server(url, username, token):
    server = jenkins.Jenkins(url, username=username, password=token);
    if server.wait_for_normal_op(30):
        user = server.get_whoami()
        version = server.get_version()
        print('Server online, running as `%s`. Jenkins Version: %s' % (user['fullName'], version))
    else:
        sys.exit(2)

    return server

def start_build(server, build, parameters):
    queueId = server.build_job(build, parameters)
    print('Queued build with Queue ID %s - waiting for build to start' % (queueId))

    jobId = None
    
    while jobId == None:
        status = server.get_queue_item(queueId)
        jobId = deep_get(status, "executable.number")
        print("Waiting for job to start")
        time.sleep(5)

    return jobId

def get_build_status(url, server, build, id, debug=False):
    building=True
    status=None
    logStart=0
    while building or status is None:
        time.sleep(1)
        buildInfo = server.get_build_info(build, id)
        building = str2bool(deep_get(buildInfo, "building"))
        status = deep_get(buildInfo, "result")

        if debug:
            print(json.dumps(buildInfo, sort_keys=True, indent=2))
        #print("Current Status: %s (Building: %s)" % (status, building))
        
        if not building and status is not None:
            print(json.dumps(buildInfo, sort_keys=True, indent=2))
        
        # Get logs
        log_url = "%s/job/%s/%d/logText/progressiveText?start=%d" %(url, build, id, logStart)
        r = requests.get(log_url)
        if r.text is not None and len(r.text) > 0:
            print(r.text)
        logStart = int(r.headers['X-Text-Size'])

    return status

def save_artifacts(server, url, build, id):
    artifacts_url = "%s/job/%s/%d/artifact/*zip*/archive.zip" %(url, build, id)
    print("Downloading artifacts from %s" %(artifacts_url))
    r = requests.get(artifacts_url, stream=True)
    z = zipfile.ZipFile(io.BytesIO(r.content))
    z.extractall("./artifacts/")
    print("Extracted Artifacts")

def main(argv):
    url=''
    username=''
    token=''
    build=''
    buildid=''
    branch=''
    servicepath=''
    imagetag=''
    try:
        opts, args = getopt.getopt(argv,"",["url=","username=","token=","build=", "buildid=","branch=","servicepath=","imagetag="])
    except getopt.GetoptError:
        sys.exit(3)

    for opt, arg in opts:
        if opt == '--url':
            url = arg
        elif opt == '--username':
            username = arg
        elif opt == '--token':
            token = arg
        elif opt == '--build':
            build = arg
        elif opt == '--buildid':
            buildid = arg
        elif opt == '--branch':
            branch = arg
        elif opt == '--servicepath':
            servicepath = arg
        elif opt == '--imagetag':
            imagetag = arg


    parameters = {
        "VSTS_BUILD_NUMBER" : buildid,
        "BRANCH_NAME" : branch,
        "SERVICE_PATH" : servicepath,
        "IMAGE_TAG" : imagetag
    }

    s = get_server(url, username, token)
    id = start_build(s, build, parameters)
    print("Job queued with ID %s" %(id))
    result=get_build_status(url, s, build, id);
    print("Build Result: %s" % (result))
    if result != "SUCCESS":
        sys.exit(2)

    save_artifacts(s, url, build, id)
    sys.exit(0)

if __name__ == "__main__":
    main(sys.argv[1:])
