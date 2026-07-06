using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Squall.SquallCode.Mechanics.Crisis;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Extensions;

public static class SquallExtensions
{
    public static async Task CheckCrisisReady(
        this Creature creature,
        PlayerChoiceContext context,
        Creature source,
        CardModel? card)
    {
        var player = creature.Player;
        if (player == null)
            return;

        if (CrisisManager.IsFull(player) &&
            !creature.HasPower<FinisherPower>())
        {
            await PowerCmd.Apply<FinisherPower>(
                context,
                creature,
                1,
                creature,
                null
            );
        }
    }
    
    public static class CombatHelpers
    {
        private const string DefaultSfx = "res://Squall/sfx/swing_1.wav";
        private const string DefaultVfx = "vfx/vfx_attack_slash";

        public static async Task SquallFakeHit(
            Creature target,
            string? sfxPath = null)
        {
            if (target == null) return;

            SfxCmd.Play(sfxPath ?? DefaultSfx);
            VfxCmd.PlayOnCreatureCenter(target, DefaultVfx);
            SfxCmd.Play(target.Monster.TakeDamageSfx);
            SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");
            await CreatureCmd.TriggerAnim(target, "Hit", 0f);
            
            if (target.Monster?.HasHurtSfx == true)
            {
                SfxCmd.Play(target.Monster.HurtSfx);
            }
        }
    }
}