using MegaCrit.Sts2.Core.Entities.Players;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Mechanics.Firepower;



public static class FirepowerHelper
{
    public static Revolver? GetRevolver(Player player)
        => player.GetRelic<Revolver>();

    public static Lionheart? GetLionheart(Player player)
        => player.GetRelic<Lionheart>();

    public static dynamic? GetFirepowerRelic(Player player)
    {
        return (dynamic?)player.GetRelic<Revolver>()
               ?? player.GetRelic<Lionheart>();
    }

    public static int GetFirepowerProgress(Player player)
    {
        var relic = GetFirepowerRelic(player);

        return relic?.GetFirepowerProgressForUI() ?? 0;
    }

    public static bool IsFirepowerCharged(Player player)
    {
        var relic = GetFirepowerRelic(player);

        return relic?.IsFirepowerChargedForUI() ?? false;
    }

    public static void GainProgress(Player player, int amount = 1)
    {
        var relic = GetFirepowerRelic(player);

        relic?.GainFirepowerProgress(amount);
    }

    public static void Consume(Player player)
    {
        var relic = GetFirepowerRelic(player);

        relic?.ConsumeFirepower();
    }
}

