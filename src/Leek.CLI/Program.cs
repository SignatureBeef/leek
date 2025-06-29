// Copyright © 2025 Leek contributors
// SPDX-License-Identifier: GPL-3.0-or-later
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Leek.CLI.Commands;
using Microsoft.Extensions.Hosting;
using Leek.Updater;
using Leek.Services;
using Microsoft.Extensions.Logging;

Console.WriteLine("Leek CLI - is there a leek in your system?");
Console.WriteLine($"Search and manage known bad hashes with ease.");
Console.WriteLine("This software is free and open source under a dual MIT/GPL-3.0 license. Use at your own risk.");

RootCommand rootCommand = new()
{
    Name = "leek",
    Description = "A simple command line tool to check secrets against known bad hashes.",
};

rootCommand.AddCommand(new CheckCommand());
rootCommand.AddCommand(new UpdateCommand());
rootCommand.AddCommand(new CopyCommand());

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHost((host) =>
    {
        host.ConfigureServices(services =>
        {
            services.AddLeekServices()
                .AddDefaultServices()
                .AddUpdateService();
        });

        host.ConfigureLogging((ctx, builder) =>
        {
            builder.AddConsole()
                .SetMinimumLevel(LogLevel.Information);
        });

        host.UseCommandHandler<CheckCommand, CheckCommandHandler>();
        host.UseCommandHandler<UpdateCommand, UpdateCommandHandler>();
        host.UseCommandHandler<CopyCommand, CopyCommandHandler>();
    })
    .Build();

return await parser.InvokeAsync(args);
// await File.WriteAllLinesAsync("test.txt", ["test1", "test2", "test3"]);
// return await parser.InvokeAsync("check test -p=hibp -p=\"sqlite://Data Source=leek.db\"");
//return await parser.InvokeAsync("check test -p=mssql -p=sqlite -p=hibp -p=wordlist://test.txt");

// return await parser.InvokeAsync("check 6677b2c394311355b54f25eec5bfacf5 -t=ntlm -p=hibp");
// return await parser.InvokeAsync("-h");
// return await parser.InvokeAsync("--version");
// return await parser.InvokeAsync("check test -p=wordlist -cs=test.txt");
// return await parser.InvokeAsync("copy -fp=wordlist -fc=test.txt -tp=sqlite");
// return await parser.InvokeAsync("check test -p=hibp");
// return await parser.InvokeAsync("check test -p=hibp");
// return await parser.InvokeAsync("import --type=wordlist test.txt");
// return await parser.InvokeAsync("check redhouse --provider=sqlite");
// return await parser.InvokeAsync("check redhouse --provider=mssql");
//return await parser.InvokeAsync("check \"redhouse\"");
// return await parser.InvokeAsync("check --type=sha1 redhouse");
// return await parser.InvokeAsync("check --type=sha1 test");
// return await parser.InvokeAsync("check test -p=wordlist -pc=test.txt ");
// return await parser.InvokeAsync("check test");
//return await parser.InvokeAsync("check test --provider=directory");
// return await parser.InvokeAsync("check --type=md5 test");
// return await parser.InvokeAsync("update --provider=mssql");
// return await parser.InvokeAsync("update --database=sqlite");
//return await parser.InvokeAsync("update");
// return await parser.InvokeAsync("copy -fp=directory -tp=mssql -fcs=F:\\filestore");
//return await parser.InvokeAsync("copy -fp=directory -tp=sqlite");


// return await parser.InvokeAsync("check");
// return await parser.InvokeAsync("");

// return await rootCommand.InvokeAsync("");
// return await rootCommand.InvokeAsync("check --type=md5 test");

// return await parser.InvokeAsync("check --type=md5 test");
// return await parser.InvokeAsync("check");
// return await parser.InvokeAsync(args);

// return await rootCommand.InvokeAsync("check --type=md5 test");
// return await rootCommand.InvokeAsync("check test");
// return await rootCommand.InvokeAsync("check");
// // return await rootCommand.InvokeAsync(""); // root command
// // return await rootCommand.InvokeAsync("-h");
// // return await rootCommand.InvokeAsync("--version");
// // return await rootCommand.InvokeAsync(args);

