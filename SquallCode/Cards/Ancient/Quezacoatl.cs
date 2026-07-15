using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Mechanics.GF;
using Squall.SquallCode.Powers;

namespace Squall.SquallCode.Cards.Ancient;

public class Quezacoatl() : SquallCard (0, CardType.Attack,
    CardRarity.Ancient, TargetType.RandomEnemy), IGFCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new RepeatVar(5)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<QuezacoatlPower>()
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
            float duration = squall.PlayAnimation(ownerCreature, "quezacoatl").total;
            SfxCmd.Play("res://Squall/sounds/summon.wav");
            await Task.Delay((int)(0.8f * 1000f));
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingRandomOpponents(base.CombatState)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .WithHitFx("vfx/vfx_attack_lightning", "event:/sfx/characters/defect/defect_lightning_passive")
            .Execute(choiceContext);
        await Task.Delay((int)(1.8f * 1000f));
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
