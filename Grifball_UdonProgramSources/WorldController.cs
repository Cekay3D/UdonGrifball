using UdonSharp;
using UnityEngine;

namespace Cekay.Grifball
{
    public class WorldController : UdonSharpBehaviour
    {
        [SerializeField] private AudioClip Join;
        [SerializeField] private AudioClip Leave;

        [SerializeField] private SettingsPage Settings;

        //public void OnPlayerJoined()
        //{
        //    Settings.InterfaceAudio.PlayOneShot(Join);
        //}

        //public void OnPlayerLeft()
        //{
        //    Settings.InterfaceAudio.PlayOneShot(Leave);
        //}
    }
}