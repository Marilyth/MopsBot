using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot.Module.Data.Session
{
    class Hangman
    {
        private int attempt, tries;
        public bool active;
        public string word, hidden;

        public Hangman(string keyword, int attempts)
        {
            active = true;
            word = keyword;
            foreach (char c in word) hidden += "-";
            tries = attempts;
            attempt = 0;
        }

        public string solve(string guess, Discord.User eUser)
        {
            if (word.ToLower().Equals(guess.ToLower()))
            {
                active = false;
                int bonus = 0, result = 0;
                foreach(char c in hidden)
                    if (c.Equals('-')) bonus++;
                result = word.Length + bonus * (tries - attempt);
                Game.addToBase(eUser, result);
                return $"{word}\nYou solved it! o;\n+**[$ {result} $]**";
            }

            else
            {
                attempt++;

                if (attempt >= tries)
                {
                    this.active = false;
                    return "You lost! HAHAHAHA";
                }

                return hidden + $" ({tries - attempt} false tries remaining)\nWrong :d";
            }
        }

        public string input(char guess, Discord.User eUser)
        {
            if (word.ToLower().Contains(guess))
            {
                for (int i = 0; i < word.Length; i++)
                {
                    char[] hidarray = hidden.ToCharArray();
                    if (char.ToLower(word[i]).Equals(guess)) hidarray[i] = word[i];
                    hidden = new string(hidarray);
                }
            }

            else attempt++;

            if (attempt >= tries)
            {
                this.active = false;
                return "You lost! HAHAHAHA";
            }

            else if (!hidden.Contains("-"))
            {
                this.active = false;
                Game.addToBase(eUser, word.Length - (tries - attempt));
                return $"{hidden}\nSeems like you won!\n+**[$ {word.Length} $]**";
            }

            return hidden + $" ({tries - attempt} false tries remaining)";
        }
    }
}
