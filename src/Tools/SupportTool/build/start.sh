#!/bin/bash

bash ./setupConfig.sh > /usr/share/nginx/html/config.js

nginx -g 'daemon off;'