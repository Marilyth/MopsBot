using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MopsBot.Module.Data.Session
{
    class Bomb
    {
        public Discord.User attacker, defender;
        public string[] wires = new string[] { "salmon", "purple", "aquamarine", "emerald", "apricot", "cerulean", "peach", "blue", "red", "yellow", "black", "white", "green", "orange", "cyan", "beige", "grey", "gold", "buff", "monza", "rose", "tan", "brown", "flax", "pink" };
        public string wire;
        public bool active;
        private Random decider = new Random();

        public Bomb(Discord.User atk, Discord.User def)
        {
            attacker = atk;
            defender = def;
            active = true;
            wires = wires.Skip(decider.Next(15, 23)).ToArray();
            wire = wires[decider.Next(0, wires.Length)];
        }

        public string guess(string eWire)
        {
            if (eWire.Equals(wire))
            {
                Game.addToBase(defender, wires.Length * 3);
                active = false;
                return $"Wow! Good job o.o\n``[$ {wires.Length * 3} $]`` You now have ${Game.findDataUser(defender).Score} in total!";
            }

            else if (!detonate())
            {
                return $"The bomb somehow jumped over to {attacker.Mention}! Quick, cut a wire!\n";
            }

            else
            {
                string output = $"No! The right wire was {wire}\nWOOOOSH!\n...ehe, I don't really have an actual bomb.";

                if (attacker != null)
                {
                    Game.addToBase(attacker, wires.Length / wires.Length);
                    Game.addToBase(defender, wires.Length / wires.Length * -1);
                    output += $"\nAnyway, {attacker.Name} just took ``[$ {wires.Length / wires.Length} $]`` from you. {attacker.Name} now has ${Game.findDataUser(attacker).Score}.\nYou now have ${Game.findDataUser(defender).Score}.";
                }

                active = false;
                return output;
            }
        }

        private bool detonate()
        {
            if (attacker != null && 2 == decider.Next(1, 4))
            {
                return true;
            }
            else
            {
                Discord.User vic = defender;
                defender = attacker;
                attacker = vic;
                return false;
            }
        }
    }
}
