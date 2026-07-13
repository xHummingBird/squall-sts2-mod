using BaseLib.Extensions;
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

namespace Squall.SquallCode.Cards.Common;

public class UpperBlues() : SquallCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FirepowerPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        bool gainBlock = false;
        if (base.Owner.HasPower<FirepowerPower>())
            gainBlock = true;
        var ownerCreature = Owner?.Creature;
        var squall = Owner?.Character as Character.Squall;
        
        if (ownerCreature != null && squall != null)
        {
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            AudioHelper.PlayRandomAttack();
            await squall.DashTo(ownerCreature, play.Target, distance: 300f);
            squall.PlayAnimation(ownerCreature, "upper_blues");
            SfxCmd.Play("res://Squall/sfx/swing_1.wav");
            SfxCmd.Play("res://Squall/sfx/gunblade_effect.wav");
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/hit_1.wav")
                .Execute(choiceContext);
            await Task.Delay(880);
            await squall.Retreat(ownerCreature);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
        else
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/hit_1.wav")
                .Execute(choiceContext);
        if (gainBlock == true)
            await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Damage.PreviewValue, ValueProp.Unpowered, play);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}