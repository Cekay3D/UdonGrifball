using Cyan.PlayerObjectPool;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Cekay.Grifball
{
    public class HurtHammer : UdonSharpBehaviour
    {
        public SettingsPage Settings;
        public Combat CombatScript;
        public CyanPlayerObjectAssigner ObjAssign;
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player != Settings.LocalPlayer)
            {
                UdonBehaviour targetScript = (UdonBehaviour)ObjAssign._GetPlayerPooledUdon(player);

                string playerName = (string)targetScript.GetProgramVariable("LocalPlayerName");

                if ((Settings.BlueTeam.Contains(playerName) && (CombatScript.CurrentTeam == "Red")) ||
                    (Settings.RedTeam.Contains(playerName) && (CombatScript.CurrentTeam == "Blue")))
                {
                    targetScript.SendCustomEvent("GetHurt");
                }
            }
        }
    }
}
