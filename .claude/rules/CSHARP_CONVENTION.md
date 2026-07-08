---
paths:
  - "**/*.cs"
  - "**/*.csx"
  - "**/*.uxml"
  - "**/*.uss"
---

# C# Code Convention for Unity

> All C# code in this project MUST follow these rules.

## Naming

| Element | Casing | Example |
|---------|--------|---------|
| Namespace | PascalCase | `MyGame.GameFlow` |
| Class / Struct | PascalCase | `PlayerController` |
| Interface | I + PascalCase | `IDamageable` |
| Method | PascalCase | `GetDirection()` |
| Public field | PascalCase | `MaxHealth` |
| Private / Protected field | camelCase (no prefix) | `currentHealth` |
| Static field | PascalCase | `SharedInstance` |
| Constant (const) | PascalCase | `MaxItems` |
| Property | PascalCase | `IsAlive` |
| Event | PascalCase (verb phrase) | `DoorOpened` |
| Enum type | PascalCase singular | `WeaponType` |
| Enum value | PascalCase | `RocketLauncher` |
| [Flags] Enum type | PascalCase plural | `AttackModes` |
| Local variable | camelCase | `elapsedTime` |
| Parameter | camelCase | `damageAmount` |
| Type parameter | T + PascalCase | `TElement` |

### Naming Details

- Use descriptive nouns for variables. Never abbreviate except for loop counters and math.
- Prefix `bool` with a verb: `isDead`, `hasKey`, `canJump`.
- Methods start with a verb: `GetDirection()`, `FindTarget()`. Bool-returning methods ask a question: `IsGameOver()`.
- Treat acronyms as words: `XmlHttpRequest` not `XMLHTTPRequest`.
- Do not duplicate the class name in member names: in class `Player`, use `score` not `playerScore`.
- Always write access modifiers explicitly, including `private`.
- One variable declaration per line.
- Omit redundant default initializers (`= 0`, `= null`, `= false`).

## Formatting

### Indentation and Whitespace

- **4 spaces**, no tabs.
- Max line length: **100 characters**.
- Continuation lines: additional 4 spaces indent.

### Braces: Allman Style

- Opening brace on a **new line**.
- **Never omit braces**, even for single-line blocks.

```csharp
if (isAlive)
{
    TakeDamage(10f);
}
else
{
    Die();
}
```

### Spacing

- Space after `,` between arguments: `Foo(a, b, c)`
- No space between method name and `(`: `DoSomething()`
- No space inside `()` or `[]`: `array[index]`, `Method(arg)`
- Space after `if`/`for`/`while`: `if (condition)`
- Space around binary operators: `x == y`, `a + b`
- No space after unary operators: `!isDead`, `++count`

### Vertical Spacing

- One blank line between methods.
- Two blank lines between classes.
- Group related methods together.
- Max one blank line between type members.

### #region

- **Do not use `#region`.** If a class needs regions, it should be split into smaller classes.

### Switch

- Indent `case` one level from `switch`. Always include `default`.

```csharp
switch (state)
{
    case State.Idle:
        DoIdle();
        break;
    default:
        break;
}
```

## Class Structure

### File Rules

- One MonoBehaviour (or primary class) per file.
- File name must match the MonoBehaviour class name.
- File and directory names: PascalCase.

### Member Order

```
1. Constants (const)
2. Static fields
3. Fields
4. Properties
5. Events / Delegates
6. Unity lifecycle methods (Awake, OnEnable, Start, Update, FixedUpdate,
   LateUpdate, OnDisable, OnDestroy)
7. Public methods
8. Private methods
```

Within each group, order by access level: `public` > `internal` > `protected` > `private`.

### Modifier Order

```
public / protected / internal / private
new
abstract / virtual / override / sealed
static
readonly
extern
unsafe
volatile
async
```

### Class Size

- Keep classes small. Follow the Single Responsibility Principle.
- Split large MonoBehaviours into focused components (e.g., `PaddleInput`, `PaddleMovement`, `PaddleAudio`).
- Mark non-inheritable `internal`/`private` classes as `sealed`.
- Use `static` for utility classes with no instance state.

## Properties

- Single-line read-only: use expression body `=>`.
- Everything else: use `{ get; set; }`.
- Use `private set` or `init` when write access should be restricted.

```csharp
public int MaxHealth => maxHealth;
public int CurrentHealth { get; private set; }
public int Armor
{
    get => armor;
    set => armor = Mathf.Max(0, value);
}
```

## Methods

- Minimize parameters. Group related params into a struct or class.
- Avoid side effects: a method should only do what its name says.
- Prefer separate methods over boolean flag parameters: use `GetAngleInDegrees()` and `GetAngleInRadians()` instead of `GetAngle(bool inRadians)`.
- Extension methods go in static classes named `{Type}Extensions`.

## Serialization

- Use `[SerializeField]` for private fields that need Inspector exposure.
- Public fields are allowed for Unity Inspector access.
- Use `[Range(min, max)]` for numeric fields.
- Group related data with `[Serializable]` structs.

```csharp
[SerializeField]
private float moveSpeed = 5f;

[SerializeField, Range(0f, 100f)]
private float maxHealth = 100f;

[Serializable]
public struct PlayerStats
{
    public int MovementSpeed;
    public int HitPoints;
}
```

## var Usage (Conservative)

- Use `var` **only** when the type is obvious from the right-hand side (`new`, explicit cast, literal).
- Use explicit types when the type is not immediately clear.
- Use explicit types in `foreach` loops.
- `var` is allowed for `for` loop counters and LINQ query results.

```csharp
// OK: type is obvious
var player = new PlayerController();
var enemies = new List<Enemy>();

// Required: type not obvious
int maxScore = ExampleClass.ResultSoFar();
PlayerController controller = GetComponent<PlayerController>();

// foreach: explicit type
foreach (Enemy enemy in enemies)
{
    enemy.TakeDamage(10f);
}
```

## Comments

- Code should be self-explanatory. Comments explain **why**, not **what**.
- Use `//` (single-line). Avoid `/* */` block comments.
- Place comments on a separate line, not at end of code.
- Start with uppercase, end with period. One space after `//`.
- Delete commented-out code. Use source control for history.
- Use `<summary>` XML comments for public members.
- Use `[Tooltip("...")]` for serialized fields instead of comments.
- TODO format: `// TODO(author, YYYY-MM-DD): Description.`

```csharp
/// <summary>
/// Inflicts damage and triggers damage effects.
/// </summary>
public void InflictDamage(float damage)
{
}

[Tooltip("The amount of side-to-side friction.")]
[SerializeField]
private float grip;
```

## Events

- Name with verb phrases. Use present participle for "before" (`OpeningDoor`) and past participle for "after" (`DoorOpened`).
- Use `System.Action<T>` delegate.
- Prefix raising methods with `On`: `OnDoorOpened()`.
- Use `?.Invoke()` for null-safe invocation.
- Handler naming: `{Publisher}_{EventName}`.

```csharp
public event Action DoorOpened;
public event Action<int> PointsScored;

private void OnDoorOpened()
{
    DoorOpened?.Invoke();
}
```

## Namespaces

- `using` directives go **outside** namespace, at file top.
- Sort: `System.*` first, then alphabetical.
- **Do NOT use file-scoped namespace** (`namespace X;`). Unity's C# compiler targets C# 9.0, which does not support this feature. Always use block-scoped namespace with braces.
- Mirror folder structure: `MyGame.AI`, `MyGame.UI`.
- Max 2-3 levels deep.

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Utils;

namespace MyGame.GameFlow
{
    public class MyClass
    {
    }
}
```

## Other Rules

### Language Keywords

Use C# keywords, not BCL types: `string` not `String`, `int` not `Int32`.

### Strings

- Use string interpolation `$""` for concatenation.
- Use `StringBuilder` for repeated concatenation in loops.

### Operators

- Always use short-circuit operators `&&` and `||`, not `&` and `|`.

### new Operator

- Use `var` or target-typed `new()` when type is clear. Use object initializers.

```csharp
var config = new GameConfig
{
    MaxPlayers = 4,
    RoundTime = 120f,
};
```

### Collections

- Growable containers: prefer `List<T>`.
- Fixed-size containers: prefer arrays.

### using Statement

- Use `using` declaration (without braces) for `IDisposable`.

### LINQ

- Meaningful query variable names.
- `where` before other clauses.
- Prefer short method chains over long ones.

### nameof

- Use `nameof()` instead of string literals for identifiers.

### struct vs class

- Default to `class`. Use `struct` only for value-type semantics (Vector3, Bounds).

## UI Toolkit Naming (UXML/USS)

Use **BEM** (Block Element Modifier) with **kebab-case**:

```
block-name__element-name--modifier-name
```

- Block: independent UI component (`navbar-menu`)
- Element: child of block, joined with `__` (`navbar-menu__shop-button`)
- Modifier: variation/state, joined with `--` (`navbar-menu__shop-button--small`)
- Use semantic names, not presentational: `button--quit` not `button--red`.
