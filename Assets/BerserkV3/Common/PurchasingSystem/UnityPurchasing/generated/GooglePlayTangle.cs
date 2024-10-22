// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("HUEHOYAKb076iCu87eWfrcLz9Figq6U/XV2+aHItn6DRoic1WXTem7llSwUn/3TKPUZa8iFcV95Bp4Y86SbjDLJ7fKYaD+7vtt7Nzs5631CT5mqYUegmr9NkaxV2OJkE4urYnTeQPTH1uqbLKEaIWLAd0Z9nrcbmX1T0DUvqQ+JBXyEOlFpCdo6IGor1XHV1iEBm2veWz/y7RAQdmSTKZWbl6+TUZuXu5mbl5eRG1atymqgo1GblxtTp4u3OYqxiE+nl5eXh5Oca5UiloaxQ1WXCHXUgJ9+BNV+/qVkTKyEk/wGXmWAHcOH3NM8nAJCQ2XC0UaS3zSlDZ+WR3b4PJr1K6se4NAGTAozApIL7miuMdthC4VitPmthU8k0VDvi8ebn5eTl");
        private static int[] order = new int[] { 6,12,3,8,5,11,11,9,12,11,13,11,13,13,14 };
        private static int key = 228;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
