
@echo off
setlocal

set region=%1
if "%region%" EQU "" set region=westus

set key=%2
if "%key%" NEQ "" set key=--authoringKey %key%

echo Building LUIS models
call bf luis:build --luConfig luconfig.json --region=%region% %key%
if %errorlevel% EQU 0 goto done

:help
echo build.cmd [region] [authoringKey]
echo Region defaults to westus.
echo Must have an explicit key or set it using "bf config:set:luis --authoringKey <LUISKEY>"

:done
