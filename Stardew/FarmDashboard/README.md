# Farm Dashboard 모드

FarmStatistics 기획서를 바탕으로 농장 운영 데이터를 실시간으로 모니터링할 수 있는 새 모드입니다.

## 핵심 기능
- HUD 카드: 오늘 수익, 작물 현황, 동물 현황, 플레이 시간 요약
- 활동 추적: 농사/채광/낚시 등 활동별 시간을 자동 집계
- 목표 트래커: 일일 수익·동물 행복·작물 수확 준비 상태를 실시간 확인
- 전용 메뉴: `F3`로 대시보드 메뉴를 열어 상세 통계 확인

## 참고한 예제 코드
- `ExampleMods/SpacechaseMods/UI-Examples/ExperienceBars/Mod.cs`: HUD 렌더링 이벤트 패턴 참고
- `ExampleMods/MatrixFishingUI/MatrixFishingUI/ModEntry.cs`: 메뉴 토글, 이벤트 구독 구조 확인

## 사용법
1. 모드를 빌드하여 `FarmDashboard.dll`과 `manifest.json`을 Stardew Valley `Mods/FarmDashboard` 폴더에 배치합니다.
2. 게임 내에서 `F2`로 HUD를 토글하고 `F3`으로 상세 대시보드 메뉴를 엽니다.
3. `config.json`을 통해 HUD 위치 및 단축키를 조정할 수 있습니다.

## 구현 메모
- 작물/동물 데이터는 매 10분 게임 시간과 30틱마다 갱신합니다.
- 계절별 수익은 `Game1.player.modData`에 저장하여 계절이 바뀔 때 초기화합니다.
- 활동 시간은 `GameLoop.UpdateTicked` 이벤트에서 `Game1.player.millisecondsPlayed` 변화를 이용해 누적합니다.

## 향후 확장 아이디어
- StardewUI 기반 탭형 대시보드로 전환
- 목표 시스템을 커스텀 목표/보상 구조로 확장
- CSV/JSON 내보내기 기능 추가
