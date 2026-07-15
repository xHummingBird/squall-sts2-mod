using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace Squall.SquallCode.Cards.Token;

public class Blizzaga() : SquallCard(0, CardType.Attack,
    CardRarity.Token, TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        new BlockVar(10m, ValueProp.Move)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            SfxCmd.Play("res://Squall/sounds/ice.wav");
            float duration = squall.PlayAnimation(ownerCreature, "cast").total;
            var targets = base.CombatState.HittableEnemies;
            if (duration > 0f)
                foreach (var target in targets)
                {
                    squall.PlayVfxOnTarget(
                        play.Target,
                        "res://Squall/scenes/vfx.tscn",
                        "ice_1"
                    );
                }
            await Task.Delay((int)(0.20f * 1000f));
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Squall/sfx/ice.wav")
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Blue);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    }
                }
            })
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(
            base.Owner.Creature,
            DynamicVars.Block.BaseValue,
            ValueProp.Move,
            play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
