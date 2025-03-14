﻿namespace ServerCode.Models
{
    //Table과 동일
    public class ItemInfo
    {
        public int itemId;
        public string itemName;
        public ItemType itemType;
        public int itemMaxStack;
        public ItemInfo(int id,string name, ItemType type,int maxStack)
        {
            itemId = id;
            itemName = name;
            itemType = type;
            itemMaxStack = maxStack;
        }
    }
    public class ItemInfos
    {
        public List<ItemInfo> items;
        public ItemInfos(List<ItemInfo> itemInfos)
        {
            items = itemInfos;
        }
    }
}
