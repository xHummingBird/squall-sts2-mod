using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Mechanics.GF;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Ancient;

public class Shiva() : SquallCard (0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies), IGFCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
        new BlockVar(12m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ShivaPower>()
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        var ownerCreature = Owner?.Creature;
        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            float duration = squall.PlayAnimation(ownerCreature, "shiva").total;
            SfxCmd.Play("res://Squall/sounds/summon_shiva.wav");
            var targets = base.CombatState.HittableEnemies;
            if (duration > 0f)
                await Task.Delay((int)(0.9f * 1000f));
            foreach (var target in targets)
            {
                squall.PlayVfxOnTarget(
                    target,
                    "res://Squall/scenes/vfx.tscn",
                    "diamond_dust"
                );
            }
            await Task.Delay((int)(0.2333f * 1000f));
            SfxCmd.Play("res://Squall/sfx/ice.wav");
            await Task.Delay((int)(0.7f * 1000f));
            SfxCmd.Play("res://Squall/sfx/ice_2.wav");
        }
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play).TargetingAllOpponents(base.CombatState)
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                NGame.Instance.ScreenShake(ShakeStrength.TooMuch, ShakeDuration.Normal);
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
        await Task.Delay((int)(1.1f * 1000f));
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
