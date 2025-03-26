@echo off
setlocal

:: 目标文件夹
set "DEST=MSSpeechServer"

:: 确保目标目录存在
if not exist "%DEST%" mkdir "%DEST%"

:: 复制文件和文件夹
xcopy /E /I /Y "bash" "%DEST%\bash"
xcopy /E /I /Y "bin" "%DEST%\bin"
xcopy /E /I /Y "libs" "%DEST%\libs"
copy /Y "Dockerfile" "%DEST%\"
copy /Y "docker-compose.yml" "%DEST%\"
copy /Y "build.sh" "%DEST%\"
xcopy /E /I /Y "%~dp0MSSpeechServer-net46-src\MSSpeechServer\bin\x86\Release\MSSpeechServer.exe"  "%DEST%\bin"
xcopy /E /I /Y "%~dp0MSSpeechServer-net46-src\MSSpeechServer\bin\x86\Release\MSSpeechServer.exe.config"  "%DEST%\bin"
xcopy /E /I /Y "%~dp0MSSpeechServer-net46-src\MSSpeechServer\bin\x86\Release\Newtonsoft.Json.dll"  "%DEST%\bin"
xcopy /E /I /Y "%~dp0MSSpeechServer-net46-src\MSSpeechServer\bin\x86\Release\Newtonsoft.Json.xml"  "%DEST%\bin"
echo 文件复制完成！
pause
