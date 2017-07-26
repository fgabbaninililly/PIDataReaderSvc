echo off
REM CREATION
SET SVCNAME=PIDataReaderSvcTest03
SET SVCEXE=C:\Program Files\Eli Lilly\PIDataReaderApps\PIDataReaderSvc.exe
SET CFGFILE=D:\Projects\DOTNET\PIDataReaderApps\SampleConfigFiles\config.batches.xml
SET FILEOPTION=none

REM CONFIGURATION
SET USER=EMA\XFDIAOSIPI
SET PASSWD=DiaOSIPI01
SET STARTTYPE=auto
SET FAILURE1=restart
SET FAILURE2=restart
SET FAILURE3=restart
SET MILLISECBEFORERESTART=60000

rem echo executing sc.exe create with following parameters: %SVCNAME% binPath= "%SVCEXE% -c %CFGFILE% -f %FILEOPTION%"
sc.exe create %SVCNAME% binPath= "%SVCEXE% -c %CFGFILE% -f %FILEOPTION%"

rem echo executing sc.exe config with following parameters: %SVCNAME% obj= "%USER%" password= "%PASSWD%"
sc.exe config %SVCNAME% obj= "%USER%" password= "%PASSWD%"
sc.exe config %SVCNAME% start= "%STARTTYPE%"

sc failure %SVCNAME% actions= %FAILURE1%/%MILLISECBEFORERESTART%/%FAILURE2%/%MILLISECBEFORERESTART%/%FAILURE3%/%MILLISECBEFORERESTART% reset= 0

echo Service was correctly configured.
echo Type net start %SVCNAME% to start.
