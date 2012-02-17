using System.Collections.Generic;
using System.Linq;
using Microsoft.Research.Kinect.Nui;
using System.IO;

namespace Kinect.Toolbox
{
    public static class Tools
    {
        public static List<Vector2> ToListOfVector2(this List<Joint> joints)
        {
            return joints.Select(j => j.Position.ToVector2()).ToList();
        }

        public static Vector3 ToVector3(this Vector vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        public static Vector2 ToVector2(this Vector vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        public static void Write(this BinaryWriter writer, Vector vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
            writer.Write(vector.W);
        }
        public static Vector ReadVector(this BinaryReader reader)
        {
            Vector result = new Vector
                                {
                                    X = reader.ReadSingle(),
                                    Y = reader.ReadSingle(),
                                    Z = reader.ReadSingle(),
                                    W = reader.ReadSingle()
                                };

            return result;
        }

        public static bool TrySetElevationAngle(this Camera camera, int angle)
        {
            bool success = false;
            try
            {
                camera.ElevationAngle = angle;
                success = true;
            }
            catch { }
            return success;
        }


    }
}
