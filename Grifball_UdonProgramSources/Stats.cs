using UdonSharp;
using VRC.SDK3.Data;

public class Stats : UdonSharpBehaviour
{
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
                {"Goals", 0}
            }
        }
    };

    void Start()
    {
        
    }
}