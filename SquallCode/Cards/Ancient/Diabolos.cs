using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Mechanics.GF;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Ancient;

public class Diabolos() : SquallCard (0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), IGFCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        new DynamicVar("HpPercent", 20)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DiabolosPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/gunblade_effect.wav")
            .Execute(choiceContext);

        //Gravity damage: a percentage of the target's current HP.
        //Unpowered so it doesn't scale with Strength or similar effects.
        if (play.Target is { IsAlive: true })
        {
            decimal bonus = Math.Ceiling(
                play.Target.CurrentHp * DynamicVars["HpPercent"].BaseValue / 100m);

            if (bonus > 0)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    play.Target,
                    bonus,
                    ValueProp.Unpowered,
                    this,
                    play);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HpPercent"].UpgradeValueBy(5m);
    }
}
