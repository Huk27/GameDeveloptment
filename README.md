# Game Development Projects

This repository contains various game development projects and mods.

## Projects

### 🎨 Stardew Valley Mods

#### [DrawingSkill](./Stardew/DrawingSkill/)
A comprehensive Stardew Valley mod that adds a new Drawing skill to the game. Create various drawing works, manage inspirations, and participate in art exhibitions.

#### [FarmStatistics](./Stardew/FarmStatistics/)
A StardewUI-based mod that displays comprehensive farm statistics with demo data. Track crops, animals, time usage, and goals with beautiful visualizations.

**FarmStatistics Features:**
- 📊 Comprehensive farm statistics dashboard
- 🌱 Crop statistics with harvest data and revenue tracking
- 🐄 Animal statistics with product counts and happiness levels
- ⏰ Time tracking for different activities (farming, mining, fishing, combat, foraging)
- 🎯 Goal setting and progress tracking with visual progress bars
- 🎨 Beautiful StardewUI-based interface with tab system
- 🔥 Hot reloading support for development
- 📱 Responsive grid layouts and card-based design

**DrawingSkill Features:**
- New Drawing skill with 10 levels and profession system
- 25 unique inspirations with permanent unlock system
- Tool management based on NPC relationships
- Daily activities for automatic experience gain
- Modern StardewUI-based interface
- Multilingual support (EN, KO, JA, ZH)
- Content Patcher architecture for easy customization

**Technical Implementation:**
- ✅ SMAPI 4.3 compatible
- ✅ SpaceCore framework integration
- ✅ StardewUI modern interface system
- ✅ Content Patcher architecture separation
- ✅ i18n internationalization support
- ✅ Best practices from official guides
- ✅ Pathoschild 모드 패턴 적용 (자동화, 캐싱, 성능 최적화)

**Installation:**
1. **FarmStatistics**: Download `Stardew/FarmStatistics/` folder and extract to your Stardew Valley Mods folder
2. **DrawingSkill**: Download both mod folders: `Stardew/DrawingSkill/` and `Stardew/CP_DrawingActivity/`
3. Launch the game with SMAPI

**Requirements:**
- SMAPI 4.3+
- Stardew Valley 1.6+
- StardewUI mod (for FarmStatistics)
- SpaceCore mod (for DrawingSkill)
- Content Patcher mod (for DrawingSkill)

## Repository Structure

```
stardew/
├── README.md
├── Documentation/                       # 개발 문서
│   ├── ModDevelopmentDocument/          # 모드 개발 가이드
│   │   ├── Pathoschild 모드 분석 가이드.md
│   │   ├── Pathoschild 핵심 코드 예제.md
│   │   ├── SpaceCore 스킬 추가 및 CP 연동 가이드.md
│   │   ├── StardewUI 모드 개발자 가이드 (심화 예제 포함).md
│   │   └── 스타듀밸리 SMAPI API 활용 가이드.md
│   ├── StardewUI/                       # StardewUI 관련 문서
│   │   ├── DataBinding.md
│   │   ├── DataBindingTroubleshooting.md
│   │   └── StarML.md
│   └── IssueResolutionTracker.md        # 이슈 해결 기록
├── ExternalLibraries/                   # 외부 라이브러리 및 예제
│   ├── SMAPI/                          # SMAPI 소스코드
│   ├── SpaceCore/                      # SpaceCore 프레임워크
│   ├── StardewValleyMods/              # 기타 모드들
│   └── PathoschildMods/                # Pathoschild 모드 모음 (새로 추가)
│       ├── Automate/                   # 자동화 시스템
│       ├── ChestsAnywhere/             # 어디서나 상자 접근
│       ├── LookupAnything/             # 정보 조회 시스템
│       ├── DataLayers/                 # 데이터 레이어
│       ├── Common/                     # 공통 라이브러리
│       └── ... (기타 9개 모드)
├── ExampleMods/                        # 예제 모드들
└── Stardew/                            # 우리가 개발한 모드들
    ├── FarmStatistics/                  # Farm Statistics Mod
    │   ├── ModEntry.cs
    │   ├── FarmStatisticsViewModel.cs
    │   ├── PlayerInfoViewModel.cs
    │   ├── manifest.json
    │   ├── assets/
    │   │   └── views/
    │   │       ├── FarmStatistics.sml
    │   │       ├── PlayerInfo.sml
    │   │       └── PlayerInfoTabs.sml
    │   └── FarmStatistics.csproj
    ├── DrawingSkill/                    # Logic Mod
    │   ├── DrawingActivityMod.cs
    │   ├── DrawingSkill.cs
    │   ├── DrawingInspirationSystem.cs
    │   ├── DrawingToolManager.cs
    │   ├── DrawingDailyActivities.cs
    │   ├── DrawingInspirationEncyclopedia.cs
    │   ├── DrawingInspirationState.cs
    │   ├── manifest.json
    │   ├── i18n/
    │   │   ├── en.json
    │   │   ├── ko.json
    │   │   ├── ja.json
    │   │   └── zh.json
    │   ├── UI/
    │   │   ├── DrawingWorkbench.sml
    │   │   ├── DrawingWorkbenchViewModel.cs
    │   │   ├── DrawingInspirationEncyclopedia.sml
    │   │   └── DrawingInspirationEncyclopediaViewModel.cs
    │   └── DrawingActivityMod_DesignDocument.md
    ├── [CP] Drawing Activity/           # Content Pack
    │   ├── manifest.json
    │   ├── content.json
    │   └── assets/
    │       └── drawing_skill_icon.png
    └── SimpleUI/                        # 예제 모드
        └── PlayerInfoViewModel.cs
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

#### 📖 **분석 문서**
- **[Pathoschild 모드 분석 가이드](./Documentation/ModDevelopmentDocument/Pathoschild%20모드%20분석%20가이드.md)**: 13개 모드의 카테고리별 분석 및 학습 포인트
- **[Pathoschild 핵심 코드 예제](./Documentation/ModDevelopmentDocument/Pathoschild%20핵심%20코드%20예제.md)**: 실제 코드 예제와 우리 프로젝트 적용 방안

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

