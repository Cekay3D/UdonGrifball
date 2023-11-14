using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Cekay.Grifball
{
    public class Combat : UdonSharpBehaviour
    {
        public Player PlayerLocal;
        public SettingsPage Settings;
        public PooledFunctions[] Players;

        public VRCPlayerApi LocalPlayerAPI;
        public string CurrentTeam;

        [SerializeField] private Transform BombTarget;
        [SerializeField] private Transform BombTrans;
        [SerializeField] private GameObject Bomb;
        [SerializeField] private Rigidbody BombRigid;
        [SerializeField] private VRCPickup BombPickup;
        [SerializeField] private Material BombMat;

        [SerializeField] private PostProcessVolume PostStart;
        [SerializeField] private PostProcessVolume PostEnd;
        [SerializeField] private PostProcessVolume Hurt;
        [SerializeField] private PostProcessVolume Dead;

        [UdonSynced] public bool InProgress = false;
        [UdonSynced] public bool InSuddenDeath = false;
        [UdonSynced] public bool IsPaused = false;

        [UdonSynced] private int RoundCountdown = 60;
        [UdonSynced] private int GameCountdown = 6;
        [UdonSynced] public int RoundNumber = 1;

        [SerializeField] private TextMeshProUGUI RoundCountdownDisplay;
        public TextMeshProUGUI RoundNumberDisplay;
        [SerializeField] private TextMeshProUGUI GameCountdownDisplay;

        public AudioSource Announcer;
        public AudioClip Beep;
        public AudioClip Boop;
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
        public AudioClip RespawnSound;
        [SerializeField] private AudioClip Final;

        [SerializeField] private AudioClip s1;
        [SerializeField] private AudioClip s2;
        [SerializeField] private AudioClip s3;
        [SerializeField] private AudioClip s4;
        [SerializeField] private AudioClip s5;
        [SerializeField] private AudioClip s6;
        [SerializeField] private AudioClip s7;
        [SerializeField] private AudioClip s8;

        [SerializeField] private AudioClip k2;
        [SerializeField] private AudioClip k3;
        [SerializeField] private AudioClip k4;
        [SerializeField] private AudioClip k5;
        [SerializeField] private AudioClip k6;
        [SerializeField] private AudioClip k7;
        [SerializeField] private AudioClip k8;
        [SerializeField] private AudioClip k9;
        [SerializeField] private AudioClip k10;

        [SerializeField] private AudioClip FirstStrike;
        [SerializeField] private AudioClip Revenge;
        [SerializeField] private AudioClip Killjoy;

        public AudioClip[] Melees;
        public AudioClip[] Punches;
        public AudioClip[] Explosions;

        [SerializeField] private Image MultikillImage;
        [SerializeField] private Image SpreeImage;
        [SerializeField] private Image SpecialImage;
        [SerializeField] private GameObject MultikillObj;
        [SerializeField] private GameObject SpreeObj;
        [SerializeField] private GameObject SpecialObj;
        [SerializeField] private Sprite k2s;
        [SerializeField] private Sprite k3s;
        [SerializeField] private Sprite k4s;
        [SerializeField] private Sprite k5s;
        [SerializeField] private Sprite k6s;
        [SerializeField] private Sprite k7s;
        [SerializeField] private Sprite k8s;
        [SerializeField] private Sprite k9s;
        [SerializeField] private Sprite k10s;
        [SerializeField] private Sprite s1s;
        [SerializeField] private Sprite s2s;
        [SerializeField] private Sprite s3s;
        [SerializeField] private Sprite s4s;
        [SerializeField] private Sprite s5s;
        [SerializeField] private Sprite s6s;
        [SerializeField] private Sprite s7s;
        [SerializeField] private Sprite s8s;
        [SerializeField] private Sprite RevengeS;
        [SerializeField] private Sprite FirstStrikeS;
        [SerializeField] private Sprite KilljoyS;
        [SerializeField] private Sprite FirstStrikeSprite;
        [SerializeField] private Sprite BombPlanted;

        [SerializeField] private GameObject[] LobbySpawns;

        public Collider BlueGoal;
        [UdonSynced] public int BluePoints;
        [UdonSynced] public int BlueGamesWon;
        public GameObject BlueExplosion;
        public int BlueBombLayer = 27;
        public int BlueGoalLayer = 23;
        public int BlueKillLayer = 29;
        public GameObject[] BlueSpawns;

        public Collider RedGoal;
        [UdonSynced] public int RedPoints;
        [UdonSynced] public int RedGamesWon;
        public GameObject RedExplosion;
        public int RedBombLayer = 26;
        public int RedGoalLayer = 22;
        public int RedKillLayer = 30;
        public GameObject[] RedSpawns;

        public DataDictionary PreviousGames;

        public int SpreeInt = 0;
        public int MultikillInt = 0;
        public int RoundKills = 0;
        private bool AnnouncerSpeaking;
        public bool MultikillActive = false;
        public float LastKillTime = 0.0f;
        public float MultikillTimer = 0.0f;
        public float TimeStored;
        public float TimeCurrent;

        private void Start()
        {
            LocalPlayerAPI = Networking.LocalPlayer;
        }

        private void Update()
        {
            //MultikillTimer += Time.deltaTime;
        }

        public void StartGameNetwork()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(StartGameCountdown));
        }

        public void StartGameCountdown()
        {
            GameCountdown--;
            if (GameCountdown >= 0)
            {
                SendCustomEventDelayedSeconds(nameof(StartGameCountdown), 1.0f);
                GameCountdownDisplay.text = GameCountdown.ToString();
            }
            if (GameCountdown < 4 && GameCountdown > 0)
            {
                Settings.InterfaceAudio.PlayOneShot(Boop);
            }
            if (GameCountdown == 0)
            {
                Settings.InterfaceAudio.PlayOneShot(Beep);
            }
            if (GameCountdown == -1)
            {
                SendCustomEventDelayedSeconds(nameof(StartGame), 1.0f);
                GameCountdown = 6;
            }
        }

        public void StartWait()
        {
            SendCustomEventDelayedSeconds(nameof(StartRound), 6.0f);
        }

        public void StartGame()
        {
            RoundNumber = 1;
            BluePoints = 0;
            RedPoints = 0;

            GameCountdownDisplay.text = "";
            StartRound();
            Announcer.PlayOneShot(PlayBall);
        }

        public void StartRound()
        {
            IsPaused = false;
            RoundCountdown = Settings.RoundLength;
            RoundTimer();

            foreach (PooledFunctions p in Players)
            {
                p.SendCustomNetworkEvent(NetworkEventTarget.All, "GameStarted");
                p.SendCustomNetworkEvent(NetworkEventTarget.All, "Respawn");
            }

            PostStart.gameObject.SetActive(true);
            Bomb.SetActive(true);
            BombTrans.position = BombTarget.position;
            BombRigid.isKinematic = false;

            InProgress = true;
            RequestSerialization();
        }

        public void GrabBomb()
        {
            BombRigid.constraints = RigidbodyConstraints.None;

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
            else
            {
                BombRigid.constraints = RigidbodyConstraints.None;
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

        public void GoalGet()
        {
            Bomb.SetActive(false);
            Announcer.PlayOneShot(BombDetonated);

            SpecialObj.SetActive(false);
            SpecialObj.SetActive(true);
            SpecialImage.sprite = BombPlanted;

            if (InSuddenDeath)
            {
                Announcer.PlayOneShot(GameOver);
                SendCustomEventDelayedSeconds(nameof(EndGame), 5.0f);
            }
            else
            {
                SendCustomEventDelayedSeconds(nameof(EndRound), 5.0f);
            }
        }

        public void EndRound()
        {
            RoundNumber += 1;
            string roundNumberString = RoundNumber.ToString() + "/" + Settings.RoundsToPlay.ToString();
            RoundNumberDisplay.text = roundNumberString;
            RoundCountdown = Settings.RoundLength;
            RedExplosion.SetActive(false);
            BlueExplosion.SetActive(false);

            StartRound();
        }

        public void EndGame()
        {
            Announcer.PlayOneShot(Final);
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
            if (IsPaused)
            {
                return;
            }

            RoundCountdown -= 1;
            if (RoundCountdown >= 0)
            {
                SendCustomEventDelayedSeconds(nameof(RoundTimer), 1.0f);
                RoundCountdownDisplay.text = DisplayTime(RoundCountdown);
            }
            AnnounceTime();
        }

        public string DisplayTime(float timeToDisplay)
        {
            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
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
                SendCustomEventDelayedSeconds(nameof(EndRound), 5.0f);
            }
            if ((RoundCountdown == 0) && (BluePoints == RedPoints) && (RoundNumber == Settings.RoundsToPlay))
            {
                Announcer.PlayOneShot(SuddenDeath);
                InSuddenDeath = true;
            }
            if ((RoundCountdown == 0) && (BluePoints != RedPoints) && (RoundNumber == Settings.RoundsToPlay))
            {
                Announcer.PlayOneShot(GameOver);
                SendCustomEventDelayedSeconds(nameof(EndGame), 5.0f);
            }
        }

        public void Spree()
        {
            switch (SpreeInt)
            {
                case 5:
                    SpreeObj.SetActive(false);
                    Announcer.PlayOneShot(s1);
                    SpreeImage.sprite = s1s;
                    SpreeObj.SetActive(true);
                    break;

                case 10:
                    SpreeObj.SetActive(false);
                    Announcer.PlayOneShot(s2);
                    SpreeImage.sprite = s2s;
                    SpreeObj.SetActive(true);
                    break;

                case 15:
                    SpreeObj.SetActive(false);
                    Announcer.PlayOneShot(s3);
                    SpreeImage.sprite = s3s;
                    SpreeObj.SetActive(true);
                    break;

                case 20:
                    SpreeObj.SetActive(false);
                    Announcer.PlayOneShot(s4);
                    SpreeImage.sprite = s4s;
                    SpreeObj.SetActive(true);
                    break;

                case 25:
                    SpreeObj.SetActive(false);
                    Announcer.PlayOneShot(s5);
                    SpreeImage.sprite = s5s;
                    SpreeObj.SetActive(true);
                    break;

                case 30:
                    SpreeObj.SetActive(false);
                    Announcer.PlayOneShot(s6);
                    SpreeImage.sprite = s6s;
                    SpreeObj.SetActive(true);
                    break;

                case 35:
                    SpreeObj.SetActive(false);
                    Announcer.PlayOneShot(s7);
                    SpreeImage.sprite = s7s;
                    SpreeObj.SetActive(true);
                    break;

                case 40:
                    SpreeObj.SetActive(false);
                    Announcer.PlayOneShot(s8);
                    SpreeImage.sprite = s8s;
                    SpreeObj.SetActive(true);

                    SpreeInt = 0;
                    MultikillInt = 0;
                    RoundKills = 0;
                    break;
            }
        }

        public void StartMultikill()
        {
            SpreeInt += 1;
            MultikillInt += 1;
            RoundKills += 1;
            MultikillActive = true;
            Multikill();
            Spree();
            //SendCustomEventDelayedSeconds(nameof(StopMultikill), 4.0f);
            FirstKill();
        }

        public void StopMultikill()
        {
            MultikillInt = 0;
            MultikillActive = false;
        }

        public void Multikill()
        {
            if (MultikillActive)
            {
                MultikillImage.enabled = false;

                switch (MultikillInt)
                {
                    case 2:
                        MultikillObj.SetActive(false);
                        Announcer.PlayOneShot(k2);
                        MultikillImage.sprite = k2s;
                        MultikillObj.SetActive(true);
                        break;

                    case 3:
                        MultikillObj.SetActive(false);
                        Announcer.PlayOneShot(k3);
                        MultikillImage.sprite = k3s;
                        MultikillObj.SetActive(true);
                        break;

                    case 4:
                        MultikillObj.SetActive(false);
                        Announcer.PlayOneShot(k4);
                        MultikillImage.sprite = k4s;
                        MultikillObj.SetActive(true);
                        break;

                    case 5:
                        MultikillObj.SetActive(false);
                        Announcer.PlayOneShot(k5);
                        MultikillImage.sprite = k5s;
                        MultikillObj.SetActive(true);
                        break;

                    case 6:
                        MultikillObj.SetActive(false);
                        Announcer.PlayOneShot(k6);
                        MultikillImage.sprite = k6s;
                        MultikillObj.SetActive(true);
                        break;

                    case 7:
                        MultikillObj.SetActive(false);
                        Announcer.PlayOneShot(k7);
                        MultikillImage.sprite = k7s;
                        MultikillObj.SetActive(true);
                        break;

                    case 8:
                        MultikillObj.SetActive(false);
                        Announcer.PlayOneShot(k8);
                        MultikillImage.sprite = k8s;
                        MultikillObj.SetActive(true);
                        break;

                    case 9:
                        MultikillObj.SetActive(false);
                        Announcer.PlayOneShot(k9);
                        MultikillImage.sprite = k9s;
                        MultikillObj.SetActive(true);
                        break;

                    case 10:
                        MultikillObj.SetActive(false);
                        Announcer.PlayOneShot(k10);
                        MultikillImage.sprite = k10s;
                        MultikillObj.SetActive(true);
                        break;
                }

                MultikillImage.enabled = true;
            }
        }

        public void FirstKill()
        {
            if (RoundKills == 1)
            {
                SpecialObj.SetActive(false);
                Announcer.PlayOneShot(FirstStrike);
                SpecialImage.sprite = FirstStrikeSprite;
                SpecialObj.SetActive(true);
            }
        }

        public void RevengeKill()
        {
            //if (LastKilledBy = LastKill)
            //{
            //    Announcer.PlayOneShot(Revenge);
            //}
        }

        public void KilljoyKill()
        {
            //if ()
            //{
            //    Announcer.PlayOneShot(Killjoy);
            //}
        }
    }
}