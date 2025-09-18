# StardewUI Developer Guide

StardewUI는 `ExternalLibraries/StardewUI` 아래에 포함된 MVVM 기반 UI 프레임워크입니다. 본 문서는 라이브러리 소스와 `ExampleMods/MatrixFishingUI` 실전 예제를 바탕으로, 새로운 모드에서 StardewUI를 사용하는 방법을 정리합니다.

## 1. 종속성 선언

`manifest.json`에 다음 항목을 추가합니다.
```json
"Dependencies": [
  { "UniqueID": "focustense.StardewUI", "IsRequired": true }
]
```

## 2. ViewEngine 획득 및 초기화

1. **OnGameLaunched에서 API 요청**  
   ```csharp
   ViewEngine = Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI");
   ViewEngine?.RegisterViews("Mods/YourMod/Views", "assets/views");
   ViewEngine?.RegisterSprites("Mods/YourMod/Sprites", "assets/sprites");
   ```
   (`ExampleMods/MatrixFishingUI/MatrixFishingUI/ModEntry.cs:243-252`)
2. `RegisterViews`는 뷰 자산 접두사와 실제 폴더를 연결합니다. 자산 경로는 SML 파일에서 `<image sprite={@Mods/...}>`와 같이 사용됩니다.
3. `IViewEngine`의 핵심 기능은 `ExternalLibraries/StardewUI/Framework/Api/IViewEngine.cs`에서 확인할 수 있습니다. 주요 메서드:
   - `CreateMenuFromAsset(assetName, context)`
   - `CreateDrawableFromAsset(assetName)`
   - `RegisterViews`, `RegisterSprites`, `EnableHotReloading`

## 3. SML(View) 파일 작성 규칙

- 뷰 정의는 `assets/views/*.sml`과 같이 저장합니다. 예: `ExampleMods/MatrixFishingUI/MatrixFishingUI/assets/views/Hud.sml`.
- 주요 위젯 태그는 `ExternalLibraries/StardewUI/Core/Widgets` 폴더의 클래스(`Lane.cs`, `Grid.cs`, `Image.cs`, `Label.cs` 등)에 대응합니다.
- **데이터 바인딩**: `{ :PropertyName }` 형태로 컨텍스트 객체의 속성을 참조합니다. 조건부 렌더링은 `*if`, `*!if`, `*switch`, `*repeat` 등의 디렉티브(구현은 `ExternalLibraries/StardewUI/Framework/Binding` 계층)로 처리합니다.
- **리소스 참조**: `@Mods/<ModId>/Sprites/...` 형식은 `RegisterSprites`에서 매핑한 자산을 로드합니다.

## 4. ViewModel 구성

- SML 뷰는 `Context` 객체를 통해 데이터를 전달받습니다.  
  `MatrixFishingUI`에서는 `HudMenuData` 같은 POCO 클래스를 사용하여 `IViewDrawable.Context`에 주입 (`ExampleMods/MatrixFishingUI/MatrixFishingUI/ModEntry.cs:205-210`).
- 프로퍼티 변경 시 UI를 갱신하려면 `INotifyPropertyChanged` 또는 `StardewUI.Framework.Binding.ObservableObject` 파생 클래스를 사용하십시오 (`ExternalLibraries/StardewUI/Framework/Binding/ObservableObject.cs`).

## 5. 메뉴 & HUD 표시

- 메뉴 열기: `ViewEngine.CreateMenuFromAsset("Mods/YourMod/Views/MainMenu", context)` 후 `Game1.activeClickableMenu`에 설정.
- HUD 드로어: `CreateDrawableFromAsset`으로 `IViewDrawable`을 얻은 뒤 `RenderedHud` 이벤트에서 `Draw` 호출 (`ExampleMods/MatrixFishingUI/MatrixFishingUI/ModEntry.cs:201-210`).
- 자식 메뉴 관리: `MatrixFishingUI/Framework/Fish/ViewEngine.cs`에서 `OpenChildMenu`, `ChangeChildMenu` 구현 참고.

## 6. 스타일 & 애니메이션

- 스타일 자산은 `ExternalLibraries/StardewUI/Framework/assets/stylesheets` 아래 정의되어 있으며, `<frame class="window">`처럼 `class`를 지정하면 적용됩니다.
- 애니메이션/전환은 `ExternalLibraries/StardewUI/Core/Animation` 네임스페이스와 `Storyboard` 시스템으로 구현 가능합니다.

## 7. 핫 리로드

- 개발 중 `ViewEngine.EnableHotReloading("Mods/YourMod/Views")`를 호출하면 SML/Sprite 수정 시 자동 업데이트가 가능합니다 (API 인터페이스 주석 참조).
- VS Code 확장 등 도구는 `ExternalLibraries/StardewUI/Tools`에 포함되어 있습니다.

## 8. 트러블슈팅

- Binding 오류는 `ExternalLibraries/StardewUI/Core/Diagnostics/BindingTraceListener.cs`에서 로그로 출력되므로 SMAPI 콘솔을 확인하세요.
- 자산 경로가 틀린 경우 `AssetRequested` 단계에서 오류가 발생합니다. `RegisterViews`/`RegisterSprites` 호출 순서를 확인하십시오.
- 컨텍스트가 null이면 `InvalidOperationException("ViewEngine Instance is not set up!!!")`가 발생 (`MatrixFishingUI/.../Framework/Fish/ViewEngine.cs:14-18`).

## 9. 샘플 워크플로

1. `OnGameLaunched`에서 `IViewEngine` 확보 및 폴더 등록.  
2. SML 뷰 작성 (`assets/views/*.sml`).  
3. 뷰 모델 클래스 작성 후 필요한 이벤트에서 `Context` 업데이트.  
4. HUD나 메뉴를 `CreateDrawableFromAsset` / `CreateMenuFromAsset`으로 생성.  
5. 변경 사항 테스트 → 필요 시 핫 리로드 활성화.

StardewUI는 복잡하지만, 위 구조를 따라가면 `MatrixFishingUI`와 유사한 현대적인 UI를 빠르게 구축할 수 있습니다.
