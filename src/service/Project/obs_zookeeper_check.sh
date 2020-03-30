#!/bin/bash
#   Use this script to wait until zookeeper is ok
echo '**************** Checking if zookeeper is OK *************************'
echo "using Zookeeper: $KAFKA_ZOOKEEPER_CONNECT"
while ! echo "ruok" | curl -v telnet://$KAFKA_ZOOKEEPER_CONNECT | grep "imok"
	do
		sleep 1
		echo 'Zookeeper not OK, retrying'
	done
echo '******************* Zookeeper is OK **********************************'
