using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot.Module.Data.Session
{
    class Scramble
    {
        private int attempt, tries;
        public bool active;
        public string word, hidden;

        public Scramble(string keyword, int attempts)
        {
            active = true;
            word = keyword.ToLower() == "random" ? Information.getRandomWord() : keyword;
            var query = from c in word.ToCharArray() orderby Guid.NewGuid() select c;
            foreach (var qchar in query) hidden += qchar;
            tries = attempts;
            attempt = 0;
            Console.WriteLine(word);
        }

        public string solve(string guess, Discord.User eUser)
        {
            if (word.ToLower().Equals(guess.ToLower()))
            {
                active = false;
                double result = ((word.Length + word.Length * (tries - attempt)) / 2);
                Game.addToBase(eUser, Convert.ToInt32(Math.Floor(result)));
                return $"{word}\nYou solved it! o;\n+**[$ {result} $]**";
            }

            else
            {
                attempt++;

                if (attempt >= tries)
                {
                    this.active = false;
                    return "You lost! HAHAHAHA\nIt was " + word + "!";
                }

                return hidden + $" ({tries - attempt} false tries remaining)\nWrong :d";
            }
        }
    }
}
