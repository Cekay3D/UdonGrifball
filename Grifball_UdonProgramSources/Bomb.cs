using Cyan.PlayerObjectPool;
using UdonSharp;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Cekay.Grifball
{
    public class Bomb : UdonSharpBehaviour
    {
        public SettingsPage Settings;
        public Combat CombatScript;
        public CyanPlayerObjectAssigner ObjAssign;

        public int Holder;

        public override void OnPickup()
        {
            CombatScript.SendCustomNetworkEvent(NetworkEventTarget.All, "GrabBomb");
            Holder = CombatScript.BombPickup.currentPlayer.playerId;
            UdonBehaviour targetScript = (UdonBehaviour)ObjAssign._GetPlayerPooledUdonById(Holder);
            targetScript.SendCustomNetworkEvent(NetworkEventTarget.All, "HammerHide");
        }

        public override void OnDrop()
        {
            CombatScript.SendCustomNetworkEvent(NetworkEventTarget.All, "DropBomb");
            UdonBehaviour targetScript = (UdonBehaviour)ObjAssign._GetPlayerPooledUdonById(Holder);
            targetScript.SendCustomNetworkEvent(NetworkEventTarget.All, "HammerShow");
        }
    }
}