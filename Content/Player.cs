using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Player
    {
        public Inventory inventory { get; private set; }
        public Vector2 position { get; set; }
        public int health { get; set; }
        public int maxHealth { get; set; }
        public float skinColorFloat { get; set; }
        public bool isControlled { get; set; }
        public string name { get; set; }

        public NPC target;
        public List<Projectile> ownedProjectiles;
        public Item equippedWeapon, equippedOffhand, equippedBodyArmor, equippedHeadArmor, equippedBoots, mouseItem, hoveredItem;
        public Texture2D textureBody;
        public Texture2D textureHead;
        public Texture2D textureEye;
        public Color skinColor;
        public Vector2 velocity, origin, center, targetMovement;
        public Rectangle rectangle, rectangleMelee;
        public int direction = 1;
        public int width, height;
        public float meleeRange, rangedRange, speed, useTimer, immunityTime, hitEffectTimer, rotationAngle, immunityTimeMax, hitEffectTimerMax, eyeTimer;
        public bool inventoryVisible, isImmune, isPicked, hasMovementOrder;
        public string aiState;
        public Player_AIHandler aiHandler;
        public Player_VisualHandler visualHandler;
        public Player_ControlHandler controlHandler;

        public Player(Texture2D bodyTexture, Texture2D headTexture, Texture2D eyeTexture, string name, Vector2 position, int healthMax, float skinColor, bool isActive)
        {
            this.skinColorFloat = skinColor;
            this.position = position;
            this.name = name;

            textureBody = bodyTexture;
            textureHead = headTexture;
            textureEye = eyeTexture;
            isControlled = isActive;
            maxHealth = healthMax;
            health = maxHealth;
            immunityTimeMax = 0.5f;
            speed = 1.5f;
            eyeTimer = 0f;
            immunityTime = 0f;
            hitEffectTimer = 0f;
            hitEffectTimerMax = 0.75f;
            targetMovement = center;
            width = textureBody.Width;
            height = textureBody.Height;
            origin = new Vector2(width / 2, height / 2);
            inventory = new Inventory(5, 6);
            inventoryVisible = true;
            hasMovementOrder = false;
            isPicked = false;
            aiState = "";

            ownedProjectiles = new List<Projectile>();

            visualHandler = new Player_VisualHandler(this);
            aiHandler = new Player_AIHandler(this, visualHandler);
            controlHandler = new Player_ControlHandler(this, visualHandler);
        }

        public void Update(GameTime gameTime, Dictionary<int, Item> itemDictionary, Item_Globals globalItem, Text_Manager textManager, Projectile_Globals globalProjectile, Particle_Globals globalParticleBelow, Particle_Globals globalParticleAbove, List<NPC> npcs, List<Item> groundItems, List<Item> items)
        {
            var inputManager = Input_Manager.Instance;
            rectangle = new Rectangle((int)position.X, (int)position.Y, width, height);
            center = position + origin;

            if (equippedWeapon != null)
            {
                meleeRange = (equippedWeapon.damageType == "melee") ? 200f : 0;
                if (equippedWeapon.damageType == "ranged" || equippedWeapon.damageType == "magic")
                {
                    rangedRange = globalProjectile.GetProjectile(equippedWeapon.shootID).lifeTimeMax * equippedWeapon.shootSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
                }
                else
                {
                    rangedRange = 0f;
                }
                if (equippedWeapon.damageType == "melee")
                {
                    rectangleMelee = new Rectangle((int)(center.X - equippedWeapon.texture.Height / 2 - width / 2), (int)(center.Y - equippedWeapon.texture.Height / 2 - height / 4), (int)(width + equippedWeapon.texture.Height), (int)(height / 2 + equippedWeapon.texture.Height));
                }
                else
                {
                    rectangleMelee = new Rectangle(0, 0, 0, 0);
                }
            }

            if (Main.random.Next(100) == 0)
            {
                eyeTimer = 0.25f;
            }

            if (eyeTimer > 0f)
            {
                eyeTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (immunityTime >= 0f)
            {
                isImmune = true;
                immunityTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                isImmune = false;
            }

            if (hitEffectTimer >= 0f)
            {
                hitEffectTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            equippedWeapon = inventory.equipmentSlots[0].equippedItem != null ? inventory.equipmentSlots[0].equippedItem : null;
            equippedBodyArmor = inventory.equipmentSlots[1].equippedItem != null ? inventory.equipmentSlots[1].equippedItem : null;
            equippedOffhand = inventory.equipmentSlots[2].equippedItem != null ? inventory.equipmentSlots[2].equippedItem : null;
            equippedHeadArmor = inventory.equipmentSlots[4].equippedItem != null ? inventory.equipmentSlots[4].equippedItem : null;

            if (health > 0f && !Main.isConsoleVisible)
            {
                if (!isControlled)
                {
                    aiHandler.ProcessAI(gameTime, npcs, globalProjectile);
                }
                else
                {
                    controlHandler.Movement(Vector2.Zero, inputManager.currentKeyboardState);
                    controlHandler.UseItem(gameTime, globalProjectile, inputManager.mousePosition);
                    controlHandler.PlayerInventoryInteractions(Keys.I, groundItems, textManager, itemDictionary, globalItem, items);
                    aiState = "";
                }
                visualHandler.ParticleEffects(globalParticleBelow, globalParticleAbove);
                controlHandler.PostUpdate(gameTime, globalProjectile, inputManager.mousePosition);
            }
            else if (Main.isConsoleVisible || inputManager.IsMouseOnInventory(true))
            {
                hoveredItem = null;
            }

            if (useTimer > 0)
            {
                useTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }
    }
}