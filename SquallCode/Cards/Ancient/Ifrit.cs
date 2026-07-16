using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Mechanics.GF;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Ancient;

public class Ifrit() : SquallCard (0, CardType.Attack,
CardRarity.Ancient, TargetType.AllEnemies), IGFCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(18m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<IfritPower>()
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        var ownerCreature = Owner?.Creature;
        if (ownerCreature != null && Owner?.Character is Character.Squall squall)
        {
            float duration = squall.PlayAnimation(ownerCreature, "ifrit").total;
            SfxCmd.Play("res://Squall/sounds/summon_ifrit.wav");
            if (duration > 0f)
                await Task.Delay((int)(1.2f * 1000f));
        }
        {
            var enemies = base.CombatState.HittableEnemies.ToList();
            if (enemies.Count == 0)
                return;
            Vector2 center = Vector2.Zero;
            int count = 0;
            foreach (var enemy in enemies)
            {
                var node = NCombatRoom.Instance?.GetCreatureNode(enemy);
                if (node != null)
                {
                    center += node.GetBottomOfHitbox();
                    count++;
                }
            }
            if (count == 0)
                return;
            center /= count;
            NLargeMagicMissileVfx vfx = NLargeMagicMissileVfx.Create(center, new Color("50b598"));
            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
            await Cmd.Wait(vfx.WaitTime);
        }
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(base.CombatState)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
            .WithHitVfxSpawnedAtBase()
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Red);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                        SfxCmd.Play("event:/sfx/characters/attack_fire");
                    }
                }
            })
            .Execute(choiceContext);
        await Task.Delay((int)(1.9f * 1000f));
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
