using Discord;
using Discord.Commands;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Start();

        }

        private DiscordClient _client;

        public void Start()
        {
            _client = new DiscordClient(x => { x.LogHandler = Log; x.LogLevel = LogSeverity.Info; }).UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
            })
           .UsingModules();

            _client.AddModule<Module.Information>("Information");
            _client.AddModule<Module.Game>("Game");
            _client.AddModule<Module.DataBase>("Data");
            _client.AddModule<Module.Moderation>("Moderation");


            var token = "Bot MjEyOTc1NTYxNzU5MzkxNzQ0.CrQ03A.z_k75FXO07-PJxb4Wmb0xZwJ-ZM";

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect(token);
            });

        }

        public void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine($"{e.Message}");
        }
    }
}
