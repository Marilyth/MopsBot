using System;
using ChatterBotAPI;
using System.Web;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using MopsBot.Module.Data;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot.Module
{
    internal class Game : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private Random ran = new Random();
        public static Data.UserScore userScores;
        private Data.Session.Hangman hangman;
        private Data.Session.Bomb bomb;
        private Data.Session.Salad salad;
        private Data.Session.Scramble scramble;

        void IModule.Install(ModuleManager manager)
        {
            Random random = new Random();
            userScores = new UserScore();

            ChatterBotFactory factory = new ChatterBotFactory();

            ChatterBot _Cleverbot = factory.Create(ChatterBotType.CLEVERBOT);
            ChatterBotSession Cleverbot1session = _Cleverbot.CreateSession();

            ChatterBot bot2 = factory.Create(ChatterBotType.PANDORABOTS, "b0dafd24ee35a477");
            ChatterBotSession bot2session = bot2.CreateSession();

            _manager = manager;
            _client = manager.Client;

            _client.MessageReceived += _client_MessageReceived;

            manager.CreateCommands("", group =>
            {
                group.PublicOnly();

                group.CreateCommand("kawush")
                .Description("Makes Kabooom")
                .Parameter("User")
                .Do(async e =>
                {
                    e.Args[0] = e.Args[0].Replace("!","");
                    if (bomb == null || !bomb.active && e.Server.FindUsers(e.Args[0], false).FirstOrDefault().Status.Value.Equals("online"))
                    {
                        bomb = new Data.Session.Bomb(e.User, e.Server.FindUsers(e.Args[0]).FirstOrDefault());

                        await e.Channel.SendMessage($"{e.Server.FindUsers(e.Args[0]).FirstOrDefault().Name} is being bombed!\n" +
                                                    "Quick, find the right wire to cut!\n" +
                                                    $"({String.Join(", ", bomb.wires)})\n");
                    }
                    else await e.Channel.SendMessage("Either the User is not online, or the Command is already in use!");
                });

                group.CreateCommand("rollDice")
                .Description("Rolls a random Number between Min and Max")
                .Parameter("Min")
                .Parameter("Max")
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"[{random.Next(int.Parse(e.Args[0]), int.Parse(e.Args[1]) + 1)}]");
                });

                group.CreateCommand("Slotmachine")
                .Description("Costs 5$")
                .Do(e =>
               {
                   if (findDataUser(e.User).Score < 5)
                   {
                       e.Channel.SendMessage("Not enough money");
                       return;
                   }
                   Random rnd = new Random();
                   int[] num = { rnd.Next(0, 6), rnd.Next(0, 6), rnd.Next(0, 6) }; //0=Bomb, 1=Cherry, 2= Free, 3= cookie, 4= small, 5= big
                   bool won = true;
                   int amount = 0;

                   if (num[0] == num[1] && num[1] == num[2])
                   {
                       switch (num[0])
                       {
                           case 0:
                               won = false;
                               amount = 250;
                               break;
                           case 1:
                               amount = 40;
                               break;
                           case 2:
                               amount = 50;
                               break;
                           case 3:
                               amount = 100;
                               break;
                           case 4:
                               amount = 200;
                               break;
                           case 5:
                               amount = 500;
                               break;
                       }
                   }
                   else if ((num[0] == num[1] && num[0] == 1) || (num[0] == num[2] && num[0] == 1) || (num[1] == num[2] && num[1] == 1))
                   {
                       won = true;
                       amount = 20;
                   }
                   else if (num[0] == 1 || num[1] == 1 | num[2] == 1)
                   {
                       amount = 5;
                   }
                   if (won)
                   {
                       addToBase(e.User, amount - 5);
                   }
                   else {
                       addToBase(e.User, (5 + amount) * -1);
                   }
                   e.Channel.SendMessage("––––––––––––––––––––\n ¦   " + ((num[0] == 0) ? "💣" : ((num[0] == 1) ? "🆓" : ((num[0] == 2) ? "🍒" : ((num[0] == 3) ? "🍪" : ((num[0] == 4) ? "🔹" : "🔷"))))) + "   ¦  " + ((num[1] == 0) ? "💣" : ((num[1] == 1) ? "🆓" : ((num[1] == 2) ? "🍒" : ((num[1] == 3) ? "🍪" : ((num[1] == 4) ? "🔹" : "🔷"))))) + "   ¦  " + ((num[2] == 0) ? "💣" : ((num[2] == 1) ? "🆓" : ((num[2] == 2) ? "🍒" : ((num[2] == 3) ? "🍪" : ((num[2] == 4) ? "🔹" : "🔷"))))) + "  ¦\n ¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯\n 🆓=5$   🆓🆓=20$   🆓🆓🆓=40$\n 🍒🍒🍒=50$ 🍪🍪🍪=100$\n 🔹🔹🔹=200$ 🔷🔷🔷=500$\n 💣💣💣=-250$\n\n You " + ((won) ? "won " : "lost ") + amount + "$" +
                       $"\nYou now have {findDataUser(e.User).Score}$");
               });

                group.CreateCommand("1-")
                .Description("Let's you talk to Chomsky")
                .Parameter("Message", ParameterType.Unparsed)
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"{bot2session.Think(e.Args[0])}");
                });

                group.CreateCommand("2-")
                .Description("Let's you talk to CleverBot")
                .Parameter("Message", ParameterType.Unparsed)
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"{HttpUtility.HtmlDecode(Cleverbot1session.Think(e.Args[0]))}");
                });

                group.CreateCommand("cleverCeption")
               .Description("Bots talk to each other for a fixed amount of messages. Try not to abuse!")
               .Parameter("Message Count", ParameterType.Required)
               .Parameter("Starting Message", ParameterType.Unparsed)
               .Do(async e =>
               {
                   ChatterBot _Cleverbot2 = factory.Create(ChatterBotType.CLEVERBOT);
                   ChatterBotSession Cleverbot2session = _Cleverbot2.CreateSession();

                   string message = e.Args[1] != "" ? e.Args[1] : "Hello";
                   await e.Channel.SendMessage("A: " + message);
                                  
                   for(int count = 0; count < int.Parse(e.Args[0]); count++)
                   {
                       if(count % 2 != 0)
                       {
                           message = HttpUtility.HtmlDecode(Cleverbot1session.Think(message));
                           await e.Channel.SendMessage("A: " + message);
                       }
                       else if(count % 2 == 0)
                       {
                           message = HttpUtility.HtmlDecode(Cleverbot2session.Think(message));
                           await e.Channel.SendMessage("B: " + message);
                       }
                   }
               });       
            });

            manager.CreateCommands("hangman", group =>
            {
                group.CreateCommand("start")
                .Description("Create a game of hangman")
                .Parameter("Word")
                .Parameter("Attempts")
                .Parameter("Server-ID")
                .Parameter("Channel-ID")
                .PrivateOnly()
                .Do(async e =>
                {
                    if (hangman == null || !hangman.active)
                    {
                        hangman = new Data.Session.Hangman(e.Args[0], int.Parse(e.Args[1]));
                        await e.User.SendMessage("Done!");
                        Channel message = _client.GetServer(ulong.Parse(e.Args[2])).GetChannel(ulong.Parse(e.Args[3]));
                        await message.SendMessage($"{e.User.Name} started a session of hangman!\n\nParticipate by using the **!hangman guess** command!\n\n{hangman.hidden} ({e.Args[1]} false tries allowed)");
                    }
                    else await e.User.SendMessage("Currently in use, sorry!");
                });

                group.CreateCommand("guess")
                .Description("Guess a character")
                .Parameter("Guess")
                .Do(async e =>
                {
                    if (hangman.active)
                    {
                        string output = "";

                        if(e.Args[0].Length == hangman.word.Length)
                        {
                            output = hangman.solve(e.Args[0], e.User);
                        }

                        else output = hangman.input(e.Args[0].ToCharArray()[0], e.User);

                        await e.Channel.SendMessage(output);
                    }
                    else await e.Channel.SendMessage("No session of hangman running, sorry!");
                });
            });

            manager.CreateCommands("scramble", group =>
            {
                group.CreateCommand("start")
                .Description("Scramble a word up and guess what it is")
                .Parameter("Word")
                .Parameter("Attempts")
                .Parameter("Server-ID")
                .Parameter("Channel-ID")
                .PrivateOnly()
                .Do(async e =>
                {
                    if (scramble == null || !scramble.active)
                    {
                        scramble = new Data.Session.Scramble(e.Args[0], int.Parse(e.Args[1]));
                        await e.User.SendMessage("Done :smiley_cat:");
                        Channel message = _client.GetServer(ulong.Parse(e.Args[2])).GetChannel(ulong.Parse(e.Args[3]));
                        await message.SendMessage($"{e.User.Name} started a session of word scrambler!\n\nParticipate by using the **!scramble guess** command!\n\n{scramble.hidden} ({e.Args[1]} false tries allowed)");
                    }
                    else await e.User.SendMessage("Currently in use, sorry :frowning:");
                });

                group.CreateCommand("guess")
                .Description("Guess the word")
                .Parameter("Guess")
                .Do(async e =>
                {
                    if (scramble.active)
                    {
                        string output = scramble.solve(e.Args[0], e.User);
                        await e.Channel.SendMessage(output);
                    }
                    else await e.Channel.SendMessage("No session of word scrambler running, sorry :frowning:");
                });
            });

            manager.CreateCommands("salad", group =>
            {
                group.CreateCommand("start")
                .Description("Create a game of word-salad")
                .Parameter("Words", ParameterType.Unparsed)
                .Do(async e =>
                {
                    string[] words = e.GetArg(0).Split(' ');
                    salad = new Data.Session.Salad(words.ToList());

                    await e.Channel.SendMessage(salad.drawMap());
                });

                group.CreateCommand("guess")
                .Description("Guess the words x/y start and end point. Example: !salad guess 1;1 1;4")
                .Parameter("Guess", ParameterType.Unparsed)
                .Do(async e =>
                {
                    await e.Channel.SendMessage(salad.guessWord(e.User, e.GetArg(0)));
                });
            });

            manager.CreateCommands("ranking", group =>
            {
                group.PublicOnly();

                group.CreateCommand("Score")
                .Description("get the top scoreboard")
                .Parameter("(true) to get global ranking", ParameterType.Optional)
                .Do(async e =>
                {
                    List<Data.Individual.User> tempSort = userScores.users.OrderByDescending(u => u.Score).ToList();

                    string output = "";

                    if (e.Args[0] == "")
                    {
                        int count = 0;

                        foreach (Data.Individual.User curUser in tempSort)
                        {
                            if (count >= 10) break;
                            count++;
                            try
                            {
                                output += $"#{count} ``$ {curUser.Score} $`` by {e.Server.GetUser(curUser.ID).Name}\n";
                            }
                            catch (NullReferenceException ex)
                            {
                                count--;
                            }
                        }
                    }
                    else if (e.Args[0] == "true")
                    {
                        for (int i = 0; i < tempSort.Count; i++)
                        {
                            try
                            {
                                output += $"#{i + 1} ``$ {tempSort[i].Score} $`` by {e.Server.GetUser(tempSort[i].ID).Name}\n";
                            }
                            catch (NullReferenceException ex)
                            {
                                output += $"#{i + 1} ``$ {tempSort[i].Score} $`` by **global** {findUser(tempSort[i].ID, _client)}\n";
                            }
                        }
                    }

                    await e.Channel.SendMessage(output);
                });

                group.CreateCommand("Experience")
                .Description("get the top expboard")
                .Parameter("(true) to get global ranking", ParameterType.Optional)
                .Do(async e =>
                {
                    List<Data.Individual.User> tempSort = userScores.users.OrderByDescending(u => u.Experience).ToList();

                    string output = "";

                    if (e.Args[0] == "")
                    {
                        int count = 0;

                        foreach (Data.Individual.User curUser in tempSort)
                        {
                            if (count >= 10) break;
                            count++;
                            try
                            {
                                output += $"#{count} ``{curUser.Experience} EXP`` by {e.Server.GetUser(curUser.ID).Name}\n";
                            }
                            catch (NullReferenceException ex)
                            {
                                count--;
                            }
                        }
                    }
                    else if (e.Args[0] == "true")
                    {
                        for (int i = 0; i < tempSort.Count; i++)
                        {
                            try
                            {
                                output += $"#{i + 1} ``{tempSort[i].Experience} EXP`` by {e.Server.GetUser(tempSort[i].ID).Name}\n";
                            }
                            catch (NullReferenceException ex)
                            {
                                output += $"#{i + 1} ``{tempSort[i].Experience} EXP`` by **global** {findUser(tempSort[i].ID, _client)}\n";
                            }
                        }
                    }

                    await e.Channel.SendMessage(output);
                });

                group.CreateCommand("Level")
                .Description("get the top levelboard")
                .Parameter("(true) to get global ranking", ParameterType.Optional)
                .Do(async e =>
                {
                    List<Data.Individual.User> tempSort = userScores.users.OrderByDescending(u => u.Level).ToList();

                    string output = "";

                    if (e.Args[0] == "")
                    {
                        int count = 0;

                        foreach (Data.Individual.User curUser in tempSort)
                        {
                            if (count >= 10) break;
                            count++;
                            try
                            {
                                output += $"#{count} ``Level {curUser.Level}`` by {e.Server.GetUser(curUser.ID).Name}\n";
                            }
                            catch (NullReferenceException ex)
                            {
                                count--;
                            }
                        }
                    }
                    else if (e.Args[0] == "true")
                    {
                        for (int i = 0; i < tempSort.Count; i++)
                        {
                            try
                            {
                                output += $"#{i + 1} ``Level {tempSort[i].Level}`` by {e.Server.GetUser(tempSort[i].ID).Name}\n";
                            }
                            catch (NullReferenceException ex)
                            {
                                output += $"#{i + 1} ``Level {tempSort[i].Level}`` by **global** {findUser(tempSort[i].ID, _client)}\n";
                            }
                        }
                    }

                    await e.Channel.SendMessage(output);
                });
            });
            }

        private void _client_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.User.IsBot) return;

            try
            {
                int pre = -1;
                if (userScores.users.Any(x => x.ID == e.User.Id)) pre = findDataUser(e.User).Level;
                addToBase(e.User, 0, e.Message.RawText.Length);
                if (pre != -1 && pre < findDataUser(e.User).Level) e.Channel.SendMessage($"{e.User.Name} advanced from level {pre} to level {findDataUser(e.User).Level}!");

                if (bomb != null && bomb.active && e.User == bomb.defender)
                {
                    if (bomb.wires.Contains(e.Message.Text))
                    {
                        e.Channel.SendMessage(bomb.guess(e.Message.Text));
                    }
                }
            }
            catch(ArgumentOutOfRangeException ex)
            {
                e.Channel.SendMessage(ex.Message);
            }
        }

        public static void addToBase(User user, int score)
        {
            userScores = new Data.UserScore();
            for(int i = 0; i<userScores.users.Count; i++)
            {
                if(userScores.users[i].ID == user.Id)
                {
                    score += userScores.users[i].Score;
                    int exp = userScores.users[i].Experience;
                    int mip = userScores.users[i].monster;
                    userScores.users.RemoveAt(i);
                    userScores.users.Add(new Data.Individual.User(user.Id, score, exp));
                    userScores.writeScore();
                    return;
                }
            }
            userScores.users.Add(new Data.Individual.User(user.Id, score, 0));
            userScores.writeScore();
        }

        private void addToBase(User user, int score, int exp)
        {
            userScores = new Data.UserScore();
            for (int i = 0; i < userScores.users.Count; i++)
            {
                if (userScores.users[i].ID == user.Id)
                {
                    score += userScores.users[i].Score;
                    exp += userScores.users[i].Experience;
                    userScores.users.RemoveAt(i);
                    userScores.users.Add(new Data.Individual.User(user.Id, score, exp));
                    userScores.writeScore();
                    return;
                }
            }
            userScores.users.Add(new Data.Individual.User(user.Id, score, exp));
            userScores.writeScore();
        }

        public static Data.Individual.User findDataUser(User user)
        {
            userScores = new Data.UserScore();
            for (int i = 0; i < userScores.users.Count; i++)
            {
                if (userScores.users[i].ID == user.Id)
                {
                    return userScores.users[i];
                }
            }
            return null;
        }

        public static string findUser(ulong ID, DiscordClient _client)
        {
            string userName = ID.ToString();

            foreach(Server se in _client.Servers)
            {
                if (se.GetUser(ID) != null) userName = se.GetUser(ID).Name;
            }

            return userName;
        }
    }
}
