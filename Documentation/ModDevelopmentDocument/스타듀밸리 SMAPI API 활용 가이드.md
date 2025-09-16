

# **완벽한 SMAPI API 가이드: 스타듀밸리 모딩을 위한 아키텍처, 구현 및 모범 사례**

---

### **Part I: 스타듀밸리 모딩과 SMAPI의 기초**

이 파트는 SMAPI 생태계를 이해하는 데 필요한 기초 지식을 다룹니다. SMAPI 자체의 고수준 아키텍처부터 모드의 실질적인 구조에 이르기까지, 개발자가 '어떻게'를 배우기 전에 '왜'를 이해할 수 있도록 구성되었습니다.

## **1장: SMAPI 프레임워크 소개**

### **1.1 SMAPI 해부: 통합 모딩 플랫폼**

SMAPI(Stardew Modding API)는 단순히 모드를 게임에 로드하는 도구가 아닙니다. 이는 안정성, 호환성, 그리고 개발 용이성을 목표로 설계된 포괄적인 모딩 프레임워크입니다.1 SMAPI의 핵심 역할은 모드 어셈블리(DLL 파일)를 게임 프로세스에 주입하고, 모드와 게임 엔진 사이의 중재자 역할을 수행하며, 게임의 내부 기능을 안전하게 제어하고 확장할 수 있는 풍부한 API 계층을 제공하는 것입니다.3

SMAPI의 작동 방식을 이해하기 위해서는 핵심적인 주입 메커니즘을 알아야 합니다. 플레이어가 게임을 실행할 때, 실제로는 StardewValley.exe가 아닌 StardewModdingAPI.exe를 실행하게 됩니다. SMAPI는 게임의 .exe와 관련 라이브러리를 어셈블리 참조로 로드한 후, 게임의 메인 Game 클래스 인스턴스를 상속받는 자체적인 하위 클래스를 생성합니다. 이 과정을 통해 SMAPI는 XNA/MonoGame 프레임워크의 핵심 생명주기 메서드인 Initialize, Update, Draw 등을 가로챌 수 있게 됩니다.5 일단 SMAPI가 게임의 메인 루프에 대한 제어권을 확보하면, 리플렉션(reflection)과 런타임 패치(runtime patching) 같은 기술을 사용하여 게임 코드 내에 이벤트 후크(hook)를 삽입하고, 모드가 게임 상태 변화에 반응할 수 있도록 하는 정교한 이벤트 시스템을 구축합니다. 이 아키텍처는 SMAPI가 제공하는 모든 기능의 기반이 되며, 모드가 게임 파일을 직접 수정하지 않고도 깊이 있는 상호작용을 할 수 있게 만드는 핵심 원리입니다.

다음 표는 SMAPI가 제공하는 핵심 API의 개요를 보여줍니다. 이 가이드의 각 장에서 이 API들을 심층적으로 다룰 것입니다.

| API 카테고리 | 인터페이스/헬퍼 이름 | 목적 | 관련 장 |
| :---- | :---- | :---- | :---- |
| 모드 구조 | Mod, IModHelper | 모드의 기본 진입점과 SMAPI 기능 접근 제공 | 2장 |
| 매니페스트 | manifest.json | 모드의 메타데이터, 의존성, 업데이트 정보 정의 | 2장 |
| 이벤트 | IEventHelper | 게임 내 이벤트(예: 시간 변경, 저장 로드)에 응답 | 3장 |
| 콘텐츠 | IContentHelper | 게임 에셋(이미지, 데이터, 맵) 로드 및 수정 | 4장 |
| 데이터 | IDataHelper | 모드의 영구 데이터 저장 및 검색 | 4장 |
| 입력 | IInputHelper | 키보드, 마우스, 컨트롤러 입력 확인 및 제어 | 5장 |
| 설정 | IModHelper.ReadConfig | config.json 파일을 통한 사용자 설정 관리 | 5장 |
| 로깅 | IMonitor | SMAPI 콘솔 및 로그 파일에 메시지 출력 | 6장 |
| 번역 | ITranslationHelper | 다국어 지원을 위한 텍스트 번역 | 6장 |
| 멀티플레이어 | IMultiplayerHelper | 멀티플레이어 환경에서 데이터 동기화 및 통신 | 6장 |
| 리플렉션 | IReflectionHelper | 비공개 코드 멤버에 대한 접근 (고급) | 6장 |
| 유틸리티 | IUtilities | 날짜, 경로, 버전 관리 등 보조 기능 제공 | 6장 |
| 모드 통합 | IModRegistry | 다른 모드에서 제공하는 API와 연동 | 7장 |
| 콘텐츠 팩 | IContentPackHelper | 프레임워크 모드가 콘텐츠 팩을 읽도록 지원 | 8장 |
| 콘솔 명령어 | IModHelper.ConsoleCommands | SMAPI 콘솔에 사용자 지정 명령어 추가 | 11장 |
| 하모니 패치 | HarmonyLib | 게임 코드 런타임 수정 (최후의 수단) | 10장 |

### **1.2 복원력 있는 생태계를 위한 핵심 아키텍처 기능**

SMAPI의 설계 철학은 단순히 모드를 실행하는 것을 넘어, 전체 모딩 생태계의 안정성과 사용자의 긍정적인 경험을 최우선으로 고려합니다. 이는 SMAPI가 제공하는 여러 지능적인 기능들을 통해 명확히 드러납니다. 이러한 기능들은 기술적 편의성을 넘어서, 모딩 커뮤니티 전체의 건강성을 유지하기 위한 전략적인 아키텍처 선택의 결과물입니다.

* **오류 가로채기 및 저장 파일 자동 복구:** 모드에서 예외가 발생했을 때, SMAPI는 이를 가로채 게임 전체가 충돌하는 것을 방지합니다. 오류 정보는 SMAPI 콘솔에 상세히 출력되어 문제 해결을 돕습니다. 더 나아가, 사용자가 특정 모드를 제거한 후 세이브 파일을 로드할 때 발생할 수 있는 충돌(예: 사용자 지정 위치나 NPC 정보 누락)을 감지하고, 문제가 되는 데이터를 자동으로 제거하여 세이브 파일을 복구하기도 합니다.3  
* **호환성 계층:** SMAPI의 가장 강력한 기능 중 하나는 모드의 컴파일된 코드(CIL)를 로드하기 전에 실시간으로 재작성하는 능력입니다. 이를 통해 리눅스, macOS, 윈도우 간의 미묘한 게임 코드 차이를 자동으로 해결하여 모드 개발자가 플랫폼별 코드를 작성할 필요가 없게 만듭니다. 또한, 게임의 마이너 업데이트로 인해 일부 코드가 변경되었을 때, SMAPI가 이를 감지하고 호환되도록 코드를 수정하여 모드가 즉시 망가지는 것을 방지하는 경우도 있습니다.3  
* **자동화된 검사 및 백업:** SMAPI는 게임 시작 시 설치된 모든 모드의 업데이트 여부를 확인하고, 새로운 버전이 있을 경우 사용자에게 알려줍니다. 또한, 오래되거나 호환되지 않는 모드를 자동으로 감지하고 비활성화하여 잠재적인 문제를 사전에 차단합니다.3 여기에 더해, SMAPI는 기본적으로  
  Save Backup 모드를 내장하고 있어, 매일 게임 저장 파일을 자동으로 백업하고 최근 10개의 백업을 유지합니다. 이는 사용자의 데이터를 보호하려는 프레임워크의 강력한 의지를 보여줍니다.3

이러한 기능들은 개별 모드 개발자의 지원 부담을 줄이고, 기술적 지식이 부족한 사용자도 비교적 안전하게 모드를 즐길 수 있는 환경을 조성합니다. 안정적인 환경은 더 많은 사용자를 유치하고, 이는 다시 더 많은 모드 개발자를 끌어들이는 선순환 구조를 만듭니다. 따라서 SMAPI의 성공은 단순히 기술적 우수함 때문이 아니라, 관리되고 안정적인 환경을 구축하려는 전략적 목표 덕분입니다.

### **1.3 전문적인 개발을 위한 환경 설정**

효율적인 SMAPI 모드 개발을 위해서는 적절한 개발 환경 구축이 필수적입니다. 이 섹션에서는 C\# 개발에 널리 사용되는 Visual Studio를 기준으로 개발 환경을 설정하는 과정을 안내합니다.

1. **프로젝트 생성:** Visual Studio를 열고 '새 프로젝트 만들기'를 선택합니다. '클래스 라이브러리(.NET Framework)' 템플릿을 선택하고 프로젝트 이름과 위치를 지정합니다. 스타듀밸리는.NET Framework를 사용하므로,.NET Core나.NET 5+가 아닌.NET Framework를 선택해야 합니다.  
2. **참조 추가:** 프로젝트가 생성되면, 솔루션 탐색기에서 '참조'를 마우스 오른쪽 버튼으로 클릭하고 '참조 추가'를 선택합니다. '찾아보기' 탭으로 이동하여 스타듀밸리 게임이 설치된 폴더로 이동합니다. 여기서 다음 파일들을 찾아 추가합니다 7:  
   * StardewValley.exe  
   * StardewModdingAPI.exe  
   * netch 폴더 안에 있는 모든 dll 파일 (특히 Netcode.dll)  
3. **빌드 후 자동 배포 설정:** 개발 과정에서 매번 컴파일된 dll 파일을 수동으로 게임의 Mods 폴더에 복사하는 것은 번거롭습니다. 이 과정을 자동화하기 위해 빌드 후 이벤트를 설정할 수 있습니다.  
   * 솔루션 탐색기에서 프로젝트를 마우스 오른쪽 버튼으로 클릭하고 '속성'을 선택합니다.  
   * '빌드 이벤트' 탭으로 이동합니다.  
   * '빌드 후 이벤트 명령줄' 텍스트 상자에 다음 명령어를 입력합니다. 이 명령어는 빌드가 성공할 때마다 모드 폴더를 생성하고 필요한 파일들을 Mods 폴더로 복사합니다. (게임 경로와 프로젝트 이름은 자신의 환경에 맞게 수정해야 합니다.)

Bash  
mkdir "%PROGRAMFILES(X86)%\\Steam\\steamapps\\common\\Stardew Valley\\Mods\\$(ProjectName)"  
copy "$(TargetPath)" "%PROGRAMFILES(X86)%\\Steam\\steamapps\\common\\Stardew Valley\\Mods\\$(ProjectName)"  
copy "$(ProjectDir)manifest.json" "%PROGRAMFILES(X86)%\\Steam\\steamapps\\common\\Stardew Valley\\Mods\\$(ProjectName)"

4. **디버깅 설정:** Visual Studio의 강력한 디버깅 기능을 활용하기 위해, 디버그 시작 동작을 SMAPI를 실행하도록 설정합니다.  
   * 프로젝트 속성의 '디버그' 탭으로 이동합니다.  
   * '시작 동작' 섹션에서 '외부 프로그램 시작'을 선택하고, StardewModdingAPI.exe의 전체 경로를 입력합니다.  
   * '명령줄 인수'에는 \--mods-path "Mods" 와 같이 필요한 SMAPI 실행 인수를 추가할 수 있습니다.  
   * '작업 디렉터리'를 스타듀밸리 게임 폴더로 설정합니다.

이 설정이 완료되면, Visual Studio에서 F5 키를 눌러 디버깅을 시작할 때마다 SMAPI와 함께 게임이 실행되고, 코드에 설정한 중단점(breakpoint)에서 실행이 멈추는 등 모든 디버깅 기능을 활용할 수 있습니다.

## **2장: SMAPI 모드의 구조**

모든 SMAPI 모드는 일관된 구조를 따릅니다. 이 구조는 SMAPI가 모드를 인식하고, 로드하며, 게임과 상호작용할 수 있도록 하는 기본 틀을 제공합니다. 이 장에서는 모드의 생명주기, 핵심 구성 요소인 manifest.json 파일, 그리고 모드 간의 관계를 정의하는 의존성 관리 시스템에 대해 자세히 알아봅니다.

### **2.1 모드 생명주기와 진입점**

SMAPI 모드를 만들기 위한 첫 단계는 SMAPI가 제공하는 StardewModdingAPI.Mod 클래스를 상속받는 것입니다. 이 클래스는 모드의 기반이 되며, SMAPI와의 통신에 필요한 기본적인 속성과 메서드를 제공합니다.

C\#

using StardewModdingAPI;

namespace MyFirstMod  
{  
    public class ModEntry : Mod  
    {  
        public override void Entry(IModHelper helper)  
        {  
            // 모드 로직이 시작되는 곳  
        }  
    }  
}

모든 모드는 Entry(IModHelper helper)라는 공개 메서드를 반드시 재정의(override)해야 합니다.7 이 메서드는 모드의 진입점(entry point) 역할을 합니다. SMAPI는 게임이 시작될 때 설치된 모든 모드를 로드하고, 모든 모드의 로드가 완료된 후에 각 모드의

Entry 메서드를 순차적으로 호출합니다. 이 시점은 다른 모드와의 상호작용을 준비하기에 안전한 첫 번째 지점입니다.

Entry 메서드는 IModHelper 타입의 helper 매개변수를 받습니다. 이 helper 객체는 모드가 SMAPI의 거의 모든 API에 접근할 수 있게 해주는 관문입니다. 예를 들어, 게임 이벤트를 구독하거나, 설정 파일을 읽거나, 다른 모드의 API를 호출하는 등의 작업은 모두 이 helper 객체를 통해 이루어집니다.

Mod 기본 클래스는 helper 외에도 세 가지 중요한 속성을 제공합니다.

* this.Helper: Entry 메서드에 전달된 helper 객체와 동일합니다. 클래스 내의 다른 메서드에서 SMAPI API에 접근해야 할 때 유용합니다.  
* this.Monitor: 로깅을 위한 IMonitor 인스턴스입니다. 이를 통해 SMAPI 콘솔과 로그 파일에 디버그 정보나 오류 메시지를 출력할 수 있습니다.  
* this.ModManifest: 모드의 manifest.json 파일에 정의된 메타데이터에 접근할 수 있는 IManifest 인스턴스입니다. 모드의 이름, 버전, 고유 ID 등의 정보를 코드 내에서 동적으로 가져올 수 있습니다.

### **2.2 매니페스트(manifest.json): 모드의 신분증**

모든 SMAPI 모드와 콘텐츠 팩은 루트 폴더에 manifest.json이라는 파일을 반드시 포함해야 합니다.9 이 파일은 모드의 이름, 제작자, 버전, 설명과 같은 기본적인 메타데이터를 담고 있으며, SMAPI가 모드를 식별하고 관리하는 데 사용하는 핵심적인 정보를 제공합니다.

이 파일의 존재는 SMAPI가 해당 폴더를 유효한 모드로 인식하기 위한 최소한의 조건입니다. 매니페스트가 없거나 형식이 잘못된 경우, SMAPI는 해당 폴더를 무시하고 오류를 출력합니다.

다음은 manifest.json 파일의 전체 필드에 대한 상세 참조 표입니다.

| 필드 이름 | 데이터 타입 | 필수/선택 | 설명 |
| :---- | :---- | :---- | :---- |
| Name | string | 필수 | 모드의 공식 이름입니다. SMAPI 콘솔 및 로그에 표시됩니다. |
| Author | string | 필수 | 모드 제작자의 이름입니다. |
| Version | string | 필수 | 모드의 버전 번호입니다. \*\*유의적 버전(Semantic Versioning)\*\*을 따르는 것이 강력히 권장됩니다. (예: "1.2.3") |
| Description | string | 필수 | 모드에 대한 간략한 설명입니다. |
| UniqueID | string | 필수 | 다른 모든 모드와 구별되는 고유한 ID입니다. 일반적으로 AuthorName.ModName 형식을 사용합니다. (예: "Pathoschild.ChestsAnywhere") |
| MinimumApiVersion | string | 선택 | 모드가 호환되는 최소 SMAPI 버전입니다. 지정된 버전보다 낮은 SMAPI에서는 모드가 로드되지 않습니다. (예: "4.0.0") |
| UpdateKeys | string | 선택 | SMAPI의 자동 업데이트 확인 기능에 사용되는 키 목록입니다. (예: \["Nexus:12345"\]) |
| Dependencies | object | 선택 | 이 모드가 실행되기 위해 필요한 다른 모드(하드 의존성) 목록입니다. 각 객체는 UniqueID와 선택적으로 MinimumVersion을 포함합니다. |
| ContentPackFor | object | 선택 | 이 모드가 특정 프레임워크 모드를 위한 콘텐츠 팩임을 나타냅니다. UniqueID 필드를 필수로 포함합니다. |
| EntryDll | string | 선택 | 모드의 진입점 dll 파일 이름입니다. 기본값은 \[프로젝트 이름\].dll이므로 대부분의 경우 생략 가능합니다. |

Version 필드는 특히 중요합니다. 유의적 버전 관리(Semantic Versioning)를 준수하면, 다른 모드 개발자들이나 사용자들이 업데이트의 성격을 쉽게 파악할 수 있습니다. 예를 들어, 주 버전(MAJOR)이 변경되면 API에 호환되지 않는 변경이 있었음을 의미하므로, 해당 모드의 API를 사용하는 다른 모드들은 업데이트가 필요하다는 것을 알 수 있습니다. 이 주제는 4부에서 더 자세히 다룹니다.

### **2.3 모드 생태계 관리: 의존성**

스타듀밸리 모딩 커뮤니티는 단일 모드들이 독립적으로 작동하는 것을 넘어, 여러 모드들이 서로 협력하고 기능을 확장하는 복잡한 생태계를 이루고 있습니다. 이러한 생태계의 안정성은 manifest.json 파일에 정의된 명시적인 의존성 관리 시스템에 크게 의존합니다. 이 시스템은 모드 간의 관계를 공식적인 '계약'으로 정의하며, 이를 통해 SMAPI는 모드를 로드하기 전에 유효한 의존성 그래프를 구성할 수 있습니다. 이 형식적이고 선언적인 시스템은 복잡한 모드 환경에서 발생할 수 있는 수많은 잠재적 오류를 예방하는 핵심적인 안전장치입니다.

* **하드 의존성 (Hard Dependencies):** Dependencies 필드는 특정 모드가 실행되기 위해 다른 모드가 반드시 설치되어 있어야 함을 명시합니다.8 예를 들어, 모드 B가 모드 A의 API를 사용하는 경우, 모드 B의 매니페스트에 모드 A를 의존성으로 추가해야 합니다.  
  JSON  
  "Dependencies":

  이 경우, SMAPI는 모드 B를 로드하기 전에 Content Patcher 모드가 설치되어 있는지, 그리고 버전이 2.0.0 이상인지 확인합니다. 조건이 충족되지 않으면 SMAPI는 모드 B를 로드하지 않고, 사용자에게 어떤 모드를 설치하거나 업데이트해야 하는지 명확한 오류 메시지를 보여줍니다. 이는 API를 제공하는 모드가 없어 발생하는 런타임 충돌을 사전에 방지합니다.  
* **프레임워크 의존성 (Framework Dependencies):** ContentPackFor 필드는 해당 모드가 일반적인 C\# 모드가 아니라, 특정 프레임워크 모드(예: Content Patcher, Dynamic Game Assets)를 위한 콘텐츠 팩임을 선언합니다.10  
  JSON  
  "ContentPackFor": {  
      "UniqueID": "Pathoschild.ContentPatcher"  
  }

  이 선언을 통해 SMAPI는 이 폴더를 Content Patcher가 관리해야 할 콘텐츠 팩으로 인식하고, Content Patcher 모드에 이 팩의 정보를 전달합니다. 그러면 Content Patcher는 해당 팩의 content.json 파일을 읽어 게임 에셋을 수정하는 작업을 수행합니다. 이러한 '프레임워크 \-\> 콘텐츠 팩' 모델은 스타듀밸리 모딩의 핵심적인 패러다임입니다. 의존성 시스템이 없다면, 프레임워크 모드는 자신을 위해 만들어진 콘텐츠 팩들을 안정적으로 찾아 로드할 방법이 없으며, 생태계는 훨씬 더 파편화되고 불안정해질 것입니다. 이 시스템 덕분에 Stardew Valley Expanded와 같은 대규모 모드가 수십 개의 다른 모드 위에 안정적으로 구축될 수 있습니다.

---

### **Part II: 핵심 SMAPI API 심층 분석**

이 파트는 가이드의 기술적인 핵심으로, 가장 일반적으로 사용되는 SMAPI API들을 철저하게 분석합니다. 각 장은 논리적으로 그룹화된 기능에 초점을 맞추며, 구체적인 코드 예제와 모범 사례를 함께 제공합니다.

## **3장: 게임에 대한 응답: 이벤트 API**

SMAPI 모드가 게임과 상호작용하는 가장 기본적이고 비침습적인 방법은 이벤트 API를 사용하는 것입니다. 이벤트 기반 모델은 모드가 게임의 특정 시점이나 상황에 반응하여 코드를 실행할 수 있도록 해줍니다. 예를 들어, '하루가 시작될 때', '플레이어가 맵을 이동할 때', '새로운 아이템이 월드에 배치될 때'와 같은 순간을 포착하여 원하는 로직을 수행할 수 있습니다.

### **3.1 SMAPI의 이벤트 기반 모델**

SMAPI의 모든 이벤트는 IEventHelper 인터페이스(주로 helper.Events를 통해 접근)에 정의되어 있습니다. 모드는 특정 이벤트에 자신의 메서드(이벤트 핸들러)를 '구독'함으로써 해당 이벤트가 발생할 때마다 알림을 받습니다. C\#에서 이벤트 구독은 \+= 연산자를 사용하여 이루어집니다.7

C\#

public override void Entry(IModHelper helper)  
{  
    helper.Events.GameLoop.DayStarted \+= this.OnDayStarted;  
}

private void OnDayStarted(object sender, DayStartedEventArgs e)  
{  
    // 매일 아침 6시에 이 코드가 실행됩니다.  
    this.Monitor.Log("좋은 아침입니다\!", LogLevel.Info);  
}

여기서 가장 중요한 점은 반드시 \+= 연산자를 사용해야 한다는 것입니다. 만약 실수로 \= 연산자를 사용하면, 해당 이벤트에 이미 구독되어 있던 다른 모든 모드의 핸들러가 제거되어 버립니다. 이는 다른 모드의 기능을 망가뜨리고 진단하기 어려운 버그를 유발하는 심각한 실수이므로 각별히 주의해야 합니다.

### **3.2 게임 상태 및 생명주기 이벤트 (GameLoop)**

helper.Events.GameLoop는 게임의 핵심적인 생명주기와 관련된 가장 중요한 이벤트들을 포함하고 있습니다. 이 이벤트들은 모드의 초기화, 데이터 로딩, 주기적인 업데이트 등을 관리하는 데 필수적입니다.

* GameLaunched: 이 이벤트는 모든 모드가 로드되고 각 모드의 Entry 메서드가 호출된 직후, 하지만 게임의 타이틀 화면이 표시되기 전에 단 한 번 발생합니다. 이 시점은 다른 모드의 API를 요청하고 연동 작업을 수행하기에 가장 이상적인 시점입니다. 왜냐하면 이 시점에서는 모든 모드가 초기화되었음이 보장되기 때문입니다.8  
  C\#  
  helper.Events.GameLoop.GameLaunched \+= this.OnGameLaunched;

  private void OnGameLaunched(object sender, GameLaunchedEventArgs e)  
  {  
      // 다른 모드의 API를 가져옵니다.  
      var someApi \= this.Helper.ModRegistry.GetApi\<ISomeApi\>("some.mod.uniqueid");  
      if (someApi\!= null)  
      {  
          // API를 사용하는 로직 초기화  
      }  
  }

* SaveLoaded: 플레이어가 세이브 파일을 로드하거나 새로 시작하여 게임 플레이가 실제로 시작될 때 발생합니다. 이 이벤트는 저장 파일에 특정한 데이터(예: 플레이어 정보, 농장 상태)에 접근해야 하는 로직을 초기화하는 데 사용됩니다.  
* DayStarted: 게임 내에서 새로운 하루가 시작될 때(아침 6시) 발생합니다. 매일 반복되는 작업을 수행하기에 완벽한 시점입니다. 예를 들어, 특정 NPC의 위치를 설정하거나, 매일 새로운 아이템을 생성하는 등의 로직을 여기에 배치할 수 있습니다.  
* TimeChanged: 게임 내 시간이 10분 단위로 변경될 때마다 발생합니다. 특정 시간에 무언가를 실행해야 할 때 유용합니다. 예를 들어, 오후 8시에 상점이 닫히는 알림을 표시하는 기능을 구현할 수 있습니다.  
* UpdateTicked: 게임의 매 프레임(업데이트 틱)마다 발생합니다. 이 이벤트는 매우 빈번하게(보통 초당 60회) 호출되므로, 성능에 민감한 영향을 미칠 수 있습니다. 플레이어의 위치를 실시간으로 추적하거나 부드러운 애니메이션을 구현하는 등, 매 프레임 실행되어야만 하는 로직에만 제한적으로 사용해야 합니다. 가능하면 TimeChanged와 같은 덜 빈번한 이벤트를 사용하는 것이 좋습니다.7

### **3.3 플레이어 및 월드 상호작용 이벤트**

GameLoop 이벤트 외에도, 플레이어의 행동이나 게임 세계의 변화에 직접적으로 반응하는 다양한 이벤트들이 있습니다.

* IPlayerEvents: helper.Events.Player를 통해 접근하며, 플레이어와 관련된 이벤트들을 포함합니다.  
  * Warped: 플레이어가 한 위치에서 다른 위치로 이동(워프)할 때마다 발생합니다. 이벤트 인자(WarpedEventArgs)는 이전 위치와 새로운 위치 정보를 포함하므로, 특정 장소에 들어왔을 때 특별한 효과를 주거나 로직을 실행하는 데 사용할 수 있습니다.  
  * InventoryChanged: 플레이어의 인벤토리에 아이템이 추가되거나 제거될 때 발생합니다. 특정 아이템 획득을 감지하는 퀘스트 시스템 등을 구현할 때 유용합니다.  
* IWorldEvents: helper.Events.World를 통해 접근하며, 게임 세계의 객체 변화와 관련된 이벤트들을 포함합니다.  
  * ObjectListChanged: 맵에 아이템(오브젝트)이 배치되거나 제거될 때 발생합니다. 예를 들어, 플레이어가 밭에 씨앗을 심거나 용광로를 설치하는 것을 감지할 수 있습니다.  
  * BuildingListChanged: 농장에 건물이 건설되거나 철거될 때 발생합니다.

이러한 세분화된 이벤트들을 적절히 활용하면, 모드는 게임의 흐름에 자연스럽게 통합되어 마치 원래 게임의 일부인 것처럼 느껴지는 정교한 기능을 구현할 수 있습니다.

## **4장: 게임 콘텐츠 관리: 콘텐츠 및 데이터 API**

스타듀밸리의 모든 시각적, 데이터적 요소(예: 아이템 이미지, NPC 초상화, 작물 정보, 맵 구조)는 '에셋(asset)'이라는 단위로 관리됩니다. SMAPI의 콘텐츠 API는 모드가 이러한 에셋을 읽고, 수정하며, 심지어 완전히 새로운 에셋을 게임에 추가할 수 있도록 하는 강력한 메커니즘을 제공합니다. 이 모든 과정은 실제 게임 파일을 변경하지 않고 메모리상에서 안전하게 이루어집니다.

### **4.1 SMAPI 에셋 파이프라인**

SMAPI의 콘텐츠 시스템을 이해하는 핵심은 SMAPI가 게임의 모든 에셋 로딩 요청을 가로챈다는 사실을 아는 것입니다.12 게임이

Data/Objects와 같은 특정 에셋을 요청하면, 이 요청은 먼저 SMAPI를 통과합니다. 이 과정에서 SMAPI는 관련 이벤트를 발생시켜 모드들이 해당 에셋에 개입할 기회를 제공합니다. 모드들이 에셋을 수정하거나 교체한 후, 최종 결과물이 게임에 전달됩니다.

각 에셋은 고유한 '에셋 이름'으로 식별됩니다. 이는 게임의 Content 폴더를 기준으로 한 상대 경로이며, 파일 확장자(.xnb)는 포함하지 않습니다. 예를 들어, 아비게일의 초상화 파일인 Content/Portraits/Abigail.xnb의 에셋 이름은 Portraits/Abigail입니다.

### **4.2 사용자 지정 에셋 로드 (IModContentHelper)**

모드는 자신만의 에셋(이미지, 데이터 파일 등)을 포함하고 이를 게임 내에서 사용할 수 있습니다. 이는 helper.ModContent.Load\<T\>() 메서드를 통해 이루어집니다.12 이 메서드는 모드의 폴더 내에 있는 파일을 지정된 타입으로 로드합니다.

C\#

using Microsoft.Xna.Framework.Graphics;

//... 클래스 내...  
private Texture2D customButtonTexture;

public override void Entry(IModHelper helper)  
{  
    // 모드가 로드될 때 텍스처를 한 번만 로드하여 필드에 저장합니다.  
    this.customButtonTexture \= helper.ModContent.Load\<Texture2D\>("assets/custom\_button.png");  
}

//... 나중에 이 텍스처를 사용...  
// e.g., in a draw event  
// spriteBatch.Draw(this.customButtonTexture,...);

성능을 위해, Draw나 UpdateTicked와 같이 자주 호출되는 메서드 내에서 Load\<T\>()를 반복적으로 호출하는 것은 피해야 합니다. 대신, 위 예제처럼 모드가 로드될 때 에셋을 한 번만 로드하고, 그 결과를 클래스의 필드나 속성에 캐시(저장)해두고 재사용하는 것이 모범 사례입니다.12

때로는 모드가 로드한 에셋을 게임의 기본 코드에 전달해야 할 수도 있습니다. 이 경우, SMAPI가 내부적으로 사용하는 고유한 에셋 키가 필요할 수 있습니다. helper.ModContent.GetActualAssetKey() 메서드를 사용하면 이 키를 얻을 수 있습니다.12

### **4.3 게임 에셋 편집 및 교체 (IContentEvents)**

기존 게임 에셋을 수정하거나 완전히 다른 것으로 교체하는 작업은 AssetRequested 이벤트를 통해 이루어집니다. 이 이벤트는 게임이 에셋을 요청할 때마다 발생하며, 모드는 이 기회를 이용해 에셋의 내용을 변경할 수 있습니다.

C\#

public override void Entry(IModHelper helper)  
{  
    helper.Events.Content.AssetRequested \+= this.OnAssetRequested;  
}

private void OnAssetRequested(object sender, AssetRequestedEventArgs e)  
{  
    // 1\. 특정 에셋을 대상으로 지정합니다.  
    if (e.Name.IsEquivalentTo("Data/Crops"))  
    {  
        // 2\. 에셋을 수정합니다.  
        e.Edit(asset \=\>  
        {  
            var data \= asset.AsDictionary\<int, string\>().Data;  
            // 파스닙(ID 24)의 성장 기간을 1일로 변경합니다.  
            string fields \= data.Split('/');  
            fields \= "1"; // 성장 단계  
            data \= string.Join("/", fields);  
        });  
    }  
    // 'Stone' 아이템의 텍스처를 사용자 지정 이미지로 완전히 교체합니다.  
    else if (e.Name.IsEquivalentTo("Maps/springobjects"))  
    {  
        e.Edit(asset \=\>  
        {  
            var editor \= asset.AsImage();  
            Texture2D sourceImage \= this.Helper.ModContent.Load\<Texture2D\>("assets/custom\_stone.png");  
            // springobjects 시트에서 Stone(ID 390\) 텍스처가 있는 위치에 덮어씁니다.  
            // 좌표는 타일 인덱스를 기반으로 계산해야 합니다.  
            int tileID \= 390;  
            int tilesheetWidth \= 16; // 타일 단위 너비  
            var targetArea \= new Microsoft.Xna.Framework.Rectangle(  
                (tileID % tilesheetWidth) \* 16,  
                (tileID / tilesheetWidth) \* 16,  
                16, 16);  
            editor.PatchImage(sourceImage, targetArea: targetArea, patchMode: PatchMode.Replace);  
        });  
    }  
    // 농장 지도를 완전히 다른 맵 파일로 교체합니다.  
    else if (e.Name.IsEquivalentTo("Maps/Farm\_Combat"))  
    {  
        e.LoadFromModFile\<xTile.Map\>("assets/MyCustomFarm.tmx", AssetLoadPriority.High);  
    }  
}

* **조건부 편집:** e.Name.IsEquivalentTo("asset/name")을 사용하여 특정 에셋만 정확하게 타겟팅하는 것이 중요합니다.  
* **에셋 수정 (e.Edit):** e.Edit() 메서드는 에셋 데이터를 수정하는 함수를 인자로 받습니다. asset.AsDictionary(), asset.AsImage(), asset.AsMap()과 같은 헬퍼 메서드를 사용하면 에셋을 다루기 쉬운 형태로 변환할 수 있습니다. 이미지 패치의 경우, PatchMode를 통해 기존 이미지 위에 겹쳐 그릴지(Overlay), 아니면 해당 영역을 완전히 대체할지(Replace) 결정할 수 있습니다.12  
* **에셋 교체 (e.LoadFromModFile):** e.LoadFromModFile()은 게임의 에셋을 모드 폴더에 있는 파일로 완전히 교체합니다.  
* **로드 우선순위:** 여러 모드가 동일한 에셋을 수정하거나 교체하려고 할 때 충돌이 발생할 수 있습니다. SMAPI는 AssetLoadPriority와 AssetEditPriority를 통해 이 순서를 제어합니다. High, Medium, Low 등의 우선순위를 지정하여 자신의 변경 사항이 다른 모드보다 먼저 또는 나중에 적용되도록 할 수 있습니다. AssetLoadPriority.Exclusive는 다른 모드의 로드를 무시하고 자신의 로드를 강제하는 강력한 옵션이지만, 모드 호환성을 크게 해칠 수 있어 신중하게 사용해야 합니다.12

### **4.4 영구적인 모드 데이터 (IDataHelper)**

모드가 게임 세션 간에 유지해야 하는 자체적인 데이터를 저장해야 할 때가 있습니다. 예를 들어, 모드가 추가한 새로운 NPC와의 호감도나, 플레이어가 모드 관련 퀘스트를 어디까지 진행했는지 등의 정보입니다. 이러한 데이터는 게임의 기본 저장 파일에 포함되지 않으므로, 별도의 파일에 저장해야 합니다. IDataHelper API는 이 과정을 단순화합니다.9

helper.Data.WriteJsonFile과 helper.Data.ReadJsonFile 메서드를 사용하면, 사용자 정의 클래스 객체를 Mods/\[모드이름\]/data 폴더에 JSON 파일 형태로 쉽게 저장하고 불러올 수 있습니다.

C\#

// 저장할 데이터를 담을 클래스  
public class ModData  
{  
    public int TimesPlayerHasSlept { get; set; } \= 0;  
}

//... 모드 클래스 내...  
private ModData data;

public override void Entry(IModHelper helper)  
{  
    // 세이브 파일이 로드될 때 모드 데이터를 읽어옵니다.  
    helper.Events.GameLoop.SaveLoaded \+= (s, e) \=\>  
    {  
        this.data \= this.Helper.Data.ReadJsonFile\<ModData\>($"data/{Constants.SaveFolderName}.json")?? new ModData();  
    };

    // 하루가 시작될 때 데이터를 수정합니다.  
    helper.Events.GameLoop.DayStarted \+= (s, e) \=\>  
    {  
        this.data.TimesPlayerHasSlept++;  
        this.Monitor.Log($"플레이어가 총 {this.data.TimesPlayerHasSlept}번 잠을 잤습니다.", LogLevel.Debug);  
    };

    // 게임이 저장될 때 모드 데이터를 파일에 씁니다.  
    helper.Events.GameLoop.Saving \+= (s, e) \=\>  
    {  
        this.Helper.Data.WriteJsonFile($"data/{Constants.SaveFolderName}.json", this.data);  
    };  
}

이 방식을 사용하면 각 세이브 파일(Constants.SaveFolderName)마다 별도의 데이터 파일을 유지하여, 모드의 상태를 안전하고 독립적으로 관리할 수 있습니다.

## **5장: 사용자 상호작용 및 설정**

훌륭한 모드는 게임에 새로운 기능을 추가할 뿐만 아니라, 사용자가 그 기능을 자신의 취향에 맞게 조정할 수 있도록 지원합니다. SMAPI는 사용자 입력을 처리하는 API와 모드의 설정을 쉽게 관리할 수 있는 표준화된 방법을 제공하여, 개발자가 사용자 친화적인 모드를 만들 수 있도록 돕습니다.

### **5.1 입력 API (IInputHelper)**

IInputHelper API는 키보드, 마우스, 게임패드의 입력을 감지하고 제어하는 데 사용됩니다. 이를 통해 사용자 지정 단축키를 만들거나, 특정 상황에서 기본 게임 입력을 무시하고 모드 고유의 동작을 구현할 수 있습니다.9

주요 메서드는 다음과 같습니다.

* IsDown(SButton button): 특정 버튼이 현재 눌려 있는지 확인합니다.  
* Suppress(SButton button): 특정 버튼 입력을 '소비'하여, 게임의 기본 로직이 해당 입력을 처리하지 못하게 합니다.

다음 예제는 F5 키를 눌렀을 때 플레이어의 돈을 1,000골드 추가하고, 이 키 입력이 다른 용도로 사용되지 않도록 막는 방법을 보여줍니다.

C\#

public override void Entry(IModHelper helper)  
{  
    helper.Events.Input.ButtonPressed \+= this.OnButtonPressed;  
}

private void OnButtonPressed(object sender, ButtonPressedEventArgs e)  
{  
    // F5 키가 눌렸는지 확인합니다.  
    if (e.Button \== SButton.F5)  
    {  
        // 이 입력이 다른 모드나 게임 자체에서 처리되지 않도록 합니다.  
        this.Helper.Input.Suppress(e.Button);

        // 플레이어에게 1000골드를 추가합니다.  
        Game1.player.Money \+= 1000;  
        this.Monitor.Log("1000골드를 추가했습니다\!", LogLevel.Info);  
    }  
}

Suppress 메서드는 입력 충돌을 방지하는 데 매우 중요합니다. 만약 Suppress를 호출하지 않으면, F5 키가 게임 내 다른 기능(예: 스크린샷 찍기)에 할당되어 있을 경우 두 가지 동작이 동시에 일어날 수 있습니다.

### **5.2 설정 API**

대부분의 모드는 사용자가 조정할 수 있는 몇 가지 옵션을 가지고 있습니다(예: 기능 활성화/비활성화, 속도 조절, 단축키 변경). SMAPI는 config.json이라는 표준화된 파일을 통해 이러한 설정을 관리하는 간단한 방법을 제공합니다.9

1. **설정 클래스 생성:** 먼저, 모드의 설정을 담을 간단한 C\# 클래스를 만듭니다. 이 클래스의 속성(property)들은 config.json 파일의 필드와 일대일로 매핑됩니다.  
   C\#  
   public class ModConfig  
   {  
       public SButton MoneyKey { get; set; } \= SButton.F5;  
       public int AmountToAdd { get; set; } \= 1000;  
       public bool EnableMod { get; set; } \= true;  
   }

2. **설정 파일 로드:** 모드의 Entry 메서드에서 helper.ReadConfig\<T\>()를 호출하여 config.json 파일을 읽어옵니다. 파일이 존재하지 않으면, SMAPI는 설정 클래스에 정의된 기본값을 사용하여 자동으로 파일을 생성합니다.  
   C\#  
   private ModConfig Config;

   public override void Entry(IModHelper helper)  
   {  
       this.Config \= this.Helper.ReadConfig\<ModConfig\>();

       helper.Events.Input.ButtonPressed \+= this.OnButtonPressed;  
   }

   private void OnButtonPressed(object sender, ButtonPressedEventArgs e)  
   {  
       // 설정 파일에서 모드가 비활성화되었는지 확인합니다.  
       if (\!this.Config.EnableMod)  
           return;

       // 설정 파일에 정의된 키와 눌린 키를 비교합니다.  
       if (e.Button \== this.Config.MoneyKey)  
       {  
           this.Helper.Input.Suppress(e.Button);

           // 설정 파일에 정의된 금액을 추가합니다.  
           Game1.player.Money \+= this.Config.AmountToAdd;  
           this.Monitor.Log($"{this.Config.AmountToAdd}골드를 추가했습니다\!", LogLevel.Info);  
       }  
   }

이 표준화된 config.json 시스템은 스타듀밸리 모딩 생태계의 사용자 친화성에 크게 기여합니다. 사용자는 텍스트 편집기로 직접 config.json 파일을 수정할 수 있으며, 더 중요한 것은 이 시스템이 다른 모드와의 통합을 가능하게 한다는 점입니다. 커뮤니티에서 널리 사용되는 'Generic Mod Config Menu (GMCM)'라는 모드는, 이 표준 설정 클래스 구조를 사용하는 모든 모드에 대해 게임 내에서 직접 설정을 변경할 수 있는 그래픽 인터페이스를 자동으로 생성해 줍니다. 개발자는 별도의 UI 코드를 작성할 필요 없이, 단지 설정 클래스를 만드는 것만으로도 사용자에게 편리한 설정 메뉴를 제공할 수 있습니다. 이는 SMAPI가 기본 기능을 제공하고, 다른 모드가 그 위에 향상된 UI 계층을 추가하는 생태계의 계층적 아키텍처를 보여주는 훌륭한 예입니다. 이러한 구조는 개발자들이 자신의 모드를 설정 가능하게 만들도록 장려하며, 최종 사용자의 경험을 크게 향상시킵니다.

## **6장: 필수 모딩 유틸리티**

SMAPI는 모드 개발을 더 쉽고 효율적으로 만들어주는 다양한 보조 API, 즉 유틸리티를 제공합니다. 이러한 유틸리티는 로깅, 번역, 멀티플레이어 지원, 그리고 일반적인 프로그래밍 작업을 돕는 도구들을 포함합니다. 이들을 잘 활용하면 코드의 안정성과 품질을 높일 수 있습니다.

### **6.1 로깅 (IMonitor): 주요 디버깅 도구**

모드 개발 과정에서 가장 중요한 도구 중 하나는 로깅입니다. IMonitor API(this.Monitor를 통해 접근)는 SMAPI 콘솔 창과 로그 파일(ErrorLogs 폴더에 생성됨)에 메시지를 출력하는 기능을 제공합니다. 이를 통해 모드의 현재 상태를 확인하고, 버그의 원인을 추적하며, 사용자에게 중요한 정보를 전달할 수 있습니다.13

this.Monitor.Log() 메서드는 메시지 문자열과 함께 LogLevel을 인자로 받습니다. 로그 레벨은 메시지의 목적과 중요도를 나타내는 의미론적 정보를 전달하므로, 상황에 맞는 레벨을 사용하는 것이 중요합니다.

| LogLevel | 대상 | 목적 | 예시 메시지 |
| :---- | :---- | :---- | :---- |
| Trace | 개발자 | 저수준의 상세한 문제 해결 정보. 사용자가 로그 파일을 보낼 때 유용합니다. | "Player position checked at (X, Y)." |
| Debug | 개발자/고급 사용자 | 일반적인 문제 해결에 필요한 정보. | "Configuration loaded. MoneyKey is set to F5." |
| Info | 사용자 | 사용자에게 유용한 일반 정보. 남용하지 않도록 주의해야 합니다. | "Custom farm map loaded successfully." |
| Warn | 사용자 | 사용자가 인지해야 할 잠재적인 문제. 드물게 사용해야 합니다. | "Could not find texture 'assets/old\_texture.png', using default." |
| Error | 사용자/개발자 | 무언가 잘못되었음을 나타내는 오류 메시지. | "Failed to parse custom data file. Error:..." |
| Alert | 사용자 | 사용자 조치가 필요한 중요한 정보. SMAPI 자체에서 주로 사용하며, 모드는 거의 사용하지 않습니다. | "A new version of YourMod is available\!" |

C\#

this.Monitor.Log("상세 추적 메시지입니다.", LogLevel.Trace);  
this.Monitor.Log("디버깅 정보입니다.", LogLevel.Debug);  
this.Monitor.Log("일반 정보 메시지입니다.", LogLevel.Info);  
this.Monitor.Log("경고 메시지입니다.", LogLevel.Warn);  
this.Monitor.Log("오류 메시지입니다.", LogLevel.Error);

때로는 동일한 경고가 반복적으로 발생하여 콘솔을 어지럽힐 수 있습니다. LogOnce 메서드를 사용하면 게임 실행 당 한 번만 메시지를 기록하여 이를 방지할 수 있습니다.13

C\#

// 이 경고는 게임이 실행되는 동안 한 번만 표시됩니다.  
this.Monitor.LogOnce("이 모드의 오래된 API가 사용되었습니다. 업데이트를 확인하세요.", LogLevel.Warn);

### **6.2 번역 (ITranslationHelper)**

스타듀밸리는 전 세계적으로 사랑받는 게임이며, 모드 역시 다국어를 지원하는 것이 좋습니다. ITranslationHelper API는 국제화(i18n)를 쉽게 구현할 수 있도록 돕습니다.9

모드 폴더에 i18n이라는 하위 폴더를 만들고, 그 안에 default.json 파일을 생성합니다. 이 파일은 키-값 쌍의 형태로 기본 언어(보통 영어)의 텍스트를 저장합니다.

**i18n/default.json:**

JSON

{  
  "greeting.morning": "Good morning, farmer\!",  
  "item.magic-stone.name": "Magic Stone"  
}

다른 언어(예: 한국어)를 지원하려면, 해당 언어 코드(예: ko.json)로 파일을 추가하면 됩니다.

**i18n/ko.json:**

JSON

{  
  "greeting.morning": "좋은 아침입니다, 농부님\!",  
  "item.magic-stone.name": "마법의 돌"  
}

코드 내에서는 helper.Translation.Get("key")를 사용하여 번역된 문자열을 가져옵니다. SMAPI는 현재 게임 언어 설정에 맞는 번역 파일을 자동으로 찾아 사용하며, 해당 언어 파일이 없으면 default.json을 사용합니다.

C\#

string greeting \= this.Helper.Translation.Get("greeting.morning");  
this.Monitor.Log(greeting, LogLevel.Info);

이 시스템을 사용하면, 코드 변경 없이 커뮤니티 기여자들이 새로운 언어 번역 파일을 추가하는 것만으로도 모드의 다국어 지원을 확장할 수 있습니다.

### **6.3 멀티플레이어 (IMultiplayerHelper)**

멀티플레이어 환경에서 모드가 올바르게 작동하도록 하는 것은 매우 중요합니다. IMultiplayerHelper는 멀티플레이어 호환성을 보장하는 데 필요한 도구들을 제공합니다.

* **데이터 동기화:** 멀티플레이에서 각 플레이어는 자신만의 게임 인스턴스를 실행합니다. 한 플레이어에게서 발생한 모드 관련 이벤트(예: 사용자 지정 기계에 아이템 넣기)는 다른 플레이어에게 자동으로 전파되지 않습니다. helper.Multiplayer.SendMessage를 사용하면 모드 간에 사용자 지정 메시지를 브로드캐스트하여 데이터를 동기화하고 원격으로 작업을 트리거할 수 있습니다.8  
* **화면별 데이터 관리:** 한 컴퓨터에서 여러 플레이어가 플레이하는 분할 화면(split-screen) 모드에서는, 일부 데이터가 각 플레이어(화면)별로 독립적으로 존재해야 합니다(예: 각 플레이어가 보고 있는 UI). PerScreen\<T\> 유틸리티는 이러한 데이터를 쉽게 관리할 수 있도록 도와줍니다. PerScreen\<T\>으로 변수를 선언하면, SMAPI는 각 화면에 대해 별도의 값을 유지하고 현재 컨텍스트에 맞는 값을 자동으로 반환합니다.9

### **6.4 리플렉션 (IReflectionHelper): 강력한 도구**

리플렉션은 코드의 비공개(private) 필드, 속성, 메서드에 접근할 수 있게 해주는 강력한.NET 기능입니다. SMAPI는 IReflectionHelper를 통해 이 기능을 더 안전하고 편리하게 사용할 수 있도록 래핑하여 제공합니다.9

하지만 리플렉션은 최후의 수단으로 사용해야 합니다. 리플렉션에 의존하는 코드는 게임의 내부 구현에 직접적으로 결합되므로, 게임이 업데이트될 때 해당 비공개 멤버의 이름이나 구조가 변경되면 모드가 쉽게 망가질 수 있습니다. SMAPI의 이벤트나 콘텐츠 API와 같이 공식적으로 지원되는 방법으로 원하는 기능을 구현할 수 없는 경우에만 제한적으로 사용해야 합니다.

C\#

// 예시: NPC의 비공개 필드에 접근 (권장되지 않음)  
IReflectionHelper reflection \= this.Helper.Reflection;  
var someNpc \= Game1.getCharacterFromName("Abigail");  
// 'isMoving'이라는 비공개 bool 필드의 값을 가져옵니다.  
bool isMoving \= reflection.GetField\<bool\>(someNpc, "isMoving").GetValue();

### **6.5 일반 유틸리티 (IUtilities 및 PathUtilities)**

SMAPI는 모드 개발에서 흔히 마주치는 작은 문제들을 해결하기 위한 다양한 유틸리티를 제공합니다.

* **날짜 및 시간 (SDate):** 게임 내 날짜를 다룰 때, 단순히 정수를 더하거나 빼는 것은 계절의 마지막 날이나 윤년 등을 고려하지 못해 버그를 유발할 수 있습니다. SDate 클래스는 "봄 28일에서 3일 더하기"와 같은 연산을 안전하고 정확하게 수행할 수 있는 메서드를 제공하여 유효하지 않은 날짜가 생성되는 것을 방지합니다.14  
* **경로 관리 (PathUtilities):** 윈도우, 리눅스, macOS는 파일 경로 구분자가 다릅니다(\\ 또는 /). PathUtilities.NormalizePathSeparators()와 같은 메서드는 현재 운영체제에 맞는 올바른 경로 형식으로 자동 변환하여, 플랫폼 간 호환성 문제를 해결해 줍니다.14  
* **버전 관리 (SemanticVersion):** SemanticVersion 클래스는 유의적 버전 문자열(예: "1.2.3-beta")을 파싱하고 비교하는 기능을 제공합니다. 이는 다른 모드의 버전을 확인하여 특정 기능의 사용 가능 여부를 결정하는 등, 모드 통합 시 매우 유용합니다.14

---

### **Part III: 고급 주제 및 프레임워크 통합**

이 파트에서는 핵심 SMAPI API를 넘어, 모드들이 서로 그리고 게임 엔진과 상호작용하는 더 복잡하고 강력한 방법들을 다룹니다. 현대 스타듀밸리 모딩을 정의하는 주요 프레임워크들에 초점을 맞춥니다.

## **7장: 모드 간 통신: 통합 API**

스타듀밸리 모딩 생태계의 진정한 힘은 개별 모드들이 서로의 기능을 확장하고 통합하는 능력에서 나옵니다. SMAPI의 통합 API는 모드들이 서로 안전하게 통신할 수 있는 표준화된 방법을 제공하여, 마치 서비스 지향 아키텍처(SOA)처럼 작동하는 모드 생태계를 가능하게 합니다.

이 시스템은 '제공자(provider)' 모드가 자신의 기능을 API로 노출하고, '소비자(consumer)' 모드가 이 API를 요청하여 사용하는 구조로 이루어집니다. 이 방식의 가장 큰 아키텍처적 장점은 '느슨한 결합(loose coupling)'입니다. 소비자 모드는 제공자 모드의 dll 파일을 직접 참조할 필요 없이, 단지 자신이 사용하고자 하는 메서드가 정의된 인터페이스만 가지고 있으면 됩니다. SMAPI가 중간에서 동적으로 이 둘을 연결해 줍니다. 이러한 구조 덕분에, 제공자 모드가 설치되어 있지 않더라도 소비자 모드는 충돌 없이 로드될 수 있습니다. API 호출 결과가 null이 될 것이고, 소비자 모드는 이 경우를 처리하여 관련 기능을 우아하게 비활성화할 수 있습니다. 이는 특정 모드 하나가 없다고 해서 연쇄적으로 다른 모드들이 작동 불능에 빠지는 것을 방지하여, 전체 생태계의 복원력을 극적으로 향상시킵니다.

### **7.1 다른 모드를 위한 API 제공하기**

자신의 모드가 다른 모드에서 사용할 수 있는 유용한 기능을 가지고 있다면, 이를 API로 제공할 수 있습니다. 안정적이고 예측 가능한 API를 제공하는 것이 중요합니다.

1. **API 인터페이스 정의:** 먼저, 외부에 노출할 메서드와 속성을 정의하는 공개 C\# 인터페이스를 만듭니다. 인터페이스를 사용하는 것은 API의 '계약'을 명확히 하고, 소비자가 구현 세부사항에 의존하지 않도록 하는 좋은 방법입니다.  
   C\#  
   // IMyCoolApi.cs  
   public interface IMyCoolApi  
   {  
       int GetMagicNumber();  
       void PerformSpecialAction(string message);  
   }

2. **API 구현:** 다음으로, 정의한 인터페이스를 구현하는 클래스를 만듭니다.  
   C\#  
   // MyCoolApi.cs  
   internal class MyCoolApi : IMyCoolApi  
   {  
       private readonly IMonitor Monitor;

       public MyCoolApi(IMonitor monitor)  
       {  
           this.Monitor \= monitor;  
       }

       public int GetMagicNumber()  
       {  
           return 42;  
       }

       public void PerformSpecialAction(string message)  
       {  
           this.Monitor.Log($"특별한 액션이 호출되었습니다: {message}", LogLevel.Info);  
       }  
   }

3. **API 반환:** 마지막으로, 메인 모드 클래스에서 GetApi() 메서드를 재정의하여 API 구현 클래스의 인스턴스를 반환합니다. 이 메서드는 다른 모드가 API를 요청할 때 SMAPI에 의해 호출됩니다.8  
   C\#  
   // ModEntry.cs  
   public class ModEntry : Mod  
   {  
       private IMyCoolApi api;

       public override void Entry(IModHelper helper)  
       {  
           this.api \= new MyCoolApi(this.Monitor);  
       }

       public override object GetApi()  
       {  
           return this.api;  
       }  
   }

API에 호환되지 않는 변경(예: 메서드 제거 또는 시그니처 변경)을 가할 때는 반드시 모드의 주 버전(MAJOR version)을 올려야 합니다. 이는 유의적 버전 관리를 통해 API 소비자들이 변경 사항을 인지하고 대응할 수 있도록 하는 중요한 약속입니다.

### **7.2 다른 모드의 API 사용하기**

다른 모드가 제공하는 API를 사용하려면, IModRegistry 헬퍼를 사용합니다.

1. **API 인터페이스 생성:** 먼저, 사용하려는 API의 메서드와 속성을 포함하는 인터페이스를 자신의 프로젝트에 만듭니다. 이 인터페이스는 제공자 모드의 API와 정확히 일치해야 합니다. 만약 제공자 모드가 공식적인 API 인터페이스를 dll과 함께 배포한다면, 그것을 직접 참조하는 것이 더 좋습니다.  
   C\#  
   // ISomeOtherApi.cs  
   public interface ISomeOtherApi  
   {  
       bool IsSomethingReady();  
   }

2. **API 요청 및 사용:** GameLoop.GameLaunched 이벤트 내에서 helper.ModRegistry.GetApi\<T\>()를 호출하여 API를 요청합니다. 이 이벤트는 모든 모드가 초기화된 후 발생하므로, API를 요청하기에 가장 안전한 시점입니다.8  
   C\#  
   public override void Entry(IModHelper helper)  
   {  
       helper.Events.GameLoop.GameLaunched \+= this.OnGameLaunched;  
   }

   private void OnGameLaunched(object sender, GameLaunchedEventArgs e)  
   {  
       // 고유 ID를 사용하여 API를 요청합니다.  
       var otherApi \= this.Helper.ModRegistry.GetApi\<ISomeOtherApi\>("AuthorName.SomeOtherMod");

       // API가 null인지 반드시 확인해야 합니다\!  
       if (otherApi \== null)  
       {  
           this.Monitor.Log("SomeOtherMod API를 찾을 수 없습니다. 관련 기능이 비활성화됩니다.", LogLevel.Warn);  
           return;  
       }

       // API 사용  
       if (otherApi.IsSomethingReady())  
       {  
           this.Monitor.Log("무언가 준비되었습니다\!", LogLevel.Info);  
       }  
   }

가장 중요한 모범 사례는 GetApi\<T\>()의 반환값이 null인지 항상 확인하는 것입니다. 이는 해당 모드가 설치되어 있지 않거나, 비활성화되었거나, API를 제공하지 않는 경우를 처리합니다. 이 확인 절차를 생략하면 NullReferenceException으로 인해 모드가 충돌할 수 있습니다.8

## **8장: 선언적 모딩: Content Patcher**

현대 스타듀밸리 모딩의 가장 중요한 패러다임 중 하나는 '프레임워크 모드'와 '콘텐츠 팩'의 분리입니다. 이 모델은 복잡한 로직을 처리하는 C\# 모드(프레임워크)와, 실제 콘텐츠(이미지, 데이터)를 담고 있는 간단한 폴더(콘텐츠 팩)로 역할을 나눕니다. 이 구조 덕분에 프로그래밍 경험이 없는 사람도 JSON 파일을 수정하는 것만으로 게임을 변경하는 모드를 만들 수 있게 되었습니다.11

이 패러다임의 중심에는 **Content Patcher**가 있습니다. Content Patcher는 게임 에셋을 동적으로 변경할 수 있게 해주는 가장 강력하고 널리 사용되는 프레임워크 모드입니다.15

### **8.1 콘텐츠 팩 패러다임**

* **프레임워크 모드:** Content Patcher와 같은 C\# 모드로, 콘텐츠 팩을 읽고 그 내용을 게임에 적용하는 모든 로직을 담당합니다.  
* **콘텐츠 팩:** manifest.json과 content.json 파일, 그리고 이미지나 데이터 파일들을 포함하는 폴더입니다. manifest.json의 ContentPackFor 필드를 통해 자신이 어떤 프레임워크를 위한 것인지 명시합니다.

이러한 역할 분리는 모드 제작의 진입 장벽을 크게 낮추었으며, 아티스트나 작가들이 코딩 없이도 자신의 창작물을 게임에 추가할 수 있는 길을 열어주었습니다.

### **8.2 Content Patcher 심층 분석**

Content Patcher의 모든 기능은 콘텐츠 팩의 루트에 있는 content.json 파일을 통해 제어됩니다. 이 파일은 Content Patcher에게 무엇을, 언제, 어떻게 변경할지를 지시하는 '패치(patch)'들의 목록을 담고 있습니다.

* **기본 구조:** content.json 파일은 Format(Content Patcher 버전)과 Changes(패치 배열)라는 두 개의 최상위 필드를 가집니다.17  
  JSON  
  {  
    "Format": "2.0.0",  
    "Changes": \[  
      // 여기에 패치들을 추가합니다.  
    \]  
  }

* **패치 액션:** 각 패치는 Action 필드를 통해 수행할 작업을 정의합니다.  
  * Load: 새로운 에셋을 로드합니다. 게임에 존재하지 않는 파일을 추가할 때 사용됩니다.  
  * EditData: 데이터 에셋(주로 키-값 쌍)의 항목을 수정하거나 추가합니다. 아이템 가격, NPC 대사 등을 변경할 때 사용됩니다.  
  * EditImage: 기존 이미지 에셋의 일부를 다른 이미지 파일로 덮어씁니다. NPC 초상화나 건물 텍스처를 변경할 때 사용됩니다.  
  * EditMap: 기존 맵에 타일이나 속성을 추가/변경합니다.18  
* **동적 토큰과 조건:** Content Patcher의 진정한 힘은 When 절과 '토큰'을 사용하여 패치가 적용될 조건을 동적으로 결정하는 능력에 있습니다. 토큰은 {{TokenName}} 형식의 플레이스홀더로, 게임의 현재 상태(계절, 날씨, 호감도 등)를 나타냅니다.18  
  다음 예제는 비가 오는 날에만 아비게일의 대사를 변경하는 패치입니다.  
  JSON  
  {  
    "Action": "EditData",  
    "Target": "Characters/Dialogue/Abigail",  
    "When": {  
      "Weather": "Rain"  
    },  
    "Entries": {  
      "Mon4": "비 오는 날은 정말 좋아. 모든 게 차분해지는 느낌이야."  
    }  
  }

  Content Patcher는 {{Season}}, {{Weather}}, {{Hearts:Abigail}} 등 수많은 내장 토큰을 제공하여, 코딩 없이도 매우 복잡하고 상황에 맞는 변화를 만들어낼 수 있습니다.

### **8.3 C\#을 통한 Content Patcher 확장**

고급 모드 개발자는 Content Patcher의 C\# API를 사용하여 자신만의 커스텀 토큰을 등록할 수 있습니다. 이를 통해 C\# 모드의 내부 상태나 로직의 결과를 콘텐츠 팩에서 조건으로 사용할 수 있게 됩니다.20

예를 들어, 플레이어가 특정 퀘스트를 완료했는지 여부를 나타내는 커스텀 토큰을 만들 수 있습니다.

1. GameLaunched 이벤트에서 Content Patcher의 API를 가져옵니다.  
2. api.RegisterToken 메서드를 사용하여 새로운 토큰을 등록합니다.

C\#

// GameLaunched 이벤트 핸들러 내에서  
var cpApi \= this.Helper.ModRegistry.GetApi\<IContentPatcherAPI\>("Pathoschild.ContentPatcher");  
if (cpApi\!= null)  
{  
    cpApi.RegisterToken(this.ModManifest, "IsMagicQuestComplete", () \=\>  
    {  
        // 플레이어가 퀘스트를 완료했는지 확인하는 로직  
        bool isComplete \= CheckIfPlayerCompletedQuest("MyMod.MagicQuest");  
        return new { isComplete.ToString() }; // 값을 문자열 배열로 반환  
    });  
}

이제 콘텐츠 팩 제작자들은 content.json에서 이 커스텀 토큰을 사용할 수 있습니다.

JSON

"When": {  
    "MyMod.Author/IsMagicQuestComplete": "true"  
}

이러한 확장성은 C\# 모드의 동적인 로직과 Content Patcher의 선언적인 편리함을 결합하여, 매우 정교하고 상호작용적인 콘텐츠 모드를 만들 수 있는 강력한 가능성을 열어줍니다.

## **9장: 새로운 콘텐츠 추가: Dynamic Game Assets (DGA)**

Content Patcher가 기존 콘텐츠를 '수정'하는 데 중점을 둔다면, \*\*Dynamic Game Assets (DGA)\*\*는 게임에 완전히 '새로운' 콘텐츠(아이템, 작물, 제작법 등)를 추가하는 데 특화된 프레임워크입니다. DGA는 현대적인 콘텐츠 추가 모딩의 표준으로 자리 잡았습니다.

### **9.1 Json Assets로부터의 진화**

DGA 이전에는 'Json Assets (JA)'라는 프레임워크가 널리 사용되었습니다. 하지만 JA는 근본적인 아키템처 문제점을 가지고 있었습니다. 가장 큰 문제는 '아이템 ID 충돌'이었습니다. JA는 모드가 추가한 아이템에 ID를 동적으로 할당했는데, 이로 인해 모드 설치 순서나 다른 모드의 추가/삭제에 따라 아이템 ID가 계속 변경되는 'ID 뒤섞임(scramble)' 현상이 발생했습니다. 이는 저장된 아이템이 다른 아이템으로 변하거나 사라지는 심각한 버그를 유발했습니다.21

Dynamic Game Assets는 이러한 문제를 해결하기 위해 처음부터 다시 설계되었습니다. DGA는 각 모드의 콘텐츠에 대해 고유하고 영구적인 ID를 생성하여, 다른 모드와의 충돌이나 ID 뒤섞임 문제 없이 안정적으로 새로운 아이템을 추가할 수 있도록 합니다. 이러한 안정성 덕분에 DGA는 JA를 대체하는 차세대 표준으로 빠르게 채택되었습니다.

### **9.2 DGA 콘텐츠 팩 제작**

DGA 콘텐츠 팩은 특정 폴더 구조와 JSON 파일을 사용하여 새로운 콘텐츠를 정의합니다. 예를 들어, 새로운 아이템을 추가하려면 Objects 폴더 안에 해당 아이템의 데이터를 정의하는 json 파일과 텍스처를 담은 png 파일을 배치합니다.24

\*\* MyStuff/Objects/MagicRock/object.json:\*\*

JSON

{  
  "Name": "Magic Rock",  
  "Description": "A rock that hums with a faint energy.",  
  "Price": 250,  
  "Category": "Mineral",  
  "Texture": "texture.png",  
  "Sprite": { "X": 0, "Y": 0, "Width": 16, "Height": 16 }  
}

DGA는 간단한 객체뿐만 아니라 새로운 작물(성장 단계, 수확물 정의), 과일 나무, 무기, 가구, 그리고 복잡한 제작법(용광로, 절임통 등)까지 다양한 종류의 콘텐츠를 지원합니다. 각 콘텐츠 유형에 대한 자세한 형식은 DGA의 공식 문서를 참조해야 합니다.

### **9.3 DGA와 프로그래밍 방식의 상호작용**

C\# 모드에서 DGA로 추가된 콘텐츠와 상호작용해야 할 때가 있습니다. 예를 들어, 퀘스트 보상으로 DGA 아이템을 주거나, 특정 DGA 아이템을 플레이어가 가지고 있는지 확인해야 할 수 있습니다. DGA는 이를 위한 C\# API를 제공합니다.

먼저, 다른 모드 API와 마찬가지로 GameLaunched 이벤트에서 DGA의 API를 가져옵니다.

C\#

public interface IDynamicGameAssetsApi  
{  
    string GetDgaItemId(string fullId);  
    object SpawnDgaItem(string fullId, int amount \= 1);  
}

//... 모드 클래스 내...  
private IDynamicGameAssetsApi dgaApi;

private void OnGameLaunched(object sender, GameLaunchedEventArgs e)  
{  
    this.dgaApi \= this.Helper.ModRegistry.GetApi\<IDynamicGameAssetsApi\>("spacechase0.DynamicGameAssets");  
}

API를 얻은 후에는 DGA 아이템을 생성하거나 ID를 조회할 수 있습니다. DGA 아이템은 /\[아이템 이름\] 형식의 전체 ID로 식별됩니다.

C\#

// 'MyMod.MyStuff/Magic Rock' 아이템을 플레이어 인벤토리에 추가합니다.  
if (this.dgaApi\!= null)  
{  
    var item \= this.dgaApi.SpawnDgaItem("MyMod.MyStuff/Magic Rock");  
    if (item is StardewValley.Object obj)  
    {  
        Game1.player.addItemToInventory(obj);  
    }  
}

이 API를 통해 C\# 모드는 DGA 콘텐츠 팩에 의해 동적으로 추가된 아이템들을 마치 게임의 기본 아이템처럼 자연스럽게 다룰 수 있으며, 이는 두 시스템 간의 강력한 시너지를 창출합니다.

## **10장: 고수준 수정: 하모니 패치**

지금까지 다룬 모든 SMAPI API는 SMAPI가 제공하는 안전한 '창구'를 통해 게임과 상호작용하는 방식이었습니다. 하지만 때로는 SMAPI가 제공하지 않는, 게임의 아주 깊숙한 곳의 로직을 변경해야만 원하는 기능을 구현할 수 있는 경우가 있습니다. 이러한 상황을 위해 존재하는 것이 **Harmony**입니다.

### **10.1 런타임 패치 소개**

Harmony는.NET 애플리케이션의 컴파일된 메서드를 실행 시간(runtime)에 동적으로 수정할 수 있게 해주는 라이브러리입니다.9 즉, 게임의 소스 코드를 직접 건드리지 않고도 특정 메서드의 동작을 바꾸거나, 앞뒤에 새로운 코드를 삽입할 수 있습니다. 이는 SMAPI의 다른 모든 API와 근본적으로 다른, 매우 강력하면서도 위험한 기술입니다.

### **10.2 결정 프레임워크: 언제 Harmony를 사용해야 하는가**

Harmony 사용 결정은 모드의 안정성과 호환성에 가장 큰 영향을 미치는 아키텍처적 선택입니다. 따라서 명확한 기준을 가지고 신중하게 접근해야 합니다. **Harmony는 항상 최후의 수단이어야 합니다**.26

| 목표/작업 | 권장 접근 방식 | 근거 및 트레이드오프 |
| :---- | :---- | :---- |
| 아이템 데이터/이미지 변경 | AssetRequested 이벤트 사용 | **안전/안정:** SMAPI가 관리하는 공식적인 방법. 다른 모드와의 호환성이 높고, 게임 업데이트에 강함. |
| 새로운 아이템/제작법 추가 | Dynamic Game Assets 프레임워크 사용 | **표준/안전:** 콘텐츠 추가를 위해 설계된 표준 프레임워크. ID 충돌 문제를 해결하며 안정적임. |
| 특정 행동(예: 도구 사용) 후 로직 추가 | 관련 이벤트(Player.Warped 등) 구독 | **비침습적/안정:** 게임의 원래 로직을 변경하지 않고, 발생한 이벤트에 반응하는 방식. 호환성이 매우 높음. |
| 게임의 비공개 데이터 읽기/쓰기 | IReflectionHelper 사용 | **주의 필요:** 게임 업데이트 시 깨질 수 있지만, Harmony보다는 덜 위험함. 로직 자체를 변경하지는 않음. |
| 이벤트가 없는 핵심 로직 변경 (예: 전투 데미지 계산 공식 수정) | **Harmony 패치** | **고위험/고성능:** 다른 방법으로는 불가능한 깊이의 수정이 가능함. 하지만 게임 충돌, 다른 모드와의 충돌 위험이 매우 높고, SMAPI가 오류를 관리하거나 호환성을 보장해줄 수 없음.26 |

Harmony를 사용하면 다음과 같은 심각한 단점들이 따릅니다:

* 게임 충돌, 미묘한 버그, 심지어 메모리 손상 오류를 유발하기 매우 쉽습니다.  
* SMAPI는 호환되지 않는 Harmony 코드를 감지하거나 수정할 수 없어, 다른 플랫폼(예: 안드로이드)이나 향후 게임 업데이트에서 모드가 쉽게 망가질 수 있습니다.  
* 다른 Harmony 모드와 예측 불가능한 방식으로 충돌할 수 있습니다.  
* SMAPI는 Harmony를 사용하는 모드가 로드될 때, 게임 안정성에 영향을 줄 수 있다는 경고를 사용자에게 표시합니다.26

### **10.3 안전하고 안정적인 패치 기법**

불가피하게 Harmony를 사용해야 한다면, 다음의 모범 사례를 철저히 준수하여 위험을 최소화해야 합니다.

1. **프로젝트 설정:** 모드의 .csproj 파일에 \<EnableHarmony\>true\</EnableHarmony\>를 추가하여 Harmony 라이브러리를 활성화합니다.26  
2. **코드 기반 API 사용:** Harmony는 어노테이션(\[HarmonyPatch\]) 방식과 코드 기반 API 방식 두 가지를 제공합니다. SMAPI의 코드 재작성 기능과의 호환성을 위해, 어노테이션 방식보다 더 안정적인 코드 기반 API(harmony.Patch(...))를 사용하는 것이 강력히 권장됩니다.26  
3. **패치 적용:** Entry 메서드에서 Harmony 인스턴스를 생성하고, 패치할 원본 메서드와 패치 메서드를 지정하여 Patch를 호출합니다.  
   C\#  
   using HarmonyLib;  
   //...

   public override void Entry(IModHelper helper)  
   {  
       var harmony \= new Harmony(this.ModManifest.UniqueID);

       harmony.Patch(  
           original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.canBePlacedHere)),  
           postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.CanBePlacedHere\_Postfix))  
       );  
   }

4. **패치 타입 선택:**  
   * Prefix: 원본 메서드 실행 **전**에 실행됩니다. 원본 메서드의 실행을 막거나 인자를 변경할 수 있습니다.  
   * Postfix: 원본 메서드 실행 **후**에 실행됩니다. 원본 메서드의 반환값을 변경할 수 있습니다. **호환성을 위해 가능한 한 Postfix를 사용하는 것이 좋습니다**.26  
   * Transpiler: 원본 메서드의 CIL(중간 언어) 코드를 직접 재작성합니다. 매우 강력하지만 극도로 복잡하고 불안정하므로, 전문가가 아니면 절대 사용해서는 안 됩니다.  
5. **가장 중요한 모범 사례: 오류 처리:** 모든 Harmony 패치 메서드는 반드시 try...catch 블록으로 감싸야 합니다. 이는 모드의 버그가 게임 전체를 충돌시키거나 다른 모드의 작동을 방해하는 것을 막는 필수적인 안전장치입니다. catch 블록에서는 예외를 상세히 로깅하고, Prefix의 경우 return true;를 통해 원본 메서드가 계속 실행되도록 보장해야 합니다.26  
   C\#  
   internal class ObjectPatches  
   {  
       private static IMonitor Monitor;

       // Entry 메서드에서 호출하여 Monitor 인스턴스를 설정합니다.  
       internal static void Initialize(IMonitor monitor)  
       {  
           Monitor \= monitor;  
       }

       // Postfix 패치는 원본 메서드의 결과를 받아 수정할 수 있습니다.  
       // '\_\_result'는 Harmony가 제공하는 특별한 이름의 매개변수입니다.  
       internal static void CanBePlacedHere\_Postfix(StardewValley.Object \_\_instance, ref bool \_\_result)  
       {  
           try  
           {  
               // 우리의 패치 로직: 특정 아이템은 항상 배치 가능하도록 만듭니다.  
               if (\_\_instance.Name \== "Magic Stone")  
               {  
                   \_\_result \= true;  
               }  
           }  
           catch (Exception ex)  
           {  
               Monitor.Log($"'{nameof(CanBePlacedHere\_Postfix)}'에서 예외 발생:\\n{ex}", LogLevel.Error);  
           }  
       }  
   }

이러한 원칙을 준수함으로써, 개발자는 Harmony의 강력한 기능을 활용하면서도 모딩 생태계 전체의 안정성에 미치는 부정적인 영향을 최소화할 수 있습니다.

---

### **Part IV: 전문적인 개발 관행**

이 마지막 파트에서는 API 자체를 넘어, 모딩 커뮤니티의 책임감 있고 효과적인 구성원이 되기 위해 필수적인 비-API 주제들, 즉 디버깅, 버전 관리, 배포 및 유지보수에 대해 다룹니다.

## **11장: 디버깅 및 문제 해결**

버그 없는 코드는 없습니다. 효과적인 디버깅 기술은 성공적인 모드 개발의 핵심입니다. SMAPI는 개발자가 문제를 신속하게 진단하고 해결할 수 있도록 강력한 도구와 관행을 제공합니다.

### **11.1 SMAPI 로그 마스터하기**

SMAPI 로그 파일은 모든 문제 해결의 시작점입니다. 이 파일에는 게임 시작 시 로드된 모드 목록, 발생한 모든 경고 및 오류에 대한 상세 정보가 기록됩니다. 사용자로부터 버그를 보고받을 때, 가장 먼저 요청해야 할 것이 바로 이 로그 파일입니다.27

* **로그 파일 위치:** SMAPI 로그는 게임 폴더 내의 ErrorLogs 폴더에 생성됩니다.  
* **로그 파서 사용:** SMAPI는 공식 로그 파서 웹사이트(smapi.io/log)를 제공합니다. 사용자는 자신의 로그 파일을 이 사이트에 업로드하기만 하면, 읽기 쉽고 공유하기 편한 형식으로 분석된 결과를 얻을 수 있습니다. 이 파서는 일반적인 오류를 자동으로 감지하고 해결 방법을 제안해주기도 합니다.13 개발자는 항상 텍스트 파일 그대로가 아닌, 파싱된 로그 링크를 요청하는 습관을 들여야 합니다.

### **11.2 SMAPI 콘솔 명령어 활용**

SMAPI 콘솔은 개발 중에 모드를 테스트하고 디버깅하는 데 유용한 내장 명령어들을 제공합니다.

* **기본 명령어:**  
  * help: 사용 가능한 모든 명령어 목록을 보여줍니다.  
  * help \[command\_name\]: 특정 명령어에 대한 상세한 도움말을 보여줍니다.29  
* **디버깅에 유용한 명령어:**  
  * debug itemnamed \[이름\]\[개수\]\[품질\]: 특정 이름의 아이템을 인벤토리에 생성합니다. 새로운 아이템을 테스트할 때 매우 유용합니다. (예: debug itemnamed "Magic Stone" 1 4).29  
  * patch summary: Content Patcher가 적용한 모든 패치의 상태를 요약해서 보여줍니다. 특정 패치가 왜 적용되지 않았는지(예: 조건 불일치, 대상 에셋 오류) 진단하는 데 필수적인 도구입니다.19  
  * log\_context: 버튼 누름, 메뉴 변경 등 더 상세한 컨텍스트 정보를 로깅하도록 설정합니다. 특정 키 코드나 메뉴 이름을 알아낼 때 유용합니다.29  
* **사용자 지정 명령어 추가:** 모드에 테스트용 기능을 자주 실행해야 한다면, helper.ConsoleCommands.Add를 사용하여 자신만의 콘솔 명령어를 추가할 수 있습니다. 이를 통해 복잡한 테스트 시나리오를 명령어 한 줄로 트리거할 수 있습니다.9

### **11.3 일반적인 모딩 함정**

개발자들이 흔히 겪는 문제들을 미리 알아두면 많은 시간을 절약할 수 있습니다.

* **Null API 확인 누락:** helper.ModRegistry.GetApi\<T\>() 호출 후 반환값이 null인지 확인하지 않으면, 해당 API 제공 모드가 없을 때 NullReferenceException이 발생합니다.  
* **이벤트 구독 연산자 오류:** 이벤트 구독 시 \+= 대신 \=를 사용하면 다른 모드의 기능을 망가뜨립니다.  
* **에셋 경로 문제:** 에셋 경로는 대소문자를 구분하며, 운영체제에 따라 경로 구분자가 다를 수 있습니다. PathUtilities를 사용하여 경로를 정규화하는 것이 안전합니다.  
* **멀티플레이어 비동기화:** 멀티플레이어 환경에서 한 클라이언트에서만 상태를 변경하고 다른 클라이언트와 동기화하지 않으면 게임 상태가 비동기화(desync)됩니다. 상태 변경은 모든 플레이어에게 전파되어야 합니다.  
* **고무 오리 디버깅 (Rubber Duck Debugging):** 코드가 왜 작동하지 않는지 막혔을 때, 고무 오리(또는 동료, 혹은 상상의 인물)에게 코드 한 줄 한 줄을 말로 설명해보는 기법입니다. 문제를 설명하는 과정에서 스스로 논리적 오류를 발견하게 되는 경우가 많습니다. 이는 의외로 효과적인 문제 해결 전략입니다.30

## **12장: 버전 관리, 배포 및 유지보수**

모드를 성공적으로 개발했다면, 이제 세상에 공개하고 지속적으로 관리하는 단계가 남았습니다. 책임감 있는 배포와 유지보수는 건강한 모딩 커뮤니티를 유지하는 데 필수적입니다.

### **12.1 유의적 버전 관리 (Semantic Versioning) 적용**

유의적 버전 관리(SemVer)는 버전 번호에 명확한 의미를 부여하는 규칙 체계입니다. 모든 모드 개발자는 이 규칙을 엄격하게 준수해야 합니다. 이는 모드 간의 복잡한 의존성 네트워크가 무너지지 않고 기능할 수 있도록 하는 사회적, 기술적 계약과도 같습니다.31

버전은 MAJOR.MINOR.PATCH (예: 2.1.0) 형식으로 구성됩니다.

* **MAJOR (주 버전):** API에 하위 호환성이 없는 변경이 있을 때 올립니다. 예를 들어, 공개 메서드의 시그니처를 바꾸거나 삭제하는 경우입니다. 이 버전이 올라가면, 해당 모드의 API를 사용하는 다른 모드들은 반드시 코드를 수정해야 합니다.  
* **MINOR (부 버전):** 하위 호환성을 유지하면서 새로운 기능이 추가될 때 올립니다. 기존 API를 사용하는 모드들은 코드를 수정하지 않아도 계속 작동합니다.  
* **PATCH (패치 버전):** 하위 호환성을 유지하는 버그 수정이 있을 때 올립니다.

SemVer를 준수하면, 모드 사용자나 다른 개발자들은 버전 번호만 보고도 업데이트의 성격을 파악할 수 있습니다. 예를 들어, 어떤 모드의 API를 사용하는 개발자는 해당 모드의 MINOR나 PATCH 버전 업데이트는 안전하게 적용할 수 있지만, MAJOR 버전 업데이트가 있다면 자신의 코드가 깨질 수 있음을 인지하고 신중하게 대응해야 합니다. SMAPI의 자동 업데이트 알림과 같은 도구도 이 버전 체계를 기반으로 작동하므로, SemVer 준수는 선택이 아닌 필수입니다.

### **12.2 출시 준비**

모드를 배포하기 전에 몇 가지 준비가 필요합니다.

* **업데이트 키 구현:** manifest.json 파일에 UpdateKeys 필드를 추가하면, SMAPI가 자동으로 모드의 새 버전을 확인하고 사용자에게 알려줍니다. Nexus Mods에 모드를 업로드했다면, \["Nexus:12345"\]와 같이 설정할 수 있습니다.11  
* **패키징:** 모드 폴더 전체( manifest.json 포함)를 .zip 파일로 압축합니다. 사용자는 이 .zip 파일을 다운로드하여 게임의 Mods 폴더에 압축을 풀기만 하면 됩니다.11

### **12.3 커뮤니티 및 장기 지원**

모드를 공개하는 것은 개발의 끝이 아니라, 커뮤니티와의 관계 시작입니다.

* **오픈 소스:** 코드를 GitHub과 같은 플랫폼에 공개하는 것을 강력히 권장합니다. 이는 다른 개발자들이 코드를 보고 배우는 데 도움이 될 뿐만 아니라, 만약 당신이 모드 업데이트를 중단하게 되더라도 커뮤니티의 다른 누군가가 비공식 업데이트를 만들어 모드의 수명을 연장할 수 있게 해줍니다.6  
* **사용자 지원:** 버그 리포트를 받을 때는 사용자에게 항상 파싱된 SMAPI 로그 링크(smapi.io/log 결과)를 함께 제출하도록 안내해야 합니다. 이는 문제의 원인을 파악하는 데 가장 중요한 정보입니다.27

## **결론**

SMAPI는 단순한 모드 로더를 넘어, 스타듀밸리 모딩 생태계의 안정성, 호환성, 개발 편의성을 보장하는 정교하고 강력한 프레임워크입니다. 이 가이드에서 탐구한 바와 같이, SMAPI는 이벤트 기반 아키텍처, 안전한 콘텐츠 관리 파이프라인, 표준화된 설정 및 데이터 저장 방식, 그리고 모드 간의 유연한 통합 메커니즘을 포함한 포괄적인 API 스위트를 제공합니다.

개발자에게 SMAPI는 게임의 핵심을 건드리는 위험한 작업부터 간단한 데이터 수정에 이르기까지, 다양한 수준의 추상화를 제공합니다. AssetRequested 이벤트를 통한 비파괴적인 에셋 수정, Content Patcher와 같은 프레임워크를 통한 선언적 모딩, 그리고 최후의 수단으로 사용되는 Harmony 런타임 패치에 이르기까지, 개발자는 주어진 과제에 가장 적합한 도구를 선택할 수 있습니다.

특히, manifest.json을 통한 명시적인 의존성 관리와 IModRegistry를 통한 느슨하게 결합된 API 통합 시스템은 수많은 모드들이 서로 충돌 없이 공존하고 협력할 수 있는 기술적 기반을 마련합니다. 이는 SMAPI의 설계가 개별 모드의 기능 구현뿐만 아니라, 전체 생태계의 건강성을 유지하는 데 중점을 두고 있음을 보여줍니다.

성공적인 모드 개발은 단순히 API를 사용하는 것을 넘어, 유의적 버전 관리, 철저한 오류 처리, 명확한 로깅, 그리고 커뮤니티와의 소통과 같은 전문적인 개발 관행을 채택하는 것을 포함합니다. 이러한 원칙을 준수할 때, 개발자는 자신의 창작물이 더 넓은 사용자층에게 오랫동안 사랑받고, 스타듀밸리 모딩이라는 활기찬 커뮤니티에 긍정적으로 기여할 수 있을 것입니다. 본 가이드가 AI 개발자를 비롯한 모든 기술적 배경을 가진 개발자들이 스타듀밸리 모딩의 세계로 들어서는 데 있어 신뢰할 수 있는 나침반이 되기를 바랍니다.

#### **참고 자료**

1. SMAPI \- SMAPI.io, 9월 16, 2025에 액세스, [https://smapi.io/](https://smapi.io/)  
2. Tool \- SMAPI: Stardew Modding API \- Chucklefish Forums, 9월 16, 2025에 액세스, [https://community.playstarbound.com/threads/smapi-stardew-modding-api.108375/](https://community.playstarbound.com/threads/smapi-stardew-modding-api.108375/)  
3. Pathoschild/SMAPI: The modding API for Stardew Valley. \- GitHub, 9월 16, 2025에 액세스, [https://github.com/Pathoschild/SMAPI](https://github.com/Pathoschild/SMAPI)  
4. Help : r/SMAPI \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/SMAPI/comments/wfkx3t/help/](https://www.reddit.com/r/SMAPI/comments/wfkx3t/help/)  
5. How was the Stardew Mod API made? Isn't the game closed source? : r/StardewValley, 9월 16, 2025에 액세스, [https://www.reddit.com/r/StardewValley/comments/rb9jpj/how\_was\_the\_stardew\_mod\_api\_made\_isnt\_the\_game/](https://www.reddit.com/r/StardewValley/comments/rb9jpj/how_was_the_stardew_mod_api_made_isnt_the_game/)  
6. Mod compatibility \- SMAPI.io, 9월 16, 2025에 액세스, [https://smapi.io/mods](https://smapi.io/mods)  
7. RELEASED \- Guide: How to make SMAPI 0.37 mod | Chucklefish Forums, 9월 16, 2025에 액세스, [https://community.playstarbound.com/threads/guide-how-to-make-smapi-0-37-mod.110224/](https://community.playstarbound.com/threads/guide-how-to-make-smapi-0-37-mod.110224/)  
8. Modding:Modder Guide/APIs/Integrations \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/APIs/Integrations](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations)  
9. Modding:Modder Guide/APIs \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/APIs](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs)  
10. Modding:Modder Guide/APIs/Content Packs \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/APIs/Content\_Packs](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Content_Packs)  
11. Modding:Content packs \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Content\_packs](https://stardewvalleywiki.com/Modding:Content_packs)  
12. Modding:Modder Guide/APIs/Content \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/APIs/Content](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Content)  
13. Modding:Modder Guide/APIs/Logging \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/APIs/Logging](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Logging)  
14. Modding:Modder Guide/APIs/Utilities \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/APIs/Utilities](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Utilities)  
15. Modding:Content pack frameworks \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Content\_pack\_frameworks](https://stardewvalleywiki.com/Modding:Content_pack_frameworks)  
16. Pathoschild/StardewMods: Mods for Stardew Valley using SMAPI. \- GitHub, 9월 16, 2025에 액세스, [https://github.com/Pathoschild/StardewMods](https://github.com/Pathoschild/StardewMods)  
17. Tutorial: Your First Content Patcher Pack \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/Tutorial:\_Your\_First\_Content\_Patcher\_Pack](https://stardewmodding.wiki.gg/wiki/Tutorial:_Your_First_Content_Patcher_Pack)  
18. Modding:Content Patcher \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Content\_Patcher](https://stardewvalleywiki.com/Modding:Content_Patcher)  
19. RELEASED \- Content Patcher | Page 11 \- Chucklefish Forums, 9월 16, 2025에 액세스, [https://community.playstarbound.com/threads/content-patcher.141420/page-11](https://community.playstarbound.com/threads/content-patcher.141420/page-11)  
20. Content Patcher | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/470174-content-patcher](https://www.moddrop.com/stardew-valley/mods/470174-content-patcher)  
21. For those who are using PPJA mods... :: Stardew Valley 일반 토론 \- Steam Community, 9월 16, 2025에 액세스, [https://steamcommunity.com/app/413150/discussions/0/3086646248532649168/?l=koreana\&ctp=2](https://steamcommunity.com/app/413150/discussions/0/3086646248532649168/?l=koreana&ctp=2)  
22. For those who are using PPJA mods... :: Stardew Valley General Discussions \- Steam Community, 9월 16, 2025에 액세스, [https://steamcommunity.com/app/413150/discussions/0/3086646248532649168/](https://steamcommunity.com/app/413150/discussions/0/3086646248532649168/)  
23. For those who are using PPJA mods... :: Stardew Valley 綜合討論 \- Steam Community, 9월 16, 2025에 액세스, [https://steamcommunity.com/app/413150/discussions/0/3086646248532649168/?l=tchinese](https://steamcommunity.com/app/413150/discussions/0/3086646248532649168/?l=tchinese)  
24. Dynamic Game Assets at Stardew Valley Nexus \- Mods and ..., 9월 16, 2025에 액세스, [https://www.nexusmods.com/stardewvalley/mods/9365](https://www.nexusmods.com/stardewvalley/mods/9365)  
25. Hyper Realistic Coins \- Dynamic Game Assets Conversion | Stardew Valley Mods, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/1070299-hyper-realistic-coins-dynamic-game-assets-conversion](https://www.moddrop.com/stardew-valley/mods/1070299-hyper-realistic-coins-dynamic-game-assets-conversion)  
26. Modding:Modder Guide/APIs/Harmony \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/APIs/Harmony](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Harmony)  
27. smapi won't let me use debug comands or warp anywhere. help? \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/SMAPI/comments/1l4eift/smapi\_wont\_let\_me\_use\_debug\_comands\_or\_warp/](https://www.reddit.com/r/SMAPI/comments/1l4eift/smapi_wont_let_me_use_debug_comands_or_warp/)  
28. Help with opening Stardew valley Mod API : r/SMAPI \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/SMAPI/comments/1hcos68/help\_with\_opening\_stardew\_valley\_mod\_api/](https://www.reddit.com/r/SMAPI/comments/1hcos68/help_with_opening_stardew_valley_mod_api/)  
29. Modding:Console commands \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Console\_commands](https://stardewvalleywiki.com/Modding:Console_commands)  
30. Debugging \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/Debugging](https://stardewmodding.wiki.gg/wiki/Debugging)  
31. Semantic Versioning 2.0.0 | Semantic Versioning, 9월 16, 2025에 액세스, [https://semver.org/](https://semver.org/)  
32. A Guide to Semantic Versioning | Baeldung on Computer Science, 9월 16, 2025에 액세스, [https://www.baeldung.com/cs/semantic-versioning](https://www.baeldung.com/cs/semantic-versioning)