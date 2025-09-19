# StardewUI 데이터 바인딩 문제 해결 가이드

## 문제 상황
SimpleUI 모드에서 StardewUI 데이터 바인딩이 작동하지 않는 문제

## 증상
- `{HeaderText}`, `{TestProperty}`, `{PlayerName}`은 정상 작동
- `{Health}`, `{Energy}` (int 타입)은 작동하지 않음
- `{HealthText}`, `{EnergyText}` (string 타입)도 작동하지 않음

## 해결 과정

### 1단계: 바인딩 문법 문제
**문제**: `text="{PropertyName}"` 사용
**해결**: `text={:PropertyName}` (one-time 바인딩) 사용

### 2단계: 타입 변환 문제
**문제**: `int` 타입을 문자열과 함께 사용할 때 변환 실패
**해결**: 별도의 `string` 프로퍼티 생성
```csharp
public string HealthText => _health.ToString();
public string EnergyText => _energy.ToString();
```

### 3단계: INotifyPropertyChanged 구현 문제
**문제**: computed property가 변경될 때 이벤트 발생하지 않음
**해결**: MatrixFishingUI 방식으로 수동 구현
```csharp
public int Health 
{ 
    get => _health; 
    set 
    { 
        if (SetField(ref _health, value))
        {
            OnPropertyChanged(nameof(HealthText));
        }
    } 
}
```

### 4단계: StarML 구조 문제
**문제**: 혼합 문자열 사용 (`"체력: {:HealthText}"`)
**해결**: MatrixFishingUI와 동일한 구조로 변경
- `<banner>` 태그 사용
- 순수 바인딩만 사용 (`{:HealthText}`)

## 최종 해결책

### ViewModel 구현
```csharp
public class PlayerInfoViewModel : INotifyPropertyChanged
{
    private int _health = 0;
    private int _energy = 0;
    
    public int Health 
    { 
        get => _health; 
        set 
        { 
            if (SetField(ref _health, value))
            {
                OnPropertyChanged(nameof(HealthText));
            }
        } 
    }
    
    public int Energy 
    { 
        get => _energy; 
        set 
        { 
            if (SetField(ref _energy, value))
            {
                OnPropertyChanged(nameof(EnergyText));
            }
        } 
    }
    
    public string HealthText => _health.ToString();
    public string EnergyText => _energy.ToString();
    
    // SetField, OnPropertyChanged 구현...
}
```

### StarML 구조
```xml
<lane orientation="vertical" horizontal-content-alignment="middle">
    <banner background={@Mods/StardewUI/Sprites/BannerBackground} 
            background-border-thickness="48,0" 
            padding="12" 
            text={:HeaderText} />
    <frame layout="400px content" 
           background={@Mods/StardewUI/Sprites/MenuBackground}
           border={@Mods/StardewUI/Sprites/MenuBorder}
           border-thickness="36, 36, 40, 36"
           margin="0,16,0,0"
           horizontal-content-alignment="middle" 
           vertical-content-alignment="middle">
        <lane orientation="vertical" horizontal-content-alignment="middle">
            <label text={:TestProperty} color="#FF00FF" margin="0,0,0,8" />
            <label text={:PlayerName} color="#4A9EFF" margin="0,0,0,8" />
            <label text={:HealthText} color="#4AFF4A" margin="0,0,0,8" />
            <label text={:EnergyText} color="#FFFF4A" margin="0,0,0,8" />
        </lane>
    </frame>
</lane>
```

## 핵심 원칙

1. **실전 예제 우선**: MatrixFishingUI, PenPals 등 실제 작동하는 모드 참고
2. **바인딩 문법**: `{:PropertyName}` (one-time 바인딩) 사용
3. **타입 변환**: `int` → `string` 변환 시 별도 프로퍼티 생성
4. **INotifyPropertyChanged**: 의존성 프로퍼티에 대한 이벤트 발생 필수
5. **StarML 구조**: MatrixFishingUI와 동일한 구조 사용

## 📅 추가 이슈 해결 기록

### 2. StardewUI 조건부 렌더링 문법 오류 (2024년 7월 25일)

#### 🚨 문제 상황
탭 시스템 구현 중 `*if` 디렉티브에서 비교 연산을 직접 사용할 때 오류 발생:
```
StardewUI.Framework.Descriptors.DescriptorException: Type PlayerInfoViewModel does not have a property named 'SelectedTab == "overview"'.
```

#### 🔍 원인 분석
- **StardewUI 제한사항**: `*if` 디렉티브는 직접적인 비교 연산(`==`, `!=` 등)을 지원하지 않음
- **잘못된 문법**: `*if={SelectedTab == "overview"}` (지원 안됨)
- **올바른 문법**: `*if={ShowOverviewTab}` (지원됨)

#### ✅ 해결 방법
1. **ViewModel에 Boolean 프로퍼티 추가**:
   ```csharp
   public bool ShowOverviewTab => SelectedTab == "overview";
   public bool ShowInventoryTab => SelectedTab == "inventory";
   public bool ShowSkillsTab => SelectedTab == "skills";
   public bool ShowSettingsTab => SelectedTab == "settings";
   ```

2. **OnTabActivated 메서드에서 PropertyChanged 이벤트 발생**:
   ```csharp
   OnPropertyChanged(nameof(ShowOverviewTab));
   OnPropertyChanged(nameof(ShowInventoryTab));
   OnPropertyChanged(nameof(ShowSkillsTab));
   OnPropertyChanged(nameof(ShowSettingsTab));
   ```

3. **StarML에서 Boolean 프로퍼티 사용**:
   ```xml
   <lane *if={ShowOverviewTab} ...>
   <lane *if={ShowInventoryTab} ...>
   ```

#### 💡 핵심 원칙
- **StardewUI 조건부 렌더링**: 직접적인 비교 연산 대신 ViewModel의 Boolean 프로퍼티 사용
- **PropertyChanged 알림**: 조건부 프로퍼티 변경 시 반드시 `OnPropertyChanged` 호출
- **예제 우선 참고**: StardewUI 공식 예제에서 `*if={IsEmpty}` 같은 패턴 확인

## 참고 자료
- MatrixFishingUI: `samples/MatrixFishingUI/MatrixFishingUI/Framework/Fish/FishMenuData.cs` (샘플 다운로드 필요)
- PenPals: 원본 저장소 참고 (현재 워크스페이스에 포함되지 않음)
- StardewUI 문서: `libraries/StardewUI/Docs/framework/binding-context.md`
- StardewUI 조건부 렌더링 예제: `libraries/StardewUI/TestMod/assets/views/Example-Tempering.sml`
