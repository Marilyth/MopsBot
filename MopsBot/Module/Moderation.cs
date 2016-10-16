using Discord;
using Discord.Commands;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Timers;

namespace MopsBot.Module
{
    class Moderation : IModule
    {

        public class EUser
        {
            public User u;
            System.Timers.Timer t;
            IEnumerable<Role> r;
            Server s;
            Moderation m;
            public EUser(User pu, int min, Server ps, List<Role> pr, Moderation pM)
            {
                m = pM;
                s = ps;
                u = pu;
                r = u.Roles;

                u.Edit(null, null, null, pr);


                if (min != 0)
                {
                    t = new System.Timers.Timer(min * 60 * 1000);
                    t.Elapsed += T_Elapsed;
                    t.AutoReset = false;
                    t.Enabled = true;

                    GC.KeepAlive(t);
                }
            }



            private void T_Elapsed(object sender, ElapsedEventArgs e)
            {

                undo();
                t.Dispose();


            }
            public void undo()
            {
                u.Edit(null, null, null, r);
                m.updateList(this);
            }


        }
        List<EUser> tu;
        private ModuleManager _manager;
        private DiscordClient _client;
        private Data.Poll poll;

        public Moderation()
        {
            tu = new List<EUser>();

        }
        public void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            _client.MessageReceived += _client_MessageReceived;
            _client.UserJoined += _client_UserJoined;

            manager.CreateCommands("", group =>
                { 
                 group.CreateCommand("poll")
                .Description("Creates a poll\nExample: !poll Am I sexy?;Yes:No;@Panda @Demon @Snail")
                .Parameter("Poll", ParameterType.Unparsed)
                .Do(async e =>
                {
                    if (!e.User.ServerPermissions.ManageChannels) return;
                    poll = null;

                    string[] pollSegments = e.Args[0].Split(';');
                    List<User> participants = new List<User>();

                    participants.AddRange(e.Message.MentionedUsers.ToList());

                    foreach(var a in e.Message.MentionedRoles)
                    {
                        if(a.IsEveryone && e.Message.Text.Contains("here")) participants.AddRange(a.Members.Where(x => x.Status == UserStatus.Online));
                        else participants.AddRange(a.Members);
                    }

                    poll = new Data.Poll(pollSegments[0], pollSegments[1].Split(':'), participants.ToArray());

                    foreach (Discord.User part in poll.participants)
                    {
                        string output = "";
                        for (int i = 0; i < poll.answers.Length; i++)
                        {
                            output += $"\n``{i + 1}`` {poll.answers[i]}";
                        }
                        try
                        {
                            await part.SendMessage($"{e.User.Name} has created a poll:\n\n📄: {poll.question}\n{output}\n\nTo vote, simply PM me the **Number** of the answer you agree with.");
                        }
                        catch { }
                    }

                    await e.Channel.SendMessage("Poll started, Participants notified!");
                });

                    group.CreateCommand("pollEnd")
            .Description("Ends the poll and returns the results.")
            .Do(async e =>
            {
                if (!e.User.ServerPermissions.ManageChannels) return;

                await e.Channel.SendMessage(poll.pollToText());

                foreach(User part in poll.participants)
                {
                    await part.SendMessage($"📄:{poll.question}\n\nHas ended without your participation, sorry!");
                    poll.participants.Remove(part);
                }
            });

                });
        }

        private void _client_UserJoined(object sender, UserEventArgs e)
        {
            if(e.Server.Id.Equals(205130885337448469)) e.Server.GetChannel(235733911257219072).SendMessage($"Willkommen im **{e.Server.Name}** Server, {e.User.Mention}!\n\nBevor Du vollen Zugriff auf den Server hast, möchten wir Dich auf die Regeln des Servers hinweisen, die Du hier findest: {e.Server.GetChannel(205136618955341825).Mention}\nSobald Du fertig bist, kannst Du Dich an einen unserer Moderatoren zu Deiner rechten wenden, die Dich alsbald zum Mitglied ernennen.\n\nHave a very mopsig day\nDein heimlicher Verehrer Mops");
        }

        private void _client_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Channel.IsPrivate && poll != null)
                {
                    if(poll.participants.ToList().ConvertAll(x => x.Discriminator).ToArray().Contains(e.User.Discriminator))
                    {
                        poll.results[int.Parse(e.Message.Text) - 1]++;
                        e.User.SendMessage("Vote accepted!");

                        poll.participants.RemoveAll(x => x.Discriminator == e.User.Discriminator);
                    }
                }

            }
            catch { }
        }

        public bool updateList(EUser pU)
        {
            if (tu.Contains(pU))
            {
                tu.Remove(pU);
                return true;
            }
            return false;
        }
        public bool updateList(User pU)
        {
            for (int i = 0; i < tu.Count; i++)
            {
                if (tu[i].u == pU)
                {
                    tu[i].undo();
                    
                    return true;
                }
            }
            return false;
        }
    }
}
