using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Cards.Ancient;

public class HeartOfLion() : SquallCard(2, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        CardModel? selectedFinisher = null;
        var rd = CombatState.CreateCard<RoughDivide>(base.Owner);
        var fc = CombatState.CreateCard<FatedCircle>(base.Owner);
        var bz = CombatState.CreateCard<BlastingZone>(base.Owner);
        var lh = CombatState.CreateCard<LionheartCard>(base.Owner);
        
        Lionheart? lionheart = base.Owner?.GetRelic<Lionheart>();
        SleepingLion? sleepingLion = base.Owner?.GetRelic<SleepingLion>();
        
            if (sleepingLion != null)
            {
                CardCmd.Upgrade(rd);
                CardCmd.Upgrade(fc);
                CardCmd.Upgrade(bz);
                CardCmd.Upgrade(lh);
            }

            List<CardModel> cards = [rd, fc, bz];

            if (lionheart != null)
            {
                cards.Add(lh);
            }
            selectedFinisher = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards.ToList(), base.Owner, canSkip: false);
        
        var ownerCreature = Owner?.Creature;
        var squall = Owner?.Character as Character.Squall;

        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        
        if (ownerCreature != null && squall != null)
        {
            if (selectedFinisher == bz)
                await CardCmd.AutoPlay(choiceContext, selectedFinisher, null);
            
            else if (selectedFinisher != null)
            {
                await squall.DashTo(ownerCreature, play.Target, distance: 300f);
                await CardCmd.AutoPlay(choiceContext, selectedFinisher, play.Target);
            }
        }
    }
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}