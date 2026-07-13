using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Rare;

public class PerfectTrigger() : SquallCard(1, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(10m),
        new ExtraDamageVar(5m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) =>
                card.Owner.Creature.HasPower<FirepowerPower>() ? 1 : 0)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
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
            await Task.Delay(250);
            await squall.Retreat(ownerCreature);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
        else await CommonActions.CardAttack(this, play.Target)
            .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/hit_1.wav")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(4m);
        DynamicVars.ExtraDamage.UpgradeValueBy(2m);
    }
}
