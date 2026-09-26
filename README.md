---

### ⚠️ IMPORTANT NOTICE / DISCLAIMER

**Original Author:** peinwastaken (pein)
**Original Repository:** SPTScrollableAttachments
**Original Link:** https://github.com/peinwastaken/SPTScrollableAttachments
**License:** No license file in the original repository (all rights reserved by the original author)

1. **Reflection & Take-Downs:** I deeply reflect on the ECOT incident. As an AI-assisted "vibe coder," I will immediately delete files if the original authors ask.
2. **No Re-Distribution:** These ported builds are unverified, temporary fixes. Please do NOT re-upload or share them anywhere else.
3. **Do Not Pester Original Authors:** Never report bugs or pester original modders regarding issues from my unofficial ports.
4. **Full Credit & Respect:** I will always credit original creators on GitHub and prioritize their decisions above all else.
5. **Support Original Creators:** Instead of using my ports, please visit the original authors' Forge pages to leave kind words or tips.

---

## 변경 이력

- 2026-09-26 17:30 (KST) — v1.2.0: SPT 4.1 포팅 + 기능 추가
  - **더 좋은 부착물 노란 테두리 표시** 추가: 무기 개조 화면에서 부착물 목록(드롭다운)을 열면, 지금 그 슬롯에 달린 부착물보다 에르고/반동(선택: 정확도·무게)이 좋은 부착물 칸에 노란 테두리가 칠해집니다.
  - **F12 메뉴에서 기능별 켜기/끄기** 추가: "스크롤 기능 사용", "더 좋은 부착물 테두리 표시" 각각 따로 끌 수 있고, 게임 중에 바로 적용됩니다.
  - 스크롤을 끄면 목록이 원래 게임 방식(스크롤 없이 전부 펼쳐짐)으로 완전히 되돌아가도록 원래 값을 저장해뒀다가 복원합니다.
  - 버그 수정: 개조 화면을 한 번도 안 연 상태에서 마우스 휠을 쓰거나 F12 설정을 바꾸면 오류(NullReference)가 날 수 있던 부분을 고쳤습니다.
  - 버그 수정: 설정보다 패치가 먼저 켜지던 순서를 바꿔서, 시작 직후 설정값을 못 읽는 일이 없게 했습니다.
  - 빌드 설정을 SPT 4.1 경로(`E:\SPT 4.1`) 기준으로 바꾸고, Release 빌드 시 배포용 zip(`Dist/`)을 자동으로 만듭니다.

## 이 모드가 하는 일

- 무기 개조 화면 / 프리셋 편집 화면의 **부착물 선택 목록을 스크롤 가능한 창**으로 바꿉니다 (원작 기능). 목록 위에서 휠을 굴려도 뒤의 무기 미리보기가 같이 확대/축소되지 않습니다.
- **(추가)** 지금 달린 부착물보다 좋은 부착물을 **노란 테두리**로 표시합니다.

## 설치

1. `Dist/ScrollableAttachments-SPT4.1-v1.2.0.zip`을 `E:\SPT 4.1`에 압축 해제
   → `E:\SPT 4.1\BepInEx\plugins\ScrollableAttachments\ScrollableAttachments.dll`
2. 게임 실행 후 F12 → `ScrollableAttachments`에서 설정

## F12 설정

### 1. 스크롤

| 항목 | 기본값 | 설명 |
|---|---|---|
| 스크롤 기능 사용 | 켜짐 | 끄면 원래 게임 목록으로 복귀 |
| 스크롤 속도 | 32 | 휠 한 칸당 이동량 |
| 목록 열 개수 | 6 | 한 줄에 보여줄 칸 수 |
| 목록 높이 | 420 | 목록 창 높이 |

### 2. 더 좋은 부착물 표시

| 항목 | 기본값 | 설명 |
|---|---|---|
| 더 좋은 부착물 테두리 표시 | 켜짐 | 테두리 기능 전체 켜기/끄기 |
| 비교: 에르고(인체공학) | 켜짐 | 높을수록 좋음 |
| 비교: 반동 | 켜짐 | 낮을수록(마이너스일수록) 좋음 |
| 비교: 정확도 | 꺼짐 | 높을수록 좋음 |
| 비교: 무게 | 꺼짐 | 가벼울수록 좋음 (부착물 자체 무게만) |
| 판정 방식 | NoneWorse | NoneWorse = 켜둔 기준이 하나도 안 나빠지고 하나 이상 좋아질 때만 / AnyBetter = 하나라도 좋아지면 |
| 테두리 색 | 노란색 | |
| 테두리 두께 | 2 | 1~6 픽셀 |

**비교 기준 참고**
- 빈 슬롯이면 "아무것도 안 단 상태(전부 0)"와 비교합니다. 예: 빈 손잡이 슬롯에서는 에르고가 +이고 반동이 늘지 않는 손잡이가 표시됩니다.
- 부착물 **자체 수치만** 비교합니다. 그 부착물 위에 또 달리는 하위 부착물(예: 핸드가드 위의 손잡이)까지 합친 값이 아닙니다.
- 지금 달린 것과 같은 부착물은 표시하지 않습니다.

## 빌드 방법 (개발자용)

- `Directory.Build.props`의 `SPTPath`가 `E:\SPT 4.1`로 잡혀 있습니다. 다른 곳이면 여기만 바꾸면 됩니다.
- `dotnet build -c Release` → DLL이 `E:\SPT 4.1\BepInEx\plugins\ScrollableAttachments`에 복사되고, `Dist/`에 zip이 생깁니다.
- 참조: `EscapeFromTarkov_Data\Managed`의 `Assembly-CSharp`/`Comfort`/`Sirenix.Serialization`/`UnityEngine.UI`, `BepInEx\core`의 `BepInEx`/`0Harmony`, `BepInEx\plugins\spt\spt-reflection.dll`.
- 가짜(1.0.0.0) `spt-reflection.dll`에 대고 빌드하면 런처가 막는 DLL이 나오므로, 빌드 단계에서 에러로 막아둡니다.

## 확인 필요 (게임 내 테스트)

컨테이너에서는 실제 4.1 게임 어셈블리로 컴파일까지만 확인했습니다. 아래는 게임에서 확인이 필요합니다.

1. 개조 화면에서 목록을 열었을 때 노란 테두리가 칸 테두리에 맞게 보이는지
2. F12에서 스크롤을 껐다 켰을 때 목록 모양이 정상으로 돌아오는지
3. 목록을 닫은 뒤 인벤토리/창고 아이템 칸에 노란 테두리가 남아 있지 않은지
4. `BepInEx\LogOutput.log`에서 `[Highlight] first apply:` / `[Scroll] enabled` 로그가 찍히는지
