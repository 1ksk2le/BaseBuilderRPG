using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Player_AIHandler
    {
        private Player player;
        private Player_VisualHandler visualHandler;

        public Player_AIHandler(Player player, Player_VisualHandler visualHandler)
        {
            this.player = player;
            this.visualHandler = visualHandler;
        }

        public void ProcessAI(GameTime gameTime, List<NPC> npcs, Projectile_Globals globalProjectile)
        {
            Movement(gameTime);
            if (player.equippedWeapon != null)
            {
                NPC targetNPC = null;
                float closestDistance;
                switch (player.equippedWeapon.damageType)
                {
                    case "melee":
                        closestDistance = player.meleeRange * player.meleeRange;
                        break;
                    case "ranged":
                        closestDistance = player.rangedRange * player.rangedRange;
                        break;
                    default:
                        closestDistance = 0f;
                        break;
                }
                foreach (NPC npc in npcs)
                {
                    float distance = Vector2.DistanceSquared(player.center, npc.center);

                    if (npc.isAlive && distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetNPC = npc;
                    }
                }
                if (player.equippedWeapon.damageType == "melee")
                {
                    if (targetNPC != null)
                    {
                        player.target = targetNPC;
                        player.direction = player.center.X > (int)player.target.center.X ? -1 : 1;
                        if (!player.rectangleMelee.Intersects(targetNPC.rectangle))
                        {
                            if (!player.hasMovementOrder)
                            {
                                Vector2 targetDirection = player.target.center - player.center;
                                targetDirection.Normalize();
                                player.position += targetDirection * player.speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                player.aiState = "Moving to target: [" + targetNPC.name + "]";
                            }


                        }
                        else
                        {
                            player.aiState = "Attacking target: [" + targetNPC.name + "]";
                            UseItem(globalProjectile, player.target.center);
                        }
                    }
                    else
                    {
                        player.target = null;
                        player.aiState = "Waiting for orders";
                    }
                }
                if (player.equippedWeapon.damageType == "ranged")
                {
                    if (targetNPC != null)
                    {
                        player.target = targetNPC;
                        player.direction = player.center.X > (int)player.target.center.X ? -1 : 1;
                        if (Vector2.Distance(player.target.center, player.center) > player.rangedRange * 0.8f)
                        {
                            if (!player.hasMovementOrder)
                            {
                                Vector2 targetDirection = player.target.center - player.center;
                                targetDirection.Normalize();
                                player.position += targetDirection * player.speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                player.aiState = "Moving to target: [" + targetNPC.name + "]";
                            }
                        }
                        else
                        {
                            if (Vector2.Distance(player.target.center, player.center) < 100f)
                            {
                                if (!player.hasMovementOrder)
                                {
                                    Vector2 targetDirection = player.target.center - player.center;
                                    targetDirection.Normalize();
                                    player.position -= targetDirection * player.speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                    player.aiState = "Running away from: [" + targetNPC.name + "]";
                                }
                            }
                            else
                            {

                                UseItem(globalProjectile, player.target.center);
                                player.aiState = "Shooting at target: [" + targetNPC.name + "]";
                            }
                        }
                    }
                    else
                    {
                        player.target = null;
                        player.aiState = "Waiting for orders";
                    }
                }
            }
        }

        public void UseItem(Projectile_Globals globalProjectile, Vector2 target)
        {
            if (player.equippedWeapon != null)
            {
                if (player.equippedWeapon.shootID > -1)
                {
                    if (player.useTimer <= 0)
                    {
                        if (!player.isControlled)
                        {
                            Vector2 pos;
                            switch (player.equippedWeapon.weaponType)
                            {
                                case "Pistol":
                                    pos = player.center + new Vector2((float)Math.Cos(visualHandler.MouseRot()) * player.equippedWeapon.texture.Width * 1.2f, (float)Math.Sin(visualHandler.MouseRot()) * player.equippedWeapon.texture.Width * 1.2f);
                                    player.animationTimer = player.equippedWeapon.animationTime;
                                    break;
                                default:
                                    pos = player.center;
                                    break;
                            }
                            globalProjectile.NewProjectile(player.equippedWeapon.shootID, pos, target, player.equippedWeapon.damage, player.equippedWeapon.shootSpeed, player.equippedWeapon.knockBack, player, true);
                            player.useTimer = player.equippedWeapon.useTime;
                        }
                    }
                }
                if (player.equippedWeapon.weaponType == "One Handed Sword")
                {
                    if (player.useTimer <= 0)
                    {
                        if (!player.isControlled)
                        {
                            globalProjectile.NewProjectile(0, player.center, target, player.equippedWeapon.damage, player.equippedWeapon.shootSpeed, player.equippedWeapon.knockBack, player, true);
                            player.useTimer = player.equippedWeapon.useTime;
                        }
                    }
                }
            }
        }

        private void Movement(GameTime gameTime)
        {
            if (player.hasMovementOrder && !player.isControlled)
            {
                player.direction = (player.center.X > player.targetMovement.X) ? -1 : 1;

                float distanceThreshold = 1f;
                float deltaX = player.targetMovement.X - player.center.X;
                float deltaY = player.targetMovement.Y - player.center.Y;
                float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                if (distance < distanceThreshold)
                {
                    player.aiState = "";
                    player.hasMovementOrder = false;
                }
                else
                {
                    Vector2 velocity = (player.targetMovement - player.center);
                    velocity.Normalize();
                    player.position += velocity * player.speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    player.aiState = "Moving to: [" + player.targetMovement.ToString() + "]";
                }
            }
        }

        public void GetDamaged(Text_Manager texMan, int damage, Particle_Globals globalParticle, NPC npc)
        {
            player.health -= damage;
            texMan.AddFloatingText("-" + damage.ToString(), "", new Vector2(player.position.X + player.width / 2 + Main.random.Next(-10, 10), player.position.Y), new Vector2(Main.random.Next(-10, 10), Main.random.Next(1, 10) + 10f), Color.Red, Color.Transparent, 2f, 1.1f);
            player.immunityTime = player.immunityTimeMax;
            player.hitEffectTimer = player.hitEffectTimerMax;

            for (int i = 0; i < damage; i++)
            {
                if (npc != null)
                {
                    globalParticle.NewParticle(1, 1, player.position + new Vector2(Main.random.Next(player.width), Main.random.Next(player.height)),
                   (npc.position.X > player.position.X) ? -1 * new Vector2(Main.random.Next(10, 50), Main.random.Next(70, 90)) : new Vector2(Main.random.Next(10, 50), Main.random.Next(-90, -70)), player.origin, 0f, 1f, Main.random.NextFloat(1.5f, 4f), Color.DarkRed, Color.Red, Color.DarkRed);
                }
            }
        }
    }
}