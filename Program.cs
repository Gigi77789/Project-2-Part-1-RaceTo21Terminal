using System;

namespace RaceTo21
{
    class Program
    {
        /// <summary>
        /// Instantiate game elements. Repeatedly prompt game to update until end condition detected.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            CardTable cardTable = new CardTable();
            Game game = new Game(cardTable);
            while (game.nextTask != Task.GameOver)
            {
                game.DoNextTask();
            }
        }
    }
}

