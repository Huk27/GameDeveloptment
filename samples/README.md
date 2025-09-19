# 🎮 샘플 모드 모음 (`samples/`)

이 폴더는 학습 및 리버스 엔지니어링용으로 보관하는 커뮤니티 모드 스냅샷과 개인 실험 코드를 담습니다. 대부분은 `setup_environment.sh` 또는 수동 `git clone --depth 1`로 내려받을 수 있으며, 필요 없을 때는 자유롭게 삭제해도 됩니다.

## 📁 현재 포함된 항목

### SpacechaseMods/
- `UI-Examples/ExperienceBars` - HUD 이벤트 패턴, StarML 레이아웃 참고
- `Gameplay-Modifications/` - `NewGamePlus`, `JumpOver`, `Satchels`
- `Visual-Enhancements/` - `GradientHairColors`, `SizableFish`
- `Crafting-Systems/` - `Potioncraft`

### CommunityMods/
- `CJBok-Collection/` - `CJBCheatsMenu`, `CJBItemSpawner`, `CJBShowItemSellPrice`, `Common`
- `Atravita-Collection/` - `AtraCore`, `MoreFertilizers`, `GrowableGiantCrops`, `CritterRings`

### PersonalExperiments/
- `SimpleUIPrototype` - StardewUI 데이터 바인딩, 탭 레이아웃 실험 코드

### 자리 표시자 (비어 있음)
- `MatrixFishingUI/`
- `Ferngill-Simple-Economy/`
> 필요 시 원본 저장소에서 `git clone --depth 1` 후 `.git` 폴더를 제거하세요.

## 🔄 재설치
```bash
# 지정한 샘플만 다시 받고 싶을 때
git clone https://github.com/LetsTussleBoiz/MatrixFishingUI.git samples/MatrixFishingUI --depth 1
rm -rf samples/MatrixFishingUI/.git

# 전체 샘플 초기화
rm -rf samples
./setup_environment.sh   # 스크립트 경로가 최신 구조로 업데이트되었는지 확인
```

샘플을 추가/삭제했다면 관련 문서(`docs/`)와 경로 레퍼런스도 함께 갱신해주세요.
