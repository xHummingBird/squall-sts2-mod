using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Mechanics.GF;

namespace Squall.SquallCode.Cards.Token;

public class Graviga() : SquallCard(0, CardType.Attack,
    CardRarity.Token, TargetType.AllEnemies), IGFCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HpPercent", 15)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        SfxCmd.Play("res://Squall/sfx/gunblade_explosion.wav");

        //Gravity damage: a percentage of each enemy's current HP.
        //Unpowered so it doesn't scale with Strength or similar effects.
        foreach (var enemy in base.CombatState.HittableEnemies.ToList())
        {
            if (enemy is not { IsAlive: true })
                continue;

            decimal damage = Math.Ceiling(
                enemy.CurrentHp * DynamicVars["HpPercent"].BaseValue / 100m);

            if (damage > 0)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    enemy,
                    damage,
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
