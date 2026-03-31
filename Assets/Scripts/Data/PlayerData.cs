using System;
using System.Collections.Generic;
using UnityEngine;

namespace KoKoKrunch.Data
{
    [Serializable]
    public class PlayerData
    {
        public string playerName;
        public int score;
        public string timestamp;

        public PlayerData(string name, int score)
        {
            playerName = name;
            this.score = score;
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<PlayerData> entries = new List<PlayerData>();
    }
}
