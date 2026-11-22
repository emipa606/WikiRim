using UnityEngine;

namespace HelpTab;

public static class Vector3_Extensions
{
    extension(Vector3 vec)
    {
        public float Sum()
        {
            return vec.x + vec.y + vec.z;
        }

        public float Max()
        {
            return Mathf.Max(vec.x, Mathf.Max(vec.y, vec.z));
        }

        public float Min()
        {
            return Mathf.Min(vec.x, Mathf.Min(vec.y, vec.z));
        }

        public Vector3 Subtract(float minus)
        {
            return new Vector3(vec.x - minus, vec.y - minus, vec.z - minus);
        }
    }
}