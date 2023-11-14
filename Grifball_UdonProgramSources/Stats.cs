using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

public class Stats : UdonSharpBehaviour
{
    public string GameStatsJSON;

    private DataDictionary GameStats = new DataDictionary()
    {
        {"Game 1", new DataDictionary()
            {
                {"RedScore", 0},
                {"BlueScore", 0},
                {"RedKills", 0},
                {"BlueKills", 0},
                {"RedDeaths", 0},
                {"BlueDeaths", 0}
            }
        }
    };

    private DataDictionary PlayerStats = new DataDictionary()
    {
        {"Player", new DataDictionary()
            {
                {"Kills", 0},
                {"Deaths", 0},
                {"K/D Ratio", 0.0f},
                {"Goals", 0},
                {"Multikill", 0},
                {"Spree", 0}
            }
        }
    };

    [UdonSynced] public int BlueWins = 0;
    [UdonSynced] public int RedWins = 0;

    public override void OnPreSerialization()
    {
        if (VRCJson.TrySerializeToJson(GameStats, JsonExportType.Minify, out DataToken resultG))
        {
            GameStatsJSON = resultG.String;
        }
        else
        {
            Debug.LogError(resultG.ToString());
        }
    }

    public override void OnDeserialization()
    {
        if (VRCJson.TryDeserializeFromJson(GameStatsJSON, out DataToken resultG))
        {
            GameStats = resultG.DataDictionary;
        }
        else
        {
            Debug.LogError(resultG.ToString());
        }
    }
}