using Cyan.PlayerObjectPool;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Cekay.Grifball
{
    public class Health : UdonSharpBehaviour
    {
        public CyanPlayerObjectAssigner ObjAssign;

        [SerializeField] private GameObject HealthPack;
        [SerializeField] private Collider HealthParent;
        [SerializeField] private AudioClip Heal;
        [SerializeField] private AudioSource HealthSource;

        private UdonBehaviour TargetScript;

        public override void Interact()
        {
            TargetScript = (UdonBehaviour)ObjAssign._GetPlayerPooledUdon(Networking.LocalPlayer);
            if ((int)TargetScript.GetProgramVariable("PlayerHealth") < 100)
            {
                TargetScript.SendCustomNetworkEvent(NetworkEventTarget.All, "Heal");
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(HealthGrab));
            }
        }

        public void HealthGrab()
        {
            HealthParent.enabled = false;
            HealthPack.SetActive(false);
            HealthSource.PlayOneShot(Heal);
            SendCustomEventDelayedSeconds(nameof(HealthRespawn), 10.0f);
        }

        public void HealthRespawnNetwork()
        {
            SendCustomEventDelayedSeconds(nameof(HealthRespawn), 10.0f);
        }

        public void HealthRespawn()
        {
            HealthPack.SetActive(true);
            HealthParent.enabled = true;
        }

        public void SetInteractable()
        {
            HealthParent.enabled = true;
        }
    }
}