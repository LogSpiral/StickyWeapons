using Terraria;
using Terraria.ModLoader;

namespace StickyWeapons;

public class StickyPlayer : ModPlayer
{
    public Item[] items = null;
    public int index = -1;
    public int max = -1;
    public bool moreTimeShoot;
}