

# **스타듀밸리 해부: spacechase0 모드 컬렉션 구현 가이드**

## **서론: 스타듀밸리 모딩에 대한 아키텍트의 관점**

### **spacechase0 리포지토리 개요**

스타듀밸리 모딩 커뮤니티에서 spacechase0/StardewValleyMods 리포지토리는 다양한 모딩 기술의 훌륭한 사례 연구 자료로서 중요한 위치를 차지하고 있습니다.1 이 컬렉션은 단순한 편의성 개선부터 복잡한 게임플레이 시스템 추가에 이르기까지 광범위한 기능을 포괄하며, 개발자 spacechase0가 모딩 생태계에 얼마나 지대한 기여를 했는지 보여줍니다. 그의 작업들은 다양한 아키텍처 패턴을 시연하며, 이제 막 모딩을 시작하는 개발자부터 숙련된 개발자에 이르기까지 모두에게 귀중한 학습 자료가 됩니다. 이 가이드는 해당 리포지토리에서 엄선된 모드들의 내부 구현을 심층적으로 분석하여, 다른 개발자들이 그 기술을 배우고 자신의 프로젝트에 적용할 수 있도록 돕는 것을 목표로 합니다.

### **모더의 툴킷: SMAPI와 Harmony**

분석 대상이 될 모드들의 기술적 기반을 이해하기 위해서는 두 가지 핵심 기술, 즉 SMAPI(Stardew Modding API)와 Harmony에 대한 이해가 선행되어야 합니다. SMAPI는 모드가 게임 루프에 안전하고 구조적으로 접근할 수 있도록 하는 이벤트 기반 프레임워크입니다. 게임의 상태 변화(예: 시간이 바뀔 때, 아이템을 획득할 때, 메뉴가 열릴 때)에 맞춰 코드를 실행할 수 있는 '훅(hook)'을 제공함으로써, 모드가 게임의 핵심 파일을 직접 수정하지 않고도 안정적으로 작동하도록 보장합니다.

반면, Harmony는 런타임에 게임의 하드코딩된 로직을 직접 수정하는 강력한 '수술용 메스'와 같은 도구입니다. 이는 SMAPI가 제공하는 이벤트만으로는 구현할 수 없는, 게임의 근본적인 동작 방식을 변경해야 할 때 사용됩니다. Harmony는 C\# 메서드의 중간 언어(IL) 코드를 동적으로 패치하여, 기존 코드 실행 전(Prefix), 후(Postfix), 또는 코드 자체를 대체(Transpiler)하는 방식으로 작동합니다. 이 두 기술의 조합은 스타듀밸리 모딩의 거의 모든 가능성을 열어주며, 이 가이드에서 분석할 핵심적인 기술적 이분법을 형성합니다.

### **방법론: 구현의 재구성**

이 분석은 한 가지 중요한 제약 조건 하에서 수행되었습니다. 분석에 필요한 모드들의 소스 코드에 대한 직접적인 접근이 불가능한 상황이었습니다.2 따라서 이 가이드는 단순한 코드 리뷰가 아닌, '구현의 재구성'이라는 접근법을 채택합니다. 이는 모드 페이지에 명시된 기능 설명 4, 커뮤니티 포럼에서의 토론 내용 6, 그리고 SMAPI 개발 커뮤니티 내에서 확립된 모범 사례들을 종합하여, 각 모드의 기능이 어떻게 구현되었을지를 논리적으로 추론하고 재현하는 과정입니다. 이 방법론은 단순히 '어떻게'를 넘어 '왜' 그러한 방식으로 설계되었는지를 탐구하며, 관찰 가능한 동작과 상황적 단서로부터 설계 패턴을 역엔지니어링하는 심층적인 학습 경험을 제공합니다.

### **분석 대상 모드 선정 이유**

이 가이드에서는 AnotherHungerMod, MultiFertilizer, RealtimeMinimap 세 가지 모드를 집중적으로 분석합니다. 이들은 각각 스타듀밸리 모딩의 세 가지 고유하고 근본적인 개발 유형을 대표하기 때문에 선정되었습니다.

1. **AnotherHungerMod**: SMAPI 이벤트를 활용하여 새로운 게임플레이 시스템(허기)을 추가하는 방식을 보여줍니다. 이는 게임의 기존 코드를 거의 건드리지 않으면서 독립적인 기능을 구현하는 '비침습적(Non-Invasive)' 패턴의 전형입니다.  
2. **MultiFertilizer**: Harmony 패치를 통해 핵심 게임 메커니즘(비료 시스템)을 변경하는 사례를 다룹니다. 이는 하드코딩된 게임의 한계를 극복하기 위해 보다 깊숙이 개입하는 '침습적(Invasive)' 패턴을 탐구하는 데 이상적입니다.  
3. **RealtimeMinimap**: 실시간 렌더링을 통해 복잡한 커스텀 UI를 생성하는 기술을 분석합니다. 이는 성능 최적화와 렌더링 파이프라인 관리에 대한 중요한 교훈을 제공합니다.

이 세 가지 모드를 통해 개발자들은 각기 다른 문제 상황에 어떤 아키텍처와 기술을 적용해야 하는지에 대한 포괄적인 이해를 얻게 될 것입니다.

---

## **제1장: 새로운 게임플레이 시스템 설계: AnotherHungerMod**

이 장에서는 스타듀밸리에 완전히 새로운 독립형 게임플레이 메커니즘, 즉 '허기' 시스템을 추가하는 방법을 해부합니다. 핵심은 SMAPI의 이벤트 기반 아키텍처를 활용하여 바닐라 게임 코드와의 직접적인 간섭을 최소화하는 것입니다. 이 접근 방식은 모드의 안정성과 향후 게임 업데이트에 대한 호환성을 크게 향상시킵니다.

### **1.1. 아키텍처 청사진: 허기 서브시스템**

#### **핵심 로직**

AnotherHungerMod의 기능은 명확하게 정의되어 있습니다. 플레이어는 최대 100의 '포만감' 수치를 가지며, 이 수치는 게임 내 시간으로 10분마다 0.8 포인트씩 감소합니다. 포만감 수치에 따라 특정 버프나 디버프가 적용됩니다. 80을 초과하면 속도와 공격력이 향상되고, 25 미만으로 떨어지면 속도가 감소하며, 0이 되면 지속적인 허기 피해를 입게 됩니다.4

#### **데이터 모델**

이러한 플레이어 상태를 추적하기 위한 데이터 구조는 비교적 간단하게 설계할 수 있습니다. CurrentFullness와 MaxFullness 필드를 포함하는 클래스나 구조체가 가장 유력한 모델입니다. 이 데이터 모델은 각 플레이어 인스턴스에 연결되어야 하며, 멀티플레이어 환경을 완벽하게 지원하기 위해 플레이어의 고유 ID를 키로 사용하는 Dictionary나 ConditionalWeakTable을 통해 관리될 가능성이 높습니다.

C\#

public class HungerData  
{  
    public float CurrentFullness { get; set; } \= 100f;  
    public float MaxFullness { get; } \= 100f;  
}

이 모드의 설계는 기존 시스템을 변경하기보다는 병렬적인 시스템을 추가하는 '비침습적' 모딩 패턴의 철학을 명확히 보여줍니다. 모드 설명에 따르면 이 시스템은 농사 방식이나 NPC의 행동을 바꾸지 않고, 단지 시간에 따라 감소하고 음식을 먹는 단일 행동에 의해 영향을 받는 새로운 플레이어 통계를 추가할 뿐입니다.4 이러한 느슨한 결합은 SMAPI의

GameLoop.TimeChanged 이벤트를 구독하여 구현하는 것이 가장 효율적이고 안정적입니다. 이 이벤트는 게임 내 10분마다 정확히 발생하므로, 지정된 감소율을 적용하기에 완벽합니다. 이는 게임의 핵심 업데이트 루프를 직접 패치하는 것보다 훨씬 안정적이며, 게임 업데이트 시 발생할 수 있는 호환성 문제를 최소화하는 현명한 설계 선택입니다.

### **1.2. 시스템의 심장 박동: 이벤트 기반 포만감 감소**

#### **TimeChanged 이벤트**

허기 시스템의 핵심인 포만감 감소 로직은 SMAPI의 GameLoop.TimeChanged 이벤트를 통해 구현됩니다. 이 이벤트는 게임 내 시간이 10분 경과할 때마다 발생하므로, 주기적인 상태 업데이트에 이상적입니다. ModEntry 클래스의 진입점에서 이벤트를 구독하고, 이벤트 핸들러 내에서 포만감 감소 및 상태 효과 적용 로직을 처리하는 것이 일반적인 구현 방식입니다.

#### **상태 효과 적용**

이벤트 핸들러 내에서는 현재 플레이어의 CurrentFullness 값을 확인하고, 사전에 정의된 임계값(80 초과, 25 미만, 0)과 비교합니다. 각 조건에 따라 SMAPI 헬퍼나 게임의 네이티브 코드를 사용하여 적절한 버프(예: Game1.player.buffs.Add(...))를 추가하거나, 디버프를 적용하고, 허기 피해를 입히는 로직이 실행됩니다.

#### **코드 샘플: 포만감 감소 및 상태 효과 적용**

다음은 TimeChanged 이벤트를 활용한 포만감 감소 및 상태 효과 적용 로직의 재구성된 C\# 코드 예시입니다.

C\#

// ModEntry.cs  
using StardewModdingAPI;  
using StardewModdingAPI.Events;  
using StardewValley;

public class ModEntry : Mod  
{  
    private HungerData playerData; // 실제 구현에서는 멀티플레이어를 위한 Dictionary 사용

    public override void Entry(IModHelper helper)  
    {  
        // 게임 데이터 로드 시 플레이어 데이터 초기화  
        helper.Events.GameLoop.SaveLoaded \+= OnSaveLoaded;  
          
        // 10분마다 포만감 감소 로직 실행  
        helper.Events.GameLoop.TimeChanged \+= OnTimeChanged;  
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)  
    {  
        this.playerData \= new HungerData(); // 새 게임 또는 로드 시 데이터 초기화  
    }

    private void OnTimeChanged(object sender, TimeChangedEventArgs e)  
    {  
        if (\!Context.IsWorldReady |

| this.playerData \== null)  
            return;

        // 포만감 감소 (0.8 포인트)  
        this.playerData.CurrentFullness \= Math.Max(0, this.playerData.CurrentFullness \- 0.8f);

        // 상태 효과 적용 로직  
        ApplyHungerEffects();  
    }

    private void ApplyHungerEffects()  
    {  
        // 기존 허기 관련 버프/디버프 제거  
        Game1.player.buffs.Remove("spacechase0.AnotherHungerMod.WellFed");  
        Game1.player.buffs.Remove("spacechase0.AnotherHungerMod.Hungry");

        if (this.playerData.CurrentFullness \> 80)  
        {  
            // '배부름' 버프: 속도 \+1, 공격력 \+1  
            Buff wellFedBuff \= new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, "spacechase0.AnotherHungerMod.WellFed", "Well-Fed");  
            Game1.player.buffs.Add(wellFedBuff);  
        }  
        else if (this.playerData.CurrentFullness \< 25 && this.playerData.CurrentFullness \> 0)  
        {  
            // '배고픔' 디버프: 속도 \-1  
            Buff hungryBuff \= new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, \-1, 0, 0, 1, "spacechase0.AnotherHungerMod.Hungry", "Hungry");  
            Game1.player.buffs.Add(hungryBuff);  
        }  
        else if (this.playerData.CurrentFullness \<= 0)  
        {  
            // '아사' 상태: 체력 감소  
            Game1.player.health \= Math.Max(0, Game1.player.health \- 5);  
        }  
    }  
}

### **1.3. 플레이어 행동 가로채기: 포만감 회복**

#### **도전 과제**

플레이어가 음식을 먹었을 때 이를 감지하고, 음식의 종류에 따라 적절한 양의 포만감을 회복시키는 것은 이 모드의 핵심 기능 중 하나입니다. 모드 설명에 따르면 포만감은 음식의 '먹을 수 있는 가치(edibility value)'에 기반하여 회복됩니다.4 이 값을 어떻게 얻고, 포만감에 적용할 수 있을까요?

#### **해결책: Harmony Postfix 패치**

이 기능은 가벼운 수준의 패치가 필요한 대표적인 사례입니다. StardewValley.Farmer.eatObject 메서드에 Harmony Postfix 패치를 적용하는 것이 가장 효과적인 해결책입니다. Postfix 패치는 원본 메서드가 완전히 실행된 직후에 실행되므로, 게임의 기본 체력 및 기력 회복 로직에 전혀 영향을 주지 않으면서, 플레이어가 섭취한 아이템 객체에 접근하여 그 edibility 값을 읽어올 수 있습니다. 이 값을 CurrentFullness에 더해주기만 하면 됩니다.

#### **코드 샘플: Farmer.eatObject Postfix 패치**

다음은 Farmer.eatObject 메서드를 패치하여 음식을 먹었을 때 포만감을 회복시키는 Harmony 패치 클래스의 재구성된 예시입니다.

C\#

// HarmonyPatcher.cs  
using HarmonyLib;  
using StardewValley;

public static class HarmonyPatcher  
{  
    private static ModEntry modEntry;

    public static void Apply(ModEntry entry, string harmonyId)  
    {  
        modEntry \= entry;  
        var harmony \= new Harmony(harmonyId);

        harmony.Patch(  
            original: AccessTools.Method(typeof(Farmer), nameof(Farmer.eatObject)),  
            postfix: new HarmonyMethod(typeof(HarmonyPatcher), nameof(EatObject\_Postfix))  
        );  
    }

    public static void EatObject\_Postfix(Farmer \_\_instance, StardewValley.Object obj, bool \_\_result)  
    {  
        // \_\_instance는 메서드를 호출한 Farmer 객체  
        // obj는 섭취한 아이템  
        // \_\_result는 원본 메서드의 반환값 (성공적으로 먹었는지 여부)  
        if (\_\_result && \_\_instance.IsLocalPlayer && obj.Edibility \> 0)  
        {  
            // ModEntry에 있는 playerData에 접근하여 포만감 회복  
            // edibility 값은 음수일 수 있으므로 양수일 때만 처리  
            // 실제 구현에서는 edibility를 포만감으로 변환하는 계수가 있을 수 있음  
            float fullnessToRestore \= obj.Edibility \* 0.75f; // 예시 변환 계수  
            modEntry.playerData.CurrentFullness \= Math.Min(  
                modEntry.playerData.MaxFullness,   
                modEntry.playerData.CurrentFullness \+ fullnessToRestore  
            );  
        }  
    }  
}

// ModEntry.cs의 Entry 메서드에 다음 코드 추가  
// HarmonyPatcher.Apply(this, this.ModManifest.UniqueID);

### **1.4. 상태 시각화: HUD 렌더링**

#### **RenderedHud 이벤트**

플레이어가 자신의 허기 상태를 직관적으로 파악할 수 있도록 HUD(Heads-Up Display)에 커스텀 UI 요소를 그리는 것은 필수적입니다. 이는 SMAPI의 helper.Events.Display.RenderedHud 이벤트를 통해 구현됩니다. 이 이벤트는 게임의 HUD가 렌더링된 후 매 프레임 호출되며, 모드가 화면에 추가적인 그래픽을 그릴 수 있는 SpriteBatch 객체를 제공합니다.

#### **구현 세부사항**

구현 과정은 다음과 같은 단계로 이루어집니다.

1. 모드 에셋 폴더에서 허기 바의 배경과 전경 텍스처를 로드합니다.  
2. CurrentFullness 값을 기반으로 허기 바의 채워진 비율을 계산합니다.  
3. 계산된 비율에 따라 전경 텍스처의 소스 사각형(source rectangle)을 조절하여 '채워지는' 효과를 만듭니다.  
4. spriteBatch.Draw() 메서드를 사용하여 화면의 고정된 위치에 배경과 전경 스프라이트를 렌더링합니다.

#### **코드 샘플: 허기 바 렌더링**

다음은 RenderedHud 이벤트 핸들러 내에서 허기 바를 그리는 로직의 재구성된 C\# 코드 예시입니다.

C\#

// ModEntry.cs  
using Microsoft.Xna.Framework;  
using Microsoft.Xna.Framework.Graphics;  
using StardewModdingAPI.Events;

public class ModEntry : Mod  
{  
    private Texture2D hungerBarTexture;  
      
    public override void Entry(IModHelper helper)  
    {  
        //... 기존 코드...  
        this.hungerBarTexture \= helper.Content.Load\<Texture2D\>("assets/hunger\_bar.png", ContentSource.ModFolder);  
        helper.Events.Display.RenderedHud \+= OnRenderedHud;  
    }

    private void OnRenderedHud(object sender, RenderedHudEventArgs e)  
    {  
        if (\!Context.IsWorldReady |

| this.playerData \== null)  
            return;

        SpriteBatch spriteBatch \= e.SpriteBatch;  
          
        // 화면 우측 하단에 위치 (기력 바 근처)  
        Vector2 position \= new Vector2(  
            Game1.graphics.GraphicsDevice.Viewport.Width \- 300,   
            Game1.graphics.GraphicsDevice.Viewport.Height \- 100  
        );

        // 허기 바 배경 그리기 (텍스처의 상단 절반)  
        Rectangle backgroundSourceRect \= new Rectangle(0, 0, 100, 20);  
        spriteBatch.Draw(this.hungerBarTexture, position, backgroundSourceRect, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 1);

        // 허기 바 전경(채워진 부분) 그리기 (텍스처의 하단 절반)  
        float fillPercentage \= this.playerData.CurrentFullness / this.playerData.MaxFullness;  
        int foregroundWidth \= (int)(100 \* fillPercentage);  
        Rectangle foregroundSourceRect \= new Rectangle(0, 20, foregroundWidth, 20);  
        spriteBatch.Draw(this.hungerBarTexture, position, foregroundSourceRect, Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, 1);  
    }  
}

### **1.5. 지속성 보장: 커스텀 데이터 저장**

#### **SMAPI 데이터 API**

모드가 추가한 커스텀 데이터(이 경우, 플레이어의 허기 상태)는 게임 세션 간에 유지되어야 합니다. 이를 위해 SMAPI는 IDataHelper API를 제공하며, 이는 게임의 세이브 파일을 직접 조작하는 복잡하고 위험한 방법 대신 안전하고 표준화된 데이터 저장 및 로드 방법을 제공합니다.

#### **저장 및 로드**

데이터 저장은 GameLoop.Saving 이벤트 핸들러 내에서 helper.Data.WriteSaveData(key, data)를 호출하여 수행합니다. 반대로 데이터 로드는 GameLoop.SaveLoaded 이벤트 핸들러에서 helper.Data.ReadSaveData\<T\>(key)를 사용하여 이루어집니다. 이 방식을 통해 플레이어의 허기 데이터는 세이브 파일에 안전하게 포함되며, 게임을 다시 로드했을 때 이전 상태가 그대로 복원됩니다. 이는 많은 초보 모더들이 간과하기 쉬운 매우 중요한 부분입니다.

#### **코드 샘플: 데이터 저장 및 로드**

C\#

// ModEntry.cs  
public class ModEntry : Mod  
{  
    private const string SAVE\_KEY \= "player-hunger-data";  
      
    public override void Entry(IModHelper helper)  
    {  
        //... 기존 코드...  
        helper.Events.GameLoop.Saving \+= OnSaving;  
        // OnSaveLoaded는 이미 구현됨  
    }

    private void OnSaving(object sender, SavingEventArgs e)  
    {  
        if (Context.IsMainPlayer)  
        {  
            this.Helper.Data.WriteSaveData(SAVE\_KEY, this.playerData);  
        }  
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)  
    {  
        // 기존 데이터가 있으면 로드, 없으면 새로 생성  
        this.playerData \= this.Helper.Data.ReadSaveData\<HungerData\>(SAVE\_KEY)?? new HungerData();  
    }  
}

이처럼 잘 설계된 비침습적 모드조차도 지속적인 유지보수가 필요하다는 점은 중요합니다. 포럼에서 이 모드의 업데이트를 요청하는 다수의 게시물은 이를 명확히 보여줍니다.6 게임 업데이트로 인해 API가 변경되거나(실제로 모드 변경 로그에 "SMAPI deprecation fixes"라는 내용이 언급됨 4), 미묘한 내부 로직 변화가 모드의 작동을 방해할 수 있습니다. 이는 모드의 생명주기가 단순히 최초 배포에서 끝나지 않으며, 게임의 진화 및 커뮤니티의 지속적인 관심과 깊이 연관되어 있음을 시사합니다.

---

## **제2장: 규칙 재작성: MultiFertilizer와 Harmony 패치**

이 장에서는 훨씬 더 복잡한 문제, 즉 하드코딩된 게임의 근본적인 메커니즘을 수정하는 과제를 다룹니다. MultiFertilizer를 사례 연구로 삼아, SMAPI API만으로는 불가능한 기능을 구현하기 위해 Harmony, 특히 트랜스파일러(transpiler)와 같은 고급 패치 기술을 어떻게 사용하는지 심층적으로 탐구합니다.

### **2.1. 아키텍처 청사진: 하드코딩된 한계 극복**

#### **핵심 문제**

스타듀밸리의 StardewValley.TerrainFeatures.HoeDirt 클래스는 본질적으로 단 하나의 비료 유형만을 저장하도록 설계되었습니다 (fertilizer.Value 필드). MultiFertilizer 모드의 목표는 이 제약을 깨고 동일한 경작지 타일에 여러 종류의 비료를 동시에 적용할 수 있도록 하는 것입니다.

#### **전략: 병렬 데이터 레이어**

이 문제를 해결하기 위한 유일하고 실행 가능한 해결책은 모드가 전적으로 관리하는 커스텀 데이터 구조를 게임의 자체 데이터와 병렬로 운영하는 것입니다. 가장 유력한 구현 방식은 Dictionary\<Vector2, List\<int\>\>와 같은 구조를 사용하는 것입니다. 여기서 Vector2 키는 타일의 좌표를 나타내고, List\<int\>는 해당 타일에 적용된 모든 비료의 아이템 ID를 저장합니다. 이 데이터는 각 게임 내 장소(location)별로 관리되어야 합니다.

이 모드는 '침습적' 모딩 패턴의 대표적인 예입니다. 이는 게임의 핵심 로직을 직접 가로채고 변경하여 강력한 기능을 구현하지만, 그에 상응하는 상당한 대가를 치릅니다. 모드 설명에서 저자가 직접 "이 모드는 트랜스파일러에 크게 의존하며 다른 모드와의 호환성을 보장할 수 없다"고 언급한 것은 이러한 패턴의 취약성을 인정한 것입니다.7 더 나아가, 다른 개발자가 유사한 기능을 가진 모드를 처음부터 다시 작성한 사례는 이 문제를 더욱 명확하게 보여줍니다. 그는 기존 모드의 코드가 "효율적이지도 깔끔하지도 않았고, 트랜스파일러를 읽는 것보다 직접 작성하는 것이 더 빨랐을 것"이라고 언급했습니다.8 이는 매우 침습적인 코드, 특히 복잡한 트랜스파일러는 유지보수의 악몽이 될 수 있음을 시사합니다. 다른 개발자(심지어 원저자조차도)가 디버깅하고 확장하기 어려워져, 결국 커뮤니티 내에서 동일한 문제를 해결하려는 여러 모드가 난립하는 파편화로 이어질 수 있습니다. 또 다른 비료 모드와의 상호작용에서 예상치 못한 동작이 발생할 수 있다는 호환성 노트는 이러한 취약성을 다시 한번 확인시켜 줍니다.5

### **2.2. 외과용 메스: 고급 Harmony 패치**

#### **타겟팅: HoeDirt.applyFertilizer**

이 메서드는 개입이 필요한 주요 지점입니다. 하지만 단순한 Postfix 패치로는 충분하지 않습니다. 원본 메서드에 포함된 if (this.fertilizer.Value\!= 0\) return false; 로직이 두 번째 비료의 적용을 원천적으로 차단하기 때문입니다.

#### **해결책: 트랜스파일러**

이 문제를 해결하기 위해 Harmony 트랜스파일러가 사용됩니다. 트랜스파일러는 메서드의 IL(중간 언어) 코드를 직접 조작할 수 있는 강력한 기능입니다. 이 경우, 트랜스파일러는 applyFertilizer 메서드의 IL 코드를 스캔하여 if (this.fertilizer.Value\!= 0\) 조건문에 해당하는 명령어들을 찾아 제거합니다. 이 '외과적 수술'을 통해 해당 조건문이 코드에서 완전히 사라지게 되어, 메서드가 여러 번 성공적으로 실행될 수 있게 됩니다. 대안으로, 원본 메서드 실행을 건너뛰고 커스텀 데이터만 관리하는 Prefix 패치도 고려할 수 있지만, 트랜스파일러 방식이 원본 로직을 최대한 활용한다는 점에서 더 정교한 접근법입니다.

#### **코드 샘플: applyFertilizer 트랜스파일러**

다음은 applyFertilizer 메서드의 중복 적용 방지 코드를 제거하는 Harmony 트랜스파일러의 재구성된 C\# 코드 예시입니다.

C\#

// HarmonyPatcher.cs  
using HarmonyLib;  
using StardewValley.TerrainFeatures;  
using System.Collections.Generic;  
using System.Reflection.Emit;

public static class HarmonyPatcher  
{  
    public static void Apply(string harmonyId)  
    {  
        var harmony \= new Harmony(harmonyId);

        harmony.Patch(  
            original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.applyFertilizer)),  
            transpiler: new HarmonyMethod(typeof(HarmonyPatcher), nameof(ApplyFertilizer\_Transpiler))  
        );  
    }

    public static IEnumerable\<CodeInstruction\> ApplyFertilizer\_Transpiler(IEnumerable\<CodeInstruction\> instructions)  
    {  
        var instructionList \= new List\<CodeInstruction\>(instructions);

        // 목표: 'if (this.fertilizer.Value\!= 0)' 블록을 찾아서 제거  
        // 이 코드는 대략 다음과 같은 IL 패턴을 가짐:  
        // ldarg.0  
        // ldfld StardewValley.Network.NetInt StardewValley.TerrainFeatures.HoeDirt::fertilizer  
        // callvirt instance\!0 class StardewValley.Network.NetField\`2\<int32, class StardewValley.Network.NetInt\>::get\_Value()  
        // brfalse.s... (또는 brzero.s) \-\> 이 분기 명령어가 핵심  
          
        for (int i \= 0; i \< instructionList.Count \- 2; i++)  
        {  
            // 'fertilizer.get\_Value()' 호출을 찾음  
            if (instructionList\[i\].opcode \== OpCodes.Ldarg\_0 &&  
                instructionList\[i+1\].LoadsField(AccessTools.Field(typeof(HoeDirt), "fertilizer")) &&  
                instructionList\[i+2\].Calls(AccessTools.PropertyGetter(typeof(NetField\<int, NetInt\>), "Value")))  
            {  
                // 그 다음 분기 명령어를 찾음  
                if (instructionList\[i+3\].opcode \== OpCodes.Brfalse |

| instructionList\[i+3\].opcode \== OpCodes.Brfalse\_S)  
                {  
                    // 해당 조건문과 관련된 코드 블록을 NOP(No Operation)으로 만들어 비활성화  
                    // 실제로는 더 정교한 패턴 매칭이 필요할 수 있음  
                    instructionList\[i\].opcode \= OpCodes.Nop;  
                    instructionList\[i+1\].opcode \= OpCodes.Nop;  
                    instructionList\[i+2\].opcode \= OpCodes.Nop;  
                    instructionList\[i+3\].opcode \= OpCodes.Nop;  
                      
                    // 'return false;'에 해당하는 ldc.i4.0, ret 부분도 제거해야 할 수 있음  
                    //... (추가적인 NOP 처리)...  
                      
                    break; // 패치 완료  
                }  
            }  
        }  
          
        return instructionList;  
    }  
}

### **2.3. 커스텀 데이터 레이어 관리**

#### **비료 적용 후킹**

트랜스파일러가 성공적으로 원본 메서드의 제약을 제거했다면, 이제 HoeDirt.applyFertilizer에 Postfix 패치를 추가하여 모드의 병렬 데이터 레이어를 업데이트해야 합니다. 이 Postfix 패치는 원본 메서드가 실행된 후, 성공적으로 적용된 비료의 ID를 해당 타일 좌표에 매핑된 리스트에 추가하는 역할을 합니다.

#### **데이터 지속성**

1장에서와 마찬가지로, 이 커스텀 데이터는 세이브 파일에 저장되어야 합니다. 하지만 이번에는 단일 플레이어 값이 아닌, 각 장소(location)에 연결된 복잡한 Dictionary 데이터를 저장해야 합니다. SMAPI의 IDataHelper를 사용하여 각 장소의 고유한 이름(예: "Farm", "Town")을 키로 삼아 비료 데이터를 저장하고 로드하는 방식이 필요합니다.

#### **코드 샘플: 병렬 데이터 업데이트 Postfix**

C\#

// MultiFertilizerData.cs  
// 이 클래스는 각 장소의 비료 데이터를 관리  
public class MultiFertilizerData  
{  
    // Key: 타일 좌표, Value: 해당 타일에 적용된 비료 ID 리스트  
    public Dictionary\<Vector2, List\<int\>\> AppliedFertilizers { get; set; } \= new();  
}

// HarmonyPatcher.cs  
public static class HarmonyPatcher  
{  
    private static ModEntry modEntry;  
      
    //... Apply 메서드에서 Postfix 패치 추가...  
    // harmony.Patch(..., postfix: new HarmonyMethod(...));

    public static void ApplyFertilizer\_Postfix(HoeDirt \_\_instance, int fertilizer\_id, Farmer who)  
    {  
        Vector2 tile \= \_\_instance.tile.Value;  
        GameLocation location \= who.currentLocation;

        // 해당 위치의 데이터 가져오기 (없으면 생성)  
        if (\!modEntry.LocationData.ContainsKey(location.NameOrUniqueName))  
        {  
            modEntry.LocationData\[location.NameOrUniqueName\] \= new MultiFertilizerData();  
        }  
        var data \= modEntry.LocationData\[location.NameOrUniqueName\];

        // 해당 타일의 데이터 가져오기 (없으면 생성)  
        if (\!data.AppliedFertilizers.ContainsKey(tile))  
        {  
            data.AppliedFertilizers\[tile\] \= new List\<int\>();  
        }

        // 중복 방지를 위해 이미 리스트에 있는지 확인 후 추가  
        if (\!data.AppliedFertilizers\[tile\].Contains(fertilizer\_id))  
        {  
            data.AppliedFertilizers\[tile\].Add(fertilizer\_id);  
        }  
    }  
}

### **2.4. 혜택 누리기: 복합 효과 적용**

#### **HoeDirt.dayUpdate 패치**

비료의 실제 효과(성장 속도 증가, 수분 유지 등)는 일반적으로 매일 밤 dayUpdate 로직을 통해 적용됩니다. 따라서 모드는 이 메서드에도 패치를 적용하여 커스텀 비료들의 복합적인 효과를 구현해야 합니다.

#### **로직**

dayUpdate에 대한 Postfix 패치는 현재 HoeDirt 타일에 대해 모드의 커스텀 데이터 레이어에서 비료 리스트를 조회합니다. 만약 해당 타일에 적용된 비료들이 있다면, 리스트를 순회하며 각 비료의 고유한 효과를 작물에 누적하여 적용합니다. 이를 위해서는 각 비료 ID에 해당하는 효과를 정의하는 내부적인 데이터베이스나 로직이 모드 내에 필요합니다.

#### **코드 샘플: dayUpdate Postfix**

C\#

// HarmonyPatcher.cs  
public static class HarmonyPatcher  
{  
    //... Apply 메서드에서 dayUpdate에 대한 Postfix 패치 추가...

    public static void DayUpdate\_Postfix(HoeDirt \_\_instance, GameLocation environment)  
    {  
        if (\!modEntry.LocationData.TryGetValue(environment.NameOrUniqueName, out var data))  
            return;  
          
        if (\!data.AppliedFertilizers.TryGetValue(\_\_instance.tile.Value, out var fertilizerList))  
            return;

        if (\_\_instance.crop\!= null)  
        {  
            foreach (int fertilizerId in fertilizerList)  
            {  
                // 각 비료 ID에 따른 커스텀 효과 적용  
                ApplyCustomFertilizerEffect(\_\_instance.crop, fertilizerId);  
            }  
        }  
    }

    private static void ApplyCustomFertilizerEffect(Crop crop, int fertilizerId)  
    {  
        // 예시: 특정 비료 ID가 성장 속도를 추가로 10% 감소시킨다고 가정  
        if (fertilizerId \== 801) // 예시 ID: Hyper-Growth Fertilizer  
        {  
            // 작물의 현재 성장 단계에 따라 추가 성장 적용  
            // 이 로직은 매우 복잡할 수 있으며, 바닐라 성장 로직을 신중하게 고려해야 함  
        }  
        // 다른 비료 효과들...  
    }  
}

---

## **제3장: 화면에 그림 그리기: RealtimeMinimap과 커스텀 렌더링**

이 장에서는 복잡한 실시간 UI 요소, 즉 미니맵을 생성하는 데 따르는 기술적 과제에 초점을 맞춥니다. RealtimeMinimap을 모델로 삼아, 에셋 생성 및 캐싱부터 동적 개체의 실시간 추적 및 드로잉에 이르는 전체 렌더링 파이프라인을 탐색합니다.

### **3.1. 아키텍처 청사진: 렌더링 파이프라인**

#### **핵심 구성 요소**

효과적인 미니맵을 구현하기 위해서는 몇 가지 핵심 구성 요소가 필요합니다.

1. **정적 배경 맵 텍스처**: 현재 위치의 지형을 나타내는 이미지.  
2. **UI 프레임/테두리**: 미니맵을 감싸는 장식적인 UI 요소.  
3. **동적 아이콘**: 플레이어, NPC, 그리고 잠재적으로 다른 관심 지점(POI)의 위치를 실시간으로 표시하는 아이콘.

#### **최우선 과제: 성능**

이 장의 중심 주제는 '성능'입니다. 상세한 맵을 렌더링하고 수십 개의 개체를 매 프레임 추적하는 작업은 효율적으로 처리되지 않으면 심각한 성능 저하를 유발할 수 있습니다. 따라서 아키텍처는 캐싱을 최우선으로 고려하고, 메인 렌더링 루프 내에서의 계산을 최소화하도록 설계되어야 합니다.

모드는 게임 프로세스 내에서 '손님'으로 실행되기 때문에, 자체적인 렌더링 예산을 신중하게 관리해야 합니다. 최적화되지 않은 모드는 게임의 프레임률을 심각하게 저하시켜 전반적인 플레이 경험을 해칠 수 있습니다. 미니맵은 수천 개의 타일로 구성된 맵과 다수의 아이콘을 그려야 합니다. 만약 매 프레임마다 원본 타일셋에서 이 모든 것을 다시 그린다면(Display.RenderedHud는 프레임당 여러 번 호출될 수 있음), 이는 성능에 재앙이 될 것입니다. 따라서 유일하게 논리적인 아키텍처는 정적 요소와 동적 요소를 분리하는 것입니다. 정적인 맵 배경은 플레이어가 맵에 진입할 때 단 한 번만 별도의 텍스처로 미리 렌더링(캐싱)되어야 합니다. 그 후 렌더링 루프에서는 이 캐시된 텍스처와 실시간으로 위치가 변경되는 동적 아이콘들만 그리면 됩니다. 이러한 캐싱 전략은 그래픽 프로그래밍의 근본적인 최적화 기법이며, 이와 같은 모드가 실사용 가능하기 위한 절대적인 필수 요건입니다.

### **3.2. 월드 뷰 구축: 맵 텍스처 캐싱**

#### **Player.Warped 이벤트**

맵 텍스처를 생성하고 캐시를 갱신하기에 가장 이상적인 시점은 플레이어가 새로운 장소로 이동했을 때입니다. SMAPI의 player.Warped 이벤트는 이 시점에 정확히 발생하며, 미니맵이 새로운 지역에 맞춰 다시 그려져야 함을 알리는 완벽한 신호입니다.

#### **생성 과정**

이벤트 핸들러 내에서의 과정은 다음과 같습니다.

1. 플레이어가 이동한 새로운 GameLocation 객체에 접근합니다.  
2. 해당 위치의 타일 레이어(location.Map.Layers)를 순회합니다.  
3. 각 타일에 해당하는 이미지를 위치의 타일셋에서 찾아, 화면에 보이지 않는 별도의 RenderTarget2D 객체에 그립니다.  
4. 이 과정이 완료되면, RenderTarget2D는 이제 해당 위치의 전체 맵을 담고 있는 단일 텍스처, 즉 캐시된 맵 텍스처가 됩니다.

#### **코드 샘플: 맵 텍스처 캐싱**

다음은 Player.Warped 이벤트 핸들러 내에서 RenderTarget2D를 사용하여 맵 텍스처를 생성하는 로직의 재구성된 C\# 코드 예시입니다.

C\#

// ModEntry.cs  
using Microsoft.Xna.Framework;  
using Microsoft.Xna.Framework.Graphics;  
using StardewModdingAPI.Events;  
using StardewValley;

public class ModEntry : Mod  
{  
    private RenderTarget2D mapCache;  
      
    public override void Entry(IModHelper helper)  
    {  
        //...  
        helper.Events.Player.Warped \+= OnWarped;  
    }

    private void OnWarped(object sender, WarpedEventArgs e)  
    {  
        if (\!e.IsLocalPlayer) return;

        GameLocation newLocation \= e.NewLocation;  
        int mapWidth \= newLocation.Map.DisplayWidth;  
        int mapHeight \= newLocation.Map.DisplayHeight;

        // 기존 캐시 해제  
        this.mapCache?.Dispose();  
        this.mapCache \= new RenderTarget2D(Game1.graphics.GraphicsDevice, mapWidth, mapHeight);

        // 렌더 타겟 설정  
        Game1.graphics.GraphicsDevice.SetRenderTarget(this.mapCache);  
        Game1.graphics.GraphicsDevice.Clear(Color.Transparent);  
          
        var spriteBatch \= new SpriteBatch(Game1.graphics.GraphicsDevice);  
        spriteBatch.Begin();

        // 맵의 모든 타일 레이어를 순회하며 렌더 타겟에 그리기  
        foreach (var layer in newLocation.Map.Layers)  
        {  
            if (layer.IsVisible)  
            {  
                // Stardew Valley의 내부 맵 드로잉 로직을 모방하거나 직접 구현  
                // layer.Draw(spriteBatch, new xTile.Dimensions.Rectangle(0, 0, mapWidth, mapHeight), Vector2.Zero, false, 4);  
                // 위 코드는 예시이며, 실제로는 더 복잡한 드로잉 로직이 필요함  
            }  
        }

        spriteBatch.End();

        // 기본 렌더 타겟으로 복귀  
        Game1.graphics.GraphicsDevice.SetRenderTarget(null);  
    }  
}

### **3.3. 렌더링 루프: HUD에 그리기**

#### **Display.RenderedHud 사용**

실제 미니맵을 화면에 그리는 작업은 Display.RenderedHud 이벤트 핸들러에서 이루어집니다. 이 핸들러는 매 프레임 호출되어 부드러운 시각적 업데이트를 보장합니다.

#### **드로잉 순서**

성능을 최적화하고 올바른 시각적 레이어를 보장하기 위해, 핸들러 내의 코드는 엄격한 드로잉 순서(화가의 알고리즘)를 따라야 합니다.

1. 미니맵의 UI 프레임/배경을 먼저 그립니다.  
2. 이전에 생성된 캐시된 맵 텍스처(mapCache)를 프레임 내부에 맞게 잘라서 그립니다.  
3. 마지막으로, NPC와 플레이어의 아이콘을 맵 텍스처 위에 그립니다.

#### **코드 샘플: HUD 렌더링**

C\#

// ModEntry.cs  
private Texture2D minimapFrame;  
//...

private void OnRenderedHud(object sender, RenderedHudEventArgs e)  
{  
    if (this.mapCache \== null |

| this.mapCache.IsDisposed) return;

    SpriteBatch spriteBatch \= e.SpriteBatch;  
    Vector2 minimapPosition \= new Vector2(10, 10); // 화면 좌측 상단  
      
    // 1\. 프레임 그리기  
    spriteBatch.Draw(this.minimapFrame, minimapPosition, Color.White);

    // 2\. 캐시된 맵 텍스처 그리기  
    // 플레이어 위치를 중심으로 맵을 잘라내어 표시  
    Rectangle sourceRect \= GetMapViewSourceRectangle();  
    Rectangle destinationRect \= new Rectangle((int)minimapPosition.X \+ 10, (int)minimapPosition.Y \+ 10, 200, 200); // 프레임 내부 영역

    spriteBatch.Draw(this.mapCache, destinationRect, sourceRect, Color.White);

    // 3\. 동적 아이콘 그리기 (다음 섹션에서 다룸)  
    DrawEntityIcons(spriteBatch, sourceRect, destinationRect);  
}

private Rectangle GetMapViewSourceRectangle()  
{  
    // 플레이어의 월드 좌표를 중심으로 맵의 특정 영역을 계산하여 반환  
    int viewSize \= 1024; // 미니맵에 표시할 월드 영역의 크기 (픽셀)  
    int playerX \= (int)Game1.player.Position.X;  
    int playerY \= (int)Game.player.Position.Y;  
      
    return new Rectangle(  
        Math.Max(0, playerX \- viewSize / 2),  
        Math.Max(0, playerY \- viewSize / 2),  
        viewSize,  
        viewSize  
    );  
}

### **3.4. 살아있는 세계 추적: 실시간 개체 위치**

#### **UpdateTicked 이벤트**

렌더링은 RenderedHud에서 수행되지만, 동적 개체들의 위치 *계산*은 GameLoop.UpdateTicked 이벤트에서 처리하는 것이 더 효율적입니다. 이 이벤트는 게임 로직 업데이트 시점에 호출되므로, 렌더링 로직과 계산 로직을 분리하여 각자의 역할에 집중하게 할 수 있습니다.

#### **좌표 변환**

UpdateTicked 핸들러 내에서 코드는 현재 위치의 모든 캐릭터 리스트(Game1.currentLocation.characters)를 순회합니다. 각 캐릭터에 대해 월드 좌표(예: npc.Position.X, npc.Position.Y)를 가져와 미니맵 좌표로 변환합니다. 이는 간단한 비례식 계산을 통해 이루어집니다(예: minimapX \= worldX / worldWidth \* minimapWidth).

#### **위치 저장**

계산된 각 개체의 미니맵 좌표는 RenderedHud 핸들러가 읽을 수 있는 임시 리스트나 딕셔너리에 저장됩니다. 이렇게 하면 RenderedHud는 복잡한 계산 없이 단순히 미리 계산된 좌표에 아이콘을 그리기만 하면 되므로, 렌더링 루프 자체의 속도를 최대한 빠르게 유지할 수 있습니다.

#### **코드 샘플: 개체 추적 및 아이콘 렌더링**

C\#

// ModEntry.cs  
private Dictionary\<NPC, Vector2\> npcMinimapPositions \= new();

public override void Entry(IModHelper helper)  
{  
    //...  
    helper.Events.GameLoop.UpdateTicked \+= OnUpdateTicked;  
}

private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)  
{  
    if (\!Context.IsWorldReady) return;

    this.npcMinimapPositions.Clear();  
    foreach (var npc in Game1.currentLocation.characters)  
    {  
        // 월드 좌표를 미니맵 좌표로 변환하는 로직  
        Vector2 minimapPos \= ConvertWorldToMinimapCoords(npc.Position);  
        this.npcMinimapPositions\[npc\] \= minimapPos;  
    }  
}

// OnRenderedHud 메서드 내에서 호출될 함수  
private void DrawEntityIcons(SpriteBatch spriteBatch, Rectangle sourceRect, Rectangle destinationRect)  
{  
    // NPC 아이콘 그리기  
    foreach (var pair in this.npcMinimapPositions)  
    {  
        NPC npc \= pair.Key;  
        Vector2 worldPos \= npc.Position;

        // 현재 미니맵 뷰에 보이는 NPC만 그리기  
        if (sourceRect.Contains(worldPos.X, worldPos.Y))  
        {  
            // 월드 좌표를 미니맵의 상대 좌표로 변환  
            float relativeX \= (worldPos.X \- sourceRect.X) / sourceRect.Width;  
            float relativeY \= (worldPos.Y \- sourceRect.Y) / sourceRect.Height;

            Vector2 iconPos \= new Vector2(  
                destinationRect.X \+ relativeX \* destinationRect.Width,  
                destinationRect.Y \+ relativeY \* destinationRect.Height  
            );  
              
            // NPC 얼굴 스프라이트를 아이콘으로 사용  
            spriteBatch.Draw(npc.Sprite.Texture, iconPos, npc.getMugShotSourceRect(), Color.White);  
        }  
    }  
      
    // 플레이어 아이콘 그리기 (중앙에 고정)  
    Vector2 playerIconPos \= new Vector2(  
        destinationRect.X \+ destinationRect.Width / 2,  
        destinationRect.Y \+ destinationRect.Height / 2  
    );  
    //... 플레이어 아이콘 그리는 로직...  
}

---

## **결론: 모딩 패턴의 종합**

### **작업에 적합한 도구 선택**

이 가이드에서 분석한 세 가지 사례 연구는 스타듀밸리 모딩의 핵심적인 교훈을 종합적으로 보여줍니다. 각 모드는 서로 다른 목표를 달성하기 위해 각기 다른 아키텍처 패턴을 채택했으며, 이는 개발자가 자신의 프로젝트에 가장 적합한 도구를 선택하는 데 중요한 지침을 제공합니다.

* \*\*AnotherHungerMod\*\*는 SMAPI의 이벤트 기반 모델이 얼마나 안전하고 안정적인지를 보여주었습니다. 새로운 독립형 시스템을 추가할 때, 이 방식은 게임의 핵심 코드와의 충돌을 최소화하고 향후 호환성을 보장하는 최선의 선택입니다.  
* \*\*MultiFertilizer\*\*는 Harmony 패치의 강력함과 그에 따르는 위험을 동시에 드러냈습니다. 하드코딩된 바닐라 동작을 반드시 변경해야 할 때 Harmony는 필수적이지만, 그 복잡성과 잠재적인 호환성 문제는 신중한 접근을 요구합니다.  
* \*\*RealtimeMinimap\*\*은 성능이 중요한 커스텀 UI를 설계할 때의 핵심 원칙을 강조했습니다. 렌더링 파이프라인에서 캐싱과 계산-렌더링 분리 원칙을 우선시하지 않으면, 아무리 훌륭한 기능이라도 플레이 경험을 저해하는 애물단지가 될 수 있습니다.

### **모더의 의사결정 매트릭스**

이러한 교훈을 바탕으로, 개발자들은 자신의 모드 아이디어를 실현하기 위한 아키텍처 패턴을 결정할 때 다음과 같은 지침을 고려할 수 있습니다.

* **"만약 새로운, 독립적인 시스템을 추가하려 한다면, SMAPI 이벤트를 사용하라."** 이는 가장 안전하고 유지보수가 용이한 방법입니다.  
* **"만약 하드코딩된 바닐라의 동작을 반드시 변경해야 한다면, Harmony를 신중하게 사용하라."** 특히 트랜스파일러는 최후의 수단으로 고려하고, 다른 모드와의 호환성을 철저히 테스트해야 합니다.  
* **"만약 커스텀 UI, 특히 실시간으로 업데이트되는 요소를 만들려 한다면, 캐싱과 성능 최적화를 최우선으로 설계하라."** 계산 비용이 높은 작업은 렌더링 루프 밖에서 처리하는 것을 원칙으로 삼아야 합니다.

궁극적으로, 성공적인 모드는 단순히 기발한 아이디어를 구현하는 것을 넘어, 게임의 생태계 내에서 조화롭게 작동하는 안정적이고 효율적인 소프트웨어를 만드는 것입니다. 이 가이드에서 분석한 세 가지 모딩 패턴은 그러한 목표를 달성하기 위한 청사진을 제공합니다.

### **최종 요약: 핵심 모딩 아키텍처 비교**

다음 표는 이 보고서의 분석 결과를 간결하게 요약하여 개발자들이 빠른 참조 자료로 활용할 수 있도록 구성되었습니다.

**표 1: 핵심 모딩 아키텍처 비교**

| 속성 | AnotherHungerMod | MultiFertilizer | RealtimeMinimap |
| :---- | :---- | :---- | :---- |
| **핵심 목표** | 새로운 게임플레이 시스템 추가 | 핵심 게임 메커니즘 변경 | 실시간 UI 렌더링 |
| **주요 통합 방식** | SMAPI 이벤트 기반 | Harmony 런타임 패치 | SMAPI 렌더링 이벤트 |
| **핵심 훅/타겟** | GameLoop.TimeChanged, Display.RenderedHud | HoeDirt.applyFertilizer, HoeDirt.dayUpdate | Display.RenderedHud, Player.Warped |
| **커스텀 데이터 전략** | IDataHelper를 통한 플레이어별 데이터 모델 | 장소별 병렬 데이터 구조 (Dictionary) | 프레임별 일시적인 개체 위치 데이터 |
| **주요 도전 과제** | 시스템 상태 관리 및 데이터 지속성 | 호환성 및 코드 복잡성 | 렌더링 성능 및 최적화 |

#### **참고 자료**

1. spacechase0/StardewValleyMods: New home for my stardew valley mod source code, 9월 16, 2025에 액세스, [https://github.com/spacechase0/StardewValleyMods](https://github.com/spacechase0/StardewValleyMods)  
2. 1월 1, 1970에 액세스, [https://github.com/spacechase0/StardewValleyMods/tree/develop/RealtimeMinimap](https://github.com/spacechase0/StardewValleyMods/tree/develop/RealtimeMinimap)  
3. 1월 1, 1970에 액세스, [https://github.com/spacechase0/StardewValleyMods/tree/develop/MultiFertilizer](https://github.com/spacechase0/StardewValleyMods/tree/develop/MultiFertilizer)  
4. Another Hunger Mod \- Stardew Valley \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mod/568430](https://www.moddrop.com/stardew-valley/mod/568430)  
5. Giant Crop Fertilizer | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/1135913-giant-crop-fertilizer](https://www.moddrop.com/stardew-valley/mods/1135913-giant-crop-fertilizer)  
6. Unofficial mod updates | Page 161 \- Stardew Valley Forums, 9월 16, 2025에 액세스, [https://forums.stardewvalley.net/threads/unofficial-mod-updates.2096/page-161](https://forums.stardewvalley.net/threads/unofficial-mod-updates.2096/page-161)  
7. atravita-mods/MoreFertilizers \- GitHub, 9월 16, 2025에 액세스, [https://github.com/atravita-mods/MoreFertilizers](https://github.com/atravita-mods/MoreFertilizers)  
8. foxwhite25/Stardew-Ultimate-Fertilizer \- GitHub, 9월 16, 2025에 액세스, [https://github.com/foxwhite25/Stardew-Ultimate-Fertilizer](https://github.com/foxwhite25/Stardew-Ultimate-Fertilizer)