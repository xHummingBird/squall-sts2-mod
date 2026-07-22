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

public class Diabolos() : SquallCard (0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies), IGFCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        new DynamicVar("HpPercent", 20)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DiabolosPower>()
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal damage;
        
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        var ownerCreature = Owner?.Creature;
        var targets = base.CombatState.HittableEnemies;
        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            float duration = squall.PlayAnimation(ownerCreature, "diabolos").total;
            SfxCmd.Play("res://Squall/sounds/summon.wav");
            foreach (var target in targets)
            {
                squall.PlayVfxOnTarget(
                    target,
                    "res://Squall/scenes/vfx.tscn",
                    "dark_messenger"
                );
            }
            await Task.Delay((int)(0.8f * 1000f));
            SfxCmd.Play("res://Squall/sfx/dark_messenger_sfx_1.wav");
            await Task.Delay((int)(0.9f * 1000f));
            SfxCmd.Play("res://Squall/sfx/dark_messenger_sfx_2.wav");
        }
        foreach (var target in targets)
        {
            decimal percentDamage = DynamicVars["HpPercent"].BaseValue / 100m;
            damage = base.DynamicVars.Damage.BaseValue + (target.CurrentHp * percentDamage);
            await CreatureCmd.Damage(choiceContext, target, damage, ValueProp.Unpowered, this, play);
        }
        await Task.Delay((int)(1.4f * 1000f));
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars["HpPercent"].UpgradeValueBy(5m);
    }
}
