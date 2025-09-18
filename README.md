# Game Development Projects

This repository contains various game development projects and mods.

## Projects

### 🎨 Stardew Valley Mods

#### [FarmDashboard](./Stardew/FarmDashboard/)
Real-time HUD + 메뉴 대시보드로 농장 운영 지표를 요약해 주는 경량 모드입니다. 오늘 수익, 계절 누적, 작물/동물 상태, 활동 시간과 목표 진행도를 즉시 확인할 수 있습니다.

**FarmDashboard Features:**
- 📊 네 장의 HUD 카드로 핵심 지표 표시 (수익, 작물, 동물, 시간)
- 🌱 작물 심기/성장/수확 준비/시든 타일 자동 집계
- 🐄 동물 수, 행복도, 생산 준비 상태를 실시간 추적
- ⏰ 활동 기반 시간 추적 + Gold-per-hour 계산
- 🎯 일일 목표 진행률 바 제공 (수익, 행복도, 수확 준비)
- ⚙️ Generic Mod Config Menu 연동 (토글 키/ 위치 설정)

#### [DrawingSkill](./Stardew/DrawingSkill/)
새로운 드로잉 스킬을 추가하는 대규모 모드입니다. 다양한 영감을 수집하고 전시회를 준비하며, 특수 도구와 활동을 통해 경험치를 획득합니다.

**DrawingSkill Features:**
- 10레벨 스킬 및 전문 직업 시스템
- 25개의 영감과 영구 해금 구조
- NPC 친밀도 기반 도구 관리
- 일일 활동 자동 경험치, StardewUI 기반 UI
- 다국어 지원 (EN, KO, JA, ZH)
- Content Patcher 콘텐츠 팩과 연동

**Technical Implementation Notes:**
- ✅ SMAPI 4.3+ 호환
- ✅ FarmDashboard: 바닐라 HUD/Menu 렌더링 + GMCM 연동 + 활동 추적 로직
- ✅ DrawingSkill: SpaceCore 스킬, StardewUI 뷰모델, Content Patcher 콘텐츠 팩, 다국어 지원
- ✅ Pathoschild & spacechase0 모드 패턴을 기반으로 캐싱/성능 최적화 참고

**Installation:**
1. **FarmDashboard**: `Stardew/FarmDashboard/`를 빌드하여 생성된 `FarmDashboard.dll`과 `manifest.json`을 `Stardew Valley/Mods/FarmDashboard`에 복사
2. **DrawingSkill**: `Stardew/DrawingSkill/`과 `[CP] Drawing Activity/` 두 폴더를 Mods 폴더에 배치
3. SMAPI로 게임 실행

**Requirements:**
- SMAPI 4.3+
- Stardew Valley 1.6+
- SpaceCore mod (DrawingSkill)
- Content Patcher mod (DrawingSkill)
- Generic Mod Config Menu 권장 (FarmDashboard 설정용, 선택 사항)

## Repository Structure

```
stardew/
├── README.md
├── Documentation/
│   ├── AgentOnboardingGuide.md     # 에이전트 온보딩 절차
│   ├── DevelopmentWorkflow.md      # 공통 작업 규칙
│   ├── ModDevelopmentDocument/     # 기획/분석 문서
│   ├── StardewUI/                  # StardewUI 관련 자료
│   └── IssueResolutionTracker.md   # 이슈 해결 로그
├── ExternalLibraries/              # SMAPI, SpaceCore 등 외부 라이브러리
├── ExampleMods/                    # 참고용 커뮤니티 모드 모음
└── Stardew/
    ├── FarmDashboard/
    ├── DrawingSkill/
    ├── [CP] Drawing Activity/
    └── SimpleUI/
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## License

This repository is released under the MIT License. See individual project directories for specific license information.

## Credits

- **Stardew Valley**: ConcernedApe
- **SMAPI**: Pathoschild
- **SpaceCore**: spacechase0
- **StardewUI**: spacechase0
- **Content Patcher**: Pathoschild

## 📚 학습 자료 및 예제

### 🏆 **Pathoschild 모드 컬렉션**

[Pathoschild의 StardewMods 저장소](https://github.com/Pathoschild/StardewMods)를 분석하여 모범 사례와 고급 기법을 학습할 수 있는 자료를 구축했습니다.

#### 📖 **개발 가이드 문서**
- **[SMAPI Developer Guide](./Documentation/ModDevelopmentDocument/SMAPI_Developer_Guide.md)**: 이벤트, 데이터, API 연동 패턴 정리
- **[StardewUI Developer Guide](./Documentation/ModDevelopmentDocument/StardewUI_Developer_Guide.md)**: SML 뷰, ViewEngine 초기화, HUD/메뉴 구성법
- **[SpaceCore Developer Guide](./Documentation/ModDevelopmentDocument/SpaceCore_Developer_Guide.md)**: 커스텀 스킬/직업 및 장비 슬롯 확장
- **[Json Assets Developer Guide](./Documentation/ModDevelopmentDocument/JsonAssets_Developer_Guide.md)**: JSON 콘텐츠 팩 구조와 API 사용법
- **[DGA Integration Guide](./Documentation/ModDevelopmentDocument/DGA_Developer_Guide.md)**: DGA 호환 패턴 및 리플렉션 접근 사례

#### 🎯 **주요 학습 모드**
- **Automate**: 자동화 시스템 설계 패턴
- **ChestsAnywhere**: 복잡한 UI 시스템 구현
- **LookupAnything**: 실시간 데이터 분석 및 표시
- **DataLayers**: 지도 오버레이 시스템
- **Common**: 공통 라이브러리 아키텍처

#### 💡 **적용 가능한 패턴**
- **자동화 시스템**: DrawingSkill 모드의 영감 자동 수집
- **캐싱 시스템**: 성능 최적화를 위한 데이터 캐싱
- **배치 처리**: 대량 데이터 처리 시 효율성
- **UI 시스템**: 고급 검색 및 필터링 기능
- **데이터 분석**: 실시간 통계 및 정보 표시

### 🎯 **실제 구현 모드 (ExampleMods)**

실제로 작동하는 완성된 모드들을 통해 구체적인 구현 방법을 학습할 수 있습니다:

#### 🎣 **MatrixFishingUI** - 낚시 정보 UI 모드
- **학습 포인트**: 게임 데이터 분석, 인게임 UI 메뉴, HUD 오버레이
- **난이도**: 입문자 (간단하고 직관적인 구조)
- **소스**: [LetsTussleBoiz/MatrixFishingUI](https://github.com/LetsTussleBoiz/MatrixFishingUI)

#### 💰 **Ferngill-Simple-Economy** - 경제 시스템 모드
- **학습 포인트**: 복잡한 시스템 아키텍처, 멀티플레이어, Harmony 패치
- **난이도**: 중급자 (체계적인 대규모 모드 구조)  
- **소스**: [paulsteele/Ferngill-Simple-Economy](https://github.com/paulsteele/Ferngill-Simple-Economy)

#### 🎮 **SpacechaseMods** - spacechase0 모드 컬렉션 (15개 모드)
- **학습 포인트**: 스킬 시스템, 대규모 콘텐츠 확장, 고급 게임플레이 메커니즘
- **난이도**: 입문자~전문가 (다양한 난이도의 모드들)
- **소스**: [spacechase0/StardewValleyMods](https://github.com/spacechase0/StardewValleyMods)
- **특징**: 
  - **LuckSkill**: 완전한 커스텀 스킬 시스템 (구버전 참고용)
  - **MoonMisadventures**: 대규모 콘텐츠 확장 (50+ 파일)
  - **BugNet, MoreRings**: 도구 및 장비 시스템
  - **PyromancersJourney**: 완전한 미니게임 구현
  - **DeepSeaFishing**: 새로운 게임 지역 추가

#### 🌟 **CommunityMods** - 커뮤니티 인정 모드 컬렉션 (8개 모드)
- **학습 포인트**: 게임 시스템 마스터, 최신 기술 패턴, 성능 최적화
- **난이도**: 중급자~상급자 (고품질 코드 분석)
- **소스**: [CJBok/SDV-Mods](https://github.com/CJBok/SDV-Mods), [atravita-mods/StardewMods](https://github.com/atravita-mods/StardewMods)
- **특징**:
  - **CJBok Collection**: 게임 시스템 완전 제어, 49개 치트 구현, 16개 언어 지원
  - **Atravita Collection**: 최신 C# 패턴, 65개 Harmony 패치, 지능형 캐싱
  - **CJBCheatsMenu**: 복잡한 탭 기반 UI 시스템의 모범 사례
  - **MoreFertilizers**: 게임 메커니즘 대규모 확장 예제
  - **AtraCore**: 모던 모드 개발 아키텍처

### 📁 **참고 자료 위치**
```
ExampleMods/                        # 실제 구현된 완성 모드들
├── MatrixFishingUI/               # 낚시 UI 모드 (입문자용)
├── Ferngill-Simple-Economy/       # 경제 시스템 모드 (중급자용)
├── SpacechaseMods/                # spacechase0 모드 컬렉션 (15개)
│   ├── UI-Examples/               # UI 구현 예제 (1개)
│   ├── Gameplay-Modifications/    # 게임플레이 수정 (3개)
│   ├── Visual-Enhancements/       # 시각적 개선 (2개)
│   ├── Crafting-Systems/          # 제작 시스템 (1개)
│   ├── Advanced-GameplayMods/     # 고급 게임플레이 (3개)
│   ├── Archived-LearningMods/     # 구버전 학습용 (3개)
│   └── README.md                  # 상세 가이드 및 학습 순서
├── CommunityMods/                 # 커뮤니티 인정 모드 컬렉션 (8개)
│   ├── CJBok-Collection/          # 게임 시스템 마스터 (4개)
│   │   ├── CJBCheatsMenu/         # 종합 치트 시스템
│   │   ├── CJBItemSpawner/        # 아이템 관리 시스템
│   │   ├── CJBShowItemSellPrice/  # 가격 표시 시스템
│   │   └── Common/                # 공통 라이브러리
│   ├── Atravita-Collection/       # 최신 기술 & 성능 (4개)
│   │   ├── AtraCore/              # 모던 개발 프레임워크
│   │   ├── MoreFertilizers/       # 농업 시스템 확장
│   │   ├── GrowableGiantCrops/    # 작물 시스템 혁신
│   │   └── CritterRings/          # 장비 효과 시스템
│   └── README.md                  # 컬렉션 상세 가이드
└── README.md                      # 예제 모드 사용법

ExternalLibraries/PathoschildMods/  # 검증된 패턴 라이브러리
├── Automate/                      # 자동화 시스템 (우선 학습)
├── ChestsAnywhere/                # UI 시스템 (우선 학습)
├── LookupAnything/                # 데이터 분석 (우선 학습)
├── Common/                        # 공통 라이브러리 (필수 학습)
├── TractorMod/                    # 커스텀 도구 시스템
└── ContentPatcher/                # Content Patcher 활용
```

## Development Status

### ✅ Completed
- [x] SMAPI 4.3 compatibility
- [x] SpaceCore skill system implementation
- [x] StardewUI modern interface
- [x] Content Patcher architecture separation
- [x] i18n multilingual support
- [x] Code refactoring based on official guides
- [x] Pathoschild 모드 분석 및 가이드 문서 작성
- [x] 핵심 코드 예제 추출 및 학습 자료 구축

### 🔄 In Progress
- [ ] Icon assets creation
- [ ] Game testing and optimization
- [ ] Performance tuning
- [ ] Pathoschild 패턴을 우리 모드에 적용

### 📝 Future Plans
- [ ] Additional language support
- [ ] Advanced UI features
- [ ] Mod compatibility testing
- [ ] 자동화 시스템 구현 (Automate 패턴)
- [ ] 고급 데이터 분석 시스템 (LookupAnything 패턴)
- [ ] 성능 최적화 적용 (캐싱, 배치 처리)

---

**Updated by jinhyy** - All code now follows best practices from official SMAPI, SpaceCore, and StardewUI guides.
