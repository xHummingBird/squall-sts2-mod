using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Rare;

public class PreemptiveShot() : SquallCard(0, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        new PowerVar<MarkedPower>(1m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MarkedPower>()
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Innate,
        CardKeyword.Exhaust
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
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}