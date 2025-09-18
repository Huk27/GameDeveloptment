# 🤖 Agent Onboarding Guide

이 문서는 다른 컴퓨터나 새로운 세션에서 Codex/AI 에이전트가 동일한 방식으로 작업을 이어갈 수 있도록 표준 절차를 정리한 가이드입니다. `setup_environment.sh`를 기반으로 한 폴더 구조, 참고용 예제 코드, 테스트 전략을 빠르게 파악하는 것을 목표로 합니다.

## 1. 환경 준비 체크리스트
1. **필수 설치 여부 확인**
   - .NET SDK 6.0 이상 (`dotnet --info`)
   - Git, zsh/bash
2. **레포지토리 클론 후 초기화**
   ```bash
   ./setup_environment.sh
   ```
   - 성공 시 `ExternalLibraries/`와 `ExampleMods/`가 채워집니다.
3. **권장 IDE/에디터 설정**
   - VS Code 혹은 JetBrains Rider: `.editorconfig` 준수
   - 한글/영문 혼용 주석 가능 (기본 ASCII 유지)

## 2. 필수 참고 자료
- `Documentation/DevelopmentWorkflow.md`: SMAPI/StardewValley API 사용 전 예제 코드 확인 및 이슈 기록 절차.
- `Documentation/ModDevelopmentDocument/`: 기획서 및 세부 설계 문서.
- `ExampleMods/`와 `ExternalLibraries/SMAPI/src/`: 공식/커뮤니티 모드 구현 예시.

## 3. 신규 작업 기본 절차
1. **기획 문서 검토**: 관련 모듈 기획서 최신화 여부 확인.
2. **레퍼런스 조사**: 유사 기능이 구현된 예제 모드나 SMAPI 소스 참고.
3. **계획 수립**: `update_plan` 도구로 2개 이상 단계 계획 공유.
4. **구현/수정**: 코드 작성 시 최소 주석, ASCII 기본.
5. **검증**: 가능하면 `dotnet build` 또는 커스텀 스크립트 실행.
6. **문서화**: README 또는 관련 문서에 변경사항 반영. 버그 해결 시 `Documentation/IssueResolutionTracker.md` 업데이트.

## 4. 테스트/빌드 메모
- 로컬 빌드 시 `FarmDashboard/FarmDashboard.csproj`의 `HintPath`를 실제 게임 경로에 맞게 수정하거나 `modbuild.config` 사용을 권장.
- 테스트 실행 예시:
  ```bash
  dotnet build Stardew/FarmDashboard/FarmDashboard.csproj
  ```
- 테스트가 불가능할 경우 사유를 최종 보고서에 명시합니다.

## 5. 산출물 구조 개요
```
Stardew/
├── FarmDashboard/       # 신규 농장 대시보드 모드
├── DrawingSkill/        # 기존 드로잉 스킬 모드
├── SimpleUI/            # 간단 UI 예제
└── [CP] Drawing Activity/
```

## 6. 협업 팁
- 다른 작업자의 변경사항은 되돌리지 않습니다.
- `git status -sb`로 변경 범위를 항상 확인하고, 필요 시 사용자와 상의 후 진행합니다.

이 가이드를 지속적으로 업데이트하여 모든 에이전트가 동일한 기준으로 작업할 수 있도록 유지해주세요.
