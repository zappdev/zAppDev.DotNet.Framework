#!/bin/bash

sudo wget -qO- https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -

echo "deb [arch=amd64] https://packages.microsoft.com/ubuntu/16.04/prod xenial main" | sudo tee /etc/apt/sources.list.d/mssql.list

sudo apt-get update 
sudo apt-get install mssql-tools unixodbc-dev