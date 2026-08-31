using System;
using Unity.Networking.Transport;
using UnityEngine;

namespace MixedRealityArcade.CrossoverNetcode
{
    /// <summary>
    /// A Component that lets a game object establish a peer to peer connection with another NetworkPeerBehavior on a remote system.
    /// Once a connection is established, <see cref="CrossoverEvent"/> objects can be sent over it.
    /// </summary>
    public class NetworkPeerBehavior : MonoBehaviour
    {
        [field: SerializeField] public ushort listenPort { get; set; }
        [field: SerializeField] public string remoteAddress { get; set; }
        [field: SerializeField] public ushort remotePort { get; set; }

        /// <summary>
        /// If this peer acts as a server, this property is true.
        /// </summary>
        public bool isServer { get; private set; }

        private NetworkDriver _networkDriver;
        private NetworkConnection _networkConnection;

        public event Action<CrossoverEvent> CrossoverEventReceived;
        public event Action<ushort> BindingSuccessful;
        public event Action<ushort> BindingFailed;
        public event Action Disconnected;
        public event Action Connected;

        private void Awake()
        {
            isServer = true;
            
            _networkDriver = NetworkDriver.Create();
        }

        public void StartListenForConnections()
        {
            if (_networkConnection.IsCreated)
            {
                return;
            }
            
            var endPoint = NetworkEndpoint.AnyIpv4.WithPort(listenPort);

            if (_networkDriver.Bind(endPoint) != 0)
            {
                BindingFailed?.Invoke(listenPort);
                Debug.LogError($"Failed to listen at port {listenPort}");
                return;
            }

            if (_networkDriver.Listen() != 0)
            {
                BindingFailed?.Invoke(listenPort);
                Debug.LogError($"Failed to listen at port {listenPort}");
                return;
            }
            BindingSuccessful?.Invoke(listenPort);
            Debug.LogFormat("Bound to Port {0} and listening for connections", listenPort);
        }

        /// <summary>
        /// Tries to establish a connection with the host at the specified remote address.
        /// </summary>
        /// <returns>true, if a new connection was successfully created. false, if it was not created successfully, or a previous connection is already established.</returns>
        public bool TryConnect()
        {
            if (_networkConnection.IsCreated)
            {
                return true;
            }

            var endPoint = NetworkEndpoint.Parse(remoteAddress, remotePort);
            _networkConnection = _networkDriver.Connect(endPoint);

            return _networkConnection.IsCreated;
        }

        private void Update()
        {
            _networkDriver.ScheduleUpdate().Complete();

            if (!_networkConnection.IsCreated)
            {
                _networkConnection = default;
            }
            
            NetworkConnection newConnection;
            if (_networkConnection == default && (newConnection = _networkDriver.Accept()) != default)
            {
                _networkConnection = newConnection;
                isServer = true;
                Connected?.Invoke();
                Debug.LogFormat("Accepted connection: {0}\nNow acting as a server", _networkConnection.ToString());
            }

            NetworkEvent.Type cmd;
            while ((cmd = _networkConnection.PopEvent(_networkDriver, out var streamReader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Connected?.Invoke();
                    Debug.Log("Established connection to Server.\nNow acting as a client");
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Disconnected?.Invoke();
                    Debug.Log(isServer ? "Disconnected from client" : "Disconnected from server");
                    _networkConnection = default;
                    isServer = false;
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    CrossoverEventReceived?.Invoke(CrossoverEventFactory.CreateCrossoverEvent(streamReader));
                }
            }
        }

        /// <summary>
        /// Disconnects the currently established connection if it exists.
        /// </summary>
        public void Disconnect()
        {
            _networkDriver.Disconnect(_networkConnection);
            _networkConnection = default;
            isServer = false;
            Disconnected?.Invoke();
            Debug.Log(isServer ? "Disconnected from client" : "Disconnected from server");
        }

        /// <summary>
        /// Tries to send a CrossoverEvent over an established connection.
        /// </summary>
        /// <param name="crossoverEvent">the CrossoverEvent to be sent</param>
        /// <returns>true if the event was sent successfully, false otherwise</returns>
        public bool TrySendCrossoverEvent(CrossoverEvent crossoverEvent)
        {
            if (!_networkConnection.IsCreated)
            {
                return false;
            }

            _networkDriver.BeginSend(_networkConnection, out var writer);
            crossoverEvent.WriteToStream(ref writer);
            _networkDriver.EndSend(writer);

            return true;
        }
        
        private void OnDestroy()
        {
            _networkDriver.Dispose();
        }
    }
}
