﻿using static Game.Minigames.BlackJack;

namespace Game.Minigames
{
    class BlackJack
    {
        public enum CardColor
        {
            clubs,
            pikes,
            diamonds,
            hearts
        }

        public enum BlackJackAction
        {
            pullCard,
            Stop,
            splitDeck
        }
        public struct Card
        {
            private static Dictionary<string, int> numeralvalues = new Dictionary<string, int>()
            {
                { "A", 11 },
                { "K", 10 },
                { "Q", 10 },
                { "J", 10 },
            };
            public static string[] CardColorSymbole = new string[]
            {
                //"♣",
                //"♠",
                //"♦",
                //"♥"
                "-",
                ".",
                "[",
                "]",
            };

            private CardColor color;
            private string numeral;
            private int value;

            public CardColor Color { get { return color; } }
            public string Numeral { get { return numeral; } }
            public int Value { get { return value; } }

            public Card(CardColor color, string numeral)
            {
                this.color = color;
                this.numeral = numeral;
                if (numeralvalues.ContainsKey(numeral))
                {
                    this.value = numeralvalues[numeral];
                    return;
                }
                if (int.TryParse(numeral, out int value) && value > 1 && value <= 10)
                {
                    this.value = value;
                    return;
                }
                throw new Exception("Nem létező kártya, rossz adatok");
            }

            public static List<Card> generateDeck()
            {
                List<Card> cards = new List<Card>();
                foreach (CardColor name in Enum.GetValues(typeof(CardColor)))
                {
                    for (int i = 2; i < 11; i++)
                    {
                        cards.Add(new Card(name, $"{i}"));
                    }
                    foreach (string key in numeralvalues.Keys)
                    {
                        cards.Add(new Card(name, key));
                    }
                }
                return cards;
            }

            public static List<Card> shuffleDeck(List<Card> cards, int howManyTimes = 1)
            {
                List<Card> newCards;
                Random r;
                int count = 0;

                do
                {
                    newCards = new List<Card>(cards);
                    r = new Random();

                    for (int i = 0; i < newCards.Count; i++)
                    {
                        int randomIndex = r.Next(cards.Count);

                        newCards[i] = cards[randomIndex];
                        cards.RemoveAt(randomIndex);
                    }

                    cards = newCards;
                    count++;
                }
                while (count < howManyTimes);


                return newCards;
            }

            public static void printDeck(List<Card> cards)
            {
                foreach (Card card in cards)
                {
                    Console.Write($"{Card.CardColorSymbole[(int)card.Color]}{card.Numeral}, ");
                }
            }
        }
        public struct BlackJackInventory
        {
            public List<Card> Cards { get; set; }
            public int Bet { get; set; }
            public bool isHidden { get; set; }
            public string Text { get; set; }
            public int CardsValue
            {
                get
                {
                    int value = 0;
                    int aces = 0;
                    foreach (Card card in Cards)
                    {
                        if (card.Numeral == "A")
                        {
                            aces++;
                        }
                        else
                        {
                            value += card.Value;
                        }

                    }
                    if (aces >= 1)
                    {
                        value += 1 * (aces - 1);
                        if (value + 11 > 21)
                        {
                            value += 1;
                        }
                        else
                        {
                            value += 11;
                        }
                    }


                    return value;
                }
            }
            public BlackJackInventory(List<Card> cards, int bet = 0, bool isHidden = false, string text = "")
            {
                this.Cards = cards;
                this.Bet = bet;
                this.isHidden = isHidden;
                this.Text = text;
            }

        }
        private List<Card> deck;
        private List<Card> allCards;
        private Player player;

        private BlackJackInventory playerInventory;
        private BlackJackInventory dealerInventory;

        private bool gameOver;

        public BlackJack(ref Player player)
        {
            this.deck = Card.shuffleDeck(Card.generateDeck(), 6);
            this.allCards = deck;
            this.player = player;
        }

        public void Run()
        {
            Console.Clear();

            if (player.Money <= 0)
            {
                return;
            }
            playerInventory = new BlackJackInventory(new List<Card>(), 0, false, "Lapjaid:\t ");
            dealerInventory = new BlackJackInventory(new List<Card>(), 0, true,  "Osztó lapjai:\t ");
            Bet();
            FirstDeal();
            while (!gameOver)
            {
                PrintTable();
                BlackJackAction action = Choose();
                switch (action)
                {
                    case BlackJackAction.pullCard:
                        Deal(ref playerInventory);
                        break;
                    case BlackJackAction.Stop:
                        dealerInventory.isHidden = false;
                        PrintTable();
                        while (dealerInventory.CardsValue <= 16)
                        {
                            Deal(ref dealerInventory);
                            PrintTable();
                        }
                        gameOver = true;
                        break;
                    case BlackJackAction.splitDeck: 
                        break;
                }
                if (playerInventory.CardsValue > 21)
                {
                    gameOver = true;
                }
            }
            PrintTable();
            #region KIÉRTÉKELÉS
            if(playerInventory.CardsValue > 21)
            {
                Console.WriteLine("Vesztettél!");
            }
            #endregion
        }

        private void Bet()
        {
            Program.PrintPlayerStat();
            Console.WriteLine("Mennyi pénzt teszel fel? ");
            string input;
            int bet;
            do
            {
                input = Console.ReadLine();

            } while (!int.TryParse(input, out bet) && bet > 0 && bet <= player.Money);

            playerInventory.Bet = bet;
            player.Money -= bet;
        }

        private void FirstDeal()
        {
            for (int i = 0; i < 2; i++)
            {
                if (deck.Count > 0)
                {
                    playerInventory.Cards.Add(deck[0]);
                    deck.RemoveAt(0);
                }
                if (deck.Count > 0)
                {
                    dealerInventory.Cards.Add(deck[0]);
                    deck.RemoveAt(0);
                }
            }
        }

        private void Deal(ref BlackJackInventory inventory)
        {
            if (deck.Count > 0)
            {
                inventory.Cards.Add(deck[0]);
                deck.RemoveAt(0);
            }
        }

        private void PrintTable()
        {
            foreach (BlackJackInventory inventory in new BlackJackInventory[] { playerInventory, dealerInventory})
            {
                Console.Write(inventory.Text);
                if (inventory.isHidden)
                {
                    Console.Write($"?? ");
                }
                else
                {
                    PrintCard(inventory.Cards[0]);
                }
                for (int i = 1; i < inventory.Cards.Count; i++)
                {
                    Card card = inventory.Cards[i];
                    PrintCard(card);
                }
                Console.WriteLine();
            }
            
        }

        private void PrintCard(Card card)
        {
            Console.Write($"{Card.CardColorSymbole[(int)card.Color]}{card.Numeral} ");
        }
        
        private BlackJackAction Choose()
        {
            bool isSplitable = false;
            int maxOptionsIndex = 1;
            maxOptionsIndex += (isSplitable ? 1 : 0);
            Console.WriteLine("Mit akarsz csinálni:");
            Console.WriteLine($"1. Húzz fel lapot");
            Console.WriteLine($"2. Passzolj");
            if (!isSplitable)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            Console.WriteLine($"3. Válaszd szét a paklit");
            Console.ForegroundColor = ConsoleColor.White;


            int choice;
            bool error = false;
            while (!int.TryParse(Console.ReadKey(true).KeyChar.ToString(), out choice) || (choice < 1 || choice > maxOptionsIndex + 1))
            {
                if (!error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ilyet nem tudsz csinálni!");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Próbáld újra!");
                    error = true;
                }
            }

            return (BlackJackAction) (choice - 1);
        }
    }
}