using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Cekay.Grifball
{
    public class SettingsPage : UdonSharpBehaviour
    {
        public Combat CombatScript;
        public AudioSource InterfaceAudio;

        public VRCPlayerApi LocalPlayer;

        [SerializeField] private AudioSource AnnouncerAudio;
        [SerializeField] private AudioSource AmbienceAudio;
        [SerializeField] private AudioSource FootstepsAudio;
        [SerializeField] private AudioSource[] WeaponsAudio;

        [SerializeField] private AudioClip Hover;
        [SerializeField] private AudioClip Click;

        public int MoveSpeed = 12;
        public int MoveSpeedCarrier = 12;
        public int JumpHeight = 8;
        public int RoundLength = 60;
        public int RoundsToPlay = 3;
        public bool BallPhysics = false;
        public bool FriendlyFire = true;
        public bool LeftHanded = true;
        public bool ShowMeter = true;

        public float AnnouncerVolume = 1.0f;
        public float InterfaceVolume = 1.0f;
        public float WeaponVolume = 1.0f;

        [SerializeField] private Slider AnnouncerVolSlider;
        [SerializeField] private Slider AmbienceVolSlider;
        [SerializeField] private Slider FootstepsVolSlider;
        [SerializeField] private Slider WeaponsVolSlider;
        [SerializeField] private Slider RoundLengthSlider;
        [SerializeField] private Slider RoundsSlider;
        [SerializeField] private Slider MoveSlider;
        [SerializeField] private Slider JumpSlider;
        [SerializeField] private TextMeshProUGUI RoundLengthSliderText;
        [SerializeField] private TextMeshProUGUI RoundsSliderText;
        [SerializeField] private Dropdown PlayerList;
        [SerializeField] private Toggle BallPhysicsToggle;
        [SerializeField] private Toggle FriendlyFireToggle;

        [UdonSynced] public string BlueTeamJSON;
        public DataList BlueTeam;
        public TextMeshProUGUI BlueTeamDisplay;

        [UdonSynced] public string RedTeamJSON;
        public DataList RedTeam;
        public TextMeshProUGUI RedTeamDisplay;


        [SerializeField] private GameObject OldSpawn;
        [SerializeField] private GameObject NewSpawn;

        [SerializeField] private GameObject MirrorH;
        [SerializeField] private GameObject MirrorL;
        [SerializeField] private GameObject MirrorT;

        [SerializeField] private GameObject CreditsCanvas;
        [SerializeField] private GameObject OtherWorlds;
        [SerializeField] private GameObject DiscordCanvas;

        private VRCPlayerApi[] allPlayers = new VRCPlayerApi[9];

        public void Start()
        {
            LocalPlayer = Networking.LocalPlayer;
            MoveSpeedCarrier = MoveSpeed + 4;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            for (int i = 0; i < allPlayers.Length; i++)
            {
                if (allPlayers[i] == null)
                {
                    allPlayers[i] = player;
                    //PlayerList.AddOptions(allPlayers[i].displayName));
                    break;
                }
                Debug.Log(allPlayers[i].displayName);
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            PlayerList.ClearOptions();
            for (int i = 0; i < allPlayers.Length; i++)
            {
                if (allPlayers[i] == player)
                {
                    allPlayers[i] = null;
                    break;
                }
            }
        }

        public override void OnPreSerialization()
        {
            if (VRCJson.TrySerializeToJson(BlueTeam, JsonExportType.Minify, out DataToken resultB))
            {
                BlueTeamJSON = resultB.String;
            }
            else
            {
                Debug.LogError(resultB.ToString());
            }

            if (VRCJson.TrySerializeToJson(RedTeam, JsonExportType.Minify, out DataToken resultR))
            {
                RedTeamJSON = resultR.String;
            }
            else
            {
                Debug.LogError(resultR.ToString());
            }
        }

        public override void OnDeserialization()
        {
            if (VRCJson.TryDeserializeFromJson(BlueTeamJSON, out DataToken resultB))
            {
                BlueTeam = resultB.DataList;
            }
            else
            {
                Debug.LogError(resultB.ToString());
            }

            if (VRCJson.TryDeserializeFromJson(RedTeamJSON, out DataToken resultR))
            {
                RedTeam = resultR.DataList;
            }
            else
            {
                Debug.LogError(resultR.ToString());
            }
        }

        public void AcceptWarning()
        {
            OldSpawn.transform.position = NewSpawn.transform.position;
            OldSpawn.transform.rotation = NewSpawn.transform.rotation;
            LocalPlayer.Respawn();
        }

        public void JoinRed()
        {
            CombatScript.CurrentTeam = "Red";
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetRedTeamText));

            RequestSerialization();
        }

        public void JoinBlue()
        {
            CombatScript.CurrentTeam = "Blue";
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetBlueTeamText));

            RequestSerialization();
        }

        public void SetBlueTeamText()
        {
            string name = LocalPlayer.displayName;
            if (!BlueTeam.Contains(name))
            {
                BlueTeam.Add(name);
                string teamString = "";
                for (int i = 0; i < BlueTeam.Count; i++)
                {
                    teamString += BlueTeam[i] + "\n";
                }

                BlueTeamDisplay.text = teamString;
                RedTeam.RemoveAll(name);

                string redTeamString = "";
                for (int i = 0; i < RedTeam.Count; i++)
                {
                    redTeamString += RedTeam[i] + "\n";
                }

                RedTeamDisplay.text = redTeamString;
            }
        }

        public void SetRedTeamText()
        {
            string name = LocalPlayer.displayName;
            if (!RedTeam.Contains(name))
            {
                RedTeam.Add(name);
                string redTeamString = "";
                for (int i = 0; i < RedTeam.Count; i++)
                {
                    redTeamString += RedTeam[i] + "\n";
                }

                RedTeamDisplay.text = redTeamString;
                BlueTeam.RemoveAll(name);

                string blueTeamString = "";
                for (int i = 0; i < BlueTeam.Count; i++)
                {
                    blueTeamString += BlueTeam[i] + "\n";
                }

                BlueTeamDisplay.text = blueTeamString;
            }
        }

        public void GetPlayers()
        {
            //VRCPlayerApi[] inLobby = VRCPlayerApi.GetPlayers();
        }

        public void InterfaceHover()
        {
            InterfaceAudio.PlayOneShot(Hover);
        }

        public void InterfaceClick()
        {
            InterfaceAudio.PlayOneShot(Click);
        }

        public void SetAnnouncerVol()
        {
            CombatScript.Announcer.volume = AnnouncerVolSlider.value;
        }

        public void SetAmbienceVol()
        {
            AmbienceAudio.volume = AmbienceVolSlider.value;
        }

        public void SetFootstepsVol()
        {
            FootstepsAudio.volume = FootstepsVolSlider.value;
        }

        public void SetWeaponsVol()
        {
            foreach (AudioSource s in WeaponsAudio)
            {
                s.volume = WeaponsVolSlider.value;
            }
        }

        public void SetBallPhysics()
        {
            if (LocalPlayer.isInstanceOwner)
            {
                BallPhysics = BallPhysicsToggle.isOn;
            }
        }

        public void SetFriendlyFire()
        {
            FriendlyFire = FriendlyFireToggle.isOn;
        }

        public void SetMove()
        {
            LocalPlayer.SetRunSpeed(MoveSlider.value);
            LocalPlayer.SetWalkSpeed(MoveSlider.value);
            LocalPlayer.SetStrafeSpeed(MoveSlider.value);
        }

        public void SetJump()
        {
            LocalPlayer.SetJumpImpulse(JumpSlider.value);
        }

        public void SetRoundLength()
        {
            RoundLength = ((int)RoundLengthSlider.value);
            RoundLengthSliderText.text = CombatScript.DisplayTime(RoundLength);
        }

        public void SetRounds()
        {
            RoundsToPlay = ((int)RoundsSlider.value);
            RoundsSliderText.text = RoundsToPlay.ToString();
            string roundNumberString = CombatScript.RoundNumber.ToString() + "/" + RoundsToPlay.ToString();
            CombatScript.RoundNumberDisplay.text = roundNumberString;
        }

        public void SetOwner()
        {
            //LocalPlayer.TakeOwnership
        }

        public void SetMirrorHQ()
        {
            MirrorH.SetActive(!MirrorH.activeSelf);
            MirrorL.SetActive(false);
            MirrorT.SetActive(false);
        }

        public void SetMirrorLQ()
        {
            MirrorH.SetActive(false);
            MirrorL.SetActive(!MirrorL.activeSelf);
            MirrorT.SetActive(false);
        }

        public void SetMirrorTrans()
        {
            MirrorH.SetActive(false);
            MirrorL.SetActive(false);
            MirrorT.SetActive(!MirrorT.activeSelf);
        }

        public void ToggleCredits()
        {
            CreditsCanvas.SetActive(!CreditsCanvas.activeSelf);
        }

        public void ToggleOtherWorlds()
        {
            OtherWorlds.SetActive(!OtherWorlds.activeSelf);
        }

        public void ToggleDiscord()
        {
            DiscordCanvas.SetActive(false);
        }

        public void ToggleMeter()
        {
            ShowMeter = !ShowMeter;
        }

        public void SetLeftHanded()
        {
            LeftHanded = true;
        }

        public void SetRightHanded()
        {
            LeftHanded = false;
        }
    }
}