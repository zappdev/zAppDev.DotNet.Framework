CREATE LOGIN AppDev WITH PASSWORD = 'AppDev', CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF;
EXEC sp_addsrvrolemember @loginame = N'AppDev', @rolename = N'sysadmin';