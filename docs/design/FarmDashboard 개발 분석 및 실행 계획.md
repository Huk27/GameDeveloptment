# 🛠️ FarmDashboard 개발 분석 및 실행 계획

> **작성일**: 2024-09-19  
> **참조 문서**: `docs/design/FarmDashboard 상세 기획서.md`, `docs/issues/FarmDashboard 작업 기록.md`

## 1. 현황 요약
- **코드 베이스**: HUD 렌더러(`mods/in-progress/FarmDashboard/Hud/DashboardHudRenderer.cs`), 데이터 수집기(`mods/in-progress/FarmDashboard/Services/FarmDataCollector.cs`), StardewUI 메뉴 뷰모델(`mods/in-progress/FarmDashboard/UI/DashboardViewModel.cs`) 초안이 존재.
- **외부 라이브러리**:
  - SMAPI 이벤트 기반 업데이트 패턴 (`StardewModdingAPI.Events`).
  - StardewUI 레이아웃 & ViewEngine (`libraries/StardewUI/Core`, `libraries/StardewUI/TestMod`).
  - HUD 샘플: `samples/SpacechaseMods/UI-Examples/ExperienceBars/Mod.cs`.
- **현재 가능 상태**: HUD 토글, 기본 데이터 스냅샷, 메뉴 뷰모델 뼈대는 이미 구현되어 있어 상세 기획을 실 코드로 옮길 수 있는 기반이 마련됨.

## 2. 기능별 개발 가능성 평가
| 기능 블록 | 기획 요구사항 | 구현 근거/참고 | 난이도 | 코멘트 |
|-----------|---------------|----------------|--------|---------|
| HUD 4카드 | 실시간 지표 + 클릭 연동 | `DashboardHudRenderer.Draw` 기반 렌더링, ExperienceBars HUD 패턴 (`samples/SpacechaseMods/UI-Examples/ExperienceBars/Mod.cs`) | 중 | 카드 클릭 처리 로직과 사운드/애니메이션 추가가 필요. SMAPI `RenderedHud` 처리로 충분히 가능.
| 작물 데이터 | 물 부족, 수확/시든 상태, 타임라인 | 이미 존재하는 `FarmDataCollector.UpdateCropData()` (`mods/in-progress/FarmDashboard/Services/FarmDataCollector.cs`) | 중 | 성장 타임라인 계산 로직 보강 필요. `Crop.dayOfCurrentPhase` 등 사용.
| 동물 데이터 | 행복도, 생산량, Star Pet | `FarmDataCollector.UpdateAnimalData()` 및 `FarmAnimal` API | 중 | 행복도 변화를 추적할 캐시 구조 추가 필요. SMAPI `FarmAnimal.happiness.Value` 접근 가능.
| 시간/활동 | Gold per Hour, 활동별 누적 | `_activityMillis` 누적 로직 (`FarmDataCollector.OnUpdateTicked`) | 하 | UI에서 그래프/게이지 표현만 남음. StardewUI `chart`는 없으므로 커스텀 progress 조합 필요.
| 목표 시스템 | 단기 목표, 배지, 알림 | `FarmDataCollector.UpdateGoals()` 자리만 존재 | 상 | 목표 정의/저장 구조 설계부터 필요. JsonConfig + GMCM 옵션 연계 검토.
| StardewUI 탭 | 5개 탭 + ViewModel 바인딩 | 기획서 구조 + `libraries/StardewUI/TestMod/Examples/TabsViewModel.cs` | 중 | ViewModel 세분화, `PerScreen<DashboardViewModel>` 고려 필요.
| GMCM 설정 | HUD/메뉴 토글, 위치 | `ModEntry.OnGameLaunched`에서 등록 완료 | 하 | 신규 옵션(필터, 경고 토글 등)만 추가하면 됨.
| 멀티플레이 | PerScreen/ModMessage | 현재 단일 플레이어 기준. `PathoschildMods/LookupAnything`의 `PerScreen` 패턴으로 확장 가능 | 중 | 우선 싱글 초점, 후속 단계에서 적용.

## 3. 세부 구현 전략
### 3.1 데이터 서비스
- `FarmDataCollector`에 탭별 섹션 클래스를 분리 (예: `CropSection`, `AnimalSection`) → ViewModel에서 사용하기 쉬운 형태로 제공.
- 성장 타임라인: `crop.currentPhase`, `crop.phaseDays` 조합으로 예상 수확일 계산. 예제: `SpaceCore` 스킬 모드의 `CropMagic` 참고 (`libraries/StardewValleyMods/gameplay/unavoidable/Potioncraft/` 내부 성장 계산 패턴).
- 동물 배지: 하루 단위 행복도 비교를 위해 `_snapshot.AnimalDetails`에 전일 값 저장 후 비교.
- 목표 시스템: 단기 과제 → JSON config (`ModData/dailyGoals.json`)로 저장, `Game1.player.modData`에 진행률 기록.
- 작물 인사이트: `CropInsightAggregator`로 물부족·시든 타일, 예상 가치, 최초 수확까지 남은 일수 등을 계산해 `snapshot.CropInsights`에 누적.
- 동물 인사이트: `AnimalTrackerEntry`로 연속 미쓰다듬기 일수/생산 여부를 추적하고 알림 메시지를 생성.
- 활동 요약: `ActivitySummarySnapshot`에 상위 활동과 추천 메시지를 묶어 Time & Activity 탭과 HUD에 공급.

### 3.2 HUD
- 카드 렌더링 구조를 `DashboardHudRenderer` 내 `DrawCard()` 메서드로 캡슐화.
- 클릭 영역: `IClickableMenu` 없이 HUD 좌표를 직접 감지 (`Game1.getMouseX/Y`와 카드 `Rectangle` 비교).
- 애니메이션: `Game1.viewportFreeze` 없이 `SpriteBatch.Draw`에 scale 파라미터 적용 (ExperienceBars에서 HUD 위치 이동 로직 참고).
- HUD 카드 클릭 시 `ModEntry`가 해당 StardewUI 탭을 활성화하도록 `HitTest` → `OpenDashboardMenu(tab)` 체인 구현.
- Crop 카드 디테일에 "다음 수확 예정" 정보를 추가해 현장 판단을 돕는다.

### 3.3 StardewUI 메뉴
- ViewModel을 탭별 partial 클래스로 분해: `OverviewViewModel`, `CropsViewModel` 등. `DashboardViewModel`은 집합 및 상태 전환 담당.
- ViewEngine 초기화: `mods/in-progress/FarmDashboard/StardewUI/MenuControllerExtensions.cs` 대체. `libraries/StardewUI/TestMod/ModEntry.cs`에서 `ViewEngine.Load` 방식 참조.
- 그래픽 요소:
  - 골드 플로우 카드: `progressbar` 대신 `slider` 컴포넌트 (`libraries/StardewUI/TestMod/assets/views/Example-Tempering.sml`) 응용.
  - 타임라인: `grid` + `label`로 직접 구성.
  - 활동 추천 카드: `ActivitySummarySnapshot`에 저장된 메시지를 SML 프레임으로 노출.

### 3.4 목표/알림 시스템
- MVP: 오늘의 자동 목표 3개를 FarmStatistics 계획서의 규칙 활용.
- 추후: `ICustomEvent` 혹은 `Game1.addHUDMessage`와 연계.
- HUD 및 메뉴 동기화를 위해 `GoalService` 싱글톤 추가, `FarmDataCollector`와 ViewModel 간에 DTO 공유.

## 4. 단계별 개발 플랜
| 단계 | 기간 | 주요 작업 | 의존성 |
|------|------|-----------|--------|
| Phase 0 | 09-19 ~ 09-20 | ViewModel 세분화, StardewUI 탭 구조 골격 작성 | `docs/design/FarmDashboard 상세 기획서.md` 탭 정의 |
| Phase 1 | 09-21 ~ 09-24 | HUD 카드 리팩터, 클릭 핸들러 + 애니메이션 | 기존 `DashboardHudRenderer` |
| Phase 2 | 09-24 ~ 09-28 | Crop/Animal/Activity 데이터 보강, 캐싱 전략 구현 | `FarmDataCollector` |
| Phase 3 | 09-28 ~ 10-02 | Goals 시스템 MVP (자동 목표 + HUD 배지) | Phase 2 데이터 |
| Phase 4 | 10-02 ~ 10-05 | QA 및 최적화 (PerScreen 준비, 로깅, 성능 계측) | 전체 기능 완료 |

## 5. 기술 체크리스트
- [x] `PerScreen<DashboardViewModel>` 적용 (`StardewModdingAPI.Utilities.PerScreen`).
- [x] `RefreshDashboardViewModelIfVisible()` 전환 → 탭별 부분 업데이트 지원.
- [x] HUD 카드 클릭 시 메뉴 탭 이동 로직 구현 (HitTest → OpenDashboardMenu).
- [ ] `Game1.player.team.SetIndividualValue` 사용해 멀티 플레이어 지원 고려.
- [ ] `config.json` 스키마 확장 (HUD/메뉴 토글) — HUD 알림 & 다음 수확 힌트 옵션 추가, 추가 항목 예정.
- [ ] `Monitor.VerboseLog` 기반 디버그 출력 옵션 추가.

## 6. 테스트 전략
- **단위 테스트**: 가능 범위 제한적 → `FarmDataCollector` 로직은 `IMonitor` Mock 불필요, `SMAPI` 없이 별도 프로젝트로 이동 시 테스트 가능.
- **수동 시나리오**:
  1. 봄/여름/가을/겨울 저장 데이터로 HUD/메뉴 동작 검증.
  2. 하루 종일 작업 후 Today Goal 검증 (돈/동물/작물 각각).
  3. HUD 위치 변경, GMCM 저장 확인.
- **성능 측정**: `Stopwatch`로 `RefreshAllData()` 시간 측정, `Monitor.Log` 출력.

## 6.1 StardewUI 바인딩 점검
- `DashboardViewModel`에서 제공하는 탭 속성 (`Overview`, `Crops`, `Animals`, `TimeActivity`, `Goals`)과 StarML `FarmDashboard.sml`의 경로를 모두 검토했고, 누락된 컬렉션 바인딩은 없음.
- 뷰 모델 레코드들은 `DashboardViewTypes.cs`에 정리되어 있으며, 각각 Boolean helper (`HasBar`, `HasProgress`, `HasRecommendation` 등)로 조건부 마크업과 일치.
- 추가 탭/컴포넌트 도입 시 **필수 절차**:
  1. ViewModel에 null 대신 빈 리스트/기본값 반환 확인
  2. StarML에서 동일 경로로 `*repeat` / `*if` 연동
  3. `docs/design/FarmDashboard_UI_Style_Guide.md`에 시각 규칙 추가
  4. `docs/issues/FarmDashboard 작업 기록.md`에 변경 내역 요약

## 7. 리스크 & 대응
| 리스크 | 영향 | 대응 |
|--------|------|------|
| StardewUI 와이어프레임 구현 난이도 | 개발 지연 | TestMod 예제 복사 후 점진적 커스터마이징, `SimpleUIPrototype` 참고.
| 목표 시스템 범위 확장 | 스코프 크리프 | MVP 요구사항을 문서화하고 이후 버전에 분리.
| HUD 클릭 충돌 (다른 모드와) | UX 저하 | HUD 위치/크기 조정 옵션, 클릭 영역 토글 제공.
| 멀티플레이 데이터 비동기 | 데이터 불일치 | `PerScreen` + ModMessage 동기화, beta 테스트로 확인.

## 8. 결론
- 기존 코드와 레퍼런스(mods/in-progress/FarmDashboard, samples/SpacechaseMods, libraries/StardewUI) 덕분에 기획서의 HUD/메뉴/데이터 요구사항은 단계적으로 구현 가능.
- 가장 큰 신규 작업은 **Goals & Rewards** 탭의 목표 시스템으로, MVP 범위를 명확히 해두고 다른 기능을 먼저 안정화하는 것이 권장된다.
- `docs/issues/FarmDashboard 작업 기록.md`에 이번 분석 내용을 반영하고, Phase 0 작업부터 즉시 진행해도 무방한 상태.

---
> 추가 참고: Pathoschild의 `LookupAnything` 및 spacechase0의 `Potioncraft` 코드에서 PerScreen 관리와 성장 계산 로직을 학습하면 구현 리스크를 낮출 수 있다.
