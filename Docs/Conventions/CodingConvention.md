# Coding Convention

This document defines the coding standards used throughout the Verdict project.

The purpose of these conventions is to improve consistency, readability, maintainability, and collaboration across the development team.

These guidelines should be followed unless there is a clear and documented reason not to.

---

# General Principles

- Prioritize readability over cleverness.
- Keep classes focused on a single responsibility.
- Prefer composition over inheritance.
- Avoid premature optimization.
- Write code that is easy to understand and modify.
- Keep systems modular and loosely coupled.
- Favor explicit code over implicit behavior.

---

# File Organization

Each public type must be placed in its own file.

```
EvidenceManager.cs
CourtroomManager.cs
CaseData.cs
```

The file name must exactly match the public type.

---

# Class Organization

Organize members using the following order.

```text
Constants

Static Fields

Serialized Fields

Private Fields

Properties

Events

Unity Callbacks

Public Methods

Private Methods
```

Example:

```csharp
private const float DefaultPenalty = 10f;

[SerializeField] private float penaltyAmount;

private bool isPenaltyEnabled;

public bool IsPenaltyEnabled => isPenaltyEnabled;

public event Action PenaltyApplied;

private void Awake()
{
}

private void OnEnable()
{
}

private void Start()
{
}

public void ApplyPenalty()
{
}

private void UpdatePenalty()
{
}
```

---

# Access Modifiers

Always specify access modifiers explicitly.

Good

```csharp
private int score;

public class EvidenceManager
```

Avoid relying on default accessibility.

---

# Serialized Fields

Prefer serialized private fields over public fields.

Good

```csharp
[SerializeField]
private AudioSource audioSource;
```

Bad

```csharp
public AudioSource audioSource;
```

Only serialize fields that need to be configured in the Inspector.

---

# Properties

Use properties to expose state.

Good

```csharp
public int Health { get; private set; }
```

Bad

```csharp
public int health;
```

---

# Constants

Avoid hardcoded values whenever possible.

Good

```csharp
private const float FadeDuration = 0.5f;
```

or

```csharp
[SerializeField]
private float fadeDuration = 0.5f;
```

Bad

```csharp
yield return new WaitForSeconds(0.5f);
```

---

# Null Validation

Always validate external references before use.

```csharp
if (target == null)
    return;
```

Fail early whenever possible.

---

# Early Return

Prefer early returns to reduce nesting.

Good

```csharp
if (evidence == null)
    return;

PresentEvidence(evidence);
```

Avoid unnecessary nesting.

---

# Unity Lifecycle

Use Unity callbacks only for their intended purpose.

| Callback | Responsibility |
|----------|----------------|
| Awake() | Initialize internal references |
| OnEnable() | Subscribe to events |
| Start() | Initialize gameplay after all objects exist |
| Update() | Continuous logic only |
| FixedUpdate() | Physics |
| LateUpdate() | Camera or post-processing |
| OnDisable() | Unsubscribe from events |
| OnDestroy() | Final cleanup |

---

# Update Methods

Avoid Update() whenever possible.

Prefer:

- Events
- Callbacks
- Coroutines

Only use Update() when logic must execute every frame.

---

# Events

Prefer events over polling.

Always unsubscribe from events.

Good

```csharp
private void OnEnable()
{
    inputReader.SubmitPressed += HandleSubmit;
}

private void OnDisable()
{
    inputReader.SubmitPressed -= HandleSubmit;
}
```

---

# Coroutines

Use coroutines for asynchronous sequences.

Examples:

- Dialogue progression
- Fade animations
- Camera transitions
- Delayed actions

Do not use coroutines as replacements for Update().

---

# Dependencies

Avoid runtime object searching.

Avoid

```csharp
FindObjectOfType<T>()
GameObject.Find()
GameObject.FindWithTag()
```

Prefer

```csharp
[SerializeField]
private EvidenceManager evidenceManager;
```

or dependency injection through initialization.

---

# Inspector

Only expose values that designers are expected to modify.

Implementation details should remain private.

Group related fields using Inspector headers when appropriate.

Example

```csharp
[Header("Movement")]
[SerializeField] private float moveSpeed;

[Header("Animation")]
[SerializeField] private Animator animator;
```

---

# Logging

Use logging only for debugging purposes.

Use the appropriate log level.

```
Debug.Log()

Debug.LogWarning()

Debug.LogError()
```

Remove unnecessary debug logs before merging.

---

# Comments

Explain **why**, not **what**.

Good

```csharp
// Prevent duplicate evidence submission during animation.
```

Bad

```csharp
// Submit evidence.
```

Self-explanatory code should not require comments.

---

# Formatting

- 4 spaces indentation
- UTF-8 encoding
- LF line endings
- Opening braces on a new line
- One blank line between logical sections
- Maximum line length of approximately 120 characters

Example

```csharp
public void PresentEvidence()
{
    // ...
}
```

---

# Regions

Avoid using `#region`.

Only use regions for exceptionally large files where organization significantly improves readability.

---

# Naming

Follow the project's Naming Convention document.

Do not invent naming styles within individual files.

---

# Managers

Managers coordinate systems.

Managers should:

- initialize systems
- coordinate communication
- control high-level flow

Managers should not:

- contain gameplay rules
- implement UI behavior
- store unrelated runtime state
- become "God Objects"

Gameplay logic belongs in dedicated gameplay systems.

---

# MonoBehaviour

MonoBehaviours should be lightweight.

Heavy gameplay logic should be implemented in plain C# classes whenever practical.

MonoBehaviours primarily act as the bridge between Unity and gameplay systems.

---

# ScriptableObjects

ScriptableObjects are immutable data containers.

Do not modify ScriptableObject data at runtime.

Runtime state should exist in dedicated runtime objects.

---

# Error Handling

Never silently ignore errors.

Unexpected situations should:

- log an appropriate error
- fail gracefully
- avoid crashing the game whenever possible

---

# Performance

Write readable code first.

Optimize only after profiling identifies a measurable bottleneck.

Avoid premature optimization.

---

# Code Reviews

Before opening a Pull Request, verify:

- No compiler errors
- No unnecessary warnings
- No missing references
- No debug code
- No commented-out code
- No unused serialized fields
- No unnecessary TODOs

Every commit merged into `main` should leave the project in a working state.
