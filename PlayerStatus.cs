using System;
namespace RaceTo21
{
	/// <summary>
	/// Player's current participation in game.
	/// "active" allows player to continue taking cards.
	/// All others skip this player until game end.
	/// </summary>
	public enum PlayerStatus
	{
		active,
		stay,
		bust,
		win
	}
}

