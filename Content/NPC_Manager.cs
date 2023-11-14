using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BaseBuilderRPG.Content
{
    public class NPC_Manager : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        private static Dictionary<int, NPC> npcDictionary;

        public List<NPC> npcs;
        private List<NPC> npcsToRemove;
        private List<Item> items;
        private List<Item> groundItems;
        private List<Player> players;
        private List<Projectile> projectiles;
        private Dictionary<int, Item> itemDictionary;

        private Item_Manager itemManager;
        private Display_Text_Manager disTextManager;

        public NPC_Manager(Game game, SpriteBatch spriteBatch, Item_Manager _itemManager, Display_Text_Manager _disTextManager, List<Item> _items, List<Item> _groundItems,
            Dictionary<int, Item> _itemDictionary, List<Player> _players, List<Projectile> projectiles)
            : base(game)
        {
            this.spriteBatch = spriteBatch;

            npcs = new List<NPC>();
            npcsToRemove = new List<NPC>();
            npcDictionary = new Dictionary<int, NPC>();

            string npcsJson = File.ReadAllText("Content/npcs.json");
            npcs = JsonConvert.DeserializeObject<List<NPC>>(npcsJson);
            foreach (var npc in npcs)
            {
                npcDictionary.Add(npc.ID, npc);
            }

            items = _items;
            groundItems = _groundItems;
            players = _players;
            itemDictionary = _itemDictionary;
            itemManager = _itemManager;
            disTextManager = _disTextManager;
            this.projectiles = projectiles;
        }

        public void Load()
        {
            foreach (var npc in npcs)
            {
                npc.Texture = Game.Content.Load<Texture2D>(npc.TexturePath);
            }
        }

        public void NewNPC(int id, Vector2 position)
        {
            if (npcDictionary.TryGetValue(id, out var npc))
            {
                npcs.Add(new NPC(npc.Texture, npc.TexturePath, npc.Name, id, npc.AI, npc.Damage, npc.MaxHealth, npc.KnockBack, position, npc.NumFrames, true));
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (NPC npc in npcs)
            {
                if (npc.IsAlive)
                {
                    ProcessAI(npc, gameTime);
                    npc.Update(gameTime);
                }
                else
                {
                    npc.Kill();
                    npcsToRemove.Add(npc);
                }
            }

            foreach (NPC npc in npcsToRemove)
            {
                npcs.Remove(npc);
            }

            base.Update(gameTime);
        }

        private void ProcessAI(NPC npc, GameTime gameTime)
        {
            Player closestPlayer = null;
            float closestDistance = float.MaxValue;

            foreach (Player player in players)
            {
                float distance = Vector2.DistanceSquared(player.Position, npc.Position);

                // Check if this player is closer than the current closest player
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            if (closestPlayer != null)
            {
                npc.Target = closestPlayer.Position;

                npc.ProcessAI(gameTime);
            }

            HitByProjectile(npc);
        }


        private void HitByProjectile(NPC npc)
        {
            Rectangle npcRectangle = new Rectangle((int)npc.Position.X, (int)npc.Position.Y, npc.Texture.Width, npc.Texture.Height / npc.NumFrames);

            foreach (Projectile proj in projectiles)
            {
                if (proj.IsAlive && proj.Damage > 0)
                {
                    Rectangle projRectangle = new Rectangle((int)proj.Position.X, (int)proj.Position.Y, proj.Width, proj.Height);
                    if (projRectangle.Intersects(npcRectangle) && !npc.IsImmune)
                    {
                        proj.Penetrate--;
                        GetDamaged(npc, disTextManager, proj.Damage);
                    }
                }
            }
        }



        private void GetDamaged(NPC npc, Display_Text_Manager texMan, int damage)
        {
            npc.Health -= damage;
            texMan.AddFloatingText("-" + damage.ToString(), "", new Vector2(npc.Position.X + npc.Width / 2, npc.Position.Y), Color.Red, Color.Transparent, 2f, 1.1f);
            npc.ImmunityTime = npc.MaxImmunityTime;
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.Identity);
            spriteBatch.DrawString(Main.TestFont, "NPC MANAGER NPC COUNT: " + npcs.Count.ToString(), new Vector2(10, 400), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            foreach (NPC npc in npcs)
            {
                if (npc.Texture != null && npc.IsAlive)
                {
                    npc.Draw(spriteBatch);
                    var Health = npc.Health;
                    var MaxHealth = npc.MaxHealth;
                    var Target = npc.Target;
                    var Position = npc.Position;
                    var Height = npc.Height;
                    var Width = npc.Width;
                    var NumFrames = npc.NumFrames;
                    var Name = npc.Name;

                    if (Health < MaxHealth)
                    {
                        var pos = Position + new Vector2(0, Height + 6);
                        float healthBarWidth = Width * ((float)Health / (float)MaxHealth);

                        Rectangle healthBarRectangleBackground = new Rectangle((int)(pos.X - 2), (int)pos.Y - 1, (int)(Width) + 4, 5);
                        Rectangle healthBarRectangleBackgroundRed = new Rectangle((int)(pos.X), (int)pos.Y, (int)(Width), 3);
                        Rectangle healthBarRectangle = new Rectangle((int)(pos.X), (int)pos.Y, (int)healthBarWidth, 3);

                        spriteBatch.DrawRectangle(healthBarRectangleBackground, Color.Black, 0.691f);
                        spriteBatch.DrawRectangle(healthBarRectangleBackgroundRed, Color.Red, 0.692f);
                        spriteBatch.DrawRectangle(healthBarRectangle, Color.Lime, 0.693f);
                    }

                    Vector2 textSize = Main.TestFont.MeasureString(Name);
                    spriteBatch.DrawStringWithOutline(Main.TestFont, Name, new Vector2(Position.X + Width / 2 - textSize.X / 2, Position.Y - 14), Color.Black, Color.White, 1f, 0.693f);

                    Rectangle npcSourceRectangle = new Rectangle(0, 0, npc.Width, npc.Texture.Height / npc.NumFrames);
                    if (npc.AI == 0)
                    {
                        spriteBatch.Draw(npc.Texture, npc.Position, null, Color.White, npc.Rotation, Vector2.Zero, 1f, SpriteEffects.None, 0.69f);
                    }
                }
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
