# 🚀 Stardew Valley 모드 개발 환경 자동 설정 가이드

**용량 최적화**: 외부 리포지터리들은 Git에서 제외하고 필요시마다 자동 다운로드합니다.

## ⚡ 빠른 설정 (권장)

### **한 번에 모든 환경 설정**
```bash
# 저장소 클론
git clone <your-repository-url>
cd stardew-mod-dev

# 자동 환경 설정 실행
./setup_environment.sh
```

**완료!** 모든 필요한 프레임워크와 예제 모드들이 자동으로 다운로드됩니다.

## 📋 자동으로 다운로드되는 구성 요소

### 🎯 핵심 프레임워크
- **SMAPI v4.3.2**: Stardew Valley 모딩 API
- **StardewUI v0.6.1**: 현대적인 UI 프레임워크  
- **SpaceCore v1.28.0**: 커스텀 스킬 및 확장 프레임워크

### 🏆 참고 모드들  
- **Pathoschild 모드들**: 검증된 모딩 패턴 및 아키텍처
- **MatrixFishingUI**: 실제 UI 구현 예제
- **Ferngill-Simple-Economy**: 복잡한 시스템 구현 예제
- **spacechase0 모드들**: 다양한 게임플레이 모드 예제

## 🔧 수동 설정 (고급 사용자)

### 1. 필수 소프트웨어 설치

```bash
# .NET 6.0 SDK
# Windows
winget install Microsoft.DotNet.SDK.6

# macOS
brew install --cask dotnet-sdk

# Linux
sudo apt-get install -y dotnet-sdk-6.0
```

### 2. 개발 도구 설치

```bash
# Visual Studio Code (권장)
# Windows
winget install Microsoft.VisualStudioCode

# macOS  
brew install --cask visual-studio-code

# Linux
sudo snap install --classic code
```

### 3. Stardew Valley + SMAPI 설치

1. **Stardew Valley 1.6.14+** (Steam/GOG)
2. **SMAPI 4.3.2+** ([smapi.io](https://smapi.io))

### 4. 수동으로 리포지터리 다운로드

```bash
# 핵심 프레임워크들
git clone https://github.com/Pathoschild/SMAPI.git ExternalLibraries/SMAPI
git clone https://github.com/focustense/StardewUI.git ExternalLibraries/StardewUI
git clone https://github.com/spacechase0/StardewValleyMods.git temp_spacechase

# spacechase0 모드들 분류
mkdir -p ExternalLibraries/SpacechaseFrameworks
cp -r temp_spacechase/framework/* ExternalLibraries/SpacechaseFrameworks/
# ... (자세한 분류는 setup_environment.sh 참고)

# 참고 모드들
git clone https://github.com/Pathoschild/StardewMods.git ExternalLibraries/PathoschildMods
git clone https://github.com/LetsTussleBoiz/MatrixFishingUI.git ExampleMods/MatrixFishingUI
git clone https://github.com/paulsteele/Ferngill-Simple-Economy.git ExampleMods/Ferngill-Simple-Economy

# .git 폴더 제거 (용량 절약)
find ExternalLibraries ExampleMods -name ".git" -type d -exec rm -rf {} +
```

## 🎯 프로젝트 구조

설정 완료 후 다음과 같은 구조가 생성됩니다:

```
stardew-mod-dev/
├── 📁 ExternalLibraries/           # 외부 프레임워크들 (Git 제외)
│   ├── SMAPI/                      # 모딩 API
│   ├── StardewUI/                  # UI 프레임워크
│   ├── SpacechaseFrameworks/       # spacechase0 프레임워크들
│   └── PathoschildMods/            # Pathoschild 참고 모드들
├── 📁 ExampleMods/                 # 예제 모드들 (Git 제외)
│   ├── MatrixFishingUI/            # UI 구현 예제
│   ├── Ferngill-Simple-Economy/   # 시스템 구현 예제
│   └── SpacechaseMods/             # spacechase0 예제들
├── 📁 Stardew/                     # 우리의 개발 모드들
│   ├── DrawingSkill/               # 그림 스킬 모드
│   ├── FarmStatistics/             # 농장 통계 모드 ⭐
│   └── SimpleUI/                   # UI 예제
├── 📁 Documentation/               # 개발 문서들
├── 🚀 setup_environment.sh         # 자동 설정 스크립트
├── 📋 SETUP_GUIDE.md              # 이 파일
└── 📖 README.md                   # 프로젝트 개요
```

## 🔄 환경 재설정

### 전체 재설정
```bash
# 모든 외부 리포지터리 제거 후 재다운로드
rm -rf ExternalLibraries ExampleMods
./setup_environment.sh
```

### 특정 구성 요소만 재설정
```bash
# SMAPI만 재다운로드
rm -rf ExternalLibraries/SMAPI
./setup_environment.sh

# 예제 모드들만 재다운로드  
rm -rf ExampleMods
./setup_environment.sh
```

## 🐛 문제 해결

### Git 클론 실패
```bash
# 네트워크 문제 시 재시도
./setup_environment.sh

# 특정 리포지터리 수동 다운로드
git clone --depth 1 <repository-url> <target-directory>
```

### 권한 문제
```bash
# 스크립트 실행 권한 부여
chmod +x setup_environment.sh

# 디렉토리 권한 확인
ls -la setup_environment.sh
```

### 용량 문제
```bash
# .git 폴더들이 제거되었는지 확인
find . -name ".git" -type d

# 수동으로 .git 폴더 제거
find ExternalLibraries ExampleMods -name ".git" -type d -exec rm -rf {} +
```

## 💡 사용법

### 개발 시작
```bash
# 1. 환경 설정 완료 확인
ls ExternalLibraries/SMAPI
ls ExampleMods/MatrixFishingUI

# 2. 모드 개발 시작
cd Stardew/FarmStatistics
# 개발 작업...

# 3. 빌드 및 테스트
dotnet build
```

### 참고 자료 활용
```bash
# Pathoschild 패턴 학습
cd ExternalLibraries/PathoschildMods/LookupAnything

# UI 구현 예제 학습  
cd ExampleMods/MatrixFishingUI

# 프레임워크 소스 확인
cd ExternalLibraries/StardewUI/Framework
```

## 🎉 완료!

이제 모든 필요한 도구와 예제들이 준비되었습니다. 

**다음 단계**: [README.md](./README.md)에서 프로젝트 개요를 확인하고 개발을 시작하세요!