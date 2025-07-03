using System.IO;
using Terraria.ModLoader.IO;

namespace StickyWeapons.Items;

partial class StickyItem
{
    public override void SaveData(TagCompound tag)
    {
        tag.Add("item1", ItemIO.Save(SubItem1));
        tag.Add("item2", ItemIO.Save(SubItem2));
    }

    public override void LoadData(TagCompound tag)
    {
        ItemIO.Load(SubItem1, tag.Get<TagCompound>("item1"));
        ItemIO.Load(SubItem2, tag.Get<TagCompound>("item2"));
        SetDefaults();
    }

    public override void NetReceive(BinaryReader reader)
    {
        ItemIO.Receive(SubItem1, reader, true, true);
        ItemIO.Receive(SubItem2, reader, true, true);
        SetDefaults();
    }

    public override void NetSend(BinaryWriter writer)
    {
        ItemIO.Send(SubItem1, writer, true, true);
        ItemIO.Send(SubItem2, writer, true, true);
    }
}