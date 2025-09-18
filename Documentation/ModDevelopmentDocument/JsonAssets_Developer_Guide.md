# Json Assets Developer Guide

Json Assets는 JSON 기반 콘텐츠 팩으로 새 아이템/작물/나무 등을 추가하는 프레임워크입니다. `ExternalLibraries/SpacechaseFrameworks/JsonAssets` 소스와 커뮤니티 모드(`ExampleMods/CommunityMods/Atravita-Collection/MoreFertilizers`) 사용 예제를 기준으로 작성했습니다.

## 1. 의존성 선언

`manifest.json`에 다음을 추가합니다.
```json
"Dependencies": [
  { "UniqueID": "spacechase0.JsonAssets", "IsRequired": true }
]
```

## 2. 콘텐츠 팩 구조

Json Assets는 SMAPI 콘텐츠 팩 형식을 사용합니다. 기본 레이아웃:
```
YourPack/
├─ content.json
├─ Objects/
│  └─ ExampleItem.json
├─ Crops/
│  └─ ExampleCrop.json
├─ BigCraftables/
│  └─ ExampleMachine.json
├─ ... (필요한 카테고리)
└─ i18n/
   └─ ko.json
```

- 각 JSON 모델은 `ExternalLibraries/SpacechaseFrameworks/JsonAssets/Data` 폴더에 정의된 클래스(`ObjectData`, `CropData`, `FruitTreeData` 등)의 필드를 따릅니다.
- 예: `ObjectData`는 `Description`, `Category`, `Recipe`, `PurchaseRequirements`, `ContextTags` 등을 지원합니다 (`Data/ObjectData.cs`).
- 다국어 문자열은 `NameLocalization` / `DescriptionLocalization` 또는 `TranslationKey`를 통해 처리됩니다.

## 3. 모드에서 Json Assets API 사용

### API 가져오기
```csharp
private JsonAssets.IApi? _jsonAssets;

public override void Entry(IModHelper helper)
{
    helper.Events.GameLoop.GameLaunched += (_, _) =>
    {
        _jsonAssets = helper.ModRegistry.GetApi<JsonAssets.IApi>("spacechase0.JsonAssets");
        _jsonAssets?.RegisterEvents(this.Monitor);
    };
}
```

실제 예시: `ExampleMods/CommunityMods/Atravita-Collection/MoreFertilizers/MoreFertilizers/ModEntry.cs:751-781` (버전 검사 포함).

### 주요 메서드
`ExternalLibraries/SpacechaseFrameworks/JsonAssets/IApi.cs` 참조:
- `LoadAssets(string path, ITranslationHelper translations = null)` : 런타임에 콘텐츠 팩 로드.
- `GetObjectId`, `GetCropId`, `GetFruitTreeId`, `GetBigCraftableId`, `GetHatId`, `GetWeaponId`, `GetClothingId`, `GetBootId` 등.
- `GetAllObjectIds()` 등으로 등록된 항목 전체 조회.
- 이벤트: `ItemsRegistered`, `IdsAssigned`, `AddedItemsToShop`.

`MoreFertilizers`는 `GetObjectId`를 통해 신규 비료의 내부 ID를 저장하고 (`ModEntry.cs:77-366`), 게임 로직/Harmony 패치에서 사용합니다.

## 4. 콘텐츠 팩 예제 (조각)

`Objects/ExampleItem.json`
```json
{
  "Name": "Example Tea",
  "Description": "Hand-crafted infusion.",
  "Category": "Cooking",
  "Price": 120,
  "Edibility": 40,
  "Recipe": {
    "ResultCount": 1,
    "Ingredients": [
      { "Object": "Tea Leaves", "Count": 1 },
      { "Object": "Sugar", "Count": 1 }
    ],
    "CraftingStation": "Kitchen"
  },
  "CanPurchase": true,
  "PurchaseFrom": "Pierre",
  "PurchaseRequirements": [ "w 4", "y 2" ],
  "ContextTags": [ "drink", "ja.exampletea" ]
}
```
위 필드명 및 자료형은 `ObjectData` 클래스에서 확인 가능합니다 (`ExternalLibraries/.../Data/ObjectData.cs`).

## 5. 이벤트 흐름

- **ItemsRegistered**: `LoadAssets`가 끝난 직후 발생. 다른 모드가 새 항목을 가져오기 전에 초기화 로직을 수행 가능.
- **IdsAssigned**: 게임이 실제 ID(정수)를 할당한 후 발생. Vanilla 데이터와 상호작용할 때 이 시점 이후에 ID를 사용하세요.
- **AddedItemsToShop**: 상점 재구성 단계 이후 호출. 커스텀 판매 조건 조정에 사용.

## 6. 개발 체크리스트

1. 콘텐츠 팩 구조 생성 및 JSON 작성 (`Data/*.cs` 필드 참조).  
2. 모드에서 `IApi`를 가져와 `LoadAssets` 호출 또는 `ContentPackFor` 메타데이터로 자동 로딩.  
3. 신규 아이템 ID를 속성으로 저장 (`GetObjectId`).  
4. 게임 로직/Harmony 패치에서 해당 ID 사용.  
5. `ItemsRegistered`/`IdsAssigned` 이벤트를 통해 의존 순서 보장.

## 7. 참조 코드

- API 구현: `ExternalLibraries/SpacechaseFrameworks/JsonAssets/Framework/Api.cs`
- 데이터 모델: `ExternalLibraries/SpacechaseFrameworks/JsonAssets/Data/`
- 실전 사용: `ExampleMods/CommunityMods/Atravita-Collection/MoreFertilizers/MoreFertilizers/ModEntry.cs`

Json Assets를 활용하면 별도의 C# 로직 없이 JSON 정의로 대규모 콘텐츠를 추가할 수 있으며, 필요 시 API를 통해 게임 코드와 직접 연동할 수 있습니다.
