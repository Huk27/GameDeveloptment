# FarmDashboard UI Style Guide

> Version 2024-09-19 – complements `docs/design/FarmDashboard 상세 기획서.md`

## 1. Palette
| Context | Color | Usage |
|---------|-------|-------|
| HUD accent (safe) | `#2ECC40` | Positive status, animals happy |
| HUD accent (warning) | `#FF851B` | Needs attention (dry crops, alerts) |
| HUD accent (critical) | `#FF4136` | Critical issues, urgent alerts |
| Overview cards | `#FFD700`, `#7FDBFF`, `#39CCCC`, `#B10DC9` | Gold, seasonal totals, wallet, weather |
| Menu frame base | `#1C1C1C` overlay + StardewUI `MenuBackground` | Maintain vanilla-inspired contrast |
| Text primary | `#FFFFFF` | Headline and key values |
| Text secondary | `#B0B0B0` | Supporting labels |
| Alert text | `#F8E473` | HUD ticker and menu alerts |

## 2. Typography
- **Headline / Primary value**: `Game1.dialogueFont`, size as-is
- **Secondary / Detail**: `Game1.smallFont`
- **Menu Tab Labels**: `Game1.smallFont` with accent colors above
- Keep UI strings in English (ASCII). Localisation will use `i18n/` later.

## 3. HUD Layout
- Grid: 2 columns x 2 rows, card size = `ModConfig.CardWidth` x `CardHeight`
- Card padding: 18px left margin, 32px vertical spacing between text lines
- Accent stripe: 6px wide vertical bar at `bounds.X + 8`
- Warning state: border color `Color(255, 99, 71)` and detail text color set to warning accent
- Crop detail line appends `| Next: …` for imminent harvests; use `#7FDBFF` for highlight text
- Alert ticker: width `(CardWidth*2)+12`, background `drawTextureBox` with slight transparency, bullet prefix `•`

## 4. Menu Layout
- Frame size 880x560 (matching existing StarML)
- Tabs: icons from `Game1.mouseCursors`, keep 64px button layout, tooltip required
- Use nested view model paths (`Overview.Cards`, `Crops.Summaries`, …) for clarity
- Repeaters (`*repeat`) should bind to view models declared in `DashboardViewModel`
- Conditional display: `ShowOverviewTab`, `ShowCropsTab`, `ShowAnimalsTab`, `ShowTimeActivityTab`, `ShowGoalsTab`
- Crops summary cards stack lines: ready status (accent), growth progress (`#C0C0C0`), estimated value (`#7FDBFF`), alerts (`#FF851B`)
- Animals insight panels should surface happiness %, produce 상태, 연속 미쓰다듬기 일수를 경고 색상(`FF851B`)으로 표시
- Time & Activity 탭 하단에는 추천 카드 1~3개를 `ControlBorder` 프레임으로 감싸고, 제목(`Title`)은 흰색, 메시지는 `#C0C0C0`, 카테고리는 `#7FDBFF`로 표기

## 5. Animation & Feedback
- HUD warning cards should pulse when value changes → TODO: add lerped alpha in renderer (Phase 2)
- Menu transitions: rely on StardewUI defaults; consider `FadeIn` effect when tabs switch (optional later)
- Sound: `Game1.playSound("smallSelect")` when toggling HUD or opening menu (already in ModEntry)

## 6. Data Binding Checklist
- Ensure every collection exposed by a tab view model returns an empty list instead of null
- Boolean helpers (`HasBar`, `HasPositiveBar`, `HasRecommendation`, `HasProgress`) guard optional markup
- When adding new fields, update both the view model record and the `.sml` to stay in sync

## 7. Implementation Notes
- Do not modify library assets in `libraries/`; mirror colors via our own config or constants
- HUD renderer uses `Game1.fadeToBlackRect` for accent stripes to avoid missing texture references
- Any new HUD card should follow the `HudCardView` pattern (title, primary value, secondary value, detail, colors, warning flag)

Pending tasks (tracked in worklog):
- Finalise iconography list for each tab (requires asset selection)
- Define GMCM settings for toggling alert ticker, accent colors, and HUD columns
