using System;
using System.Collections.Generic;

namespace RaceTo21
{
    /// <summary>
    /// Game Manager class.
    /// Handles game flow and scoring.
    /// </summary>
    public class Game
    {
        int numberOfPlayers; // number of players in current game
        List<Player> players = new List<Player>(); // list of objects containing player data
        CardTable cardTable; // object in charge of displaying game information
        Deck deck = new Deck(); // deck of cards
        int currentPlayer = 0; // current player on list
        public Task nextTask; // keeps track of game state
        private bool cheating = false; // lets you cheat for testing purposes if true

        private bool didAnyPlayerTakeACard = false; //Add a variable to track whether any player has drawn a card.
        /// <summary>
        /// Game Manager constructor.
        /// </summary>
        /// <param name="c">A CardTable instance, which manages player input & output</param>
        public Game(CardTable c)
        {
            cardTable = c;
            deck.Shuffle();
            deck.ShowAllCards();
            nextTask = Task.GetNumberOfPlayers;
        }

        /// <summary>
        /// Adds a player to the current game.
        /// Called by DoNextTask() method.
        /// </summary>
        /// <param name="n">Player name</param>
        public void AddPlayer(string n)
        {
            players.Add(new Player(n));
        }

        /// <summary>
        /// Figures out what task to do next in game as represented by field nextTask.
        /// Calls methods required to complete task then sets nextTask.
        /// </summary>
        public void DoNextTask()
        {
            Console.WriteLine("================================"); // this line should be elsewhere right?
            if (nextTask == Task.GetNumberOfPlayers)
            {
                numberOfPlayers = cardTable.GetNumberOfPlayers();
                nextTask = Task.GetNames;
            }
            else if (nextTask == Task.GetNames)
            {
                for (var count = 1; count <= numberOfPlayers; count++)
                {
                    var name = cardTable.GetPlayerName(count);
                    AddPlayer(name); // NOTE: player list will start from 0 index even though we use 1 for our count here to make the player numbering more human-friendly
                }
                nextTask = Task.IntroducePlayers;
            }
            else if (nextTask == Task.IntroducePlayers)
            {
                cardTable.ShowPlayers(players);
                didAnyPlayerTakeACard = false; // Reset the game to a state where no player has drawn a card.
                nextTask = Task.PlayerTurn;
            }
            else if (nextTask == Task.PlayerTurn)
            {
                
                Player player = players[currentPlayer];
                if (player.status == PlayerStatus.active)
                {
                    if (cardTable.OfferACard(player))
                    {
                        Card card = deck.DealTopCard();
                        player.cards.Add(card);
                        player.score = ScoreHand(player);
                        didAnyPlayerTakeACard = true;

                        if (player.score > 21)
                        {
                            player.status = PlayerStatus.bust;
                        }
                        else if (player.score == 21)
                        {
                            player.status = PlayerStatus.win;

                            //"If the player's score reaches 21,immediately declare victory and end the game.
                            cardTable.AnnounceWinner(player);
                            nextTask = Task.GameOver;
                            return;
                        }
                    }
                    else
                    {
                        player.status = PlayerStatus.stay;
                    }
                }
                cardTable.ShowHand(player);
                nextTask = Task.CheckForEnd;
            }
            else if (nextTask == Task.CheckForEnd)
            {
                // End the game if no player took a card this round
                if (!didAnyPlayerTakeACard)
                {
                    
                    //Directly end the game here, or choose an appropriate ending method according to your game rules
                    Player winner = DoFinalScoring(); 
                    cardTable.AnnounceWinner(winner);
                    nextTask = Task.GameOver;
                    return; 
                }

                // Initialize counter and variable to store the last non-busted player
                int activeOrStayingPlayers = 0;
                Player lastStandingPlayer = null; // To record the non-busted player

                // Iterate through the list of players to count non-busted players
                foreach (var player in players)
                {
                    if (player.status != PlayerStatus.bust)
                    {
                        activeOrStayingPlayers++; // Update count of non-busted players
                        lastStandingPlayer = player; // Record the current non-busted player
                    }
                }

                // If there's only one player who hasn't busted, that player wins
                if (activeOrStayingPlayers == 1 && lastStandingPlayer != null)
                {
                    // Set the player's status to win and announce them as the winner
                    lastStandingPlayer.status = PlayerStatus.win;
                    cardTable.AnnounceWinner(lastStandingPlayer);
                    nextTask = Task.GameOver; //Update game state to end
                }
                else if (!CheckActivePlayers())
                {
                    //If there are no active players left, do final scoring and announce the winner
                    Player winner = DoFinalScoring();
                    cardTable.AnnounceWinner(winner);
                    nextTask = Task.GameOver; // Update game state to end
                }
                else
                {
                    // If the game hasn't ended, prepare for the next player's turn
                    currentPlayer = (currentPlayer + 1) % players.Count; // Loop through players
                    nextTask = Task.PlayerTurn; //Set the next task as player's turn
                }

                
                if (nextTask == Task.GameOver)
                {
                    PlayAgain();
                }
            }
        }

        /// <summary>
        /// Score the cards in the player's list of cards OR (if cheating)
        /// ask for a value and set player score to it.
        /// </summary>
        /// <param name="player">Instance representing one player</param>
        /// <returns></returns>
        public int ScoreHand(Player player)
        {
            int score = 0;
            if (cheating == true && player.status == PlayerStatus.active)
            {
                string response = null;
                while (int.TryParse(response, out score) == false)
                {
                    Console.Write("OK, what should player " + player.name + "'s score be?");
                    response = Console.ReadLine();
                }
                return score;
            }
            else
            {
                foreach (Card card in player.cards)
                {
                    string faceValue = card.ID.Remove(card.ID.Length - 1);
                    switch (faceValue)
                    {
                        case "K":
                        case "Q":
                        case "J":
                            score = score + 10;
                            break;
                        case "A":
                            score = score + 1;
                            break;
                        default:
                            score = score + int.Parse(faceValue);
                            break;
                    }
                }
                /* Alternative method of handling the above foreach loop using char math instead of strings.
                 * No need to do this; just showing you a trick!
                 */
                //foreach (Card card in player.cards)
                //{
                //    char faceValue = card.ID[0];
                //    switch (faceValue)
                //    {
                //        case 'K':
                //        case 'Q':
                //        case 'J':
                //            score = score + 10;
                //            break;
                //        case 'A':
                //            score = score + 1;
                //            break;
                //        default:
                //            score = score + (faceValue - '0'); // clever char math!
                //            break;
                //    }
                //}
            }
            return score;
        }

        /// <summary>
        /// Checks if any player remain active
        /// </summary>
        /// <returns>true if any player still can take a turn</returns>
        public bool CheckActivePlayers()
        {
            /* Reminder that var is perfectly OK in C# unlike in JavaScript; it is handy for temporary variables! */
            foreach (var player in players)
            {
                if (player.status == PlayerStatus.active)
                {
                    return true; // at least one player is still going!
                }
            }
            return false; // everyone has stayed or busted, or someone won!
        }

        /// <summary>
        /// Check win conditions from best to worst:
        /// player hit 21, player scored highest, player didn't bust
        /// </summary>
        /// <returns>winning player or null if everyone busted</returns>
        public Player DoFinalScoring()
        {
            int highScore = 0;
            foreach (var player in players)
            {
                cardTable.ShowHand(player);
                if (player.status == PlayerStatus.win) // someone hit 21
                {
                    return player;
                }
                if (player.status == PlayerStatus.stay) // still could win...
                {
                    if (player.score > highScore)
                    {
                        highScore = player.score;
                    }
                }
                // if busted don't bother checking!
            }
            if (highScore > 0) // someone scored, anyway!
            {
                // find the FIRST player in list who meets win condition
                return players.Find(player => player.score == highScore);
            }
            return null; // everyone must have busted because nobody won!
        }

        private void PlayAgain()
        {
            Console.WriteLine("Play again? Y for Yes, N for No.");
            string answer = Console.ReadLine().ToUpper();

            if (answer == "Y")
            {
                RestartGame();
            }
            else
            {
                Console.WriteLine("Game over.");
                Environment.Exit(0);
            }
        }

        private void RestartGame()
        {
            deck = new Deck();
            deck.Shuffle(); 

            var rng = new Random();
            players.Sort((x, y) => rng.Next(-1, 2));

            // Reset game state
            currentPlayer = 0;
            nextTask = Task.GetNumberOfPlayers;
            didAnyPlayerTakeACard = false;

            Console.WriteLine("Let's start a new game!");
            DoNextTask();
        }

        private void AskPlayersIfContinue()
        {
            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                if (player.status == PlayerStatus.active)
                {
                    Console.WriteLine($"Player {player.name}, do you want to continue playing? (Y/N)");
                    string response = Console.ReadLine().ToUpper();
                    if (response == "N")
                    {
                        players.RemoveAt(i);
                        i--; // Adjust index since player is removed
                    }
                }
            }
        }
    }
}
