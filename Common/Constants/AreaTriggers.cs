using System.Collections.Generic;
using Common.Structs;

namespace Common.Constants
{
    public static class AreaTriggers
    {
        public static readonly IDictionary<uint, Location> Triggers = new Dictionary<uint, Location>()
        {
            {45,  new Location(1688.99f, 1053.48f, 18.6775f, 0.00117f, 189, "Scarlet Monastery - Graveyard (Entrance)")},
            {78,  new Location(-16.4f, -383.07f, 61.78f, 1.86f, 36, "DeadMines")},
            {101, new Location(54.23f, 0.28f, -18.34f, 6.26f, 34, "Stormwind Stockades")},
            {104, new Location(-8764.83f, 846.075f, 87.4842f, 3.77934f, 0, "Stormwind Stockades - Outside")},
            {107, new Location(-0.91f, 40.57f, -24.23f, 0f, 35, "Stormwind Vault")},
            {109, new Location(-8653.45f, 606.19f, 91.16f, 0f, 0, "Stormwind Vault Instance")},
            {119, new Location(-11208.3f, 1672.52f, 24.66f, 4.55217f, 0, "Deadmines - Outside Start")},
            {121, new Location(-11339.4f, 1571.16f, 100.56f, 0f, 0, "Deadmines - Outside End")},
            {145, new Location(-229.135f, 2109.18f, 76.8898f, 1.267f, 33, "Shadowfang Keep")},
            {194, new Location(-232.796f, 1568.28f, 76.8909f, 4.398f, 0, "Shadowfang keep - Entrance")},
            {226, new Location(-740.059f, -2214.23f, 16.1374f, 5.68f, 1, "Wailing Caverns - Outside")},
            {228, new Location(-163.49f, 132.9f, -73.66f, 5.83f, 43, "Wailing Caverns")},
            {242, new Location(-4464.92f, -1666.24f, 90f, 0f, 1, "Razorfen Kraul Instance Start")},
            {244, new Location(1943f, 1544.63f, 82f, 1.38f, 47, "Razorfen Kraul")},
            {257, new Location(-151.89f, 106.96f, -39.87f, 4.53f, 48, "Blackfathom Deeps")},
            {259, new Location(4247.74f, 745.879f, -24.5299f, 4.5828f, 1, "Blackfathom Deeps Instance Start")},
            {286, new Location(-226.8f, 49.09f, -46.03f, 1.39f, 70, "Uldaman")},
            {288, new Location(-6066.73f, -2955.63f, 209.776f, 3.20443f, 0, "Uldaman Instance Start")},
            {322, new Location(-5163.33f, 927.623f, 257.188f, 0f, 0, "Gnomeregan Instance Start")},
            {324, new Location(-332.22f, -2.28f, -150.86f, 2.77f, 90, "Gnomeregan")},
            {442, new Location(2592.55f, 1107.5f, 51.29f, 4.74f, 129, "Razorfen Downs")},
            {444, new Location(-4658.12f, -2526.35f, 81.492f, 1.25978f, 1, "Razorfen Downs Instance Start")},
            {446, new Location(-319.24f, 99.9f, -131.85f, 3.19f, 109, "Altar of Atal'Hakkar")},
            {448, new Location(-10175.1f, -3995.15f, -112.9f, 2.95938f, 0, "Altar Of Atal'Hakkar Instance Start")},
            {503, new Location(-8764.83f, 846.075f, 87.4842f, 3.77934f, 0, "Stockades Instance")},
            {523, new Location(-736.51f, 2.71f, -249.99f, 3.14f, 90, "Gnomeregan Train Depot")},
            {525, new Location(-4858.27f, 756.435f, 244.923f, 0f, 0, "Gnomeregan Train Depot Instance")},
            {527, new Location(8786.36f, 967.445f, 30.197f, 3.39632f, 1, "Teddrassil - Ruth Theran")},
            {542, new Location(9945.13f, 2616.89f, 1316.46f, 4.61446f, 1, "Teddrassil - Darnassus")},
            {602, new Location(2913.92f, -802.404f, 160.333f, 3.50405f, 0, "Scarlet Monastery - Graveyard (Exit)")},
            {604, new Location(2906.14f, -813.772f, 160.333f, 1.95739f, 0, "Scarlet Monastery - Cathedral (Exit)")},
            {606, new Location(2884.45f, -822.01f, 160.333f, 1.95268f, 0, "Scarlet Monastery - Armory (Exit)")},
            {608, new Location(2870.9f, -820.164f, 160.333f, 0.387856f, 0, "Scarlet Monastery - Library (Exit)")},
            {610, new Location(855.683f, 1321.5f, 18.6709f, 0.001747f, 189, "Scarlet Monastery - Cathedral")},
            {612, new Location(1610.83f, -323.433f, 18.6738f, 6.28022f, 189, "Scarlet Monastery - Armory")},
            {614, new Location(255.346f, -209.09f, 18.6773f, 6.26656f, 189, "Scarlet Monastery - Library")},
            {702, new Location(-9015.97f, 875.318f, 148.617f, 0f, 0, "Stormwind - Wizard Sanctum Room")},
            {704, new Location(-9019.16f, 887.596f, 29.6206f, 0f, 0, "Stormwind - Wizard Sanctum Tower Portal")},
            {882, new Location(-6620.48f, -3765.19f, 266.226f, 3.13531f, 0, "Uldaman Instance End")},
            {902, new Location(-214.02f, 383.607f, -38.7687f, 0.5f, 70, "Uldaman Exit")},
            {922, new Location(-6796.49f, -2890.77f, 8.88063f, 3.30496f, 1, "Zul'Farrak Instance Start")},
            {924, new Location(1213.52f, 841.59f, 8.93f, 6.09f, 209, "Zul'Farrak Entrance")},
            {943, new Location(-5187.47f, -2804.32f, -8.375f, 5.76f, 1, "Leap of Faith - End of fall")},
            {1064, new Location(-4747.17f, -3753.27f, 49.8122f, 0.713271f, 1, "Onyxia's Lair - Dustwallow Instance")},
            {1466, new Location(458.32f, 26.52f, -70.67f, 4.95f, 230, "Blackrock Mountain - Searing Gorge Instance?")},
            {1468, new Location(78.5083f, -225.044f, 49.839f, 5.1f, 229, "Blackrock Spire - Searing Gorge Instance (Inside)")},
            {1470, new Location(-7524.19f, -1230.13f, 285.743f, 2.09544f, 0, "Blackrock Spire - Searing Gorge Instance")},
            {1472, new Location(-7179.63f, -923.667f, 166.416f, 1.84097f, 0, "Blackrock Dephts - Searing Gorge Instance")},
            {2068, new Location(-7524.19f, -1230.13f, 285.743f, 2.09544f, 0, "Blackrock Spire - Fall out")},
            {2166, new Location(-4838.95f, -1318.46f, 501.868f, 1.42372f, 0, "Deeprun Tram - Ironforge Instance (Inside)")},
            {2171, new Location(-8364.57f, 535.981f, 91.7969f, 2.24619f, 0, "Deeprun Tram - Stormwind Instance (Inside)")},
            {2173, new Location(68.3006f, 2490.91f, -4.29647f, 3.12192f, 369, "Deeprun Tram - Stormwind Entrance")},
            {2175, new Location(69.2542f, 10.257f, -4.29664f, 3.09832f, 369, "Deeprun Tram - Ironforge Entrance")},
            {2214, new Location(3593.15f, -3646.56f, 138.5f, 5.33f, 329, "Stratholme - Eastern Plaguelands Instance")},
            {2216, new Location(3395.09f, -3380.25f, 142.702f, 0.1f, 329, "Stratholme - Eastern Plaguelands Instance")},
            {2217, new Location(3395.09f, -3380.25f, 142.702f, 0.1f, 329, "Stratholme - Eastern Plaguelands Instance")},
            {2221, new Location(3235.46f, -4050.6f, 108.45f, 1.93522f, 0, "Stratholme - Eastern Plaguelands Instance (Inside)")},
            {2226, new Location(1813.49f, -4418.58f, -18.57f, 1.78f, 1, "Ragefire Chasm - Ogrimmar Instance (Inside)")},
            {2230, new Location(3.81f, -14.82f, -17.84f, 4.39f, 389, "Ragefire Chasm - Ogrimmar Instance")},
            {2527, new Location(221.322f, 74.4933f, 25.7195f, 0.484836f, 450, "Hall of Legends - Ogrimmar")},
            {2530, new Location(1637.32f, -4242.7f, 56.1827f, 4.1927f, 1, "Hall of Legends - Ogrimmar (Inside)")},
            {2532, new Location(-0.299116f, 4.39156f, -0.255884f, 1.54805f, 449, "Stormwind - Champions Hall")},
            {2534, new Location(-8762.45f, 403.062f, 103.902f, 5.34463f, 0, "Stormwind (Inside) - Champions Hall")},
            {2567, new Location(196.37f, 127.05f, 134.91f, 6.09f, 289, "Scholomance")},
            {2568, new Location(1275.05f, -2552.03f, 90.3994f, 3.6631f, 0, "Scholomance Instance")},
            {2606, new Location(534.868f, -1087.68f, 106.119f, 3.35758f, 0, "Alterac Valley - Horde Exit")},
            {2608, new Location(98.432f, -182.274f, 127.52f, 5.02654f, 0, "Alterac Valley - Alliance Exit")},
            {2848, new Location(29.1607f, -71.3372f, -8.18032f, 4.58f, 249, "Onyxia's Lair")},
            {2886, new Location(1096f, -467f, -104.6f, 3.64f, 409, "The Molten Bridge")},
            {2890, new Location(1115.35f, -457.35f, -102.7f, 0.5f, 230, "Molten Core Entrance")},
            {3126, new Location(-1186.98f, 2875.95f, 85.7258f, 1.78443f, 1, "Maraudon")},
            {3131, new Location(-1471.07f, 2618.57f, 76.1944f, 0f, 1, "Maraudon")},
            {3133, new Location(1019.69f, -458.31f, -43.43f, 0.31f, 349, "Maraudon")},
            {3134, new Location(752.91f, -616.53f, -33.11f, 1.37f, 349, "Maraudon")},
            {3183, new Location(44.4499f, -154.822f, -2.71201f, 0f, 429, "Dire Maul 1")},
            {3184, new Location(-201.11f, -328.66f, -2.72f, 5.22f, 429, "Dire Maul 2")},
            {3185, new Location(9.31119f, -837.085f, -32.5305f, 0f, 429, "Dire Maul 3")},
            {3186, new Location(-62.9658f, 159.867f, -3.46206f, 3.14788f, 429, "Dire Maul 4")},
            {3187, new Location(31.5609f, 159.45f, -3.4777f, 0.01f, 429, "Dire Maul 5")},
            {3189, new Location(255.249f, -16.0561f, -2.58737f, 4.7f, 429, "Dire Maul 6")},
            {3190, new Location(-3831.79f, 1250.23f, 160.223f, 0f, 1, "Dire Maul")},
            {3191, new Location(-3747.96f, 1249.18f, 160.217f, 3.15827f, 1, "Dire Maul")},
            {3193, new Location(-3520.65f, 1077.72f, 161.138f, 1.5009f, 1, "Dire Maul")},
            {3194, new Location(-3737.48f, 934.975f, 160.973f, 3.13864f, 1, "Dire Maul")},
            {3195, new Location(-3980.58f, 776.193f, 161.006f, 0f, 1, "Dire Maul")},
            {3196, new Location(-4030.21f, 127.966f, 26.8109f, 0f, 1, "Dire Maul")},
            {3197, new Location(-3577.67f, 841.859f, 134.594f, 0f, 1, "Dire Maul")},
            {3528, new Location(1096f, -467f, -104.6f, 3.64f, 409, "The Molten Core Window")},
            {3529, new Location(1096f, -467f, -104.6f, 3.64f, 409, "The Molten Core Window (Lava)")},
            {3726, new Location(-7666.23f, -1102.79f, 399.68f, 0.601256f, 469, "Blackwing Lair")},
            {3728, new Location(-7524.19f, -1230.13f, 285.743f, 2.09544f, 0, "Blackrock Spire Unknown")},
            {3928, new Location(-11916.1f, -1230.53f, 92.5334f, 4.71867f, 309, "Zul'Gurub")},
            {3930, new Location(-11916.3f, -1208.37f, 92.2868f, 1.61792f, 0, "Zul'Gurub Exit")},
            {3948, new Location(-1198f, -2533f, 22f, 0f, 0, "Arathi Basin Alliance Out")},
            {3949, new Location(-817f, -3509f, 73f, 0f, 0, "Arathi Basin Horde Out")},
            {4006, new Location(-8418.5f, 1505.94f, 31.8232f, 0f, 1, "Ruins Of Ahn'Qiraj (Inside)")},
            {4008, new Location(-8429.74f, 1512.14f, 31.9074f, 2.58f, 509, "Ruins Of Ahn'Qiraj")},
            {4010, new Location(-8231.33f, 2010.6f, 129.861f, 0f, 531, "Ahn'Qiraj Temple")},
            {4012, new Location(-8242.67f, 1992.06f, 129.072f, 0f, 1, "Ahn'Qiraj Temple (Inside)")},
            {4055, new Location(3005.87f, -3435.01f, 293.882f, 0f, 533, "Naxxramas")},
            {4156, new Location(3498.28f, -5349.9f, 144.968f, 1.31324f, 533, "Naxxramas (Entrance)")},
        };

        public static IEnumerable<KeyValuePair<string, Location>> FindTrigger(string needle)
        {
            needle = needle.Replace(" ", "").Replace("'", "").Trim();

            foreach (var trigger in Triggers)
            {
                if (trigger.Value.HasDescriptionValue(needle, true) && trigger.Value.Map > 1)
                {
                    yield return new KeyValuePair<string, Location>($"{trigger.Value.Description} : {trigger.Key}", trigger.Value);
                    yield break;
                }
            }

            foreach (var trigger in Triggers)
            {
                if (trigger.Value.HasDescriptionValue(needle, false) && trigger.Value.Map > 1)
                    yield return new KeyValuePair<string, Location>($"{trigger.Value.Description} : {trigger.Key}", trigger.Value);
            }
        }
    }
}
