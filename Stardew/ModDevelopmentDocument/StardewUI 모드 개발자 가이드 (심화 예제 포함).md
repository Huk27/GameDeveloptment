

# **StardewUI를 활용한 스타듀밸리 모드 UI 개발 가이드 (심화 예제 포함)**

## **서론: 왜 StardewUI를 사용해야 할까요?**

스타듀밸리 모드를 만들다 보면 필연적으로 커스텀 메뉴나 정보창 같은 사용자 인터페이스(UI)를 만들어야 할 때가 옵니다. 기존 SMAPI API를 사용해 UI를 만드는 것은 모든 요소의 위치를 직접 계산하고, 화면에 그리고, 데이터를 일일이 업데이트하는 고된 작업이었습니다. 코드는 길어지고, UI 구조를 파악하기 어려워 유지보수도 힘들어집니다.

StardewUI는 바로 이 문제를 해결하기 위해 등장한 프레임워크입니다. "UI 개발을 고된 작업이 아닌 즐거운 경험으로 만드는 것"을 목표로, 웹 개발이나 최신 앱 개발에서 사용하는 선언적(declarative) 방식을 스타듀밸리 모딩에 도입했습니다.

쉽게 말해, C\# 코드로 UI를 조립하는 대신, HTML처럼 생긴 StarML이라는 언어로 UI의 뼈대를 만들고, UI에 표시될 데이터와 기능은 C\# 코드로 따로 관리하여 서로 연결하는 방식입니다. 이를 통해 UI의 디자인과 로직이 분리되어 훨씬 깔끔하고 효율적인 개발이 가능해집니다.

이 가이드는 StardewUI를 사용하여 여러분의 모드에 멋진 UI를 추가하는 방법을 단계별로 안내합니다.

---

## **파트 I: StardewUI 시작하기**

본격적인 개발에 앞서 StardewUI의 구조를 이해하고 개발 환경을 설정하는 방법을 알아봅니다.

### **1.1. 사전 준비**

StardewUI로 개발을 시작하기 전에 다음 환경이 준비되어 있어야 합니다 :

* **SMAPI**: 스타듀밸리 모딩의 필수 도구입니다.  
* **.NET 6 SDK**: 게임이 사용하는.NET 버전입니다.  
* **IDE (통합 개발 환경)**: Visual Studio Community나 JetBrains Rider 같은 C\# 개발 도구가 필요합니다.

### **1.2. 내게 맞는 방식 선택하기: Core vs. Framework**

StardewUI는 두 가지 사용 방식을 제공하며, 프로젝트의 성격에 맞게 선택해야 합니다.

| 구분 | StardewUI Core | StardewUI Framework |
| :---- | :---- | :---- |
| **설치 방식** | 내 모드 프로젝트에 소스 코드를 직접 포함 | 별도의 모드로, 내 모드의 종속성으로 설정 |
| **사용자 설치** | 내 모드만 설치하면 됨 | 내 모드와 StardewUI Framework 모드 둘 다 설치해야 함 |
| **주요 기능** | 기본적인 UI 레이아웃 및 렌더링 기능 | Core의 모든 기능 \+ 강력한 데이터 바인딩 시스템 |
| **언제 사용할까?** | 간단한 UI를 만들거나, 사용자가 다른 모드를 추가로 설치하는 것을 원치 않을 때 | 복잡한 데이터를 실시간으로 보여주는 UI를 만들 때 (개발이 훨씬 편해짐) |

대부분의 경우, 강력한 데이터 바인딩 기능을 제공하는 \*\*StardewUI Framework\*\*를 종속성으로 추가하여 사용하는 것이 개발 효율성 면에서 더 유리합니다.

### **1.3. 나의 첫 StardewUI 화면 만들기: 3단계 핵심 과정**

StardewUI의 개발 과정은 크게 **View 생성**, **ViewModel 생성**, 그리고 **둘의 연결**이라는 세 단계로 이루어집니다. 공식 예제인 '먹을 수 있는 아이템 목록'을 통해 이 과정을 살펴보겠습니다.

#### **1단계: UI 뼈대 만들기 (View \- StarML)**

먼저 UI가 어떻게 생겼는지 정의하는 .sml 파일을 만듭니다. 이 파일은 HTML이나 XML과 유사한 StarML 언어로 작성됩니다.

**ScrollableItemGrid.sml 예시:**

XML

\<lane orientation\="vertical" horizontal-content-alignment\="middle"\>  
    \<banner background\={@Mods/StardewUI/Sprites/BannerBackground} text\={HeaderText} /\>  
    \<frame layout\="880px 640px" padding\="32,24" background\={@Mods/StardewUI/Sprites/ControlBorder}\>  
        \</frame\>  
\</lane\>

* **\<lane\>, \<frame\> 등**: UI 요소의 배치를 결정하는 태그입니다. \<lane\>은 자식 요소들을 수직(vertical)이나 수평으로 정렬합니다.  
* **orientation, layout, padding 등**: 각 태그의 모양과 속성을 정의하는 속성입니다.  
* **text={HeaderText}**: 이 부분이 핵심입니다. {} 구문은 이 텍스트가 C\# 코드(ViewModel)에 있는 HeaderText라는 속성의 값을 가져와 표시한다는 의미입니다. 이것이 바로 **데이터 바인딩**입니다.

#### **2단계: UI의 데이터와 로직 만들기 (ViewModel \- C\#)**

다음으로, UI에 표시될 실제 데이터와 로직을 담을 C\# 클래스(또는 record)를 만듭니다. 이것을 '뷰모델(ViewModel)'이라고 부릅니다.

**EdiblesViewModel.cs 예시:**

C\#

internal record EdiblesViewModel(string HeaderText, ParsedItemData Items)  
{  
    public static EdiblesViewModel LoadFromGameData()  
    {  
        // 게임 데이터에서 먹을 수 있는 아이템 카테고리 목록을 가져옵니다.  
        int edibleCategories \= { /\*... 카테고리 ID... \*/ };

        // 모든 아이템 중 먹을 수 있는 것만 필터링합니다.  
        var items \= ItemRegistry.ItemTypes  
         .Single(type \=\> type.Identifier \== ItemRegistry.type\_object)  
         .GetAllIds()  
         .Select(id \=\> ItemRegistry.GetDataOrErrorItem(id))  
         .Where(data \=\> edibleCategories.Contains(data.Category))  
         .ToArray();

        // "All Edibles"라는 제목과 필터링된 아이템 목록을 담아 반환합니다.  
        return new("All Edibles", items);  
    }  
}

* 이 코드는 게임의 아이템 등록소(ItemRegistry)에서 모든 아이템 정보를 가져와 '먹을 수 있는' 카테고리에 해당하는 아이템만 골라냅니다.  
* 최종적으로 HeaderText와 Items라는 두 속성에 데이터를 담아 EdiblesViewModel 객체를 생성합니다.

#### **3단계: View와 ViewModel 연결하고 화면에 띄우기**

마지막으로, 모드의 메인 C\# 코드에서 이 둘을 연결하고 플레이어에게 보여주는 코드를 작성합니다.

C\#

// 1\. ViewModel 인스턴스를 생성합니다.  
// LoadFromGameData() 메서드를 호출해 게임 데이터로 ViewModel을 채웁니다.  
var viewModel \= EdiblesViewModel.LoadFromGameData();

// 2\. StardewUI를 사용해 UI를 생성하고 표시합니다.  
// "내모드ID/ScrollableItemGrid"는.sml 파일의 경로입니다.  
// viewModel을 함께 전달하여 View와 ViewModel을 연결합니다.  
var ui \= new StardewUI.Menus.SomeMenu("내모드ID/ScrollableItemGrid", viewModel);

// 3\. 게임의 현재 메뉴로 설정하여 화면에 표시합니다.  
Game1.activeClickableMenu \= ui;

이것이 StardewUI의 핵심 원리입니다. **UI의 모양(SML)과 데이터/로직(C\#)을 분리**하고, 데이터 바인딩으로 연결함으로써 개발자는 더 이상 UI 요소를 수동으로 업데이트하는 코드를 작성할 필요가 없습니다.

---

## **파트 II: 실전 UI 제작: StardewPenPals 사례 분석**

기본 원리를 익혔으니, 실제 모드인 StardewPenPals의 선물 보내기 UI를 통해 StardewUI가 어떻게 복잡하고 동적인 인터페이스를 구현하는지 심층적으로 분석해 보겠습니다.2 이 모드는 우체통을 통해 주민들에게 선물을 보낼 수 있게 해주는 기능을 제공하며, 그 핵심에는

StardewUI로 제작된 정교한 UI가 있습니다.

### **2.1. 사례 연구: StardewPenPals 선물 메뉴**

PenPals의 선물 메뉴는 단순히 주민 목록을 보여주는 것을 넘어, 선물을 줄 수 있는지 여부, 이미 보낸 선물, 퀘스트, 생일 등 다양한 게임 상태에 따라 동적으로 모습이 바뀌는 복잡한 UI입니다.2

#### **ViewModel 설계: 모든 상태를 담는 그릇**

PenPals의 UI를 제어하기 위한 가상의 PenPalsViewModel은 다음과 같은 속성들을 가질 것입니다.

* IEnumerable\<NpcModel\> Npcs: 화면에 표시할 모든 NPC의 목록입니다.  
* Item CurrentHeldItem: 플레이어가 현재 손에 들고 있는 아이템입니다.  
* string SearchFilterText: 사용자가 필터 창에 입력한 텍스트입니다.  
* bool ShowBirthdaysOnly: '생일' 필터 체크 여부입니다.

여기서 중요한 점은 Npcs 목록에 들어가는 NpcModel이 단순히 NPC의 이름만 담고 있는 것이 아니라는 점입니다. 각 NPC 아이템은 UI에 필요한 모든 상태 정보를 포함해야 합니다.

**NpcModel의 가상 구조:**

C\#

public class NpcModel  
{  
    public string Name { get; set; }  
    public Texture2D Portrait { get; set; }  
    public bool CanReceiveGift { get; set; } // 주간 선물 한도, 호감도 최대치 등 체크  
    public string DisabledReason { get; set; } // 선물을 못 주는 이유 (툴팁용)  
    public bool HasPendingQuest { get; set; } // 배달 퀘스트 여부  
    public bool IsBirthdayToday { get; set; } // 생일 여부  
    public Item SentGift { get; set; } // 오늘 이미 보낸 선물이 있는지  
    public GiftTaste TasteForCurrentItem { get; set; } // 현재 아이템에 대한 호감도  
}

이처럼 UI에 필요한 모든 데이터를 ViewModel에서 미리 계산하고 가공하여 속성으로 만들어두는 것이 핵심입니다.

#### **StarML과 데이터 바인딩: 동적 UI 구현**

PenPals의 UI 기능들은 StarML의 데이터 바인딩을 통해 매우 효율적으로 구현될 수 있습니다.

* **NPC 목록 표시**: StarML에서는 foreach 같은 반복 구문을 사용해 Npcs 목록을 순회하며 각 NpcModel에 대한 UI 조각(초상화, 이름 등)을 생성합니다.  
* **조건부 스타일링 (회색 처리 및 녹색 배경)**: 선물을 줄 수 없는 NPC를 회색으로 표시하는 기능은 CanReceiveGift 속성을 사용합니다. StarML에서는 이 bool 값을 UI 요소의 opacity나 enabled 같은 속성에 바인딩할 수 있습니다.2  
  XML  
  \<portrait image\={Portrait} opacity\={CanReceiveGift? 1.0 : 0.5} /\>

  이미 선물을 보낸 NPC에게 녹색 배경을 표시하는 것도 SentGift\!= null 같은 조건을 배경색 속성에 바인딩하여 유사하게 구현합니다.2  
* **조건부 가시성 (생일 풍선, 퀘스트 아이콘)**: 특정 조건에서만 나타나는 UI 요소는 visibility 속성에 바인딩합니다.  
  XML  
  \<icon image\={@Mods/PenPals/BalloonIcon} visibility\={IsBirthdayToday} /\>

  퀘스트 아이콘 역시 HasPendingQuest 속성을 이용해 동일한 방식으로 제어합니다.2  
* **툴팁 (마우스 오버 정보)**: 선물을 줄 수 없는 이유를 마우스 오버 시 보여주는 기능은 tooltip이나 hoverText 같은 속성에 DisabledReason 문자열 속성을 바인딩하여 간단하게 구현할 수 있습니다.2  
* **필터링 기능**: 사용자가 필터 텍스트를 입력하거나 체크박스를 클릭하면, ViewModel은 그 조건에 맞게 Npcs 목록을 다시 필터링하여 업데이트합니다. StardewUI의 데이터 바인딩 시스템 덕분에, 개발자는 UI를 직접 조작할 필요 없이 C\#의 Npcs 목록 데이터만 변경하면 UI가 자동으로 갱신됩니다.

이처럼 StardewPenPals는 StardewUI의 데이터 바인딩 기능을 십분 활용하여, 복잡한 게임 로직과 UI 표현을 완벽하게 분리하고 동적인 인터페이스를 매우 효율적으로 구현한 훌륭한 사례입니다.

---

## **파트 III: 고급 고려사항 및 전략적 권장 사항**

이 마지막 파트에서는 성능, 다른 모딩 도구와의 비교 분석, 커뮤니티 참여 등 개발자를 위한 전문가 수준의 조언을 제공합니다.

### **3.1. 성능, 확장성 및 모범 사례**

StardewFishingSea 모드의 설정 항목 중 CatchPreviewTileRadius에는 "이 값을 너무 높게 설정하면 성능이 저하될 수 있다"는 경고가 포함되어 있습니다. 이는 StardewUI의 성능 특성에 대한 중요한 단서를 제공합니다.

이 경고는 많은 수의 UI 요소를 렌더링하는 작업, 특히 각 요소가 복잡한 게임 로직(예: 여러 타일에 대한 낚시 예측)과 연결되어 있을 때 상당한 연산 비용이 발생할 수 있음을 시사합니다. StardewUI의 편리한 데이터 바인딩 및 레이아웃 시스템은 무료가 아니며, 남용될 경우 성능에 영향을 줄 수 있습니다.

따라서 StardewUI를 사용하여 복잡한 UI를 구축할 때는 다음과 같은 모범 사례를 따르는 것이 중요합니다.

* **데이터 바인딩 최소화**: UI에 바인딩하는 데이터의 양, 특히 매 프레임마다 업데이트되는 데이터의 양에 유의해야 합니다. 불필요한 데이터는 뷰모델에 포함시키지 않도록 합니다.  
* **가상화(Virtualization)**: 긴 목록을 표시할 때는 화면에 보이는 항목만 렌더링하는 가상화 기법을 고려해야 합니다. 이는 수백, 수천 개의 아이템을 스크롤하는 UI에서 성능을 유지하는 데 필수적입니다.  
* **업데이트 조절(Debouncing/Throttling)**: 매우 빈번하게 발생하는 이벤트(예: 마우스 이동)에 직접 UI 업데이트를 바인딩하는 것을 피하고, 일정 시간 간격으로 업데이트를 모아서 처리하는 디바운싱 또는 스로틀링 기법을 적용해야 합니다.

이러한 전략들은 StardewUI로 제작된 데이터 집약적인 UI가 복잡한 상황에서도 부드럽게 작동하도록 보장하는 데 도움이 됩니다.

### **3.2. 비교 분석: 모딩 툴체인 내 StardewUI의 위치**

스타듀밸리 모딩 생태계에는 Generic Mod Config Menu (GMCM)와 같은 특수 목적의 UI 도구와 SMAPI가 제공하는 저수준 렌더링 API가 이미 존재합니다. StardewUI는 이 생태계 내에서 특정 역할을 수행하며, 모든 것을 대체하는 만능 도구는 아닙니다.

* **vs. SMAPI 기본 API**: StardewUI는 SMAPI의 직접적인 그리기 호출에 비해 훨씬 높은 수준의 추상화를 제공합니다. 복잡한 레이아웃을 제작할 때 개발 속도가 극적으로 빨라지지만, 최적화된 저수준 API 호출에 비해 약간의 오버헤드가 발생할 수 있습니다.  
* **vs. GMCM**: GMCM은 모드의 config.json 파일을 위한 표준화된 설정 메뉴를 생성하는 데 고도로 특화된 프레임워크입니다. 반면, StardewUI는 상점, 인벤토리, HUD 등 모든 종류의 커스텀 UI를 제작하기 위한 범용 프레임워크입니다. 이 둘은 경쟁 관계가 아니라 상호 보완적인 도구입니다.

따라서 개발자는 목적에 따라 적절한 도구를 선택해야 합니다.

* **GMCM**: 표준적인 모드 설정 메뉴가 필요할 때 사용합니다.  
* **SMAPI 기본 API**: 극도로 단순하고 성능이 매우 중요한 단일 오버레이(예: FPS 카운터)에 사용합니다.  
* **StardewUI**: 개발 속도와 유지보수성이 중요한 모든 종류의 복잡하고 데이터 중심적인 커스텀 메뉴 또는 HUD에 사용합니다.

### **3.3. 향후 개발 및 커뮤니티 기여**

StardewUI의 GitHub 저장소 README에는 프로젝트에 기여하는 방법에 대한 명확한 가이드라인이 제시되어 있습니다. 새로운 기능 제안 전 토론 시작, 최신 마일스톤 확인, 그리고 버그 보고 시 SSCCE(Short, Self-Contained, Correct Example) 첨부 등이 권장됩니다. 또한, 사용자는 스타듀밸리 공식 디스코드 채널을 통해 지원을 받을 수 있습니다.1

이는 StardewUI가 커뮤니티의 참여를 환영하는 활발한 오픈소스 프로젝트임을 보여줍니다. 스타듀밸리 모딩 위키에서도 StardewUI가 UI 개발을 "더 쉽게" 만들어 줄 수 있는 도구로 언급되며, 커뮤니티 내에서 그 인지도가 점차 높아지고 있음을 알 수 있습니다.

---

## **결론: 모드 개발의 새로운 가능성**

StardewUI는 스타듀밸리 모드에 복잡하고 동적인 UI를 추가하려는 개발자에게 강력한 솔루션을 제공합니다. 선언적인 StarML과 데이터 바인딩을 통해 UI의 디자인과 로직을 분리함으로써, 개발자는 UI의 세부적인 구현보다 모드의 핵심 기능에 더 집중할 수 있습니다.

StardewPenPals 사례에서 보았듯이, StardewUI는 단순한 목록 표시를 넘어 조건부 스타일링, 동적 요소 가시성, 실시간 필터링 등 정교한 상호작용이 필요한 UI를 구축하는 데 매우 효과적입니다.

단순한 설정 메뉴를 넘어, 커스텀 상점, 복잡한 인벤토리 관리 시스템, 실시간 정보 HUD 등 UI가 중요한 역할을 하는 모드를 기획하고 있다면 StardewUI는 여러분의 개발 시간을 단축시키고 코드의 품질을 높여줄 훌륭한 선택이 될 것입니다.

#### **참고 자료**

1. Mod Ideas \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/Mod\_Ideas](https://stardewmodding.wiki.gg/wiki/Mod_Ideas)  
2. focustense/StardewPenPals: Stardew Valley mod for mailing gifts \- GitHub, 9월 16, 2025에 액세스, [https://github.com/focustense/StardewPenPals](https://github.com/focustense/StardewPenPals)