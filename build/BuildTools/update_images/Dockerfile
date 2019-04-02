FROM 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:alpine-dind
COPY *.sh /usr/local/bin/ 
RUN chmod 755 /usr/local/bin/*.sh
ENTRYPOINT ["dockerd-entrypoint.sh","bash","update_images.sh"]
