set DOCKER_TLS_VERIFY=1
set DOCKER_HOST=tcp://10.97.96.103:2376
set DOCKER_CERT_PATH=Merino-ProjectMDM
set DOCKER_MACHINE_NAME="Merino-ProjectMDM"

docker-compose -f docker-compose-dev.yml up --build 