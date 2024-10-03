using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Data
{
    [Serializable]
    public struct TutorialVector2
    {
        [JsonIgnore] [SerializeField] public Vector2 vector;

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

        public static implicit operator Vector3(TutorialVector2 value)
        {
            return new Vector3(value.x, value.y);
        }

        public static implicit operator Vector2(TutorialVector2 value)
        {
            return new Vector2(value.x, value.y);
        }

        public static implicit operator TutorialVector2(Vector3 value)
        {
            return new TutorialVector2 {vector = value};
        }

        public static implicit operator TutorialVector2(Vector2 value)
        {
            return new TutorialVector2 {vector = value};
        }

        public static implicit operator TutorialVector2(TutorialVector3 value)
        {
            return new TutorialVector2 {vector = value};
        }
        
        public static TutorialVector2 operator *(TutorialVector2 left, float right)
        {
            return new TutorialVector2 {vector = left.vector * right};
        }

        public static TutorialVector2 operator *(TutorialVector2 left, TutorialVector3 right)
        {
            return new TutorialVector2 {vector = new Vector3(left.x * right.x, left.y * right.y)};
        }
    }
}