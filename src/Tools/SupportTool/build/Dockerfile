# build stage
FROM node:lts-alpine as build-stage
ARG environment
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build -- 

# production stage
FROM nginx:stable-alpine as production-stage
RUN apk add --no-cache bash
COPY --from=build-stage /app/dist /usr/share/nginx/html
COPY build/setupConfig.sh .
COPY build/start.sh .
EXPOSE 80
# We need to save the env variables to a file for Node
CMD ["./start.sh"]