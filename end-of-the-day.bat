@echo off
cls
echo Date format = %date%
echo dd = %date:~7,2%
echo mm = %date:~4,2%
echo yyyy = %date:~10,4%
echo.
#!/bin/bash
git status
git add .
git commit -m "generated files on date: %date:~7,2%-%date:~4,2%-%date:~10,4%"
git push