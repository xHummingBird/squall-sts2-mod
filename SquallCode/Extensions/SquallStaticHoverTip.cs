using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Squall.SquallCode.Extensions;

public class SquallStaticHoverTip
{
    public static readonly IHoverTip Firepower = new HoverTip(
        new LocString("static_hover_tips", "SQUALL_FIREPOWER.title"),
        new LocString("static_hover_tips", "SQUALL_FIREPOWER.description")
    );

    public static readonly IHoverTip Crisis = new HoverTip(
        new LocString("static_hover_tips", "SQUALL_CRISIS.title"),
        new LocString("static_hover_tips", "SQUALL_CRISIS.description")
    );
}