using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Item
    {
        public Texture2D Texture { get; set; }

        public int ID { get; set; }
        public int PrefixID { get; set; }
        public int SuffixID { get; set; }
        public int Damage { get; set; }
        public int Rarity { get; set; }
        public int StackLimit { get; set; }
        public int StackSize { get; set; }
        public int Shoot { get; set; }
        public float UseTime { get; set; }
        public float ShootSpeed { get; set; }
        public float KnockBack { get; set; }
        public string TexturePath { get; set; }
        public string Name { get; set; }
        public string SuffixName { get; set; }
        public string PrefixName { get; set; }
        public string Type { get; set; }
        public string DamageType { get; set; }
        public string WeaponType { get; set; }
        public Color RarityColor { get; set; }
        public Vector2 Position { get; set; }
        public bool OnGround { get; set; }
        public bool CanBeUsed { get; set; }
        public List<string> ToolTips { get; set; }

        public float LevitationTimer = 0.0f;

        public Item(Texture2D texture, string texturePath, int id, string name, string type, string damageType, string weaponType, Vector2 position, float shootSpeed, int shoot, int rarity, int prefixID, int suffixID, int damage, float knockBack, float useTime, int stackLimit, int dropAmount, bool onGround)
        {
            Texture = texture;
            TexturePath = texturePath;
            ID = id;
            Name = name;
            Type = type;
            Rarity = rarity;
            PrefixID = prefixID;
            SuffixID = suffixID;
            UseTime = useTime;
            KnockBack = knockBack;
            Damage = damage;
            DamageType = damageType;
            WeaponType = weaponType;
            Position = position;
            OnGround = onGround;

            if (Type != "Weapon")
            {
                DamageType = "";
                Shoot = -1;
                Damage = -1;
                PrefixID = -1;
                SuffixID = -1;
                UseTime = -1;
                KnockBack = -1;
                Damage = -1;
                StackLimit = stackLimit;
                StackSize = StackLimit == 1 ? 1 : dropAmount;
            }
            else
            {
                StackLimit = 1;
                StackSize = 1;
                if (DamageType != "ranged")
                {
                    Shoot = -1;
                    ShootSpeed = 0f;
                }
                else
                {
                    Shoot = shoot;
                    ShootSpeed = shootSpeed;
                }
            }
            SetDefaults();

            ToolTips = new List<string>();

            ToolTips.Add(PrefixName + " " + Name + " " + SuffixName);

            if (Damage > 0)
            {
                ToolTips.Add("[" + Type + " - " + WeaponType + "]");
                ToolTips.Add(Damage.ToString() + " " + DamageType + " damage");
            }
            else
            {
                ToolTips.Add("[" + Type + "]");
            }
            if (UseTime > 0)
            {
                ToolTips.Add((UseTime * 10).ToString() + " use time");
            }
            if (KnockBack > -1)
            {
                ToolTips.Add(KnockBack.ToString() + " knockback");
            }
            if (Shoot > -1)
            {
                ToolTips.Add(ShootSpeed.ToString() + " velocity");
            }
            TooltipsBasedOnID();
        }

        public void Update(GameTime gameTime)
        {
            LevitationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public bool PlayerClose(Player player, float pickRange)
        {
            float distance = Vector2.Distance(Position, player.Position);
            if (distance <= pickRange)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InteractsWithMouse()
        {
            Rectangle slotRect = new((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            return slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public void RemoveItem(List<Item> itemList)
        {
            itemList.Remove(this);
        }

        /* public Item Clone(bool onGround)
         {
             return new Item(Texture, TexturePath, ID, Name, Type, DamageType, WeaponType, Position, ShootSpeed, Rarity, Shoot, PrefixID, SuffixID, Damage, KnockBack, UseTime, StackLimit, StackSize, onGround);
         }

         public Item Clone(int itemID, int prefixID, int suffixID, int dropAmount, bool onGround)
         {
             return new Item(Texture, TexturePath, itemID, Name, Type, DamageType, WeaponType, Position, ShootSpeed, Rarity, Shoot, prefixID, suffixID, Damage, UseTime, StackLimit, dropAmount, OnGround);
         }

         public Item Clone(int itemID, int dropAmount, bool onGround)
         {
             return new Item(Texture, TexturePath, itemID, Name, Type, DamageType, WeaponType, Position, ShootSpeed, Rarity, Shoot, PrefixID, SuffixID, Damage, UseTime, StackLimit, dropAmount, onGround);
         }*/

        public void SetDefaults()
        {
            switch (Rarity)
            {
                case 0:
                    RarityColor = Color.LightGray;
                    break;

                case 1:
                    RarityColor = Color.White;
                    break;

                case 2:
                    RarityColor = new Color(30, 255, 0);
                    break;

                case 3:
                    RarityColor = new Color(0, 112, 221);
                    break;

                case 4:
                    RarityColor = new Color(163, 53, 238);
                    break;

                case 5:
                    RarityColor = Color.Gold;
                    break;

                case 6:
                    RarityColor = new Color(255, 128, 0);
                    break;

                case 7:
                    RarityColor = Color.Aqua;
                    break;

                default:
                    RarityColor = Color.Red;
                    break;
            }
            switch (PrefixID)
            {
                case 0:
                    PrefixName = "Broken";
                    break;

                case 1:
                    PrefixName = "Reinforced";
                    break;

                case 2:
                    PrefixName = "Magical";
                    break;

                case 3:
                    PrefixName = "Unwieldy";
                    break;

                default:
                    PrefixName = "";
                    break;
            }
            switch (SuffixID)
            {
                case 0:
                    SuffixName = "of Flames";
                    break;

                case 1:
                    SuffixName = "of Death";
                    break;

                case 2:
                    SuffixName = "of Arthur";
                    break;

                case 3:
                    SuffixName = "of King";
                    break;

                default:
                    SuffixName = "";
                    break;
            }
        }

        private void TooltipsBasedOnID()
        {
        }
    }
}