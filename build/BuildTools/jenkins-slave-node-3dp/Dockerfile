FROM node:8
COPY . /root/UI

COPY .netrc /root/
RUN chmod 666 /root/.netrc
ENV HOME /root/

WORKDIR /root/UI
RUN ls -la 
RUN yarn install 
RUN npm install gulp -g
RUN npm i -g gulp-cli
