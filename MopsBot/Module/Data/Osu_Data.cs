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
    class Osu_Data
    {
        public List<osuUser> osuUsers = new List<osuUser>();

        public Osu_Data()
        {
            StreamReader read = new StreamReader("data//osuid.txt");

            string stats = "";
            while ((stats = read.ReadLine()) != null)
            {
                string[] data = stats.Split(':');
                if (osuUsers.Exists(x => x.discordID == ulong.Parse(data[1])))
                    osuUsers.Find(x => x.discordID == ulong.Parse(data[1])).channels.Add(DataBase.getChannel(ulong.Parse(data[0])));
                else
                {
                    osuUsers.Add(new osuUser(ulong.Parse(data[1]), data[2], data[3], DataBase.getChannel(ulong.Parse(data[0]))));
                }
            }

            read.Close();
        }

        public string drawDiagram(Server reqServer)
        {
            List<osuUser> tempOsuUsers = osuUsers.FindAll(x => x.channels.Exists(y => y.Server == reqServer));

            tempOsuUsers = tempOsuUsers.OrderByDescending(x => x.pp).ToList();

            double maximum = tempOsuUsers[0].pp;

            string[] lines = new string[tempOsuUsers.Count];

            for (int i = 0; i < tempOsuUsers.Count; i++)
            {
                lines[i] = (i + 1) < 10 ? $"#{i + 1} |" : $"#{i + 1}|";
                double relPercent = tempOsuUsers[i].pp / ((double)maximum / 10);
                for (int j = 0; j < relPercent; j++)
                {
                    lines[i] += "■";
                }
                lines[i] += $" {Math.Round(tempOsuUsers[i].pp)}pp / {tempOsuUsers[i].username}";
            }

            string output = "```coq\n" + string.Join("\n", lines) + "```";

            return output;
        }

        public void writeInformation()
        {
            StreamWriter write = new StreamWriter("data//osuid.txt");

            foreach(osuUser user in osuUsers)
            {
                foreach (Channel ch in user.channels)
                    write.WriteLine($"{ch.Id}:{user.discordID}:{user.ident}:{user.mainMode}");
            }

            write.Close();
        }
    }

    class osuUser
    {
        public string username, ident, mainMode;
        public double accuracy, pp;
        public ulong score, playcount;
        public ulong discordID;
        public List<Channel> channels;

        public osuUser(ulong disID, string osuID, string mode, Channel channel)
        {
            channels = new List<Channel>();
            ident = osuID;
            discordID = disID;
            mainMode = mode;
            channels.Add(channel);
            updateStats();
        }

        public osuUser(ulong disID, string osuID, Channel channel)
        {
            channels = new List<Channel>();
            ident = osuID;
            discordID = disID;
            mainMode = "m=0";
            channels.Add(channel);
            updateStats();
        }

        public static dynamic userStats(string id, string mode)
        {
            string query = Information.readURL($"https://osu.ppy.sh/api/get_user?u={id}&{mode}&k=8ad11f6daf7b439f96eee1c256d474cd9925d4d8");
            query = query.Remove(0, 1);
            query = query.Remove(query.Length - 1, 1);

            var jss = new JavaScriptSerializer();
            return jss.Deserialize<dynamic>(query);
        }

        public static dynamic userStats(string id)
        {
            string query = Information.readURL($"https://osu.ppy.sh/api/get_user?u={id}&k=8ad11f6daf7b439f96eee1c256d474cd9925d4d8");
            query = query.Remove(0, 1);
            query = query.Remove(query.Length - 1, 1);

            var jss = new JavaScriptSerializer();
            return jss.Deserialize<dynamic>(query);
        }

        public void updateStats()
        {
            dynamic tempDict = userStats(ident, mainMode);

            try
            {
                username = tempDict["username"];
                playcount = ulong.Parse(tempDict["playcount"]);
                score = ulong.Parse(tempDict["total_score"]);
                pp = double.Parse(tempDict["pp_raw"], CultureInfo.InvariantCulture);
                accuracy = double.Parse(tempDict["accuracy"], CultureInfo.InvariantCulture);
            } catch(Exception e) { Console.WriteLine(e.Message); }
        }

        public void updateStats(dynamic tempDict)
        {
            pp = double.Parse(tempDict["pp_raw"], CultureInfo.InvariantCulture);
            accuracy = double.Parse(tempDict["accuracy"], CultureInfo.InvariantCulture);
            username = tempDict["username"];
            score = ulong.Parse(tempDict["total_score"]);
            playcount = ulong.Parse(tempDict["playcount"]);
        }

        public string modeToString()
        {
            switch (mainMode)
            {
                case "m=0":
                    return "Standard";
                case "m=1":
                    return "Taiko";
                case "m=2":
                    return "CtB";
                case "m=3":
                    return "Mania";
                default:
                    return "Nothing";
            }
        }
    }
}
