using Cyan.PlayerObjectPool;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Cekay.Grifball
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PooledFunctions : CyanPlayerObjectPoolObject
    {
        public HUD HeadsUp;
        public Combat CombatScript;
        public SettingsPage Settings;

        public VRCPlayerApi playerAssignedPlayer;
        public VRCPlayerApi playerUnassignedPlayer;
        public int playerAssignedIndex;
        public int playerUnassignedIndex;

        [SerializeField] private GameObject Hammer;
        [SerializeField] private GameObject HammerZone;
        [SerializeField] private GameObject HeartAudio;
        [SerializeField] private AudioSource HammerAudio;
        [SerializeField] private Material HammerMatBlue;
        [SerializeField] private Material HammerMatRed;

        [SerializeField] private GameObject HurtPost;
        [SerializeField] private GameObject DeadPost;
        [SerializeField] private GameObject SpawnPost;
        [SerializeField] private GameObject EndPost;

        [SerializeField] private ParticleSystem Confetti;

        public string LocalPlayerName;

        private Vector3 LastRotation;

        private bool Swinging = false;

        public int PlayerHealth = 100;
        public int RespawnCountdown = 3;

        [PublicAPI]
        public override void _OnOwnerSet()
        {
            if (Owner.isLocal)
            {
                LocalPlayerName = Owner.displayName;

                RequestSerialization();
            }

            Debug.Log("FUCK " + Owner.displayName);
        }

        [PublicAPI]
        public override void _OnCleanup()
        {
            if (Networking.IsMaster)
            {
                RequestSerialization();
            }
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void Update()
        {
            if (!Utilities.IsValid(Owner))
            {
                return;
            }

            if (Settings.LeftHanded)
            {
                transform.position = Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                transform.rotation = Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
            }
            else
            {
                transform.position = Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                transform.rotation = Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
            }

            if (CombatScript.InProgress == true)
            {
                if (Swinging == false)
                {
                    if (Owner.IsUserInVR() == false)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SwingHammer));
                            Swinging = true;
                        }

                        // Temporary for debugging in editor
                        Vector3 RotationDifference = new Vector3(transform.rotation.eulerAngles.x - LastRotation.x,
                                                                 transform.rotation.eulerAngles.y - LastRotation.y,
                                                                 transform.rotation.eulerAngles.z - LastRotation.z);
                        if (Mathf.Abs(RotationDifference.magnitude) >= 15)
                        {
                            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SwingHammer));
                            Swinging = true;
                        }
                    }
                    else if (Owner.IsUserInVR() == true)
                    {
                        Vector3 RotationDifference = new Vector3(transform.rotation.eulerAngles.x - LastRotation.x,
                                                                 transform.rotation.eulerAngles.y - LastRotation.y,
                                                                 transform.rotation.eulerAngles.z - LastRotation.z);
                        if (Mathf.Abs(RotationDifference.magnitude) >= 15)
                        {
                            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SwingHammer));
                            Swinging = true;
                        }
                    }
                }
            }
            LastRotation = transform.rotation.eulerAngles;
        }

        public void Die()
        {
            PlayerHealth = 0;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(HammerHide));
            HeartAudio.SetActive(false);
            HurtPost.SetActive(false);
            DeadPost.SetActive(false);
            DeadPost.SetActive(true);
            Owner.SetVoiceGain(0.0f);
            CombatScript.SetImmobile();

            if (LocalPlayerName == "Cekay")
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Cekay));
            }

            SendCustomEventDelayedSeconds(nameof(RespawnTimer), 2.0f);
        }

        public void GetHurt()
        {
            PlayerHealth = 50;
            HeartAudio.SetActive(true);
            HurtPost.SetActive(true);
        }

        public void SetHealedNetwork()
        {
            PlayerHealth = 100;
        }
        
        public void SetHealedLocal()
        {
            HeartAudio.SetActive(false);
            HurtPost.SetActive(false);
        }

        private void KillBetrayed()
        {
            Settings.AnnouncerAudio.PlayOneShot(CombatScript.Betrayed);
        }

        public void RespawnTimer()
        {
            if (CombatScript.IsPaused)
            {
                return;
            }

            RespawnCountdown--;
            if (RespawnCountdown >= 0)
            {
                Settings.AnnouncerAudio.PlayOneShot(CombatScript.Boop);
                SendCustomEventDelayedSeconds(nameof(RespawnTimer), 1.0f);
            }
            if (RespawnCountdown == -1)
            {
                Settings.AnnouncerAudio.PlayOneShot(CombatScript.Beep);
                RespawnCountdown = 3;
                SendCustomEventDelayedSeconds(nameof(Respawn), 1.0f);
            }
        }

        public void Respawn()
        {
            CombatScript.RespawnLocal();

            DeadPost.SetActive(false);
            HurtPost.SetActive(false);
            SpawnPost.SetActive(false);
            SpawnPost.SetActive(true);
            Settings.AnnouncerAudio.PlayOneShot(CombatScript.RespawnSound);
            CombatScript.SetMobile();
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(HammerShow));
        }

        public void GameStarted()
        {
            var mats = Hammer.GetComponent<MeshRenderer>().sharedMaterials;

            if (Settings.RedTeam.Contains(LocalPlayerName))
            {
                mats[3] = HammerMatRed;
                Hammer.GetComponent<MeshRenderer>().sharedMaterials = mats;
            }
            if (Settings.BlueTeam.Contains(LocalPlayerName))
            {
                mats[3] = HammerMatBlue;
                Hammer.GetComponent<MeshRenderer>().sharedMaterials = mats;
            }
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(HammerShow));
        }

        public void GameEnded()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(HammerHide));
        }

        public void HammerHide()
        {
            Hammer.SetActive(false);
        }

        public void HammerShow()
        {
            Hammer.SetActive(true);
        }

        public void SwingHammer()
        {
            HammerZone.SetActive(true);
            HammerAudio.PlayOneShot(CombatScript.Melees[Random.Range(0, CombatScript.Melees.Length)]);
            SendCustomEventDelayedSeconds(nameof(ResetHammer), 2.0f);

            if (Settings.ShowMeter && PlayerHealth > 0)
            {
                HeadsUp.EnableSwingMeter();
            }
        }

        public void ResetHammer()
        {
            HammerZone.SetActive(false);
            SendCustomEventDelayedSeconds(nameof(ResetSwing), 1.0f);
        }

        public void ResetSwing()
        {
            Swinging = false;
        }

        public void Cekay()
        {
            Confetti.transform.position = Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            Confetti.transform.rotation = Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            Confetti.Play();
            Settings.AnnouncerAudio.PlayOneShot(CombatScript.Birthday);
        }
    }
}