using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Cekay.Grifball
{
    public class Combat : UdonSharpBehaviour
    {
        [SerializeField] private Transform BombTarget;
        [SerializeField] private GameObject Bomb;
        [SerializeField] private Rigidbody BombRigid;
        [SerializeField] private Transform BombTrans;
        [SerializeField] private VRCPickup BombPickup;
        [SerializeField] private Material BombMat;

        public Player PlayerLocal;

        [SerializeField] private PostProcessVolume PostStart;
        [SerializeField] private PostProcessVolume PostEnd;
        [SerializeField] private PostProcessVolume Hurt;
        [SerializeField] private PostProcessVolume Dead;

        public bool InProgress = false;
        public bool InSuddenDeath = false;
        public bool IsPaused = false;

        private int RoundCountdown = 60;
        private int GameCountdown = 6;
        public int RoundNumber = 1;

        [SerializeField] private TextMeshProUGUI RoundCountdownDisplay;
        [SerializeField] private TextMeshProUGUI RoundNumberDisplay;
        [SerializeField] private TextMeshProUGUI GameCountdownDisplay;

        public AudioSource Announcer;
        [SerializeField] private AudioClip PlayBall;
        [SerializeField] private AudioClip BombTaken;
        [SerializeField] private AudioClip BombDropped;
        [SerializeField] private AudioClip BombReset;
        [SerializeField] private AudioClip BombArmed;
        [SerializeField] private AudioClip BombDetonated;
        [SerializeField] private AudioClip OneMinute;
        [SerializeField] private AudioClip ThirtySeconds;
        [SerializeField] private AudioClip TenSeconds;
        [SerializeField] private AudioClip SuddenDeath;
        [SerializeField] private AudioClip RoundOver;
        [SerializeField] private AudioClip GameOver;
        [SerializeField] private AudioClip RespawnSound;
        [SerializeField] private AudioClip Final;

        public AudioClip[] Melees;
        public AudioClip[] Punches;
        public AudioClip[] Explosions;

        [SerializeField] private GameObject[] LobbySpawns;

        public string CurrentTeam;

        public Collider BlueGoal;
        [UdonSynced] public int BluePoints;
        [UdonSynced] public int BlueGamesWon;
        public GameObject BlueExplosion;
        public int BlueBombLayer = 27;
        public int BlueGoalLayer = 23;
        public int BlueKillLayer = 29;

        public Collider RedGoal;
        [UdonSynced] public int RedPoints;
        [UdonSynced] public int RedGamesWon;
        public GameObject RedExplosion;
        public int RedBombLayer = 26;
        public int RedGoalLayer = 22;
        public int RedKillLayer = 30;

        public VRCPlayerApi LocalPlayerAPI;

        [SerializeField] private PooledFunctions[] Players;
        [SerializeField] private SettingsPage Settings;

        public float TimeStored;
        public float TimeCurrent;

        private bool AnnouncerSpeaking;

        public DataDictionary PreviousGames;

        private void Start()
        {
            LocalPlayerAPI = Networking.LocalPlayer;
        }

        public void StartGameCountdown()
        {
            GameCountdown--;
            if (GameCountdown >= 0)
            {
                SendCustomEventDelayedSeconds(nameof(StartGameCountdown), 1.0f);
                GameCountdownDisplay.text = GameCountdown.ToString();
            }
            if (GameCountdown == -1)
            {
                SendCustomEventDelayedSeconds(nameof(StartRound), 1.0f);
                GameCountdown = 6;
                GameCountdownDisplay.text = "";
            }
        }

        public void StartWait()
        {
            SendCustomEventDelayedSeconds(nameof(StartRound), 6.0f);
        }

        public void StartRound()
        {
            SendCustomEventDelayedSeconds(nameof(RoundTimer), 1.0f);

            foreach (PooledFunctions p in Players)
            {
                p.GameStarted();
            }

            PostStart.gameObject.SetActive(true);
            Announcer.PlayOneShot(PlayBall);

            Bomb.SetActive(true);
            BombTrans.position = BombTarget.position;
            BombRigid.isKinematic = false;

            PlayerLocal.Respawn();

            InProgress = true;
        }

        public void GrabBomb()
        {
            if (!Settings.BallPhysics)
            {
                BombRigid.constraints = RigidbodyConstraints.None;
            }
            if (Bomb.activeSelf == true && AnnouncerSpeaking == false)
            {
                Announcer.PlayOneShot(BombTaken);
                AnnouncerSpeaking = true;
                SendCustomEventDelayedSeconds(nameof(ResetAnnouncerSpeaking), 1.0f);
            }

            string heldBy = BombPickup.currentPlayer.displayName;
            if (Settings.RedTeam.Contains(heldBy))
            {
                Bomb.layer = RedBombLayer;
                BombMat.SetColor("_EmissionColor", new Color(1.0f, 0.0f, 0.0f) * 5);
            }
            if (Settings.BlueTeam.Contains(heldBy))
            {
                Bomb.layer = BlueBombLayer;
                BombMat.SetColor("_EmissionColor", new Color(0.0f, 0.25f, 1.0f) * 5);
            }
        }

        public void DropBomb()
        {
            if (!Settings.BallPhysics)
            {
                BombRigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                BombTrans.rotation = Quaternion.identity;
            }
            if (Bomb.activeSelf == true && AnnouncerSpeaking == false)
            {
                Announcer.PlayOneShot(BombDropped);
                AnnouncerSpeaking = true;
                SendCustomEventDelayedSeconds(nameof(ResetAnnouncerSpeaking), 2.0f);
            }
            BombMat.SetColor("_EmissionColor", new Color(0.7f, 0.5f, 0.0f) * 5);
        }

        public void ResetAnnouncerSpeaking()
        {
            AnnouncerSpeaking = false;
        }

        public void Goal()
        {
            Bomb.SetActive(false);
            Announcer.PlayOneShot(BombDetonated);
            SendCustomEventDelayedSeconds(nameof(EndRound), 2.0f);
        }

        private void EndRound()
        {
            Announcer.PlayOneShot(RoundOver);
            RoundNumber++;
            string roundNumberString = "Round\n" + RoundNumber.ToString() + "/" + Settings.RoundsToPlay.ToString();
            RoundNumberDisplay.text = roundNumberString;
            RoundCountdown = Settings.RoundLength;
            RedExplosion.SetActive(false);
            BlueExplosion.SetActive(false);

            StartWait();
        }

        public void EndGame()
        {
            Announcer.PlayOneShot(GameOver);
            InProgress = false;
            InSuddenDeath = false;

            int spawnInt = Random.Range(0, LobbySpawns.Length);
            GameObject selectedSpawn = LobbySpawns[spawnInt];
            LocalPlayerAPI.TeleportTo(selectedSpawn.transform.position, selectedSpawn.transform.rotation);

            foreach (PooledFunctions p in Players)
            {
                p.GameEnded();
            }
        }

        public void RoundTimer()
        {
            RoundCountdown--;
            if (RoundCountdown >= 0 && IsPaused == false)
            {
                SendCustomEventDelayedSeconds(nameof(RoundTimer), 1.0f);
                RoundCountdownDisplay.text = RoundCountdown.ToString();
            }
            AnnounceTime();
        }

        public void AnnounceTime()
        {
            if (RoundCountdown == 60)
            {
                Announcer.PlayOneShot(OneMinute);
            }
            if (RoundCountdown == 30)
            {
                Announcer.PlayOneShot(ThirtySeconds);
            }
            if (RoundCountdown == 10)
            {
                Announcer.PlayOneShot(TenSeconds);
            }
            if ((RoundCountdown == 0) && (RoundNumber < Settings.RoundsToPlay))
            {
                Announcer.PlayOneShot(RoundOver);
                SendCustomEventDelayedSeconds(nameof(EndRound), 3.0f);
            }
            if ((RoundCountdown == 0) && (BluePoints == RedPoints) && (RoundNumber == Settings.RoundsToPlay))
            {
                Announcer.PlayOneShot(SuddenDeath);
                InSuddenDeath = true;
            }
            if ((RoundCountdown == 0) && (BluePoints != RedPoints) && (RoundNumber == Settings.RoundsToPlay))
            {
                Announcer.PlayOneShot(GameOver);
                SendCustomEventDelayedSeconds(nameof(EndGame), 3.0f);
            }
        }
    }
}