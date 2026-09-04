# Artesia

> 우주를 배경으로 한 로그라이크 턴제 던전 RPG
> Unity 2D 팀 프로젝트 (2024)

---

## 소개

Artesia는 매번 다르게 생성되는 던전을 탐험하며 미지의 비밀을 파헤치는 2D 로그라이크 RPG입니다.
이상한 던전 시리즈의 턴제 던전 탐험 방식에 서브컬처 감성을 녹여 독창적인 플레이 경험을 제공합니다.

---

## 스크린샷

<!-- 스크린샷 또는 GIF 삽입 -->
| 던전 탐험 | 전투 화면 |
|:---------:|:---------:|
| | |

---

## 한눈에 보는 턴 루프

매 프레임 모든 몬스터가 자기 턴을 마쳤는지 확인해서 플레이어 턴을 다시 열어주는, 이 프로젝트의 가장 기본적인 순환입니다.

```csharp
void Update()
{
    if (CheckUnitTurn())
        SetTurn(Player, false);   // 모든 몬스터가 턴을 마치면 플레이어 턴 개방

    if (TurnCnt > spawnTurn && MobList != null)
    {
        EnemySpawner.instance.RandomSpawnEnemy();
        TurnCnt = 0;
    }
}
```
`TurnManager.cs`

---

## 핵심 기능

- **절차적 던전 생성** — BSP 알고리즘으로 매 플레이마다 새로운 맵 구조 생성
- **턴제 전투 시스템** — 이동, 공격, 스킬, 아이템 사용을 조합한 전략적 전투
- **속성 시스템** — 캐릭터·적·아이템의 속성 조합에 따른 데미지 계산
- **AI 적 행동** — A* Pathfinding 기반의 적 추적 및 행동 패턴
- **8방향 이동** — 그리드 맵 위에서 자유로운 이동 및 스킬을 통한 이동 범위 확장

---

## 기술 스택

| 분야 | 기술 |
|------|------|
| 엔진 | Unity 2D |
| 언어 | C# |
| 맵 생성 | BSP (Binary Space Partitioning) Algorithm |
| 경로 탐색 | A* Pathfinding |

---

## 핵심 기술 구현

### 1. ServiceLocator + Null Object — Manager 간 직접 참조 제거

`PlayerAtk`, `PlayerStat` 같은 상태/전투 로직이 `BattleManager`, `UIManager`를 직접 참조하면 씬 구성이 바뀔 때마다 참조가 깨지고, Manager 없는 테스트 씬에서는 곧바로 NullReferenceException이 났습니다. 인터페이스(`IBattleLog`, `IUIDamageNotifier` 등)로 등록·조회하는 경량 ServiceLocator를 두고, 서비스가 없으면 Null 구현체로 폴백하도록 했습니다.

```csharp
public static T Get<T>() where T : class
{
    if (_registry.TryGetValue(typeof(T), out var service))
        return service as T;

    // 등록된 서비스가 없으면 Null 구현체로 폴백 (테스트 씬 등에서 에러 방지)
    return GetNullImplementation<T>();
}
```
`ServiceLocator.cs`

### 2. 제네릭 `StateMachine<T>` — Player/Mob 공통 상태 전이

플레이어(Idle/Move/Atk/Skill)와 몬스터(Idle/Move/Atk)는 상태 종류는 다르지만 전이 흐름(Exit → 교체 → Enter)은 동일합니다. 대상 타입을 제네릭으로 받는 하나의 상태 머신으로 양쪽 다 처리해 중복 구현을 없앴습니다.

```csharp
public void SetState(IState<T> state)
{
    if (m_sender == null || CurState == state) return;

    CurState?.OperateExit(m_sender);
    CurState = state;
    CurState?.OperateEnter(m_sender);
}
```
`StateMachine.cs`

### 3. BSP 기반 절차적 던전 생성

매 플레이마다 다른 구조의 던전을 만들기 위해, 맵 영역을 이진 트리로 재귀 분할(BSP)하고 리프 노드마다 방을 배치합니다. `maxDepth`로 방 개수를, 분할 비율 범위로 방 크기의 편차를 조절합니다.

```csharp
void Divide(Node Tree, int n)
{
    if (n == maxDepth) { rooms.Add(Tree); return; }

    int maxLength = Mathf.Max(Tree.nodeRect.width, Tree.nodeRect.height);
    int split = Mathf.RoundToInt(Random.Range(maxLength * minDevideRate, maxLength * maxDevideRate));
    // 긴 변을 기준으로 분할 후 양쪽을 재귀 호출
}
```
`MapGenerator.cs`

### 4. 논리적 좌표 vs 실제 좌표 — 턴제 이동 동기화 버그 수정

플레이어가 이동 상태에 진입하면 `EndPlayerTurn()`이 즉시 호출되어 몬스터 경로를 다시 계산하는데, 이 시점의 `transform.position`은 아직 이전 칸이라 몬스터가 한 턴 늦게 쫓아오는 문제가 있었습니다. 실제 좌표 대신 이동이 이미 확정된 "논리적 목표 좌표"(`TargetPos`)를 기준으로 경로를 계산하도록 고쳤습니다.

```csharp
PlayerController pc = Player.GetComponent<PlayerController>();
Vector2 playerLogical = pc != null ? pc.TargetPos : (Vector2)Player.transform.position;

foreach (GameObject Obj in MobList)
    EnemySpawner.instance.updatePath(Obj, playerLogical);
```
`TurnManager.cs`

---

## 프로젝트 구조

```
Assets/
├── 00.Scenes/                  # 씬 파일
│   ├── MainScene.unity
│   ├── DungeonSample.unity
│   └── BaseCamp.unity
│
├── 01.Scripts/
│   ├── Core/                   # 인터페이스 및 상태 머신
│   │   ├── IAbility.cs
│   │   ├── IDamageable.cs
│   │   ├── IState.cs
│   │   ├── ITurn.cs
│   │   └── StateMachine.cs
│   ├── Player/                 # 플레이어 상태 및 행동
│   │   ├── PlayerController.cs
│   │   ├── PlayerMove.cs
│   │   ├── PlayerIdle.cs
│   │   ├── PlayerAtk.cs
│   │   ├── PlayerStat.cs
│   │   ├── PlayerSkill.cs
│   │   └── PlayerCollider.cs
│   ├── Monster/                # 몬스터 AI 및 행동
│   │   ├── MobController.cs
│   │   ├── MobMove.cs
│   │   ├── MobIdle.cs
│   │   ├── MobAtk.cs
│   │   ├── MobStat.cs
│   │   └── EnemySpawner.cs
│   ├── Dungeon/                # 맵 생성 및 경로 탐색
│   │   ├── MapGenerator.cs
│   │   ├── DrawTile.cs
│   │   ├── AStarPathfinder.cs
│   │   └── Node.cs
│   ├── Item/                   # 아이템 및 스포너
│   │   ├── PotionStat.cs
│   │   └── ItemSpawner.cs
│   ├── Interaction/            # 맵 오브젝트 상호작용
│   │   ├── PortalCollider.cs
│   │   └── StairCollider.cs
│   ├── UI/                     # HUD 및 UI 컴포넌트
│   │   ├── StatusUI.cs
│   │   ├── DmgText.cs
│   │   ├── MinimapOnPlayer.cs
│   │   ├── CanvasStateListener.cs
│   │   ├── KeySettingSwitch.cs
│   │   ├── CameraController.cs
│   │   └── Btn/                # 씬 전환·메뉴 버튼
│   │       ├── BackBtn.cs
│   │       ├── BaseCampBtn.cs
│   │       ├── MainMenuButton.cs
│   │       ├── NextStageButton.cs
│   │       ├── OptionBtn.cs
│   │       ├── SaveDataBtn.cs
│   │       └── SelectDunBtn.cs
│   ├── Data/                   # 데이터 모델
│   │   ├── Data.cs
│   │   └── CharacterInformation.cs
│   ├── Manager/                # 싱글톤 매니저
│   │   ├── GameManager.cs
│   │   ├── BattleManager.cs
│   │   ├── TurnManager.cs
│   │   ├── UIManager.cs
│   │   ├── DataManager.cs
│   │   └── SceneLoader.cs
│   └── Shader/
│       └── SpriteOutline.cs
│
├── 03.Tilemaps/                # 타일맵 및 룰 타일
├── Animation/                  # 애니메이터 컨트롤러 및 애니메이션 클립
├── Resources/                  # 런타임 로드 리소스
│   ├── Prefabs/
│   ├── Player/
│   ├── Monster/
│   └── Image/
└── InputSystem/                # Unity Input System 액션
```

---

## 팀 구성

| 이름 | 역할 |
|------|------|
| 권상헌 | Game Director |
| 송수민 | Game Programmer |
| 고가영 | Game Designer |
| 김상진 | Game Designer |
| 이서연 | Game Designer |

---

## 개발 기간

2024년 3월 — 2024년 6월

---

## 레퍼런스

- 이상한 던전 시리즈 (不思議のダンジョン) — 포켓몬스터 불가사의 던전
- 원신, 붕괴: 스타레일, 블루 아카이브 — 서브컬처 비주얼 방향성
