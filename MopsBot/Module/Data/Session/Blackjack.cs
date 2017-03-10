﻿using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot.Module.Data.Session
{
    class Blackjack
    {
        public List<Blackjack_User> players;
        private List<Card> cards;
        public bool active;

        public Blackjack(User pDealer)
        {
            active = true;
            players = new List<Blackjack_User>();
            players.Add(new Blackjack_User(pDealer, true));
            fillDeck();
            shuffleDeck();

            drawCard(players[0].player, false);
            drawCard(players[0].player, false);
        }

        private void fillDeck()
        {
            cards = new List<Card>();

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    cards.Add(new Card() { Suit = (Suit)i, Face = (Face)j });

                    if (j <= 8)
                        cards[cards.Count - 1].Value = j + 1;
                    else
                        cards[cards.Count - 1].Value = 10;
                }
            }
        }

        private void shuffleDeck()
        {
            Random random = new Random();

            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                var temp = cards[k];
                cards[k] = cards[n];
                cards[n] = temp;
            }
        }

        public string drawCard(User pUser, bool show)
        {
            string output = "";

            if (cards.Count > 0)
            {
                if (players.Find(x => x.player.Equals(pUser)) != null)
                    players.Find(x => x.player.Equals(pUser)).cardsHeld.Add(cards[0]);

                if (show)
                {
                    players.Find(x => x.player.Equals(pUser)).done = true;
                    output += showCards();
                }
                cards.RemoveAt(0);
            }
            if(show)
                output += endRound();

            return output;
        }

        public string skipRound(User pUser)
        {
            string output = "";

            players.Find(x => x.player.Equals(pUser)).skipped = true;
            players.Find(x => x.player.Equals(pUser)).done = true;

            output += endRound();

            return output;
        }

        public string userJoin(User pUser, int bet)
        {
            string output = "";

            Blackjack_User temp = new Blackjack_User(pUser, bet);

            if (!players.Contains(temp))
            {
                players.Add(temp);
                Game.addToBase(pUser, -bet);
                output += $"Ooooh. A newcomer. {pUser.Name} just joined our table.\n\n";
                drawCard(players[players.Count - 1].player, false);
                drawCard(players[players.Count - 1].player, false);
            }
            else
                output += "You are already at the table. Duh.";

            return output;
        }

        public string showCards()
        {
            string output = "";

            foreach(Blackjack_User curUser in players)
            {
                output += "\n" + curUser.player.Name + ": ";
                foreach (Card cur in curUser.cardsHeld)
                {
                    if (curUser.dealer && cur.Equals(curUser.cardsHeld[0]))
                    {
                        output += $"**[ ? ]** ";
                    }
                    else
                        output += $"**[ :{cur.Suit.ToString()}: {cur.Face.ToString()}]** ";
                }
            }

            return output;
        }

        private string winner()
        {
            string output = "", winners = "";

            int dealerValue = players.Find(x => x.dealer == true).cardsValue();

            if (dealerValue > 21)
                dealerValue = 0;

            List<Blackjack_User> winner = new List<Blackjack_User>();

            foreach (Blackjack_User cur in players)
            {
                if (!cur.dealer && cur.cardsValue() <= 21 && cur.cardsValue() > dealerValue)
                    winner.Add(cur);
                output += $"{cur.player.Name}: **{cur.cardsValue()}**\n";
            }

            foreach (Blackjack_User win in winner)
            {
                winners += win.player.Name + ", ";
                Game.addToBase(win.player, win.bet*2);
            }

            return output += $"\nThe winner is: {(winner.Count > 0 ? winners : "Nobody" )} yay.\nWinners have been paid out double their original bet.";
        }

        public string endRound()
        {
            string output = "\n\n";
            bool done = true;
            bool result = true;

            foreach (Blackjack_User cur in players)
                if (!cur.done)
                {
                    done = false;
                }

            if (done)
            {
                foreach (Blackjack_User cur in players)
                {
                    if (!cur.skipped)
                        result = false;

                    if(!cur.dealer)
                        cur.done = false;

                    if (cur.cardsValue() > 21)
                    {
                        cur.done = true;
                        cur.skipped = true;
                    }
                }

                if (result)
                {
                    output += $"We are done. My turn";

                    while (players.Find(x => x.dealer == true).cardsValue() < 17)
                    {
                        drawCard(players.Find(x => x.dealer == true).player, false);
                    }
                    output += showCards();
                    output += "\n\n" + winner();

                    active = false;
                }
                else
                    output += endRound();
            }

            else
                output += $"\n{players.Find(x => x.done == false).player.Name}, what about you?";

            return output;
        }
    }

    class Blackjack_User
    {
        public User player;
        public bool done, dealer, skipped;
        public int bet;
        public List<Card> cardsHeld;

        public Blackjack_User(User pPlayer, bool pDealer)
        {
            player = pPlayer;
            dealer = pDealer;

            if (dealer)
            {
                done = true;
                skipped = true;
            }

            else
                done = false;

            cardsHeld = new List<Card>();
        }

        public Blackjack_User(User pPlayer, int pBet)
        {
            player = pPlayer;
            bet = pBet;
            dealer = false;
            done = false;

            cardsHeld = new List<Card>();
        }

        public int cardsValue()
        {
            int value = 0;

            foreach(Card cur in cardsHeld)
            {
                value += cur.Value;
            }

            return value;
        }
    }

    class Card
    {
        public Suit Suit { get; set; }
        public Face Face { get; set; }
        public int Value { get; set; }
    }

    public enum Suit
    {
        hearts,
        diamonds,
        spades,
        clubs
    }

    public enum Face
    {
        A,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        J,
        Q,
        K,
    }
}
