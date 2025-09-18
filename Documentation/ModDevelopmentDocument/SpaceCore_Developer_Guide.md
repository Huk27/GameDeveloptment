# SpaceCore Developer Guide

SpaceCore는 커스텀 스킬, 직업, 데이터 직렬화, 장비 슬롯 등을 확장하기 위한 프레임워크입니다. `ExternalLibraries/SpacechaseFrameworks/SpaceCore` 소스와 `Stardew/DrawingSkill` 예제를 기준으로 핵심 사용법을 정리합니다.

## 1. 설정

`manifest.json`에 SpaceCore 의존성을 선언합니다.
```json
"Dependencies": [
  { "UniqueID": "spacechase0.SpaceCore", "IsRequired": true }
]
```

`ModEntry.Entry`에서 API가 필요한 경우 `Helper.ModRegistry.GetApi<SpaceCore.IApi>("spacechase0.SpaceCore")`로 획득할 수 있습니다 (`ExternalLibraries/SpacechaseFrameworks/SpaceCore/Api.cs`).

## 2. 커스텀 스킬 등록

1. **`Skills.Skill` 파생 클래스 작성**  
   - 추상 클래스 정의는 `ExternalLibraries/SpacechaseFrameworks/SpaceCore/Skills.cs` 참조.  
   - `ExperienceCurve`, `ExperienceBarColor`, `Professions` 등을 설정합니다.  
   - 예: `Stardew/DrawingSkill/DrawingSkill.cs`.
2. **RegisterSkill 호출**  
   `Skills.RegisterSkill(new DrawingSkill());` (`Stardew/DrawingSkill/DrawingActivityMod.cs:48`).
3. **직업 구성**  
   - `Skills.Skill.Profession`을 상속하거나 해당 객체를 생성해 `Professions` 리스트에 추가.  
   - `GetVanillaId()`는 SpaceCore 내부에서 고유 ID를 계산합니다 (`Skills.cs:24-83`).
4. **레벨업 UI/직업 선택**  
   SpaceCore가 제공하는 `SpaceCore.Interface.NewSkillsPage` 및 `SkillLevelUpMenu`가 자동으로 커스텀 스킬을 표시합니다.

## 3. 경험치 관리

- 경험치 추가: `farmer.AddCustomSkillExperience("drawing", amount);` (`Stardew/DrawingSkill/DrawingSkill.cs:54`).
- 현재 레벨 조회: `farmer.GetCustomSkillLevel("drawing");` (`SpaceCore/Api.cs`에서도 동일 로직 사용).
- API 메서드 요약 (`ExternalLibraries/SpacechaseFrameworks/SpaceCore/Api.cs`):
  - `GetCustomSkills()`
  - `AddExperienceForCustomSkill(Farmer, skillId, amount)`
  - `GetSkillIconForCustomSkill`, `GetSkillPageIconForCustomSkill`
  - `GetProfessionId`

## 4. 직렬화 & Content Patcher 연동

- SpaceCore는 `RegisterSerializerType(Type)`을 통해 커스텀 `Net` 필드를 저장할 수 있도록 합니다 (`Api.cs:70`).  
- 타입은 `[XmlType("Mods_<YourId>")]` 속성을 가져야 하며, `SpaceCore.ModTypes` 목록에 추가됩니다.
- DrawingSkill 예제처럼 아이콘은 Content Patcher 팩(`[CP] Drawing Activity/`)으로 공급하면 SpaceCore UI가 자동으로 불러옵니다.

## 5. 커스텀 속성과 장비 슬롯

- `RegisterCustomProperty` (`Api.cs:94`)로 리플렉션 기반 커스텀 프로퍼티를 등록해 저장/동기화가 가능하게 할 수 있습니다.
- `RegisterEquipmentSlot`으로 새로운 장비 슬롯을 추가할 수 있으며, 검증자(`Func<Item, bool> slotValidator`)와 표시 이름 함수가 필요합니다. 내부 구현은 `SpaceCore.cs:112-170` 참고.

## 6. 이벤트 확장

- SpaceCore는 추가 이벤트를 `SpaceCore.Events` 네임스페이스로 제공합니다. 예를 들어 `Skills.GetSkillList()`는 SpaceCore 초기화 시 등록된 모든 스킬 ID를 반환합니다.
- `SpaceCore.Events` 아래 정의된 커스텀 이벤트를 구독하려면 `helper.Events`와 별개로 `SpaceCore.Events.EventManager`를 사용할 수 있습니다 (해당 소스 참조).

## 7. 가이드북/대화 확장 등 기타 기능

- `SpaceCore`는 Guidebook, NPC 질문, 던전 확장 등의 기능도 포함합니다 (`SpaceCore/Guidebooks`, `SpaceCore/Dungeons`).  
- 필요시 관련 서브 폴더의 API/모델 클래스를 살펴보고, `SpaceCore.Instance.Helper`를 통해 추가 리소스를 로드합니다.

## 8. 구현 체크리스트

1. 스킬/데이터 구조 준비 → `Skills.Skill` 상속.  
2. `Entry`에서 `Skills.RegisterSkill` 호출.  
3. 번역/아이콘 자산 준비 (Content Patcher 권장).  
4. 경험치/직업 획득 로직에서 SpaceCore API 사용.  
5. 필요 시 `IApi` 기능(장비 슬롯, 직렬화 등) 확장 적용.

## 9. 참고 자료

- SpaceCore 전체 동작: `ExternalLibraries/SpacechaseFrameworks/SpaceCore/SpaceCore.cs`
- API 정의: `ExternalLibraries/SpacechaseFrameworks/SpaceCore/Api.cs`
- 실전 예시: `Stardew/DrawingSkill/` 전체 구조

본 가이드를 통해 SpaceCore 기반의 스킬/시스템 확장을 안정적으로 구현할 수 있습니다.
