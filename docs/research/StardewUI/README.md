# StardewUI 공식 문서

## 개요
StardewUI는 Stardew Valley 모드를 위한 편리하고 확장 가능한 UI 프레임워크로, Angular와 XAML에서 영감을 받아 개발되었습니다.

## 주요 기능

### 1. 동적 레이아웃
- 픽셀 위치에 신경 쓰지 않고, 흐름, 그리드 등 다양한 레이아웃을 활용
- 콘텐츠에 맞게 UI를 설계할 수 있음

### 2. 높은 성능
- 변경된 부분만 업데이트하는 유지 모드 UI
- 성능 저하 없이 부드러운 UI 제공

### 3. 컨트롤러 지원
- 마우스나 게임패드 등 다양한 입력 장치 지원
- 복잡한 설정 없이 바로 사용 가능

### 4. Model-View-Whatever 아키텍처
- HTML과 유사한 마크업과 데이터 바인딩
- 뷰와 데이터를 분리하여 관리

### 5. 다양한 위젯 제공
- 텍스트, 이미지, 드롭다운 리스트, 슬라이더, 입력 상자, 스크롤바 등

## 시작하기

### 1. 프레임워크 설치
- 최신 릴리스를 GitHub 또는 Nexus Mods에서 다운로드
- `Stardew Valley\Mods` 폴더에 `StardewUI` 폴더를 복사

### 2. API 추가
```csharp
// IViewEngine.cs 파일을 모드의 소스 디렉토리에 추가
// ModEntry.cs 파일에서
using StardewUI.Framework;

// GameLaunched 이벤트 핸들러에서
viewEngine = Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI");
```

### 3. 자산 경로 등록
- 뷰와 기타 자산이 위치할 디렉토리를 결정
- 해당 경로를 프레임워크에 등록

### 4. 뷰 파일 생성
- `.sml` 확장자를 가진 새로운 뷰 파일을 생성
- 프로젝트에 포함시켜 빌드 시 출력에 복사되도록 설정

### 5. 뷰 작성
- StarML 가이드를 참고하여 뷰를 작성

### 6. 뷰 표시
- `IViewEngine` API를 사용하여 뷰를 표시

## 참고 자료
- [공식 문서](https://focustense.github.io/StardewUI/)
- [예제 모음](https://focustense.github.io/StardewUI/examples/)
- [표준 뷰](https://focustense.github.io/StardewUI/library/standard-views/)
