# 📚 libraries/

모드 개발에 참고하는 외부 프레임워크와 모드 소스 스냅샷을 보관합니다. 모두 `git clone --depth 1` 후 `.git` 폴더를 삭제한 버전이며, 필요 시 제거하고 다시 내려받을 수 있습니다.

## 포함된 주요 항목

### SMAPI/
- **출처**: https://github.com/Pathoschild/SMAPI
- **용도**: Stardew Valley 모드 API 및 문서 확인

### StardewUI/
- **출처**: https://github.com/focustense/StardewUI
- **용도**: StarML, ViewModel 패턴, TestMod 예제 분석

### SpaceCore/ & SpacechaseFrameworks/
- **출처**: https://github.com/spacechase0/StardewValleyMods
- **용도**: 커스텀 스킬, JsonAssets, Generic Mod Config Menu 등 확장 프레임워크

### PathoschildMods/
- **출처**: https://github.com/Pathoschild/StardewMods
- **용도**: Automate, LookupAnything 등 고품질 모드 패턴 분석

### StardewValleyMods/
- **출처**: spacechase0/StardewValleyMods 전체 복제본
- **용도**: 아직 분류하지 않은 spacechase0 모드 소스 (필요 시 이곳에서 추가 복사)

## 재설치 예시
```bash
rm -rf libraries/SMAPI
git clone https://github.com/Pathoschild/SMAPI.git libraries/SMAPI --depth 1
rm -rf libraries/SMAPI/.git
```

새로운 라이브러리를 추가하거나 버전을 갱신했다면, 관련 문서와 `setup_environment.sh` 스크립트도 함께 업데이트해주세요.
