using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Uncommon;

public class DevastatingSlug() : SquallCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        {
            var ownerCreature = Owner?.Creature;

            if (ownerCreature != null && Owner?.Character is Character.Squall squall)
            {
                CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
                AudioHelper.PlayRandomAttack();
                await squall.DashTo(ownerCreature, play.Target, distance: 300f, overrideAnim:"attack_explosion");
                SfxCmd.Play("res://Squall/sounds/attack_special_4.wav");
                SfxCmd.Play("res://Squall/sfx/swing_1.wav");
                SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");
                await CommonActions.CardAttack(this, play.Target)
                    .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/hit_1.wav")
                    .Execute(choiceContext);
                await Task.Delay(200);
                await squall.Retreat(ownerCreature);
                CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            }
            else await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/hit_1.wav")
                .Execute(choiceContext);
            await PowerCmd.Apply<FreeAttackPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}