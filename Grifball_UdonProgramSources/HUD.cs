using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Cekay.Grifball
{
    public class HUD : UdonSharpBehaviour
    {
        public SettingsPage Settings;
        public VRCPlayerApi LocalPlayerApi;

        [SerializeField] private GameObject HeadFollower;
        [SerializeField] private GameObject JoinObj;
        [SerializeField] private GameObject SwingMeter;
        [SerializeField] private TextMeshProUGUI NameText;

        [SerializeField] private AudioClip Join;
        [SerializeField] private AudioClip Leave;

        private void Start()
        {
            LocalPlayerApi = Networking.LocalPlayer;
        }

        private void Update()
        {
            HeadFollower.transform.SetPositionAndRotation(LocalPlayerApi.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position,
                                                          LocalPlayerApi.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            JoinObj.SetActive(true);
            string newText = player.displayName + " joined";
            Settings.InterfaceAudio.PlayOneShot(Join);
            NameText.text = newText;
            SendCustomEventDelayedSeconds(nameof(DisableCanvas), 3.5f);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            JoinObj.SetActive(true);
            string newText = player.displayName + " left";
            Settings.InterfaceAudio.PlayOneShot(Leave);
            NameText.text = newText;
            SendCustomEventDelayedSeconds(nameof(DisableCanvas), 3.5f);
        }

        public void DisableCanvas()
        {
            JoinObj.SetActive(false);
        }

        public void EnableSwingMeter()
        {
            SwingMeter.SetActive(true);
            SendCustomEventDelayedSeconds(nameof(DisableSwingMeter), 3.0f);
        }

        public void DisableSwingMeter()
        {
            SwingMeter.SetActive(false);
        }
    }
}