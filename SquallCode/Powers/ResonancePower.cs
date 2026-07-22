using MegaCrit.Sts2.Core.Entities.Powers;

namespace Squall.SquallCode.Powers;

/// <summary>
/// Junction bonuses apply Amount additional times.
/// Read by FirepowerRelicBase when resolving Junction bonuses.
/// </summary>
public class ResonancePower : SquallPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
