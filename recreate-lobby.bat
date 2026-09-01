@echo off
cd /d "%~dp0"
echo === Пересоздание Lobby.API ===

echo [1/7] Удаляю старую папку Lobby.API...
rmdir /s /q src\Services\Lobby.API

echo [2/7] Удаляю старые записи из solution...
dotnet sln remove src\Services\Ordering.API\Ordering.API.csproj
dotnet sln remove src\Services\Lobby.API\Lobby.API.csproj

echo [3/7] Создаю новый проект Lobby.API...
dotnet new webapi -n Lobby.API -o src\Services\Lobby.API

echo [4/7] Добавляю ссылку на SharedKernel...
dotnet add src\Services\Lobby.API\Lobby.API.csproj reference src\Shared\SharedKernel\SharedKernel.csproj

echo [5/7] Обновляю уязвимый пакет OpenApi...
dotnet add src\Services\Lobby.API\Lobby.API.csproj package Microsoft.OpenApi

echo [6/7] Добавляю в solution...
dotnet sln add src\Services\Lobby.API\Lobby.API.csproj

echo [7/7] Создаю структуру папок...
mkdir src\Services\Lobby.API\Domain
mkdir src\Services\Lobby.API\Application
mkdir src\Services\Lobby.API\Infrastructure
mkdir src\Services\Lobby.API\Controllers

echo === Готово! ===
pause