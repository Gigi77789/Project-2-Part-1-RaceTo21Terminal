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
            Console.WriteLine("================================");

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


        private void AskPlayersIfContinue()
        {
            bool allAgreeToContinue = true; // 假设所有人都想继续
            List<Player> playersToContinue = new List<Player>(); // 用于存储想继续的玩家

            // 询问每位玩家是否想继续游戏
            foreach (Player player in new List<Player>(players))
            {
                Console.WriteLine($"Player {player.name}, do you want to continue playing? (Y/N)");
                string response = Console.ReadLine().ToUpper();
                if (response == "N")
                {
                    allAgreeToContinue = false; // 如果有人不想继续，标记为false
                    players.Remove(player); // 如果玩家不想继续，从玩家列表中移除
                }
                else
                {
                    playersToContinue.Add(player); // 如果玩家想继续，加入到继续的列表中
                }
            }

            // 如果只剩下一个玩家，自动宣布该玩家为赢家
            if (players.Count == 1)
            {
                Player winner = players[0];
                cardTable.AnnounceWinner(winner);
                nextTask = Task.GameOver;
            }

            // 如果所有人都同意继续，可以继续游戏并重新洗牌    
            if (allAgreeToContinue)
            {
                deck.Shuffle();
                players = new List<Player>(playersToContinue);
                nextTask = Task.PlayerTurn; // 设置下一步任务为玩家回合
            }

            // 游戏继续后等待用户按下 Enter 键
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }


    }
}
