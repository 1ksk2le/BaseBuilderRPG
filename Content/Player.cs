using BaseBuilderRPG.Content.BaseBuilderRPG.Content;
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
        public Item equippedWeapon, mouseItem, hoveredItem;
        public Texture2D textureBody;
        public Texture2D textureHead;
        public Texture2D textureEye;
        public Color skinColor;
        public Vector2 velocity, origin, center, targetMovement;
        public Rectangle rectangle, rectangleMelee, rectangleMeleeAI;
        public int direction = 1;
        public int width, height;
        public float meleeRange, rangedRange, speed, useTimer, immunityTime, hitEffectTimer, rotationAngle, immunityTimeMax, hitEffectTimerMax;
        public bool inventoryVisible, isImmune, isSwinging, isPicked, canHit, didSpawn, hasMovementOrder, aiAttackCheck;
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
            immunityTimeMax = 0.4f;
            speed = 1.5f;
            immunityTime = 0f;
            hitEffectTimer = 0f;
            hitEffectTimerMax = 0.75f;
            targetMovement = center;
            width = textureBody.Width;
            height = textureBody.Height;
            origin = new Vector2(width / 2, height / 2);
            inventory = new Inventory(5, 6);
            inventoryVisible = true;
            canHit = true;
            didSpawn = false;
            aiAttackCheck = false;
            hasMovementOrder = false;
            isPicked = false;
            aiState = "";

            ownedProjectiles = new List<Projectile>();

            aiHandler = new Player_AIHandler(this);
            visualHandler = new Player_VisualHandler(this);
            controlHandler = new Player_ControlHandler(this);
        }

        public void Update(GameTime gameTime, Dictionary<int, Item> itemDictionary, Item_Globals globalItem, Text_Manager textManager, Projectile_Globals globalProjectile, Particle_Globals globalParticle, List<NPC> npcs, List<Item> groundItems, List<Item> items)
        {
            var inputManager = Input_Manager.Instance;
            rectangle = new Rectangle((int)position.X, (int)position.Y, width, height);
            center = position + origin;

            if (equippedWeapon != null)
            {
                meleeRange = (equippedWeapon.damageType == "melee") ? 200f : 0;
                if (equippedWeapon.damageType == "ranged")
                {
                    rangedRange = globalProjectile.GetProjectile(equippedWeapon.shootID).lifeTimeMax * equippedWeapon.shootSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
                }
                else if (equippedWeapon.damageType == "melee")
                {
                    Vector2 pos = (direction == 1) ? new Vector2(width + equippedWeapon.texture.Height * 0.2f, height / 2)
                                               : new Vector2(-equippedWeapon.texture.Height * 0.2f, height / 2);
                    rectangleMelee = CalcMeleeRectangle(position + pos, (int)(equippedWeapon.texture.Width), (int)(equippedWeapon.texture.Height * 1.1f), rotationAngle);
                    rectangleMeleeAI = new Rectangle((int)(center.X - equippedWeapon.texture.Height / 2), (int)(center.Y - equippedWeapon.texture.Height * 1.2f / 2), (int)(equippedWeapon.texture.Height), (int)(equippedWeapon.texture.Height * 1.2f));
                }
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

            if (inventory.equipmentSlots[0].equippedItem != null)
            {
                equippedWeapon = inventory.equipmentSlots[0].equippedItem;
            }
            else
            {
                equippedWeapon = null;
            }

            if (health > 0f)
            {
                if (!isControlled)
                {
                    aiHandler.ProcessAI(gameTime, npcs, globalProjectile);
                }
                else
                {
                    aiHandler.Shoot(gameTime, globalProjectile, new Vector2(inputManager.previousMouseState.X, inputManager.previousMouseState.Y));
                    controlHandler.PlayerInventoryInteractions(Keys.I, groundItems);
                    controlHandler.AddItem(Keys.X, true, Main.random.Next(0, 11), itemDictionary, globalItem, groundItems, items);
                    controlHandler.PickItem(groundItems, inputManager, textManager);
                    aiState = "";
                }

                if (equippedWeapon != null)
                {
                    if (equippedWeapon.weaponType == "One Handed")
                    {
                        //controlHandler.OneHandedSwing(gameTime, globalParticle);
                    }
                }
                visualHandler.ParticleEffects(globalParticle);
                controlHandler.Movement(Vector2.Zero, Input_Manager.Instance.currentKeyboardState);
            }
        }

        public Rectangle CalcMeleeRectangle(Vector2 position, int width, int height, float rotation)
        {
            Matrix transform = Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(position.X, position.Y, 0);

            Vector2 leftTop = Vector2.Transform(new Vector2(-width / 2, -height / 2), transform);
            Vector2 rightTop = Vector2.Transform(new Vector2(width / 2, -height / 2), transform);
            Vector2 leftBottom = Vector2.Transform(new Vector2(-width / 2, height / 2), transform);
            Vector2 rightBottom = Vector2.Transform(new Vector2(width / 2, height / 2), transform);

            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop), Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop), Vector2.Max(leftBottom, rightBottom));

            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }
    }
}