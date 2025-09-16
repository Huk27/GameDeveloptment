

# **스타듀밸리 모딩 해부: spacechase0의 핵심 메커닉에 대한 개발자 가이드**

## **서론: 아카이브에서 배우기**

### **목적과 전제**

본 보고서는 스타듀밸리 모딩 커뮤니티에서 지대한 영향을 끼친 모더(modder) spacechase0가 제작한 몇 가지 핵심 모드의 아키텍처를 심층적으로 분석하는 교육 자료 역할을 합니다.1 분석의 초점은 "아카이브된(archived)" 모드에 맞춰져 있으며, 이를 복잡한 설계 문제를 해결하는 방법을 보여주는 귀중한 사례 연구로 다룹니다. 이 가이드는 다른 개발자들이 이러한 모드들의 독특한 기능이 어떤 원리로, 어떻게 구현되었는지 학습하고 재현할 수 있도록 실전적인 통합 개발 지침을 제공하는 것을 목표로 합니다.

### **방법론**

이 분석은 공개적으로 이용 가능한 정보, 커뮤니티 토론, 그리고 스타듀밸리 모딩 생태계의 표준 구현 패턴에 기반합니다. 분석 대상 모드들의 리포지토리가 아카이브된 상태이므로, 소스 코드에 직접 접근하는 것이 불가능한 경우가 많습니다.2 따라서 본 보고서에서 제공되는 코드 샘플은 커뮤니티의 일반적인 개발 관행과 디컴파일된 게임 코드 분석을 통해 재구성된 것입니다.7 이 코드들은 기능적으로 원본을 대표하고 교육적 가치를 갖도록 설계되었습니다.

### **핵심 기술 입문**

분석에 앞서, 모딩에 사용되는 기반 기술들을 간략히 살펴봅니다.

* **SMAPI (Stardew Modding API):** 모드가 게임과 상호작용할 수 있도록 안정적이고 이벤트 기반의 프레임워크를 제공하는 필수 모드 로더입니다.8 본문에서는 분석 대상 모드들이 SMAPI의 이벤트 시스템을 어떻게 활용하여 입력, 업데이트, 렌더링을 처리하는지 탐구할 것입니다.  
* **Harmony:** SMAPI 이벤트만으로는 불충분할 때, 하드코딩된 게임 로직을 변경할 수 있게 해주는 런타임 패치(메서드 주입) 라이브러리입니다. 강력한 기능만큼이나 불안정성을 야기할 수 있는 잠재력은 본 보고서 전반에 걸쳐 반복적으로 다뤄질 주제입니다.10

### **1.6 업데이트의 맥락**

스타듀밸리 1.6 버전은.NET 6로의 전환, 네임스페이스 기반 ID 시스템 도입 등 주요 아키텍처 변경 사항을 포함했습니다.12 이 가이드는 각 모드를 본래의 개발 환경 맥락에서 분석하고, 마지막 장에서 이러한 레거시 기술들을 현대적인 1.6+ 환경에 맞게 조정하는 방법을 집중적으로 논의할 것입니다.

**표 1: 모딩 기술 요약**

| 모드 이름 | 핵심 모딩 과제 | 주요 기술/API |
| :---- | :---- | :---- |
| Cooking Skill | 시스템 확장 (신규 성장 시스템) | SpaceCore API & Harmony |
| Experience Bars | 커스텀 UI 렌더링 | SMAPI 디스플레이 이벤트 |
| Jump Over | 물리 및 충돌 오버라이드 | SMAPI 게임 루프 & 입력 이벤트 |
| Custom NPC Fixes | 런타임 버그 수정 | Harmony (Postfix 패치) |
| Throwable Axe | 아이템 행동 확장 | SMAPI 입력 이벤트 & Projectile 클래스 |

## **제1장: 시스템 확장 \- Cooking Skill 모드**

### **1.1. 기능 분석**

Cooking Skill 모드는 게임의 기존 5가지 기술 시스템에 여섯 번째 기술인 '요리'를 도입합니다. 여기에는 요리를 통해 경험치를 얻고, 레벨을 올려 혜택을 잠금 해제하며, 5레벨과 10레벨에 도달했을 때 전문직을 선택하는 기능이 포함됩니다.13 이는 게임의 핵심 플레이 루프에 대한 중대한 확장입니다.

### **1.2. 아키텍처 원칙: 프레임워크의 힘**

이 모드의 핵심 아키텍처 원칙은 spacechase0의 또 다른 창작물인 **SpaceCore** 프레임워크에 대한 의존성입니다.9 SpaceCore는 새로운 기술을 등록하기 위한 전용 API를 제공하며, 이를 통해 게임의 UI, 저장 데이터, 레벨업 로직을 수정하는 복잡한 내부 작업을 추상화합니다.

이러한 접근 방식은 성숙하고 모듈화된 모드 개발 방법론을 보여줍니다. Cooking Skill 모드 내부에 모든 UI 및 데이터 관리 코드를 포함하는 단일 구조(monolithic)로 구축하는 대신, spacechase0는 먼저 재사용 가능하고 일반적인 "기술 프레임워크"(SpaceCore)를 만들었습니다. 이는 소프트웨어 설계에 대한 중요한 교훈을 시사합니다. 즉, 일반적인 문제를 먼저 해결하면 특정 문제(요리 기술 추가 등)를 해결하는 것이 훨씬 간단해진다는 것입니다. 다른 여러 요리 기술 모드(YACS, Love of Cooking 등)의 존재는 이 기능이 바람직하지만 구현하기 어려운 기능임을 강조하며, 프레임워크 기반 접근 방식의 타당성을 입증합니다.18

Cooking Skill 모드를 이해하는 데 있어 가장 중요한 것은 이 모드가 독립적인 개체가 아니라, SpaceCore라는 API의 클라이언트라는 점입니다. 이 관계는 때로는 하나의 모드를 만드는 최선의 방법이, 필요한 도구를 제공하는 더 기초적인 다른 모드를 먼저 만드는 것일 수 있다는 사실을 개발자들에게 가르쳐 줍니다.

### **1.3. 구현 심층 분석 및 코드 샘플**

#### **기술 등록**

모드의 진입점(Entry 메서드)은 먼저 SMAPI의 헬퍼를 통해 SpaceCore API를 가져온 다음, 기술 등록 메서드를 호출하는 방식으로 구현됩니다.

C\#

// ModEntry.cs의 GameLaunched 이벤트 핸들러 내부  
public override void Entry(IModHelper helper)  
{  
    helper.Events.GameLoop.GameLaunched \+= (s, e) \=\>  
    {  
        var api \= this.Helper.ModRegistry.GetApi\<ISpaceCoreApi\>("spacechase0.SpaceCore");  
        if (api \== null)  
        {  
            this.Monitor.Log("SpaceCore API를 가져올 수 없습니다. Cooking Skill 모드가 비활성화됩니다.", LogLevel.Error);  
            return;  
        }

        // CookingSkill은 SpaceCore의 기술 인터페이스를 구현하는 커스텀 클래스  
        api.RegisterSkill(new CookingSkill());  
    };  
}

#### **경험치 부여**

모드는 플레이어가 아이템을 요리하는 시점을 감지해야 합니다. 이는 일반적으로 게임의 요리 로직을 패치하여 수행됩니다. CraftingPage 클래스의 생성자나 clickCraftingRecipe 메서드에 Harmony 패치를 적용하는 것이 제작 완료 시점을 가로채는 가장 직접적인 방법입니다.

C\#

// Harmony 패치 파일 내부

public static class CraftingPage\_Click\_Patch  
{  
    // Postfix 패치는 원본 메서드 실행 \*후\*에 코드를 실행합니다.  
    public static void Postfix(CraftingPage \_\_instance, ClickableTextureComponent c, bool playSound)  
    {  
        // 현재 페이지가 요리 페이지이고, 제작이 성공적으로 이루어졌는지 확인  
        if (\_\_instance.cooking && Game1.player.craftingRecipes.ContainsKey(c.name))  
        {  
            // 요리된 아이템의 가치나 재료를 기반으로 경험치 계산  
            int experienceGained \= CalculateXpForRecipe(c.name);

            // SpaceCore API를 사용하여 경험치 추가  
            // "spacechase0.Cooking"은 등록된 스킬의 고유 ID  
            SpaceCore.API.AddExperience("spacechase0.Cooking", experienceGained);  
        }  
    }

    private static int CalculateXpForRecipe(string recipeName)  
    {  
        // 실제 구현에서는 CraftingRecipe.createItem()을 통해 아이템 객체를 생성하고  
        // 그 가치(price)를 기반으로 경험치를 계산하는 로직이 필요합니다.  
        // 예시로 고정값 반환  
        return 10;  
    }  
}

#### **전문직 효과 적용**

전문직은 게임 메커닉을 수정하는 지속 효과(passive bonus)를 부여합니다. 예를 들어, "미식가(Gourmet)" 전문직은 요리된 아이템의 판매 가격을 20% 증가시킵니다.13 이를 구현하려면 아이템의 판매 가격을 계산하는

StardewValley.Object 클래스의 sellToStorePrice 메서드에 Harmony 패치를 적용해야 합니다.

C\#

// 다른 Harmony 패치 파일 내부

public static class Object\_SellPrice\_Patch  
{  
    // \_\_instance는 원본 메서드가 호출된 객체 인스턴스  
    // \_\_result는 원본 메서드의 반환값 (ref 키워드로 수정 가능)  
    public static void Postfix(StardewValley.Object \_\_instance, ref int \_\_result)  
    {  
        // 플레이어가 "Gourmet" 전문직을 가지고 있고, 아이템이 요리 카테고리인지 확인  
        if (SpaceCore.API.HasProfession("spacechase0.Cooking", "Gourmet") && \_\_instance.Category \== StardewValley.Object.CookingCategory)  
        {  
            // 판매 가격을 20% 증가시킴  
            \_\_result \= (int)(\_\_result \* 1.20f);  
        }  
    }  
}

## **제2장: 커스텀 UI 렌더링 \- Experience Bars 모드**

### **2.1. 기능 분석**

이 모드는 각 기술 레벨의 다음 단계까지 얼마나 남았는지 보여주는 그래픽 바를 HUD에 표시하는, 간단하지만 필수적인 편의성(quality-of-life) 기능을 제공합니다.20 또한 표시 여부를 전환하고 화면상의 위치를 변경하는 기능도 포함합니다.

### **2.2. 아키텍처 원칙: 상태 읽기 및 HUD에 그리기**

핵심 원칙은 게임의 렌더링 루프에 연결하여 Game1.player 객체에서 직접 데이터를 읽고, 진행률을 계산한 다음, 게임의 SpriteBatch를 사용하여 커스텀 텍스처와 텍스트를 화면에 직접 그리는 것입니다. 이는 게임 상태를 수정하지 않는 "읽기 전용" 접근 방식으로, 본질적으로 안정적이며 다른 모드와의 호환성이 높습니다.

이 모드는 비침습적 UI 모드의 전형적인 예시입니다. 그 우아함은 단순성과 게임 데이터에 대한 직접적인 접근에 있습니다. 이는 이벤트 연결 \-\> 데이터 읽기 \-\> 계산 \-\> 그리기라는 근본적인 패턴을 가르쳐 줍니다. 이 패턴은 허수아비 범위를 보여주거나 NPC 위치를 지도에 표시하는 등 수많은 편의성 모드의 기초가 됩니다. 이는 개발자들이 게임 메커닉을 변경하지 않고 플레이어에게 정보를 제공하는 방법을 이해하기 위한 완벽한 출발점입니다.

### **2.3. 구현 심층 분석 및 코드 샘플**

#### **렌더링 루프 연결**

HUD 요소를 그리는 데 가장 이상적인 SMAPI 이벤트는 Display.RenderedHud입니다. 이 이벤트는 바닐라 HUD가 그려진 후 매 프레임마다 발생하므로, 커스텀 UI가 항상 위에 표시되도록 보장합니다.

C\#

// ModEntry.cs 내부  
public override void Entry(IModHelper helper)  
{  
    helper.Events.Display.RenderedHud \+= this.OnRenderedHud;  
}

#### **경험치 데이터 접근 및 그리기**

이벤트 핸들러 내에서 모드는 Game1.player의 experiencePoints 배열에 저장된 플레이어의 경험치에 접근합니다. 그런 다음 현재 레벨과 다음 레벨에 필요한 경험치를 조회하여 바의 채우기 비율을 계산합니다.

C\#

// 각 레벨업에 필요한 누적 경험치  
private static readonly int XpNeededForLevel \= { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };

private void OnRenderedHud(object sender, RenderedHudEventArgs e)  
{  
    if (\!Context.IsWorldReady) return;

    // 그리기를 위한 SpriteBatch 가져오기  
    var b \= e.SpriteBatch;

    // 농사 기술(인덱스 0)에 대한 예시  
    int skillIndex \= Farmer.farmingSkill;  
    int currentLevel \= Game1.player.GetSkillLevel(skillIndex);  
    if (currentLevel \>= 10) return; // 최고 레벨이면 그리지 않음

    int currentXp \= Game1.player.experiencePoints\[skillIndex\];  
    int xpForCurrentLevel \= currentLevel \> 0? XpNeededForLevel\[currentLevel \- 1\] : 0;  
    int xpForNextLevel \= XpNeededForLevel\[currentLevel\];

    // 현재 레벨에서의 진행률 계산  
    float progress \= (float)(currentXp \- xpForCurrentLevel) / (xpForNextLevel \- xpForCurrentLevel);  
    progress \= Math.Clamp(progress, 0f, 1f); // 0과 1 사이의 값으로 제한

    // UI 위치 (x, y)는 설정 파일에서 읽어옴  
    Vector2 position \= new Vector2(this.Config.X, this.Config.Y);

    // 배경 바 그리기 (barBackground와 barForeground는 미리 로드된 Texture2D 에셋)  
    b.Draw(barBackground, position, Color.White);

    // 전경/진행률 바 그리기  
    b.Draw(barForeground, position,  
           new Rectangle(0, 0, (int)(barForeground.Width \* progress), barForeground.Height),  
           Color.White);  
}

#### **사용자 설정 구현**

모드는 사용자가 표시 여부를 전환하고 바를 이동할 수 있도록 합니다.20 이는

Input.ButtonPressed 이벤트를 수신하여 처리됩니다.

C\#

// ModEntry.cs 내부  
private bool isMoving \= false;

private void OnButtonPressed(object sender, ButtonPressedEventArgs e)  
{  
    if (\!Context.IsPlayerFree) return;

    // 설정된 키(예: X)가 눌렸는지 확인  
    if (e.Button \== this.Config.ToggleKey)  
    {  
        // Shift 키와 함께 누르면 이동 모드 전환  
        if (e.IsDown(SButton.LeftShift))  
        {  
            this.isMoving \=\!this.isMoving;  
        }  
        else // 그냥 누르면 표시 여부 전환  
        {  
            this.Config.Visible \=\!this.Config.Visible;  
            this.Helper.WriteConfig(this.Config); // 변경 사항 저장  
        }  
    }

    // 이동 모드에서 마우스 클릭 시 위치 고정  
    if (this.isMoving && e.Button.IsUseToolButton())  
    {  
        this.isMoving \= false;  
        this.Helper.WriteConfig(this.Config); // 새 위치 저장  
    }  
}

// GameLoop.UpdateTicked 이벤트 핸들러에서 이동 로직 처리  
private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)  
{  
    if (this.isMoving)  
    {  
        // 바의 위치를 마우스 커서 위치로 업데이트  
        this.Config.X \= Game1.getMouseX();  
        this.Config.Y \= Game1.getMouseY();  
    }  
}

## **제3장: 게임 물리 조작 \- Jump Over 모드**

### **3.1. 기능 분석**

이 모드는 스타듀밸리의 2D 타일 기반 이동 시스템에는 존재하지 않는 메커닉인 수직 점프를 도입합니다.23 이를 통해 플레이어는 울타리와 같은 낮은 장애물을 넘어갈 수 있어 맵 탐색에 새로운 차원을 더합니다.

### **3.2. 아키텍처 원칙: Z축 위장하기**

게임 엔진에는 플레이어 이동을 위한 Z축 개념이 내장되어 있지 않으므로, 모드는 이를 시뮬레이션해야 합니다. 여기에는 세 가지 핵심 요소가 포함됩니다.

1. **상태 관리:** 일반적인 플레이어 물리 및 행동이 일시 중단되는 임시 "점프 중" 상태를 만듭니다.  
2. **이동 시뮬레이션:** 짧은 시간 동안 플레이어의 이동을 위한 포물선 궤적을 계산합니다.  
3. **시각적 착시:** 플레이어의 스프라이트 렌더링을 조작하여 공중에 떠 있는 것처럼 보이게 합니다.

이 모드는 창의적으로 한계를 극복하는 방법을 보여주는 훌륭한 예시입니다. 이는 모더의 목표가 단지 게임이 제공하는 도구를 사용하는 것이 아니라, 게임에 *없는* 도구가 있는 것처럼 보이게 만드는 것임을 보여줍니다. 2D 이동이라는 게임의 한계 \-\> 수직 이동이라는 모더의 목표 \-\> Z축 시뮬레이션이라는 창의적 해결책으로 이어지는 인과 관계가 성립합니다. 이는 렌더링 파이프라인(yJumpOffset, 스프라이트 드로잉 순서)을 이해하는 것이 게임 로직 파이프라인을 이해하는 것만큼 중요하다는 교훈을 줍니다. 모드는 점프가 끝날 때까지 플레이어의 실제 타일 위치를 변경하지 않습니다. 사전 계산된 경로 위에 순전히 시각적 효과를 덧씌우는 것입니다. 이처럼 로직과 표현을 분리하는 것은 정교한 프로그래밍 개념입니다.

### **3.3. 구현 심층 분석 및 코드 샘플**

#### **점프 발동**

모드는 키 입력을 감지한 다음, 일련의 검사를 수행하여 점프의 유효성을 확인합니다.

C\#

// GameLoop.UpdateTicked 이벤트 핸들러 내부  
private bool isJumping \= false;

private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)  
{  
    // 이미 점프 중이거나 플레이어가 움직일 수 없는 상태면 아무것도 하지 않음  
    if (this.isJumping ||\!Game1.player.CanMove) return;

    // 설정된 점프 키가 눌렸는지 확인  
    if (this.Helper.Input.IsDown(this.Config.JumpKey))  
    {  
        // 플레이어가 바라보는 방향에 따라 목표 타일 결정  
        Vector2 currentTile \= Game1.player.getTileLocation();  
        Vector2 targetTile \= currentTile \+ GetFacingDirectionVector(Game1.player.FacingDirection);  
        Vector2 landingTile \= targetTile \+ GetFacingDirectionVector(Game1.player.FacingDirection);

        // 목표 타일이 점프 가능한 장애물(예: 울타리)이고 착지 지점이 비어있는지 확인  
        if (IsJumpable(targetTile) && IsPassable(landingTile))  
        {  
            StartJump(currentTile \* 64f, landingTile \* 64f); // 픽셀 위치로 변환하여 전달  
        }  
    }  
}

private bool IsJumpable(Vector2 tile)  
{  
    // 해당 타일에 Fence 객체가 있는지 확인하는 로직  
    return Game1.currentLocation.objects.ContainsKey(tile) && Game1.currentLocation.objects\[tile\] is Fence;  
}

private bool IsPassable(Vector2 tile)  
{  
    // 해당 타일이 비어있고 통과 가능한지 확인하는 로직  
    return\!Game1.currentLocation.isTileOccupied(tile) && Game1.currentLocation.isTilePassable(new xTile.Dimensions.Location((int)tile.X, (int)tile.Y), Game1.viewport);  
}

#### **점프 궤적 및 상태 관리**

점프가 시작되면 UpdateTicked 이벤트가 여러 프레임에 걸쳐 점프의 진행 상황을 관리하는 데 사용됩니다. 이 동안 플레이어의 조작은 일시적으로 비활성화됩니다.

C\#

private int jumpTicksRemaining \= 0;  
private const int JUMP\_DURATION\_TICKS \= 30; // 30틱 \= 0.5초  
private Vector2 jumpStartPosition;  
private Vector2 jumpTargetPosition;

private void StartJump(Vector2 startPixel, Vector2 targetPixel)  
{  
    this.isJumping \= true;  
    this.jumpTicksRemaining \= JUMP\_DURATION\_TICKS;  
    this.jumpStartPosition \= startPixel;  
    this.jumpTargetPosition \= targetPixel;  
    Game1.player.CanMove \= false; // 플레이어 조작 비활성화  
}

// OnUpdateTicked 내부  
if (this.isJumping)  
{  
    this.jumpTicksRemaining--;

    // 점프 진행률 계산  
    float progress \= 1.0f \- (this.jumpTicksRemaining / (float)JUMP\_DURATION\_TICKS);

    // 선형 보간(Lerp)을 사용하여 플레이어를 목표 지점으로 이동  
    Game1.player.Position \= Vector2.Lerp(this.jumpStartPosition, this.jumpTargetPosition, progress);

    // 시각적 착시를 위한 수직 오프셋 계산 (포물선 운동)  
    float halfDuration \= JUMP\_DURATION\_TICKS / 2.0f;  
    float parabola \= \-1 \* (this.jumpTicksRemaining \- halfDuration) \* (this.jumpTicksRemaining \- halfDuration) \+ (halfDuration \* halfDuration);  
    Game1.player.yJumpOffset \= (int)(parabola \* 0.5f); // 점프 높이 조절을 위한 계수

    if (this.jumpTicksRemaining \<= 0)  
    {  
        // 점프 종료  
        this.isJumping \= false;  
        Game1.player.CanMove \= true;  
        Game1.player.yJumpOffset \= 0;  
        // 위치를 타일에 정확히 맞춤  
        Game1.player.Position \= this.jumpTargetPosition;  
    }  
}

## **제4장: Harmony를 이용한 고급 런타임 패치 \- Custom NPC Fixes 모드**

### **4.1. 기능 분석**

Custom NPC Fixes는 플레이어가 커스텀(모드로 추가된) NPC를 사용할 때 발생하는 일반적인 게임 충돌 버그를 해결하기 위해 설계된 유틸리티 모드입니다. 이 모드는 NPC 스폰, 경로 탐색, 게임 이벤트 참여와 관련된 문제들을 해결합니다.24

### **4.2. 아키텍처 원칙: Harmony를 이용한 정밀 코드 주입**

이 모드의 존재 이유는 SMAPI API의 한계에 있습니다. 이벤트나 퀘스트를 위해 게임 내 모든 캐릭터 목록을 수집하는 유틸리티 함수와 같은 특정 핵심 게임 기능은 SMAPI 이벤트를 통해 노출되지 않습니다. 이 동작을 수정하는 유일한 방법은 Harmony를 사용하여 런타임에 게임의 컴파일된 코드를 직접 변경하는 것입니다.10

이 모드에서 얻을 수 있는 가장 심오한 교훈은 모드의 전체 생애주기 그 자체입니다. 이 모드는 바닐라 게임이 모드 콘텐츠를 처리하는 데 있던 결함을 수정하기 위해 만들어졌습니다. 이 모드의 GitHub 이슈 페이지에는 StardewValley.Utility.getAllCharacters 메서드가 커스텀 NPC를 제대로 처리하지 못해 충돌이 발생하는 상황이 명시적으로 기록되어 있습니다.28 이것이 바로 모드 개발의 동기가 된 결정적 증거입니다. 모드는 이 메서드를 패치하여 문제를 해결했습니다. 그러나 스타듀밸리 1.6 업데이트에서 기본 게임 코드가 커스텀 NPC를 올바르게 처리하도록 개선되면서 이 모드의 주요 기능은 쓸모없게 되었습니다.29 이는 흥미로운 공생 관계를 보여줍니다. 모드는 기본 게임에 대한 임시적인, 커뮤니티 기반의 패치 역할을 할 수 있으며, 그 성공은 공식 개발자가 해당 수정을 통합할 때 궁극적으로 자신의 소멸로 이어질 수 있습니다. 이는 모딩 커뮤니티가 때로는 공식 패치보다 수년 앞서 핵심 게임 문제를 식별하고 해결하는 데 중요한 역할을 한다는 강력한 서사를 구성합니다.

### **4.3. 구현 심층 분석 및 코드 샘플**

#### **Harmony 인스턴스 설정**

Harmony 기반 모드의 첫 단계는 Entry 메서드에서 고유한 Harmony 인스턴스를 생성하는 것입니다.

C\#

// ModEntry.cs 내부  
public override void Entry(IModHelper helper)  
{  
    var harmony \= new Harmony(this.ModManifest.UniqueID);

    // 패치 적용  
    // StardewValley.Utility 클래스의 getAllCharacters 메서드를 대상으로 함  
    // 이 메서드는 여러 오버로드가 있으므로, List\<NPC\>를 인자로 받는 버전을 명시  
    harmony.Patch(  
        original: AccessTools.Method(typeof(StardewValley.Utility), nameof(StardewValley.Utility.getAllCharacters), new Type { typeof(List\<NPC\>) }),  
        postfix: new HarmonyMethod(typeof(GetAllCharacters\_Patch), nameof(GetAllCharacters\_Patch.Postfix))  
    );

    // 다른 클래스에서 로깅을 사용할 수 있도록 IMonitor 인스턴스 전달  
    GetAllCharacters\_Patch.Initialize(this.Monitor);  
}

#### **Postfix 패치**

Postfix는 가장 안전한 패치 유형입니다. 원본 메서드가 완료된 *후*에 실행되므로, 모드가 결과를 검사하거나 수정할 수 있습니다. 여기서 패치는 바닐라 메서드가 누락했을 수 있는 커스텀 NPC를 최종 목록에 추가하도록 보장합니다.

C\#

// 별도의 패치 파일 내부  
public static class GetAllCharacters\_Patch  
{  
    private static IMonitor Monitor;  
    public static void Initialize(IMonitor monitor) { Monitor \= monitor; }

    // list는 원본 메서드의 인자로 전달된 List\<NPC\> 객체  
    public static void Postfix(List\<NPC\> list)  
    {  
        try  
        {  
            if (list \== null) return;

            // 모든 게임 위치를 순회하며 원본 메서드가 놓쳤을 수 있는 NPC를 찾음  
            foreach (GameLocation location in Game1.locations)  
            {  
                foreach (NPC npc in location.characters)  
                {  
                    // 목록에 이 NPC가 아직 포함되어 있지 않다면 추가  
                    // Contains는 참조 비교를 하므로 동일한 인스턴스인지 확인  
                    if (\!list.Contains(npc))  
                    {  
                        list.Add(npc);  
                    }  
                }  
            }  
        }  
        catch (Exception ex)  
        {  
            // 오류 발생 시 로그를 남겨 디버깅을 용이하게 함  
            Monitor.Log($"Failed in {nameof(GetAllCharacters\_Patch.Postfix)}:\\n{ex}", LogLevel.Error);  
        }  
    }  
}

이 예시는 Harmony의 주요 모범 사례를 보여줍니다. 최대 호환성을 위해 Postfix를 사용하고, 모드가 게임을 중단시키는 것을 방지하기 위해 로직을 try-catch 블록으로 감싸며, 쉬운 디버깅을 위해 모든 오류를 기록하는 것입니다.10

## **제5장: 아이템 행동 확장 \- Throwable Axe 모드**

### **5.1. 기능 분석**

이 모드는 채집 도구인 일반 도끼를 하이브리드 도구/무기로 변환합니다. 플레이어는 마우스 오른쪽 버튼을 클릭하여 도끼를 던질 수 있으며, 던져진 도끼는 일정 거리를 날아가 몬스터에게 피해를 주고 부메랑처럼 돌아옵니다.31

### **5.2. 아키텍처 원칙: 입력 가로채기 및 발사체 관리**

모드의 로직은 두 부분으로 나뉩니다.

1. **기본 행동 오버라이드:** 도끼를 들고 있을 때 플레이어의 입력(오른쪽 클릭)을 가로채고, 게임의 기본 동작(도끼의 오른쪽 클릭에는 아무 동작도 없음)이나 다른 모드의 동작을 방지해야 합니다.  
2. **임시 엔티티 생성 및 관리:** 게임 세계에 Projectile 객체를 생성하고, 커스텀 물리(초기 속도, 부메랑 복귀)를 부여하며, 적 및 환경과의 충돌을 처리해야 합니다.

이 모드는 기존 아이템에 완전히 새로운 기능을 부여하는 방법을 개발자에게 가르쳐 줍니다. 이는 게임의 엄격한 "도구" 대 "무기" 범주를 모호하게 만듭니다.32 여기서 핵심은 아이템의 정체성이

*그것과 상호작용하는 시스템*에 의해 정의된다는 점입니다. ThrowAxe라는 새로운 상호작용 시스템을 만들어냄으로써, 모드는 도끼를 재맥락화합니다. 이는 새로운 게임플레이를 만들기 위해 항상 처음부터 새로운 아이템을 만들 필요는 없다는 것을 보여주는 강력한 디자인 패턴입니다.

### **5.3. 구현 심층 분석 및 코드 샘플**

#### **입력 가로채기**

Input.ButtonPressed 이벤트를 사용하여 동작을 감지합니다. 핵심은 활성화된 아이템이 도끼인지 확인한 다음, 해당 입력이 게임의 다른 부분에서 처리되지 않도록 억제하는 것입니다.

C\#

// ModEntry.cs 내부  
private void OnButtonPressed(object sender, ButtonPressedEventArgs e)  
{  
    // 게임 월드가 준비되지 않았거나, 액션 버튼이 아니거나, 플레이어가 자유롭지 않으면 반환  
    if (\!Context.IsWorldReady ||\!e.Button.IsActionButton() ||\!Game1.player.CanMove) return;

    // 현재 도구가 도끼인지 확인  
    if (Game1.player.CurrentTool is Axe axe)  
    {  
        // 다른 동작을 방지하기 위해 입력을 억제  
        this.Helper.Input.Suppress(e.Button);

        // 도끼를 던지는 메서드 호출  
        this.ThrowAxe(axe);  
    }  
}

#### **발사체 생성**

ThrowAxe 메서드는 게임의 Projectile 클래스를 인스턴스화하고, 속성을 구성한 다음, 현재 위치의 발사체 목록에 추가합니다.

C\#

private void ThrowAxe(Axe axe)  
{  
    GameLocation location \= Game1.player.currentLocation;

    // 커스텀 타일시트에서 도끼 발사체 스프라이트의 인덱스 (예: 15\)  
    // 발사체는 위치, 속도, 데미지 등 다양한 속성을 가짐  
    int projectileSheetIndex \= 15;  
    int damage \= GetDamageFromAxe(axe); // 도끼 등급에 따라 데미지 계산  
    int facingDirection \= Game1.player.FacingDirection;  
    Vector2 velocity \= GetVelocityForDirection(facingDirection, 16f); // 방향과 속력

    // Projectile 생성자 호출  
    Projectile axeProjectile \= new Projectile(  
        projectileSheetIndex, // parentSheetIndex  
        damage,  
        0, // knockback  
        0, // bounces  
        0, // rotationVelocity  
        Game1.player.GetBoundingBox().Center.ToVector2(), // 시작 위치  
        velocity, // 초기 속도  
        location,  
        Game1.player.Name,  
        true, // is-weapon  
        null, // collision behavior  
        Game1.player  
    );

    // 모드의 커스텀 데이터를 발사체에 첨부하여 부메랑 로직을 추적  
    axeProjectile.modData \= "false";  
    axeProjectile.modData \= "30"; // 30틱 동안 직진

    location.projectiles.Add(axeProjectile);  
}

#### **부메랑 로직 관리**

부메랑 효과는 커스텀 로직이 필요합니다. modData를 사용하여 발사체에 데이터를 첨부하고, GameLoop.UpdateTicked 이벤트에서 그 궤도를 업데이트하는 것이 간단한 방법입니다.

C\#

// GameLoop.UpdateTicked 이벤트 핸들러 내부  
private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)  
{  
    if (\!Context.IsWorldReady) return;

    // 현재 위치의 모든 발사체를 순회  
    foreach (var p in Game1.currentLocation.projectiles.ToList()) // ToList()로 복사본 순회  
    {  
        // 이 모드에 의해 생성된 발사체인지 확인  
        if (p.owner.Value is Farmer owner && owner.UniqueMultiplayerID \== Game1.player.UniqueMultiplayerID  
            && p.modData.ContainsKey("spacechase0.ThrowableAxe/TravelTicks"))  
        {  
            bool isReturning \= bool.Parse(p.modData);  
            if (isReturning)  
            {  
                // 플레이어에게 돌아오는 로직  
                Vector2 directionToPlayer \= Game1.player.GetBoundingBox().Center.ToVector2() \- p.position;  
                directionToPlayer.Normalize();  
                p.xVelocity.Value \= directionToPlayer.X \* 16f;  
                p.yVelocity.Value \= directionToPlayer.Y \* 16f;

                // 플레이어와 충분히 가까워지면 발사체 제거  
                if (Vector2.Distance(p.position, Game1.player.GetBoundingBox().Center.ToVector2()) \< 32f)  
                {  
                    p.kill.Value \= true;  
                }  
            }  
            else  
            {  
                // 직진하는 로직  
                int ticksLeft \= int.Parse(p.modData);  
                ticksLeft--;  
                p.modData \= ticksLeft.ToString();  
                if (ticksLeft \<= 0)  
                {  
                    p.modData \= "true";  
                }  
            }  
        }  
    }  
}

## **결론: 교훈 통합 및 스타듀밸리 1.6+ 적응**

### **핵심 패턴 요약**

본 보고서에서 분석한 모드들은 스타듀밸리 모딩의 다섯 가지 핵심 패턴을 보여줍니다.

1. **API 기반 시스템 확장:** SpaceCore와 같은 프레임워크를 사용하여 주요 신규 진행 시스템을 추가합니다.  
2. **비침습적 UI 렌더링:** SMAPI의 렌더링 이벤트를 사용하여 게임 데이터를 안전하게 표시합니다.  
3. **물리 및 상태 시뮬레이션:** 플레이어 상태와 시각 효과를 관리하여 Z축과 같은 엔진 기능을 창의적으로 구현합니다.  
4. **정밀한 런타임 패치:** API가 없는 곳에 로직을 추가하거나 핵심 게임 버그를 수정하기 위해 Harmony를 방어적으로 사용합니다.  
5. **행동 확장:** 입력을 가로채 바닐라 아이템에 완전히 새로운 동적 행동을 부여합니다.

### **앞으로의 길: 1.6+ 환경으로의 현대화**

이 가이드를 오늘날의 모더들에게 실용적으로 만들기 위해, 레거시 기술을 현대화하는 방법을 논의하는 것은 매우 중요합니다.

* **.NET 6 및 C\# 10:** .NET 5에서 .NET 6로의 마이그레이션을 통해 얻을 수 있는 성능상의 이점과 모더가 사용할 수 있는 새로운 언어 기능들을 강조해야 합니다.12  
* **문자열 ID:** 정수 기반 ID에서 네임스페이스를 포함하는 문자열 ID(예: YourModID\_ItemName)로의 전환을 강조해야 합니다. 이는 초기 모딩 시대를 괴롭혔던 ID 충돌을 방지하여 호환성을 대폭 향상시키는 중요한 개선 사항입니다.12  
* **Harmony 모범 사례:** 커뮤니티가 Harmony 어노테이션 방식에서 수동 패치 방식으로 전환하고 있음을 재차 강조해야 합니다. 수동 패치는 게임 업데이트에 대한 더 큰 호환성과 복원력을 제공하기 때문입니다.33  
* **단종 요인:** 마지막으로 Custom NPC Fixes의 생애주기를 되돌아봅니다. "수정" 모드의 궁극적인 목표는 불필요해지는 것임을 개발자들에게 조언해야 합니다. 이는 게임 개발자와의 협력적인 관계를 장려하고, 기본 게임이 진화함에 따라 자신의 작업이 은퇴하거나 대대적으로 리팩토링되어야 할 수 있다는 현실에 모더들이 대비하도록 합니다. 이러한 이해는 모딩 커뮤니티가 게임 생태계의 건강한 일부로서 지속적으로 기여할 수 있는 기반이 됩니다.

#### **참고 자료**

1. spacechase0/StardewValleyMods: New home for my stardew valley mod source code, 9월 16, 2025에 액세스, [https://github.com/spacechase0/StardewValleyMods](https://github.com/spacechase0/StardewValleyMods)  
2. 1월 1, 1970에 액세스, [https://github.com/spacechase0/StardewValleyMods/tree/1970d6fb6b969304c9e47216c594e0ed1a62cd9d/\_archived/CookingSkill](https://github.com/spacechase0/StardewValleyMods/tree/1970d6fb6b969304c9e47216c594e0ed1a62cd9d/_archived/CookingSkill)  
3. 1월 1, 1970에 액세스, [https://github.com/spacechase0/StardewValleyMods/tree/1970d6fb6b969304c9e47216c594e0ed1a62cd9d/\_archived/ExperienceBars](https://github.com/spacechase0/StardewValleyMods/tree/1970d6fb6b969304c9e47216c594e0ed1a62cd9d/_archived/ExperienceBars)  
4. 1월 1, 1970에 액세스, [https://github.com/spacechase0/StardewValleyMods/tree/1970d6fb6b969304c9e47216c594e0ed1a62cd9d/\_archived/JumpOver](https://github.com/spacechase0/StardewValleyMods/tree/1970d6fb6b969304c9e47216c594e0ed1a62cd9d/_archived/JumpOver)  
5. 1월 1, 1970에 액세스, [https://github.com/spacechase0/StardewValleyMods/tree/1970d6fb6b969304c9e47216c594e0ed1a62cd9d/\_archived/CustomNPCFixes](https://github.com/spacechase0/StardewValleyMods/tree/1970d6fb6b969304c9e47216c594e0ed1a62cd9d/_archived/CustomNPCFixes)  
6. 1월 1, 1970에 액세스, [https://github.com/spacechase0/StardewValleyMods/tree/1970d6fb6b969304c9e47216c594e0ed1a62cd9d/\_archived/ThrowableAxe](https://github.com/spacechase0/StardewValleyMods/tree/1970d6fb6b969304c9e47216c594e0ed1a62cd9d/_archived/ThrowableAxe)  
7. Stardew valley source code : r/StardewValley \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/StardewValley/comments/1bs32fc/stardew\_valley\_source\_code/](https://www.reddit.com/r/StardewValley/comments/1bs32fc/stardew_valley_source_code/)  
8. Modding:Modder Guide/Get Started \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/Get\_Started](https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started)  
9. Annosz/StardewValleyModdingGuide: Stardew Valley modding guide for all fellow farmers out there. \- GitHub, 9월 16, 2025에 액세스, [https://github.com/Annosz/StardewValleyModdingGuide](https://github.com/Annosz/StardewValleyModdingGuide)  
10. Modding:Modder Guide/APIs/Harmony \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Modder\_Guide/APIs/Harmony](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Harmony)  
11. Tutorial: Harmony Patching \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/Tutorial:\_Harmony\_Patching](https://stardewmodding.wiki.gg/wiki/Tutorial:_Harmony_Patching)  
12. Modding:Migrate to Stardew Valley 1.6, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Migrate\_to\_Stardew\_Valley\_1.6](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6)  
13. Cooking Skill \- Stardew Valley Mods \- CurseForge, 9월 16, 2025에 액세스, [https://www.curseforge.com/stardewvalley/mods/cooking-skill](https://www.curseforge.com/stardewvalley/mods/cooking-skill)  
14. Luck skill and cooking skill mods : r/StardewValley \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/StardewValley/comments/4yxo1l/luck\_skill\_and\_cooking\_skill\_mods/](https://www.reddit.com/r/StardewValley/comments/4yxo1l/luck_skill_and_cooking_skill_mods/)  
15. SpaceCore crashed on entry \- Smapi error \- Stardew Valley Forums, 9월 16, 2025에 액세스, [https://forums.stardewvalley.net/threads/spacecore-crashed-on-entry-smapi-error.37447/](https://forums.stardewvalley.net/threads/spacecore-crashed-on-entry-smapi-error.37447/)  
16. SpaceCore | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/136998-spacecore](https://www.moddrop.com/stardew-valley/mods/136998-spacecore)  
17. SPACECORE FIXED •|• STARDEW VALLEY MOBILE \- YouTube, 9월 16, 2025에 액세스, [https://www.youtube.com/watch?v=JvzkSi1acAs](https://www.youtube.com/watch?v=JvzkSi1acAs)  
18. Cooking Mods Recommendation \- Stardew Modding Wiki, 9월 16, 2025에 액세스, [https://stardewmodding.wiki.gg/wiki/Cooking\_Mods\_Recommendation](https://stardewmodding.wiki.gg/wiki/Cooking_Mods_Recommendation)  
19. b-b-blueberry/CooksAssistant: Stardew Valley content ... \- GitHub, 9월 16, 2025에 액세스, [https://github.com/b-b-blueberry/CooksAssistant](https://github.com/b-b-blueberry/CooksAssistant)  
20. Experience Bars | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/127275-experience-bars](https://www.moddrop.com/stardew-valley/mods/127275-experience-bars)  
21. What mod adds this experience bar at the bottom left of the game? : r/StardewValleyMods, 9월 16, 2025에 액세스, [https://www.reddit.com/r/StardewValleyMods/comments/1i6651w/what\_mod\_adds\_this\_experience\_bar\_at\_the\_bottom/](https://www.reddit.com/r/StardewValleyMods/comments/1i6651w/what_mod_adds_this_experience_bar_at_the_bottom/)  
22. Mods To Try To Improve Stardew Valley's Gamer Experience \- Boss Rush Network, 9월 16, 2025에 액세스, [https://bossrush.net/2024/05/25/mods-to-try-to-improve-stardew-valleys-gamer-experience/](https://bossrush.net/2024/05/25/mods-to-try-to-improve-stardew-valleys-gamer-experience/)  
23. Jump Over | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/384782-jump-over](https://www.moddrop.com/stardew-valley/mods/384782-jump-over)  
24. Custom NPC Fixes | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mods/771669-custom-npc-fixes](https://www.moddrop.com/stardew-valley/mods/771669-custom-npc-fixes)  
25. Custom NPC Fixes \- spacechase0's Site, 9월 16, 2025에 액세스, [https://spacechase0.com/mods/stardew-valley/custom-npc-fixes/](https://spacechase0.com/mods/stardew-valley/custom-npc-fixes/)  
26. I've got a couple mod errors, can someone please help me fix them? :: Stardew Valley Allgemeine Diskussionen \- Steam Community, 9월 16, 2025에 액세스, [https://steamcommunity.com/app/413150/discussions/0/3198115500352522196/?l=german](https://steamcommunity.com/app/413150/discussions/0/3198115500352522196/?l=german)  
27. Requires smapi, uses harmony. : r/StardewValleyMods \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/StardewValleyMods/comments/1eprtf6/requires\_smapi\_uses\_harmony/](https://www.reddit.com/r/StardewValleyMods/comments/1eprtf6/requires_smapi_uses_harmony/)  
28. Custom NPC error Day started loop · Issue \#58 · spacechase0 ..., 9월 16, 2025에 액세스, [https://github.com/spacechase0/StardewValleyMods/issues/58](https://github.com/spacechase0/StardewValleyMods/issues/58)  
29. Unofficial mod updates | Page 234 \- Stardew Valley Forums, 9월 16, 2025에 액세스, [https://forums.stardewvalley.net/threads/unofficial-mod-updates.2096/page-234](https://forums.stardewvalley.net/threads/unofficial-mod-updates.2096/page-234)  
30. Custom NPC Fixes dead? : r/StardewValleyMods \- Reddit, 9월 16, 2025에 액세스, [https://www.reddit.com/r/StardewValleyMods/comments/1bk403k/custom\_npc\_fixes\_dead/](https://www.reddit.com/r/StardewValleyMods/comments/1bk403k/custom_npc_fixes_dead/)  
31. Throwable Axe | Stardew Valley Mods \- ModDrop, 9월 16, 2025에 액세스, [https://www.moddrop.com/stardew-valley/mod/771683](https://www.moddrop.com/stardew-valley/mod/771683)  
32. Modding:Weapons \- Stardew Valley Wiki, 9월 16, 2025에 액세스, [https://stardewvalleywiki.com/Modding:Weapons](https://stardewvalleywiki.com/Modding:Weapons)  
33. Unofficial mod updates | Page 140 \- Stardew Valley Forums, 9월 16, 2025에 액세스, [https://forums.stardewvalley.net/threads/unofficial-mod-updates.2096/page-140](https://forums.stardewvalley.net/threads/unofficial-mod-updates.2096/page-140)  
34. \[SUGGEST\] Use Harmony for method hijacking · Issue \#2 · spacechase0/SpaceCore\_SDV, 9월 16, 2025에 액세스, [https://github.com/spacechase0/SpaceCore\_SDV/issues/2](https://github.com/spacechase0/SpaceCore_SDV/issues/2)