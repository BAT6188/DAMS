@echo off
color 0A
 TIME /T
set datey=%Date:~0,4%
set datemon=%Date:~5,2%
set dated=%Date:~8,2%
set timeh=%time:~0,2% 

if %timeh% == 0  set timeh1=00
if %timeh% == 1  set timeh1=01
if %timeh% == 2  set timeh1=02
if %timeh% == 3  set timeh1=03
if %timeh% == 4  set timeh1=04
if %timeh% == 5  set timeh1=05
if %timeh% == 6  set timeh1=06
if %timeh% == 7  set timeh1=07
if %timeh% == 8  set timeh1=08
if %timeh% == 9  set timeh1=09


echo %timeh%
set timem=%time:~3,2% 
echo %timem%
set times=%time:~6,2% 
set data=%Date:~0,4%/%Date:~5,2%/%Date:~8,2%/%timeh1%/%time:~3,2%/%time:~6,2%
set data1=%Date:~0,4%/%Date:~5,2%/%Date:~8,2%/%time:~0,2%/%time:~3,2%/%time:~6,2%
echo  %data%
echo  %data1%

cd zm
cddz.exe -w time %data%
cddz.exe -w time %data1%

cddz.exe -msc 201411
exit