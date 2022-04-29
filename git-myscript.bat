@echo off
cls
echo Date format = %date%
echo dd = %date:~0,2%
echo mm = %date:~3,2%
echo yyyy = %date:~6,8%
echo.
echo Time format = %time%
echo hh = %time:~0,2%
echo mm = %time:~3,2%
echo ss = %time:~6,2%
echo.
echo Timestamp = %date:~0,2%_%date:~3,2%_%date:~6,8%-%time:~0,2%_%time:~3,2%_%time:~6,2%
#!/bin/bash
git status
git add .
git commit -m "generated files on date: %date:~0,2%-%date:~3,2%-%date:~6,8%"