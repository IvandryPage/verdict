# Commit Convention

The project follows the Conventional Commits specification.

---

# Format

```
<type>: <description>
```

Example

```
feat: implement evidence system

fix: resolve statement progression bug

docs: update README

chore: initialize project architecture
```

---

# Types

| Type | Usage |
|------|-------|
| feat | New feature |
| fix | Bug fix |
| docs | Documentation |
| refactor | Code restructuring |
| perf | Performance improvements |
| test | Testing |
| chore | Setup, tooling, maintenance |
| ci | CI/CD |

---

# Rules

- Use imperative mood.
- Use lowercase type.
- Keep subject under 72 characters.
- Do not end with a period.

Good

```
feat: add evidence evaluation
```

Bad

```
Added Evidence
```

---

# Commit Frequency

Commit logical units of work.

Avoid massive commits covering unrelated changes.

---

# Branches

```
feature/data-model

feature/courtroom-flow

feature/ui-framework

feature/audio-system
```

One feature branch should correspond to one parent issue whenever possible.
