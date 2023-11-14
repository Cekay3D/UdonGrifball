using UdonSharp;
using UnityEngine;

namespace Cekay.Grifball
{
    public class Health : UdonSharpBehaviour
    {
        public Player LocalPlayer;

        [SerializeField] private GameObject HealthPack;
        [SerializeField] private Collider HealthParent;
        [SerializeField] private AudioClip Heal;
        [SerializeField] private AudioSource HealthSource;

        public override void Interact()
        {
            HealthParent.enabled = false;
            HealthPack.SetActive(false);
            HealthSource.PlayOneShot(Heal);

            LocalPlayer.PlayerHealth = 100;

            SendCustomEventDelayedSeconds(nameof(HealthRespawn), 10.0f);
        }

        public void HealthRespawn()
        {
            HealthParent.enabled = true;
            HealthPack.SetActive(true);
        }
    }
}