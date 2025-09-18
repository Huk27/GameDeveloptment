# StarML 문법 가이드

## 기본 구조

StarML은 HTML과 유사한 마크업 언어입니다.

### 1. 기본 태그
```xml
<lane orientation="vertical" horizontal-content-alignment="middle">
    <label text="Hello, World!" />
    <button text="Click Me" />
</lane>
```

### 2. 레이아웃 태그
- `<lane>`: 수직 또는 수평 레이아웃
- `<frame>`: 테두리가 있는 컨테이너
- `<panel>`: 투명한 컨테이너
- `<grid>`: 그리드 레이아웃

### 3. 컨텐츠 태그
- `<label>`: 텍스트 표시
- `<button>`: 버튼
- `<image>`: 이미지 표시
- `<textinput>`: 텍스트 입력

## 데이터 바인딩

### 1. 프로퍼티 바인딩
```xml
<label text="{PlayerName}" />
<label text="체력: {Health}" />
```

### 2. 조건부 렌더링
```xml
<label *if="{HasItem}" text="아이템이 있습니다" />
<label *if="{!HasItem}" text="아이템이 없습니다" />
```

### 3. 반복 렌더링
```xml
<label *repeat="{Items}" text="{Name}" />
```

## 스타일링

### 1. 색상
```xml
<label text="빨간색 텍스트" color="#FF0000" />
<label text="파란색 텍스트" color="#0000FF" />
```

### 2. 폰트
```xml
<label text="대화 폰트" font="dialogue" />
<label text="작은 폰트" font="smallFont" />
```

### 3. 마진과 패딩
```xml
<label text="마진이 있는 텍스트" margin="10,5,10,5" />
<frame padding="20,10,20,10">
    <label text="패딩이 있는 컨테이너" />
</frame>
```

## 레이아웃 속성

### 1. 정렬
```xml
<lane horizontal-content-alignment="middle" vertical-content-alignment="center">
    <label text="중앙 정렬된 텍스트" />
</lane>
```

### 2. 크기
```xml
<image layout="64px 64px" />
<button layout="200px 50px" />
```

### 3. 방향
```xml
<lane orientation="vertical">
    <label text="수직 레이아웃" />
</lane>
<lane orientation="horizontal">
    <label text="수평 레이아웃" />
</lane>
```

## 이벤트 처리

### 1. 클릭 이벤트
```xml
<button text="클릭" left-click=|OnButtonClick()| />
```

### 2. 포커스 이벤트
```xml
<button text="포커스" focus=|OnButtonFocus()| />
```

## 일반적인 문제들

### 1. 태그 이름 오류
- `<text>` 태그는 지원되지 않음 → `<label>` 사용
- 올바른 태그 이름 사용

### 2. 속성 이름 오류
- `font-size` → `scale` 또는 `font` 사용
- 올바른 속성 이름 사용

### 3. 데이터 바인딩 오류
- `{PropertyName}` 형태로 올바르게 작성
- ViewModel에 해당 프로퍼티가 존재하는지 확인
