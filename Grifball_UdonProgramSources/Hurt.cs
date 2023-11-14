using Cyan.PlayerObjectPool;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Cekay.Grifball
{
    public class Hurt : UdonSharpBehaviour
    {
        public CyanPlayerObjectAssigner ObjAssign;
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            UdonBehaviour targetScript = (UdonBehaviour)ObjAssign._GetPlayerPooledUdon(player);

            targetScript.SendCustomEvent("GetHurt");
        }
    }
}