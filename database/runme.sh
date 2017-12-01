echo ">>> Making sure MySQL is up"
echo ">>> Sleep for 10 seconds"
sleep 10s
eval /scripts/wait-for-it.sh "$MYSQL_SERVER_NAME:$MYSQL_PORT" -t 0
echo "<<< Done checking on MySQL"
echo "<<< Migrate the database with flyway"
echo flyway -url="jdbc:mysql://$MYSQL_SERVER_NAME" -schemas="$MYSQL_DATABASE_NAME" -user="$MYSQL_USERNAME" -password="$MYSQL_ROOT_PASSWORD" -locations=filesystem:/scripts/sql -validateOnMigrate=false -outOfOrder=true migrate
eval flyway -url="jdbc:mysql://$MYSQL_SERVER_NAME" -schemas="$MYSQL_DATABASE_NAME" -user="$MYSQL_USERNAME" -password="$MYSQL_ROOT_PASSWORD" -locations=filesystem:/scripts/sql -validateOnMigrate=false -outOfOrder=true migrate
echo "<<< Finished creating the database and tables"
