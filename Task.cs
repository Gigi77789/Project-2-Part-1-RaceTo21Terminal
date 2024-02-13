using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTo21
{
    /// <summary>
    /// Tracks game state. 
    /// Replaces less reliable string-based tracking in original.
    /// </summary>
    public enum Task
    {
        GetNumberOfPlayers,
        GetNames,
        IntroducePlayers,
        PlayerTurn,
        CheckForEnd,
        GameOver
    }
}
