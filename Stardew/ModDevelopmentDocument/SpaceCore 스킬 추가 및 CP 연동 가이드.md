

# **Stardew Valley 커스텀 스킬 아키텍처: SpaceCore API 개발자 가이드**

## **Part 1: SpaceCore 스킬 모드의 기초**

SpaceCore API를 사용하여 새로운 스킬을 구현하는 과정은 단순히 C\# 코드를 작성하는 것을 넘어, 모드의 구조적 설계와 스타듀밸리 모딩 생태계의 모범 사례를 이해하는 것에서 시작됩니다. 성공적인 스킬 모드는 게임 로직을 처리하는 핵심 C\# 모드와 사용자 인터페이스(UI) 및 시각적 자산을 관리하는 콘텐츠 팩(Content Pack)이라는 두 가지 주요 구성 요소의 상호 작용에 의존합니다. 이 섹션에서는 이러한 이중 구성 요소 아키텍처를 분석하고, 각 구성 요소가 SMAPI(Stardew Modding API)와 상호 작용하는 방식을 정의하는 매니페스트 파일의 구성 방법을 심층적으로 탐구합니다.

### **1.1. 이중 구성 요소 아키텍처: 로직과 표현의 분리**

제공된 LuckSkill 예제는 현대 스타듀밸리 모딩의 핵심적인 설계 패턴을 보여줍니다. 바로 로직과 표현의 분리입니다. 이 아키텍처는 두 개의 독립적인 프로젝트로 구성됩니다.

1. **SMAPI 로직 모드 (LuckSkill)**: C\#으로 작성된 이 프로젝트는 스킬의 모든 핵심 기능을 담당합니다. 여기에는 스킬 자체의 정의, 직업(profession) 계층 구조, 경험치 획득 규칙, 그리고 직업이 플레이어에게 부여하는 실제 게임 내 효과(예: 능력치 증가, 아이템 드랍률 변경)가 포함됩니다. 이 모드는 게임의 이벤트에 직접 연결되어 플레이어의 행동에 반응하고 게임 상태를 수정합니다.  
2. **콘텐츠 패처(Content Patcher) 팩 (CP\_LuckSkill)**: 이 프로젝트는 스킬의 시각적 표현을 담당합니다. 여기에는 스킬 페이지에 표시될 아이콘, 레벨 업 메뉴의 아이콘 등 모든 그래픽 자산이 포함됩니다. 콘텐츠 패처를 사용함으로써, 이 모드는 게임의 원본 파일을 직접 수정하지 않고도 안전하고 호환성 높은 방식으로 새로운 이미지를 게임에 '주입'할 수 있습니다.

이러한 구조적 분리는 의도적이며 강력한 설계상의 이점을 제공합니다. 첫째, **관심사의 분리(Separation of Concerns)** 원칙을 따릅니다. 개발자는 스킬의 복잡한 로직에 집중할 수 있으며, 아티스트나 다른 기여자는 C\# 코드를 전혀 건드리지 않고도 CP\_LuckSkill 폴더의 이미지 파일과 content.json만 수정하여 스킬의 외형을 변경하거나 개선할 수 있습니다. 둘째, **유지보수성과 확장성**이 향상됩니다. UI 변경이 필요할 때 전체 C\# 프로젝트를 다시 컴파일할 필요가 없으며, 여러 콘텐츠 팩이 서로 다른 시각적 스타일을 제공하며 공존할 수 있습니다. 이는 모딩 커뮤니티 내에서의 협업과 사용자 정의를 촉진하는 핵심적인 요소입니다.

### **1.2. 로직 모드 매니페스트 구성 (LuckSkill/manifest.json)**

모든 SMAPI 모드의 심장부에는 manifest.json 파일이 있습니다. 이 파일은 SMAPI 로더에게 모드의 정체성, 제작자, 버전, 그리고 가장 중요하게는 실행에 필요한 다른 모드(의존성)에 대한 정보를 제공하는 계약서와 같습니다. LuckSkill의 매니페스트는 다음과 같은 핵심 필드를 포함합니다.

* "UniqueID": "pet.LuckSkill": 이 모드를 스타듀밸리 모딩 생태계 전체에서 고유하게 식별하는 ID입니다. 충돌을 피하기 위해 모든 모드는 자신만의 UniqueID를 가져야 합니다. 일반적으로 "제작자이름.모드이름" 형식을 따릅니다.  
* "Dependencies": 이 섹션은 모드가 올바르게 작동하기 위해 반드시 먼저 로드되어야 하는 다른 모드들을 명시합니다. SMAPI는 이 목록을 확인하고, 만약 의존성 모드가 없거나 버전이 맞지 않으면 사용자에게 경고를 표시하고 모드 로드를 중단합니다.  
  * "spacechase0.SpaceCore": 이 모드의 핵심 의존성입니다. SpaceCore는 커스텀 스킬을 생성하는 데 필요한 기본 Skill 클래스와 관련 API를 제공합니다. 이 의존성을 명시함으로써, SMAPI는 LuckSkill을 로드하기 전에 SpaceCore가 반드시 로드되도록 보장합니다.  
  * "Pathoschild.ContentPatcher": C\# 코드가 직접적으로 콘텐츠 패처의 API를 호출하지는 않더라도, 이 모드의 완전한 기능을 위해서는 콘텐츠 팩이 필요하므로 의존성으로 명시하는 것이 좋습니다. 이는 사용자에게 완전한 경험을 위해 두 구성 요소를 모두 설치해야 함을 알려주는 역할을 합니다.  
* "ContentPackFor": 이 키가 **존재하지 않는다**는 점이 중요합니다. 이 키의 부재는 SMAPI에게 이 폴더가 C\# 코드를 포함하고 컴파일된 .dll 파일을 실행해야 하는 '로직 모드'임을 알립니다.

### **1.3. 콘텐츠 팩 매니페스트 구성 (CP\_LuckSkill/manifest.json)**

콘텐츠 팩의 매니페스트는 로직 모드의 것과 유사하지만, 그 목적과 핵심 구성에서 결정적인 차이가 있습니다.

* "UniqueID": "pet.LuckSkill\_CP": 로직 모드와는 별개의 고유 ID를 가집니다. 이는 두 구성 요소를 독립적으로 관리하고 식별하기 위함입니다.  
* "ContentPackFor": { "UniqueID": "Pathoschild.ContentPatcher" }: 이 파일에서 가장 중요한 부분입니다. 이 줄은 SMAPI에게 이 폴더가 실행 가능한 C\# 모드가 아니라, Pathoschild.ContentPatcher라는 UniqueID를 가진 모드(즉, 콘텐츠 패처)에 의해 처리되어야 할 데이터와 자산의 묶음임을 명시적으로 선언합니다. SMAPI는 이 폴더의 .dll을 찾지 않고, 대신 콘텐츠 패처에게 이 팩의 내용을 전달하여 처리하도록 합니다.

이 두 매니페스트 파일은 명확한 의존성 관계를 구축합니다. 사용자는 CP\_LuckSkill 없이 LuckSkill을 설치할 수 있습니다. 이 경우 스킬은 기능적으로는 작동하지만 아이콘이 보이지 않는 등 시각적 결함이 발생합니다. 반대로, LuckSkill 없이 CP\_LuckSkill만 설치하는 것은 아무런 의미가 없습니다. 콘텐츠 패처는 LuckSkill이 존재할 때만 패치를 적용하도록 구성되어 있기 때문입니다. 이러한 모듈성은 견고한 소프트웨어 설계의 특징이며, 사용자가 설치 과정을 더 쉽게 이해하고 문제를 해결할 수 있도록 돕습니다.

### **1.4. C\# 프로젝트에서 SpaceCore 참조**

C\# 로직 모드를 개발하기 위해서는 먼저 개발 환경을 설정해야 합니다. Visual Studio와 같은 IDE(통합 개발 환경)에서 새 C\# 클래스 라이브러리(.NET Framework) 프로젝트를 생성한 후, SpaceCore API를 사용하기 위해 SpaceCore.dll 파일을 프로젝트 참조로 추가해야 합니다.

이 과정은 일반적으로 다음과 같습니다.

1. SMAPI를 통해 게임을 한 번 실행하여 모든 의존성 모드가 게임의 Mods 폴더에 설치되도록 합니다.  
2. Visual Studio의 솔루션 탐색기에서 프로젝트의 '참조(References)' 항목을 마우스 오른쪽 버튼으로 클릭하고 '참조 추가(Add Reference...)'를 선택합니다.  
3. '찾아보기(Browse...)' 버튼을 클릭하여 스타듀밸리 Mods/SpaceCore 폴더로 이동합니다.  
4. SpaceCore.dll 파일을 선택하고 확인을 누릅니다.

이 단계를 완료하면, 프로젝트는 SpaceCore가 제공하는 모든 클래스, 메서드, 속성(예: SpaceCore.Skills.Skill)을 인식하게 됩니다. 이를 통해 코드 자동 완성(IntelliSense) 기능을 활용할 수 있으며, 컴파일러는 SpaceCore API를 사용하는 코드를 올바르게 빌드할 수 있습니다.

## **Part 2: 스킬의 해부학: LuckSkill.cs 심층 분석**

스킬의 핵심 로직은 SpaceCore의 Skill 클래스를 상속받는 C\# 클래스에 정의됩니다. 이 섹션에서는 LuckSkill.cs 파일을 한 줄 한 줄 분석하여, 새로운 스킬에 정체성을 부여하고 게임 세계에 통합하는 방법을 탐구합니다. 스킬의 내부 ID부터 플레이어에게 보여지는 이름, 설명, 아이콘에 이르기까지 각 구성 요소의 역할과 구현 방식을 상세히 살펴봅니다.

### **2.1. 스킬 클래스 정의**

모든 SpaceCore 기반 스킬은 SpaceCore.Skills.Skill 추상 클래스를 상속받는 것으로 시작됩니다. LuckSkill의 클래스 서명은 이 구조를 명확히 보여줍니다.

public class LuckSkill : SpaceCore.Skills.Skill

이 상속은 LuckSkill 클래스가 SpaceCore의 스킬 프레임워크에 편입되기 위한 계약입니다. 이 계약을 통해 LuckSkill은 스킬이 갖추어야 할 기본적인 속성과 메서드를 구현해야 할 의무를 지게 됩니다.

클래스의 생성자는 스킬의 가장 근본적인 식별자를 설정합니다.

public LuckSkill() : base("luck")

여기서 base("luck") 구문은 부모 클래스인 Skill의 생성자를 호출하며, 문자열 "luck"을 인자로 전달합니다. 이 문자열은 스킬의 내부 \*\*고유 ID(Unique ID)\*\*가 됩니다. 이 ID는 플레이어에게 직접 노출되지 않지만, API를 통해 스킬을 참조할 때 사용되는 핵심 키입니다. 예를 들어, 플레이어에게 행운 스킬 경험치를 부여하려면 player.AddExperience("luck", 10)과 같이 이 ID를 사용해야 합니다. 따라서 이 ID는 게임 내에 존재하는 모든 스킬(바닐라 및 다른 모드 스킬 포함) 중에서 유일해야 합니다.

### **2.2. 핵심 스킬 속성과 정체성**

스킬 클래스는 여러 속성과 메서드를 재정의(override)하여 스킬의 이름, 설명, 아이콘과 같은 세부 정보를 게임에 제공해야 합니다.

* GetName() 및 GetDescription(): 이 메서드들은 각각 스킬의 이름과 설명을 반환하며, 플레이어의 스킬 페이지에 표시됩니다. LuckSkill 예제는 지역화(localization)를 위한 모범 사례를 보여줍니다.  
  return ModEntry.Instance.Helper.Translation.Get("skill.name");  
  하드코딩된 문자열(예: "Luck")을 직접 반환하는 대신, SMAPI의 번역 헬퍼(Helper.Translation)를 사용하여 i18n 폴더의 언어 파일에서 해당 키(skill.name)에 맞는 문자열을 가져옵니다. 이 접근 방식 덕분에 다른 언어를 사용하는 플레이어를 위해 손쉽게 번역을 추가할 수 있습니다.  
* **Icon 및 SkillsPageIcon**: 이 속성들은 각각 스킬의 아이콘으로 사용될 Texture2D 객체를 반환해야 합니다.  
  * Icon: 레벨 업 메뉴나 기타 작은 UI 요소에 사용되는 아이콘입니다.  
  * SkillsPageIcon: 플레이어의 스킬 탭에 표시되는 주 아이콘입니다.

여기서 LuckSkill의 구현은 매우 흥미로운 지점을 드러냅니다. 두 속성 모두 null을 반환하도록 되어 있습니다. 이는 의도적인 설계 결정이며, C\# 로직 모드와 콘텐츠 패처 팩 간의 미묘하고 중요한 상호작용을 보여줍니다. C\# 코드에서 아이콘을 null로 남겨두면, SpaceCore는 기본 동작으로 폴백(fallback)합니다. 스킬 페이지를 렌더링할 때, 게임은 LooseSprites\\skill\_sheet라는 텍스처 파일을 로드하여 모든 스킬 아이콘을 그립니다. LuckSkill의 C\# 코드는 이 과정에 직접 개입하지 않습니다. 대신, CP\_LuckSkill 콘텐츠 팩이 이 시점을 가로챕니다. 콘텐츠 패처는 게임이 skill\_sheet를 로드하려는 요청을 감지하고, content.json에 정의된 규칙에 따라 assets/luck\_skill\_icon.png 파일을 원본 텍스처의 지정된 위치에 덧붙여 수정된 버전을 게임에 반환합니다.

결론적으로, C\# 코드는 "나는 'luck'이라는 ID를 가진 스킬이며, UI에 표시될 공간이 필요하다"고 선언하는 역할을 합니다. 그러면 콘텐츠 패처가 그 선언된 공간을 실제 그래픽으로 채우는 것입니다. 이 간접적인 협력 방식은 모드의 유연성과 호환성을 극대화하는 세련된 해결책입니다.

### **2.3. 핵심 Skill 클래스 속성 참조 테이블**

다음 표는 개발자가 커스텀 스킬 클래스를 구현할 때 반드시 고려해야 할 SpaceCore.Skills.Skill의 주요 멤버들을 요약한 것입니다. 이는 새로운 스킬을 만들 때 필요한 구성 요소들을 빠짐없이 구현하기 위한 체크리스트 역할을 할 수 있습니다.

| 속성/메서드 | 반환 타입 | 목적 | LuckSkill 구현 예시 |
| :---- | :---- | :---- | :---- |
| Id | string | 스킬의 내부 고유 식별자. API 호출에 사용됩니다. | base("luck") 생성자에서 설정 |
| GetName() | string | 스킬 페이지에 표시될, 번역 가능한 스킬의 이름. | return ModEntry.Instance.Helper.Translation.Get("skill.name"); |
| GetDescription() | string | 스킬 페이지에 표시될, 번역 가능한 스킬의 설명. | return ModEntry.Instance.Helper.Translation.Get("skill.description"); |
| SkillsPageIcon | Texture2D | 스킬 페이지 탭에 표시될 아이콘. null일 경우 CP 패치에 의존. | return null; |
| Icon | Texture2D | 레벨 업 메뉴 등에 사용될 작은 아이콘. null일 경우 CP 패치에 의존. | return null; |
| Professions | List\<Profession\> | 이 스킬에 속한 모든 직업(레벨 5 및 10)의 목록. | 생성자에서 new Profession(...)으로 채워짐 |
| ExperienceCurve | int | 각 레벨업에 필요한 누적 경험치 양. | new { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 } |
| ExperienceBarColor | Color | 스킬 경험치 바의 색상. | new Color(255, 150, 0\) (주황색 계열) |

이 표는 Skill 클래스의 추상적인 요구사항을 구체적이고 실행 가능한 항목으로 변환하여, 개발자가 API의 계약을 충족하는 완전한 기능의 스킬 클래스를 효율적으로 작성할 수 있도록 돕습니다.

## **Part 3: 스킬 트리 설계: 직업과 특전**

스킬의 핵심적인 재미와 전략적 깊이는 레벨 5와 10에 도달했을 때 플레이어가 선택하는 직업(Profession)에서 나옵니다. 이 섹션에서는 LuckSkill이 어떻게 분기되는 직업 트리를 구성하는지, 그리고 더 중요하게는, 이 직업들이 어떻게 실제 게임 플레이에 영향을 미치는 효과를 구현하는지를 분석합니다. 직업을 단순히 '선언'하는 것과 그 효과를 '구현'하는 것 사이의 중요한 차이점을 이해하는 것이 핵심입니다.

### **3.1. Profession 클래스**

SpaceCore는 스킬 트리의 각 노드를 표현하기 위해 SpaceCore.Skills.Profession 클래스를 제공합니다. 개발자는 이 클래스의 인스턴스를 생성하여 각 직업의 ID, 이름, 설명, 요구 레벨 및 아이콘과 같은 속성을 정의합니다. LuckSkill의 생성자에서는 이 Profession 객체들을 생성하여 스킬의 Professions 리스트에 추가하는 코드를 볼 수 있습니다. 이 리스트가 스킬 페이지의 직업 트리 UI를 구성하는 데이터 소스가 됩니다.

### **3.2. 레벨 5 직업 정의**

LuckSkill은 레벨 5에서 두 가지 직업, "Fortunate"와 "Popular"를 제공합니다. 이 직업들은 다음과 같이 정의됩니다.

C\#

// Fortunate 직업 정의  
var prof1 \= new Profession(this, "fortunate")  
{  
    Icon \= null, // 아이콘은 Content Patcher가 처리  
    Name \= () \=\> ModEntry.Instance.Helper.Translation.Get("profession.fortunate.name"),  
    Description \= () \=\> ModEntry.Instance.Helper.Translation.Get("profession.fortunate.description"),  
    Level \= 5,  
};  
Professions.Add(prof1);

이 코드 조각을 분석해 보면 몇 가지 중요한 점을 알 수 있습니다.

* new Profession(this, "fortunate"): 생성자의 첫 번째 인자는 이 직업이 속한 스킬 객체(this)이고, 두 번째 인자는 직업의 고유 ID("fortunate")입니다. 이 ID는 나중에 플레이어가 이 직업을 가지고 있는지 확인할 때 사용됩니다.  
* Name과 Description: 이 속성들은 string이 아닌 Func\<string\> 람다 식으로 정의되었습니다. 이는 지역화를 지원하기 위함으로, 게임 언어가 변경될 때마다 이 함수가 다시 호출되어 올바른 언어의 문자열을 가져올 수 있도록 합니다.  
* Level \= 5: 이 직업이 레벨 5에 도달했을 때 선택 가능함을 명시합니다.

"Popular" 직업(prof2) 역시 동일한 구조로 정의됩니다.

### **3.3. 레벨 10 분기 직업 정의**

레벨 10에서는 레벨 5 선택에 따라 다른 두 가지 직업 중 하나를 선택할 수 있는 분기 구조가 나타납니다. LuckSkill은 이 구조를 ParentProfessionId 속성을 사용하여 구현합니다.

C\#

// "Fortunate"의 하위 직업 "Lucky Star" 정의  
var prof1a \= new Profession(this, "luckystar")  
{  
    //... 이름, 설명, 아이콘 등...  
    Level \= 10,  
    ParentProfessionId \= "fortunate", // 부모 직업 ID 명시  
};  
Professions.Add(prof1a);

여기서 가장 중요한 부분은 ParentProfessionId \= "fortunate"입니다. 이 한 줄의 코드는 SpaceCore에게 "Lucky Star" 직업은 레벨 5에서 "Fortunate" 직업을 선택한 플레이어에게만 제공되어야 함을 알려줍니다. 마찬가지로, "Shooting Star" 직업은 ParentProfessionId를 "fortunate"으로, "Beloved"와 "Golden Heart" 직업은 각각 ParentProfessionId를 "popular"로 설정하여 완벽한 2x2 분기 트리를 구성합니다. 이 속성은 스킬 트리 로직의 핵심이며, 복잡한 분기 구조를 매우 직관적으로 정의할 수 있게 해줍니다.

### **3.4. 직업 효과 구현**

Professions 리스트에 직업을 추가하는 것은 UI에 선택지를 표시하고 플레이어 데이터에 선택 사항을 저장하는 역할만 합니다. 이는 직업을 '선언'하는 단계일 뿐, 그 자체로는 아무런 게임 내 효과를 발생시키지 않습니다. 예를 들어, "Fortunate" 직업이 "매일의 행운을 약간 증가시킨다"고 설명되어 있더라도, 이 효과는 자동으로 적용되지 않습니다. 개발자는 이 효과를 별도의 코드로 직접 '구현'해야 합니다.

이것이 '선언과 구현의 분리'라는 중요한 아키텍처 패턴입니다. 직업의 효과는 일반적으로 SMAPI의 이벤트 핸들러 내에서 구현됩니다. LuckSkill의 코드를 살펴보면, ModEntry.cs 파일에 GameLoop.DayStarted와 같은 이벤트를 구독하는 부분이 있습니다. DayStarted 이벤트는 게임 내에서 새로운 날이 시작될 때마다 발생하며, 이 시점에서 직업 효과를 적용하기에 이상적입니다.

OnDayStarted 이벤트 핸들러 내의 로직은 다음과 같은 형태를 띨 것입니다.

C\#

private void OnDayStarted(object sender, DayStartedEventArgs e)  
{  
    // "Fortunate" 직업의 바닐라 ID를 가져옵니다.  
    int fortunateProfessionId \= Skills.GetSkill("luck").Professions\["fortunate"\].GetVanillaId();

    // 현재 플레이어가 "Fortunate" 직업을 가지고 있는지 확인합니다.  
    if (Game1.player.professions.Contains(fortunateProfessionId))  
    {  
        // 직업 효과를 적용합니다. (예: 하루 동안의 행운 수치 증가)  
        Game1.player.team.sharedDailyLuck.Value \+= 0.01;  
    }  
}

이 코드의 흐름은 명확합니다.

1. 날이 시작될 때마다 코드가 실행됩니다.  
2. Skills.GetSkill("luck").Professions\["fortunate"\].GetVanillaId()를 통해 "Fortunate" 직업에 할당된 게임 내부의 숫자 ID를 가져옵니다. SpaceCore는 커스텀 직업을 바닐라 직업 ID 시스템에 통합하기 위해 고유한 숫자 ID를 동적으로 할당합니다.  
3. Game1.player.professions.Contains(...)를 사용하여 현재 플레이어가 해당 ID를 가지고 있는지, 즉 해당 직업을 선택했는지 확인합니다.  
4. 조건이 참이면, 직업의 설명에 명시된 효과(이 경우, 공유 일일 행운 증가)를 플레이어에게 적용합니다.

이러한 패턴은 모든 종류의 직업 효과에 적용될 수 있습니다. 몬스터를 처치했을 때 아이템 드랍률을 높이는 직업이라면 MonsterSlayer.MonsterSlain 이벤트 핸들러 내에서, 특정 아이템의 판매 가격을 높이는 직업이라면 Player.InventoryChanged 또는 아이템을 판매하는 시점의 로직에 패치를 적용하여 구현할 수 있습니다. 이처럼 직업의 선언은 Skill 클래스 내에서 정적으로 이루어지지만, 그 효과는 게임의 동적인 이벤트에 반응하여 구현된다는 점을 이해하는 것이 매우 중요합니다.

## **Part 4: 경험치 엔진: 게임 이벤트에 연결하기**

어떤 스킬이든 그 핵심에는 경험치(XP) 획득 메커니즘이 있습니다. 플레이어는 특정 행동을 통해 경험치를 얻고, 이를 통해 레벨을 올려 새로운 능력을 잠금 해제합니다. 이 섹션에서는 LuckSkill이 SMAPI의 강력한 이벤트 시스템을 활용하여 게임 내 다양한 활동에 대한 경험치 보상을 어떻게 구현하는지 상세히 분석합니다. 이는 개발자가 자신만의 독창적인 경험치 획득 트리거를 만드는 데 필요한 청사진을 제공할 것입니다.

### **4.1. SMAPI 이벤트 시스템: 모드의 감각 기관**

SMAPI는 이벤트 기반 아키텍처(event-driven architecture) 위에서 동작합니다. 이는 모드가 끊임없이 게임 상태를 확인하는 무한 루프를 실행하는 대신, 게임의 특정 '사건'이 발생했을 때 알림을 받도록 구독하는 방식입니다. 예를 들어, '새로운 날이 시작됨', '플레이어가 아이템을 수확함', '몬스터가 처치됨'과 같은 사건들이 모두 이벤트로 정의되어 있습니다. 모드는 이러한 이벤트에 '이벤트 핸들러'라는 특정 메서드를 등록하여, 해당 사건이 발생했을 때 자신의 코드가 실행되도록 할 수 있습니다. 이 시스템은 효율적이며, 모드가 게임의 핵심 로직과 느슨하게 결합(loosely coupled)되도록 하여 호환성을 높입니다.

### **4.2. ModEntry.cs에서 이벤트 구독하기**

모드가 처음 로드될 때 실행되는 진입점(entry point)은 ModEntry.cs 파일의 Entry 메서드입니다. 이 메서드는 모드의 모든 초기 설정을 수행하는 이상적인 장소이며, 이벤트 구독도 여기서 이루어집니다. LuckSkill의 Entry 메서드에는 다음과 같은 코드가 포함되어 있습니다.

C\#

public override void Entry(IModHelper helper)  
{  
    //... 다른 초기화 코드...

    // SpaceCore API를 통해 새로운 스킬을 등록합니다.  
    Skills.RegisterSkill(new LuckSkill());

    // 다양한 게임 이벤트에 이벤트 핸들러 메서드를 연결합니다.  
    helper.Events.GameLoop.DayStarted \+= OnDayStarted;  
    helper.Events.Player.Warped \+= OnWarped;  
    helper.Events.Display.MenuChanged \+= OnMenuChanged;  
}

helper.Events 객체는 SMAPI가 제공하는 모든 이벤트에 대한 접근점을 제공합니다. \+= 연산자는 특정 이벤트에 실행할 메서드(이벤트 핸들러)를 등록하는 데 사용됩니다. 예를 들어, helper.Events.GameLoop.DayStarted \+= OnDayStarted; 라인은 GameLoop.DayStarted 이벤트가 발생할 때마다 OnDayStarted라는 이름의 메서드를 호출하도록 SMAPI에 지시합니다. LuckSkill은 행운과 관련된 다양한 활동(정동석 깨기, 카지노 이용, 팬닝 등)을 감지하기 위해 여러 이벤트에 핸들러를 등록합니다.

### **4.3. 경험치 부여 로직 구현**

이벤트 핸들러가 등록되면, 실제 경험치를 부여하는 로직을 해당 메서드 내에 작성해야 합니다. LuckSkill은 다양한 시나리오에서 경험치를 부여하는 훌륭한 예시를 제공합니다.

**예시 1: 정동석(Geode) 깨기**

LuckSkill은 정동석을 깼을 때 경험치를 부여하기 위해, 대장간 메뉴가 닫힐 때를 감지하는 Display.MenuChanged 이벤트를 활용합니다.

C\#

private void OnMenuChanged(object sender, MenuChangedEventArgs e)  
{  
    //... 메뉴가 대장간의 정동석 처리 메뉴인지 확인하는 로직...

    if (/\* 정동석을 깬 후 메뉴가 닫혔다면 \*/)  
    {  
        // 정동석에서 나온 아이템의 가치나 희귀도에 따라 XP를 계산합니다.  
        int xpToGive \= CalculateXpFromGeodeContents(geodeContents);

        // SpaceCore API를 사용하여 플레이어에게 경험치를 추가합니다.  
        Game1.player.AddExperience(Skills.GetSkill("luck").Id, xpToGive);  
    }  
}

이 로직의 핵심은 Game1.player.AddExperience(string skillId, int amount) 메서드 호출입니다.

* 첫 번째 인자 Skills.GetSkill("luck").Id는 경험치를 추가할 스킬의 고유 ID("luck")를 동적으로 가져옵니다.  
* 두 번째 인자 xpToGive는 부여할 경험치의 양입니다.

**예시 2: 몬스터 처치**

몬스터 처치 시 경험치를 부여하려면 MonsterSlayer.MonsterSlain 이벤트를 사용하는 것이 일반적입니다.

C\#

private void OnMonsterSlain(object sender, MonsterSlainEventArgs e)  
{  
    // 특정 몬스터(예: 박쥐, 해골)가 행운과 관련이 있다고 가정  
    if (e.Monster.Name \== "Bat" |

| e.Monster.Name \== "Skeleton")  
    {  
        // 이벤트 인자 e.Player를 사용하여 몬스터를 처치한 플레이어에게 XP를 부여합니다.  
        e.Player.AddExperience(Skills.GetSkill("luck").Id, 5);  
    }  
}

이 예시는 멀티플레이어 환경에서의 중요성을 보여줍니다. Game1.player는 항상 로컬 플레이어를 가리키지만, e.Player는 이벤트를 실제로 발생시킨 플레이어(몬스터를 처치한 플레이어)를 정확히 가리킵니다. 따라서 멀티플레이어 호환성을 위해서는 항상 이벤트 인자(event arguments)에 포함된 플레이어 객체를 사용하는 것이 필수적입니다.

이처럼 어떤 이벤트에 연결하고, 핸들러 내에서 어떤 조건을 확인하며, 얼마만큼의 경험치를 부여할지 결정하는 것이 스킬의 핵심적인 게임 플레이 루프를 설계하는 과정입니다.

### **4.4. 균형 잡히고 매력적인 경험치 곡선 설계**

경험치 시스템을 구현하는 것은 단순히 기술적인 작업을 넘어, 섬세한 게임 디자인의 영역입니다. 어떤 행동에 경험치를 부여하고 그 양을 얼마로 설정할지는 플레이어의 경험에 지대한 영향을 미칩니다.

LuckSkill은 행운과 관련된 다양한 활동(정동석 깨기, 팬닝, 카지노, 행운의 점심 식사 등)에 걸쳐 경험치를 분산시킴으로써, 플레이어가 다양한 콘텐츠를 즐기도록 유도합니다. 이는 훌륭한 디자인 결정입니다. 만약 쉽게 반복할 수 있는 단 하나의 행동(예: 돌멩이 줍기)에 막대한 경험치를 부여한다면, 플레이어는 해당 행동만 반복하게 되어 게임이 지루해지고 스킬의 가치가 떨어질 것입니다.

개발자는 경험치 시스템을 설계할 때 다음 사항을 고려해야 합니다.

* **플레이어 동기 부여**: 경험치 획득 경로가 스킬의 테마와 일치하는가? 플레이어가 스킬을 올리기 위해 흥미로운 도전을 하도록 유도하는가?  
* **진행 속도(Pacing)**: 플레이어가 너무 빠르거나 느리게 레벨업하지 않도록 경험치 양과 레벨업 요구량(ExperienceCurve)을 조절해야 합니다.  
* **악용 방지(Exploit Prevention)**: 플레이어가 의도치 않은 방식으로 매우 쉽게 대량의 경험치를 얻을 수 있는 허점이 없는지 검토해야 합니다.

균형 잡힌 경험치 시스템은 플레이어에게 꾸준한 성취감을 제공하고, 스킬의 테마를 강화하며, 게임의 전반적인 재미에 기여하는 핵심 요소입니다.

## **Part 5: 프론트엔드 통합: 콘텐츠 패처로 UI 주입하기**

스킬의 로직이 완성되었다면, 이제 플레이어가 게임 내에서 스킬을 보고 상호작용할 수 있도록 시각적인 요소를 추가해야 합니다. 이 과정은 콘텐츠 패처(Content Patcher)와 content.json 파일을 통해 이루어집니다. 이 섹션에서는 C\# 코드 한 줄 없이 간단한 JSON 파일 하나로 어떻게 게임의 UI에 새로운 스킬 아이콘을 '마법처럼' 추가할 수 있는지, 그 원리를 심층적으로 해부합니다.

### **5.1. content.json 구조**

CP\_LuckSkill 폴더의 핵심은 content.json 파일입니다. 이 파일은 콘텐츠 패처에게 어떤 게임 자산을 어떻게 수정할지를 지시하는 명령어 집합입니다. 파일의 기본 구조는 다음과 같습니다.

JSON

{  
    "Format": "1.29.0",  
    "Changes": \[  
        // 여기에 모든 변경 사항(패치) 목록이 들어갑니다.  
    \]  
}

* "Format": 이 콘텐츠 팩이 호환되는 콘텐츠 패처의 버전을 명시합니다.  
* "Changes": 실제 모든 작업이 정의되는 배열입니다. 각 요소는 게임 자산을 수정하는 하나의 '패치' 객체입니다.

### **5.2. EditImage 액션: 비파괴적 패치**

LuckSkill 아이콘을 추가하기 위해 Changes 배열에는 EditImage 액션을 사용하는 패치 객체가 포함됩니다. EditImage는 기존 이미지 파일 전체를 교체하는 Load 액션과 달리, 이미지의 특정 영역에 다른 이미지를 덧그리는(overlay) 비파괴적인 방식입니다. 이는 여러 모드가 동일한 이미지 파일(스프라이트 시트)을 수정할 때 충돌을 최소화하는 매우 중요한 기능입니다.

JSON

{  
    "Action": "EditImage",  
    "Target": "LooseSprites\\\\skill\_sheet",  
    "FromFile": "assets/luck\_skill\_icon.png",  
    "ToArea": { "X": 0, "Y": 72, "Width": 16, "Height": 18 },  
    "When": {  
        "HasMod": "pet.LuckSkill"  
    }  
}

각 키의 역할은 다음과 같습니다.

* "Action": "EditImage": 수행할 작업을 지정합니다. 여기서는 이미지 편집입니다.  
* "Target": "LooseSprites\\\\skill\_sheet": 수정할 대상이 되는 바닐라 게임 자산의 경로입니다. skill\_sheet.png 파일은 모든 스킬 아이콘이 모여 있는 스프라이트 시트입니다. 개발자는 게임의 콘텐츠 파일을 직접 열어보거나 커뮤니티 위키를 참조하여 이러한 대상 경로를 찾을 수 있습니다.  
* "FromFile": "assets/luck\_skill\_icon.png": 덧그릴 이미지 파일의 경로입니다. 이 경로는 콘텐츠 팩 폴더 내부를 기준으로 합니다.  
* "ToArea": { "X": 0, "Y": 72, "Width": 16, "Height": 18 }: 대상 이미지(skill\_sheet.png)에서 이미지를 덧그릴 정확한 위치와 크기를 지정하는 사각형 영역입니다. 이 좌표를 찾는 가장 좋은 방법은 원본 skill\_sheet.png 파일을 Aseprite, GIMP, Photoshop과 같은 이미지 편집 프로그램으로 여는 것입니다. 파일을 열어 비어있는 공간을 찾고, 해당 공간의 왼쪽 위 모서리의 X, Y 좌표와 아이콘의 너비(Width), 높이(Height)를 픽셀 단위로 측정하여 이 값을 결정합니다. 이는 매우 실용적이고 정확한 접근 방식입니다.

### **5.3. "When" 토큰을 이용한 조건부 패치**

위 JSON 객체에서 가장 정교하고 중요한 부분은 "When" 블록입니다.

"When": { "HasMod": "pet.LuckSkill" }

이 조건부 토큰은 콘텐츠 패처에게 이 EditImage 패치를 **오직 pet.LuckSkill이라는 UniqueID를 가진 모드가 설치되고 활성화되었을 때만 적용하라**고 지시합니다. 이것은 모듈성과 호환성의 초석입니다.

이 조건이 없다면 어떤 일이 발생할까요? 만약 사용자가 CP\_LuckSkill 콘텐츠 팩만 설치하고 핵심 로직 모드인 LuckSkill을 설치하지 않았다면, 콘텐츠 패처는 여전히 skill\_sheet.png에 행운 스킬 아이콘을 추가하려고 할 것입니다. 하지만 게임에는 해당 아이콘과 연결될 스킬 로직이 존재하지 않으므로, UI가 깨지거나 빈 공간이 생기는 등의 문제가 발생할 수 있습니다.

"When" 조건을 사용함으로써, 콘텐츠 팩은 스스로의 의존성을 인지하고 관리하게 됩니다. 로직 모드가 존재하지 않으면 콘텐츠 팩은 아무 작업도 수행하지 않고 조용히 비활성화 상태로 남아 오류를 방지합니다. 이 패턴은 모드 제작자가 사용자의 잠재적인 설치 실수를 방지하고, 다른 모드와의 예기치 않은 상호작용을 줄이며, 전반적으로 더 안정적이고 견고한 모드를 만드는 데 필수적입니다.

### **5.4. 콘텐츠 패처 EditImage 액션 분석 테이블**

다음 표는 content.json의 EditImage 액션에 사용되는 주요 키-값 쌍을 분해하여 설명합니다. JSON 구문에 익숙하지 않은 개발자에게 각 필드의 목적을 명확히 전달하는 빠른 참조 가이드 역할을 합니다.

| JSON 키 | 설명 | 데이터 타입 | CP\_LuckSkill 예시 |
| :---- | :---- | :---- | :---- |
| Action | 수행할 작업의 종류를 지정합니다. | string | "EditImage" |
| Target | 수정할 게임의 원본 자산 경로입니다. | string | "LooseSprites\\\\skill\_sheet" |
| FromFile | 덮어쓸 이미지 파일의 경로 (콘텐츠 팩 내부 기준). | string | "assets/luck\_skill\_icon.png" |
| ToArea | Target 이미지 내에서 이미지를 붙여넣을 사각형 영역. | Object | { "X": 0, "Y": 72, "Width": 16, "Height": 18 } |
| FromArea | FromFile 이미지에서 복사할 영역 (생략 시 전체 이미지 사용). | Object | (사용되지 않음) |
| PatchMode | 이미지를 덮어쓸 방식 (예: Overlay). 기본값은 Overlay. | string | (사용되지 않음) |
| When | 이 패치를 적용할 조건을 담은 객체. | Object | { "HasMod": "pet.LuckSkill" } |

이 표는 content.json의 강력한 기능을 체계적으로 이해하고, 개발자가 자신만의 UI 패치를 정확하고 효과적으로 작성할 수 있도록 돕습니다.

## **Part 6: 고급 구현 및 모범 사례**

기능적으로 동작하는 스킬 모드를 만드는 것을 넘어, 전문적인 수준의 모드는 유지보수성, 번역 가능성, 그리고 다른 모드와의 조화로운 공존을 고려하여 제작됩니다. 이 마지막 섹션에서는 지역화, 자산 관리, 멀티플레이어 호환성 등 모드의 완성도를 높이는 데 필수적인 고급 주제와 모범 사례들을 다룹니다.

### **6.1. i18n을 통한 지역화**

전 세계의 플레이어들이 모드를 즐길 수 있도록 하는 가장 좋은 방법은 텍스트를 지역화하는 것입니다. SMAPI는 i18n 폴더를 통해 이를 간편하게 지원합니다.

LuckSkill 프로젝트의 i18n/default.json 파일은 영어 텍스트의 기본 소스 역할을 합니다.

JSON

{  
    "skill.name": "Luck",  
    "skill.description": "Increases your daily luck and chances of finding treasure.",  
    "profession.fortunate.name": "Fortunate",  
    "profession.fortunate.description": "Slightly increases daily luck."  
}

이 파일은 '키(key)'와 '값(value)'의 쌍으로 구성됩니다. 키는 코드에서 텍스트를 참조하는 데 사용되는 고유한 식별자이고, 값은 플레이어에게 실제로 표시될 문자열입니다.

이후 C\# 코드에서는 이 키를 사용하여 번역된 문자열을 가져옵니다. Part 2에서 살펴본 바와 같이, GetName() 메서드는 다음과 같이 구현됩니다.

return this.Helper.Translation.Get("skill.name");

이 Get 메서드는 현재 게임에 설정된 언어에 해당하는 i18n 파일을 찾아 "skill.name" 키에 대한 값을 반환합니다. 만약 i18n/ko.json 파일이 존재하고 그 안에 "skill.name": "행운"이라는 항목이 있다면, 한국어로 게임을 플레이하는 사용자에게는 "행운"이라고 표시될 것입니다. 모든 사용자 대면 텍스트(스킬 이름, 설명, 직업 이름 등)를 이 시스템을 통해 관리하는 것은 매우 중요한 모범 사례입니다.

### **6.2. 자산 관리**

프로젝트의 구조를 깔끔하게 유지하는 것은 장기적인 유지보수에 큰 도움이 됩니다. LuckSkill 예제처럼, 모든 시각적 자산(예: .png 아이콘 파일)은 프로젝트 루트에 흩어놓기보다 전용 assets 폴더에 정리하는 것이 좋습니다.

**로직 모드 (LuckSkill):**

/LuckSkill

|-- /i18n  
| |-- default.json  
|-- LuckSkill.cs  
|-- ModEntry.cs  
|-- manifest.json

**콘텐츠 팩 (CP\_LuckSkill):**

/CP\_LuckSkill

|-- /assets  
| |-- luck\_skill\_icon.png  
|-- content.json  
|-- manifest.json

이러한 구조는 코드 파일과 자산 파일을 명확하게 분리하여 프로젝트를 탐색하고 이해하기 쉽게 만듭니다. content.json에서 "FromFile": "assets/luck\_skill\_icon.png"와 같이 상대 경로를 사용하여 이 자산을 참조할 수 있습니다.

### **6.3. 멀티플레이어 고려사항**

스타듀밸리는 멀티플레이를 지원하므로, 모드 역시 멀티플레이 환경에서 올바르게 작동하도록 설계해야 합니다.

* **올바른 플레이어 참조**: 가장 흔한 실수 중 하나는 항상 Game1.player를 사용하는 것입니다. Game1.player는 현재 게임을 실행하고 있는 로컬 플레이어를 가리킵니다. 멀티플레이 환경에서 다른 플레이어가 어떤 행동을 했을 때 발생하는 이벤트(예: 몬스터 처치)의 핸들러에서 Game1.player에게 경험치를 주면, 엉뚱한 사람에게 보상이 돌아가게 됩니다. 해결책은 Part 4에서 언급했듯이, 항상 이벤트 인자(예: MonsterSlainEventArgs e의 e.Player)가 제공하는 플레이어 객체를 사용하는 것입니다. 이 객체는 이벤트를 실제로 유발한 플레이어를 정확하게 가리킵니다.  
* **상태 동기화**: 플레이어의 스킬 레벨이나 선택한 직업과 같은 데이터는 모든 플레이어에게 동기화되어야 합니다. 다행히도 SpaceCore 프레임워크는 스킬 레벨, 경험치, 직업 선택과 같은 핵심 데이터를 자동으로 동기화해주는 기능을 내장하고 있습니다. 개발자가 SpaceCore의 API(AddExperience, player.professions 등)를 올바르게 사용하는 한, 대부분의 동기화 문제는 프레임워크 수준에서 처리됩니다.  
* **컨텍스트 인식**: SMAPI는 Context 클래스를 통해 현재 게임 상태에 대한 유용한 정보를 제공합니다. 예를 들어, Context.IsMainPlayer는 현재 코드를 실행하는 플레이어가 게임의 호스트인지 확인하는 데 사용될 수 있으며, Context.IsWorldReady는 모든 플레이어가 월드에 접속하여 상호작용이 가능한 상태인지 확인하는 데 사용됩니다. 이러한 컨텍스트 확인은 특정 로직이 적절한 시점과 환경에서만 실행되도록 보장하는 데 도움이 됩니다.

### **6.4. 최종 조립 및 테스트 체크리스트**

새로운 스킬 모드를 커뮤니티에 공개하기 전에, 다음 체크리스트를 통해 모든 주요 구성 요소가 올바르게 설정되었는지 최종 점검하는 것이 좋습니다.

1. **매니페스트 파일**:  
   * 로직 모드와 콘텐츠 팩의 manifest.json 파일이 각각 올바른 UniqueID를 가지고 있는가?  
   * 로직 모드의 Dependencies에 spacechase0.SpaceCore가 포함되어 있는가?  
   * 콘텐츠 팩의 ContentPackFor가 Pathoschild.ContentPatcher를 정확히 가리키고 있는가?  
2. **스킬 클래스**:  
   * 스킬 클래스가 SpaceCore.Skills.Skill을 상속하고 있는가?  
   * 생성자에서 고유한 스킬 ID를 base()로 전달하고 있는가?  
   * GetName, GetDescription, ExperienceCurve 등 필요한 모든 멤버가 구현되었는가?  
3. **직업 및 효과**:  
   * 레벨 10 직업에 ParentProfessionId가 올바르게 설정되어 분기 트리가 의도대로 작동하는가?  
   * 모든 직업의 효과가 이벤트 핸들러 내에서 player.professions.Contains()와 같은 확인 로직을 통해 구현되었는가?  
   * 멀티플레이 환경을 고려하여 이벤트 인자의 플레이어 객체를 사용하고 있는가?  
4. **콘텐츠 패처**:  
   * content.json의 Target 경로가 정확한가?  
   * ToArea 좌표가 원본 스프라이트 시트의 비어있는 공간을 정확히 가리키는가?  
   * 안정성을 위해 "When": { "HasMod": "..." } 조건이 포함되어 있는가?  
5. **지역화**:  
   * 게임에 표시되는 모든 문자열(스킬 이름, 설명 등)이 하드코딩되지 않고 i18n 시스템을 통해 제공되는가?

이 체크리스트를 통과한 모드는 기술적으로 견고하고, 사용자 친화적이며, 다른 모드와도 잘 어울리는 완성도 높은 작품이 될 것입니다.