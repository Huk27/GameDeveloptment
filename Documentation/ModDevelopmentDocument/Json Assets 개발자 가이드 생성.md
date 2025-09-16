

# **Json Assets 최종 개발자 가이드: 첫 아이템부터 완전한 콘텐츠 팩까지**

## **서론: 스타듀밸리 커스텀 아이템의 관문**

### **Json Assets란 무엇인가?**

Json Assets(이하 JA)는 스타듀밸리 모딩 프레임워크로, 모드 개발자가 C\# 코드를 작성하지 않고도 간단한 JSON 파일을 사용하여 게임에 새로운 커스텀 콘텐츠를 추가할 수 있게 해주는 SMAPI 모드입니다.1 JA는 제작법, 작물(거대 작물 포함), 과일나무, 거대 설치물(Big Craftables), 모자, 무기, 의류, 신발 등 다양한 종류의 아이템 추가를 지원합니다.2 이는 게임의 원본 파일을 직접 교체하여 잠재적인 충돌과 데이터 손상을 유발할 수 있는 구시대적인 XNB 모딩 방식과는 근본적으로 다른, 안전하고 현대적인 접근법입니다.3

### **모딩 생태계: JA 대 Content Patcher**

스타듀밸리 모딩 생태계에서 JA와 Content Patcher(이하 CP)는 가장 핵심적인 두 프레임워크입니다. 이 둘의 핵심적인 기능적 차이를 이해하는 것이 중요합니다. Json Assets는 고유한 ID를 가진 **새로운 아이템을 게임에 추가**하는 데 특화되어 있습니다. 반면, Content Patcher는 주로 기존 게임 에셋(이미지, 데이터, 대화 등)을 **수정하거나 덮어쓰는** 역할을 합니다.2

이러한 차이점 때문에 두 프레임워크는 경쟁 관계가 아닌, 상호 보완적인 관계에 있습니다. JA로 새로운 아이템을 추가한 뒤, CP를 사용하여 해당 아이템을 상점에 추가하거나, NPC의 선물 취향에 반영하는 등 게임 세계에 깊숙이 통합하는 것이 일반적인 모딩 패턴입니다. 본 가이드의 후반부에서는 이 두 프레임워크를 함께 사용하는 강력한 기술을 심도 있게 다룰 것입니다.

### **1.6 업데이트 이후의 Json Assets**

스타듀밸리 1.6 업데이트는 CP를 통해 일부 커스텀 아이템을 추가할 수 있는 네이티브 기능을 도입했습니다. 이로 인해 JA가 '구식' 또는 '더 이상 사용되지 않는' 프레임워크라는 인식이 생겨났습니다.6 하지만 이는 JA의 역할이 축소되었다기보다는 재정의되었음을 의미합니다.

JA는 여전히 다음과 같은 중요한 이유로 모딩 커뮤니티에서 필수적인 위치를 차지하고 있습니다:

1. **구조화된 접근 방식:** JA는 각 아이템 유형에 맞는 명확하고 체계적인 JSON 구조를 제공하여 복잡한 아이템도 쉽게 정의할 수 있게 합니다.  
2. **광범위한 호환성:** 'Project Populate Json Assets'(PPJA)와 같은 수많은 대형 모드들이 JA를 기반으로 제작되었습니다.7 이러한 모드들을 사용하거나, 이들과 상호작용하는 모드를 만들기 위해서는 JA가 반드시 필요합니다.  
3. **동적 ID 할당:** JA의 가장 근본적인 가치는 아이템 ID를 동적으로 할당하여 모드 간의 충돌을 원천적으로 방지하는 데 있습니다.4 초기 스타듀밸리 모딩의 가장 큰 골칫거리였던 ID 충돌 문제를 JA가 해결함으로써, 수많은 모드가 공존하는 현대적인 모딩 환경의 기틀을 마련했습니다.

따라서 1.6 이후의 환경에서 JA를 배우는 것은 단순히 새로운 아이템을 만드는 기술을 넘어, 스타듀밸리 모딩의 방대한 유산과 상호작용하고 그 위에 새로운 것을 구축하는 방법을 배우는 것과 같습니다.

---

## **챕터 1: 기초와 첫걸음: 나의 첫 콘텐츠 팩**

### **1.1. 필수 도구 및 설정**

JA를 사용한 모드 개발을 시작하기 전에, 몇 가지 필수 도구를 준비해야 합니다.

* **SMAPI:** 스타듀밸리 모드의 기반이 되는 모드 로더입니다. 반드시 최신 버전으로 설치해야 합니다.9  
* **Json Assets:** 본 가이드의 핵심인 프레임워크 모드입니다. SMAPI와 마찬가지로 최신 버전을 설치합니다.1  
* **텍스트 편집기:** JSON 파일을 편집하기 위한 프로그램이 필요합니다. 메모장도 가능하지만, 코드 하이라이팅과 같은 편의 기능을 제공하는 Visual Studio Code(VS Code)나 Notepad++ 사용을 강력히 권장합니다.2  
* **(권장) JSON 스키마 설정:** VS Code를 사용한다면, 스타듀밸리 모딩용 JSON 스키마를 설정하는 것이 좋습니다. 스키마는 JSON 파일 작성 시 실시간으로 오류를 검사하고 자동 완성을 제공하여 개발 효율을 극적으로 높여줍니다.2 이는 JSON 스키마의 일반적인 활용법 중 하나입니다.11

### **1.2. 콘텐츠 팩의 구조**

모든 JA 콘텐츠 팩은 정해진 기본 구조를 따릅니다. 이 구조를 이해하는 것이 첫걸음입니다.

#### **폴더 구조**

커뮤니티에서 정립된 표준 폴더 구조는 다음과 같습니다 13:

Stardew Valley/Mods/  
└── \[JA\] MyFirstMod/  
    ├── manifest.json  
    └── Objects/  
        └── Geode Shard/  
            ├── object.json  
            └── object.png

* \[JA\] MyFirstMod/: 모드의 루트 폴더입니다. \[JA\] 접두사는 이 팩이 Json Assets를 사용함을 나타내는 커뮤니티 관례입니다.13  
* manifest.json: 모드의 '신분증' 역할을 하는 가장 중요한 파일입니다.  
* Objects/: 추가하려는 아이템의 카테고리에 해당하는 폴더입니다. 작물은 Crops, 거대 설치물은 BigCraftables 등으로 명명합니다.  
* Geode Shard/: 추가하려는 특정 아이템의 이름을 딴 폴더입니다.

이러한 계층적 폴더 구조는 JA 프레임워크의 엄격한 요구사항은 아니지만, PPJA와 같은 대규모 프로젝트에서 수백 개의 아이템을 체계적으로 관리하기 위해 자연스럽게 형성된 사실상의 표준입니다.14 새로운 개발자는 처음부터 이 관례를 따르는 것이 좋습니다.

#### **manifest.json 분석**

manifest.json 파일은 SMAPI에게 모드에 대한 기본 정보를 알려줍니다. 모든 콘텐츠 팩은 이 파일을 반드시 포함해야 합니다.9

JSON

{  
  "Name": "My First Mod",  
  "Author": "Your Name",  
  "Version": "1.0.0",  
  "Description": "Adds a simple Geode Shard item to the game.",  
  "UniqueID": "YourName.MyFirstMod",  
  "ContentPackFor": {  
    "UniqueID": "spacechase0.JsonAssets"  
  }  
}

* Name: 모드의 이름입니다.  
* Author: 제작자의 이름입니다.  
* Version: 모드의 버전입니다. SMAPI는 이 정보를 사용하여 업데이트를 확인합니다.  
* Description: 모드에 대한 간략한 설명입니다.  
* UniqueID: 다른 모드와 충돌하지 않는 고유한 ID입니다. Author.ModName 형식을 사용하는 것이 일반적입니다.  
* ContentPackFor: 이 콘텐츠 팩이 어떤 프레임워크를 필요로 하는지 명시하는 가장 중요한 부분입니다. JA 콘텐츠 팩의 경우, UniqueID를 spacechase0.JsonAssets로 반드시 지정해야 합니다.

JSON 문법에 익숙하지 않다면, 작성한 manifest.json 파일을 SMAPI 공식 JSON 검사기([https://smapi.io/json](https://smapi.io/json))에 업로드하여 문법 오류가 없는지 확인하는 습관을 들이는 것이 좋습니다.9

### **1.3. 첫 아이템 만들기: "정동석 파편"**

이제 이론을 바탕으로 실제 아이템을 만들어 보겠습니다. JA 모딩의 "Hello, World\!" 과정입니다.

1. **폴더 생성:** Stardew Valley/Mods 폴더 안에 \[JA\] MyFirstMod라는 이름의 새 폴더를 만듭니다. 그 안에 Objects 폴더를, 또 그 안에 Geode Shard 폴더를 만듭니다.  
2. **스프라이트 제작:** 16x16 픽셀 크기의 투명 배경 .png 이미지를 만듭니다. 간단한 보석 조각처럼 보이게 그리고, object.png라는 이름으로 Geode Shard 폴더에 저장합니다.  
3. **JSON 파일 작성:** Geode Shard 폴더에 object.json이라는 새 텍스트 파일을 만들고 다음 내용을 입력합니다.  
   JSON  
   {  
     "Name": "Geode Shard",  
     "Description": "A small, sharp fragment from a broken geode.",  
     "Category": "Junk",  
     "Price": 5,  
     "Edibility": \-300  
   }

   * Name: 아이템의 내부 이름입니다.  
   * Description: 게임 내에서 마우스를 올렸을 때 표시될 설명입니다.  
   * Category: 아이템의 분류입니다. 여기서는 "Junk"로 설정했습니다.  
   * Price: 기본 판매 가격입니다.  
   * Edibility: 먹었을 때 회복되는 에너지 양입니다. 먹을 수 없는 아이템은 관례적으로 \-300으로 설정합니다.  
4. **테스트:** 스타듀밸리를 실행합니다. 게임 내에서 아이템을 확인하기 위해 CJB Item Spawner와 같은 디버그용 모드를 사용하여 "Geode Shard"를 검색합니다. 아이템이 정상적으로 나타난다면 첫 번째 JA 콘텐츠 팩 제작에 성공한 것입니다.

---

## **챕터 2: 에셋 유형 종합 카탈로그**

이 챕터는 JA가 지원하는 모든 에셋 유형을 상세히 다룹니다. 각 섹션은 **개요**, **JSON 속성 참조 표**, 그리고 유명 모드에서 가져온 **주석이 달린 실전 예제** 순서로 구성됩니다.

### **2.1. 오브젝트 (제작품, 음식 등)**

**개요:** 가장 기본적이고 흔하게 사용되는 에셋 유형입니다. 간단한 재료부터 복잡한 요리까지, 플레이어가 인벤토리에 소지할 수 있는 대부분의 아이템이 여기에 해당합니다.

**표 2.1: object.json 속성**

| 속성 | 타입 | 필수 여부 | 설명 |
| :---- | :---- | :---- | :---- |
| Name | string | 예 | 아이템의 내부 이름. 코드 및 다른 JSON 파일에서 이 아이템을 참조할 때 사용됩니다. |
| Description | string | 예 | 게임 내에서 아이템 위에 마우스를 올렸을 때 표시되는 기본 설명입니다. |
| Category | string | 예 | 아이템의 분류 ("Vegetable", "Flower", "Crafting", "Junk" 등). |
| Price | integer | 예 | 아이템의 기본 판매 가격입니다. |
| Edibility | integer | 아니요 | 아이템을 섭취했을 때 회복되는 에너지의 양. 먹을 수 없는 아이템은 \-300으로 설정합니다. |
| IsDrink | boolean | 아니요 | 아이템이 음료수인지 여부를 설정합니다. true로 설정하면 마시는 애니메이션이 재생됩니다. 기본값은 false입니다. |
| Buffs | object | 아니요 | 섭취 시 적용될 버프 효과를 정의하는 객체의 배열입니다. |
| Recipe | object | 아니요 | 이 아이템을 제작하는 방법을 정의하는 중첩된 객체입니다. |
| GiftTastes | object | 아니요 | 이 아이템을 선물로 받았을 때 NPC들이 보이는 반응을 정의합니다. |

#### **실전 예제: '\[JA\] Fruits and Veggies'의 "쌀" 분해하기**

PPJA 모드 팩의 핵심인 '\[JA\] Fruits and Veggies'에 포함된 "쌀(Rice)" 아이템은 오브젝트의 다양한 속성을 잘 보여주는 훌륭한 예제입니다.14

* **파일 구조:** \[JA\] Fruits and Veggies/Objects/Rice/ 폴더에는 object.json과 object.png 파일이 위치합니다.  
* **주석이 달린 object.json:**  
  JSON  
  {  
    "Name": "Rice", // 내부 이름. 레시피 등에서 이 이름으로 참조됩니다.  
    "Description": "A basic grain, often served with other dishes.", // 게임 내 설명 텍스트.  
    "Category": "Vegetable", // 분류. 상점 탭이나 기계 상호작용에 영향을 줍니다.  
    "Edibility": 10, // 약간의 에너지를 회복합니다. 먹을 수 없으면 \-300.  
    "Price": 35, // 기본 판매 가격.  
    "IsDrink": false, // 음식 아이템이므로 false여야 합니다.  
    "Recipe": { // 이 아이템의 제작법을 정의하는 부분입니다.  
      "ResultCount": 1, // 한 번 제작 시 생산되는 개수.  
      "Ingredients":,  
      "CanPurchase": true, // 플레이어가 이 레시피를 구매할 수 있는지 여부.  
      "PurchaseFrom": "Pierre", // 판매하는 NPC.  
      "PurchasePrice": 500, // 구매 가격.  
      "PurchaseRequirements": \[ "y 2" \] // 구매 조건. "y 2"는 게임 내 2년차 이상을 의미합니다.  
    },  
    "GiftTastes": { // NPC들의 선물 반응을 정의합니다.  
        "Love": \["Gus"\],  
        "Like":,  
        "Dislike":  
    }  
  }

* **에셋 분석:** object.png 파일은 이 아이템의 인게임 아이콘으로 사용되는 16x16 픽셀 크기의 이미지입니다.

### **2.2. 작물**

**개요:** 농장에서 재배할 수 있는 새로운 식물을 추가할 때 사용됩니다. 작물 자체의 속성과 씨앗의 속성을 함께 정의합니다.

**표 2.2: crop.json 속성**

| 속성 | 타입 | 필수 여부 | 설명 |
| :---- | :---- | :---- | :---- |
| Name | string | 예 | 작물의 내부 이름. |
| Product | string | 예 | 수확 시 얻게 되는 object.json 아이템의 Name. |
| SeedName | string | 예 | 이 작물을 심는 씨앗 아이템의 이름. |
| SeedDescription | string | 예 | 씨앗 아이템의 설명. |
| Type | string | 예 | 작물의 종류 ("Flower", "Fruit", "Vegetable" 등). 벌통에 영향을 줍니다. |
| Seasons | string | 예 | 작물이 자랄 수 있는 계절의 배열 ("spring", "summer", "fall", "winter"). |
| Phases | integer | 예 | 각 성장 단계에 걸리는 일수의 배열. 총 성장 기간은 이 배열의 합입니다. |
| RegrowthPhase | integer | 아니요 | 재성장 시 돌아갈 성장 단계의 인덱스. 재성장하지 않는 경우 \-1로 설정합니다. |
| HarvestWithScythe | boolean | 아니요 | 수확 시 낫이 필요한지 여부. 기본값은 false입니다. |
| TrellisCrop | boolean | 아니요 | 플레이어의 이동을 막는 격자 작물인지 여부. 기본값은 false입니다. |
| Colors | string | 아니요 | (꽃 전용) 꽃이 가질 수 있는 색상의 배열. RGB 값을 문자열로 지정합니다. |
| Bonus | object | 아니요 | 추가 수확량에 대한 확률을 정의하는 중첩된 객체입니다. |
| SeedPurchasePrice | integer | 예 | 씨앗의 구매 가격. |
| SeedPurchaseFrom | string | 예 | 씨앗을 판매하는 NPC 또는 상점 이름. |

#### **실전 예제: '\[JA\] Mizu's Flowers'의 "페튜니아" 심기**

PPJA의 인기 모드 중 하나인 '\[JA\] Mizu's Flowers'는 아름다운 커스텀 꽃들을 추가하며, crop.json의 좋은 예시를 제공합니다.17

* **파일 구조:** \[JA\] Mizus Flowers/Crops/Petunia/ 폴더에는 crop.json, crop.png (성장 단계 스프라이트 시트), seeds.png (씨앗 아이콘) 파일이 있습니다.  
* **주석이 달린 crop.json:**  
  JSON  
  {  
    "Name": "Petunia",  
    "Product": "Petunia", // 수확 시 'Petunia'라는 이름의 오브젝트를 생산합니다.  
    "SeedName": "Petunia Seeds", // 씨앗 아이템의 이름.  
    "SeedDescription": "Plant these in the spring. Takes 6 days to mature.", // 씨앗 설명.  
    "Type": "Flower", // 작물 유형. 벌통이 이 꽃으로 특별한 꿀을 만듭니다.  
    "Seasons": \[ "spring" \], // 봄에만 자랍니다.  
    "Phases": \[ 1, 2, 2, 1 \], // 성장 단계별 소요 일수. 총 1+2+2+1 \= 6일 소요.  
    "RegrowthPhase": \-1, // \-1은 재성장하지 않음을 의미합니다.  
    "HarvestWithScythe": false, // 낫이 필요 없습니다.  
    "TrellisCrop": false, // 격자 작물이 아닙니다.  
    "Colors": \[ "255 180 255", "255 100 255", "200 0 200" \], // 꽃이 필 때 이 색상들 중 하나를 무작위로 가집니다.  
    "Bonus": { // 추가 수확량 설정.  
      "MinimumPerHarvest": 1,  
      "MaximumPerHarvest": 1,  
      "MaxIncreasePerFarmLevel": 0,  
      "ExtraChance": 0.0  
    },  
    "SeedPurchasePrice": 40, // 씨앗 구매 가격.  
    "SeedPurchaseFrom": "Pierre" // 피에르 상점에서 판매.  
  }

* **에셋 분석:** crop.png는 작물의 각 성장 단계를 순서대로 나열한 스프라이트 시트이며, seeds.png는 씨앗 아이템의 16x16 픽셀 아이콘입니다. JA는 이 두 이미지를 사용하여 게임 내에서 작물과 씨앗을 렌더링합니다. 이 구조에서 crop.json의 "Product": "Petunia"는 JA가 /Objects/Petunia/object.json 파일을 찾아 수확물을 정의하도록 지시하는, 이름 기반의 관계를 형성합니다. 이는 JA 프레임워크의 핵심적인 작동 원리로, 개발자는 이 관계를 유지하기 위해 이름의 일관성을 반드시 지켜야 합니다.

### **2.3. 거대 설치물 (Big Craftables)**

**개요:** 가구나 기계처럼 맵에 배치할 수 있는 1x2 또는 1x3 타일 크기의 대형 아이템을 추가할 때 사용됩니다.

**표 2.3: big-craftable.json 속성**

| 속성 | 타입 | 필수 여부 | 설명 |
| :---- | :---- | :---- | :---- |
| Name | string | 예 | 거대 설치물의 내부 이름. |
| Description | string | 예 | 게임 내 설명. |
| Price | integer | 예 | 기본 판매 가격. |
| Fragility | integer | 아니요 | 내구성. 기본값은 0 (일반)입니다. |
| IsLamp | boolean | 아니요 | 밤에 빛을 내는지 여부. 기본값은 false입니다. |
| Recipe | object | 아니요 | 이 아이템을 제작하는 방법을 정의하는 중첩된 객체입니다. |

#### **실전 예제: '\[JA\] Artisan Valley'의 "분쇄기" 제작하기**

PPJA의 확장팩인 '\[JA\] Artisan Valley'는 수많은 커스텀 기계를 추가하며, 거대 설치물의 좋은 예시를 제공합니다.18

* **파일 구조:** \[JA\] Artisan Valley Machine Goods/BigCraftables/Grinder/ 폴더에는 big-craftable.json과 big-craftable.png가 있습니다.  
* **주석이 달린 big-craftable.json:**  
  JSON  
  {  
    "Name": "Grinder",  
    "Description": "A machine that grinds down certain crops into a more refined product.",  
    "Price": 2500,  
    "Fragility": 0, // 일반적인 내구성.  
    "IsLamp": false, // 빛을 내지 않습니다.  
    "Recipe": {  
      "ResultCount": 1,  
      "Ingredients":,  
      "IsDefault": false, // 레시피를 배워야 제작 가능.  
      "CanPurchase": true,  
      "PurchaseFrom": "Robin", // 로빈에게서 구매.  
      "PurchasePrice": 5000,  
      "SkillUnlockName": "Farming", // 해금에 필요한 스킬.  
      "SkillUnlockLevel": 7 // 해금에 필요한 스킬 레벨.  
    }  
  }

* **에셋 분석:** big-craftable.png 파일은 이 기계의 인게임 외형을 나타내는 이미지입니다. 표준 1x2 타일 크기 기계의 경우, 이미지 크기는 16x32 픽셀이어야 합니다.

### **2.4. 무기, 모자, 의류, 신발**

이러한 착용 가능 아이템들도 각각 고유한 JSON 파일을 통해 추가할 수 있습니다.

* **무기 (weapon.json):** Type("Dagger", "Club", "Sword"), MinimumDamage, MaximumDamage, Knockback, Speed, CritChance 등 무기의 전투 성능과 관련된 고유한 속성들을 정의합니다.  
* **모자 (hat.json):** CanBeDyed, ShowHair와 같은 외형 관련 옵션을 포함합니다.  
* **의류 (shirt.json, pants.json):** 의류 아이템은 남성 및 여성 농부 캐릭터의 스프라이트 시트에 맞춰 별도의 이미지가 필요하며, JSON 파일에서 이를 연결합니다.  
* **신발 (boots.json):** Defense, Immunity와 같은 방어 관련 능력치를 추가할 수 있습니다.

각 유형에 대한 상세한 속성 목록은 부록의 참조 표에서 확인할 수 있습니다.

---

## **챕터 3: 고급 기술 및 생태계 통합**

### **3.1. 국제화 (i18n)**

모드를 여러 언어로 제공하기 위해 국제화(internationalization, i18n) 기능을 사용할 수 있습니다. 이는 모드 폴더에 i18n 폴더를 만들고 언어별 번역 파일을 추가하는 방식으로 작동합니다.20

* **구조:** i18n 폴더 안에 default.json 파일을 만듭니다. 이 파일은 다른 언어 번역이 없을 때 사용될 기본 텍스트(주로 영어)를 담습니다. 한국어 번역을 추가하려면 ko.json 파일을, 스페인어는 es.json 파일을 추가하면 됩니다.  
* **사용법:** object.json과 같은 파일에서 하드코딩된 텍스트 대신 번역 키를 사용합니다.  
  JSON  
  // object.json  
  {  
    "Name": "Rice",  
    "Description": "{{i18n:item.rice.description}}",  
   ...  
  }

  JSON  
  // i18n/ko.json  
  {  
    "item.rice.description": "다른 요리와 함께 자주 제공되는 기본적인 곡물입니다."  
  }

  이렇게 하면 게임 언어 설정에 따라 JA가 자동으로 적절한 번역을 불러옵니다.

### **3.2. 환상의 조합: Json Assets \+ Content Patcher**

JA로 아이템을 추가하면, 그 아이템은 게임의 내부 데이터 테이블의 일부가 됩니다. 이는 CP를 통해 해당 아이템의 속성을 동적으로 수정할 수 있음을 의미합니다.2 이 둘의 연동은 현대적인 스타듀밸리 모딩의 핵심입니다.

#### **핵심 도구: CP 토큰**

CP가 JA 아이템을 안정적으로 참조하기 위해서는 {{spacechase0.JsonAssets/ObjectId:Item Name}} 토큰을 사용해야 합니다. JA는 모드를 로드할 때마다 아이템에 동적으로 숫자 ID를 할당하는데, 이 ID는 설치된 모드에 따라 달라질 수 있습니다. 이 토큰은 CP에게 "JA에게 'Item Name'이라는 아이템의 현재 숫자 ID를 물어봐줘"라고 지시하는 역할을 합니다. 이를 통해 하드코딩된 ID를 사용하는 것보다 훨씬 안정적이고 호환성이 높은 패치를 만들 수 있습니다.21

#### **실전 예제 1: 상점 재고 패치하기**

Mizu's Flowers의 "페튜니아 씨앗"을 여행 카트의 판매 목록에 추가하는 CP 패치입니다. 이는 JA 아이템과 관련된 게임 데이터를 수정하는 대표적인 예입니다.

* 별도의 \[CP\] MyCompanionPack 모드를 만들고, 그 안의 content.json에 다음 내용을 추가합니다.  
  JSON  
  {  
    "Format": "2.0.0",  
    "Changes":  
          }  
        }  
      }  
    \]  
  }

#### **실전 예제 2: 커스텀 NPC 선물 취향 패치하기**

JA로 추가한 아이템을 커스텀 NPC의 선물 취향에 추가하는 것은 모드 간 상호작용의 흔한 사례입니다. 이는 포럼에서 자주 제기되는 질문이기도 합니다.21

* \[CP\] MyCompanionPack의 content.json에 다음 패치를 추가합니다.  
  JSON  
  {  
    "Action": "EditData",  
    "Target": "Data/NPCGiftTastes",  
    "Entries": {  
      "FlashShifter.SomeCustomNPC/love": "+{{spacechase0.JsonAssets/ObjectId:Petunia}}",  
    }  
  }

  * Target은 NPC 선물 취향 데이터 파일입니다.  
  * Entries의 키는 모드UniqueID.NPC이름/취향 형식입니다.  
  * 값의 \+는 기존 목록에 추가함을 의미하며, 그 뒤에 JA 토큰을 사용하여 페튜니아의 ID를 동적으로 가져옵니다.

이러한 '컴패니언 팩' 아키텍처는 JA 모드를 게임 세계에 완벽하게 통합하는 표준 방식입니다. PPJA의 다운로드 파일이 \[JA\], \[CP\], \[MFM\] 등 여러 폴더를 포함하는 이유가 바로 이것입니다.14 개발자는 단일 JA 팩을 만드는 것을 넘어, 이러한 다중 팩 구조를 이해하고 활용할 수 있어야 합니다.

### **3.3. C\# 개발자를 위한 JA API 활용**

SMAPI 모드(C\#)를 직접 개발하는 경우, JA가 제공하는 API를 통해 JA 콘텐츠 팩과 상호작용할 수 있습니다.15

* **API 가져오기:** 모드의 진입점(ModEntry.cs)에서 API 인터페이스를 가져옵니다.  
  C\#  
  IJsonAssetsApi api \= this.Helper.ModRegistry.GetApi\<IJsonAssetsApi\>("spacechase0.JsonAssets");

* **ID 조회:** 아이템의 내부 이름으로 동적 ID를 조회할 수 있습니다.  
  C\#  
  int petuniaId \= api.GetObjectId("Petunia");

* **콘텐츠 팩 로드:** C\# 모드 내에서 직접 JA 콘텐츠 팩을 로드할 수도 있습니다.  
  C\#  
  api.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets"));

---

## **챕터 4: 모범 사례 및 문제 해결**

### **4.1. 에셋 제작 및 관례**

* **스프라이트 제작:** 모든 아이템 스프라이트는 16의 배수 크기를 가집니다. 오브젝트는 16x16, 거대 설치물은 16x32(1x2 타일) 또는 16x48(1x3 타일) 픽셀입니다. 작물 스프라이트 시트와 의류 스프라이트 시트는 게임의 요구사항에 맞는 특정 레이아웃을 따라야 합니다.  
* **폴더명 규칙:** 모드 폴더 이름에 \[JA\] ModName과 같이 프레임워크 약어를 접두사로 붙이는 관례를 따르는 것이 좋습니다. 이는 사용자와 다른 개발자가 모드의 의존성을 한눈에 파악하는 데 도움을 줍니다.13

### **4.2. 콘텐츠 팩 디버깅**

모드 개발 중 발생하는 문제는 대부분 SMAPI 콘솔 창의 오류 메시지를 통해 해결할 수 있습니다.

* **흔한 오류 1: Mod crashed when editing asset...** 25  
  * **원인:** JSON 파일의 문법 오류일 가능성이 가장 높습니다. 쉼표가 빠졌거나, 괄호가 짝이 맞지 않는 경우입니다.  
  * **해결책:** SMAPI JSON 검사기를 사용하여 오류가 발생한 파일을 검사합니다.  
* **흔한 오류 2: 아이템이 오류 아이템(초록색 상자 등)으로 표시됨**  
  * **원인:** .png 파일이 없거나, 파일 이름이 잘못되었거나, JSON 파일의 Name 속성과 아이템 폴더 이름이 일치하지 않는 경우입니다.  
  * **해결책:** 파일과 폴더의 이름, 경로를 다시 한번 확인합니다. 대소문자도 구분하므로 주의해야 합니다.  
* **흔한 오류 3: Couldn't parse JSON file...**  
  * **원인:** JSON 파일의 문법이 완전히 깨져서 파싱 자체가 불가능한 경우입니다.  
  * **해결책:** JSON 검사기를 통해 문법을 교정합니다.  
* **흔한 오류 4: 의존성 문제**  
  * **원인:** Json Assets 모드 자체가 없거나, 너무 낮은 버전일 경우 발생합니다.  
  * **해결책:** SMAPI가 시작 시 알려주는 대로 최신 버전의 Json Assets를 설치합니다.

## **결론**

Json Assets는 스타듀밸리 1.6 업데이트 이후에도 여전히 강력하고 필수적인 모딩 프레임워크입니다. 이 가이드에서 다룬 바와 같이, JA는 새로운 아이템을 추가하는 체계적인 방법을 제공하며, 특히 Content Patcher와의 연동을 통해 그 잠재력을 최대한 발휘할 수 있습니다.

성공적인 JA 모드 개발의 핵심은 프레임워크의 기본 문법을 익히는 것뿐만 아니라, PPJA와 같은 성공적인 프로젝트에서 나타나는 커뮤니티의 모범 사례와 아키텍처 패턴(계층적 폴더 구조, 컴패니언 팩 사용 등)을 이해하고 적용하는 데 있습니다. 본 가이드에서 제공된 실전 예제와 상세한 설명을 통해, 개발자는 이제 자신만의 아이디어를 스타듀밸리 세계에 생명을 불어넣는 기능적인 콘텐츠 팩으로 구현할 준비가 되었을 것입니다. 앞으로의 모딩 여정에서 이 가이드가 든든한 참고 자료가 되기를 바랍니다.

---

## **부록: 빠른 참조 표**

### **A.1. object.json**

| 속성 | 타입 | 필수 여부 | 설명 |
| :---- | :---- | :---- | :---- |
| Name | string | 예 | 아이템의 내부 이름. |
| Description | string | 예 | 게임 내 설명. |
| Category | string | 예 | 아이템 분류 ("Vegetable", "Gem", "Fish", "Artifact" 등). |
| Price | integer | 예 | 기본 판매 가격. |
| Edibility | integer | 아니요 | 에너지 회복량. 먹을 수 없으면 \-300. |
| IsDrink | boolean | 아니요 | 음료수 여부. 기본값 false. |
| Buffs | object | 아니요 | 섭취 시 버프 효과. 각 객체는 Farming, Fishing, Mining, Luck, Foraging, MaxStamina, MagnetRadius, Speed, Defense, Attack 중 하나와 Amount, Duration을 포함. |
| Recipe | object | 아니요 | 제작법. ResultCount, Ingredients 배열, CanPurchase, PurchaseFrom, PurchasePrice, PurchaseRequirements, SkillUnlockName, SkillUnlockLevel 포함. |
| GiftTastes | object | 아니요 | NPC 선물 취향. Love, Like, Neutral, Dislike, Hate 키와 NPC 이름 배열을 값으로 가짐. |

### **A.2. crop.json**

| 속성 | 타입 | 필수 여부 | 설명 |
| :---- | :---- | :---- | :---- |
| Name | string | 예 | 작물의 내부 이름. |
| Product | string | 예 | 수확물 오브젝트의 Name. |
| SeedName | string | 예 | 씨앗 아이템의 Name. |
| SeedDescription | string | 예 | 씨앗 아이템의 설명. |
| Type | string | 예 | 작물 종류 ("Flower", "Fruit", "Vegetable", "Basic" 등). |
| Seasons | string | 예 | 성장 가능 계절 배열 ("spring", "summer", "fall", "winter"). |
| Phases | integer | 예 | 성장 단계별 소요 일수 배열. |
| RegrowthPhase | integer | 아니요 | 재성장 시 돌아갈 Phases 배열의 인덱스. 재성장 안 하면 \-1. |
| HarvestWithScythe | boolean | 아니요 | 낫 필요 여부. 기본값 false. |
| TrellisCrop | boolean | 아니요 | 격자 작물 여부. 기본값 false. |
| Colors | string | 아니요 | (꽃 전용) 가능한 꽃 색상 (RGB 문자열 배열). |
| Bonus | object | 아니요 | 추가 수확량. MinimumPerHarvest, MaximumPerHarvest, MaxIncreasePerFarmLevel, ExtraChance 포함. |
| SeedPurchasePrice | integer | 예 | 씨앗 구매 가격. |
| SeedPurchaseFrom | string | 예 | 씨앗 판매처. |
| SeedPurchaseRequirements | string | 아니요 | 씨앗 구매 조건. |

### **A.3. fruittree.json**

| 속성 | 타입 | 필수 여부 | 설명 |
| :---- | :---- | :---- | :---- |
| Name | string | 예 | 과일나무의 내부 이름. |
| Product | string | 예 | 수확물 오브젝트의 Name. |
| SaplingName | string | 예 | 묘목 아이템의 Name. |
| SaplingDescription | string | 예 | 묘목 아이템의 설명. |
| Season | string | 예 | 과일이 열리는 계절 ("spring", "summer", "fall", "winter"). |
| SaplingPurchasePrice | integer | 예 | 묘목 구매 가격. |
| SaplingPurchaseFrom | string | 예 | 묘목 판매처. |
| SaplingPurchaseRequirements | string | 아니요 | 묘목 구매 조건. |

### **A.4. big-craftable.json**

| 속성 | 타입 | 필수 여부 | 설명 |
| :---- | :---- | :---- | :---- |
| Name | string | 예 | 거대 설치물의 내부 이름. |
| Description | string | 예 | 게임 내 설명. |
| Price | integer | 예 | 기본 판매 가격. |
| Fragility | integer | 아니요 | 내구성. 기본값 0\. |
| IsLamp | boolean | 아니요 | 조명 여부. 기본값 false. |
| Recipe | object | 아니요 | 제작법. object.json의 Recipe와 동일한 구조. |

### **A.5. weapon.json**

| 속성 | 타입 | 필수 여부 | 설명 |
| :---- | :---- | :---- | :---- |
| Name | string | 예 | 무기의 내부 이름. |
| Description | string | 예 | 게임 내 설명. |
| Type | string | 예 | 무기 종류 ("Dagger", "Club", "Sword"). |
| MinimumDamage | integer | 예 | 최소 공격력. |
| MaximumDamage | integer | 예 | 최대 공격력. |
| Knockback | float | 예 | 넉백. |
| Speed | integer | 예 | 공격 속도. |
| Accuracy | integer | 예 | 명중률. |
| Defense | integer | 예 | 방어력. |
| CritChance | float | 예 | 치명타 확률. |
| CritMultiplier | float | 예 | 치명타 배율. |
| CanPurchase | boolean | 아니요 | 구매 가능 여부. 기본값 false. |
| PurchaseFrom | string | 아니요 | 판매처. |
| PurchasePrice | integer | 아니요 | 구매 가격. |
| PurchaseRequirements | string | 아니요 | 구매 조건. |

### **A.6. hat.json, shirt.json, pants.json, boots.json**

| 속성 (공통) | 타입 | 필수 여부 | 설명 |
| :---- | :---- | :---- | :---- |
| Name | string | 예 | 아이템의 내부 이름. |
| Description | string | 예 | 게임 내 설명. |
| CanPurchase | boolean | 아니요 | 구매 가능 여부. |
| PurchaseFrom | string | 아니요 | 판매처. |
| PurchasePrice | integer | 아니요 | 구매 가격. |
| PurchaseRequirements | string | 아니요 | 구매 조건. |
| **속성 (모자)** |  |  |  |
| ShowHair | boolean | 아니요 | 머리카락 표시 여부. 기본값 true. |
| IgnoreHairstyleOffset | boolean | 아니요 | 헤어스타일 오프셋 무시 여부. 기본값 false. |
| **속성 (의류)** |  |  |  |
| CanBeDyed | boolean | 아니요 | 염색 가능 여부. 기본값 false. |
| DefaultColor | string | 아니요 | 기본 색상 (RGBA 문자열). |
| **속성 (신발)** |  |  |  |
| Defense | integer | 예 | 추가 방어력. |
| Immunity | integer | 예 | 추가 면역. |

#### **참고 자료**

1. Json Assets | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/399895-json-assets](https://www.moddrop.com/stardew-valley/mods/399895-json-assets)  
2. Modding:Content pack frameworks \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Content\_pack\_frameworks](https://stardewvalleywiki.com/Modding:Content_pack_frameworks)  
3. Can someone please explain : r/SMAPI \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/SMAPI/comments/xxip9v/can\_someone\_please\_explain/](https://www.reddit.com/r/SMAPI/comments/xxip9v/can_someone_please_explain/)  
4. Question \- Re: making mods using content patcher | Stardew Valley Forums, 9월 16, 2025에 액세스, [https://forums.stardewvalley.net/threads/re-making-mods-using-content-patcher.7174/](https://forums.stardewvalley.net/threads/re-making-mods-using-content-patcher.7174/)  
5. Modding:Content Patcher \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Content\_Patcher](https://stardewvalleywiki.com/Modding:Content_Patcher)  
6. Python script: Json Assets to Content Patcher converter : r/SMAPI \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/SMAPI/comments/1bk6sgs/python\_script\_json\_assets\_to\_content\_patcher/](https://www.reddit.com/r/SMAPI/comments/1bk6sgs/python_script_json_assets_to_content_patcher/)  
7. PPJA Stands for Project Populate Json Assets, this serves as the official github for PPJA packs., 9월 16, 2025에 액세스, [https://github.com/paradigmnomad/PPJA](https://github.com/paradigmnomad/PPJA)  
8. Can you add custom objects with just content patcher, or do you need JSON assets? : r/SMAPI \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/SMAPI/comments/19d01gg/can\_you\_add\_custom\_objects\_with\_just\_content/](https://www.reddit.com/r/SMAPI/comments/19d01gg/can_you_add_custom_objects_with_just_content/)  
9. Tutorial: Your First Content Patcher Pack \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/Tutorial:\_Your\_First\_Content\_Patcher\_Pack](https://stardewmodding.wiki.gg/wiki/Tutorial:_Your_First_Content_Patcher_Pack)  
10. Tutorial: Adding a Recipe \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/Tutorial:\_Adding\_a\_Recipe](https://stardewmodding.wiki.gg/wiki/Tutorial:_Adding_a_Recipe)  
11. Creating your first schema \- JSON Schema, 9월 16, 2025에 액세스, [https://json-schema.org/learn/getting-started-step-by-step](https://json-schema.org/learn/getting-started-step-by-step)  
12. JSON Schema, 9월 16, 2025에 액세스, [https://json-schema.org/](https://json-schema.org/)  
13. Modding:Content packs \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Content\_packs](https://stardewvalleywiki.com/Modding:Content_packs)  
14. PPJA \- Fruits and Veggies | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/660885-ppja-fruits-and-veggies](https://www.moddrop.com/stardew-valley/mods/660885-ppja-fruits-and-veggies)  
15. Create a JA content-pack in SMAPI mod \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/Create\_a\_JA\_content-pack\_in\_SMAPI\_mod](https://stardewmodding.wiki.gg/wiki/Create_a_JA_content-pack_in_SMAPI_mod)  
16. How I Created My First STARDEW VALLEY Mod\! (Content Patcher) \- YouTube, 9월 16, 2025에 액세스, [https://www.youtube.com/watch?v=ZxOuca9qaaI](https://www.youtube.com/watch?v=ZxOuca9qaaI)  
17. PPJA \- Mizu's Flowers | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/661008-ppja-mizus-flowers](https://www.moddrop.com/stardew-valley/mods/661008-ppja-mizus-flowers)  
18. PPJA \- Artisan Valley | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/660997-ppja-artisan-valley](https://www.moddrop.com/stardew-valley/mods/660997-ppja-artisan-valley)  
19. JsonAssets \- PPJA \- Artisan Valley \- Stardew Valley Forums, 9월 16, 2025에 액세스, [https://forums.stardewvalley.net/resources/ppja-artisan-valley.10/](https://forums.stardewvalley.net/resources/ppja-artisan-valley.10/)  
20. How to make your mod compatible with other languages \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/How\_to\_make\_your\_mod\_compatible\_with\_other\_languages](https://stardewmodding.wiki.gg/wiki/How_to_make_your_mod_compatible_with_other_languages)  
21. Making a patch to edit an item added as Json asset by another mod? : r/SMAPI \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/SMAPI/comments/m37ge5/making\_a\_patch\_to\_edit\_an\_item\_added\_as\_json/](https://www.reddit.com/r/SMAPI/comments/m37ge5/making_a_patch_to_edit_an_item_added_as_json/)  
22. Json Assets \- Chucklefish Forums \- Starbound, 9월 16, 2025에 액세스, [https://community.playstarbound.com/resources/json-assets.5138/](https://community.playstarbound.com/resources/json-assets.5138/)  
23. Coding Help \- Is it possible to create a CP mod for a JA mod? \- Stardew Valley Forums, 9월 16, 2025에 액세스, [https://forums.stardewvalley.net/threads/is-it-possible-to-create-a-cp-mod-for-a-ja-mod.22309/](https://forums.stardewvalley.net/threads/is-it-possible-to-create-a-cp-mod-for-a-ja-mod.22309/)  
24. \[Visual Studio\] How to use Content Patcher and Json Assets within a SMAPI mod? \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/SMAPI/comments/143kydy/visual\_studio\_how\_to\_use\_content\_patcher\_and\_json/](https://www.reddit.com/r/SMAPI/comments/143kydy/visual_studio_how_to_use_content_patcher_and_json/)  
25. json assets | Stardew Valley Forums, 9월 16, 2025에 액세스, [https://forums.stardewvalley.net/tags/json-assets/](https://forums.stardewvalley.net/tags/json-assets/)