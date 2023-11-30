using Microsoft.Xna.Framework;

namespace BaseBuilderRPG.Content
{
    public static class Vector2Extensions
    {
        public static Vector2 RotateVector(Vector2 vector, float rotation, Vector2 origin)
        {
            return Vector2.Transform(vector - origin, Matrix.CreateRotationZ(rotation)) + origin;
        }

    }
}