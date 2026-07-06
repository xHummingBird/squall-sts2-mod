using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Squall.SquallCode.Character;
using Squall.SquallCode.Extensions;
using Godot;

namespace Squall.SquallCode.Relics;

[Pool(typeof(SquallRelicPool))]
public abstract class SquallRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}