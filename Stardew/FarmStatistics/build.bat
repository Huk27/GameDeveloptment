@echo off
echo SimpleUI 모드 빌드 시작...

REM .NET 6 SDK가 설치되어 있는지 확인
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo .NET 6 SDK가 설치되지 않았습니다. https://dotnet.microsoft.com/download 에서 다운로드하세요.
    pause
    exit /b 1
)

REM 프로젝트 빌드
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
    echo 2. 이 모드를 Stardew Valley의 Mods 폴더에 복사
    echo 3. 게임을 실행하고 'P' 키를 눌러 UI 테스트
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
