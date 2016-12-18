﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

namespace MopsBot.Module.Data
{
    class UserScore
    {
        public List<Individual.User> users = new List<Individual.User>();

        public UserScore()
        {
            StreamReader read = new StreamReader("data//scores.txt");

            string fs = "";
            while ((fs = read.ReadLine()) != null)
            {
                string[] s = fs.Split(':');
                users.Add(new Individual.User(ulong.Parse(s[0]),int.Parse(s[1]), int.Parse(s[2])));
            }
            read.Close();
            users = users.OrderByDescending(u => u.Experience).ToList();
        }

        public void writeScore()
        {
            users = users.OrderByDescending(u => u.Experience).ToList();

            StreamWriter write = new StreamWriter("data//scores.txt");

            foreach (Individual.User that in users)
            {
                write.WriteLine($"{that.ID}:{that.Score}:{that.Experience}");
            }
            write.Close();
        }

        public string drawDiagram(int count, DiagramType type)
        {
            List<Individual.User> tempUsers = users.Take(count).ToList();

            int maximum = 0;
            string[] lines = new string[count];

            switch (type)
            {
                case DiagramType.Experience:
                    tempUsers = tempUsers.OrderByDescending(x => x.Experience).ToList();

                    maximum = tempUsers[0].Experience;

                    for (int i = 0; i < count; i++)
                    {
                        lines[i] = (i + 1).ToString().Length < 2 ? $"#{i + 1} |" : $"#{i + 1}|";
                        double relPercent = users[i].Experience / ((double)maximum / 10);
                        for (int j = 0; j < relPercent; j++)
                        {
                            lines[i] += "■";
                        }
                        lines[i] += $"  ({users[i].Experience} / {Game.findUser(users[i].ID)})";
                    }
                    break;

                case DiagramType.Level:
                    tempUsers = tempUsers.OrderByDescending(x => x.Level).ToList();

                    maximum = tempUsers[0].Level;

                    for (int i = 0; i < count; i++)
                    {
                        lines[i] = (i + 1).ToString().Length < 2 ? $"#{i + 1} |" : $"#{i + 1}|";
                        double relPercent = users[i].Level / ((double)maximum / 10);
                        for (int j = 0; j < relPercent; j++)
                        {
                            lines[i] += "■";
                        }
                        lines[i] += $"  ({users[i].Level} / {Game.findUser(users[i].ID)})";
                    }
                    break;

                case DiagramType.Score:
                    tempUsers = tempUsers.OrderByDescending(x => x.Score).ToList();

                    maximum = tempUsers[0].Score;

                    for (int i = 0; i < count; i++)
                    {
                        lines[i] = (i + 1).ToString().Length < 2 ? $"#{i+1} |" : $"#{i+1}|";
                        double relPercent = users[i].Score / ((double)maximum / 10);
                        for (int j = 0; j < relPercent; j++)
                        {
                            lines[i] += "■";
                        }
                        lines[i] += $"  ({users[i].Score} / {Game.findUser(users[i].ID)})";
                    }
                    break;
            }

            string output = "```" + string.Join("\n", lines) + "```";

            return output;
        }

        public enum DiagramType{Experience, Level, Score}
    }
}
