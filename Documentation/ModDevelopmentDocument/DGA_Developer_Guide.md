# Dynamic Game Assets (DGA) Integration Guide

본 레포지토리는 DGA 원본 코드를 포함하고 있지 않지만, `ExampleMods/CommunityMods/Atravita-Collection/MoreFertilizers`에서 제공하는 호환성 패턴과 공용 API 규약을 기반으로 DGA와 연동하는 방법을 정리합니다.

## 1. 모듈 존재 여부 확인

- SMAPI 모드 목록에서 DGA를 확인하려면 `Helper.ModRegistry.Get("spacechase0.DynamicGameAssets")`를 사용합니다 (`MoreFertilizers/ModEntry.cs:676`).
- 버전 조건이 필요한 경우 `IModInfo.Manifest.Version`으로 비교합니다. DGA 1.4.1 이상에서만 특정 호환 패치를 적용하도록 구성되어 있습니다.

```csharp
if (this.Helper.ModRegistry.Get("spacechase0.DynamicGameAssets") is IModInfo dga
    && dga.Manifest.Version.IsNewerThan("1.4.1"))
{
    // DGA 전용 패치 적용
}
```

## 2. DGA 타입에 대한 리플렉션 접근

DGA는 런타임에 커스텀 타입을 생성하므로 직접 참조하기보다는 `HarmonyLib.AccessTools.TypeByName`을 사용하는 것이 일반적입니다. `MoreFertilizers`는 다음과 같은 타입을 활용합니다.

| 대상 | 리플렉션 문자열 | 사용 위치 |
| --- | --- | --- |
| 커스텀 작물 | `DynamicGameAssets.Game.CustomCrop` | `HarmonyPatches/CropNewDayTranspiler.cs:26`, `CropHarvestTranspiler.cs:53`
| 커스텀 과일나무 | `DynamicGameAssets.Game.CustomFruitTree` | `FruitTreeDayUpdateTranspiler.cs:28`, `FruitTreeDrawTranspiler.cs:30`
| 커스텀 오브젝트 | `DynamicGameAssets.Game.CustomObject` | `HarmonyPatches/SObjectPatches.cs:23`
| 팩 데이터 | `DynamicGameAssets.PackData.CropPackData` | `CropHarvestTranspiler.cs:436`
| 아이템 추상화 | `DynamicGameAssets.ItemAbstraction` | `CropHarvestTranspiler.cs:476`

리플렉션을 사용할 때는 `Type? dgaType = AccessTools.TypeByName("..."); if (dgaType is null) return;` 형태로 안전하게 체크하십시오.

## 3. 콘텐츠 팩 메타데이터

DGA 콘텐츠 팩의 `manifest.json`에는 다음과 같은 메타데이터가 포함됩니다.
```json
{
  "ContentPackFor": {
    "UniqueID": "spacechase0.DynamicGameAssets"
  }
}
```
이는 `Documentation/ModDevelopmentDocument/DGA_Developer_Guide.md` 기존 버전에서 사용하던 형식을 유지합니다. DGA는 콘텐츠 폴더 내 `content.json` 파일을 읽어 커스텀 아이템/작물을 등록합니다.

## 4. API 사용 시 주의 사항

- 공식 API 인터페이스(`IDynamicGameAssetsApi`)는 DGA 모드에서 제공됩니다. 이 저장소에는 정의가 없으므로, DGA가 설치된 환경에서 `Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets")` 형태로 가져와야 합니다.
- 일반적으로 다음 기능을 제공합니다.
  - 콘텐츠 팩 수동 로드 (`LoadContentPack`)
  - 아이템/작물 ID 조회 (`GetItemId`, `GetCropId` 등)
  - `AboutToRegister`, `Registered`와 같은 이벤트

API 사용 전, null 체크 및 버전 비교를 수행하여 하위 버전 호환성을 유지하십시오.

## 5. 호환성 패턴 요약 (MoreFertilizers 기준)

1. `ApplyPatches` 단계에서 DGA 설치 여부와 버전을 확인 (`ModEntry.cs:676-688`).
2. DGA가 존재할 경우 Harmony 트랜스파일러를 추가 적용해 DGA 커스텀 타입까지 비료 효과가 작동하도록 확장 (`FruitTreeDayUpdateTranspiler.ApplyDGAPatch`, 등).
3. 특정 기능이 DGA 타입에 접근해야 한다면, 리플렉션으로 메서드/필드를 가져온 뒤 `DynamicMethod` 또는 `AccessTools.Property` 등을 이용해 조작.

## 6. 개발 체크리스트

- [ ] `Helper.ModRegistry`로 DGA 설치 여부 확인
- [ ] 대상 버전 요구 사항 정의 (`IsNewerThan` / `IsOlderThan`)
- [ ] Harmony 패치에서 DGA 커스텀 타입을 리플렉션으로 처리
- [ ] (선택) `GetApi<IDynamicGameAssetsApi>`를 통해 공식 API 사용
- [ ] DGA 미설치 환경에서도 정상 동작하도록 조건 분기

DGA 소스가 포함되어 있지 않더라도 위 패턴을 따라가면, DGA 사용자와의 호환성을 안정적으로 유지할 수 있습니다.
