using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Cards.Ancient;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Mechanics.Crisis;
using Squall.SquallCode.Powers;
using Squall.SquallCode.Relics;

namespace Squall.SquallCode.Cards.Basic;

public class Renzokuken() : SquallCard(2, CardType.Attack,
    CardRarity.Basic, TargetType.AnyEnemy)
{
    private bool _finisher;
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<FinisherPower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(15, ValueProp.Move),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        _finisher = false;
        CardModel? selectedFinisher = null;
        var rd = CombatState.CreateCard<RoughDivide>(base.Owner);
        var fc = CombatState.CreateCard<FatedCircle>(base.Owner);
        var bz = CombatState.CreateCard<BlastingZone>(base.Owner);
        var lh = CombatState.CreateCard<LionheartCard>(base.Owner);
        
        Lionheart? lionheart = base.Owner?.GetRelic<Lionheart>();
        
        if (CrisisManager.GetCrisis(base.Owner) >= 100)
        {
            List<CardModel> cards;
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(rd);
                CardCmd.Upgrade(fc);
                CardCmd.Upgrade(bz);
                CardCmd.Upgrade(lh);

                cards = new()
                {
                    rd,
                    fc,
                    bz
                };
            }

            else
            {
                cards = new()
                {
                    rd,
                    fc,
                    bz
                };
            }

            if (lionheart != null)
            {
                cards.Add(lh);
            }
            selectedFinisher = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards.ToList(), base.Owner, canSkip: false);
            CrisisManager.SetCrisis(base.Owner, 0);
            _finisher = true;
        }
        
        var ownerCreature = Owner?.Creature;
        var squall = Owner?.Character as Character.Squall;

        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && squall != null)
        {
            SfxCmd.Play("res://Squall/sounds/ikuzo.wav");
            await squall.DashTo(ownerCreature, play.Target, distance: 300f, overrideAnim:"renzokuken");
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay(300);
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay(600);
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay(300);
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play).Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/gunblade_effect.wav") 
                .Execute(choiceContext);
            if (!_finisher)
            {
                await Task.Delay(350);
                await squall.Retreat(ownerCreature);
                CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            }
            else if (_finisher && selectedFinisher != null)
            {
                if (selectedFinisher is BlastingZone || selectedFinisher is FatedCircle)
                    await CardCmd.AutoPlay(choiceContext, selectedFinisher, null);
                else await CardCmd.AutoPlay(choiceContext, selectedFinisher, play.Target);
            }
        }
        
        else
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play).Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/gunblade_effect.wav")
                .Execute(choiceContext);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
    }
    protected override void OnUpgrade()
    {
        
    }
}