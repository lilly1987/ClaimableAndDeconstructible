@echo off
setlocal

set "MOD_ROOT=%~dp0"
set "GAME_ROOT=%MOD_ROOT%..\.."
set "PROJECT=%MOD_ROOT%Mod\ClaimableAndDeconstructible.csproj"
set "HARMONY_DLL=%GAME_ROOT%\..\..\workshop\content\294100\2009463077\1.7\Assemblies\0Harmony.dll"

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: Install the .NET 10 SDK before building this mod.
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

if not exist "%GAME_ROOT%\Assembly-CSharp.dll" (
    echo ERROR: RimWorld Assembly-CSharp.dll not found: "%GAME_ROOT%\Assembly-CSharp.dll"
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

if not exist "%GAME_ROOT%\UnityStubs.dll" (
    echo ERROR: RimWorld UnityStubs.dll not found: "%GAME_ROOT%\UnityStubs.dll"
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

if not exist "%HARMONY_DLL%" (
    echo ERROR: Harmony 1.7 DLL not found: "%HARMONY_DLL%"
    set "BUILD_EXIT_CODE=1"
    goto :finish
)

dotnet build "%PROJECT%" --configuration Release --nologo -p:GameRoot="%GAME_ROOT%"
set "BUILD_EXIT_CODE=%ERRORLEVEL%"

if "%BUILD_EXIT_CODE%"=="0" (
    echo.
    echo Build succeeded: "%MOD_ROOT%Assemblies\ClaimableAndDeconstructible.dll"
)

:finish
if not "%BUILD_EXIT_CODE%"=="0" echo Build failed with exit code %BUILD_EXIT_CODE%.
if /I not "%~1"=="/nopause" pause
exit /b %BUILD_EXIT_CODE%
