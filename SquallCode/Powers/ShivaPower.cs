using MegaCrit.Sts2.Core.Entities.Powers;

namespace Squall.SquallCode.Powers;


public class ShivaPower : SquallPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}