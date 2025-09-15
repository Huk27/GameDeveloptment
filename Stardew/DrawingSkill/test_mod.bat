@echo off
echo 예술 활동 모드 테스트 시작...

REM 모드 빌드
echo 1. 모드 빌드 중...
call build.bat

if %errorlevel% neq 0 (
    echo 빌드 실패! 테스트를 중단합니다.
    pause
    exit /b 1
)

echo.
echo 2. 빌드 성공! 테스트를 진행합니다.
echo.

REM 테스트용 안내
echo 테스트 방법:
echo 1. 스타듀 밸리를 실행하세요
echo 2. SMAPI로 게임을 실행하세요
echo 3. 게임 내에서 다음을 확인하세요:
echo    - Pierre 상점에서 예술 재료 구매 가능
echo    - 제작 메뉴에서 예술 작품 제작 가능
echo    - 예술 작품 제작 시 경험치 획득
echo    - NPC와의 대화에서 예술 관련 대사 확인
echo.

echo 모드 파일 위치: bin\Release\net5.0\
echo.
echo 설치하려면 bin\Release\net5.0\ 폴더의 모든 파일을
echo 스타듀 밸리 Mods 폴더에 복사하세요.
echo.
pause
