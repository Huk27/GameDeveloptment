@echo off
echo 예술 활동 모드 빌드 시작...

REM .NET 5.0 SDK가 설치되어 있는지 확인
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo .NET 5.0 SDK가 설치되어 있지 않습니다.
    echo https://dotnet.microsoft.com/download/dotnet/5.0 에서 다운로드하세요.
    pause
    exit /b 1
)

REM 프로젝트 빌드
echo 프로젝트 빌드 중...
dotnet build --configuration Release

if %errorlevel% neq 0 (
    echo 빌드 실패!
    pause
    exit /b 1
)

echo 빌드 성공!
echo.
echo 모드 파일들이 bin\Release\net5.0\ 폴더에 생성되었습니다.
echo.
echo 설치 방법:
echo 1. bin\Release\net5.0\ 폴더의 모든 파일을 복사
echo 2. 스타듀 밸리 Mods 폴더에 붙여넣기
echo 3. 게임 실행
echo.
pause
