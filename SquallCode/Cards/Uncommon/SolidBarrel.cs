using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Uncommon;

public class SolidBarrel() : SquallCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(14, ValueProp.Move),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var squall = Owner?.Character as Character.Squall;

        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (base.Owner.HasPower<FirepowerPower>())
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, base.Owner);
        if (ownerCreature != null && squall != null)
        {
            await squall.DashTo(ownerCreature, play.Target, distance: 300f);
            float duration = squall.PlayAnimation(ownerCreature, "solid_barrel").total;
            AudioHelper.PlayRandomPhrase();
            await Task.Delay((int)(0.19f * 1000f));
            SfxCmd.Play("res://Squall/sfx/hit_1.wav");
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay((int)(0.3f * 1000f));
            SfxCmd.Play("res://Squall/sfx/hit_2.wav");
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay((int)(0.533f * 1000f));
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay((int)(0.267f * 1000f));
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay((int)(0.267f * 1000f));
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay((int)(0.267f * 1000f));
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            SfxCmd.Play("res://Squall/sfx/gunblade_explosion.wav");
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx("res://Squall/sfx/gunblade_effect.wav")
                .Execute(choiceContext);
            await Task.Delay((int)(0.350f * 1000f));
            await squall.Retreat(ownerCreature);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
        else await CommonActions.CardAttack(this, play.Target)
            .WithHitFx("res://Squall/sfx/gunblade_effect.wav")
            .Execute(choiceContext);
       
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}