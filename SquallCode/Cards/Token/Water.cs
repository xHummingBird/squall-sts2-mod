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
using Squall.SquallCode.Mechanics.GF;

namespace Squall.SquallCode.Cards.Token;

public class Water() : SquallCard(0, CardType.Attack,
    CardRarity.Token, TargetType.AllEnemies), IGFCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            AudioHelper.PlayRandomPhrase();
            float duration = squall.PlayAnimation(ownerCreature, "cast").total;
            var targets = base.CombatState.HittableEnemies;
            if (duration > 0f)
                foreach (var target in targets)
                {
                    squall.PlayVfxOnTarget(
                        play.Target,
                        "res://Squall/scenes/vfx.tscn",
                        "water"
                    );
                }
            await Task.Delay((int)(0.167f * 1000f));
            SfxCmd.Play("res://Squall/sfx/water_2.wav");
            await Task.Delay((int)(0.1f * 1000f));
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, null)
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
        await Task.Delay((int)(0.4f * 1000f));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
