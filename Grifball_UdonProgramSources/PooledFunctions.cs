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

        [SerializeField] private GameObject Hammer;
        [SerializeField] private GameObject HammerZone;
        [SerializeField] private AudioSource HammerAudio;
        [SerializeField] private Material HammerMatBlue;
        [SerializeField] private Material HammerMatRed;

        private VRCPlayerApi _localPlayer;
        public string LocalPlayerName;

        private Vector3 LastRotation;

        private bool Swinging = false;

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
            Hammer.SetActive(true);
        }

        public void GameEnded()
        {
            Hammer.SetActive(false);
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

            if (Settings.ShowMeter)
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