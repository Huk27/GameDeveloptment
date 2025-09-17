#!/bin/bash

# 🚀 Stardew Valley 모드 개발 환경 자동 설정 스크립트
# 필요한 외부 리포지터리들을 자동으로 다운로드하고 설정합니다.

echo "🚀 Stardew Valley 모드 개발 환경 설정을 시작합니다..."

# 색상 정의
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 로그 함수들
log_info() { echo -e "${BLUE}ℹ️  $1${NC}"; }
log_success() { echo -e "${GREEN}✅ $1${NC}"; }
log_warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
log_error() { echo -e "${RED}❌ $1${NC}"; }

# Git이 설치되어 있는지 확인
if ! command -v git &> /dev/null; then
    log_error "Git이 설치되어 있지 않습니다. Git을 먼저 설치해주세요."
    exit 1
fi

# 디렉토리 생성 함수
create_directory() {
    local dir=$1
    if [ ! -d "$dir" ]; then
        mkdir -p "$dir"
        log_info "디렉토리 생성: $dir"
    else
        log_info "디렉토리 존재: $dir"
    fi
}

# Git 리포지터리 클론 함수
clone_repository() {
    local url=$1
    local target_dir=$2
    local description=$3
    
    log_info "다운로드 중: $description"
    
    if [ -d "$target_dir" ]; then
        log_warning "$target_dir 이미 존재합니다. 스킵합니다."
        return 0
    fi
    
    if git clone "$url" "$target_dir" --depth 1; then
        # .git 폴더 제거 (용량 절약)
        rm -rf "$target_dir/.git"
        log_success "$description 다운로드 완료"
    else
        log_error "$description 다운로드 실패"
        return 1
    fi
}

# 메인 설정 시작
log_info "=== 1단계: 디렉토리 구조 생성 ==="

# 필요한 디렉토리들 생성
create_directory "ExternalLibraries"
create_directory "ExampleMods"

log_info "=== 2단계: 핵심 프레임워크 다운로드 ==="

# SMAPI 다운로드
clone_repository \
    "https://github.com/Pathoschild/SMAPI.git" \
    "ExternalLibraries/SMAPI" \
    "SMAPI v4.3.2 (모딩 API)"

# StardewUI 다운로드  
clone_repository \
    "https://github.com/focustense/StardewUI.git" \
    "ExternalLibraries/StardewUI" \
    "StardewUI v0.6.1 (UI 프레임워크)"

# SpaceCore 및 spacechase0 모드들 다운로드
clone_repository \
    "https://github.com/spacechase0/StardewValleyMods.git" \
    "temp_spacechase" \
    "spacechase0 모드 컬렉션"

if [ -d "temp_spacechase" ]; then
    log_info "spacechase0 모드들을 분류하여 이동 중..."
    
    # 프레임워크들을 ExternalLibraries로 이동
    create_directory "ExternalLibraries/SpacechaseFrameworks"
    
    # 핵심 프레임워크들 복사
    if [ -d "temp_spacechase/framework" ]; then
        cp -r temp_spacechase/framework/SpaceCore ExternalLibraries/SpacechaseFrameworks/ 2>/dev/null || true
        cp -r temp_spacechase/framework/JsonAssets ExternalLibraries/SpacechaseFrameworks/ 2>/dev/null || true
        cp -r temp_spacechase/framework/GenericModConfigMenu ExternalLibraries/SpacechaseFrameworks/ 2>/dev/null || true
        cp -r temp_spacechase/framework/ConsoleCode ExternalLibraries/SpacechaseFrameworks/ 2>/dev/null || true
        cp -r temp_spacechase/framework/HybridCropEngine ExternalLibraries/SpacechaseFrameworks/ 2>/dev/null || true
        cp -r temp_spacechase/framework/YarnEvents ExternalLibraries/SpacechaseFrameworks/ 2>/dev/null || true
    fi
    
    # 예제 모드들을 ExampleMods로 이동
    create_directory "ExampleMods/SpacechaseMods/UI-Examples"
    create_directory "ExampleMods/SpacechaseMods/Visual-Enhancements"
    create_directory "ExampleMods/SpacechaseMods/Gameplay-Modifications"
    create_directory "ExampleMods/SpacechaseMods/Crafting-Systems"
    
    # 분류별로 모드들 복사
    if [ -d "temp_spacechase/cosmetic" ]; then
        cp -r temp_spacechase/cosmetic/ExperienceBars ExampleMods/SpacechaseMods/UI-Examples/ 2>/dev/null || true
        cp -r temp_spacechase/cosmetic/GradientHairColors ExampleMods/SpacechaseMods/Visual-Enhancements/ 2>/dev/null || true
        cp -r temp_spacechase/cosmetic/SizableFish ExampleMods/SpacechaseMods/Visual-Enhancements/ 2>/dev/null || true
    fi
    
    if [ -d "temp_spacechase/gameplay/ignorable" ]; then
        cp -r temp_spacechase/gameplay/ignorable/NewGamePlus ExampleMods/SpacechaseMods/Gameplay-Modifications/ 2>/dev/null || true
        cp -r temp_spacechase/gameplay/ignorable/JumpOver ExampleMods/SpacechaseMods/Gameplay-Modifications/ 2>/dev/null || true
        cp -r temp_spacechase/gameplay/ignorable/Satchels ExampleMods/SpacechaseMods/Gameplay-Modifications/ 2>/dev/null || true
    fi
    
    if [ -d "temp_spacechase/gameplay/unavoidable" ]; then
        cp -r temp_spacechase/gameplay/unavoidable/Potioncraft ExampleMods/SpacechaseMods/Crafting-Systems/ 2>/dev/null || true
    fi
    
    # 임시 폴더 제거
    rm -rf temp_spacechase
    log_success "spacechase0 모드들 분류 완료"
fi

log_info "=== 3단계: Pathoschild 모드들 다운로드 ==="

# Pathoschild 모드들 다운로드 (호환성 검증된 모드들만)
clone_repository \
    "https://github.com/Pathoschild/StardewMods.git" \
    "ExternalLibraries/PathoschildMods" \
    "Pathoschild 모드 컬렉션 (최신 호환)"

log_info "=== 4단계: 예제 모드들 다운로드 ==="

# MatrixFishingUI 다운로드
clone_repository \
    "https://github.com/LetsTussleBoiz/MatrixFishingUI.git" \
    "ExampleMods/MatrixFishingUI" \
    "MatrixFishingUI (낚시 UI 모드)"

# Ferngill-Simple-Economy 다운로드  
clone_repository \
    "https://github.com/paulsteele/Ferngill-Simple-Economy.git" \
    "ExampleMods/Ferngill-Simple-Economy" \
    "Ferngill Simple Economy (경제 시스템 모드)"

log_info "=== 5단계: README 파일들 생성 ==="

# ExternalLibraries README 업데이트
cat > ExternalLibraries/README.md << 'EOF'
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
EOF

# ExampleMods README 업데이트
cat > ExampleMods/README.md << 'EOF'
# 🎮 Example Mods

이 폴더의 모든 예제 모드들은 `setup_environment.sh` 스크립트를 통해 자동으로 다운로드됩니다.

## 📁 포함된 모드들

### MatrixFishingUI/ - 낚시 정보 UI 모드
- **소스**: https://github.com/LetsTussleBoiz/MatrixFishingUI
- **학습 포인트**: 게임 데이터 분석, UI 메뉴 구현

### Ferngill-Simple-Economy/ - 경제 시스템 모드
- **소스**: https://github.com/paulsteele/Ferngill-Simple-Economy  
- **학습 포인트**: 복잡한 시스템 구현, 멀티플레이어 지원

### SpacechaseMods/ - spacechase0 예제 모드들
- **소스**: https://github.com/spacechase0/StardewValleyMods
- **분류**: UI, 시각적 개선, 게임플레이, 제작 시스템별로 정리

## 🔄 재설치 방법

```bash  
# 모든 예제 모드 재다운로드
rm -rf ExampleMods/MatrixFishingUI ExampleMods/Ferngill-Simple-Economy ExampleMods/SpacechaseMods
./setup_environment.sh
```
EOF

log_success "=== 환경 설정 완료! ==="

echo ""
log_info "📊 다운로드된 구성 요소:"
log_success "  🎯 핵심 프레임워크: SMAPI, StardewUI, SpaceCore 등"
log_success "  🏆 참고 모드들: Pathoschild, spacechase0 모드들"  
log_success "  🎮 예제 모드들: MatrixFishingUI, Ferngill-Simple-Economy"

echo ""
log_info "📁 생성된 구조:"
echo "  ExternalLibraries/"
echo "  ├── SMAPI/"
echo "  ├── StardewUI/"
echo "  ├── SpacechaseFrameworks/"
echo "  └── PathoschildMods/"
echo "  ExampleMods/"
echo "  ├── MatrixFishingUI/"
echo "  ├── Ferngill-Simple-Economy/"
echo "  └── SpacechaseMods/"

echo ""
log_success "🎉 개발 환경 설정이 완료되었습니다!"
log_info "이제 Stardew Valley 모드 개발을 시작할 수 있습니다."

echo ""
log_warning "⚠️  참고사항:"
echo "  • 이 폴더들은 Git에서 제외됩니다 (.gitignore)"
echo "  • 필요시 ./setup_environment.sh 재실행으로 재설치 가능"
echo "  • 각 라이브러리의 라이선스를 확인하세요"
