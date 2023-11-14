using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using VRC.SDKBase;

namespace Cekay.Grifball
{
    public class Player : UdonSharpBehaviour
    {
        public Combat CombatScript;
        public SettingsPage Settings;

        private VRCPlayerApi LocalPlayerAPI;

        [SerializeField] private AudioSource Announcer;
        [SerializeField] private AudioClip Beep;
        [SerializeField] private AudioClip Boop;
        [SerializeField] private AudioClip RespawnSound;
        [SerializeField] private AudioClip Betrayal;
        [SerializeField] private AudioClip Betrayed;

        [SerializeField] private PostProcessVolume PostStart;
        [SerializeField] private PostProcessVolume Hurt;
        [SerializeField] private PostProcessVolume Dead;

        [SerializeField] private GameObject[] BlueSpawns;
        [SerializeField] private GameObject[] RedSpawns;

        public int PlayerHealth = 100;
        public int RespawnCountdown = 5;

        private void Start()
        {
            LocalPlayerAPI = Networking.LocalPlayer;
        }

        public void Die()
        {
            Hurt.enabled = false;
            Dead.enabled = true;
            Settings.LocalPlayer.SetVoiceGain(0.0f);
            Settings.LocalPlayer.SetWalkSpeed(0.0f);
            Settings.LocalPlayer.SetStrafeSpeed(0.0f);
            Settings.LocalPlayer.SetRunSpeed(0.0f);
            Settings.LocalPlayer.SetJumpImpulse(0.0f);

            SendCustomEventDelayedSeconds(nameof(RespawnTimer), 1.0f);
        }

        public void RespawnTimer()
        {
            RespawnCountdown--;
            if (RespawnCountdown >= 0)
            {
                Announcer.PlayOneShot(Beep);
                SendCustomEventDelayedSeconds(nameof(RespawnTimer), 1.0f);
            }
            if (RespawnCountdown == -1)
            {
                Announcer.PlayOneShot(Boop);
                RespawnCountdown = 3;
                SendCustomEventDelayedSeconds(nameof(Respawn), 1.0f);
            }
        }

        public void Respawn()
        {
            int spawnInt = Random.Range(0, BlueSpawns.Length);
            GameObject selectedSpawn = null;

            if (CombatScript.CurrentTeam == "Blue")
            {
                selectedSpawn = BlueSpawns[spawnInt];
            }
            else if (CombatScript.CurrentTeam == "Red")
            {
                selectedSpawn = RedSpawns[spawnInt];
            }

            Settings.LocalPlayer.TeleportTo(selectedSpawn.transform.position, selectedSpawn.transform.rotation);

            Dead.enabled = false;
            PostStart.enabled = true;
            Announcer.PlayOneShot(RespawnSound);
            Settings.LocalPlayer.SetVoiceGain(1.0f);
            Settings.LocalPlayer.SetWalkSpeed(Settings.MoveSpeed);
            Settings.LocalPlayer.SetStrafeSpeed(Settings.MoveSpeed);
            Settings.LocalPlayer.SetRunSpeed(Settings.MoveSpeed);
            Settings.LocalPlayer.SetJumpImpulse(Settings.JumpHeight);
        }

        private void KillBetrayal()
        {
            Announcer.PlayOneShot(Betrayal);
        }

        private void KillBetrayed()
        {
            Announcer.PlayOneShot(Betrayed);
        }
    }
}