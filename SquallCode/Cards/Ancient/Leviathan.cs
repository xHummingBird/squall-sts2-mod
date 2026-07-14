using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Mechanics.GF;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Ancient;

public class Leviathan() : SquallCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies), IGFCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new PowerVar<VulnerablePower>(2m),
        new PowerVar<WeakPower>(2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<LeviathanPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(null, "res://Squall/sfx/ice.wav")
            .Execute(choiceContext);

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, play.Target,
            DynamicVars.Vulnerable.BaseValue,
            base.Owner.Creature, this);

        await PowerCmd.Apply<WeakPower>(
            choiceContext, play.Target,
            DynamicVars.Weak.BaseValue,
            base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
        DynamicVars.Weak.UpgradeValueBy(1m);
    }
}
