using MegaCrit.Sts2.Core.Entities.Relics;

namespace Squall.SquallCode.Relics;

public class Lionheart() : FirepowerRelicBase
{
    protected override int FirepowerAmount => 4;

    public override RelicRarity Rarity => RelicRarity.Ancient;
}