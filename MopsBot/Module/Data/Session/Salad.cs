using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot.Module.Data.Session
{
    class Salad
    {
        public List<string> words;
        public List<Word> guessWords;
        public Individual.Field[,] mapset;
        private Random decider;

        public Salad(List<string> pWords)
        {
            decider = new Random();
            
            words = pWords;

            int fieldSize = 0;



            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].ToLower().Equals("random")) words[i] = Information.readURL("http://www.setgetgo.com/randomword/get.php");
                if (words[i].Length + 3 > fieldSize) fieldSize = words[i].Length + 3;
            }

            mapset = new Individual.Field[fieldSize, fieldSize];
            for(int i = 0; i< mapset.GetLength(0); i++)
            {
                for (int j = 0; j < mapset.GetLength(1); j++)
                    mapset[i, j] = new Individual.Field(Individual.Field.Type.empty);
            }

            guessWords = new List<Word>();

            allocate();
        }

        private void allocate()
        {
            foreach(string word in words)
            {
                int x = 0;
                int y = 0;

                direction wordDirection = direction.UnAllocated;

                switch (decider.Next(0, 2))
                {
                    case 0:
                        wordDirection = direction.Right;
                        x = decider.Next(0, mapset.GetLength(0) - word.Length);
                        y = decider.Next(0, mapset.GetLength(1));
                        guessWords.Add(new Word(x, y, x + word.Length-1, y, word));
                        break;
                    case 1:
                        wordDirection = direction.Down;
                        x = decider.Next(0, mapset.GetLength(0));
                        y = decider.Next(0, mapset.GetLength(1) - word.Length);
                        guessWords.Add(new Word(x, y, x, y + word.Length-1, word));
                        break;
                }

                for (int i = 0; i < word.Length; i++)
                {
                    switch (wordDirection)
                    {
                        case direction.Right:
                            mapset[x + i, y].setChar(char.ToUpper(word[i]));
                            break;
                        case direction.Down:
                            mapset[x, y + i].setChar(char.ToUpper(word[i]));
                            break;
                    }
                }
            }

            foreach(Individual.Field field in mapset)
            {
                if (field.fieldType.Equals(Individual.Field.Type.empty))
                    field.setChar((char)decider.Next(65,91));
            }
        }

        public string drawMap()
        {
            string[] lines = new string[mapset.GetLength(1)+1];

            lines[0] = "    ";

            for(int i = 0; i < mapset.GetLength(0); i++)
            {
                lines[0] += i.ToString().Length<2?$"{i}  ": $"{i} ";
                lines[i+1] += i.ToString().Length < 2 ? $"{i}  " : $"{i} ";
                for(int j = 0; j < mapset.GetLength(1); j++)
                {
                    lines[i+1] += $"[{mapset[j, i].letter}]";
                }
            }

            return  "```\n" +
                    $"{String.Join("\n",lines)}" +
                    $"\n```\nA total of {guessWords.Count} words are being searched. Good luck :d";
        }

        public string guessWord(Discord.User pUser, string guess)
        {
            var points = guess.Split(' ', ';');

            Word tempWord = guessWords.Where(x => x.Xstart == int.Parse(points[0]) && x.Ystart == int.Parse(points[1]) && x.Xend == int.Parse(points[2]) && x.Yend == int.Parse(points[3])).ElementAt(0);

            if (tempWord != null)
            {
                guessWords.Remove(tempWord);
                Game.addToBase(pUser, (mapset.GetLength(0) - (tempWord.word.Length - 1)) * tempWord.word.Length);
                return $"Yes! You found {tempWord.word} ({guessWords.Count} words remaining)\n+**[$ {(mapset.GetLength(0) - (tempWord.word.Length - 1)) * tempWord.word.Length} $]**";
            }

            else return "Nope";
        }

        public enum direction { Right, Down, DownRight, UpRight, UnAllocated};
    }

    class Word
    {
        public string word;
        public int Xstart, Ystart, Xend, Yend;

        public Word(int pStartx,int pStarty,int pEndx,int pEndy, string pWord)
        {
            Xstart = pStartx;
            Ystart = pStarty;
            Xend = pEndx;
            Yend = pEndy;
            word = pWord;
        }
    }
}
