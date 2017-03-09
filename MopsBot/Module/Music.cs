using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot.Module
{
    internal class Music : IModule
    {
        void IModule.Install(ModuleManager manager)
        {
            ModuleManager _manager = manager;
            DiscordClient _client = _manager.Client;

            manager.CreateCommands("", group =>
            {
                group.CreateCommand("join")
                .Parameter("Channelname")
                .Description("Joins the specified voice channel")
                .Do(e =>
                {

                });

                group.CreateCommand("summon")
                .Description("Joins the voice channel you are in currently")
                .Do(e =>
                {

                });

                group.CreateCommand("play")
                .Parameter("URL")
                .Description("Streams the audio fetched from the URL")
                .Do(e =>
                {

                });

                group.CreateCommand("volume")
                .Parameter("amount")
                .Description("Sets the volume from 0% - 100%")
                .Do(e =>
                {

                });

                group.CreateCommand("pause")
                .Description("Pauses streaming")
                .Do(e =>
                {

                });

                group.CreateCommand("resume")
                .Description("Resumes streaming")
                .Do(e =>
                {

                });

                group.CreateCommand("skip")
                .Description("Skips current song and jumps to next in Queue")
                .Do(e =>
                {

                });

                group.CreateCommand("playing")
                .Description("Returns current song name")
                .Do(e =>
                {

                });

                group.CreateCommand("stop")
                .Description("Leaves voice channel")
                .Do(e =>
                {

                });
            });
        }
    }
}
