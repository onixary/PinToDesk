using PinToDesk.Models;

namespace PinToDesk.Common;

internal sealed record TodoSwatch(TodoColor Value, string Name, string Chip, string Light, string Dark);

internal static class TodoPalette
{
    internal static readonly TodoSwatch[] Options =
    {
        new(TodoColor.None,    "无色", "#FFFFFF", "Transparent", "Transparent"),
        new(TodoColor.Rose,    "玫瑰", "#E7A8B2", "#EAAEBB", "#51383E"),
        new(TodoColor.Apricot, "杏橙", "#ECB28C", "#EFBA95", "#534034"),
        new(TodoColor.Honey,   "蜂蜜", "#E9CD83", "#E9CD85", "#514831"),
        new(TodoColor.Sage,    "鼠尾草", "#AFCE9E", "#B3D09E", "#3C4936"),
        new(TodoColor.Aqua,    "青碧", "#92CCC2", "#9DCDC1", "#304B47"),
        new(TodoColor.Sky,     "天蓝", "#9BBFE7", "#A4C4E8", "#344659"),
        new(TodoColor.Lilac,   "丁香", "#C5ACE1", "#C8B1E1", "#483B55")
    };
}
