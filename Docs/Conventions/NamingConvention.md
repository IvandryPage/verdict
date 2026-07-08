# Naming Convention

This document defines the naming conventions used throughout the Verdict project.

The goal is to maintain consistency and improve readability across the codebase and project assets.

> This document only covers naming rules.
>
> For ScriptableObject guidelines, see `ScriptableObjectConvention.md`.
> For Prefab guidelines, see `PrefabConvention.md`.

---

# General Principles

- Use English for all identifiers.
- Prefer descriptive names over abbreviations.
- Use nouns for data.
- Use verbs for actions.
- Be consistent throughout the project.

---

# Classes

Use **PascalCase**.

Good

```csharp
EvidenceManager
CourtroomManager
DialogueSystem
CaseLoader
```

Avoid

```csharp
evidenceManager
evidence_manager
EvidenceMgr
```

---

# Interfaces

Prefix interfaces with **I**.

```csharp
IEvidenceProvider
ICaseLoader
IDialogueSource
```

---

# Enums

Enum names use **PascalCase**.

```csharp
CourtroomState
EvidenceType
DialogueState
```

Enum values also use **PascalCase**.

```csharp
Idle
CrossExamination
Verdict
Finished
```

---

# Methods

Methods should describe actions.

Use verbs whenever possible.

Good

```csharp
LoadCase()

PresentEvidence()

EvaluateStatement()

ApplyPenalty()
```

Avoid

```csharp
DoStuff()

Handle()

Run()

Process()
```

---

# Properties

Use **PascalCase**.

```csharp
public bool IsActive { get; }

public int EvidenceCount { get; }
```

---

# Variables

Use **camelCase**.

Private fields

```csharp
private int score;

private AudioSource audioSource;
```

Serialized fields

```csharp
[SerializeField]
private GameObject evidencePanel;
```

Local variables

```csharp
var selectedEvidence = currentEvidence;
```

Method parameters

```csharp
PresentEvidence(EvidenceData evidence)
```

---

# Boolean Variables

Prefix boolean names with one of the following:

```
Is
Has
Can
Should
```

Examples

```csharp
IsCorrect

HasEvidence

CanPresent

ShouldSkip
```

Avoid

```csharp
correct

visible

enabled
```

---

# Constants

Use **PascalCase**.

```csharp
private const int MaxEvidence = 10;

private const float DefaultPenalty = 15f;
```

---

# Events

Events should describe something that has already happened.

Use **PascalCase** and **past tense**.

```csharp
EvidencePresented

StatementFinished

PenaltyApplied

CaseCompleted
```

---

# Event Handlers

Prefix event handlers with **Handle**.

```csharp
HandleSubmit()

HandleEvidencePresented()

HandleDialogueFinished()
```

---

# Namespaces

Use **PascalCase**.

Follow the project hierarchy.

```text
Verdict

Verdict.Core

Verdict.Courtroom

Verdict.Data

Verdict.UI

Verdict.Input

Verdict.Audio
```

---

# Files

File names must match the primary class name.

```text
EvidenceManager.cs

DialogueController.cs

CourtroomManager.cs
```

---

# Folders

Use **PascalCase**.

```text
Scripts

Prefabs

Data

Art

Audio

Settings

Scenes
```

---

# Scenes

Gameplay scenes should use numeric prefixes.

```text
00_Bootstrap

01_MainMenu

02_Courtroom
```

Development scenes do not require numbering.

```text
Sandbox

Testing
```

---

# Git Branches

Use **lowercase kebab-case**.

```text
feature/project-structure

feature/data-model

feature/evidence-system

feature/courtroom-flow

fix/dialogue-bug

docs/update-readme
```

---

# GitHub Issues

Use concise, action-oriented titles.

Good

```text
Implement Statement System

Create Courtroom UI

Configure Input System

Design Evidence Data Model
```

---

# Abbreviations

Avoid abbreviations unless they are widely understood.

Good

```text
Evidence

Dialogue

Statement

Controller
```

Avoid

```text
Evd

Dlg

Ctrl

Mgr
```

---

# Acronyms

Treat acronyms as regular words.

Good

```csharp
UiManager

JsonLoader
```

Avoid

```csharp
UIManager

JSONLoader
```

> Exception: When referencing Unity APIs or external libraries, follow their official naming.
>
> Example:
>
> - UI Toolkit
> - JSONUtility
> - TMP_Text

---

# Summary

| Element | Convention |
|----------|------------|
| Classes | PascalCase |
| Interfaces | IPascalCase |
| Enums | PascalCase |
| Enum Values | PascalCase |
| Methods | PascalCase |
| Properties | PascalCase |
| Events | PascalCase (Past Tense) |
| Event Handlers | Handle + PascalCase |
| Constants | PascalCase |
| Variables | camelCase |
| Parameters | camelCase |
| Local Variables | camelCase |
| Namespaces | PascalCase |
| Files | PascalCase |
| Folders | PascalCase |
| Scenes | Numeric Prefix + PascalCase |
| Git Branches | kebab-case |
