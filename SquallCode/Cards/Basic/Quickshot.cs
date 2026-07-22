using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Basic;

public class Quickshot() : SquallCard(0, CardType.Attack,
    CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        [
            new CalculationBaseVar(4m),
            new ExtraDamageVar(3m),
            new CalculatedDamageVar(ValueProp.Move)
                .WithMultiplier((CardModel card, Creature? _) =>
                    card.Owner.Creature.HasPower<FirepowerPower>() ? 1 : 0)
        ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            AudioHelper.PlayRandomPhrase();
            float duration = squall.PlayAnimation(ownerCreature, "shoot").total;
            if (duration > 0f)
                await Task.Delay((int)(0.36f * 1000f));
            SfxCmd.Play("res://Squall/sfx/gunblade_explosion.wav");
            SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");
            squall.PlayVfxOnTarget(
                play.Target,
                "res://Squall/scenes/vfx.tscn",
                "explosion"
            );
        }

        await CommonActions.CardAttack(this, play.Target)
            .Execute(choiceContext);
        await Task.Delay((int)(0.36f * 1000f));
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
    
