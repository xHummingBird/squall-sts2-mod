using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Squall.SquallCode.Extensions;
using Squall.SquallCode.Mechanics.Crisis;

// namespace Squall.SquallCode.Cards.Basic;
//
// public class CrisisTest() : SquallCard(0, CardType.Skill,
//     CardRarity.Basic, TargetType.Self)
// {
//     protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
//     protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];
//
//     protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
//     {
//         CrisisManager.SetCrisis(base.Owner, 100);
//     }
//
//     protected override void OnUpgrade()
//     {
//         DynamicVars["Block"].UpgradeValueBy(3m);
//     }
// }