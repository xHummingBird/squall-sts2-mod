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

namespace Squall.SquallCode.Cards.Ancient;

public class Renzokuken() : SquallCard(2, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy)
{
    private bool _finisher;
    public bool IsPerformingFinisher => _finisher;
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<FinisherPower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(14, ValueProp.Move),
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
            if (base.IsUpgraded)
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
            CrisisManager.SetCrisis(base.Owner, 0);
            _finisher = true;
        }
        
        var ownerCreature = Owner?.Creature;
        var squall = Owner?.Character as Character.Squall;

        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);

        Func<string, int, Task> fakeHit = async (sfx, delay) =>
        {
            SfxCmd.Play(sfx);
            SquallExtensions.CombatHelpers.SquallFakeHit(play.Target);
            await Task.Delay(delay);
        };
        
        if (ownerCreature != null && squall != null)
        {
            SfxCmd.Play("res://Squall/sounds/ikuzo.wav");
            await squall.DashTo(ownerCreature, play.Target, distance: 300f, overrideAnim:"renzokuken");
            await fakeHit("res://Squall/sfx/hit_1.wav", 300);
            await fakeHit("res://Squall/sfx/hit_2.wav", 600);
            await fakeHit("res://Squall/sfx/hit_3.wav", 300);
            await fakeHit("res://Squall/sfx/hit_1.wav", 500);
            SfxCmd.Play("res://Squall/sfx/hit_2.wav");
            SfxCmd.Play("res://Squall/sounds/attack_special_4.wav");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, play).Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash", "res://Squall/sfx/gunblade_effect.wav") 
                .Execute(choiceContext);
            if (!_finisher || selectedFinisher == bz)
            {
                await Task.Delay(200);
                await squall.Retreat(ownerCreature, "renzokuken_return", true, 0.467f);
                if (!_finisher)
                    CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
                else if (selectedFinisher != null)
                {
                    CrisisManager.SetCrisis(base.Owner, 0);
                    await CardCmd.AutoPlay(choiceContext, selectedFinisher, null);
                }
            }
            else if (_finisher && selectedFinisher != null){
                CrisisManager.SetCrisis(base.Owner, 0);
                await CardCmd.AutoPlay(choiceContext, selectedFinisher, play.Target);
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