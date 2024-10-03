using System;

namespace RR.Game.SettingSystem
{
    public enum SerializedValueType
    {
        Bool = 0,
        Int = 1,
        Float = 2,
        String = 3
    }

    [Serializable]
    public class SerializedValue
    {
        public SerializedValueType ValueType = SerializedValueType.Float;
        public bool BoolValue;
        public int IntValue;
        public float FloatValue;
        public string StringValue;
    }
}