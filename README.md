# Stardew Valley Modding Workspace

현대 Stardew Valley 모드 개발을 위한 학습 및 프로토타입 작업공간입니다. 실험 중인 `FarmDashboard` 모드를 중심으로, 참고용 라이브러리와 커뮤니티 모드 스냅샷, 그리고 내부 기획 문서를 한곳에 정리했습니다.

## 지금 집중하고 있는 것들

### FarmDashboard (`mods/in-progress/FarmDashboard`)
- 농장 운영 지표를 HUD와 전용 메뉴로 실시간 요약
- 활동 시간, 작물/동물 상태, 일일 목표 진행도를 추적
- StardewUI 기반 레이아웃으로 확장 준비 중 (현재는 HUD + 커스텀 메뉴 단계)
- `README.md`에 구현 메모와 참조 예제를 정리했습니다

### 학습용 자료
- `libraries/` - SMAPI, SpaceCore, StardewUI 등 핵심 프레임워크와 Pathoschild, spacechase0 소스 아카이브
- `samples/` - 커뮤니티 모드 스냅샷 및 개인 실험 폴더 (SpacechaseMods 7종, CommunityMods 8종 등)
  - `samples/MatrixFishingUI`, `samples/Ferngill-Simple-Economy`는 현재 비어 있으며, 필요 시 스크립트로 내려받습니다
  - `samples/PersonalExperiments/SimpleUIPrototype`에 StardewUI 실험 코드가 있습니다
- `docs/` - 기획/연구/이슈 정리: `design/`, `research/`, `issues/`

## 리포지터리 구조

```
stardew/
├── README.md
├── SETUP_GUIDE.md
├── DEV_RULES.md
├── docs/
│   ├── design/
│   ├── issues/
│   └── research/
├── libraries/
│   ├── PathoschildMods/
│   ├── SMAPI/
│   ├── SpaceCore/
│   ├── SpacechaseFrameworks/
│   ├── StardewUI/
│   ├── StardewValleyMods/
│   └── README.md
├── mods/
│   └── in-progress/FarmDashboard/
├── samples/
│   ├── CommunityMods/
│   ├── MatrixFishingUI/
│   ├── Ferngill-Simple-Economy/
│   ├── PersonalExperiments/
│   ├── SpacechaseMods/
│   └── README.md
├── setup_environment.sh
└── ... (솔루션/에셋 생성 시 추가)
```

## 환경 준비

1. 요구 사항: .NET 6 SDK, Stardew Valley 1.6+, SMAPI 4.3+
2. 선택 사항: `./setup_environment.sh`를 실행하면 누락된 라이브러리와 샘플 모드를 내려받습니다 *(현재 스크립트는 기존 디렉터리명 `ExternalLibraries/`, `ExampleMods/`를 사용합니다. 실행 후 `libraries/`와 `samples/`로 이동하거나 스크립트를 업데이트하세요.)*
3. 수동 확인: `libraries/SMAPI`, `libraries/StardewUI`, `samples/SpacechaseMods`가 채워져 있는지 점검
4. 모드 개발은 `mods/in-progress/FarmDashboard`에서 진행하고 `dotnet build`로 테스트합니다

## 문서 & 노하우
- `docs/design` - 단계별 개발 보고서, 시스템 설계, 외부 모드 분석
- `docs/research` - StardewUI 데이터 바인딩 등 기술 리서치
- `docs/issues/IssueResolutionTracker.md` - 문제/해결 사례 및 참고 경로 (경로가 전부 최신 구조로 갱신되었습니다)
- `SETUP_GUIDE.md` - 자동/수동 환경 세팅 가이드

## 진행 상황

### 완료
- [x] FarmDashboard HUD/메뉴 골격 구현
- [x] StardewUI 기반 레이아웃 실험 (SimpleUIPrototype)
- [x] spacechase0 & Pathoschild 모드 소스 정리

### 진행 중
- [ ] FarmDashboard UI 리팩터링 및 StardewUI 도입
- [ ] MatrixFishingUI/Ferngill 샘플 자동 다운로드 경로 정리
- [ ] 성능/데이터 캐싱 전략 문서화

### 예정
- [ ] DrawingSkill 기획서를 토대로 신규 모드 프로토타입
- [ ] 고급 UI 위젯 (탭, 그래프) 컴포넌트화
- [ ] 외부 참고 모드 업데이트 자동화

## 크레딧
- ConcernedApe - Stardew Valley
- Pathoschild - SMAPI & Content Patcher
- spacechase0 - SpaceCore, StardewUI, 다양한 예제 모드
- 커뮤니티 모드 개발자분들께 감사드립니다

---

필요한 정보가 누락되었거나 구조와 맞지 않는 부분이 보이면 `docs/issues/IssueResolutionTracker.md`에 추가로 기록해주세요.
