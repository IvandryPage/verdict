# ScriptableObject Convention

This document defines how ScriptableObjects are used throughout the Verdict project.

ScriptableObjects are the primary source of **static game content** and should only contain data that is authored by designers.

They must **never** contain runtime state.

---

# Purpose

ScriptableObjects are used to store game content that is:

- reusable
- editable in the Unity Inspector
- shared across multiple systems
- independent from scene objects

Examples include:

- Cases
- Evidence
- Witnesses
- Statements
- Dialogue
- Character Profiles

---

# Responsibilities

ScriptableObjects should only contain:

- configuration
- metadata
- dialogue content
- evidence information
- static relationships
- default values

ScriptableObjects should **not** contain:

- gameplay logic
- temporary state
- player progress
- save data
- scene references
- runtime calculations

---

# Naming

## Classes

Suffix every ScriptableObject class with **Data**.

```csharp
CaseData

EvidenceData

WitnessData

StatementData

DialogueData
```

---

## Assets

Prefix assets using their type.

```text
Case_001

Evidence_BloodyKnife

Evidence_CCTVFootage

Witness_Detective

Witness_Suspect

Statement_Intro_01

Dialogue_Courtroom_Intro
```

---

# Folder Structure

All ScriptableObjects are stored inside

```text
Assets/Data/
```

Organize assets by category.

```text
Data
│
├── Cases
├── Evidence
├── Witnesses
├── Statements
├── Dialogue
├── Characters
└── Localization
```

Avoid mixing different asset types inside the same folder.

---

# Runtime State

Never modify ScriptableObjects during gameplay.

Bad

```csharp
evidenceData.isPresented = true;
```

Good

```csharp
EvidenceRuntime runtimeEvidence;
```

ScriptableObjects define the initial state.

Runtime objects represent the current gameplay state.

---

# Runtime Objects

Whenever mutable data is required, create a dedicated runtime model.

Example

```text
EvidenceData
        ↓
EvidenceRuntime
```

```text
StatementData
        ↓
StatementRuntime
```

```text
CaseData
        ↓
CaseRuntime
```

The runtime object may reference the original ScriptableObject.

---

# References

Prefer direct ScriptableObject references.

Good

```csharp
[SerializeField]
private EvidenceData evidence;
```

Avoid

```csharp
private int evidenceId;

private string evidenceName;
```

IDs should only be used when persistence, networking, or serialization requires them.

---

# Relationships

Relationships between game content should also use ScriptableObject references.

Good

```text
StatementData
    └── supports → EvidenceData

EvidenceData
    └── belongsTo → CaseData

WitnessData
    └── statements → StatementData[]
```

Avoid manually resolving relationships using IDs whenever possible.

---

# Scene References

Never reference scene objects.

Bad

```csharp
public GameObject witness;
```

Good

```csharp
public WitnessData witness;
```

Scene references break reusability and make assets difficult to maintain.

---

# Gameplay Logic

Avoid gameplay logic inside ScriptableObjects.

Bad

```csharp
public bool IsCorrectEvidence()
```

Good

```text
EvidenceEvaluationSystem

CourtroomSystem

DialogueSystem
```

ScriptableObjects define data.

Gameplay systems interpret that data.

---

# CreateAssetMenu

Every ScriptableObject should expose a Create Asset menu.

Example

```csharp
[CreateAssetMenu(
    fileName = "Evidence",
    menuName = "Verdict/Evidence")]
```

Group assets under the **Verdict** root menu.

---

# Inspector

Organize Inspector fields using headers when appropriate.

Example

```csharp
General

Presentation

Relationships

Gameplay
```

Keep Inspector layouts clean and readable.

---

# Validation

Validate required references using `OnValidate()` whenever possible.

Example

```csharp
private void OnValidate()
{
    if (icon == null)
        Debug.LogWarning($"{name} has no icon assigned.");
}
```

Validation should detect authoring mistakes before entering Play Mode.

---

# Versioning

Avoid deleting ScriptableObjects that are already referenced.

Instead:

- rename carefully
- migrate references
- remove only after verifying no dependencies remain

---

# Serialization

Only serialize data that must be authored by designers.

Avoid storing derived values.

Bad

```text
WordCount
```

Good

```text
DialogueText
```

Derived values should be calculated at runtime.

---

# Extensibility

When adding a new gameplay feature, prefer extending existing ScriptableObjects before introducing entirely new asset types.

Maintain a consistent and scalable data model.

---

# Summary

ScriptableObjects are:

- immutable
- reusable
- designer-friendly
- data-only

They should never:

- store runtime state
- reference scene objects
- contain gameplay logic
- track player progress
- implement gameplay systems
