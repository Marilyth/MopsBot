using System;
using Discord;
using System.Globalization;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace MopsBot.Module.Data
{
    class Overwatch_Data
    {
        public List<OW_User> OW_Users = new List<OW_User>();

        public Overwatch_Data()
        {
            StreamReader read = new StreamReader("data//overwatchid.txt");

            string stats = "";
            while ((stats = read.ReadLine()) != null)
            {
                string[] data = stats.Split(':');
                if (OW_Users.Exists(x => x.discordID == ulong.Parse(data[1])))
                    OW_Users.Find(x => x.discordID == ulong.Parse(data[1])).channels.Add(DataBase.getChannel(ulong.Parse(data[0])));
                else
                {
                    OW_Users.Add(new OW_User(ulong.Parse(data[1]), data[2], DataBase.getChannel(ulong.Parse(data[0]))));
                }
            }

            read.Close();
        }

        public void writeInformation()
        {
            StreamWriter write = new StreamWriter("data//overwatchid.txt");

            foreach (OW_User user in OW_Users)
            {
                foreach (Channel ch in user.channels)
                    write.WriteLine($"{ch.Id}:{user.discordID}:{user.battletag}");
            }

            write.Close();
        }

        public string drawDiagram(int count)
        {
            OW_Users = OW_Users.OrderByDescending(x => x.rank).ToList();

            List<OW_User> tempUsers = OW_Users.Take(count).ToList();

            int maximum = tempUsers[0].rank;

            string[] lines = new string[count];

            for (int i = 0; i < count; i++)
            {
                lines[i] = (i + 1) < 10 ? $"#{i + 1} |" : $"#{i + 1}|";
                double relPercent = OW_Users[i].rank / ((double)maximum / 10);
                for (int j = 0; j < relPercent; j++)
                {
                    lines[i] += "■";
                }
                lines[i] += $" ({OW_Users[i].rank} / {OW_Users[i].username})";
            }

            string output = "```" + string.Join("\n", lines) + "```";

            return output;
        }
    }

    class OW_User
    {
        public ulong discordID;
        public string battletag, username;
        public int rank, quickWins, compWins, compLost, level;
        public List<Channel> channels = new List<Channel>();

        public OW_User(ulong disID, string pBattletag, Channel channel)
        {
            discordID = disID;
            battletag = pBattletag;

            channels.Add(channel);

            updateStats();
        }

        public static dynamic userStats(string battleTag)
        {
            battleTag = battleTag.Replace("#", "-");
            string query = Information.readURL($"https://api.lootbox.eu/pc/eu/{battleTag}/profile");

            var jss = new JavaScriptSerializer();
            return jss.Deserialize<dynamic>(query);
        }

        public static string statsToString(string BattleTag)
        {
            var dict = userStats(BattleTag);

            dict = dict["data"];

            string username = dict["username"];
            int level = level = dict["level"];
            string qWins = dict["games"]["quick"]["wins"];
            string cWins = dict["games"]["competitive"]["wins"];
            int cLost = dict["games"]["competitive"]["lost"];
            string rank = dict["competitive"]["rank"];

            string output = $"User: {username}\nLevel: {level}\nQuick wins: {qWins}\nRank: {rank} ({cWins}W/{cLost}L)";

            return output;
        }

        public string statsToString()
        {
            string output = $"User: {username}\nLevel: {level}\nQuick wins: {quickWins}\nRank: {rank} ({compWins}W/{compLost}L)";

            return output;
        }

        public void updateStats()
        {
            var dict = userStats(battletag);

            dict = dict["data"];

            username = dict["username"];
            level = dict["level"];
            quickWins = int.Parse(dict["games"]["quick"]["wins"]);
            compWins = int.Parse(dict["games"]["competitive"]["wins"]);
            compLost = dict["games"]["competitive"]["lost"];
            rank = int.Parse(dict["competitive"]["rank"]);
        }

        public string trackChange()
        {
            OW_User compare = (OW_User)this.MemberwiseClone();

            updateStats();

            string output = "";

            if (compare.level != level)
                output += $"Tracked level advancement of {compare.level} to **{level}** by **{username}**!\n";
            if (compare.quickWins != quickWins)
                output += $"**{username}** has won another quick match! ({quickWins})\n";
            if (compare.compWins != compWins)
                output += $"**{username}** has won another competitive match! ({compWins})\n";
            if (compare.rank != rank)
                output += $"**{username}** advanced from rank {compare.rank} to rank **{rank}**!";

            return output;
        }
    }
}
