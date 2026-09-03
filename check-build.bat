@echo off
echo Building solution...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo.
    echo Build FAILED with errors
    pause
) else (
    echo.
    echo Build SUCCEEDED
    pause
)