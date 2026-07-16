using MegaCrit.Sts2.Core.Entities.Relics;

namespace Squall.SquallCode.Relics;

//Lets the player Junction an additional GF at the start of combat.
//Handled in FirepowerRelicBase's first-turn junction flow.
public class JunctionRing() : SquallRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
}
