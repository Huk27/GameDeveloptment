

# **종합 DGA 개발 가이드: Artisan Valley 모드 해부**

## **서론: 위대한 유산으로부터 배우기**

### **이 가이드의 목적**

이 문서는 스타듀밸리 모딩 프레임워크인 Dynamic Game Assets(DGA)의 마스터클래스 역할을 합니다. 기존 DGA 가이드들이 실제적인 예제가 부족하다는 일반적인 피드백을 반영하여, 지금껏 제작된 가장 야심 찬 콘텐츠 모드 중 하나인 'Artisan Valley'(AV)를 실용적인 사례 연구로 사용합니다. 이 가이드를 통해 DGA의 핵심 개념과 실제 적용 방법을 심도 있게 학습할 수 있을 것입니다.

### **Artisan Valley의 맥락 (중요한 서문)**

본격적인 논의에 앞서, 이 가이드에서 다루는 Artisan Valley 모드의 현재 상태를 명확히 이해하는 것이 매우 중요합니다. Project Populate Json Assets(PPJA) 컬렉션의 핵심이었던 Artisan Valley는 이제 레거시(legacy) 모드입니다. 이 모드는 스타듀밸리 1.6 이전 버전을 위해 설계되었으며, **현재 게임 버전과 호환되지 않습니다**. 공식적인 후속작으로 여겨지는 Cornucopia 모드는 DGA가 아닌 다른 프레임워크를 기반으로 제작되었습니다.

### **왜 단종된 모드를 연구하는가?**

그렇다면 왜 우리는 더 이상 사용되지 않는 모드를 심도 있게 분석하려 할까요? 그 이유는 Artisan Valley를 단순히 복사해야 할 템플릿으로 보는 것이 아니라, 귀중한 '고고학적 발굴' 대상으로 삼기 위함입니다. 이 모드의 엄청난 복잡성, 과거의 성공, 그리고 최신 게임 환경으로 전환하는 데 실패한 과정은 모드 아키텍처, 의존성 관리, 프레임워크의 진화에 대한 심오한 교훈을 제공합니다. Artisan Valley의 구조를 해부함으로써, 간단한 "Hello, World" 예제로는 결코 가르칠 수 없는 방식으로 DGA의 핵심 메커니즘을 배우게 될 것입니다. 이 가이드는 과거의 유산을 통해 미래의 모드를 만드는 지혜를 전달하는 것을 목표로 합니다.

---

## **섹션 1: 모딩 프레임워크의 지형도: 콘텐츠 주입의 역사**

### **Json Assets (JA)의 시대**

대규모 콘텐츠 모딩의 역사는 Json Assets(JA) 프레임워크에서 시작되었습니다. 이 섹션에서는 JA의 역할과 Artisan Valley가 어떻게 이 생태계의 중심이 되었는지 살펴봅니다.

#### **기능과 목적**

JA는 모더들이 C\# 코드를 직접 작성하지 않고도 JSON 파일을 통해 게임에 새로운 아이템, 기계, 작물 등을 추가할 수 있게 해준 최초의 혁신적인 프레임워크였습니다. Artisan Valley는 근본적으로 JA를 기반으로 구축된 거대한 콘텐츠 팩이었습니다. 모더들은 JA가 제공하는 규격에 맞춰 아이템의 속성을 정의한 JSON 파일과 이미지 에셋만 준비하면 되었고, JA 프레임워크가 게임 데이터에 이 정보들을 주입하는 복잡한 과정을 처리했습니다. S8에서 발견된 오류 로그에 명시된 JsonAssets.Framework.ContentInjector1이라는 경로는 Artisan Valley가 기술적으로 JA에 깊이 의존하고 있었음을 보여주는 직접적인 증거입니다.

#### **PPJA 생태계**

Artisan Valley는 단독 모드가 아니라, 'Project Populate Json Assets'(PPJA)라는 거대한 협업 프로젝트의 일부였습니다. PPJA의 목표는 수많은 모더들이 각자의 콘텐츠(작물, 과일, 제작품 등)를 만들되, 서로 호환되고 표준화된 확장 세계관을 구축하는 것이었습니다. 이 거대한 생태계는 JA를 공통의 뼈대로 삼아 작동했습니다. 수백 가지가 넘는 아이템과 수십 개의 기계가 이 생태계 안에서 유기적으로 연결될 수 있었던 것은 모두 JA 덕분이었습니다.

### **Dynamic Game Assets (DGA)의 부상**

시간이 흐르면서 JA의 몇 가지 한계점이 드러났고, 이를 해결하기 위해 더욱 강력한 후속 프레임워크인 Dynamic Game Assets(DGA)가 등장했습니다.

#### **JA의 한계점 해결**

DGA는 JA의 단점을 보완하고 더 많은 기능을 제공하기 위해 개발되었습니다. 가장 큰 차이점은 아이템 ID 관리 방식이었습니다. JA에서는 모든 아이템이 고유한 숫자 ID를 가져야 했고, 여러 모드를 함께 사용할 경우 이 ID가 충돌하여 게임 오류를 일으키는 주된 원인이 되었습니다. DGA는 이러한 숫자 ID 대신 문자열 기반의 고유 ID("UniqueID.ItemID")를 사용하여 ID 충돌 문제를 원천적으로 해결했습니다. 또한, 제작 레시피나 생산 규칙 같은 복잡한 로직을 아이템 정의 파일 안에 통합하여 관리의 편의성을 높였습니다.

#### **불완전했던 전환 과정**

PPJA 제작팀은 JA 기반의 모드들을 DGA로 전환하려는 시도를 했습니다. "Farmer to Florist"와 같은 일부 모드는 성공적으로 DGA 버전을 출시했지만, Artisan Valley와 같은 거대 모드의 전환은 순탄치 않았습니다. 사용자들은 DGA 버전으로 전환하는 과정에서 버터 교반기(Butter Churn)나 유리병(Glass Jar)과 같은 특정 기계들이 제대로 작동하지 않는 등 수많은 버그를 보고했습니다.

이러한 문제들은 Artisan Valley의 완전하고 안정적인 공식 DGA 버전이 사실상 존재하지 않았음을 시사합니다. 커뮤니티에서 "DGA 버전"이라고 부르던 것은 단일 릴리즈가 아니라, 개발자와 사용자들이 기존 JA 팩을 DGA 로더와 호환되도록 만들려는 지속적이고 불완전한 변환 과정 그 자체였을 가능성이 높습니다. 이 가이드의 핵심 참고 자료가 될 S7과 S21의 문제 해결 과정에서도, 수정 대상 파일이 \`\` 폴더가 아닌 \[JA\] Artisan Valley Machine Goods 폴더 내에 있다는 점은 이러한 추측을 강력하게 뒷받침합니다. 따라서 이 가이드는 존재하지 않는 "AV의 DGA 코드"를 분석하는 대신, AV의 JA 및 PFM 파일에 담긴 데이터와 로직을 청사진 삼아 DGA 프레임워크 내에서 **어떻게 재구축할 수 있는지**를 가르치는 방향으로 진행될 것입니다. 이러한 접근 방식은 DGA의 원리를 배우는 데 가장 효과적이고 정직한 방법입니다.

---

## **섹션 2: DGA 콘텐츠 팩의 해부학: Artisan Valley 재구축**

이 섹션에서는 Artisan Valley의 콘텐츠를 DGA 프레임워크를 사용하여 처음부터 다시 만드는 과정을 단계별로 살펴봅니다. 이를 통해 DGA 콘텐츠 팩의 기본 구조와 핵심 파일 작성법을 익힐 수 있습니다.

### **2.1 기본 파일 (manifest.json과 content-pack.json)**

모든 DGA 콘텐츠 팩은 SMAPI가 모드를 인식하고 DGA가 콘텐츠를 로드할 수 있도록 두 개의 기본 설정 파일로 시작합니다.

#### **manifest.json**

이 파일은 모든 SMAPI 모드의 표준적인 '명함'입니다. SMAPI에게 모드의 이름, 제작자, 버전, 고유 ID 등의 기본 정보를 제공합니다. DGA 콘텐츠 팩의 경우 가장 중요한 부분은 ContentPackFor 필드입니다.

JSON

{  
  "Name": "Artisan Valley for DGA (Example)",  
  "Author": "Your Name",  
  "Version": "1.0.0",  
  "Description": "An example DGA content pack based on Artisan Valley.",  
  "UniqueID": "YourName.ArtisanValleyDGA",  
  "MinimumApiVersion": "4.0.0",  
  "ContentPackFor": {  
    "UniqueID": "spacechase0.DynamicGameAssets"  
  }  
}

여기서 ContentPackFor의 UniqueID를 spacechase0.DynamicGameAssets로 정확하게 지정해야 SMAPI가 이 폴더를 DGA의 콘텐츠 팩으로 인식하고 전달합니다.

#### **content-pack.json**

이 파일은 DGA 프레임워크 자체를 위한 진입점 파일입니다. 대부분의 경우, 이 파일의 내용은 매우 간단하며 DGA에게 이 폴더에 로드할 콘텐츠가 포함되어 있음을 알리는 역할만 합니다.

JSON

{  
  "Format": "1.0"  
}

이 두 파일이 준비되면 DGA 콘텐츠 팩을 만들 기본 골격이 완성된 것입니다.

### **2.2 커스텀 아이템 제작 (DGA Objects)**

이제 실제 콘텐츠를 추가해 보겠습니다. Artisan Valley의 간단한 가공품인 "건포도(Raisins)"를 DGA 아이템으로 정의하는 방법을 알아봅니다. S21의 문제 해결 과정에서 삭제 대상으로 언급된 바로 그 아이템입니다.

#### **파일 구조**

DGA는 직관적인 폴더 구조를 사용합니다. 각 아이템은 자신의 이름을 딴 폴더 안에 object.json 파일과 이미지 파일을 가집니다.

YourModName/Objects/Raisins/object.json  
YourModName/Objects/Raisins/raisins.png

#### **주석이 포함된 object.json (건포도 예시)**

object.json 파일은 아이템의 모든 속성을 정의합니다.

JSON

{  
  "$schema": "https://smapi.io/schemas/dga-object.json",  
  "ID": "Raisins",  
  "Name": "건포도",  
  "Description": "달콤하게 말린 포도. 건강에 좋은 간식입니다.",  
  "Category": "ArtisanGoods",  
  "Price": 75,  
  "Edibility": 20,  
  "Texture": "raisins.png",  
  "ContextTags": \[  
    "color\_purple",  
    "fruit\_item",  
    "artisan\_good"  
  \]  
}

#### **속성 심층 분석**

* $schema: (선택 사항) 이 JSON 파일이 DGA 객체 스키마를 따른다는 것을 명시하여, Visual Studio Code와 같은 편집기에서 자동 완성 및 유효성 검사 기능을 활성화합니다.  
* ID: DGA 내부에서 사용되는 이 아이템의 고유 식별자입니다. JA와 달리 숫자가 아닌 문자열을 사용하므로 다른 모드와 충돌할 염려가 없습니다.  
* Name: 게임 내에서 플레이어에게 표시될 이름입니다.  
* Description: 아이템에 마우스를 올렸을 때 표시될 설명입니다.  
* Category: 아이템의 분류입니다. 바닐라 게임의 카테고리("ArtisanGoods", "Fruit", "Vegetable" 등)를 사용할 수 있습니다.  
* Price: 아이템의 기본 판매 가격입니다.  
* Edibility: 먹었을 때 회복되는 체력의 양입니다.  
* Texture: 아이템의 이미지를 담고 있는 파일명입니다. 이 파일은 object.json과 같은 폴더에 있어야 합니다.  
* ContextTags: 이 아이템의 특성을 나타내는 태그 목록입니다. 이 태그들은 퀘스트, 선물 취향, 기계의 생산 규칙 등 게임의 다양한 시스템과 상호작용하는 데 사용됩니다. 예를 들어, fruit\_item 태그가 있으면 게임은 이 아이템을 과일로 인식합니다.

### **2.3 커스텀 기계 제작 (DGA BigCraftables)**

다음으로, 아이템을 가공하는 기능적인 기계를 만들어 보겠습니다. Artisan Valley의 핵심 기계 중 하나였던 "건조기(Dehydrator)"를 예로 들어 DGA의 BigCraftable을 정의하는 방법을 배웁니다.

#### **파일 구조**

기계 역시 아이템과 유사한 폴더 구조를 가집니다. 다만, 정의 파일의 이름이 big-craftable.json입니다.

YourModName/BigCraftables/Dehydrator/big-craftable.json  
YourModName/BigCraftables/Dehydrator/dehydrator.png

#### **주석이 포함된 big-craftable.json (건조기 예시)**

JSON

{  
  "$schema": "https://smapi.io/schemas/dga-big-craftable.json",  
  "ID": "Dehydrator",  
  "Name": "건조기",  
  "Description": "과일과 버섯을 말려 보존합니다.",  
  "Price": 2000,  
  "ProvidesLight": false,  
  "Texture": "dehydrator.png",  
  "Frames": { "Width": 16, "Height": 32, "Count": 2 },  
  "Recipe": {  
    "Result": "Dehydrator",  
    "Ingredients":,  
    "IsDefault": false,  
    "SkillUnlockName": "Foraging",  
    "SkillUnlockLevel": 6  
  },  
  "Production":  
}

#### **속성 심층 분석**

* Frames: 기계의 애니메이션을 위한 설정입니다. dehydrator.png 스프라이트 시트가 가로 16픽셀, 세로 32픽셀 크기의 프레임 2개로 구성되어 있음을 의미합니다 (예: 꺼진 상태, 작동 중인 상태).  
* Recipe: 이 기계를 제작하는 방법을 정의하는 강력한 기능입니다.  
  * Result: 레시피의 결과물입니다. 이 기계 자체의 ID를 가리킵니다.  
  * Ingredients: 제작에 필요한 재료 목록입니다. 바닐라 아이템 이름이나 다른 DGA 아이템의 ID를 사용할 수 있습니다.  
  * IsDefault: true이면 플레이어가 게임 시작부터 레시피를 알고 있고, false이면 특정 조건을 만족해야 배울 수 있습니다.  
  * SkillUnlockName & SkillUnlockLevel: IsDefault가 false일 때, 레시피를 배우게 되는 조건입니다. 이 예시에서는 채집(Foraging) 레벨이 6이 되면 자동으로 레시피를 습득합니다.  
* Production: 이 기계가 무엇을 생산하는지에 대한 규칙을 정의하는 부분입니다. 다음 섹션에서 자세히 다룹니다.

| 속성 분류 | DGA Object (object.json) | DGA BigCraftable (big-craftable.json) |
| :---- | :---- | :---- |
| **공통 핵심 속성** | ID, Name, Description, Category, Price, Texture, ContextTags | ID, Name, Description, Price, Texture |
| **아이템 특화 속성** | Edibility, GiftTastes | 해당 없음 |
| **기계 특화 속성** | 해당 없음 | ProvidesLight, Frames, Recipe, Production |

### **2.4 기계 로직 정의 (생산 규칙)**

기계의 핵심은 생산 능력입니다. DGA에서는 이 로직을 Production 배열 안에 직접 정의하여 아이템 데이터와 로직을 하나로 통합합니다.

#### **PFM의 선례**

원래 Artisan Valley는 생산 규칙을 정의하기 위해 Producer Framework Mod(PFM)라는 별도의 모드를 사용했습니다. S7/S21의 해결책은 PFM의 설정 파일인 ProducerRules.json과 ProducersConfig.json을 직접 수정하는 것을 포함합니다. 이러한 구조, 즉 아이템 데이터(JA)와 아이템 로직(PFM)을 분리하는 것은 당시로서는 강력했지만, 동시에 아키텍처의 취약점이 되었습니다.

Artisan Valley의 성공과 실패는 여러 프레임워크에 의존하는 복잡한 시스템의 본질적인 문제를 보여줍니다. JA(아이템), PFM(로직), Content Patcher(CP, 데이터 패치), Mail Framework Mod(MFM, 편지) 등 여러 모드가 사슬처럼 얽혀있는 구조는 방대한 콘텐츠를 가능하게 했지만, 극도로 불안정했습니다. 스타듀밸리 게임 본체나 이 의존성 사슬의 단 하나의 고리(프레임워크)에서 호환성이 깨지는 변경이 발생하면, 전체 시스템이 연쇄적으로 붕괴될 수 있었습니다. 실제로 PFM의 개발이 스타듀밸리 1.6 업데이트를 기다리며 중단되었고, 1.6이 출시되었을 때 지원이 끊긴 이 복잡한 스택은 완전히 무너져 Artisan Valley를 사용할 수 없게 만들었습니다. 이는 모드 설계 시 의존성을 최소화하고, 단일 프레임워크나 자체 포함적인 구조가 게임 업데이트에서 살아남을 가능성이 더 높다는 중요한 교훈을 줍니다.

#### **PFM 로직을 DGA 생산 규칙으로 변환하기**

DGA는 PFM과 같은 외부 의존성을 필요로 하지 않습니다. 생산 규칙은 big-craftable.json 파일 내부에 직접 포함됩니다. PFM의 ProducerRules.json에 있었을 법한 건조기 규칙을 DGA 방식으로 변환해 보겠습니다.

##### **PFM 예시 (개념적)**

JSON

// \[PFM\] Artisan Valley/ProducerRules.json 파일에 있었을 법한 내용  
{  
  "ProducerName": "Dehydrator",  
  "InputName": "category\_fruit",  
  "MinutesUntilReady": 1200,  
  "OutputName": "Dried Fruit"  
}

##### **DGA 구현 (통합)**

이 로직은 건조기의 big-craftable.json 파일 내 Production 배열에 다음과 같이 포함됩니다.

JSON

// Dehydrator의 big-craftable.json 파일 내부  
"Production": },  
    "Output": { "Object": "Dried Mushrooms", "Quantity": 1 },  
    "Minutes": 900  
  }  
\]

* Production은 여러 생산 규칙 객체를 담을 수 있는 배열입니다.  
* 각 규칙은 Input, Output, Minutes 필드를 가집니다.  
* Input: 투입 가능한 아이템을 지정합니다. 바닐라 카테고리("Category": "Fruit")나 ContextTags를 사용하여 유연하게 지정할 수 있습니다.  
* Output: 생산될 아이템의 DGA ID와 수량을 지정합니다.  
* Minutes: 생산에 소요되는 시간(게임 내 분)입니다.

이처럼 DGA는 아이템 정의, 제작법, 생산 로직을 하나의 파일로 통합하여 관리의 복잡성을 크게 줄이고 외부 프레임워크에 대한 의존성을 제거합니다.

---

## **섹션 3: 고급 기술 및 실제 문제 해결**

이론을 배웠으니 이제 실제 상황에 적용해 볼 차례입니다. 이 섹션에서는 S7과 S21에서 제기된 실제 문제 해결 사례를 바탕으로, 복잡한 모드 환경에서 발생하는 문제를 진단하고 해결하는 과정을 단계별로 따라가 봅니다. 이 사례는 모드 호환성, 게임 업데이트 대응, 디버깅에 대한 귀중한 통찰을 제공합니다.

### **3.1 사례 연구: 대(大) 건조기 사태**

#### **시나리오**

"다른 모드를 통해 새로운 버섯을 추가했더니 게임이 충돌하고, 스타듀밸리 1.6에 추가된 새로운 아이템들과 관련하여 건조기가 예기치 않게 동작합니다."라는 버그 리포트를 받았다고 가정해 봅시다. 이 문제는 Artisan Valley의 여러 구성 요소가 최신 게임 환경과 충돌하면서 발생한 복합적인 문제입니다. 해결 과정은 모드의 여러 부분을 체계적으로 수정해야 합니다.

#### **1단계: 컨텍스트 태그 통합 문제 해결 (\[CP\] content.json)**

첫 번째 해결책은 \[CP\] Artisan Valley 폴더의 content.json 파일을 수정하여 바닐라 버섯(일반 버섯, 보라색 버섯 등)에 "edible\_mushroom"이라는 컨텍스트 태그를 추가하는 것입니다.

JSON

// 예시: content.json 수정 전  
"Common Mushroom": "color\_dark\_brown, forage\_item, season\_spring, season\_fall, mushroom\_item",

// 예시: content.json 수정 후  
"Common Mushroom": "color\_dark\_brown, forage\_item, season\_spring, season\_fall, mushroom\_item, edible\_mushroom",

이 작업이 왜 필요했을까요? Artisan Valley의 건조기나 다른 PPJA 모드들은 특정 아이템을 식별하기 위해 mushroom\_item 외에도 edible\_mushroom과 같은 좀 더 구체적인 태그를 사용하도록 설계되었을 수 있습니다. 바닐라 아이템에 이 태그가 누락되면, 모드의 로직이 해당 아이템을 올바르게 인식하지 못해 오류를 일으킬 수 있습니다. 이는 모드가 게임의 기본 데이터와 일관성을 유지하는 것이 얼마나 중요한지를 보여줍니다.

#### **2단계: 중복 콘텐츠 제거 (\[JA\] 폴더)**

두 번째 해결책은 \[JA\] Artisan Valley Machine Goods 폴더에서 "Dehydrator", "Dried Mushrooms", "Dried Fruit", "Raisin" 폴더를 완전히 삭제하는 것입니다.

이 조치는 스타듀밸리 1.6 업데이트와 직접적인 관련이 있습니다. 1.6 업데이트에서 게임은 자체적인 건조기(Dehydrator)와 말린 과일/버섯 아이템을 공식적으로 추가했습니다. 이로 인해 Artisan Valley가 정의한 구식 건조기와 말린 아이템들은 이제 바닐라 에셋과 이름 및 기능 면에서 충돌하게 되었습니다. 게임이 동일한 개념의 아이템을 두 개의 다른 소스(바닐라와 모드)로부터 로드하려고 시도하면서 데이터 충돌, ID 중복, 그리고 궁극적으로는 게임 충돌로 이어진 것입니다. 오래된 모드 콘텐츠를 제거하는 것은 이러한 충돌을 해결하는 가장 확실한 방법이며, 게임 업데이트 시 모드 유지보수에서 가장 흔하게 발생하는 작업 중 하나입니다.

#### **3단계: 기계 로직 비활성화 (\[PFM\] 파일)**

마지막 해결책은 \[PFM\] Artisan Valley 폴더에서 두 개의 파일을 수정하는 것입니다. 먼저, ProducersConfig.json 파일에서 "Dehydrator" 항목 전체를 삭제합니다. 다음으로, ProducerRules.json 파일에서 "Dried Mushrooms", "Dried Fruit", "Raisin"을 생산하는 모든 규칙을 찾아 삭제합니다.

이 작업은 2단계에서 아이템과 기계 자체를 제거한 것에 대한 후속 조치입니다. 기계의 '몸체'(\[JA\] 폴더)를 제거했더라도, 그 기계의 '두뇌'에 해당하는 생산 로직(\[PFM\] 파일)이 여전히 남아있다면 PFM 프레임워크는 존재하지 않는 기계의 규칙을 로드하려고 시도하며 오류를 발생시킬 것입니다. 이처럼 콘텐츠를 제거할 때는 관련된 모든 구성 요소(데이터, 로직, 레시피 등)를 함께 제거해야 시스템의 안정성을 보장할 수 있습니다.

이 일련의 해결 과정은 복잡한 콘텐츠 모드가 단순히 새로운 것을 '추가'하는 행위가 아님을 명확히 보여줍니다. 모드는 실제로는 게임의 기본 데이터에 대한 일련의 정교한 '패치(patch)' 작업을 수행합니다. 이 사례에서 모더는 CP 파일을 편집하여 아이템의 속성 데이터베이스를 패치했고, JA 파일을 삭제하여 아이템 목록 데이터베이스를 패치했으며, PFM 파일을 편집하여 생산 규칙 데이터베이스를 패치했습니다. 모드를 아이템의 집합이 아닌, 데이터 패치의 집합으로 이해하는 것은 디버깅과 모드 호환성 확보에 필수적인 상위 수준의 사고방식입니다. 문제를 해결하기 위해서는 모드의 어떤 부분이 게임 데이터의 어떤 계층에 영향을 미치는지, 그리고 이 데이터베이스들이 어떻게 서로 연관되어 있는지를 파악해야 합니다.

---

## **섹션 4: 앞으로 나아갈 길: 스타듀밸리 1.6+ 시대의 DGA**

Artisan Valley의 사례를 통해 DGA의 구조와 문제 해결 방법을 배웠지만, 스타듀밸리 모딩 커뮤니티는 끊임없이 발전하고 있습니다. 이 섹션에서는 Artisan Valley의 시대 이후 모딩 생태계가 어떻게 변화했는지, 그리고 오늘날 새로운 콘텐츠 모드를 만들 때 어떤 접근 방식이 권장되는지 살펴봅니다.

### **4.1 Content Patcher의 부상**

#### **Cornucopia의 등장**

Cornucopia는 PPJA와 Artisan Valley의 정신적 후속작으로 여겨지는 대규모 콘텐츠 모드입니다. 이 모드는 기존 PPJA 팀의 베테랑 모더들이 참여하여 제작했으며, Artisan Valley가 남긴 유산을 현대적인 방식으로 계승하고 있습니다.

#### **의도적인 아키텍처 전환**

Cornucopia에서 배울 수 있는 가장 중요한 교훈은 바로 그 아키텍처에 있습니다. GitHub 저장소의 폴더 구조(\[CP\] Cornucopia...)와 공식 문서에서 확인할 수 있듯이, 이 모드는 작물, 과일, 가공품 등 **콘텐츠의 대부분을 추가하기 위해 Content Patcher(CP)를 사용합니다**.1 이는 DGA나 JA와 같은 콘텐츠 주입 프레임워크에서 벗어나, 데이터 파일을 직접 편집하는 CP 중심으로 전환한 의도적인 설계 결정입니다.

#### **왜 이러한 변화가 일어났는가?**

이전 섹션에서 분석한 Artisan Valley의 취약점을 고려하면 이러한 전환의 전략적 이유를 명확히 이해할 수 있습니다.

* **의존성 감소:** CP는 단일하고 매우 안정적인 의존성입니다. 반면 Artisan Valley는 JA, PFM, MFM 등 여러 프레임워크가 얽혀있는 불안정한 스택에 의존했습니다. 의존성이 적을수록 게임 업데이트 시 모드가 고장 날 확률이 줄어듭니다.  
* **향상된 호환성:** CP는 별도의 시스템을 통해 새로운 객체를 주입하는 대신, 게임의 원본 데이터 에셋(Data/Objects 등)을 직접 수정하는 방식으로 작동합니다. 이 방식은 게임의 기본 메커니즘을 따르기 때문에, 새로운 시스템을 도입하는 것보다 게임 업데이트와의 호환성을 유지하기에 더 유리한 경우가 많습니다.  
* **커뮤니티 표준:** Content Patcher는 스타듀밸리 모딩 커뮤니티에서 가장 신뢰받는 개발자가 유지보수하며, 사실상의 표준으로 자리 잡았습니다. 강력한 기능과 지속적인 업데이트 덕분에 대부분의 모더들이 선호하는 도구가 되었습니다.

### **4.2 DGA 개념을 현대 모딩에 적용하기**

그렇다면 DGA를 통해 배운 지식은 쓸모없어진 것일까요? 그렇지 않습니다. 프레임워크의 구문은 다를지라도, 콘텐츠를 정의하는 핵심 *개념*은 동일하며, 이 지식은 직접적으로 이전될 수 있습니다.

#### **비교 예제**

섹션 2.2에서 만들었던 DGA 방식의 "건포도" object.json과, 이를 현대적인 Content Patcher 방식으로 구현한 코드를 나란히 비교해 보겠습니다.

##### **Content Patcher content.json 스니펫**

JSON

{  
  "Format": "2.0.0",  
  "Changes":  
        }  
      }  
    }  
  \]  
}

#### **개념의 연결**

두 예제를 비교해 보면, JSON의 구조와 필드 이름(ID vs. YourName.Raisins 키, Category 문자열 vs. 숫자 등)은 다르지만, 정의해야 하는 정보의 본질은 같습니다. 두 방식 모두 고유한 ID, 표시 이름, 설명, 가격, 카테고리, 컨텍스트 태그 등을 지정해야 합니다. DGA를 통해 "아이템이란 무엇이며 어떤 속성으로 구성되는가"를 이해했다면, 새로운 프레임워크의 구문에 적응하는 것은 훨씬 수월합니다.

### **4.3 최종 권장 사항: 상황에 맞는 올바른 도구 선택**

#### **DGA의 지속적인 역할**

Content Patcher가 많은 경우에 선호되지만, DGA가 완전히 쓸모없어진 것은 아닙니다. DGA는 단순한 데이터 입력을 넘어선 더 복잡한 콘텐츠를 추가할 때 여전히 매우 강력한 도구입니다. 예를 들어, 독특한 성장 메커니즘을 가진 커스텀 작물, 상호작용이 가능한 커스텀 가구, 또는 CP만으로는 구현하기 어려운 새로운 유형의 콘텐츠를 만들 때 DGA는 훌륭한 선택지입니다.

#### **Content Patcher 우선 접근법**

하지만 Artisan Valley와 같이 새로운 아이템, 기계, 레시피를 대량으로 추가하는 것을 목표로 하는 프로젝트의 경우, 현대적인 권장 접근 방식은 **Content Patcher를 우선적으로 사용하는 것**입니다. 이 방법은 더 안정적이고, 호환성이 높으며, 현재 모딩 커뮤니티의 방향과 더 잘 부합합니다. 기계의 생산 로직 역시 Alternative Textures나 Producer Framework Mod의 현대화된 버전을 통해 CP와 함께 구현할 수 있는 방법들이 존재합니다.

#### **결론**

Artisan Valley의 이야기는 복잡한 다중 프레임워크 접근 방식에서 더 간결하고 데이터 중심적인 접근 방식으로 진화해 온 스타듀밸리 모딩의 역사를 압축적으로 보여줍니다. 이 여정을 이해함으로써, 새로운 모드 개발자는 DGA를 사용하는 기술적 능력뿐만 아니라, 견고하고 호환성이 뛰어나며 스타듀밸리의 미래에 대비할 수 있는 모드를 구축하는 아키텍처적 지혜를 갖추게 될 것입니다. 과거의 위대한 유산은 현재의 개발자들에게 가장 효과적인 교과서가 되어줍니다.

#### **참고 자료**

1. MizuJakkaru/Cornucopia: An additional crops, recipes, and ... \- GitHub, 9월 16, 2025에 액세스, [https://github.com/MizuJakkaru/Cornucopia](https://github.com/MizuJakkaru/Cornucopia)