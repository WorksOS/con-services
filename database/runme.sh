echo ">>> Making sure MySQL is up"
eval /scripts/wait-for-it.sh "$MYSQL_SERVER_NAME_VSPDB:$MYSQL_PORT" -t 0
echo "<<< Done checking on MySQL"
echo flyway -url="jdbc:mysql://$MYSQL_SERVER_NAME_VSPDB" -schemas="$MYSQL_DATABASE_NAME" -user="$MYSQL_USERNAME_BUILD" '-password=$MYSQL_PASSWORD_BUILD' -locations=filesystem:/scripts/sql -validateOnMigrate=false -outOfOrder=true migrate
eval flyway -url="jdbc:mysql://$MYSQL_SERVER_NAME_VSPDB" -schemas="$MYSQL_DATABASE_NAME" -user="$MYSQL_USERNAME_BUILD" '-password=$MYSQL_PASSWORD_BUILD' -locations=filesystem:/scripts/sql -validateOnMigrate=false -outOfOrder=true migrate
 