using MixedRealityArcade.ArcadeXR.Network;
using UnityEngine;

public class XRIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetworkManager.Instance.Connected += OnConnect;
    }

    // Update is called once per frame
    void OnConnect()
    {
        this.gameObject.SetActive(false);
    }
}
