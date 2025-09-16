# SimpleUI - StardewUI 예제 모드

이 모드는 StardewUI 프레임워크를 사용하여 간단한 플레이어 정보 UI를 만드는 예제 모드입니다.

## 기능

- 플레이어 이름 표시
- 농사 스킬 레벨 표시  
- 현재 위치 표시
- 소지금 표시
- 현재 시간 표시

## 사용법

1. **StardewUI 모드 설치**: 이 모드를 사용하려면 먼저 [StardewUI](https://www.nexusmods.com/stardewvalley/mods/xxx) 모드가 설치되어 있어야 합니다.

2. **모드 설치**: 
   - 이 모드 폴더를 Stardew Valley의 `Mods` 폴더에 복사
   - 또는 빌드 후 `bin/Release/net6.0` 폴더의 내용을 복사

3. **게임 실행**: 
   - SMAPI로 게임을 실행
   - 게임 내에서 **'P' 키**를 눌러 플레이어 정보 UI 열기/닫기

## 빌드 방법

1. .NET 6 SDK 설치
2. `build.bat` 파일 실행
3. 빌드된 파일을 Mods 폴더에 복사

## 파일 구조

```
SimpleUI/
├── manifest.json          # 모드 메타데이터
├── ModEntry.cs            # 메인 모드 로직
├── PlayerInfoViewModel.cs # UI 데이터 모델
├── UI/
│   └── PlayerInfo.sml     # StarML UI 정의
├── assets/                # 에셋 파일들 (필요시)
├── SimpleUI.csproj        # C# 프로젝트 파일
└── build.bat              # 빌드 스크립트
```

## StardewUI 학습 포인트

이 모드는 StardewUI의 핵심 개념을 보여줍니다:

1. **StarML**: HTML과 유사한 선언적 UI 언어
2. **ViewModel**: UI에 표시될 데이터를 관리하는 C# 클래스
3. **데이터 바인딩**: `{PropertyName}` 구문으로 ViewModel의 속성을 UI에 연결
4. **StardewUIMenu**: StarML과 ViewModel을 연결하는 메뉴 클래스

## 확장 아이디어

- 더 많은 플레이어 정보 추가 (스킬, 아이템 등)
- 실시간 업데이트 기능 개선
- UI 디자인 개선
- 설정 메뉴 추가

## 문제 해결

- **UI가 열리지 않음**: StardewUI 모드가 설치되어 있는지 확인
- **빌드 오류**: .NET 6 SDK가 설치되어 있는지 확인
- **게임 충돌**: SMAPI 로그를 확인하여 오류 메시지 확인
