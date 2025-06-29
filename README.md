# 🥬 Leek - Hash Auditor for Security Hygiene
[![Build and Test](https://github.com/SignatureBeef/leek/actions/workflows/test.yml/badge.svg)](https://github.com/SignatureBeef/leek/actions/workflows/test.yml) [![Source: GPL v3](https://img.shields.io/badge/Source-GPL%20v3-blue)](https://www.gnu.org/licenses/gpl-3.0) [![Binaries: MIT](https://img.shields.io/badge/Binaries-MIT-blue)](https://opensource.org/license/MIT)

**Leek** is a CLI and .NET toolset for detecting breached or weak secrets such as passwords.  
Inspired by _"There's a leek in the boat!" -Cloudy with a Chance of Meatballs_, it helps you **plug security holes before your system sinks.**

📦 **.NET API** – Integrate into your applications for real-time hash auditing and response.
<br/>
💻 **CLI Tool** – Use via terminal to import, scan, and analyze existing hash databases.

### Features

- 🔐 Hash-based breach detection <small>(e.g. SHA1)</small>
- 🛠️ Wordlist imports and reading
- ⚙️ EF Core integration with flexible data access <small>(support list below)</small>
- 🔄 Works online or offline — leverage databases, APIs, or wordlists from local or remote sources
- 🌐 ASP.NET demo web app included
<!-- - 🐳 Docker support for containerized deployment -->


### Use cases
- Enforce secure password policies by blocking weak or breached credentials at login, signup, or during password changes.
- Audit stored password hashes to proactively identify accounts at risk before a breach occurs.
- Integrate into CI/CD pipelines to prevent known or default credentials from being used in development or deployments.
- Automate hash database updates by syncing with trusted online breach or wordlist sources.

## Philosophy

Leek focuses on the **hash** — not the password, and this means:
- Any hashed value can be audited (not just passwords)
- It works across various platforms and runtimes, while allowing extensibility.
- It integrates where you need it — login-time, scan-time, or CI/CD-time

---

## Supported Data Providers

Leek is built using one or more Data Provider5 that can be injected at runtime — below are the defaults supported:

- Microsoft SQL Server (`mssql`)
- SQLite (`sqlite`)
- File store (`directory`)
- [Have I Been Pwned k-Anonymity search](https://haveibeenpwned.com/API/v3#SearchingPwnedPasswordsByRange) (`hibp`)
- Wordlists (`wordlist`)

## Frequently Asked Questions
### How does it work, will my secrets be exposed?
By default Leek will always assume inputs are secret and will produce a hash for each algorithm, which will subsequently be used to check with the local or remote databases.

### What if my input is already hashed?
You would instead supply a filter to the check command or API you are using. This will allow you to provide a flag to indicate what your hash algorithm is, allowing efficient and secure auditing.

### Have a suggestion or issue?
Please ensure you check existing issues first! You're welcome to also read the [CONTRIBUTING.md](CONTRIBUTING.md) if you are interested in contributing to the project.

---

## CLI Usage

```bash
# Show help
leek -h
# Show the current apps full version.
leek --version

# Check a secret or hash
leek check <secret> [--type=<enter>] -p=<provider>[://<connection>]
# e.g.
# check test (check if test is found in the defaults)
# check test -p=hibp -p="sqlite://Data Source=leek.db" (check specific providers)

# Copies hashes from one provider/connection to another
leek copy -fp=<provider>[://<connection>] -tp=<provider>[://<connection>]

# Loads online hash data into the destination provider/connection.
# nb. this could take hours-days until our sources allow differentials
leek update -p=<provider>[://<connection>]
```

## API Usage

```C#
// register services with DI
builder.Services.AddLeekServices()
    .AddDefaultServices(); // adds all providers, e.g. HIBP, db, directory
// configure your options
builder.Services.Configure<MyOptions>(); // customise as needed

// inject and use the IAuditor interface
public class Example(IAuditor auditor, IOptions<MyOptions> options)
{
    async Task CheckSecret(string secret)
    {
        LeekSearchResponse response = await auditor.SearchBreaches(options.Value.Connections, new LeekSearchRequest(secret));
        // do something with response.IsBreached
        Console.WriteLine($"Secret is{(response.IsBreached ? "" : " not")} breached");
    }
}
public class MyOptions
{
    public required ConnectionContext[] Connections { get; set; }
}

```

See [here](./demo/webapp/Areas/Identity/Pages/Account/Login.cshtml.cs) and [here](demo/webapp/Program.cs) for an example use in a ASP.NET application.

## External Hash Sources
Leek currently supports the following online sources:
- [Have I Been Pwned](https://haveibeenpwned.com/)
- [SecLists by Daniel Miessler](https://github.com/danielmiessler/SecLists/tree/master/Passwords/Leaked-Databases)

Of course, online features are optional and instead you can read from wordlists you may already have, or existing databases if they match the Leek schema (table or view adapter).

<!-- ### Docker
```bash
docker build -t leek-cli .
docker run --rm leek-cli check hunter2
``` -->

---

<!-- ## Flexible Scan Modes

Leek supports two main scanning strategies:

1. **Local Join Mode**: When your app's users and Leek wordlist live on the same DB engine (best perf)
2. **Remote Fetch Mode**: When databases are separate (via secure TLS connection)

In Remote Mode, you can:
- Pull hash batches from app → check in Leek
- OR pull breach hashes from Leek → check in app

You choose the direction. Configure what's safest or fastest for your environment.

--- -->

## Considerations

Leek is built with extensibility in mind. Future enhancements may include:

- `--output json|csv|sarif`: For use with audit tooling & CI pipelines
- Integration with external scanning tools and log systems
- Auto-redaction alerts (log scanning)
- Agent/daemon mode for long-running environments
- Table/schema inference via adapters (e.g. WordPress, Laravel, etc.)
- Additional hash methods, e.g. NTLM

---

## License and more

Leek is licensed under the **GPLv3** for source code to protect community contributions.

However, to make it easier for real-world adoption:

- **All official NuGet packages and CLI binaries are MIT licensed**
- This means you can safely use them in closed-source projects, CI pipelines, and internal systems
- All we ask for is a star, mention, or shout-out to help raise awareness

| Component             | License |
|-----------------------|---------|
| Source code (this repo) | GPLv3 |
| NuGet packages (e.g., Leek.Core) | MIT |
| CLI binaries           | MIT |
| Modifying the source   | GPLv3 applies |

For full policy details, see [`LICENSE_POLICY.md`](./LICENSE_POLICY.md)

> ⚠️ **Note**: While Leek is not a certified security product, it provides practical tooling to help integrate hash auditing into existing security workflows and improve the overall posture of .NET applications.  
> It’s designed to be accessible and extensible, making it easier for teams to adopt better security hygiene — especially when faced with limited time, budget, or tooling.

No warranties or official support provided. Use ethically at your own risk.
