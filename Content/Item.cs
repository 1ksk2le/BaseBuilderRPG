using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Item
    {
        public Texture2D texture { get; set; }
        public Color rarityColor { get; set; }
        public Vector2 position { get; set; }
        public int id { get; set; }
        public int prefixID { get; set; }
        public int suffixID { get; set; }
        public int damage { get; set; }
        public int rarity { get; set; }
        public int stackLimit { get; set; }
        public int stackSize { get; set; }
        public int shootID { get; set; }
        public float useTime { get; set; }
        public float shootSpeed { get; set; }
        public float knockBack { get; set; }
        public string texturePath { get; set; }
        public string name { get; set; }
        public string suffixName { get; set; }
        public string prefixName { get; set; }
        public string type { get; set; }
        public string damageType { get; set; }
        public string weaponType { get; set; }
        public bool onGround { get; set; }
        public bool canBeUsed { get; set; }

        public List<string> toolTips;
        public Rectangle rectangle;
        public Vector2 origin, center;
        public float levTimer = 0.0f;
        public bool didSpawn;

        private Random random;

        public Item(Texture2D texture, string texturePath, int id, string name, string type, string damageType, string weaponType, Vector2 position, float shootSpeed, int shoot, int rarity, int prefixID, int suffixID, int damage, float knockBack, float useTime, int stackLimit, int dropAmount, bool onGround)
        {
            this.texture = texture;
            this.texturePath = texturePath;
            this.id = id;
            this.name = name;
            this.type = type;
            this.rarity = rarity;
            this.prefixID = prefixID;
            this.suffixID = suffixID;
            this.useTime = useTime;
            this.knockBack = knockBack;
            this.damage = damage;
            this.damageType = damageType;
            this.weaponType = weaponType;
            this.position = position;
            this.onGround = onGround;

            if (this.type != "Weapon")
            {
                this.damageType = "";
                this.shootID = -1;
                this.damage = -1;
                this.prefixID = -1;
                this.suffixID = -1;
                this.useTime = -1;
                this.knockBack = -1;
                this.damage = -1;
                this.stackLimit = stackLimit;
                stackSize = this.stackLimit == 1 ? 1 : dropAmount;
            }
            else
            {
                this.stackLimit = 1;
                stackSize = 1;
                if (this.damageType != "ranged")
                {
                    this.shootID = -1;
                    this.shootSpeed = 0f;
                }
                else
                {
                    this.shootID = shoot;
                    this.shootSpeed = shootSpeed;
                }
            }


            SetDefaults();

            toolTips = new List<string>();

            toolTips.Add("[" + this.id + "] " + prefixName + " " + this.name + " " + suffixName);

            if (this.damage > 0)
            {
                toolTips.Add("[" + this.type + " - " + this.weaponType + "]");
                toolTips.Add("Damage: " + this.damage.ToString() + " " + this.damageType + " damage");
            }
            else
            {
                toolTips.Add("[" + this.type + "]");
            }
            if (this.useTime > 0)
            {
                toolTips.Add("Use Time: x" + (1f / this.useTime).ToString("F2") + " per second");
            }
            if (this.knockBack > -1)
            {
                toolTips.Add("Knockback: " + this.knockBack.ToString());
            }
            if (this.shootID > -1)
            {
                toolTips.Add("Shoot Speed: " + this.shootSpeed.ToString() + " pps");
            }
            TooltipsBasedOnID();

            didSpawn = false;

            random = Main_Globals.GetRandomInstance();
        }

        public void Update(GameTime gameTime, Global_Particle globalParticle)
        {
            levTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (!didSpawn)
            {
                origin = new Vector2(texture.Width / 2, texture.Height / 2);
            }
            center = position + origin;
            rectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);

            int numberOfParticles = 16;
            float particleSpeed = 15f;

            Vector2 particleOffset = new Vector2(-texture.Width / 4, texture.Height / 2);

            for (int i = 0; i < numberOfParticles; i++)
            {
                Vector2 randomOffset = new Vector2(random.Next(texture.Width), random.NextFloat(-5f, 5f));

                Vector2 particlePosition = position + particleOffset + randomOffset;

                float particleScale = 1f * random.NextFloat(0.1f, 1f);

                Vector2 particleVelocity = new Vector2(0, -particleSpeed);
                if (random.Next(120) == 0)
                {
                    globalParticle.NewParticle(0, 0, particlePosition, particleVelocity, Vector2.Zero, 0f, 2.5f, particleScale, rarityColor);
                    //globalParticle.NewParticle(0, 0, particlePosition, -particleVelocity, Vector2.Zero, 0f, 3f, particleScale, rarityColor);
                }

            }
        }

        public bool PlayerClose(Player player, float pickRange)
        {
            float distance = Vector2.Distance(position, player.position);
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
            var inputManager = Input_Manager.Instance;
            Rectangle slotRect = new((int)position.X, (int)position.Y, texture.Width, texture.Height);
            return slotRect.Contains(inputManager.mousePosition);
        }

        public void SetDefaults()
        {
            switch (rarity)
            {
                case 0:
                    rarityColor = Color.LightGray;
                    break;

                case 1:
                    rarityColor = Color.White;
                    break;

                case 2:
                    rarityColor = new Color(30, 255, 0);
                    break;

                case 3:
                    rarityColor = new Color(0, 112, 221);
                    break;

                case 4:
                    rarityColor = new Color(163, 53, 238);
                    break;

                case 5:
                    rarityColor = Color.Gold;
                    break;

                case 6:
                    rarityColor = new Color(255, 128, 0);
                    break;

                case 7:
                    rarityColor = Color.Aqua;
                    break;

                default:
                    rarityColor = Color.Red;
                    break;
            }
            switch (prefixID)
            {
                case 0:
                    prefixName = "Broken";
                    break;

                case 1:
                    prefixName = "Reinforced";
                    break;

                case 2:
                    prefixName = "Magical";
                    break;

                case 3:
                    prefixName = "Unwieldy";
                    break;

                default:
                    prefixName = "";
                    break;
            }
            switch (suffixID)
            {
                case 0:
                    suffixName = "of Flames";
                    break;

                case 1:
                    suffixName = "of Death";
                    break;

                case 2:
                    suffixName = "of Arthur";
                    break;

                case 3:
                    suffixName = "of King";
                    break;

                default:
                    suffixName = "";
                    break;
            }
        }

        private void TooltipsBasedOnID()
        {
        }
    }
}