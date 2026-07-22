using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
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
        if (play.Target is not { IsAlive: true })
            return;
        
        decimal damage = Math.Ceiling(
            play.Target.CurrentHp * DynamicVars["HpPercent"].BaseValue / 100m);
        
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            
            AudioHelper.PlayRandomPhrase();
            float duration = squall.PlayAnimation(ownerCreature, "cast").total;
            squall.PlayVfxOnTarget(
                play.Target,
                "res://Squall/scenes/vfx.tscn",
                "demi"
            );
            SfxCmd.Play("res://Squall/sfx/dark_messenger_vfx_2.wav");
            await Task.Delay((int)(0.10f * 1000f));
        }
        
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
        DynamicVars["HpPercent"].UpgradeValueBy(4m);
    }
}
