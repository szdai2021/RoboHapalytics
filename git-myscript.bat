@echo off
cls
echo Date format = %date%
echo dd = %date:~0,2%
echo mm = %date:~3,2%
echo yyyy = %date:~6,8%
echo.

#!/bin/bash
git status
git add .
git commit -m "generated files on date: %date:~0,2%-%date:~3,2%-%date:~6,8%"