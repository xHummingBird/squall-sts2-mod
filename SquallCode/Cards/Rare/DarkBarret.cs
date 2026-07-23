using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Rare;

public class DarkBarret() : SquallCard(1, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<FirepowerPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        new("HpLoss", 5m)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        bool hasFirePower = false;
        if (base.Owner.HasPower<FirepowerPower>())
            hasFirePower = true;
        decimal finalDamage = DynamicVars.Damage.BaseValue;

        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            AudioHelper.PlayRandomPhrase();
            float duration = squall.PlayAnimation(ownerCreature, "shoot").total;
            await Task.Delay((int)(0.26f * 1000f));
            squall.PlayVfxOnTarget(
                play.Target,
                "res://Squall/scenes/vfx.tscn",
                "demi"
            );
            SfxCmd.Play("res://Squall/sfx/dark_messenger_vfx_2.wav");
            await Task.Delay((int)(0.10f * 1000f));
            SfxCmd.Play("res://Squall/sfx/gunblade_explosion.wav");
        }
        if (hasFirePower)
            finalDamage = DynamicVars.Damage.BaseValue + ((DynamicVars["HpLoss"].BaseValue / 100m)* play.Target.MaxHp);
        await DamageCmd.Attack(finalDamage).FromCard(this, play).Targeting(play.Target)
            .Execute(choiceContext);
        await Task.Delay((int)(0.36f * 1000f));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
    }
}
