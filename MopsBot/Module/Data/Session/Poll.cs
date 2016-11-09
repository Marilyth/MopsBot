using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot.Module.Data.Session
{
    class Poll
    {
        public string question;
        public string[] answers;
        public int[] results;
        public List<Discord.User> participants;

        public Poll(string q, string[] a, Discord.User[] p)
        {
            question = q;
            answers = a;
            participants = p.ToList();

            results = new int[answers.Length];
        }

        public string pollToText()
        {
            string output = "";
            for(int i = 0; i < answers.Length; i++)
            {
                output += $"\n{answers[i]} -> {results[i]}";
            }

            return $"📄: {question}\n{output}";
        }
    }
}
