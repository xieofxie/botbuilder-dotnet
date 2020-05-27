
@echo off
if "%REPOS%" EQU "" goto help
set region=%1
if "%region%" EQU "" set region=westus

if exist luis.settings.* goto run
call build.cmd %1 %2
if not exist luis.settings.* goto done

:run
call nuget sources add -name "MyGet" -source "https://botbuilder.myget.org/F/botbuilder-v4-dotnet-daily/api/v3/index.json" -verbosity quiet 2> nul
dotnet run --project %REPOS%/BotBuilder-Samples/experimental/generation/runbot/runbot.csproj --root %~dp0 --region %region%

if %errorlevel% EQ 0 goto done

:help
echo In order to use this script you must:
echo 1) Setup from https://github.com/microsoft/BotBuilder-Samples/blob/master/experimental/generation/docs/get-started.md
echo 2) Set the environment variable REPOS to your git repo root.
echo run [region] [luis authoring key]
echo Region defaults to westus.
echo Key defaults to LUIS_AUTHORING_KEY environment variable.

:done
