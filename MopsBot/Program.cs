using Discord;
using Discord.Commands;
using Discord.Modules;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot
{
    class Program
    {
        static void Main(string[] args)
        {
            //Process process = new Process();
            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //startInfo.FileName = @"C:\Users\May\Desktop\Mops_Music\playlist.py";
            //process.StartInfo = startInfo;
            //process.Start();

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
            _client.AddModule<Module.Music>("Music");

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect("MjEyOTc1NTYxNzU5MzkxNzQ0.C39SUg.Wv0w8yRhzyX - _4LFjbrQUjCL - 8o", TokenType.Bot);
            });

        }

        public void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine($"{e.Message} {e.Source} {e.Exception}");
        }
    }
}
