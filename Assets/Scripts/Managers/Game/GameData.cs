using System.Collections.Generic;
using UnityEngine;

namespace Managers.Game
{
    public class GameData : MonoBehaviour
    {
        public static Dictionary<ulong, PlayerData> Games = new();
        public struct PlayerData
        {
            public uint PrefabID;
        }
    }
    
}
