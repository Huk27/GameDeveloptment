

# **spacechase0 코드 해부: 스타듀밸리 모딩을 위한 고급 개발자 가이드**

## **서론**

스타듀밸리 모딩 커뮤니티에서 spacechase0는 단순한 콘텐츠 추가를 넘어, 강력한 프레임워크와 새로운 게임 메커니즘을 창조해 온 선구적인 인물로 알려져 있습니다.1 그의 작업물들은 종종 다른 모더들이 자신의 창의력을 발휘할 수 있는 기반을 제공하며, 이는 커뮤니티 전체의 성장에 기여했습니다. 이 가이드의 핵심 철학은 추상적인 이론을 통해 고급 모딩 아키텍처를 배우는 것이 아니라, 숙련된 전문가의 실용적이고 검증된 코드를 직접 리버스 엔지니어링하여 그 원리를 터득하는 것입니다.

본 가이드에서는 spacechase0의 방대한 아카이브 저장소에서 세 가지 핵심적인 모드 및 개념을 사례 연구로 선정했습니다. 이들은 각각 고급 모딩의 근본적인 기둥을 대표합니다. 첫째, BugNet 모드는 커스텀 도구를 제작하는 방법을 보여줍니다. 둘째, CustomCritters는 다른 창작자들이 코딩 없이 콘텐츠를 추가할 수 있게 하는 콘텐츠 프레임워크 구축의 정수를 담고 있습니다. 마지막으로, CustomNPCFixes와 같은 모드에서 사용되는 원리를 통해 Harmony 라이브러리를 사용한 심층적인 게임 코드 수정을 탐구할 것입니다.

이 가이드는 단순히 각 모드의 기능을 나열하는 것을 넘어, 다른 개발자가 그 핵심 구현을 명확히 이해하고 자신의 프로젝트에서 유사한 기능을 재현할 수 있도록 실제 코드 샘플을 대폭 보강하여 구성되었습니다. 각 섹션은 독립적인 주제를 다루면서도 유기적으로 연결되어, 커스텀 아이템 제작, 콘텐츠 프레임워크 구축, 그리고 게임 코드 패치라는 고급 모딩 기술의 전체적인 그림을 제공할 것입니다. 이를 통해 개발자들은 단편적인 지식을 넘어, 안정적이고 호환성이 높으며 잘 설계된 모드를 만드는 데 필요한 종합적인 시각과 기술을 갖추게 될 것입니다.

---

## **섹션 1: 커스텀 도구 구현 \- BugNet 모드 심층 분석**

이 섹션에서는 BugNet 모드를 실용적인 사례 연구로 사용하여, 기존 도구의 단순한 외형 변경을 넘어 고유한 C\# 로직을 가진 새로운 기능성 도구를 만드는 과정을 심층적으로 분석합니다.1 이는 콘텐츠 모딩에서 게임플레이 메커니즘 모딩으로 나아가는 중요한 단계를 보여주는 훌륭한 예시입니다.

### **1.1. 커스텀 도구 정의: Data/Tools를 넘어서**

스타듀밸리에서 새로운 도구를 추가하는 현대적인 접근 방식은 게임의 핵심 데이터 에셋인 Data/Tools를 수정하는 것에서 시작합니다. 이 에셋은 도구의 이름, 설명, 텍스처와 같은 기본적인 속성을 정의하는 키-값 쌍의 데이터 집합입니다.3 그러나 진정한 커스텀 기능을 구현하기 위해서는 이 데이터 정의만으로는 부족합니다.

이때 핵심적인 역할을 하는 것이 ClassName 필드에 지정할 수 있는 특수한 값인 GenericTool입니다. Axe, Pickaxe와 같은 바닐라 도구 클래스 이름 대신 GenericTool을 지정하면, 게임은 특정 기능에 얽매이지 않는 범용 도구 객체를 생성합니다. 이는 C\# 모더에게 일종의 관문 역할을 하며, 이 범용 객체를 기반으로 커스텀 C\# 클래스를 연결하여 바닐라 도구로는 불가능했던 독창적인 동작을 구현할 수 있게 해줍니다.3

예를 들어, BugNet의 Data/Tools 항목은 다음과 같이 구성될 수 있습니다.

JSON

{  
    "spacechase0.BugNet": {  
        "ClassName": "GenericTool",  
        "Name": "Bug Net",  
        "DisplayName": "Bug Net",  
        "Description": "Used to catch critters.",  
        "Texture": "spacechase0.BugNet/tools",  
        "SpriteIndex": 0,  
        "SalePrice": 500,  
        "SetProperties": {  
            "InstantUse": true  
        }  
    }  
}

이 JSON 데이터는 게임에게 spacechase0.BugNet이라는 ID를 가진 아이템이 GenericTool 클래스를 사용하며, 커스텀 텍스처(spacechase0.BugNet/tools)의 첫 번째 스프라이트(SpriteIndex: 0)를 사용하고, 즉시 사용 가능한(InstantUse: true) 특성을 가짐을 알려줍니다. 이제 이 데이터 정의에 생명을 불어넣는 것은 C\# 코드의 몫입니다.

### **1.2. BugNetTool 클래스 아키텍처: StardewValley.Tool 상속**

커스텀 도구의 핵심 로직은 StardewValley.Tool 기본 클래스를 상속받는 C\# 클래스에 구현됩니다. SMAPI 모드 프로젝트 내에서 BugNetTool.cs와 같은 파일을 생성하고, 이 클래스가 Tool 클래스의 기능을 확장하도록 설계해야 합니다.4

클래스의 기본 구조는 다음과 같은 핵심 요소들을 포함합니다.

* **생성자 (Constructor):** 도구가 처음 생성될 때 초기 상태를 설정합니다. 예를 들어, 도구의 내부 이름, 기본 속성 등을 여기서 정의할 수 있습니다.  
* **속성 재정의 (Property Overrides):** Tool 클래스의 가상(virtual) 속성들을 재정의하여 커스텀 도구의 특성을 명시합니다. DisplayName, description 등이 여기에 해당합니다.  
* **핵심 메서드 재정의 (Core Method Overrides):** 도구의 고유한 기능을 구현하기 위해 가장 중요한 부분입니다. 플레이어가 도구를 사용할 때 호출되는 DoFunction() 메서드를 재정의하는 것이 일반적입니다. 이 외에도 beginUsing(), endUsing() 등 다양한 메서드를 재정의하여 더 복잡한 상호작용을 만들 수 있습니다.

다음은 BugNetTool 클래스의 기본적인 아키텍처를 보여주는 코드 예시입니다.

C\#

using StardewValley;  
using StardewValley.Tools;

namespace BugNetMod  
{  
    public class BugNetTool : Tool  
    {  
        public BugNetTool() : base("Bug Net", 0, 7, 7, false)  
        {  
            // 기본 생성자에서 도구의 기본 속성을 설정합니다.  
            // 이름, 업그레이드 레벨, 초기 타일 X/Y, 스택 가능 여부 등  
        }

        // 아이템 설명 등을 재정의할 수 있습니다.  
        public override string getDescription()  
        {  
            return "Used to catch various critters found throughout the valley.";  
        }

        // 플레이어가 도구를 사용할 때 호출되는 핵심 로직  
        protected override bool DoFunction(GameLocation location, int x, int y, int power, Farmer who)  
        {  
            // 이 메서드 내부에 '포획' 메커니즘을 구현합니다.  
            // 성공적으로 기능을 수행했다면 true를 반환합니다.  
              
            // 기본 Tool의 DoFunction을 호출하여 기본 동작(예: 기력 소모)을 수행하게 합니다.  
            base.DoFunction(location, x, y, power, who);

            //... 포획 로직 구현...

            return true;  
        }  
    }  
}

이 구조는 커스텀 도구의 뼈대를 형성하며, 다음 단계에서는 DoFunction() 메서드 내부를 채워 BugNet의 핵심 기능인 '포획' 메커니즘을 구현하게 됩니다.1

### **1.3. "포획" 메커니즘 구현: DoFunction()의 로직**

BugNet 모드의 핵심은 플레이어가 도구를 휘둘렀을 때 주변의 작은 동물(critter)을 아이템으로 바꾸어 인벤토리에 넣어주는 기능입니다. 이 모든 로직은 재정의된 DoFunction(GameLocation location, int x, int y, int power, Farmer who) 메서드 내에서 순차적으로 처리됩니다.

로직의 흐름은 다음과 같이 세분화할 수 있습니다.

1. **대상 타일 식별:** 플레이어가 바라보는 방향의 바로 앞 타일을 계산합니다. 이 타일이 포획 시도의 대상이 됩니다.  
2. **크리터 스캔:** 현재 위치(location)의 critters 리스트를 순회하며, 대상 타일 위에 있는 크리터가 있는지 확인합니다.  
3. **포획 실행:** 대상 타일에서 크리터를 발견하면, 해당 크리터를 location.critters 리스트에서 제거하여 게임 월드에서 사라지게 합니다.  
4. **포획된 아이템 생성:** ItemRegistry.Create() 메서드를 사용하여 포획된 크리터에 해당하는 새로운 인벤토리 아이템을 생성합니다. 예를 들어, 나비를 잡았다면 (O)Butterfly와 같은 형식의 아이템 ID를 사용합니다.6  
5. **인벤토리 추가:** 생성된 아이템을 플레이어(who)의 인벤토리에 추가합니다. who.addItemToInventoryBool() 메서드를 사용하여 인벤토리가 가득 찼는지 여부도 처리할 수 있습니다.  
6. **피드백 제공:** 포획 성공을 알리는 소리를 재생하거나 간단한 애니메이션 효과를 보여주어 사용자 경험을 향상시킵니다.

다음은 이 로직을 구현한 DoFunction 메서드의 상세 코드 예시입니다.

C\#

using Microsoft.Xna.Framework;  
using StardewValley;  
using StardewValley.Tools;  
using System.Linq;

//... 클래스 정의...

protected override bool DoFunction(GameLocation location, int x, int y, int power, Farmer who)  
{  
    base.DoFunction(location, x, y, power, who);

    // 1\. 대상 타일 식별  
    Vector2 toolTile \= who.GetToolLocation(false);

    // 2\. 크리터 스캔 및 포획  
    // location.critters 리스트를 복사하여 순회 중 수정으로 인한 오류를 방지합니다.  
    var crittersOnTile \= location.critters.Where(c \=\> c.getBoundingBox().Intersects(new Rectangle((int)toolTile.X, (int)toolTile.Y, 64, 64))).ToList();

    if (crittersOnTile.Any())  
    {  
        var critterToCatch \= crittersOnTile.First();  
          
        // 포획할 크리터의 종류에 따라 생성할 아이템 ID를 결정합니다.  
        // 이 예시에서는 간단하게 나비만 처리합니다.  
        string itemIdToCreate \= null;  
        if (critterToCatch.GetType().Name \== "Butterfly")  
        {  
            itemIdToCreate \= "(O)Butterfly"; // 실제 게임에 나비 아이템이 없으므로, 예시 ID입니다. Json Assets 등으로 추가해야 합니다.  
        }  
        //... 다른 종류의 크리터(예: 다람쥐, 개구리)에 대한 처리를 추가할 수 있습니다.

        if (itemIdToCreate\!= null)  
        {  
            // 3\. 포획 실행 (크리터 제거)  
            location.critters.Remove(critterToCatch);

            // 4\. 포획된 아이템 생성  
            Item caughtItem \= ItemRegistry.Create(itemIdToCreate);

            // 5\. 인벤토리 추가  
            if (who.addItemToInventoryBool(caughtItem))  
            {  
                // 6\. 피드백 제공  
                location.playSound("coin"); // 적절한 사운드로 교체  
                who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 50f, 6, 1, toolTile \* 64f, false, false));  
                return true;  
            }  
            else  
            {  
                // 인벤토리가 가득 찼을 경우, 아이템을 땅에 떨어뜨립니다.  
                Game1.createItemDebris(caughtItem, toolTile \* 64f, who.FacingDirection);  
                location.playSound("coin");  
                return true;  
            }  
        }  
    }

    // 아무것도 잡지 못했을 경우  
    return false;  
}

이처럼 DoFunction 메서드는 게임의 상태(location.critters)를 직접 조회하고 수정하며, 플레이어의 상태(who.addItemToInventoryBool)와 상호작용합니다. 이는 단순한 데이터 추가를 넘어 새로운 게임플레이 '동사'인 '포획'을 만들어내는 과정이며, 이것이 바로 커스텀 도구 모딩의 핵심입니다. 바닐라 게임에는 '포획'이라는 메커니즘이 존재하지 않으므로, 모더는 이 모든 절차적 로직을 C\# 코드로 직접 정의해야만 합니다.

### **1.4. 게임 통합 및 에셋 관리**

커스텀 C\# 클래스와 로직을 완성했다면, 이제 이 도구를 실제 게임 세계에 통합하여 플레이어가 획득하고 사용할 수 있도록 만들어야 합니다. 이 과정은 크게 에셋 로딩과 상점 통합의 두 단계로 나뉩니다.

* **에셋 로딩:** BugNet이 게임 내에서 올바른 이미지로 표시되려면 커스텀 스프라이트 시트를 로드해야 합니다. 이는 SMAPI의 IContentHelper API를 사용하여 처리할 수 있습니다. 모드의 assets 폴더에 tools.png와 같은 이미지 파일을 포함시키고, ModEntry 클래스에서 이 파일을 게임의 콘텐츠 파이프라인에 로드합니다. Content Patcher를 사용하면 이 과정을 더욱 간소화할 수 있습니다. content.json 파일에 Load 액션을 사용하여 커스텀 텍스처를 특정 에셋 이름(예: spacechase0.BugNet/tools)으로 로드하도록 지시할 수 있습니다.7  
* **상점 통합:** 플레이어가 BugNet을 구매할 수 있도록 상점의 판매 목록에 추가해야 합니다. 이는 Content Patcher를 사용하여 Data/Shops 에셋을 수정하는 방식으로 가장 쉽게 구현할 수 있습니다. BugNet이 피에르의 상점에서 판매된다는 사실을 바탕으로 2, 다음과 같은 Content Patcher 항목을 작성할 수 있습니다.

JSON

{  
    "Action": "EditData",  
    "Target": "Data/Shops",  
    "Entries": {  
        "SeedShop": {  
            "Items":  
        }  
    }  
}

이 코드는 SeedShop(피에르의 상점)의 판매 목록에 spacechase0.BugNet 아이템을 추가합니다. 가격은 500g, 재고는 1개로 설정되며, 특정 편지를 받았거나 게임 내 시간이 2년 차 이상일 때만 구매할 수 있다는 조건(Condition)을 부여하여 게임의 진행도에 맞춰 도구가 해금되도록 설계할 수 있습니다.

### **1.5. 개발자 워크스루: 나만의 커스텀 도구 만들기**

지금까지 논의된 개념들을 종합하여, 개발자가 직접 따라 할 수 있는 단계별 가이드를 제공합니다. 이 워크스루는 간단한 '표본 채집기'라는 커스텀 도구를 만드는 과정을 안내합니다.

1. **프로젝트 설정:**  
   * Visual Studio를 열고 .NET 6을 대상으로 하는 새로운 '클래스 라이브러리' 프로젝트를 생성합니다.4  
   * Pathoschild.Stardew.ModBuildConfig NuGet 패키지를 설치하여 스타듀밸리 및 SMAPI 참조를 자동으로 설정합니다.  
2. **C\# 코드 작성 (SampleTool.cs):**  
   * 프로젝트에 SampleTool.cs 파일을 추가하고 아래의 전체 코드를 붙여넣습니다. 이 코드는 플레이어가 도구를 사용하면 땅에서 '점토'를 파낼 확률을 갖는 간단한 기능을 구현합니다.

C\#  
using Microsoft.Xna.Framework;  
using StardewValley;  
using StardewValley.Tools;

namespace SampleToolMod  
{  
    public class SampleTool : Tool  
    {  
        public SampleTool() : base("Sample Collector", 0, 16, 16, false)  
        {  
            this.InstantUse \= true;  
        }

        protected override bool DoFunction(GameLocation location, int x, int y, int power, Farmer who)  
        {  
            base.DoFunction(location, x, y, power, who);  
            who.Stamina \-= 2; // 기력 소모

            Vector2 toolTile \= who.GetToolLocation(false);

            // 25% 확률로 점토 획득  
            if (Game1.random.NextDouble() \< 0.25)  
            {  
                Item clay \= ItemRegistry.Create("(O)330"); // 점토(Clay)의 ID  
                Game1.createItemDebris(clay, toolTile \* 64f, who.FacingDirection, location);  
                location.playSound("digup");  
            }  
            else  
            {  
                location.playSound("woodyHit");  
            }

            return true;  
        }  
    }  
}

3. **Content Patcher 설정 (content.json):**  
   * 모드 폴더에 content.json 파일을 만들고 아래 내용을 추가합니다. 이 파일은 도구의 데이터, 텍스처, 그리고 대장간 판매 목록을 정의합니다.

JSON  
{  
    "Format": "2.0.0",  
    "Changes":  
                }  
            }  
        }  
    \]  
}

4. **에셋 준비:**  
   * 모드 폴더 내에 assets 폴더를 생성합니다.  
   * 16x16 픽셀 크기의 tools.png 이미지 파일을 만들어 assets 폴더에 넣습니다. 이 이미지가 게임 내에서 도구의 아이콘이 됩니다.

이 네 단계를 완료하면, 플레이어는 대장간에서 '표본 채집기'를 구매하여 땅에서 점토를 채집하는 새로운 상호작용을 경험할 수 있습니다. 이 워크스루는 커스텀 도구 제작의 전체 과정을 압축하여 보여주며, 개발자가 자신만의 독창적인 도구를 만드는 출발점으로 삼을 수 있습니다.

---

## **섹션 2: 콘텐츠 프레임워크 구축 \- CustomCritters 모드 해부**

이 섹션에서는 CustomCritters 모드를 분석하여 프레임워크 모드의 설계 원칙과 구현 방법을 탐구합니다.1 프레임워크 모드는 C\# 프로그래밍 지식이 없는 다른 모더(콘텐츠 제작자)들이 간단한 JSON 파일과 이미지 에셋만으로 게임에 새로운 콘텐츠를 추가할 수 있도록 강력한 기반 엔진을 제공합니다.

### **2.1. 프레임워크 설계 철학: 엔진과 콘텐츠의 분리**

프레임워크 모드의 핵심 아키텍처는 '관심사의 분리(Separation of Concerns)' 원칙에 기반합니다. 즉, 기능의 핵심 로직을 담당하는 '엔진'과, 실제 게임에 표시될 구체적인 데이터를 담은 '콘텐츠'를 명확하게 분리하는 것입니다.7

CustomCritters 모드에서 이 원칙은 다음과 같이 구현됩니다.

* **엔진 (CustomCritters.dll):** 이 C\# 라이브러리는 특정 데이터 형식(critter.json)을 이해하고 처리하는 범용 엔진 역할을 합니다. 콘텐츠 팩을 감지하고, JSON 데이터를 파싱하며, 정의된 규칙에 따라 게임 월드에 크리터를 생성(spawn), 애니메이션, 렌더링하는 모든 복잡한 로직을 담당합니다.  
* **콘텐츠 팩 (하위 폴더들):** 아티스트나 디자이너와 같은 콘텐츠 제작자들은 CustomCritters/Critters 폴더 내에 자신만의 하위 폴더를 만듭니다.11 각 폴더에는 크리터의 외형을 정의하는  
  critter.png 이미지와, 언제 어디서 어떻게 나타날지를 정의하는 critter.json 파일이 포함됩니다.

이러한 분리 구조는 여러 가지 중요한 이점을 가집니다.

* **진입 장벽 완화:** 아티스트와 디자이너는 복잡한 C\# 코드를 배울 필요 없이 자신의 창작물을 게임에 추가할 수 있습니다.  
* **모듈성 및 생태계:** 수많은 콘텐츠 팩들이 독립적으로 개발되고 공유될 수 있는 모듈식 생태계가 조성됩니다. CustomCritters를 필요로 하는 수많은 모드들이 이를 증명합니다.12  
* **호환성 향상:** 핵심 로직이 중앙 엔진에 집중되어 있으므로, 게임 업데이트 시 프레임워크 모드만 업데이트되면 이를 사용하는 모든 콘텐츠 팩이 계속해서 작동할 가능성이 높습니다.

이러한 설계는 CustomCritters가 단순한 모드가 아니라, 다른 창작자들을 위한 하나의 플랫폼이자 개발 도구로 기능하게 만듭니다. 이는 모딩 커뮤니티의 사회적 역학 관계에 근본적인 변화를 가져왔습니다. C\# 프로그래머(프레임워크 저자)와 콘텐츠 제작자(아티스트, 디자이너) 간의 공생 관계가 형성된 것입니다. 이러한 노동의 분업은 모든 모더가 코딩과 에셋 제작 모두에 능숙해야 할 때보다 훨씬 더 많은 양과 다양성의 모드를 탄생시켰습니다. 동시에 이는 하나의 프레임워크 모드의 안정성에 수많은 콘텐츠 팩의 운명이 좌우되는 '의존성 계층'을 형성하기도 합니다.

### **2.2. critter.json 스키마: 콘텐츠 팩의 DNA**

critter.json 파일은 CustomCritters 프레임워크의 공개 API 역할을 하며, 콘텐츠 제작자가 엔진과 소통하는 유일한 수단입니다. 이 파일의 구조와 각 필드의 의미를 정확히 이해하는 것이 콘텐츠 팩 제작의 핵심입니다. 아래는 critter.json의 주요 필드를 기능별로 그룹화하여 상세히 설명한 것입니다.13

* **외형 (Appearance):** 크리터의 기본적인 시각적 속성을 정의합니다.  
  * Id: 크리터 유형을 식별하는 고유한 문자열입니다. 충돌을 피하기 위해 \[작가이름\].\[크리터이름\] 형식을 권장합니다.  
  * Texture: critter.json 파일 기준 상대 경로로 된 스프라이트 시트 이미지 파일명입니다.  
  * FrameWidth, FrameHeight: 스프라이트 시트 내 단일 애니메이션 프레임의 너비와 높이(픽셀 단위)입니다.  
  * Scale: 게임 내에서 크리터가 표시될 크기 배율입니다. 기본값은 4입니다.  
* **애니메이션 (Animation):** 크리터의 움직임을 시각적으로 표현하는 방법을 정의합니다.  
  * Animation 블록 내에 프레임 시퀀스를 정의합니다. 각 항목은 Frame(스프라이트 시트 내 프레임 번호, 0부터 시작)과 Duration(해당 프레임이 지속되는 시간, 밀리초 단위로 추정)으로 구성됩니다.  
* **생성 조건 (Spawning Conditions):** 크리터가 언제, 어디서 나타날지를 결정하는 규칙입니다.  
  * Seasons: 생성이 허용되는 계절의 배열입니다 (예: \["spring", "summer"\]).  
  * Locations: 생성이 허용되는 야외 장소 이름의 배열입니다. 바닐라 및 모드로 추가된 장소 모두 가능합니다.  
  * MinTimeOfDay, MaxTimeOfDay: 생성이 가능한 시간 범위입니다 (예: 1800 \~ 2600).  
  * ChancePerTile: 유효한 타일 하나당 생성 시도 시 크리터가 나타날 확률입니다.  
  * RequireDarkOut, AllowRain: 각각 어두울 때만 생성될지, 비 오는 날에도 생성될지를 결정하는 불리언 값입니다.  
* **행동 (Behavior):** 크리터의 이동 패턴을 정의합니다.  
  * Behavior 블록 내에 여러 이동 명령을 배열로 정의합니다. 각 명령은 Type (예: move, pause)과 관련 파라미터(예: X, Y, Duration)로 구성됩니다.  
* **조명 (Light):** 크리터가 빛을 발산하도록 설정합니다.  
  * Light 블록 내에 Radius, Color, Pulse 등의 속성을 정의하여 반딧불이와 같은 효과를 만들 수 있습니다.

이 스키마를 체계적으로 정리한 것이 아래의 **표 2.1**입니다. 이 표는 콘텐츠 제작자가 critter.json을 작성할 때 필요한 모든 정보를 한눈에 파악할 수 있도록 돕는 핵심 참조 자료가 될 것입니다.

#### **표 2.1: critter.json 필드 참조**

| 필드명 | 데이터 타입 | 설명 | 예시 값 |
| :---- | :---- | :---- | :---- |
| Id | string | 크리터 유형의 고유 식별자. | "mynamespace.firefly" |
| Texture | string | critter.json 파일에 대한 상대 경로의 스프라이트 시트. | "critter.png" |
| FrameWidth | int | 단일 애니메이션 프레임의 너비 (픽셀). | 16 |
| FrameHeight | int | 단일 애니메이션 프레임의 높이 (픽셀). | 16 |
| Scale | float | 게임 내 크리터의 크기 배율. | 4.0 |
| Animation | object | 프레임과 지속 시간으로 구성된 애니메이션 시퀀스. | \`\` |
| Seasons | string | 크리터가 생성될 수 있는 계절의 배열. | \["spring", "summer"\] |
| Locations | string | 크리터가 생성될 수 있는 장소의 배열. | \["Forest", "Farm"\] |
| MinTimeOfDay | int | 생성 가능한 최소 시간 (24시간 형식, 예: 1800은 오후 6시). | 600 |
| MaxTimeOfDay | int | 생성 가능한 최대 시간. | 2600 |
| ChancePerTile | double | 타일당 생성 확률 (0.0 \~ 1.0). | 0.005 |
| Behavior | object | 이동, 정지 등 행동 패턴을 정의하는 명령 배열. | \`\` |
| Light | object | 크리터의 광원 효과를 정의하는 객체. | {"Radius": 1.0, "Color": "255 255 0", "Pulse": true} |

### **2.3. C\# 구현: 콘텐츠 팩 로딩 및 파싱**

CustomCritters 엔진의 첫 번째 임무는 게임이 시작될 때 모든 콘텐츠 팩을 발견하고 그 내용을 메모리로 읽어들이는 것입니다. 이 과정은 SMAPI의 GameLaunched 이벤트를 통해 트리거되는 것이 일반적입니다.

엔진의 로딩 로직은 다음과 같은 단계로 이루어집니다.

1. **콘텐츠 팩 탐색:** 모드는 자신의 설치 폴더 내에 있는 /Critters 디렉토리를 스캔하여 모든 하위 디렉토리를 찾습니다. 각 하위 디렉토리는 하나의 독립적인 콘텐츠 팩으로 간주됩니다.  
2. **critter.json 유효성 검사 및 읽기:** 각 하위 디렉토리에서 critter.json 파일의 존재 여부를 확인합니다. 파일이 존재하면, 파일의 전체 내용을 문자열로 읽어들입니다.  
3. **JSON 역직렬화 (Deserialization):** 읽어들인 JSON 문자열을 C\# 객체로 변환합니다. 이를 위해 Newtonsoft.Json과 같은 라이브러리를 사용하며, critter.json의 구조를 그대로 반영하는 C\# 모델 클래스(예: CritterData)를 미리 정의해 두어야 합니다.14 이  
   CritterData 클래스는 Id, Texture, FrameWidth, Seasons 등 JSON의 모든 필드를 속성으로 가집니다.  
4. **텍스처 로딩:** CritterData 객체에 명시된 Texture 파일 경로를 사용하여 SMAPI의 IModHelper.ModContent.Load\<Texture2D\>() 메서드를 통해 critter.png 이미지를 Texture2D 객체로 로드합니다.  
5. **데이터 저장:** 성공적으로 파싱된 CritterData 객체와 로드된 Texture2D 객체를 쌍으로 묶어, 엔진이 언제든지 접근할 수 있는 전역 리스트나 딕셔너리(예: List\<LoadedCritterPack\>)에 저장합니다.

다음은 이 과정을 간략하게 표현한 C\# 코드의 개념적 예시입니다. 이는 Json Assets 모드가 콘텐츠 팩을 로드하는 방식과 유사한 원리를 따릅니다.15

C\#

// ModEntry.cs 내  
private List\<LoadedCritterPack\> CritterPacks \= new List\<LoadedCritterPack\>();

private void OnGameLaunched(object sender, GameLaunchedEventArgs e)  
{  
    // 1\. 콘텐츠 팩 탐색  
    string critterPacksPath \= Path.Combine(this.Helper.DirectoryPath, "Critters");  
    if (\!Directory.Exists(critterPacksPath)) return;

    foreach (var dir in Directory.GetDirectories(critterPacksPath))  
    {  
        try  
        {  
            // 2\. critter.json 읽기  
            string jsonPath \= Path.Combine(dir, "critter.json");  
            if (\!File.Exists(jsonPath)) continue;  
              
            // 3\. JSON 역직렬화  
            CritterData data \= this.Helper.Data.ReadJsonFile\<CritterData\>(Path.GetRelativePath(this.Helper.DirectoryPath, jsonPath));  
            if (data \== null) continue;

            // 4\. 텍스처 로딩  
            string texturePath \= Path.Combine(Path.GetDirectoryName(jsonPath), data.Texture);  
            Texture2D texture \= this.Helper.ModContent.Load\<Texture2D\>(Path.GetRelativePath(this.Helper.DirectoryPath, texturePath));

            // 5\. 데이터 저장  
            CritterPacks.Add(new LoadedCritterPack(data, texture));  
            this.Monitor.Log($"Loaded critter pack: {data.Id}", LogLevel.Info);  
        }  
        catch (Exception ex)  
        {  
            this.Monitor.Log($"Failed to load critter pack in {dir}.\\n{ex}", LogLevel.Error);  
        }  
    }  
}

// 데이터를 저장할 도우미 클래스  
public class LoadedCritterPack  
{  
    public CritterData Data { get; }  
    public Texture2D Texture { get; }

    public LoadedCritterPack(CritterData data, Texture2D texture)  
    {  
        this.Data \= data;  
        this.Texture \= texture;  
    }  
}

// critter.json에 매핑될 모델 클래스  
public class CritterData  
{  
    public string Id { get; set; }  
    public string Texture { get; set; }  
    //... 나머지 모든 필드  
}

이 과정을 통해 엔진은 게임 시작 시 모든 콘텐츠 팩의 정보를 구조화된 데이터로 변환하여 메모리에 준비시키고, 다음 단계인 생성 엔진에서 이 데이터를 활용할 수 있게 됩니다.

### **2.4. 생성 엔진: 크리터에 생명 불어넣기**

콘텐츠 팩 로딩이 완료되면, CustomCritters의 핵심 엔진은 게임의 특정 시점마다 어떤 크리터를 어디에, 얼마나 자주 생성할지 결정해야 합니다. 이 생성 로직은 게임의 몰입감과 성능에 직접적인 영향을 미치는 매우 중요한 부분입니다.

생성 엔진은 주로 SMAPI의 GameLoop.UpdateTicked 이벤트에 연결되어, 매 게임 틱(1/60초)마다 실행됩니다. 하지만 성능을 고려하여 매 틱마다 실행하기보다는, 1초에 한 번 또는 특정 조건이 만족될 때만 실행되도록 조절하는 것이 일반적입니다.

생성 엔진의 작동 방식은 다음과 같습니다.

1. **현재 상태 확인:** 현재 플레이어의 위치(Game1.currentLocation), 게임 내 시간(Game1.timeOfDay), 계절(Game1.season), 날씨(Game1.isRaining) 등 현재 게임 상태 정보를 가져옵니다.  
2. **생성 조건 평가:** 메모리에 로드된 모든 CritterData 객체(CritterPacks 리스트)를 순회합니다. 각 크리터에 대해, critter.json에 정의된 생성 조건(Locations, Seasons, MinTimeOfDay 등)이 현재 게임 상태와 일치하는지 확인합니다.  
3. **생성 확률 계산:** 조건이 일치하는 크리터에 대해, 생성 확률(ChancePerTile)에 기반한 무작위 확률 검사를 수행합니다. 예를 들어, Game1.random.NextDouble() \< critter.Data.ChancePerTile와 같은 코드를 사용하여 생성 여부를 결정합니다. 이 검사는 맵의 특정 타일 수만큼 반복하거나, 정해진 SpawnAttempts 횟수만큼 시도될 수 있습니다.13  
4. **생성 위치 결정:** 확률 검사를 통과하면, 크리터를 생성할 구체적인 위치를 결정합니다. Game1.currentLocation의 타일 중에서 무작위로 유효한 타일(예: 장애물이 없는 땅)을 선택합니다.  
5. **크리터 인스턴스화 및 추가:** 선택된 위치에 새로운 Critter 객체 인스턴스를 생성합니다. 이 때 CritterData와 Texture2D 정보를 생성자에 전달하여 크리터의 외형과 행동 패턴을 설정합니다. 마지막으로, 생성된 크리터 객체를 현재 위치의 크리터 리스트인 Game1.currentLocation.critters.Add()에 추가합니다. 이렇게 추가된 크리터는 다음 프레임부터 게임 월드에 나타나고 움직이기 시작합니다.

이러한 생성 로직은 Farm Type Manager와 같은 다른 모드에서 사용되는 커스텀 생성 로직과 개념적으로 유사합니다. 둘 다 특정 위치, 확률, 지형 유형에 따라 게임 객체를 동적으로 추가하는 원리를 공유합니다.16

### **2.5. 렌더링 및 행동: Critter 클래스**

게임 월드에 생성된 각각의 크리터는 StardewValley.Critter 클래스를 상속받는 커스텀 Critter 클래스의 인스턴스입니다. 이 클래스는 개별 크리터의 실시간 동작을 책임집니다.

Critter 클래스의 주요 메서드는 다음과 같습니다.

* **update(GameTime time, GameLocation location):** 이 메서드는 게임 루프에 의해 매 프레임마다 호출됩니다. 이 안에서 크리터의 현재 위치를 critter.json의 Behavior 규칙에 따라 업데이트하고, 애니메이션 타이머를 관리하여 현재 표시해야 할 스프라이트 프레임을 결정합니다. 예를 들어, 'move' 행동이 진행 중이라면 지정된 방향과 속도로 position 벡터를 변경하고, 'pause' 행동 중이라면 타이머만 감소시킵니다.  
* **draw(SpriteBatch b):** 이 메서드 역시 매 프레임마다 호출되며, update에서 결정된 현재 애니메이션 프레임을 화면에 그리는 역할을 합니다. critter.png 텍스처, 현재 프레임에 해당하는 소스 사각형(source rectangle), 화면상의 위치, 그리고 Scale 값을 사용하여 SpriteBatch.Draw()를 호출합니다.

이 두 메서드의 긴밀한 협력을 통해, JSON 파일에 정의된 추상적인 데이터(애니메이션과 행동 규칙)가 게임 화면 위에서 살아 움직이는 구체적인 크리터로 표현됩니다. 이는 데이터 기반 설계를 통해 어떻게 동적인 게임 요소를 만들어낼 수 있는지를 보여주는 훌륭한 예시입니다.12

---

## **섹션 3: 고급 게임 수정 \- Harmony와 NPC 로직**

이 섹션에서는 스타듀밸리 모딩의 가장 강력하면서도 복잡한 기술인 Harmony 라이브러리를 사용한 직접적인 게임 코드 패치를 다룹니다. CustomNPCFixes나 CustomNPCExclusions와 같은 모드가 해결하는 문제들을 사례로 들어, 표준 SMAPI API만으로는 해결할 수 없는 경우 왜, 그리고 어떻게 Harmony를 사용해야 하는지 탐구합니다.18

### **3.1. SMAPI 이벤트가 충분하지 않을 때: Harmony의 필요성**

SMAPI는 게임 내 다양한 시점에 코드를 실행할 수 있도록 강력하고 안정적인 이벤트 시스템을 제공합니다.21 예를 들어,

GameLoop.DayStarted 이벤트는 매일 아침, Input.ButtonPressed 이벤트는 플레이어가 키를 누를 때마다 코드를 실행할 수 있게 해줍니다. 대부분의 모드는 이 이벤트 시스템만으로도 충분히 원하는 기능을 구현할 수 있습니다.

하지만 SMAPI가 모든 것을 포괄할 수는 없습니다. 게임의 특정 로직이 외부에서 수정할 수 없는 비공개(private) 메서드 내부에 깊숙이 하드코딩되어 있거나, 해당 로직의 실행 전후에 개입할 수 있는 이벤트가 존재하지 않는 경우가 있습니다. 예를 들어, 겨울 별의 만찬 축제에서 선물 교환 대상을 무작위로 선택하는 로직은 Utility.getRandomTownNPC()와 같은 단일 메서드 내부에 캡슐화되어 있을 수 있으며, SMAPI는 이 선택 과정에 직접 개입할 수 있는 이벤트를 제공하지 않을 수 있습니다.

이러한 '모딩의 사각지대'를 해결하기 위해 등장한 것이 Harmony입니다.22 Harmony는 C\#의 리플렉션(reflection)과 런타임 코드 생성 기술을 이용하여, 이미 컴파일된 게임의 C\# 메서드를 실행 중에 직접 수정하거나 대체할 수 있게 해주는 라이브러리입니다. 즉, 게임의 소스 코드 없이도 메서드의 실행 흐름을 바꾸거나, 새로운 코드를 주입하는 것이 가능해집니다.

그러나 이 강력함에는 큰 책임이 따릅니다. Harmony 패치는 매우 불안정할 수 있으며, 다른 모드와의 충돌, 진단하기 어려운 미묘한 버그, 심지어 메모리 손상 오류까지 유발할 수 있습니다.22 따라서 Harmony는 SMAPI 이벤트나 다른 API로는 도저히 해결할 수 없는 문제에 대한 '최후의 수단'으로 사용되어야 합니다.

### **3.2. Harmony 패치의 해부: Prefix, Postfix, 그리고 State**

Harmony를 이용한 메서드 패치는 주로 세 가지 유형으로 나뉩니다: Prefix, Postfix, 그리고 Transpiler. 이 중 Prefix와 Postfix가 가장 일반적으로 사용되며, 이 둘의 조합으로 대부분의 문제를 해결할 수 있습니다.23

* **Prefix (접두사):** 원본 메서드가 실행되기 *직전*에 실행되는 패치입니다.  
  * **주요 용도:** 원본 메서드로 전달되는 인자(argument)를 검사하거나 수정, 특정 조건에서 원본 메서드의 실행 자체를 건너뛰기, 그리고 Postfix에서 사용할 상태(state) 정보를 미리 저장하는 데 사용됩니다.  
  * **실행 제어:** bool을 반환하도록 작성하여, false를 반환하면 원본 메서드와 그 이후의 Prefix들의 실행을 막을 수 있습니다.  
* **Postfix (접미사):** 원본 메서드가 실행된 *직후*에 실행되는 패치입니다.  
  * **주요 용도:** 원본 메서드의 반환값(return value)을 읽거나 수정, 원본 메서드 실행 후에 추가적인 로직(부수 효과)을 수행, Prefix에서 저장한 상태 정보를 활용하는 데 사용됩니다.  
  * **호환성:** 일반적으로 원본 메서드의 실행을 막지 않기 때문에 Prefix보다 다른 모드와의 호환성이 더 높습니다.

이 두 패치 유형은 특별한 이름의 인자를 통해 원본 메서드의 컨텍스트에 접근할 수 있습니다.

* \_\_instance: 원본 메서드가 속한 클래스의 인스턴스에 접근합니다 (정적 메서드가 아닌 경우).  
* \_\_result: 원본 메서드의 반환값에 접근합니다. Postfix에서 ref 키워드와 함께 사용하면 반환값을 수정할 수 있습니다.  
* \_\_state: Prefix와 Postfix 간에 데이터를 전달하기 위한 통로입니다. Prefix에서 out으로 선언된 \_\_state 변수에 값을 할당하면, Postfix에서 in으로 선언된 동일한 이름의 \_\_state 변수로 그 값을 받을 수 있습니다.23

이러한 구성 요소들을 이해하는 것은 효과적이고 안정적인 Harmony 패치를 작성하는 데 필수적입니다. 아래 **표 3.1**은 각 패치 유형의 특징을 비교하여 개발자가 상황에 맞는 최적의 패치 유형을 선택하는 데 도움을 줍니다.

#### **표 3.1: Harmony 패치 유형 비교**

| 패치 유형 | 실행 순서 | 주요 사용 사례 | 원본 실행 건너뛰기 | 주요 인자 |
| :---- | :---- | :---- | :---- | :---- |
| **Prefix** | 원본 이전 | 메서드 인자 수정; 조건부 원본 실행 건너뛰기; 실행 전 상태 저장. | 가능 ( false 반환 시) | \_\_instance, 메서드 매개변수, ref 매개변수, out \_\_state |
| **Postfix** | 원본 이후 | 반환값 읽기 또는 수정; 원본 로직 후 코드 실행; Prefix 상태 사용. | 불가능 | \_\_instance, \_\_result ( ref 가능), in \_\_state |
| **Transpiler** | JIT 컴파일 이전 | 메서드의 CIL 명령어를 직접 저수준으로 수정. (극도의 주의 필요). | 해당 없음 (메서드 자체를 수정) | IEnumerable\<CodeInstruction\>, ILGenerator |

### **3.3. 사례 연구: NPC 선택 로직 패치하기**

이제 실제 문제를 해결하기 위해 Harmony 패치를 어떻게 적용하는지 구체적인 사례를 통해 살펴보겠습니다. 바닐라 게임의 Utility.getRandomTownNPC() 메서드가 모드로 추가된 커스텀 NPC를 결과에 포함하지 않는 문제를 해결하는 것이 목표입니다.

**문제점:** Utility.getRandomTownNPC()는 게임의 기본 NPC 목록만을 기반으로 무작위 NPC를 반환할 수 있습니다. 이로 인해 아이템 배달 퀘스트나 축제 이벤트에서 커스텀 NPC가 완전히 배제되는 문제가 발생합니다.20

**해결책 (Postfix 패치 사용):** 이 문제는 원본 메서드가 실행된 *후* 그 결과를 수정하는 것이 가장 안전하고 효율적이므로 Postfix 패치가 적합합니다.

1. **패치 대상 지정:** 패치할 대상은 StardewValley.Utility 클래스의 getRandomTownNPC 메서드입니다.  
2. **Postfix 메서드 작성:** 원본 메서드의 반환값을 수정해야 하므로, \_\_result 인자를 ref 키워드와 함께 받습니다.  
   C\#  
   // Patches.cs  
   using StardewValley;  
   using HarmonyLib;

   public static class UtilityPatches  
   {  
       // 이 메서드는 ModEntry에서 호출되어 Monitor 인스턴스를 설정합니다.  
       // 오류 로깅을 위해 필요합니다.  
       internal static IMonitor Monitor;  
       internal static void Initialize(IMonitor monitor)  
       {  
           Monitor \= monitor;  
       }

       public static void GetRandomTownNPC\_Postfix(ref NPC \_\_result, bool includeKids \= false)  
       {  
           try  
           {  
               // 원본 메서드가 반환한 NPC가 특정 조건을 만족하지 않거나,  
               // 더 나은 선택(예: 커스텀 NPC 포함)을 하고 싶을 때 로직을 추가합니다.

               // 예시: 50% 확률로 결과를 모든 캐릭터 중에서 다시 선택하도록 강제  
               if (Game1.random.NextDouble() \< 0.5)  
               {  
                   var allCharacters \= Utility.getAllCharacters();  
                   // 배제하고 싶은 NPC (예: 드워프, 크로버스 등)를 필터링합니다.  
                   var validCharacters \= allCharacters.Where(npc \=\> npc.isVillager() && npc.CanSocialize).ToList();

                   if (validCharacters.Any())  
                   {  
                       // 새로운 NPC를 무작위로 선택하여 반환값을 덮어씁니다.  
                       \_\_result \= validCharacters\[Game1.random.Next(validCharacters.Count)\];  
                   }  
               }  
           }  
           catch (Exception ex)  
           {  
               Monitor.Log($"Failed in {nameof(GetRandomTownNPC\_Postfix)}:\\n{ex}", LogLevel.Error);  
           }  
       }  
   }

3. **패치 적용:** 모드의 Entry 메서드에서 Harmony 인스턴스를 생성하고, 작성한 Postfix 메서드를 원본 메서드에 적용합니다.  
   C\#  
   // ModEntry.cs  
   using HarmonyLib;  
   using System.Reflection;

   public class ModEntry : Mod  
   {  
       public override void Entry(IModHelper helper)  
       {  
           // 패치 클래스에 Monitor 인스턴스 전달  
           UtilityPatches.Initialize(this.Monitor);

           var harmony \= new Harmony(this.ModManifest.UniqueID);

           harmony.Patch(  
               original: AccessTools.Method(typeof(Utility), nameof(Utility.getRandomTownNPC)),  
               postfix: new HarmonyMethod(typeof(UtilityPatches), nameof(UtilityPatches.GetRandomTownNPC\_Postfix))  
           );  
       }  
   }

이 패치를 통해 Utility.getRandomTownNPC()가 호출될 때마다, 우리의 Postfix 코드가 실행되어 그 결과를 검토하고 필요에 따라 커스텀 NPC를 포함한 더 넓은 범위의 캐릭터 중에서 새로운 결과를 선택하여 반환하게 됩니다. 이는 게임의 핵심 로직을 직접 수정하지 않고도 그 동작을 확장하는 강력한 방법을 보여줍니다.23

### **3.4. 대안: CustomNPCExclusions와 같은 데이터 기반 API**

모든 모드가 각자의 필요에 따라 동일한 게임 함수를 Harmony로 패치하려고 시도한다면, 이는 충돌의 지름길이 될 수 있습니다. 한 모드의 패치가 다른 모드의 패치를 무력화시키거나, 예측 불가능한 상호작용을 일으킬 수 있기 때문입니다.

이러한 문제를 해결하기 위해 모딩 생태계가 성숙하면서 더 발전된 해결책이 등장했습니다. 바로 단일 프레임워크 모드가 필요한 Harmony 패치를 중앙에서 관리하고, 다른 모드들에게는 안전하고 사용하기 쉬운 데이터 기반 API를 제공하는 방식입니다. CustomNPCExclusions 모드가 바로 이 접근 방식의 훌륭한 예시입니다.19

CustomNPCExclusions는 내부적으로 NPC 선택과 관련된 여러 바닐라 함수들을 Harmony로 패치합니다. 그리고 Data/CustomNPCExclusions라는 새로운 데이터 에셋을 게임에 추가합니다. 다른 모드들은 Harmony를 직접 다룰 필요 없이, Content Patcher를 사용하여 이 데이터 에셋에 자신들의 NPC 이름과 배제 규칙을 추가하기만 하면 됩니다.

예를 들어, 어떤 커스텀 NPC를 겨울 별의 만찬 이벤트에서 제외하고 싶다면, 해당 NPC 모드의 content.json에 다음과 같은 항목을 추가하면 됩니다.

JSON

{  
    "Action": "EditData",  
    "Target": "Data/CustomNPCExclusions",  
    "Entries": {  
        "MyCustomNPCName": "WinterStar"  
    }  
}

이 방식은 다음과 같은 장점을 가집니다.

* **안전성 및 안정성:** 콘텐츠 팩 제작자는 위험한 Harmony 코드를 다룰 필요가 없습니다.  
* **호환성:** 모든 NPC 제외 로직이 중앙 프레임워크를 통해 관리되므로 모드 간 충돌 가능성이 현저히 줄어듭니다.  
* **단순성:** 간단한 JSON 항목 추가만으로 복잡한 로직 제어가 가능합니다.

이러한 접근 방식의 등장은 모딩 커뮤니티가 확장성과 호환성이라는 문제에 어떻게 대응해왔는지를 보여줍니다. 초기에는 각 모드가 개별적으로 문제를 해결하는 '서부 개척 시대'와 같은 방식이 주를 이루었다면, 점차 프레임워크 저자가 '관리자' 역할을 하며 공통 문제에 대한 안정적이고 중앙화된 해결책을 제공하는 보다 구조화되고 협력적인 모델로 발전한 것입니다. 이는 개별적인 해결책에서 커뮤니티 인프라로의 전환을 의미하며, 성숙한 소프트웨어 생태계의 특징을 보여줍니다.

---

## **결론**

이 가이드는 spacechase0의 세 가지 핵심적인 모딩 사례를 통해 스타듀밸리 모딩의 고급 아키텍처를 심층적으로 분석했습니다. 우리는 단일 기능을 가진 독립적인 커스텀 도구(BugNet) 제작에서 시작하여, 다른 창작자들을 위한 확장 가능한 플랫폼(CustomCritters)을 구축하는 방법, 그리고 최종적으로 게임의 핵심 코드를 직접 수정하는 Harmony 패치 기술에 이르기까지, 모딩의 복잡성이 점진적으로 심화되는 과정을 탐구했습니다.

BugNet 사례는 단순한 데이터 추가를 넘어, C\# 코드를 통해 '포획'이라는 새로운 플레이어 '동사'를 창조함으로써 게임플레이 자체를 확장하는 방법을 보여주었습니다. 이는 모더가 게임의 상태를 직접 조회하고 수정하는 능동적인 역할을 수행해야 함을 의미하며, 콘텐츠 모딩에서 시스템 모딩으로 나아가는 중요한 전환점을 제시했습니다.

CustomCritters는 엔진과 콘텐츠를 분리하는 프레임워크 설계의 정수를 보여주었습니다. 이 접근 방식은 C\# 개발자와 콘텐츠 제작자 간의 협업 생태계를 조성하여 모딩 커뮤니티의 생산성과 다양성을 폭발적으로 증가시켰습니다. 이는 잘 설계된 API(critter.json)가 어떻게 기술적 장벽을 낮추고 창의성을 촉진할 수 있는지에 대한 강력한 증거입니다.

마지막으로, Harmony와 NPC 로직에 대한 탐구는 모딩의 가장 깊은 수준을 다루었습니다. 우리는 SMAPI API의 한계를 인지하고, Harmony를 사용하여 게임의 하드코딩된 로직에 직접 개입하는 방법을 배웠습니다. 더 나아가, CustomNPCExclusions와 같은 API 기반 프레임워크의 등장은 모딩 커뮤니티가 개별적인 패치의 충돌 문제를 해결하고, 보다 안정적이고 협력적인 개발 환경으로 성숙해가는 과정을 명확히 보여주었습니다.

결론적으로, 이 세 가지 사례는 고립된 기술이 아니라, 한 명의 개발자가 마스터할 수 있는 다재다능한 툴킷의 일부입니다. 효과적인 모딩은 단순히 코드를 작성하는 것을 넘어, 문제에 가장 적합한 접근 방식을 선택하는 것에서 시작됩니다. 때로는 독립적인 기능 구현이, 때로는 확장 가능한 프레임워크 구축이, 그리고 때로는 정밀한 코드 수정이 필요합니다. 이 가이드가 제공한 심층적인 분석과 코드 예시들이 개발자 여러분이 자신만의 창의적인 아이디어를 안정적이고, 호환성이 높으며, 커뮤니티 전체를 풍요롭게 하는 훌륭한 모드로 구현하는 데 든든한 발판이 되기를 바랍니다.

#### **참고 자료**

1. spacechase0/StardewValleyMods: New home for my ... \- GitHub, 9월 16, 2025에 액세스, [https://github.com/spacechase0/StardewValleyMods](https://github.com/spacechase0/StardewValleyMods)  
2. Bug Net \- spacechase0's Site, 9월 16, 2025에 액세스, [https://spacechase0.com/mods/stardew-valley/bug-net/](https://spacechase0.com/mods/stardew-valley/bug-net/)  
3. Modding:Tools \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Tools](https://stardewvalleywiki.com/Modding:Tools)  
4. Modding:Modder Guide/Get Started \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/Get\_Started](https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started)  
5. Modding Guides and General Modding Discussion--REDUX | Chucklefish Forums, 9월 16, 2025에 액세스, [https://community.playstarbound.com/threads/modding-guides-and-general-modding-discussion-redux.109131/](https://community.playstarbound.com/threads/modding-guides-and-general-modding-discussion-redux.109131/)  
6. Modding:Items \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Items](https://stardewvalleywiki.com/Modding:Items)  
7. Modding:Content pack frameworks \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Content\_pack\_frameworks](https://stardewvalleywiki.com/Modding:Content_pack_frameworks)  
8. Modding:Content packs \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Content\_packs](https://stardewvalleywiki.com/Modding:Content_packs)  
9. Bug Net | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/771693](https://www.moddrop.com/stardew-valley/mods/771693)  
10. Custom Critters \- Stardew Valley Mods \- CurseForge, 9월 16, 2025에 액세스, [https://www.curseforge.com/stardewvalley/mods/custom-critters](https://www.curseforge.com/stardewvalley/mods/custom-critters)  
11. Help installing Custom Critters mod? \- Chucklefish Forums, 9월 16, 2025에 액세스, [https://community.playstarbound.com/threads/help-installing-custom-critters-mod.138156/](https://community.playstarbound.com/threads/help-installing-custom-critters-mod.138156/)  
12. More Wildlife \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/More\_Wildlife](https://stardewmodding.wiki.gg/wiki/More_Wildlife)  
13. SDV PC Modding: Configuring Custom Critters – PixelHag ..., 9월 16, 2025에 액세스, [https://pixelhag.com/2024/04/05/sdv-pc-modding-configuring-custom-critters/](https://pixelhag.com/2024/04/05/sdv-pc-modding-configuring-custom-critters/)  
14. Read and parse a Json File in C\# \- Stack Overflow, 9월 16, 2025에 액세스, [https://stackoverflow.com/questions/13297563/read-and-parse-a-json-file-in-c-sharp](https://stackoverflow.com/questions/13297563/read-and-parse-a-json-file-in-c-sharp)  
15. Create a JA content-pack in SMAPI mod \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/Create\_a\_JA\_content-pack\_in\_SMAPI\_mod](https://stardewmodding.wiki.gg/wiki/Create_a_JA_content-pack_in_SMAPI_mod)  
16. Esca-MMC/FarmTypeManager: A mod for the game Stardew Valley, allows players and modders to spawn customizable features from each of Stardew's farm types. Requires the SMAPI mod loader. \- GitHub, 9월 16, 2025에 액세스, [https://github.com/Esca-MMC/FarmTypeManager](https://github.com/Esca-MMC/FarmTypeManager)  
17. Modding:Maps \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Maps](https://stardewvalleywiki.com/Modding:Maps)  
18. Custom NPC Fixes | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/771669-custom-npc-fixes](https://www.moddrop.com/stardew-valley/mods/771669-custom-npc-fixes)  
19. Custom NPC Exclusions | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/847997-custom-npc-exclusions](https://www.moddrop.com/stardew-valley/mods/847997-custom-npc-exclusions)  
20. Esca-MMC/CustomNPCExclusions: A mod for the game ... \- GitHub, 9월 16, 2025에 액세스, [https://github.com/Esca-MMC/CustomNPCExclusions](https://github.com/Esca-MMC/CustomNPCExclusions)  
21. Modding:Modder Guide/APIs/Events \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/APIs/Events](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Events)  
22. Modding:Modder Guide/APIs/Harmony \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/APIs/Harmony](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Harmony)  
23. Tutorial: Harmony Patching \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/Tutorial:\_Harmony\_Patching](https://stardewmodding.wiki.gg/wiki/Tutorial:_Harmony_Patching)