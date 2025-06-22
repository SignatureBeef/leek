# Contributing to Leek 🥬

Thank you for your interest in contributing — whether through code, ideas, testing, or documentation.  
**Leek** is a community-driven project focused on improving password and hash auditing tools for .NET and CLI users.

---

## New to security or open source?

You’re welcome here.

Leek is built to be understandable, extensible, and helpful — for beginners and veterans alike.  
Don’t hesitate to open issues or PRs, ask questions, or suggest improvements.  
Leek is what we make of it, so if you have skills or experience, we encourage you to help make Leek better.

---

## What to expect from this project

Leek is actively developed but maintained on a **as-needed, best-effort basis**.  
Please understand that:

- **Response times may vary**, especially for larger issues or contributions.
- **Bug reports and feature suggestions are valued** but may not always be addressed immediately.
- **Pull requests are welcome** but may take time to review and merge depending on complexity and availability.
- Contributions that are respectful, scoped, and well-documented will always be appreciated and prioritized.
- Contributors may not be experts in every field, so community review and guidance are essential to steer Leek’s future.

This helps keep the project sustainable — thanks for understanding!

---

## How to contribute

### Bug reports and feature suggestions
- First, check [open issues](https://github.com/SignatureBeef/leek/issues) to avoid duplicates.
- Include clear steps to reproduce bugs if possible. One liners may be rejected, or be de-prioritized.
- Describe expected vs. actual behavior.
- Feature suggestions should include rationale and ideally link to real-world scenarios.

### Code contributions
1. Fork the repository and work in a branch.
2. Follow the existing project structure and coding style — though these are not yet set in stone, so strong opinions are welcome.
3. Double-check your editor config is working.
4. Include or update tests if relevant.
5. Run `dotnet test` before submitting — though we plan to automate this in future CI workflows, too.
6. Open a pull request with a meaningful description.

### Documentation & examples
Contributions to documentation are always welcome:
- Fix typos or outdated information.
- Improve CLI usage examples or integration guidance.
- Add real-world use cases or onboarding tips.

---

## Development setup

```bash
git clone https://github.com/SignatureBeef/leek.git
cd leek

dotnet build
dotnet test
