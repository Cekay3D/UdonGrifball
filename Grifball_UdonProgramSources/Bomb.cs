using UdonSharp;
using VRC.Udon.Common.Interfaces;

namespace Cekay.Grifball
{
    public class Bomb : UdonSharpBehaviour
    {
        public SettingsPage Settings;
        public Combat CombatScript;

        public override void OnPickup()
        {
            CombatScript.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(CombatScript.GrabBomb));
        }

        public override void OnDrop()
        {
            CombatScript.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(CombatScript.DropBomb));
        }
    }
}