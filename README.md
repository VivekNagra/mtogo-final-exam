# Large Systems Collaboration Template (Udvikling af Store Systemer)

This repository is a **GitHub template repository** created for the class assignment in *Udvikling af Store Systemer*.  
The goal is to demonstrate a modern collaboration setup using GitHub: **Issues + Project board**, **Pull Requests**, **reviews**, **branch rules**, **automation**, and **lightweight metrics**.

> I worked alone, so I used a secondary GitHub account to demonstrate “real” PR approvals and CODEOWNER review.

Link to project board: https://github.com/users/VivekNagra/projects/3

### proof of change

---

## 1. About the template

This template provides a ready-to-reuse baseline that includes:

- A simple .NET solution (`src/` + `tests/`)
- Standard collaboration scaffolding:
  - Issue templates
  - PR template
  - CODEOWNERS
  - Labels + label automation
  - Protected `main` via Ruleset
  - CI workflow
  - Project automation for Issue status
  - A weekly metrics summary workflow

---

## 2. How to use this template

1. Click **Use this template** on GitHub (creates a new repository from this one).
2. Clone your new repo:
   ```bash
   git clone https://github.com/VivekNagra/large-systems-collab-template.git
   ```
3. Start work by creating an Issue, adding it to the Project board, and opening PRs using the provided templates.

---
### 3. Repository Structure

```text
.github/
├── ISSUE_TEMPLATE/           # Issue forms and templates
├── pull_request_template.md  # Pull Request template
├── CODEOWNERS                # Required reviewers for specific paths
├── labeler.yml               # Configuration for automatic labeling
└── workflows/                # GitHub Actions (CI, Metrics, Board automation)
.devcontainer/                # Optional development container configuration
src/                          # Application source code (.NET solution)
tests/                        # Unit and integration test projects
```
---

## 4. Workflow: Issues → Project → PR → Review → Merge

### 4.1 Issues
Create Issues using the provided templates (**Bug** or **Feature**). To keep the repository organized, apply labels consistently:

* **type:*** (e.g., `type:docs`, `type:automation`, `type:bug`, `type:feature`)
* **area:*** (e.g., `area:docs`, `area:api`, `area:tests`, `area:github`)
* **priority:*** (e.g., `priority:high`, `priority:low`)

### 4.2 Project Board
* **Project Name:** `@VivekNagra’s template project`
* **Workflow Columns (Status):** `Backlog` → `Ready` → `In Progress` → `In Review` → `Done`

### 4.3 PR Discipline
All changes must go through **Pull Requests**. To enable automation, PRs should reference an Issue using "closing keywords" in the description:
> **Example:** `Fixes #123`

This keyword triggers the automation that moves the linked Issue across the Project board automatically.



---

## 5. Branching Strategy

To maintain code quality, the `main` branch is **protected**. All work happens in short-lived branches:

* `feature/...` for new functionality.
* `docs/...` for documentation updates.
* `fix/...` for bug fixes.
* `chore/...` for maintenance or configuration.

**Merge Process:** Changes are only merged into `main` via PR after passing all required status checks and receiving the necessary approvals.

--- 

## 6. Automation

### 6.1 PR Label Automation (Labeler)
We use the **GitHub Labeler Action** to automatically apply labels based on the file paths changed in a PR. This ensures consistent categorization without manual effort.
* **Workflow:** `.github/workflows/labeler.yml`
* **Rules:** `.github/labeler.yml`
* *Example:* Changes to files in the `docs/` folder automatically receive the `type:docs` label.

### 6.2 Project Board Automation (PR Lifecycle → Issue Status)
A custom GitHub Action manages the movement of cards on the Project board to keep the team's status up to date.
* **PR Opened/Updated:** Moves linked Issue to `In Progress`.
* **PR Marked Ready for Review:** Moves linked Issue to `In Review`.
* **PR Merged:** Moves linked Issue to `Done`.
* **Workflow:** `.github/workflows/project-board.yml`

> **Note:** This workflow utilizes a Repository Secret (PAT) to interact with the Projects v2 API.

### 6.3 CI (Required Status Check)
To prevent breaking changes, the Continuous Integration (CI) pipeline runs on every PR and push to `main`. It builds the solution and runs all tests.
* **Workflow:** `.github/workflows/ci.yml`

### 6.4 Metrics Summary Workflow
A scheduled (weekly) and manually triggered workflow that provides transparency into team velocity by printing a summary of merged and open PRs.
* **Workflow:** `.github/workflows/metrics.yml`

---

## 7. Protected main (Ruleset)

The `main` branch is secured using a **GitHub Ruleset** to enforce collaboration standards. Direct pushes are disabled, and the following "gates" must be passed:

1.  **Pull Request Required:** All changes must come through a PR.
2.  **Required Approvals:** At least **1 approval** from a peer.
3.  **CODEOWNER Review:** A review from the designated code owner is mandatory.
4.  **Status Checks:** The **CI workflow** must pass successfully before merging.

--- 
## 8. Metrics (Task 9)

We track lightweight collaboration metrics to reflect on workflow quality and team velocity.

### 8.1 Metrics Table (PR Evidence)
The table below tracks the lifecycle of each task from Issue creation to PR merge.

| Item | Issue | Issue Created (UTC) | PR | PR Merged (UTC) | Lead Time |
| :--- | :---: | :--- | :---: | :--- | :--- |
| Issue templates | # 3 |  | #11 | 2026-01-15T00:02:21Z | <calc> |
| CODEOWNERS + PR template | # 12 |  | #12 | 2026-01-15T00:16:49Z | <calc> |
| PR labeler workflow | #13 |  | #13 | 2026-01-15T00:27:38Z | <calc> |
| Labeler config fix (v5) | #15 |  | #15 | 2026-01-15T01:12:29Z | <calc> |
| CI workflow | #17  |  |  | 2026-01-15T01:20:05Z | <calc> |
| Metrics table + workflow | #<ID> |  | #19 | 2026-01-15T01:44:01Z | <calc> |
| Metrics fixes | #18 |  | #21 | 2026-01-15T01:57:56Z | <calc> |
| Project board automation | #26 |  | #22 | 2026-01-15T02:45:31Z | <calc> |

* **Lead Time:** The duration from Issue creation to PR merge.
* **Throughput:** Number of PRs merged per week (tracked via the Metrics Summary workflow).

### 8.2 Metrics Summary workflow output (evidence)

The repository includes a workflow that prints a lightweight PR throughput summary to the workflow logs:

- Workflow file: `.github/workflows/metrics.yml`
- How to run right now: **manually** via GitHub Actions  
  **Actions → Metrics Summary → Run workflow**
- The workflow is also scheduled weekly, but for the assignment evidence I run it manually and record the output.

#### Latest manual run output (example)

Repository: `VivekNagra/large-systems-collab-template`  
Time window: last 7 days  
Merged PRs since `2026-01-08`:

- #26 Add conditional check for repository in project board automation (merged: 2026-01-15T09:48:44Z)
- #24 Add automation proof line (merged: 2026-01-15T09:50:40Z)
- #22 Automate Project status updates from PR events (merged: 2026-01-15T02:45:31Z)
- #21 Fix metrics workflow to run gh against explicit repo (merged: 2026-01-15T01:57:56Z)
- #20 Fix metrics workflow to use ISO merge date filter (merged: 2026-01-15T01:50:55Z)
- #19 Add weekly metrics summary workflow (merged: 2026-01-15T01:44:01Z)
- #18 Add metrics table to README (merged: 2026-01-15T01:38:12Z)
- #17 Add CI workflow for build and tests (merged: 2026-01-15T01:20:05Z)
- #16 Add final labeler proof doc (merged: 2026-01-15T01:14:06Z)
- #15 Fix labeler config to v5 format (merged: 2026-01-15T01:12:29Z)
- #13 Add PR labeler workflow (merged: 2026-01-15T00:27:38Z)
- #12 Add CODEOWNERS and pull request template (merged: 2026-01-15T00:16:49Z)
- #11 Add issue templates for bug reports and feature requests (merged: 2026-01-15T00:02:21Z)

Open PRs: (none at the time of this run)

---

## 9. Requirement Mapping

This section maps the assignment requirements to specific features implemented in this repository.

| Requirement | Implementation / Evidence |
| :--- | :--- |
| **README + Documentation** | `README.md` and `CONTRIBUTING.md` |
| **Project Board** | @VivekNagra’s template project (v2) |
| **Board Automation** | `.github/workflows/project-board.yml` |
| **Issue Templates** | `.github/ISSUE_TEMPLATE/` (Bug & Feature) |
| **PR Workflow** | `.github/pull_request_template.md` |
| **PR Discipline** | Multiple PRs linked to Issues; reviews enforced |
| **Protected Main** | Ruleset requiring 1 approval, CODEOWNER, & CI |
| **Label Automation** | `.github/workflows/labeler.yml` + `.github/labeler.yml` |
| **Metrics Implementation** | Section 8.1 and `.github/workflows/metrics.yml` |
| **Template Repository** | Repository is configured as a GitHub Template |

---

## 10. Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed collaboration guidelines.

## 11. License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.