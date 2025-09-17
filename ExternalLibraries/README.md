# External Libraries and Examples

이 폴더는 Stardew Valley 모드 개발을 위한 외부 라이브러리와 예제 모드들을 포함합니다.

## 📁 구조

### 🎯 핵심 프레임워크 (소스코드)

#### SMAPI/ 
**SMAPI v4.3.2** - Stardew Valley 모딩 API 프레임워크
- **소스**: [Pathoschild/SMAPI](https://github.com/Pathoschild/SMAPI)
- **호환성**: Stardew Valley 1.6.14+ 지원
- **용도**: 모드 로딩, API 제공, 호환성 관리
- **핵심 파일**: `src/SMAPI/`, `docs/`

#### StardewUI/
**StardewUI v0.6.1** - 선언적 UI 프레임워크  
- **소스**: [focustense/StardewUI](https://github.com/focustense/StardewUI)
- **호환성**: .NET 6.0, SMAPI 최신 버전 지원
- **용도**: StarML 기반 UI 시스템, 데이터 바인딩
- **핵심 파일**: `Framework/`, `TestMod/`, `docs/`

#### SpacechaseFrameworks/
**spacechase0 프레임워크 컬렉션** - 다양한 확장 프레임워크들
- **소스**: [spacechase0/StardewValleyMods](https://github.com/spacechase0/StardewValleyMods) 
- **호환성**: .NET 6.0, SMAPI 4.0+
- **포함 프레임워크들**:
  - **SpaceCore v1.28.0**: 커스텀 스킬 및 확장 시스템
  - **JsonAssets v1.11.10**: 커스텀 콘텐츠 프레임워크
  - **GenericModConfigMenu v1.15.0**: 모드 설정 UI
  - **ConsoleCode**: 콘솔 명령어 시스템
  - **HybridCropEngine**: 하이브리드 작물 시스템
  - **YarnEvents**: 이벤트 시스템 프레임워크
- **상세 정보**: [SpacechaseFrameworks/README.md](./SpacechaseFrameworks/README.md)

### 🏆 참고 모드들 (Pathoschild)

#### PathoschildMods/
Pathoschild의 선별된 모드들 (Stardew 1.6+ / SMAPI 4.0+ 호환)
- **LookupAnything**: 데이터 분석 패턴 학습용
- **Automate**: 자동화 시스템 및 성능 최적화 패턴
- **ChestsAnywhere**: 복잡한 UI 시스템 패턴  
- **Common**: 공통 라이브러리 패턴
- **TractorMod**: 멀티플레이어 지원 패턴
- **ContentPatcher**: 설정 시스템 패턴

### 📦 기타 라이브러리

#### StardewValleyMods/
기타 참고용 모드들 (향후 추가 예정)

## 🔧 개발 환경 요구사항

### 필수 소프트웨어
- **.NET 6.0 SDK** 이상
- **Visual Studio 2022** 또는 **VS Code**
- **Stardew Valley 1.6.14+**
- **SMAPI 4.3.2+**

### 호환성 매트릭스
| 프레임워크 | 버전 | Stardew Valley | SMAPI | .NET |
|-----------|------|----------------|-------|------|
| SMAPI | 4.3.2 | 1.6.14+ | - | 6.0+ |
| StardewUI | 0.6.1 | 1.6+ | 4.0+ | 6.0 |
| SpaceCore | 1.28.0 | 1.6+ | 4.0+ | 6.0 |
| JsonAssets | 1.11.10 | 1.6+ | 4.0+ | 6.0 |
| GenericModConfigMenu | 1.15.0 | 1.6+ | 4.0+ | 6.0 |

## 🚀 재구축 가이드

다른 환경에서 동일한 개발 환경을 구축하려면:

```bash
# 1. 기본 저장소 클론
git clone <your-repo-url>
cd stardew

# 2. 핵심 프레임워크 다운로드
cd ExternalLibraries
git clone https://github.com/Pathoschild/SMAPI.git
git clone https://github.com/focustense/StardewUI.git  
git clone https://github.com/spacechase0/StardewValleyMods.git SpaceCore

# 3. Git 중첩 문제 방지
rm -rf ./SMAPI/.git ./StardewUI/.git ./SpaceCore/.git

# 4. 프로젝트에 추가
git add .
git commit -m "Add core frameworks source code"
```

## ⚠️ 주의사항
- 이 폴더의 코드들은 **참고 및 학습용**입니다
- 실제 사용 시 각 프로젝트의 **라이선스를 확인**하세요
- **최신 호환성을 확인한 코드들만** 포함되어 있습니다
- 중첩 Git 저장소 문제를 방지하기 위해 `.git` 폴더는 제거되었습니다

## 📚 학습 자료
- **SMAPI 문서**: [smapi.io](https://smapi.io)
- **StardewUI 문서**: Framework 폴더 내 README 참조
- **SpaceCore 가이드**: framework/SpaceCore/docs/ 참조
