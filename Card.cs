using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTo21
{
    /// <summary>
    /// Represents an individual card in deck two ways: 
    /// ID string is two-character short name and name is full card name written out
    /// </summary>
    public class Card
    {
        public string ID;
        public string name; // Option 1: just store the name for each card alongside the ID

        public Card(string id, string name) //store short aand long names in the class
        {
            ID = id; // short name
            this.name = name; //long name
        }

        // Option 2: figure out what the name should be based on the ID
        // (only uncomment this code if you choose to use this INSTEAD OF the name property, for now)
        /// <summary>
        /// Example of other approach to card name. If using this approach, you would
        /// replace any reference to card.name with card.GetName(). Later in the course,
        /// you will learn how to create a "getter" that combines these two approaches.
        /// Note that a lot of the code in this method is just slightly modified from Deck.cs
        /// </summary>
        /// <returns>String containing full name of card, calculated from the ID</returns>
        public string GetName()
        {
            string cardLongName;
            string cardVal = ID.Remove(ID.Length - 1);
            switch (cardVal)
                    {
                        case "A":
                            cardLongName = "Ace of ";
                            break;
                        case "J":
                            cardLongName = "Jack of ";
                            break;
                        case "Q":
                            cardLongName = "Queen of ";
                            break;
                        case "K":
                            cardLongName = "King of ";
                            break;
                        default:
                            cardLongName = cardVal.ToString() + " of ";
                            break;
                    }

            string cardSuit = ID.Remove(ID.Length);
            switch (cardSuit)
                    {
                        case "S":
                            cardLongName += "Spades";
                            break;
                        case "H":
                            cardLongName += "Hearts";
                            break;
                        case "C":
                            cardLongName += "Clubs";
                            break;
                        case "D":
                            cardLongName += "Diamonds";
                            break;
                    }

            return cardLongName;
        }
    }
}
