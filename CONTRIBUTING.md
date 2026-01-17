# Contributing Guidelines

This document describes how we work in this repository. It is meant to remove friction for new contributors and enforce discipline in Issues, commits, and Pull Requests.

## 1. Getting started

### Clone the repository
```bash
git clone <YOUR_REPO_URL>
cd <YOUR_REPO_FOLDER>
```

## Running project using dev Container 
Requirements:
- Docker Desktop
- Visual Studio Code
- VS Code extension: “Dev Containers”

Steps:
- Open the repository folder in VS Code
- Run: Dev Containers: Reopen in Container
- Once the container is ready, run:
```bash
dotnet restore
dotnet run --project src/MyTemplate.Api/MyTemplate.Api.csproj
```

API endpoint:
- GET http://localhost:5080/health

## Running locally 
Requirements:
- .NET SDK 8.x
```bash
dotnet restore
dotnet run --project src/MyTemplate.Api/MyTemplate.Api.csproj
```

--- 

## 2. Branching Rules

We use **GitHub Flow**.

### Rules
* **Always branch off `main`**
* **Keep branches short-lived**
* **Prefer one Issue per branch**
* **Merge through Pull Requests (PRs)**

### Branch Naming
Use the following prefixes for your branch names:

* `feature/<short-description>`
* `bugfix/<short-description>`
* `docs/<short-description>`
* `chore/<short-description>`

#### Examples
* `feature/add-labeler-workflow`
* `docs/improve-readme-getting-started`

--- 
## 3. Commit Message Guidelines

We use clear, imperative commit messages.

* **Start with a verb:** Use actions like “Add”, “Fix”, “Update”, or “Remove”.
* **Be specific:** Avoid vague messages like “stuff” or “updates”.
* **Reference Issues:** In PR descriptions, use keywords to link work.
  * *Example:* `Fixes #12` or `Closes #34`

#### Examples
* `Add PR template`
* `Fix health endpoint response`
* `Update devcontainer instructions`

---

## 4. How to Create a Pull Request

1. **Pick an Issue:** Open a new GitHub Issue or pick an existing one.
2. **Branch out:** Create a new branch from `main`.
3. **Commit:** Save changes in small, meaningful increments.
4. **Push:** Push your branch to GitHub.
5. **Open PR:** Use the provided PR template.
6. **Link Issue:** Include the issue reference (e.g., `Fixes #12`).
7. **Request Review:** Note that `CODEOWNERS` may auto-assign reviewers.
8. **Iterate:** Address any review comments or requested changes.
9. **Merge:** Merge once approved (branch protection may be enforced).

> **Note:** Use **Draft PRs** for work-in-progress to communicate status early and get feedback before the code is finished.

---

## 5. Code Style Rules

* **Keep changes small:** Smaller PRs are easier to review and less likely to break things.
* **Separation of Concerns:** Avoid mixing refactoring with new feature changes in the same PR.
* **Consistency:** Prefer consistent formatting throughout the project.
* **Tooling:** If a formatter or linter is introduced, all subsequent changes must follow it.

---

## 6. Expected Workflow

1. **Create Issue** → Define the work to be done.
2. **Project Board** → Add the Issue to the Project board.
3. **Branch** → Create a branch from `main`.
4. **Implement** → Write code and commit.
5. **Open PR** → Open as a Draft if WIP; link the relevant Issue.
6. **Review** → Peer review and approvals.
7. **Merge** → PR is merged into `main`.
8. **Done** → Issue closes and moves to "Done" automatically.

--- 
