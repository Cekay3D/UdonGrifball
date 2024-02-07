using Cyan.PlayerObjectPool;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Cekay.Grifball
{
    public class PlayerObjectListener : CyanPlayerObjectPoolEventListener
    {
        public CyanPlayerObjectAssigner objectPool;
        private DemoPooledObject _localPoolObject;
        public override void _OnLocalPlayerAssigned()
        {
            Debug.Log("The local player has been assigned an object from the pool!");

            _localPoolObject = (DemoPooledObject)objectPool._GetPlayerPooledUdon(Networking.LocalPlayer);
        }

        public override void _OnPlayerAssigned(VRCPlayerApi player, int poolIndex, UdonBehaviour poolObject)
        {
            Debug.Log($"Object {poolIndex} assigned to player {player.displayName} {player.playerId}");
        }

        public override void _OnPlayerUnassigned(VRCPlayerApi player, int poolIndex, UdonBehaviour poolObject)
        {
            Debug.Log($"Object {poolIndex} unassigned from player {player.displayName} {player.playerId}");
        }
    }
}