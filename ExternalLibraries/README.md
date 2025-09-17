# 📚 External Libraries

이 폴더의 모든 라이브러리들은 `setup_environment.sh` 스크립트를 통해 자동으로 다운로드됩니다.

## 🔧 포함된 프레임워크들

### SMAPI/ - Stardew Modding API v4.3.2
- **소스**: https://github.com/Pathoschild/SMAPI
- **용도**: 모든 모드의 기반이 되는 API

### StardewUI/ - UI 프레임워크 v0.6.1  
- **소스**: https://github.com/focustense/StardewUI
- **용도**: 현대적인 UI 구현을 위한 프레임워크

### SpacechaseFrameworks/ - spacechase0 프레임워크들
- **소스**: https://github.com/spacechase0/StardewValleyMods
- **용도**: 다양한 확장 프레임워크들 (SpaceCore, JsonAssets 등)

### PathoschildMods/ - Pathoschild 참고 모드들
- **소스**: https://github.com/Pathoschild/StardewMods  
- **용도**: 검증된 모딩 패턴 및 아키텍처 학습

## 🔄 재설치 방법

```bash
# 모든 라이브러리 재다운로드
rm -rf ExternalLibraries/SMAPI ExternalLibraries/StardewUI ExternalLibraries/SpacechaseFrameworks ExternalLibraries/PathoschildMods
./setup_environment.sh
```
