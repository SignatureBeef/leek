// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using DemoWebApp.Data;
using Leek.AspNet;
using Leek.Core.Providers;
using Leek.Core.Services;
using Leek.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddPasswordValidator<LeekPasswordValidator<IdentityUser>>();
builder.Services.AddRazorPages();

// bind leek to the existing ef connection, and throw in a wordlist for a bonus example.
ConnectionContext sqliteConnection = new("sqlite", connectionString);
builder.Services.Configure<LeekPasswordValidatorOptions>(opt =>
{
    var wordlist = "example-wordlist.txt"; // Replace from config, or use a real wordlist file.
    File.WriteAllLines(wordlist, ["password", "123456", "letmein", "qwerty"]);
    opt.Connections = [
        sqliteConnection,
        new ConnectionContext("wordlist", wordlist), // nb i wouldnt really recommend this in a production instance, rather you would copy it into a database or use hibp's provider.
        new ConnectionContext("hibp", "") // this uses the HIBP k-Anonymity provider, which does not require a connection string.
    ];
});

// register leek services needed for the connections defined above.
builder.Services.AddLeekServices()
    .AddDatabaseProvider()
    .AddHIBPProvider()
    .AddWordlistReader()
    .AddWordlistProvider();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// migrate the database if needed
using (var scope = app.Services.CreateScope())
{
    // migrate the leek db first - no migrations currently configured and only works with a new db
    var databaseProvider = scope.ServiceProvider.GetRequiredService<IEnumerable<IDataProvider>>().First(x => x is DatabaseProvider)
        ?? throw new InvalidOperationException("No database provider found.");

    using var ctx = ((DatabaseProvider)databaseProvider).CreateDbContext(sqliteConnection);
    await ctx.Database.EnsureCreatedAsync();

    // asp supports migrations, so add over leek
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
