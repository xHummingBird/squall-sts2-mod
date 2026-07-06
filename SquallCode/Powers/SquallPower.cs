using BaseLib.Abstracts;
using BaseLib.Extensions;
using Squall.SquallCode.Extensions;
using Godot;

namespace Squall.SquallCode.Powers;

public abstract class SquallPower : CustomPowerModel
{
    //Loads from Squall/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}