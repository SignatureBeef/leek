# Leek Licensing Policy

## Summary

Leek is developed under the GNU General Public License v3.0 (GPLv3) to ensure that improvements made to the source code benefit the wider community.

However, we recognize that many teams — especially those in security or development operations — want to use Leek in production environments without needing to open-source their entire applications.

To make that possible, we publish **precompiled NuGet packages and CLI binaries under the MIT license**. This makes it safe to **use, link, and deploy Leek in proprietary or closed environments**, provided the tools are used as-is without modifications.

---

## Component Licensing Table

| Component             | License |
|-----------------------|---------|
| Source code (repo)    | GPLv3   |
| NuGet packages        | MIT     |
| CLI binaries          | MIT     |
| Modified forks        | GPLv3   |

---

## Usage Scenarios

### ✅ You Can:
- Use the NuGet packages in your ASP.NET Core, console, or internal tooling projects.
- Deploy the Leek CLI tool in automation or DevSecOps pipelines.
- Write your own providers or integrations upon NuGet libraries and **keep them private**, <i> however we strongly urge contributions back.</i>
- Help grow the project by starring the repo, reporting bugs, suggesting improvements, or mentioning Leek in your docs, blog posts, or social media.


### 🚫 You Cannot:
- Modify the Leek **source code** and redistribute it without also releasing your changes under GPLv3.
- Repackage Leek as your own product or SaaS without explicit permission.

