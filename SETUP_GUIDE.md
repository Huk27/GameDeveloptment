# 🚀 Stardew Valley 모드 개발 환경 설정 가이드

외부 리포지터리는 Git 히스토리에서 제외하고 필요할 때마다 내려받는 방식을 사용합니다. `libraries/`와 `samples/`는 언제든지 삭제 후 재생성할 수 있는 캐시 디렉터리입니다.

## ⚡ 빠른 설정
```bash
# 저장소 클론
git clone <your-repository-url> stardew
cd stardew

# 자동 다운로드 (선택)
./setup_environment.sh
```
> **참고**: 스크립트는 아직 구 경로(`ExternalLibraries/`, `ExampleMods/`)를 사용합니다. 실행 후 생성된 폴더를 `libraries/`, `samples/`로 옮기거나 스크립트를 업데이트하세요.

## 📋 자동/수동으로 내려받는 구성 요소

### 핵심 프레임워크 (`libraries/`)
- `SMAPI/` - Stardew Valley 모딩 API (v4.3.2 기준)
- `StardewUI/` - 현대적인 UI 프레임워크 (v0.6.1)
- `SpaceCore/`, `SpacechaseFrameworks/` - spacechase0 확장 프레임워크 모음
- `PathoschildMods/` - Pathoschild 모드 소스 스냅샷

### 학습용 샘플 (`samples/`)
- `SpacechaseMods/` - UI/게임플레이/시각 효과 등 7개 모드 추출본
- `CommunityMods/` - CJBok, Atravita 컬렉션 8개 모드
- `MatrixFishingUI/`, `Ferngill-Simple-Economy/` - 현재 비어 있는 자리 표시자 (필요 시 다운로드)
- `PersonalExperiments/` - 내부 실험용 샘플 (SimpleUIPrototype 등)

## 🔧 수동 설치 절차

### 1. 필수 소프트웨어
```bash
# .NET 6.0 SDK
# Windows
winget install Microsoft.DotNet.SDK.6
# macOS
brew install --cask dotnet-sdk
# Linux
sudo apt-get install -y dotnet-sdk-6.0
```

### 2. 개발 편의 도구 (선택)
```bash
# Visual Studio Code
# Windows
winget install Microsoft.VisualStudioCode
# macOS
brew install --cask visual-studio-code
# Linux
sudo snap install --classic code
```

### 3. Stardew Valley + SMAPI
1. Stardew Valley 1.6.14 이상 설치 (Steam/GOG)
2. [smapi.io](https://smapi.io)에서 SMAPI 4.3 이상 설치

### 4. 참조 리포지터리 수동 다운로드 예시
```bash
# libraries
git clone https://github.com/Pathoschild/SMAPI.git libraries/SMAPI --depth 1
rm -rf libraries/SMAPI/.git

git clone https://github.com/focustense/StardewUI.git libraries/StardewUI --depth 1
rm -rf libraries/StardewUI/.git

# spacechase0 프레임워크와 모드 분류
git clone https://github.com/spacechase0/StardewValleyMods.git temp_spacechase --depth 1
mkdir -p libraries/SpacechaseFrameworks
cp -r temp_spacechase/framework/SpaceCore libraries/SpaceCore
cp -r temp_spacechase/framework/JsonAssets libraries/SpacechaseFrameworks/JsonAssets
cp -r temp_spacechase/framework/GenericModConfigMenu libraries/SpacechaseFrameworks/GenericModConfigMenu
cp -r temp_spacechase/cosmetic/ExperienceBars samples/SpacechaseMods/UI-Examples/
# ... (나머지 세부 분류는 필요에 따라 진행)
rm -rf temp_spacechase

# Pathoschild 모드 아카이브
git clone https://github.com/Pathoschild/StardewMods.git libraries/PathoschildMods --depth 1
rm -rf libraries/PathoschildMods/.git

# MatrixFishingUI, Ferngill 모드는 필요 시 다운로드
```

## 📁 목표 구조 (예시)
```
stardew/
├── libraries/
│   ├── SMAPI/
│   ├── StardewUI/
│   ├── SpaceCore/
│   ├── SpacechaseFrameworks/
│   └── PathoschildMods/
├── samples/
│   ├── SpacechaseMods/
│   ├── CommunityMods/
│   ├── MatrixFishingUI/
│   └── Ferngill-Simple-Economy/
├── mods/in-progress/FarmDashboard/
└── docs/
```

## 🔄 재설치 / 초기화
```bash
# 전체 초기화
rm -rf libraries samples
./setup_environment.sh

# 특정 프레임워크만 초기화
rm -rf libraries/SMAPI
./setup_environment.sh

# 샘플 모드만 재설치 (수동)
rm -rf samples/SpacechaseMods
# 필요한 항목만 다시 복사 또는 git clone
```

## 🐛 문제 해결 팁

### Git 다운로드 실패
```bash
./setup_environment.sh          # 재시도
# 혹은 개별 리포지터리 수동 git clone --depth 1
```

### 권한 오류
```bash
chmod +x setup_environment.sh
ls -la setup_environment.sh
```

### 용량 관리
```bash
find libraries samples -name ".git" -type d -exec rm -rf {} +
```

## 💡 활용 흐름
```bash
# 1. 참고 자료 존재 여부 확인
ls libraries/SMAPI
ls samples/SpacechaseMods/UI-Examples

# 2. 모드 개발 착수
dotnet build mods/in-progress/FarmDashboard/FarmDashboard.csproj

# 3. 참고 코드 분석
code samples/SpacechaseMods/UI-Examples/ExperienceBars/Mod.cs
```

이제 README와 `docs/` 폴더의 가이드를 참고하여 개발을 진행하세요. 구조가 바뀌면 이 파일과 관련 문서를 함께 업데이트해 주세요.
