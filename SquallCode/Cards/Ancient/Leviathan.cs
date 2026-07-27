using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
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

public class Leviathan() : SquallCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies), IGFCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new PowerVar<VulnerablePower>(2m),
        new PowerVar<WeakPower>(2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<LeviathanPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>()
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
            float duration = squall.PlayAnimation(ownerCreature, "leviathan").total;
            SfxCmd.Play("res://Squall/sounds/summon_leviathan.wav");
            var targets = base.CombatState.HittableEnemies;
            if (duration > 0f)
                await Task.Delay((int)(0.533f * 1000f));
            foreach (var target in targets)
            {
                squall.PlayVfxOnTarget(
                    target,
                    "res://Squall/scenes/vfx.tscn",
                    "flood"
                );
            }
            SfxCmd.Play("res://Squall/sfx/water_2.wav");
            await Task.Delay((int)(0.667f * 1000f));
            SfxCmd.Play("res://Squall/sfx/water_2.wav");
            await Task.Delay((int)(0.333f * 1000f));
            SfxCmd.Play("res://Squall/sfx/water_2.wav");
            await Task.Delay((int)(0.5f * 1000f));
        }
        DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play).TargetingAllOpponents(base.CombatState)
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
        await Task.Delay((int)(0.533f * 1000f));
        SfxCmd.Play("res://Squall/sfx/water.mp3");
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, base.CombatState.HittableEnemies,
            DynamicVars.Vulnerable.BaseValue,
            base.Owner.Creature, this);

        await PowerCmd.Apply<WeakPower>(
            choiceContext, base.CombatState.HittableEnemies,
            DynamicVars.Weak.BaseValue,
            base.Owner.Creature, this);
        
        await Task.Delay((int)(0.7f * 1000f));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
        DynamicVars.Weak.UpgradeValueBy(1m);
    }
}
