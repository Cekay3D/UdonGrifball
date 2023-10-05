using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace Cekay.Grifball
{
    public class Goal : UdonSharpBehaviour
    {
        [SerializeField] private Combat CombatScript;

        [SerializeField] private TextMeshProUGUI RedPointsDisplay;
        public GameObject RedExplosion;

        [SerializeField] private TextMeshProUGUI BluePointsDisplay;
        public GameObject BlueExplosion;

        public void OnTriggerEnter(Collider other)
        {
            // Red score
            if (other.gameObject.layer == CombatScript.RedBombLayer && gameObject.layer == CombatScript.BlueGoalLayer)
            {
                BlueExplosion.SetActive(true);

                CombatScript.BlueGoal.enabled = false;
                CombatScript.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Goal));
                CombatScript.RedPoints += 1;
                RedPointsDisplay.text = CombatScript.RedPoints.ToString();

                SendCustomEventDelayedSeconds(nameof(ResetBlue), 6.0f);
            }
            // Blue score
            else if (other.gameObject.layer == CombatScript.BlueBombLayer && gameObject.layer == CombatScript.RedGoalLayer)
            {
                RedExplosion.SetActive(true);

                CombatScript.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Goal));

                CombatScript.RedGoal.enabled = false;
                CombatScript.BluePoints += 1;
                BluePointsDisplay.text = CombatScript.BluePoints.ToString();

                SendCustomEventDelayedSeconds(nameof(ResetRed), 6.0f);
            }
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