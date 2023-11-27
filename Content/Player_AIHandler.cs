using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Player_AIHandler
    {
        private Player player;

        public Player_AIHandler(Player player)
        {
            this.player = player;
        }

        public void ProcessAI(GameTime gameTime, List<NPC> npcs, Projectile_Globals projManager)
        {
            Movement();
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
                        closestDistance = player.rangedRange * player.rangedRange * 2;
                        break;
                    default:
                        closestDistance = 0f;
                        break;
                }
                foreach (NPC npc in npcs)
                {
                    float distance = Vector2.DistanceSquared(player.center, npc.center);

                    if (distance < closestDistance)
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
                        if (!player.rectangleMelee.Intersects(targetNPC.rectangle))
                        {
                            if (player.center.X > player.target.center.X)
                            {
                                player.direction = -1;
                            }
                            else
                            {
                                player.direction = 1;
                            }

                            if (!player.hasMovementOrder)
                            {
                                Vector2 targetDirection = player.target.center - player.center;
                                targetDirection.Normalize();
                                player.position += targetDirection * player.speed;
                                player.aiState = "Moving to target: [" + targetNPC.name + "]";
                            }


                        }
                        else
                        {
                            player.aiState = "Attacking target: [" + targetNPC.name + "]";
                            UseItem(gameTime, projManager, player.target.center);
                        }
                    }
                    else
                    {
                        player.aiState = "Waiting for orders";
                    }
                }
                if (player.equippedWeapon.damageType == "ranged")
                {
                    if (targetNPC != null)
                    {
                        player.target = targetNPC;
                        if (Vector2.Distance(player.target.center, player.center) > player.rangedRange * 0.8f)
                        {
                            if (player.position.X > player.target.position.X)
                            {
                                player.direction = -1;
                            }
                            else
                            {
                                player.direction = 1;
                            }

                            if (!player.hasMovementOrder)
                            {
                                Vector2 targetDirection = player.target.center - player.center;
                                targetDirection.Normalize();
                                player.position += targetDirection * player.speed;
                                player.aiState = "Moving to target: [" + targetNPC.name + "]";
                            }
                        }
                        else
                        {
                            if (Vector2.Distance(player.target.center, player.center) < 100f)
                            {
                                if (player.position.X > player.target.center.X)
                                {
                                    player.direction = -1;
                                }
                                else
                                {
                                    player.direction = 1;
                                }
                                if (!player.hasMovementOrder)
                                {
                                    Vector2 targetDirection = player.target.center - player.center;
                                    targetDirection.Normalize();
                                    player.position -= targetDirection * player.speed;
                                    player.aiState = "Running away from: [" + targetNPC.name + "]";
                                }
                            }
                            else
                            {
                                UseItem(gameTime, projManager, player.target.center);
                                player.aiState = "Shooting at target: [" + targetNPC.name + "]";
                            }
                        }
                    }
                    else
                    {
                        player.aiState = "Waiting for orders";
                    }
                }
            }
        }

        public void UseItem(GameTime gameTime, Projectile_Globals projManager, Vector2 target)
        {
            if (player.equippedWeapon != null)
            {
                if (player.equippedWeapon.shootID > -1)
                {
                    if (player.useTimer <= 0)
                    {
                        if (!player.isControlled)
                        {
                            projManager.NewProjectile(player.equippedWeapon.shootID, player.center, target, player.equippedWeapon.damage, player.equippedWeapon.shootSpeed, player.equippedWeapon.knockBack, player, true);
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
                            projManager.NewProjectile(0, player.center, target, player.equippedWeapon.damage, player.equippedWeapon.shootSpeed, player.equippedWeapon.knockBack, player, true);
                            player.useTimer = player.equippedWeapon.useTime;
                        }
                    }
                }
            }
        }

        private void Movement()
        {
            if (player.hasMovementOrder && !player.isControlled)
            {
                player.direction = (player.position.X > player.targetMovement.X) ? -1 : 1;

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
                    float directionX = deltaX / distance;
                    float directionY = deltaY / distance;
                    float newPositionX = player.position.X + directionX * player.speed;
                    float newPositionY = player.position.Y + directionY * player.speed;
                    player.aiState = "Moving to: [" + player.targetMovement.ToString() + "]";
                    player.position = new Vector2(newPositionX, newPositionY);
                }
            }
            else
            {
                if (player.isControlled)
                {
                    player.isPicked = false;
                }
                player.targetMovement = player.center;
                player.hasMovementOrder = false;
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