using System;
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

            string s = "";
            while((s = read.ReadLine()) != null)
            {
                s.Split(':');
                osuUsers.Add(new osuUser(ulong.Parse(s.Split(':')[0]), s.Split(':')[1], s.Split(':')[2]));
            }

            read.Close();
        }

        public void writeInformation()
        {
            StreamWriter write = new StreamWriter("data//osuid.txt");

            foreach(osuUser user in osuUsers)
            {
                write.WriteLine($"{user.discordID}:{user.ident}:{user.mainMode}");
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

        public osuUser(ulong disID, string osuID, string mode)
        {
            ident = osuID;
            discordID = disID;
            mainMode = mode;
            updateStats();
        }

        public osuUser(ulong disID, string osuID)
        {
            ident = osuID;
            discordID = disID;
            mainMode = "m=0";
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
