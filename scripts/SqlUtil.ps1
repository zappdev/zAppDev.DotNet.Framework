# 1 DEFINE HELPER FUNCTIONS (CAN BE REUSED)
# function that connects to an instance of SQL Server / Azure SQL Server and saves the 
# connection object as a global variable for future reuse
function ConnectToDB {
    # define parameters
    param(
        [string] $servername,
        [string] $database,
        [string] $sqluser,
        [string] $sqlpassword
    )
    # create connection and save it as global variable
    $global:Connection = New-Object System.Data.SQLClient.SQLConnection
    $Connection.ConnectionString = "server='$servername';database='$database';trusted_connection=false; user id = '$sqluser'; Password = '$sqlpassword'; integrated security='False'"
    $Connection.Open()
    Write-Verbose 'Connection established'
}
# function that executes sql commands against an existing Connection object; In pur case
# the connection object is saved by the ConnectToDB function as a global variable
function ExecuteSqlQuery {
    # define parameters
    param(
         
        [string]
        $sqlquery
        
    )
        
    Begin {
        If (!$Connection) {
            Throw "No connection to the database detected. Run command ConnectToDB first."
        }
        elseif ($Connection.State -eq 'Closed') {
            Write-Verbose 'Connection to the database is closed. Re-opening connection...'
            try {
                # if connection was closed (by an error in the previous script) then try reopen it for this query
                $Connection.Open()
            }
            catch {
                Write-Verbose "Error re-opening connection. Removing connection variable."
                Remove-Variable -Scope Global -Name Connection
                throw "Unable to re-open connection to the database. Please reconnect using the ConnectToDB commandlet. Error is $($_.exception)."
            }
        }
    }
        
    Process {
        #$Command = New-Object System.Data.SQLClient.SQLCommand
        $command = $Connection.CreateCommand()
        $command.CommandText = $sqlquery
        
        Write-Verbose "Running SQL query '$sqlquery'"
        try {
            $result = $command.ExecuteReader()      
        }
        catch {
            $Connection.Close()
        }
        $Datatable = New-Object "System.Data.Datatable"
        $Datatable.Load($result)
        return $Datatable          
    }
    End {
        Write-Verbose "Finished running SQL query."
    }
}
# 2 BEGIN EXECUTE (CONNECT ONCE, EXECUTE ALL QUERIES)
ConnectToDB -servername '192.168.2.201' -database 'ClmsFrameworkSampleDB' -sqluser 'AppDev' -sqlpassword 'AppDev'
ExecuteSqlQuery -sqlquery 'select * from Order' | Format-Table       # use Format-Table for pretier listing
# 3 CLEANUP
$Connection.Close()
Remove-Variable -Scope Global -Name Connection