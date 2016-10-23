using System;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot.Module
{
    internal class Information : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private Data.TextInformation info;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;
            info = new Data.TextInformation();

            manager.CreateCommands("", group =>
            {
                group.PublicOnly();

                group.CreateCommand("howLong")
                .Description("Returns the date you joined the server")
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"{e.User.Name} joined {e.Server.Name} on {e.User.JoinedAt}");
                });

                group.CreateCommand("joinServer")
                .Description("Make me join one of your servers, hurray.")
                .Hide()
                .Do(async e =>
                {
                    await e.Channel.SendMessage("https://discordapp.com/oauth2/authorize?client_id=212975561759391744&scope=bot&permissions=66186303");
                });

                group.CreateCommand("con")
                .Description("Converts your text.")
                .Parameter("Text", ParameterType.Unparsed)
                .Hide()
                .Do(async e =>
                {
                    char[] text = e.Args[0].ToCharArray();
                    int[] ascii = new int[text.Length];
                    for(int i = 0; i < text.Length; i++)
                    {
                        ascii[i] = (int)text[i];
                    }
                    string output = "";
                    foreach(int ASC in ascii)
                    {
                        output += Convert.ToString(ASC, 2) + " ";
                    }

                    await e.Channel.SendMessage(output);
                });

                group.CreateCommand("getStats")
                .Description("Returns your: Score, Level/Progression, Experience")
                .Do(async e =>
                {
                    int curLevel = Game.findDataUser(e.User).Level;
                    await e.Channel.SendMessage($"${Game.findDataUser(e.User).Score}\n" +
                                                $"Level: {curLevel} (Experience Bar: {Game.findDataUser(e.User).calcNextLevel()})\n" +
                                                $"EXP: {Game.findDataUser(e.User).Experience}\n");
                });
            });
        }
        public static string readURL(string URL)
        {
            string s = "";
            try
            {
                var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(URL);
                using (var response = request.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new System.IO.StreamReader(content))
                {
                    s = reader.ReadToEnd();
                }
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return s;
        }
    }
}
