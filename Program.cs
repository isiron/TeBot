﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace TeBot
{
    class Program
    {
        private static readonly string DATA_LOCATION = (new FileInfo(AppDomain.CurrentDomain.BaseDirectory)).Directory.FullName;

        private readonly IConfiguration config;

        private DiscordSocketClient client;
        private CommandHandler commandHandler;
        private CommandService service;

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public Program()
        {
#if DEBUG
            string jsonPath = Path.Combine(DATA_LOCATION, @"stagingconfig.json");
#else
            Console.WriteLine("Looking for Json and DB in: " + DATA_LOCATION);
            string jsonPath = Path.Combine(DATA_LOCATION, @"config.json");
#endif

            // Create and build the configuration and assign to config          
            config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(path: jsonPath)
                    .Build();
        }

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();

            // Add the command service to the collection
            service = new CommandService(new CommandServiceConfig
            {
                // Tell the logger to give Verbose amount of info
                LogLevel = LogSeverity.Verbose,

                // Force all commands to run async by default
                DefaultRunMode = RunMode.Async,
            });

            SQLManager sqlManager = new SQLManager(CreateConnection());

            commandHandler = new CommandHandler(client, service, config, sqlManager);

            client.Log += Log;
            await client.LoginAsync(TokenType.Bot, config["Token"]);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        static SQLiteConnection CreateConnection()
        {
            // Create a new database connection:
            SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=TeDB.db;Version=3;New=False;Compress=True;");

            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect to the database: " + ex.ToString());
            }

            return sqlite_conn;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
