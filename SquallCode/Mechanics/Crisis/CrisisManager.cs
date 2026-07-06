using MegaCrit.Sts2.Core.Entities.Players;

namespace Squall.SquallCode.Mechanics.Crisis;

public static class CrisisManager
{
    public class CrisisData
    {
        public int Value;
        public Action<int>? OnCrisisChanged;
    }

    private static readonly Dictionary<Player, CrisisData> _data = new();

    private const int MaxCrisis = 100;

    private static CrisisData GetData(Player player)
    {
        if (!_data.TryGetValue(player, out var data))
        {
            data = new CrisisData
            {
                Value = 0
            };

            _data[player] = data;
        }

        return data;
    }

    public static int GetCrisis(Player player)
    {
        return GetData(player).Value;
    }

    public static void SetCrisis(Player player, int value)
    {
        var data = GetData(player);

        // Clamp between 0 and 100
        value = Math.Max(0, Math.Min(value, MaxCrisis));

        if (data.Value == value)
            return;

        data.Value = value;

        // Notify UI
        data.OnCrisisChanged?.Invoke(value);
    }

    public static void GainCrisis(Player player, int amount)
    {
        SetCrisis(player, GetCrisis(player) + amount);
    }

    public static void SpendCrisis(Player player, int amount)
    {
        SetCrisis(player, GetCrisis(player) - amount);
    }

    public static bool IsFull(Player player)
    {
        return GetCrisis(player) >= MaxCrisis;
    }

    public static CrisisData GetDataForUI(Player player)
    {
        return GetData(player);
    }

    public static void Reset(Player player)
    {
        SetCrisis(player, 0);
    }
}
