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
            word = keyword.ToLower();
            foreach (char c in word) hidden += "-";
            tries = attempts;
            attempt = 0;
        }

        public string input(char guess)
        {
            if (word.Contains(guess))
            {
                for (int i = 0; i < word.Length; i++)
                {
                    char[] hidarray = hidden.ToCharArray();
                    if (word[i].Equals(guess)) hidarray[i] = guess;
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
                return hidden + "\nSeems like you won!";
            }

            return hidden + $" ({tries - attempt} false tries remaining)";
        }
    }
}
