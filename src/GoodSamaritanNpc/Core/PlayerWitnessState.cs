namespace GoodSamaritanNpc;

internal sealed class PlayerWitnessState
{
    internal PlayerModeManager Player;
    internal double NextReportTime;

    internal PlayerWitnessState(PlayerModeManager player)
    {
        Player = player;
    }
}
