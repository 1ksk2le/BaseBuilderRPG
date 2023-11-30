using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class NPCAI_Handler
    {
        private NPC npc;

        public NPCAI_Handler(NPC npc)
        {
            this.npc = npc;
        }

        public void ProcessAI(GameTime gameTime, List<Player> players, List<Projectile> projectiles, Text_Manager textManager, Particle_Globals globalParticle)
        {
            OnHitByProjectile(gameTime, projectiles, textManager, globalParticle);
            HitPlayer(gameTime, players, globalParticle, textManager);

            Player targetPlayer = FindTargetPlayer(players);

            if (npc.target == null)
            {
                if (targetPlayer != null)
                {
                    npc.target = targetPlayer;
                }
                else
                {
                    npc.target = null;
                }
            }
            else
            {
                AI_1(gameTime, players);
            }
        }

        private void AI_1(GameTime gameTime, List<Player> players)
        {
            if (npc.ai == 1)
            {
                const float leapingTimer = 2f;
                if (npc.aiTimer[0] > leapingTimer)
                {
                    npc.aiTimer[1] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (npc.aiTimer[1] >= 0f)
                    {
                        if (npc.aiTimer[2] == 0)
                        {
                            npc.aiTimer[2]++;
                            npc.velocity = npc.target.center - npc.center;
                        }

                        npc.velocity = Normalize(npc.velocity);
                        if (Math.Abs(npc.velocity.Length() - 1.0f) > 0.001f)
                        {
                            throw new ArgumentException("NPC velocity is not normalized");
                        }
                        npc.position += npc.velocity * 90f * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (npc.aiTimer[1] > 1f)
                        {
                            npc.aiTimer[0] = 0f;
                            npc.aiTimer[1] = 0f;
                        }
                    }
                }
                else
                {
                    npc.aiTimer[2] = 0;
                    if (!npc.rectangle.Intersects(npc.target.rectangle))
                    {
                        npc.velocity = npc.target.center - npc.center;
                        npc.velocity = Normalize(npc.velocity);
                        npc.position += npc.velocity * 30f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    npc.aiTimer[0] += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }

        private Vector2 Normalize(Vector2 vector)
        {
            float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (length > 0.001f)
            {
                float invLength = 1.0f / length;
                return new Vector2(vector.X * invLength, vector.Y * invLength);
            }
            else
            {
                return Vector2.Zero;
            }
        }

        private Player FindTargetPlayer(List<Player> players)
        {
            Player targetPlayer = null;
            float distanceLimit = npc.targetRange * npc.targetRange;

            foreach (Player player in players)
            {
                float distance = Vector2.DistanceSquared(player.center, npc.center);

                if (distance < distanceLimit)
                {
                    distanceLimit = distance;
                    targetPlayer = player;
                }
            }

            return targetPlayer;
        }

        private void OnHitByProjectile(GameTime gameTime, List<Projectile> projectiles, Text_Manager textManager, Particle_Globals globalParticle)
        {
            foreach (Projectile proj in projectiles)
            {
                if (proj.isAlive && proj.damage > 0)
                {
                    if (proj.rectangle.Intersects(npc.rectangle) && !npc.isImmune)
                    {
                        Vector2 hitDirection = npc.center - proj.center;
                        hitDirection.Normalize();

                        proj.penetrate--;
                        npc.target = proj.owner;
                        GetDamaged(textManager, proj.damage, globalParticle, proj, null);
                    }
                }

            }
        }

        private void HitPlayer(GameTime gameTime, List<Player> players, Particle_Globals globalParticle, Text_Manager textManager)
        {
            foreach (Player player in players)
            {
                if (npc.rectangle.Intersects(player.rectangle) && !player.isImmune && npc.damage > 0)
                {
                    player.aiHandler.GetDamaged(textManager, npc.damage, globalParticle, npc);
                }
            }
        }

        private void ApplyKnockBack(GameTime gameTime)
        {
            float progress = 1f - (npc.kbTimer / npc.kbTimerMax);
            npc.position = Vector2.Lerp(npc.kbStartPos, npc.kbEndPos, progress);

            npc.kbTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (npc.kbTimer <= 0)
            {
                npc.position = npc.kbEndPos;
            }
        }

        private void GetDamaged(Text_Manager textManager, int damage, Particle_Globals globalParticle, Projectile projectile, Player player)
        {
            npc.health -= damage;

            textManager.AddFloatingText("-" + damage.ToString(), "", new Vector2(npc.position.X + npc.width / 2 + Main.random.Next(-10, 10), npc.position.Y), new Vector2(Main.random.Next(-10, 10) * 1f, Main.random.Next(1, 10) + 10f), Color.Red, Color.Transparent, 2f, 1.1f);

            npc.hitEffectTimer = npc.hitEffectTimerMax;
            npc.immunityTime = npc.immunityTimeMax;
        }
    }
}