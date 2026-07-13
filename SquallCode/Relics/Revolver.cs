using MegaCrit.Sts2.Core.Entities.Relics;

namespace Squall.SquallCode.Relics;

public class Revolver() : FirepowerRelicBase
{
    protected override int FirepowerAmount => 2;

    public override RelicRarity Rarity => RelicRarity.Starter;
}