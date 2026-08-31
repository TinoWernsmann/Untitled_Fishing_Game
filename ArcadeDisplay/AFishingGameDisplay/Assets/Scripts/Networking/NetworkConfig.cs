using UnityEngine;

[System.Serializable]
public class NetworkConfig
{
  //HMD Adress
    public string targetAddress;
    //Arcade Port
      public ushort listenPort;
      //HMD Port
    public ushort remotePort;
    
}
