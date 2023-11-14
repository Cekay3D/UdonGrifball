using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace Cekay.Grifball
{
    public class Goal : UdonSharpBehaviour
    {
        public Combat CombatScript;

        [SerializeField] private TextMeshProUGUI RedPointsDisplay;
        public GameObject RedExplosion;

        [SerializeField] private TextMeshProUGUI BluePointsDisplay;
        public GameObject BlueExplosion;

        public void OnTriggerEnter(Collider other)
        {
            // Red score
            if (other.gameObject.layer == CombatScript.RedBombLayer && gameObject.layer == CombatScript.BlueGoalLayer)
            {
                CombatScript.IsPaused = true;

                BlueExplosion.SetActive(true);

                CombatScript.BlueGoal.enabled = false;
                CombatScript.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(CombatScript.GoalGet));
                CombatScript.RedPoints += 1;
                RedPointsDisplay.text = CombatScript.RedPoints.ToString();

                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(WaitResetBlue));
                CombatScript.BombPickup.Drop();
            }
            // Blue score
            else if (other.gameObject.layer == CombatScript.BlueBombLayer && gameObject.layer == CombatScript.RedGoalLayer)
            {
                CombatScript.IsPaused = true;

                RedExplosion.SetActive(true);

                CombatScript.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(CombatScript.GoalGet));

                CombatScript.RedGoal.enabled = false;
                CombatScript.BluePoints += 1;
                BluePointsDisplay.text = CombatScript.BluePoints.ToString();

                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(WaitResetRed));
                CombatScript.BombPickup.Drop();
            }
        }

        public void WaitResetRed()
        {
            SendCustomEventDelayedSeconds(nameof(ResetRed), 5.0f);
        }

        public void WaitResetBlue()
        {
            SendCustomEventDelayedSeconds(nameof(ResetBlue), 5.0f);
        }

        public void ResetBlue()
        {
            BlueExplosion.SetActive(false);
            CombatScript.BlueGoal.enabled = true;
        }

        public void ResetRed()
        {
            RedExplosion.SetActive(false);
            CombatScript.RedGoal.enabled = true;
        }
    }
}