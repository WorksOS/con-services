FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:alpine-dind
COPY *.sh /usr/local/bin/ 
RUN chmod 755 /usr/local/bin/*.sh
ENTRYPOINT ["dockerd-entrypoint.sh","bash","update_images.sh"]
