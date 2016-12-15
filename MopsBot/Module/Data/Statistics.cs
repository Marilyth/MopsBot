using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot.Module.Data
{
    class Statistics
    {
        public List<Day> days = new List<Day>();
        public string today = DateTime.Today.ToString("dd/MM/yyyy");

        public Statistics()
        {
            StreamReader read = new StreamReader("data//statistics.txt");

            string s = "";

            while ((s = read.ReadLine()) != null)
            {
                string[] data = s.Split(':');
                days.Add(new Day(data[0], int.Parse(data[1])));
            }
            read.Close();

            days = days.OrderByDescending(x => x.date).ToList();
        }

        public void addValue(int increase)
        {
            if (days.Exists(x => x.date.Equals(today)))
                days.Find(x => x.date.Equals(today)).value += increase;

            else days.Add(new Day(today, increase));

            saveData();
        }

        private void saveData()
        {
            StreamWriter write = new StreamWriter("data//statistics.txt");

            foreach(Day cur in days)
            {
                write.WriteLine($"{cur.date}:{cur.value}");
            }

            write.Close();
        }

        public string drawDiagram(int count)
        {
            List<Day> tempDays = days.Take(count).ToList();
            tempDays = tempDays.OrderByDescending(x => x.value).ToList();

            int maximum = tempDays[0].value;

            string[] lines = new string[count];

            for(int i = 0; i < count; i++)
            {
                lines[i] = $"{days[i].date}|";
                double relPercent = days[i].value / ((double)maximum / 10);
                for(int j = 0; j < relPercent; j++)
                {
                    lines[i] += "■";
                }
                lines[i] += $" ({days[i].value})";
            }

            string output = "```" + string.Join("\n", lines) + "```";

            return output;
        }
    }

    class Day
    {
        public string date;
        public int value;

        public Day(string pDate, int pValue)
        {
            date = pDate;
            value = pValue;
        }
    }
}
