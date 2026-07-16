using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Mechanics.GF;

namespace Squall.SquallCode.Cards.Token;

public class Demi() : SquallCard(0, CardType.Attack,
    CardRarity.Token, TargetType.AnyEnemy), IGFCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HpPercent", 8)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        //Gravity damage: a percentage of the target's current HP.
        //Unpowered so it doesn't scale with Strength or similar effects.
        if (play.Target is not { IsAlive: true })
            return;

        SfxCmd.Play("res://Squall/sfx/gunblade_explosion.wav");

        decimal damage = Math.Ceiling(
            play.Target.CurrentHp * DynamicVars["HpPercent"].BaseValue / 100m);

        if (damage > 0)
        {
            await CreatureCmd.Damage(
                choiceContext,
                play.Target,
                damage,
                ValueProp.Unpowered,
                this,
                play);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HpPercent"].UpgradeValueBy(2m);
    }
}
