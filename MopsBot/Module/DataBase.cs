using System;
using System.Timers;
using System.Globalization;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;

namespace MopsBot.Module
{
    internal class DataBase : IModule
    {
        private Timer updater;
        public static DiscordClient _client;
        private Data.TextInformation info;
        private Data.Osu_Data osuInfo;
        private Data.Overwatch_Data owInfo;
        private string apiKey = "8ad11f6daf7b439f96eee1c256d474cd9925d4d8";


        void IModule.Install(ModuleManager manager)
        {
            _client = manager.Client;

            info = new Data.TextInformation();
            updater = new Timer(10000);
            updater.Elapsed += Updater_Elapsed;
            updater.Start();

            _client.MessageReceived += _client_MessageReceived;

                manager.CreateCommands("quote", group =>
                {
                    group.PublicOnly();

                    group.CreateCommand("get")
                .Description("Gets the Quote at Index or Random if no Index is given")
                .Parameter("Index", ParameterType.Optional)
                .Do(async e =>
                {
                    if (e.Args[0] == "") await e.Channel.SendMessage($"Quote[{new Random().Next(0, info.quotes.Count)}]:\n{info.quotes[new Random().Next(0, info.quotes.Count)]}");
                    else await e.Channel.SendMessage($"Quote[{e.Args[0]}]:\n{info.quotes[int.Parse(e.Args[0])]}");
                });

                    group.CreateCommand("search")
                    .Description("Searches all Quote containing the keyword")
                    .Parameter("keyword", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        string output = "";

                        for (int i = 0; i < info.quotes.Count; i++)
                        {
                            if (info.quotes[i].ToString().ToLower().Contains(e.Args[0].ToLower())) output += $"Quote[{i}]:\n{info.quotes[i]}\n\n";
                        }

                        await e.Channel.SendMessage(output);
                    });

                    group.CreateCommand("add")
                    .Description("Adds a Quote at the end of the List")
                    .Parameter("Quote", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        info.quotes.Add($"{e.Args[0]}");
                        info.writeInformation();
                        await e.Channel.SendMessage($"Added Quote[{info.quotes.Count - 1}]");
                    });

                    group.CreateCommand("de")
                    .Description("Removes Quote at Index")
                    .Parameter("Index")
                    .Do(async e =>
                    {
                        info.quotes.RemoveAt(int.Parse(e.Args[0]));
                        info.writeInformation();
                        await e.Channel.SendMessage($"Removed Quote[{e.Args[0]}]");
                    });
                });

            manager.CreateCommands("", group =>
            {
                group.CreateCommand("giveCookie")
                .Description("You make me survive for a few minutes longer!")
                .Do(async e =>
                {
                    info.cookies++;
                    info.writeInformation();
                    await e.Channel.SendMessage($"Up to now, I have been fed {info.cookies} cookies!");
                });
            });

            manager.CreateCommands("osu", group =>
            {
                group.CreateCommand("user")
                .Description("test")
                .Parameter("name")
                .Do(async e =>
                {

                    var dict = Module.Data.osuUser.userStats(e.GetArg("name"));

                    try { await e.Channel.SendMessage($"User: {dict["username"]} \nPP: {dict["pp_raw"]}\n\nhttps://osu.ppy.sh/u/{dict["user_id"]}"); } catch (Exception ex) { await e.Channel.SendMessage(ex.Message); }
                });

                group.CreateCommand("signup")
                .Description("Signs you up into the Osu! Database")
                .Parameter("name")
                .Do(async e =>
                {
                    if (osuInfo.osuUsers.Exists(x => x.discordID == e.User.Id))
                        osuInfo.osuUsers.Find(x => x.discordID == e.User.Id).channels.Add(e.Channel);

                    else
                    {
                        var dict = Module.Data.osuUser.userStats(e.GetArg("name"));

                        osuInfo.osuUsers.Add(new Data.osuUser(e.User.Id, dict["user_id"], e.Channel));
                    }

                    osuInfo.writeInformation();

                    await e.Channel.SendMessage($"Signed you up on {e.Channel.Id} ({e.Channel.Name})\n" +
                                                $"Keeping track of your **{osuInfo.osuUsers.Find(x => x.discordID == e.User.Id).modeToString()}** pp.\n" +
                                                $"Change the game mode by using the !setMainMode command.");
                });

                group.CreateCommand("setMainMode")
                .Description("0 = Standard, 1 = Taiko, 2 = CtB, 3 = Mania")
                .Parameter("Mode")
                .Do(async e =>
                {
                    if (!osuInfo.osuUsers.Exists(x => x.discordID == e.User.Id)) return;

                    osuInfo.osuUsers.Find(x => x.discordID == e.User.Id).mainMode = "m=" + e.GetArg("Mode");
                    osuInfo.osuUsers.Find(x => x.discordID == e.User.Id).updateStats();

                    osuInfo.writeInformation();

                    await e.Channel.SendMessage("Set mode to " + e.GetArg("Mode"));
                });

                group.CreateCommand("getStats")
                .Description("Fetches and updates your Data from the DataBase")
                .Do(async e =>
                {
                    Data.osuUser OUser = osuInfo.osuUsers.Find(x => x.discordID == e.User.Id);
                    OUser.updateStats();

                    await e.Channel.SendMessage($"Name: {OUser.username}\nScore: {OUser.score} ({Math.Round(OUser.accuracy, 2)}%)\nPP: {OUser.pp}  {OUser.mainMode}\n\nhttps://osu.ppy.sh/u/{OUser.ident}");
                });

                group.CreateCommand("ranking")
                .Description("returns pp leaderboard")
                .Do(async e =>
                {
                    await e.Channel.SendMessage(osuInfo.drawDiagram(e.Server));
                });
            });

            manager.CreateCommands("overwatch", group =>
                {
                    group.CreateCommand("user")
                    .Description("Returns user stats")
                    .Parameter("Battletag")
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage(Data.OW_User.statsToString(e.GetArg("Battletag")));
                    });

                    group.CreateCommand("signup")
                    .Description("In development")
                    .Parameter("Battletag")
                    .Do(async e =>
                    {
                        if (owInfo == null) await e.Channel.SendMessage("Try again in around a minute o3o");

                        if (owInfo.OW_Users.Exists(x => x.discordID == e.User.Id))
                            owInfo.OW_Users.Find(x => x.discordID == e.User.Id).channels.Add(e.Channel);

                        else
                        {
                            owInfo.OW_Users.Add(new Data.OW_User(e.User.Id, e.GetArg("Battletag"), e.Channel));
                        }

                        owInfo.writeInformation();

                        await e.Channel.SendMessage($"Signed you up on {e.Channel.Id} ({e.Channel.Name})\n" +
                                                    $"Keeping track of your **IN DEVELOPMENT**");
                    });

                    group.CreateCommand("getStats")
                    .Description("Returns your OW stats")
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage(owInfo.OW_Users.Find(x => x.discordID == e.User.Id).statsToString());
                    });

                    group.CreateCommand("ranking")
                    .Description("Returns top limit ranked users")
                    .Parameter("Limit")
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage(owInfo.drawDiagram(int.Parse(e.GetArg("Limit"))));
                    });
                });
        }

        private void Updater_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(osuInfo == null && owInfo == null)
            {
                updater.Interval = 60000;
                osuInfo = new Data.Osu_Data();
                owInfo = new Data.Overwatch_Data();
                return;
            }

            foreach (Data.osuUser OUser in osuInfo.osuUsers)
            {
                try
                {
                    if (OUser.channels[0].GetUser(OUser.discordID).Status.Value.Equals(UserStatus.Online.Value) || OUser.channels[0].Id.Equals(226499471683485697))
                    {
                        dynamic dict = Data.osuUser.userStats(OUser.ident.ToString(), OUser.mainMode);
                        if (OUser.pp + 1 <= double.Parse(dict["pp_raw"], CultureInfo.InvariantCulture))
                        {
                            string query = Information.readURL($"https://osu.ppy.sh/api/get_user_recent?u={OUser.ident}&{OUser.mainMode}&limit=1&k={apiKey}");
                            query = query.Remove(0, 1);
                            query = query.Remove(query.Length - 1, 1);

                            var jss = new JavaScriptSerializer();
                            var dict2 = jss.Deserialize<dynamic>(query);

                            string beatmap_ID = dict2["beatmap_id"];
                            query = Information.readURL($@"https://osu.ppy.sh/api/get_scores?b={beatmap_ID}&{OUser.mainMode}&u={OUser.username}&limit=1&k={apiKey}");
                            query = query.Remove(0, 1);
                            query = query.Remove(query.Length - 1, 1);

                            var dict3 = jss.Deserialize<dynamic>(query);

                            string output = $"Recieved pp change of `+{Math.Round(double.Parse(dict["pp_raw"], CultureInfo.InvariantCulture) - OUser.pp, 2)}` by **{OUser.username}**  ({Math.Round(double.Parse(dict["pp_raw"], CultureInfo.InvariantCulture), 2)}pp)\n\nOn https://osu.ppy.sh/b/{dict2["beatmap_id"]}&{OUser.mainMode}\nScore: {string.Format("{0:n0}", int.Parse(dict2["score"]))}  ({calcAcc(dict3, OUser.mainMode)}% , {dict2["maxcombo"]}x)\n{dict2["rank"]}, `{dict3["pp"]}pp`";

                            foreach (Channel ch in OUser.channels)
                                ch.SendMessage(output);

                            OUser.updateStats(dict);
                        }
                        else if (OUser.pp < double.Parse(dict["pp_raw"], CultureInfo.InvariantCulture))
                            OUser.updateStats(dict);
                    }
                }
                catch { }
            }

            foreach (Data.OW_User user in owInfo.OW_Users)
            {
                if (!user.channels[0].GetUser(user.discordID).Status.Value.Equals(UserStatus.Offline.Value)
                    && user.channels[0].GetUser(user.discordID).CurrentGame.Value.Equals("Overwatch"))
                {
                    string tracker = user.trackChange();

                    if (!tracker.Equals(""))
                        foreach (Channel ch in user.channels)
                            ch.SendMessage(tracker);
                }
            }

        }

        private void _client_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message.Text.StartsWith("https://osu.ppy.sh/b/")){
                string[] information = e.Message.Text.Split('/','?','&',' ');
                int specMods = 0;

                if (e.Message.Text.Contains("+"))
                {
                    string[] mods = e.Message.Text.Split('+');
                    foreach(string mod in mods)
                    {
                        switch (mod.ToLower().Trim(' '))
                        {
                            case "hidden":
                                specMods += 8;
                                break;
                            case "hardrock":
                                specMods += 16;
                                break;
                            case "doubletime":
                                specMods += 64;
                                break;
                            case "flashlight":
                                specMods += 1024;
                                break;
                            case "easy":
                                specMods += 2;
                                break;
                            case "halftime":
                                specMods += 256;
                                break;
                            case "nightcore":
                                specMods += 512;
                                break;
                            case "nomod":
                                specMods = 0;
                                break;
                        }
                    }
                }

                string query = Information.readURL($"https://osu.ppy.sh/api/get_beatmaps?b={information[4]}&{information[5]}&k={apiKey}");
                string query2 = Information.readURL($"https://osu.ppy.sh/api/get_scores?b={information[4]}&{information[5]}&mods={specMods}&limit=1&k={apiKey}");
                query = query.Remove(0, 1); query2 = query2.Remove(0, 1);
                query = query.Remove(query.Length - 1, 1); query2 = query2.Remove(query2.Length - 1, 1);

                var jss = new JavaScriptSerializer();
                var dict = jss.Deserialize<dynamic>(query);
                var dict2 = jss.Deserialize<dynamic>(query2);

                try { e.Channel.SendMessage($"Title: {dict["artist"]} - {dict["title"]}[{dict["version"]}]\nDifficulty: {Math.Round(double.Parse(dict["difficultyrating"], CultureInfo.InvariantCulture), 2)}\nPlayed {dict["playcount"]} times.\nMaximum Combo: {dict["max_combo"]}x\n\n#1 **{dict2["username"]}**, {dict2["pp"]}pp, Combo: {dict2["maxcombo"]}x"); } catch(Exception ex) { e.Channel.SendMessage(ex.Message); }
            }

        }

        private decimal calcAcc(dynamic dict3, string mainMode)
        {

            switch (mainMode)
            {
                case "m=3":
                    decimal pointsOfHits = (decimal.Parse(dict3["count50"]) * 50 + decimal.Parse(dict3["count100"]) * 100 + decimal.Parse(dict3["count300"]) * 300m + decimal.Parse(dict3["countkatu"]) * 200 + decimal.Parse(dict3["countgeki"]) * 300);
                    decimal numberOfHits = (decimal.Parse(dict3["count50"]) + decimal.Parse(dict3["count100"]) + decimal.Parse(dict3["count300"]) + decimal.Parse(dict3["countmiss"]) + decimal.Parse(dict3["countkatu"]) + decimal.Parse(dict3["countgeki"]));
                    decimal accuracy = (pointsOfHits / (numberOfHits * 300m));
                    return Math.Round(accuracy*100m, 2);
                case "m=2":
                    decimal fruitsCaught = (decimal.Parse(dict3["count50"]) + decimal.Parse(dict3["count100"]) + decimal.Parse(dict3["count300"]));
                    decimal numberOfFruits = (decimal.Parse(dict3["count50"]) + decimal.Parse(dict3["count100"]) + decimal.Parse(dict3["count300"]) + decimal.Parse(dict3["countkatu"]) + decimal.Parse(dict3["countmiss"]));
                    accuracy = (numberOfFruits / fruitsCaught);
                    return Math.Round(accuracy*100m, 2);
                case "m=1":
                    pointsOfHits = (decimal.Parse(dict3["countmiss"]) * 0m + decimal.Parse(dict3["count100"]) * 0.5m + decimal.Parse(dict3["count300"]) * 1m)*300m;
                    numberOfHits = (decimal.Parse(dict3["countmiss"]) + decimal.Parse(dict3["count100"]) + decimal.Parse(dict3["count300"]));
                    accuracy = (pointsOfHits / (numberOfHits * 300m));
                    return Math.Round(accuracy * 100m, 2);
                case "m=0":
                    pointsOfHits = (decimal.Parse(dict3["count50"]) * 50 + decimal.Parse(dict3["count100"]) * 100 + decimal.Parse(dict3["count300"]) * 300m);
                    numberOfHits = (decimal.Parse(dict3["count50"]) + decimal.Parse(dict3["count100"]) + decimal.Parse(dict3["count300"]) + decimal.Parse(dict3["countmiss"]));
                    accuracy = (pointsOfHits / (numberOfHits * 300m));
                    return Math.Round(accuracy*100m, 2);
            }
            return 0;
        }

        public static Channel getChannel(ulong ID)
        {
            Channel test = _client.GetChannel(219423083537235968);
            return _client.GetChannel(ID);
        }

        enum Mods
        {
            None = 0,
            NoFail = 1,
            Easy = 2,
            //NoVideo      = 4,
            Hidden = 8,
            HardRock = 16,
            SuddenDeath = 32,
            DoubleTime = 64,
            Relax = 128,
            HalfTime = 256,
            Nightcore = 512, // Only set along with DoubleTime. i.e: NC only gives 576
            Flashlight = 1024,
            Autoplay = 2048,
            SpunOut = 4096,
            Relax2 = 8192,  // Autopilot?
            Perfect = 16384,
            Key4 = 32768,
            Key5 = 65536,
            Key6 = 131072,
            Key7 = 262144,
            Key8 = 524288,
            keyMod = Key4 | Key5 | Key6 | Key7 | Key8,
            FadeIn = 1048576,
            Random = 2097152,
            LastMod = 4194304,
            FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | keyMod,
            Key9 = 16777216,
            Key10 = 33554432,
            Key1 = 67108864,
            Key3 = 134217728,
            Key2 = 268435456
        }
    }
}
