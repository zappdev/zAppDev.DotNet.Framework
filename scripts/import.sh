#!/bin/bash

host="localhost"
username="AppDev"
password="AppDev"

/opt/mssql-tools/bin/sqlcmd -S $host -U $username -P $password -d master -i "./sample-model.sql"
/opt/mssql-tools/bin/sqlcmd -S $host -U $username -P $password -d master -i "./sample-data.sql"