using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RowData
{
    //public List<NetworkPlayer> columns = new List<NetworkPlayer>();
    public List<PlayerCell> columns = new List<PlayerCell>();
}

[System.Serializable]
public class PlayerCell
{
    public string playerName;
    public int currentHealth;
    public bool isReady;
}
