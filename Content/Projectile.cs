using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Projectile
    {
        public Texture2D Texture { get; set; }

        public int ID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Penetrate { get; set; }
        public int AI { get; set; }
        public int Damage { get; set; }
        public float LifeTime { get; set; }
        public float CurrentLifeTime { get; set; }
        public float KnockBack { get; set; }
        public float Speed { get; set; }
        public string TexturePath { get; set; }
        public string Name { get; set; }
        public bool IsAlive { get; set; }

        public Vector2 Position { get; set; }
        private Vector2 SpawnPosition;
        private Vector2 Target;
        public float Rotation;
        public Player Owner { get; set; }

        public Projectile(Texture2D texture, string texturePath, string name, int id, int ai, int damage, int penetrate, float lifeTime, float knockBack, float speed, Vector2 position, Player owner, int width, int height, bool isAlive)
        {
            CurrentLifeTime = 0f;
            Texture = texture;
            TexturePath = texturePath;
            ID = id;
            AI = ai;
            Name = name;
            Damage = damage;
            LifeTime = lifeTime;
            Speed = speed;
            KnockBack = knockBack;
            Position = position;
            Owner = owner;
            MouseState mouseState = Mouse.GetState();
            Target = new Vector2(mouseState.X, mouseState.Y);
            SpawnPosition = Position;
            IsAlive = isAlive;
            Width = width;
            Height = height;
            Penetrate = penetrate;
        }

        public void Update(GameTime gameTime)
        {
            if (CurrentLifeTime >= LifeTime)
            {
                IsAlive = false;
            }
            else
            {
                CurrentLifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (Penetrate < 0)
            {
                IsAlive = false;
            }

            if (AI == 0 || AI == 1)
            {
                Vector2 direction = Target - SpawnPosition;
                direction.Normalize();

                Position += direction * Speed;
                Rotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        public void Kill()
        {
            /* if (AI == 0)
             {
                 Main.projManager.NewProjectile(id: 1, damage: 10, lifeTime: 2, knockBack: 3, speed: 10, position: Position, owner: Owner, isAlive: true);
                 Main.projManager.NewProjectile(id: 1, damage: 10, lifeTime: 2, knockBack: 3, speed: 20, position: Position * 2, owner: Owner, isAlive: true);
             }*/
        }

        public void RemoveProjectile(List<Projectile> projectiles)
        {
            projectiles.Remove(this);
        }

        /*public Projectile Clone(Player owner, bool isAlive)
        {
            return new Projectile(Texture, TexturePath, Name, ID, AI, Damage, LifeTime, KnockBack, Position, owner, isAlive);
        }

        public Projectile Clone(int id, int ai, int damage, Vector2 pos, Player owner, bool isAlive)
        {
            return new Projectile(Texture, TexturePath, Name, id, ai, damage, LifeTime, KnockBack, pos, owner, isAlive);
        }

        public Projectile Clone(int id, int damage, Vector2 pos, Player owner, bool isAlive)
        {
            return new Projectile(Texture, TexturePath, Name, id, AI, damage, LifeTime, KnockBack, pos, owner, isAlive);
        }*/
    }
}