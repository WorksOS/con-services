# build stage
FROM node:lts-alpine as build-stage
ARG environment
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN echo "Building for environment: ${environment}"
RUN npm run build -- --mode ${environment}

# production stage
FROM nginx:stable-alpine as production-stage
COPY --from=build-stage /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]