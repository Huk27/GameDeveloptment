# StardewUI 데이터 바인딩 가이드

## 데이터 바인딩 기본 개념

StardewUI는 Model-View-Whatever 아키텍처를 사용하여 뷰와 데이터를 분리합니다.

### 1. ViewModel 패턴
- ViewModel은 `INotifyPropertyChanged` 인터페이스를 구현해야 함
- 프로퍼티 변경 시 UI가 자동으로 업데이트됨

### 2. PropertyChanged.SourceGenerator 사용
```csharp
using PropertyChanged.SourceGenerator;

public partial class MyViewModel : INotifyPropertyChanged
{
    [Notify]
    private string playerName = "";
    
    [Notify]
    private int health = 0;
}
```

### 3. StarML에서 데이터 바인딩
```xml
<label text="{PlayerName}" />
<label text="체력: {Health}" />
<label text="에너지: {Energy}" />
```

## 데이터 바인딩 문제 해결

### 1. PropertyChanged.SourceGenerator 설정
- `.csproj` 파일에 패키지 추가:
```xml
<PackageReference Include="PropertyChanged.SourceGenerator" Version="1.1.1">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

### 2. ViewModel 구현
- `partial class`로 선언
- `[Notify]` 어트리뷰트 사용
- 필드명을 프로퍼티명과 일치시키기

### 3. 데이터 업데이트
```csharp
public void UpdateData()
{
    if (Game1.player != null)
    {
        playerName = Game1.player.Name;
        health = Game1.player.health;
        energy = (int)Game1.player.Stamina;
    }
}
```

## 일반적인 문제들

### 1. 데이터 바인딩이 작동하지 않는 경우
- ViewModel이 `INotifyPropertyChanged`를 구현했는지 확인
- `PropertyChanged.SourceGenerator`가 올바르게 설정되었는지 확인
- 필드명과 프로퍼티명이 일치하는지 확인

### 2. 빌드 오류
- `partial class`로 선언했는지 확인
- `[Notify]` 어트리뷰트를 올바르게 사용했는지 확인

### 3. 런타임 오류
- ViewModel이 올바르게 초기화되었는지 확인
- 데이터 업데이트 메서드가 올바르게 호출되는지 확인
