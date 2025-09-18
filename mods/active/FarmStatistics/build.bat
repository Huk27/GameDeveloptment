@echo off
setlocal

echo FarmStatistics 모드 빌드 시작...
set "DOTNET_VERSION="

for /f "delims=" %%v in ('dotnet --version 2^>nul') do (
    set "DOTNET_VERSION=%%v"
    goto :checkVersion
)

:checkVersion
if not defined DOTNET_VERSION (
    echo dotnet CLI를 찾을 수 없습니다. https://dotnet.microsoft.com/download 에서 .NET 8 SDK를 설치하세요.
    pause
    exit /b 1
)

for /f "tokens=1 delims=." %%v in ("%DOTNET_VERSION%") do set "DOTNET_MAJOR=%%v"

if not "%DOTNET_MAJOR%"=="8" (
    echo 감지된 dotnet SDK 버전이 8이 아닙니다. 현재 버전: %DOTNET_VERSION%
    echo .NET 8 SDK 설치를 권장합니다.
    echo 계속 진행합니다...
)

echo 프로젝트 빌드 중...
dotnet build --configuration Release

if %errorlevel% equ 0 (
    echo.
    echo ========================================
    echo 빌드 성공!
    echo ========================================
    echo.
    echo 다음 단계:
    echo 1. StardewUI 모드가 설치되어 있는지 확인
    echo 2. 빌드 산출물을 Stardew Valley\Mods\FarmStatistics 로 복사
    echo 3. 게임에서 'P' 키로 UI 테스트
    echo.
) else (
    echo.
    echo ========================================
    echo 빌드 실패!
    echo ========================================
    echo 오류를 확인하고 다시 시도하세요.
    echo.
)

pause
endlocal