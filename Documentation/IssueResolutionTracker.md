# 🐛 모드 개발 이슈 해결 기록 통합 관리

> **목적**: 모든 모드 개발 시 발생한 이슈와 해결 방법을 체계적으로 관리하여 동일한 실수를 방지하고 개발 효율성을 향상시킵니다.

## 📋 사용 방법

### 🔍 이슈 검색
1. **키워드 검색**: `Ctrl+F`로 관련 키워드 검색
2. **카테고리별 확인**: 해당 모드/라이브러리 섹션 확인
3. **해결 방법 적용**: 검증된 해결 방법 참고

### 📝 새 이슈 기록
1. **이슈 발생 시**: 즉시 기록 추가
2. **해결 완료 후**: 해결 방법과 핵심 원칙 정리
3. **검증 완료**: 다른 모드에서도 동일한 해결 방법 적용 가능한지 확인

---

## 🎯 카테고리별 이슈 해결 기록

### 📱 StardewUI 관련 이슈

#### 1. 데이터 바인딩 문제 (SimpleUI 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: UI에 `{PropertyName}` 문자열이 그대로 표시됨
- **해결**: One-time binding `{:PropertyName}` 사용, Computed Property에 PropertyChanged 이벤트 발생
- **상세 기록**: `Documentation/StardewUI/DataBindingTroubleshooting.md`

#### 2. 조건부 렌더링 문법 오류 (SimpleUI 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: `*if={SelectedTab == "overview"}` 문법 오류
- **해결**: ViewModel에 Boolean 프로퍼티 생성 후 `*if={ShowOverviewTab}` 사용
- **핵심 원칙**: StardewUI는 직접적인 비교 연산을 지원하지 않음

#### 3. 탭 시스템 구현 (SimpleUI 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: 탭 전환 시 콘텐츠가 표시되지 않음
- **해결**: `OnTabActivated` 메서드에서 모든 조건부 프로퍼티에 PropertyChanged 이벤트 발생
- **참고 예제**: `ExampleMods/StardewUI/TestMod/Examples/TabsViewModel.cs`

#### 4. StarML 동적 레이아웃 바인딩 오류 (FarmStatistics 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: `layout="{:Percentage}% 20px"` 문법 오류로 StarML 파싱 실패
- **해결**: 동적 레이아웃 값을 고정값으로 변경 (`layout="80% 20px"`)
- **핵심 원칙**: StardewUI는 레이아웃 속성에서 동적 바인딩을 지원하지 않음
- **대안**: 프로그레스 바는 Slider 컴포넌트 사용 권장

#### 5. horizontal-content-alignment 속성값 오류 (FarmStatistics 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: `horizontal-content-alignment="space-between"` 및 `"stretch"` 값이 StardewUI에서 지원되지 않음
- **오류**: `System.ArgumentException: Requested value 'space-between' was not found`
- **해결**: `horizontal-content-alignment` 속성 제거 (기본값 사용)
- **핵심 원칙**: StardewUI의 Alignment enum은 3가지 값만 지원
- **지원되는 값**: `Start`, `Middle`, `End`
- **참고**: 양쪽 정렬이 필요한 경우 별도의 레이아웃 구조 사용 필요

#### 6. Lane에 item-spacing 속성 사용 오류 (FarmStatistics 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: `Lane`에 `item-spacing` 속성 사용 시 오류 발생
- **오류**: `DescriptorException: Type Lane does not have a property named 'ItemSpacing'`
- **해결**: `item-spacing` 속성 제거 (margin으로 대체)
- **핵심 원칙**: `item-spacing`은 `Grid` 전용 속성
- **대안**: 개별 아이템에 `margin` 사용하여 간격 조절

#### 7. INotifyPropertyChanged 미구현으로 인한 바인딩 오류 (FarmStatistics 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: `TimeStatistic`과 `GoalStatistic` 클래스가 `INotifyPropertyChanged`를 구현하지 않아 바인딩 실패
- **오류**: `System.NotSupportedException: No value converter registered for String -> Func'2`
- **해결 시도 1**: 두 클래스에 `INotifyPropertyChanged` 인터페이스 구현 및 `PropertyChanged` 이벤트 추가
- **해결 시도 2**: `Progress` computed property를 일반 속성으로 변경하고 `UpdateProgress()` 메서드로 값 계산
- **해결 시도 3**: `value-format` 속성 제거 (StardewUI 버전 호환성 문제 가능성)
- **핵심 원칙**: 
  - StardewUI 데이터 바인딩을 위해서는 ViewModel과 모든 데이터 클래스가 `INotifyPropertyChanged` 구현 필요
  - Computed properties (계산된 속성)는 바인딩 문제를 일으킬 수 있음
  - `slider` 컴포넌트의 `value` 속성은 `float` 타입의 일반 속성이어야 함

#### 8. Slider에 margin 속성 사용 오류 (FarmStatistics 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: `Slider`에 `margin` 속성 사용 시 오류 발생
- **오류**: `DescriptorException: Type Slider does not have a property named 'Margin'`
- **해결**: `slider`에서 `margin` 속성 제거
- **핵심 원칙**: `slider` 컴포넌트는 `margin` 속성을 지원하지 않음
- **대안**: 상위 컨테이너(`lane`, `frame`)에 `margin` 사용하여 간격 조절

### 🎮 Stardew Valley 모드 개발 이슈

#### 1. SpaceCore 스킬 시스템 호환성 (DrawingSkill 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: `GetCustomSkillLevel`, `AddCustomSkillExperience` 메서드 없음
- **해결**: SpaceCore 버전 확인 후 호환되는 API 사용
- **핵심 원칙**: 외부 라이브러리 사용 시 버전 호환성 확인 필수

#### 2. 모드 빌드 및 배포 (SimpleUI 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: 게임 실행 중 DLL 파일 사용으로 인한 배포 실패
- **해결**: 게임 종료 후 빌드 또는 Hot Reloading 활용
- **핵심 원칙**: C# 코드 변경 시 게임 재시작, .sml 파일 변경 시 Hot Reloading

### 🔧 개발 환경 이슈

#### 1. Cursor Rule 파일 적용 문제
- **발생 일시**: 2024년 7월 25일
- **문제**: `.md` 파일이 Rule로 인식되지 않음
- **해결**: `.mdc` 확장자 사용, YAML front matter 구조 적용
- **핵심 원칙**: Cursor Rule 파일은 특정 구조와 확장자 필요

#### 2. C# 언어 버전 호환성
- **발생 일시**: 2024년 7월 25일
- **문제**: C# 10.0에서 컬렉션 식 `[]` 사용 불가
- **해결**: `new List<T>()` 사용
- **핵심 원칙**: .NET 6.0 프로젝트는 C# 10.0 문법 제한

---

## 🚀 모드별 특화 이슈 해결 가이드

### SimpleUI 모드
- **주요 기능**: 플레이어 정보 표시, 탭 시스템, 인벤토리 그리드
- **핵심 이슈**: 데이터 바인딩, 조건부 렌더링, 스크롤 가능한 아이템 그리드
- **해결 패턴**: MatrixFishingUI 모드 구조 참고, StardewUI 공식 예제 우선 적용

### DrawingSkill 모드
- **주요 기능**: 커스텀 스킬 시스템, UI 연동
- **핵심 이슈**: SpaceCore API 호환성, StardewUI 연동
- **해결 패턴**: SpaceCore 버전 확인, 단순화된 기능 구현

---

## 📚 참고 자료 및 예제 모드

### 🎯 우선 참고 순서
1. **ExampleMods**: 실제 작동하는 모드들 (최우선)
2. **원본 소스코드**: 라이브러리 공식 소스코드
3. **Documentation**: 개발 가이드 문서들 (참고용)

### 📁 주요 예제 모드
- **StardewUI**: `ExampleMods/StardewUI/TestMod/` - 공식 예제
- **MatrixFishingUI**: `ExampleMods/MatrixFishingUI/` - 실제 사용 모드
- **PenPals**: `ExampleMods/PenPals/` - 실제 사용 모드
- **Ferngill Simple Economy**: `ExampleMods/Ferngill-Simple-Economy/` - 복잡한 UI 구현

### 📖 문서 위치
- **StardewUI 데이터 바인딩**: `Documentation/StardewUI/DataBindingTroubleshooting.md`
- **모드 개발 가이드**: `Documentation/ModDevelopmentDocument/`
- **통합 이슈 추적**: `Documentation/IssueResolutionTracker.md` (현재 파일)

---

## 🔄 업데이트 로그

### 2024년 7월 25일
- ✅ StardewUI 데이터 바인딩 이슈 해결 기록 추가
- ✅ StardewUI 조건부 렌더링 이슈 해결 기록 추가
- ✅ 통합 이슈 해결 기록 파일 생성
- ✅ 모드별 특화 가이드 섹션 추가

---

## 💡 개발 효율성 향상 팁

### 🚫 실수 방지 체크리스트
- [ ] StardewUI 사용 시 `{:PropertyName}` one-time binding 사용
- [ ] 조건부 렌더링 시 ViewModel에 Boolean 프로퍼티 생성
- [ ] Computed Property 변경 시 PropertyChanged 이벤트 발생
- [ ] 외부 라이브러리 사용 시 버전 호환성 확인
- [ ] C# 코드 변경 시 게임 재시작, .sml 파일 변경 시 Hot Reloading

### 🎯 문제 해결 우선순위
1. **ExampleMods 확인**: 비슷한 기능을 구현한 모드 찾기
2. **공식 문서 참고**: 라이브러리 공식 문서 확인
3. **이슈 기록 검색**: 이 파일에서 관련 이슈 해결 방법 확인
4. **단계별 디버깅**: 문제를 작은 단위로 나누어 해결

### 📝 기록 작성 가이드
- **문제 상황**: 구체적인 오류 메시지와 증상
- **원인 분석**: 왜 발생했는지 근본 원인 파악
- **해결 방법**: 단계별 해결 과정
- **핵심 원칙**: 다른 상황에서도 적용 가능한 일반적인 원칙
- **참고 자료**: 관련 예제 모드나 문서 위치
