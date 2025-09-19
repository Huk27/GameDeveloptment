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
- **상세 기록**: `docs/research/StardewUI/DataBindingTroubleshooting.md`

#### 2. 조건부 렌더링 문법 오류 (SimpleUI 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: `*if={SelectedTab == "overview"}` 문법 오류
- **해결**: ViewModel에 Boolean 프로퍼티 생성 후 `*if={ShowOverviewTab}` 사용
- **핵심 원칙**: StardewUI는 직접적인 비교 연산을 지원하지 않음

#### 3. 탭 시스템 구현 (SimpleUI 모드)
- **발생 일시**: 2024년 7월 25일
- **문제**: 탭 전환 시 콘텐츠가 표시되지 않음
- **해결**: `OnTabActivated` 메서드에서 모든 조건부 프로퍼티에 PropertyChanged 이벤트 발생
- **참고 예제**: `libraries/StardewUI/TestMod/Examples/TabsViewModel.cs`

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
1. **samples**: 실제 작동하는 모드들 (최우선)
2. **원본 소스코드**: 라이브러리 공식 소스코드
3. **docs/**: 개발 가이드 문서들 (참고용)

### 📁 주요 예제 모드
- **StardewUI**: `libraries/StardewUI/TestMod/` - 공식 예제
- **MatrixFishingUI**: `samples/MatrixFishingUI/` - 샘플 다운로드 필요 (현재 비어 있음)
- **PenPals**: 원본 저장소 참고 (현재 워크스페이스에 포함되지 않음)
- **Ferngill Simple Economy**: `samples/Ferngill-Simple-Economy/` - 샘플 다운로드 필요 (현재 비어 있음)

### 📖 문서 위치
- **StardewUI 데이터 바인딩**: `docs/research/StardewUI/DataBindingTroubleshooting.md`
- **모드 개발 가이드**: `docs/design/`
- **통합 이슈 추적**: `docs/issues/IssueResolutionTracker.md` (현재 파일)

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
1. **samples 확인**: 비슷한 기능을 구현한 모드 찾기
2. **공식 문서 참고**: 라이브러리 공식 문서 확인
3. **이슈 기록 검색**: 이 파일에서 관련 이슈 해결 방법 확인
4. **단계별 디버깅**: 문제를 작은 단위로 나누어 해결

---

## 🔧 Issue #5: StardewUI 바인딩 오류 대량 해결 (공식 예제 분석 후)

**날짜**: 2025-09-17  
**모드**: FarmStatistics  
**해결자**: Assistant

### 문제 상황
```
[StardewUI] Failed to update node:
  <tab *repeat={Tabs} layout="64px" active={<>Active} tooltip={Name} activate=|^OnTabActivated(Name)|/>

StardewUI.Framework.Descriptors.DescriptorException: Type TabData does not have a property named 'Active'.
```

다수의 StardewUI 바인딩 오류 발생:
1. `TabData` 클래스 속성 누락 (`Active`, `Name`, `Sprite` 등)
2. 중복 클래스 정의 (`TabData`, `Tabs` 속성)  
3. `Sprite` 타입 불일치 (string vs Tuple<Texture2D, Rectangle>)
4. `*repeat` 지시문에서 `{<>}`, `{.}` 등 잘못된 바인딩 (실제로는 공식 구문이었음)

### 원인 분석 (공식 예제 분석 후 수정)
1. **공식 예제 미참고**: 추측으로 바인딩 구문을 사용하여 실제 공식 사용법과 달랐음
2. **데이터 모델 불일치**: UI에서 참조하는 속성들이 ViewModel 클래스에 정의되지 않음
3. **TabData 구조 불일치**: 공식 예제의 `Tuple<Texture2D, Rectangle>` 대신 `string` 사용
4. **중복 정의**: 개발 과정에서 같은 클래스/속성을 여러 번 정의

### 해결 방법

#### 1. TabData 클래스 표준화 (공식 예제 기준)
```csharp
public class TabData : INotifyPropertyChanged
{
    public string Name { get; set; } = "";                              // 탭 식별자
    public Tuple<Texture2D, Rectangle>? Sprite { get; set; }           // 탭 아이콘 (공식 구조)
    
    private bool _active;
    public bool Active                                                   // 활성화 상태 (PropertyChanged 필수)
    { 
        get => _active;
        set
        {
            if (_active != value)
            {
                _active = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

#### 2. ViewModel 속성 추가
```csharp
// 탭 관련 데이터
public IReadOnlyList<TabData> Tabs { get; set; } = new List<TabData>();
public bool ShowOverviewTab { get; set; } = true;
public bool ShowCropsTab { get; set; } = false;
// ... 다른 Show*Tab 속성들

// 탭 활성화 이벤트 처리
public void OnTabActivated(string tabName)
{
    // 모든 탭 비활성화 후 선택된 탭만 활성화
    foreach (var tab in Tabs) { tab.Active = false; }
    var selectedTab = Tabs.FirstOrDefault(t => t.Name == tabName);
    if (selectedTab != null) { selectedTab.Active = true; }
    
    // Show*Tab 속성들 업데이트
    ShowOverviewTab = tabName == "overview";
    ShowCropsTab = tabName == "crops";
    // ... 다른 탭들
    
    // UI 업데이트 알림
    OnPropertyChanged(nameof(Tabs));
    OnPropertyChanged(nameof(ShowOverviewTab));
    // ... 다른 속성들
}
```

#### 3. 올바른 바인딩 구문 사용 (공식 예제 기준)
```xml
<!-- 잘못된 예시 -->
<label text={<>} />           <!-- ❌ 잘못된 구문 -->
<label text={.} />            <!-- ❌ 잘못된 구문 -->
<lane *if={<ShowTab} />       <!-- ❌ 잘못된 구문 -->

<!-- 올바른 예시 (공식 예제에서 확인됨) -->
<tab active={<>Active} />         <!-- ✅ 공식 예제에서 사용하는 구문 -->
<tab *repeat={Tabs} />            <!-- ✅ 컬렉션 바인딩 (: 없이) -->
<image sprite={Sprite} />         <!-- ✅ Tuple<Texture2D, Rectangle> 타입 -->
<lane *if={ShowTab} />            <!-- ✅ 조건부 렌더링 -->
```

**중요**: 공식 StardewUI 예제 분석 결과, `{<>Active}` 구문이 정식 사용법입니다!

#### 4. 중복 정의 제거
- `grep -n "TabData\|Tabs"` 명령어로 중복 확인
- 한 파일에서만 클래스 정의
- 속성도 한 번만 정의

#### 5. 잘못된 repeat 구문 수정
```xml
<!-- 문제가 있던 코드 -->
<label *repeat={KeyInsights} text={<>} color="#E0E0E0" />

<!-- 해결 방법 1: 하드코딩 -->
<label text="• 가을 작물이 가장 수익성이 높음" color="#E0E0E0" />
<label text="• 동물 관리가 잘 되고 있음" color="#E0E0E0" />
<label text="• 낚시 시간을 줄이고 농사에 집중하면 더 좋을 것" color="#E0E0E0" />

<!-- 해결 방법 2: 올바른 바인딩 (데이터 구조가 맞을 때) -->
<label *repeat={KeyInsights} text={Text} color="#E0E0E0" />
```

### 핵심 원칙 (공식 예제 분석 후 수정)

1. **공식 예제 우선**: 추측하지 말고 `libraries/StardewUI/TestMod/Examples/` 참고 필수
2. **올바른 바인딩 구문**: `{<>PropertyName}` (공식), `{PropertyName}` (직접), `*repeat={Collection}` (: 없이)
3. **TabData 구조**: `Name` (string), `Sprite` (Tuple<Texture2D, Rectangle>), `Active` (bool with INotifyPropertyChanged)
4. **데이터 모델 일치**: UI에서 참조하는 모든 속성이 ViewModel에 정의되어야 함
5. **중복 방지**: 빌드 전 `grep` 명령어로 중복 정의 확인
6. **단계별 테스트**: 바인딩 오류는 한 번에 여러 개 발생하므로 하나씩 수정

### StardewUI 바인딩 구문 정리 (공식 예제 기준)

- `{<>PropertyName}` - 동적 속성 바인딩 (공식 예제에서 사용) ⭐ **중요**
- `{PropertyName}` - 직접 속성 바인딩
- `{@Path/To/Resource}` - 정적 리소스 참조
- `{#localization.key}` - 현지화 텍스트
- `*if={Condition}` - 조건부 렌더링
- `*repeat={Collection}` - 컬렉션 반복 렌더링 (: 없이)

### 참고 자료
- **해결된 파일들**: 
  - `FarmStatisticsViewModel.cs` - TabData 클래스 및 탭 시스템
  - `PlayerInfoViewModel.cs` - TabData 사용 수정
  - `FarmStatistics.sml` - 바인딩 구문 수정
- **빌드 결과**: 오류 32개 → 0개, 경고 67개 (nullable 관련)

---

## 🔧 Issue #6: StardewUI Sprite 변환 오류

**날짜**: 2025-09-17  
**모드**: FarmStatistics  
**해결자**: Assistant

### 문제 상황
```
[StardewUI] Failed to update node:
  <image layout="32px" sprite={<Sprite} vertical-alignment="middle"/>

System.NotSupportedException: No value converter registered for String -> Sprite.
```

### 원인 분석
1. **타입 불일치**: `sprite` 속성에 문자열 값을 바인딩하려고 함
2. **변환기 부재**: StardewUI에서 String을 Sprite 객체로 자동 변환하는 기능이 없음
3. **잘못된 데이터 모델**: TabData.Sprite가 string으로 정의되었지만 UI에서는 Sprite 객체를 요구함

### 해결 방법

#### 해결책 1: 이미지를 텍스트로 교체 (적용됨)
```xml
<!-- 문제가 있던 코드 -->
<image layout="32px" sprite={Sprite} vertical-alignment="middle" />

<!-- 해결된 코드 -->
<label text={Title} vertical-alignment="middle" />
```

#### 해결책 2: 정적 스프라이트 사용 (대안)
```xml
<!-- 모든 탭에 같은 아이콘 사용 -->
<image layout="32px" sprite={@Mods/StardewUI/Sprites/Tab} vertical-alignment="middle" />
```

#### 해결책 3: 조건부 스프라이트 (고급)
```xml
<!-- 탭별로 다른 아이콘 사용 -->
<image *switch={Name}>
    <image *case="overview" layout="32px" sprite={@Mods/StardewUI/Sprites/Overview} />
    <image *case="crops" layout="32px" sprite={@Mods/StardewUI/Sprites/Crops} />
    <!-- ... 다른 탭들 -->
</image>
```

### 핵심 원칙

1. **타입 일치**: UI 속성의 요구 타입과 바인딩 데이터 타입이 일치해야 함
2. **변환기 확인**: 자동 타입 변환이 지원되는지 확인 필요
3. **대안 고려**: 복잡한 바인딩 대신 간단한 해결책 우선 고려
4. **예제 참고**: samples에서 비슷한 사용 사례 확인

### 참고 자료
- **samples sprite 사용법**:
  - 정적: `sprite={@Mods/StardewUI/Sprites/Tab}`
  - 객체: `sprite={ItemData}` (실제 게임 객체)
  - 바인딩: `sprite={:ParsedFish}` (적절한 타입의 속성)
- **해결된 파일**: `FarmStatistics.sml` - image를 label로 교체

---

## 🔧 Issue #7: StardewUI Label 속성 오류

**날짜**: 2025-09-17  
**모드**: FarmStatistics  
**해결자**: Assistant

### 문제 상황
```
[StardewUI] Failed to update node:
  <label text={<Title} vertical-alignment="middle"/>

StardewUI.Framework.Descriptors.DescriptorException: Type Label does not have a property named 'VerticalAlignment'.
```

### 원인 분석
1. **잘못된 바인딩 구문**: `text={<Title}` (여는 괄호 `<` 오류)
2. **지원하지 않는 속성**: `vertical-alignment`는 Label에서 지원하지 않음
3. **요소별 속성 차이**: image와 label은 지원하는 속성이 다름

### 해결 방법

#### 1. 잘못된 바인딩 구문 수정
```xml
<!-- 문제가 있던 코드 -->
<label text={<Title} />

<!-- 해결된 코드 -->
<label text={Title} />
```

#### 2. 지원하지 않는 속성 제거
```xml
<!-- 문제가 있던 코드 -->
<label text={Title} vertical-alignment="middle" />

<!-- 해결된 코드 -->
<label text={Title} />
```

#### 3. 정렬은 부모 컨테이너에서 처리
```xml
<!-- 올바른 방법: 부모에서 정렬 -->
<tab vertical-content-alignment="middle">
    <label text={Title} />
</tab>
```

### 핵심 원칙

1. **요소별 속성 확인**: 각 UI 요소가 지원하는 속성을 정확히 파악
2. **바인딩 구문 검증**: `{<`, `{.}`, `{<>}` 등은 모두 잘못된 구문
3. **예제 참고**: samples에서 해당 요소의 올바른 사용법 확인
4. **정렬 처리**: 자식 요소 정렬은 부모 컨테이너의 `*-content-alignment` 속성 사용

### StardewUI 요소별 정렬 속성 정리

#### Image 요소
- `horizontal-alignment`, `vertical-alignment` 지원
- 자체적으로 정렬 가능

#### Label 요소  
- `vertical-alignment` 지원하지 않음
- 부모의 `vertical-content-alignment`에 의존

#### 컨테이너 요소 (lane, panel 등)
- `horizontal-content-alignment`, `vertical-content-alignment` 사용
- 자식 요소들의 정렬 제어

### 참고 자료
- **samples에서 확인된 사용법**:
  - `<image vertical-alignment="middle" />` ✅
  - `<label text={Property} />` ✅  
  - `<lane vertical-content-alignment="middle">` ✅
- **해결된 파일**: `FarmStatistics.sml` - vertical-alignment 속성 제거

---

## 🔧 Issue #8: 공식 예제 우선 원칙 확립

**날짜**: 2025-09-17  
**모드**: FarmStatistics  
**해결자**: Assistant

### 문제 상황
- 추측으로 StardewUI 바인딩 구문을 작성하여 잘못된 정보를 문서화함
- `{:PropertyName}` 구문을 올바른 것으로 잘못 기록
- 실제로는 `{<>PropertyName}` 구문이 공식 예제에서 사용됨

### 발견 과정
1. **공식 예제 발견**: `libraries/StardewUI/TestMod/assets/views/Example-Tabs.sml`
2. **실제 구문 확인**: `active={<>Active}`, `*repeat={Tabs}`, `sprite={Sprite}`
3. **TabsViewModel.cs 분석**: `Tuple<Texture2D, Rectangle>` 타입 사용 확인

### 해결 방법

#### 1. 공식 예제 우선 원칙 확립
```
1. 추측 금지: 모든 구문은 공식 예제에서 확인 후 사용
2. 예제 위치: libraries/StardewUI/TestMod/Examples/
3. 검증 순서: 공식 예제 → 소스코드 → 문서 → 추측(금지)
```

#### 2. 올바른 StardewUI 바인딩 구문 (공식 확인됨)
```xml
<!-- 탭 시스템 (Example-Tabs.sml) -->
<tab *repeat={Tabs}
     layout="64px"
     active={<>Active}
     tooltip={Name}
     activate=|^OnTabActivated(Name)|>
    <image layout="32px" sprite={Sprite} vertical-alignment="middle" />
</tab>
```

#### 3. 올바른 TabData 구조 (TabsViewModel.cs)
```csharp
internal partial class TabData(string name, Texture2D texture, Rectangle sourceRect) : INotifyPropertyChanged
{
    public string Name { get; } = name;
    public Tuple<Texture2D, Rectangle> Sprite { get; } = Tuple.Create(texture, sourceRect);

    [Notify]
    private bool active;
}
```

### 핵심 교훈

1. **공식 예제 필수 참고**: 추측하지 말고 항상 공식 예제 먼저 확인
2. **문서 검증**: 작성된 문서도 실제 예제와 비교하여 검증 필요
3. **단계적 접근**: 예제 → 소스 → 문서 → 구현 순서 준수
4. **이슈 트래킹 수정**: 잘못된 정보는 즉시 수정하여 향후 참고 시 오류 방지

### 참고 자료
- **공식 예제**: `libraries/StardewUI/TestMod/assets/views/Example-Tabs.sml`
- **ViewModel**: `libraries/StardewUI/TestMod/Examples/TabsViewModel.cs`
- **수정된 파일들**: 
  - `FarmStatisticsViewModel.cs` - TabData 구조 공식 예제에 맞춤
  - `PlayerInfoViewModel.cs` - 동일하게 수정
  - `FarmStatistics.sml` - 바인딩 구문 공식 예제에 맞춤

---

### 📝 기록 작성 가이드
- **문제 상황**: 구체적인 오류 메시지와 증상
- **원인 분석**: 왜 발생했는지 근본 원인 파악
- **해결 방법**: 단계별 해결 과정
- **핵심 원칙**: 다른 상황에서도 적용 가능한 일반적인 원칙
- **참고 자료**: 관련 예제 모드나 문서 위치
## �ű� Issue #9: �м� �� ������ ���ε� ����
**��¥**: 2025-09-17  
**���**: FarmStatistics  
**�ذ���**: Assistant

### ���� ���
- �м� ���� ���� placeholder �ؽ�Ʈ�� ����� ��ȹ�� KeyInsights/ActionableRecommendations �� ǥ�õ��� ����
- �� �ǿ� GrowthComparison �ؽ�Ʈ�� �����Ǿ� 4���� ��ǥ�� �ϰ���� ����
- ViewModel �� ���ڿ� Ű �񱳿� �����Ͽ� ���ڵ� �ջ� �� ��Ÿ�� ���� ���ɼ��� ����

### �ذ� ����
1. FarmStatisticsViewModel �� ActionableRecommendations �Ӽ��� helper �޼��� �߰� �� ��ú��� �����͸� �ε��� ������� ����
2. �� ������ NormalizeComparisonScore/FormatComparisonText �� ����ȭ�Ͽ� Slider/�ؽ�Ʈ�� �ϰ��ǰ� ǥ�õǵ��� ����
3. FarmStatistics.sml �� ���ǿ� GrowthComparison ���� �߰��� 4��° ��ǥ ����
4. dotnet build --configuration Release �������� ������ Ȯ�� (���� CS86xx nullable ����� ���� ���� �ʿ�)

### ���� ����
- mods/active/FarmStatistics/FarmStatisticsViewModel.cs
- mods/active/FarmStatistics/assets/views/FarmStatistics.sml

### �ļ� ��ġ
- AdvancedAnalysisManager �� IAnalysisProvider ���ݿ� ���� nullable ��� ����
