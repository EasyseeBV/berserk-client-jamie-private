using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Data
{
    [Serializable]
    public struct TutorialVector3
    {
        [JsonIgnore] [SerializeField] public Vector3 vector;

        public float x
        {
            get => vector.x;
            set => vector.x = value;
        }

        public float y
        {
            get => vector.y;
            set => vector.y = value;
        }

        public float z
        {
            get => vector.z;
            set => vector.z = value;
        }

        public static implicit operator Vector3(TutorialVector3 vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }

        public static implicit operator Vector2(TutorialVector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        public static implicit operator TutorialVector3(Vector3 value)
        {
            return new TutorialVector3 {vector = value};
        }

        public static implicit operator TutorialVector3(Vector2 value)
        {
            return new TutorialVector3 {vector = value};
        }

        public static implicit operator TutorialVector3(TutorialVector2 value)
        {
            return new TutorialVector3 {vector = value};
        }

        public static TutorialVector3 operator *(TutorialVector3 left, float right)
        {
            return new TutorialVector3 {vector = left.vector * right};
        }

        public static TutorialVector3 operator *(TutorialVector3 left, TutorialVector3 right)
        {
            return new TutorialVector3 {vector = new Vector3(left.x * right.x, left.y * right.y, left.z * right.z)};
        }
    }
}