# Prefab Convention

This document defines how prefabs are created, organized, and maintained throughout the Verdict project.

Prefabs should be modular, reusable, and easy to maintain.

---

# Purpose

Use prefabs for reusable GameObjects.

Examples include:

- Characters
- Environment props
- Gameplay objects
- UI elements
- Visual effects

Avoid creating prefabs that are only used once unless there is a clear benefit.

---

# Folder Structure

Store prefabs under:

```text
Assets/Prefabs/
```

Organize by category.

```text
Prefabs
│
├── Characters
├── Environment
├── Gameplay
├── Props
├── UI
└── VFX
```

---

# Naming

Use **PascalCase**.

Good

```text
Judge

Lawyer

WitnessStand

EvidenceCard

CourtroomDesk
```

Avoid

```text
PF_Judge

judgePrefab

Judge_New
```

Unity already identifies prefabs through their icon.

---

# Root Object

The root GameObject name must match the prefab name.

Example

```text
EvidenceCard.prefab

└── EvidenceCard
```

Avoid

```text
GameObject

Root

Prefab
```

---

# Variants

Use Prefab Variants whenever multiple prefabs share the same base structure.

Example

```text
WitnessBase

├── WitnessPolice

├── WitnessDoctor

└── WitnessJournalist
```

Modify shared functionality in the base prefab whenever possible.

---

# Composition

Prefer composition over deep inheritance.

Attach reusable components instead of duplicating entire prefabs.

Example

```text
Judge

├── Animator

├── DialogueSpeaker

└── CharacterAudio
```

---

# Nesting

Nested prefabs are allowed when they improve modularity.

Examples

```text
Courtroom

├── JudgeStand

├── WitnessStand

├── DefenseTable

└── ProsecutionTable
```

Avoid excessive nesting.

As a general guideline:

- 1–2 levels are acceptable.
- More than 3 levels should be reconsidered.

---

# Components

Arrange components consistently.

Recommended order

```text
Transform

Mesh Filter

Mesh Renderer

Collider

Rigidbody

Animator

Audio Source

Gameplay Scripts
```

Group custom scripts near the bottom.

---

# Serialized References

Avoid missing references.

Every serialized reference should be assigned intentionally.

Never leave empty fields unless they are explicitly optional.

---

# Overrides

Avoid unnecessary prefab overrides.

Apply changes to the prefab asset whenever appropriate.

Do not keep accidental instance overrides inside scenes.

---

# Scene References

Prefabs should never reference scene objects directly.

Bad

```text
Main Camera

Directional Light

Canvas
```

Good

Use dependency injection or assign references at runtime.

---

# Runtime Instantiation

Objects that are spawned during gameplay should always be instantiated from prefabs.

Avoid creating gameplay objects manually through code.

Good

```csharp
Instantiate(evidenceCardPrefab);
```

Avoid

```csharp
new GameObject("Evidence");
```

unless creating temporary utility objects.

---

# Scale

Prefab root scale should remain

```text
(1, 1, 1)
```

Avoid non-uniform scaling whenever possible.

---

# Position & Rotation

The prefab root should have

```text
Position

(0, 0, 0)
```

```text
Rotation

(0, 0, 0)
```

unless a different default transform is intentionally required.

---

# Organization

Every prefab should have

- meaningful root name
- clean hierarchy
- no missing references
- no unused components
- no disabled components without purpose

---

# Responsibilities

Each prefab should represent a single reusable object.

Avoid combining unrelated functionality into one prefab.

Good

```text
EvidenceCard

WitnessStand

Judge
```

Avoid

```text
CourtroomEverything
```

---

# UI Prefabs

Reusable UI should also be prefabs.

Examples

```text
ButtonPrimary

EvidenceCard

DialoguePanel

PenaltyBar
```

Avoid duplicating UI objects across scenes.

---

# Testing

Before committing a prefab, verify:

- No missing references
- Correct layer
- Correct tag (if applicable)
- Correct root transform
- Correct scale
- No unnecessary overrides
- No editor-only objects
- No unused components

---

# Summary

A good prefab should be:

- reusable
- modular
- self-contained
- easy to understand
- easy to extend
- free of missing references
