using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceTo21
{
    /// <summary>
    /// Game Manager class.
    /// Handles game flow and scoring.
    /// </summary>
    public class Game
    {
        public int NumberOfPlayers { get; private set; } // number of players in current game
        List<Player> players = new List<Player>(); // list of objects containing player data
        CardTable cardTable; // object in charge of displaying game information
        Deck deck = new Deck(); // deck of cards
        int currentPlayer = 0; // current player on list
        public Task nextTask; // keeps track of game state
        private bool cheating = false; // lets you cheat for testing purposes if true
        private Player lastRoundWinner; //At the beginning of the game, there is no winner yet.

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

        private void SetNumberOfPlayers(int count)
        {
            NumberOfPlayers = count;
        }

        /// <summary>
        /// Figures out what task to do next in game as represented by field nextTask.
        /// Calls methods required to complete task then sets nextTask.
        /// </summary>
        public void DoNextTask()
        {
            Console.WriteLine("================================");

            if (nextTask == Task.GetNumberOfPlayers)
            {
                int playerCount = cardTable.GetNumberOfPlayers();
                SetNumberOfPlayers(playerCount); // To set the number of players using an internal method
                nextTask = Task.GetNames;
            }
            else if (nextTask == Task.GetNames)
            {
                for (var count = 1; count <= NumberOfPlayers; count++)
                {
                    var name = cardTable.GetPlayerName(count);
                    AddPlayer(name);
                }
                nextTask = Task.IntroducePlayers;
            }
            else if (nextTask == Task.IntroducePlayers)
            {
                cardTable.ShowPlayers(players);
                didAnyPlayerTakeACard = false;
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
                            cardTable.AnnounceWinner(player);
                            nextTask = Task.GameOver;
                            
                            AskPlayersIfContinue();
                           

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
                if (!didAnyPlayerTakeACard)
                {
                    Player winner = DoFinalScoring();
                    cardTable.AnnounceWinner(winner);
                    lastRoundWinner = winner;
                    nextTask = Task.GameOver;
                }
                else
                {
                    int activeOrStayingPlayers = 0;
                    Player lastStandingPlayer = null;

                    foreach (var player in players)
                    {
                        if (player.status != PlayerStatus.bust)
                        {
                            activeOrStayingPlayers++;
                            lastStandingPlayer = player;
                        }
                    }

                    if (activeOrStayingPlayers == 1 && lastStandingPlayer != null)
                    {
                        lastStandingPlayer.status = PlayerStatus.win;
                        cardTable.AnnounceWinner(lastStandingPlayer);
                        nextTask = Task.GameOver;
                    }
                    else if (!CheckActivePlayers())
                    {
                        Player winner = DoFinalScoring();
                        
                        cardTable.AnnounceWinner(winner);
                        nextTask = Task.GameOver;
                    }
                    else
                    {
                        currentPlayer = (currentPlayer + 1) % players.Count;
                        nextTask = Task.PlayerTurn;
                    }
                }
            }

            // 在游戏结束后执行询问玩家是否继续游戏的逻辑
            if (nextTask == Task.GameOver)
            {
                // 先询问玩家是否继续游戏
                AskPlayersIfContinue();
               

                // 如果游戏结束且所有玩家都选择继续游戏，则继续游戏
                if (nextTask == Task.GameOver)
                {
                    // 继续游戏后需要重新进入游戏循环
                    DoNextTask();
                    
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

        private void ResetPlayers()
        {
            foreach (var player in players)
            {
                player.cards.Clear();
                player.score = 0;
                player.status = PlayerStatus.active;
               
            }
            if (lastRoundWinner != null && players.Contains(lastRoundWinner))
            {
                players.Remove(lastRoundWinner);
                players.Add(lastRoundWinner);
            }
          
            
        }

        private void AskPlayersIfContinue()
        {
           // bool allAgreeToContinue = true; // Assuming everyone wants to continue
            List<Player> playersToContinue = new List<Player>(); //To store players who want to continue

            //Asking each player if they want to continue the game
            foreach (Player player in new List<Player>(players))
            {
                Console.WriteLine($"Player {player.name}, do you want to continue playing? (Y/N)");
                string response = Console.ReadLine().ToUpper();
                if (response == "N")
                {
                 //   allAgreeToContinue = false; // If anyone does not want to continue, mark as false
                    players.Remove(player); //If a player does not want to continue, remove them from the list of players
                }
                else
                {
                    playersToContinue.Add(player);
                }
            }


            if (playersToContinue.Count == 0) // no one wants too play
            {
                Console.WriteLine("no player, game over.");
                Environment.Exit(0); 
            }

            //If only one player remains, automatically declare that player as the winner
            else if (players.Count == 1)
            {
                Player winner = players[0];
                cardTable.AnnounceWinner(winner);
                Console.WriteLine("Since there is only one player in the game, that player automatically becomes the winner.");

                Console.WriteLine("Do you want to restart the game？(Y/N)");
                string response = Console.ReadLine().ToUpper();

                if (response == "Y")
                {
                    Console.WriteLine("Game start.");
                    players.Clear(); 
                    deck = new Deck();
                    deck.Shuffle();
                    ResetPlayers();
                    nextTask = Task.GetNumberOfPlayers;
                }
                else
                {
                    Console.WriteLine("Game over.");
                    Environment.Exit(0);
                }
            }

            else if (playersToContinue.Count > 1)
            {
                deck.Shuffle();

                Random rng = new Random();
                ResetPlayers();
                List<Player> shuffledPlayersToContinue = playersToContinue.OrderBy(a => rng.Next()).ToList();
                players = new List<Player>(shuffledPlayersToContinue);

                deck.ShowAllCards();
                currentPlayer = 0;
                nextTask = Task.PlayerTurn;
            }

            /* if (playersToContinue.Contains(lastRoundWinner))
             {
                 // 先从列表中移除赢家
                 playersToContinue.Remove(lastRoundWinner);
                 // 再将赢家添加到列表的末尾
                 playersToContinue.Add(lastRoundWinner);
             }
             //playersOrder = playersToContinue;
             players = new List<Player>(playersToContinue);//Update the players list to only include players who choose to continue.
         */
        }


    }
}
