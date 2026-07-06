using MegaCrit.Sts2.Core.Commands;

namespace Squall.SquallCode.Extensions;

public static class AudioHelper
{
    private static readonly Random rng = new Random();
    
    private static readonly string[] attackSfx =
    {
        "res://Squall/sounds/attack_1.wav",
        "res://Squall/sounds/attack_2.wav",
        "res://Squall/sounds/attack_3.wav",
        "res://Squall/sounds/attack_4.wav"
    };
    
    private static readonly string[] damagedSfx =
    {
        "res://Squall/sounds/hit_low_1.wav",
        "res://Squall/sounds/hit_low_2.wav",
        "res://Squall/sounds/hit_low_3.wav",
    };
    
    private static readonly string[] highDamagedSfx =
    {
        "res://Squall/sounds/hit_med_1.wav",
        "res://Squall/sounds/hit_med_2.wav",
        "res://Squall/sounds/hit_med_3.wav",
    };
    
    private static readonly string[] criticalDamagedSfx =
    {
        "res://Squall/sounds/hit_high_1.wav",
        "res://Squall/sounds/hit_high_2.wav",
        "res://Squall/sounds/hit_high_3.wav",
        "res://Squall/sounds/hit_high_4.wav",
    };
    
    private static readonly string[] defendSfx =
    {
        "res://Squall/sounds/defend_1.wav",
        "res://Squall/sounds/defend_2.wav",
        "res://Squall/sounds/defend_3.wav",
        "res://Squall/sounds/defend_4.wav",
    };
    
    private static readonly string[] victorySfx =
    {
        "res://Squall/sounds/victory_1.wav",
        "res://Squall/sounds/victory_2.wav",
        "res://Squall/sounds/victory_3.wav",
        "res://Squall/sounds/victory_4.wav",
    };
    
    private static readonly string[] phraseSfx =
    {
        "res://Squall/sounds/nemure.wav",
        "res://Squall/sounds/kimeru.wav",
        "res://Squall/sounds/jaana.wav",
        "res://Squall/sounds/kokoda.wav",
        "res://Squall/sounds/ochiro.wav",
        "res://Squall/sounds/sora.wav",
        "res://Squall/sounds/nigasan.wav",
    };
    
    public static void PlayRandomAttack()
    {
        PlayRandom(attackSfx);
    }
    
    public static void PlayRandomDefend()
    {
        PlayRandom(defendSfx);
    }

    public static void PlayRandomPhrase()
    {
        PlayRandom(phraseSfx);
    }
    
    public static void PlayRandomDamaged()
    {
        PlayRandom(damagedSfx);
    }

    public static void PlayRandomDamagedHigh()
    {
        PlayRandom(highDamagedSfx);
    }

    public static void PlayRandomDamagedCritical()
    {
        PlayRandom(criticalDamagedSfx);
    }
    
    public static void PlayRandomVictory()
    {
        PlayRandom(victorySfx);
    }

    public static void PlayRandom(string[] pool)
    {
        int index = rng.Next(pool.Length);
        SfxCmd.Play(pool[index]);
    }
}