using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;

namespace Cekay.Grifball
{
    public class SettingsPage : UdonSharpBehaviour
    {
        [SerializeField] private Combat CombatScript;

        public AudioSource InterfaceAudio;

        [SerializeField] private AudioClip Hover;
        [SerializeField] private AudioClip Click;

        public int MoveSpeed = 12;
        public int MoveSpeedCarrier = 12;
        public int JumpHeight = 8;
        public int RoundLength = 60;
        public int RoundsToPlay = 3;
        public bool BallPhysics = false;
        public bool FriendlyFire = true;

        public float AnnouncerVolume = 1.0f;
        public float InterfaceVolume = 1.0f;
        public float WeaponVolume = 1.0f;

        [SerializeField] private Slider AnnouncerVolSlider;
        [SerializeField] private Slider InterfaceVolSlider;
        [SerializeField] private Slider WeaponVolSlider;

        [SerializeField] private Toggle BallPhysicsToggle;
        [SerializeField] private Toggle FriendlyFireToggle;

        public DataList BlueTeam;
        public DataList RedTeam;
        public TextMeshProUGUI BlueTeamDisplay;
        public TextMeshProUGUI RedTeamDisplay;

        public void Start()
        {
            MoveSpeedCarrier = MoveSpeed + 4;
        }

        public void JoinRed()
        {
            CombatScript.CurrentTeam = "Red";
            string name = CombatScript.LocalPlayerAPI.displayName;
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

        public void JoinBlue()
        {
            CombatScript.CurrentTeam = "Blue";
            string name = CombatScript.LocalPlayerAPI.displayName;
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
        
        public void SetInterfaceVol()
        {
            InterfaceAudio.volume = InterfaceVolSlider.value;
        }

        public void SetBallPhysics()
        {
            BallPhysics = BallPhysicsToggle.isOn;
        }

        public void SetFriendlyFire()
        {
            FriendlyFire = FriendlyFireToggle.isOn;
        }
    }
}