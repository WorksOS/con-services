echo ">>> Making sure MySQL is up"
echo ">>> Sleep for 15 seconds."
sleep 15s
eval /scripts/wait-for-it.sh "$MYSQL_SERVER_NAME_VSPDB:$MYSQL_PORT" -t 0
echo "<<< Done checking on MySQL"
echo "<<< Migrate the database with flyway"
eval flyway -url="jdbc:mysql://$MYSQL_SERVER_NAME_VSPDB" -schemas="$MYSQL_DATABASE_NAME" -user="$MYSQL_USERNAME" -password="$MYSQL_ROOT_PASSWORD" -locations=filesystem:/scripts/sql -validateOnMigrate=false -outOfOrder=true migrate
echo "<<< Finished creating the database and tables"


echo Infinite=$INF
echo 
if [ "$INF" ]; then
    echo Doing infinite loop
    exec /bin/bash -c "trap : TERM INT; sleep infinity & wait" ;
fi