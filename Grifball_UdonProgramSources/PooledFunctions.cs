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
        public Player PlayerLocal;

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

        private VRCPlayerApi _localPlayer;
        public string LocalPlayerName;

        private Vector3 LastRotation;

        private bool Swinging = false;

        public int PlayerHealth = 100;
        public int RespawnCountdown = 3;

        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            LocalPlayerName = _localPlayer.displayName;
        }

        [PublicAPI]
        public override void _OnOwnerSet()
        {
            if (Owner.isLocal)
            {
                RequestSerialization();
            }
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
                    if (_localPlayer.IsUserInVR() == false)
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
                    else if (_localPlayer.IsUserInVR() == true)
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

        // TODO: Move local player functions to be pooled so each PlayerAPI can easily be called

        //public override void OnPlayerCollisionEnter(VRCPlayerApi Player)
        //{
        //    if (Player != _localPlayer)
        //    {
        //        string playerName = Player.displayName;

        //        if ((Settings.BlueTeam.Contains(playerName) && (CombatScript.CurrentTeam == "Red")) ||
        //            (Settings.RedTeam.Contains(playerName) && (CombatScript.CurrentTeam == "Blue")))
        //        {
        //            Die();
        //        }
        //        else
        //        {
        //            if (Settings.FriendlyFire)
        //            {
        //                if ((Settings.RedTeam.Contains(playerName) && (CombatScript.CurrentTeam == "Red")) ||
        //                    (Settings.BlueTeam.Contains(playerName) && (CombatScript.CurrentTeam == "Blue")))
        //                {
        //                    Die();
        //                    KillBetrayal();
        //                }
        //            }
        //        }
        //    }
        //}

        public void Die()
        {
            PlayerHealth = 0;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(HammerHide));
            HeartAudio.SetActive(false);
            HurtPost.SetActive(false);
            DeadPost.SetActive(false);
            DeadPost.SetActive(true);
            Settings.LocalPlayer.SetVoiceGain(0.0f);
            Settings.LocalPlayer.SetWalkSpeed(0.0f);
            Settings.LocalPlayer.SetStrafeSpeed(0.0f);
            Settings.LocalPlayer.SetRunSpeed(0.0f);
            Settings.LocalPlayer.SetJumpImpulse(0.0f);

            SendCustomEventDelayedSeconds(nameof(RespawnTimer), 2.0f);
        }

        public void GetHurt()
        {
            PlayerHealth = 50;
            HeartAudio.SetActive(true);
            HurtPost.SetActive(true);
        }

        public void Heal()
        {
            PlayerHealth = 100;
            HeartAudio.SetActive(false);
            HurtPost.SetActive(false);
        }

        public void RespawnTimer()
        {
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
            int spawnInt = Random.Range(0, 3);
            GameObject selectedSpawn = null;

            if (CombatScript.CurrentTeam == "Blue")
            {
                selectedSpawn = CombatScript.BlueSpawns[spawnInt];
            }
            else if (CombatScript.CurrentTeam == "Red")
            {
                selectedSpawn = CombatScript.RedSpawns[spawnInt];
            }

            Settings.LocalPlayer.TeleportTo(selectedSpawn.transform.position, selectedSpawn.transform.rotation);

            DeadPost.SetActive(false);
            HurtPost.SetActive(false);
            SpawnPost.SetActive(false);
            SpawnPost.SetActive(true);
            Settings.AnnouncerAudio.PlayOneShot(CombatScript.RespawnSound);
            Settings.LocalPlayer.SetVoiceGain(1.0f);
            Settings.LocalPlayer.SetWalkSpeed(Settings.MoveSpeed);
            Settings.LocalPlayer.SetStrafeSpeed(Settings.MoveSpeed);
            Settings.LocalPlayer.SetRunSpeed(Settings.MoveSpeed);
            Settings.LocalPlayer.SetJumpImpulse(Settings.JumpHeight);
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(HammerShow));
        }

        public void GameStarted()
        {
            var mats = Hammer.GetComponent<MeshRenderer>().sharedMaterials;

            string heldBy = _localPlayer.displayName;
            if (Settings.RedTeam.Contains(heldBy))
            {
                mats[3] = HammerMatRed;
                Hammer.GetComponent<MeshRenderer>().sharedMaterials = mats;
            }
            if (Settings.BlueTeam.Contains(heldBy))
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
    }
}