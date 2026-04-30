using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Multiplayer.Widgets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Collections.Specialized.BitVector32;

namespace SLUMBER_PARTY.LobbyUtils
{
    public class TestSessionManager : MonoBehaviour
    {
        public static TestSessionManager Instance { get; private set; }

        public event Action OnSessionUpdated;
        public event Action OnSessionsListUpdated;

        [SerializeField] private bool isRefreshing;
        bool hasStartedClient = false;
        public IList<ISessionInfo> availableSessions { get; private set; } = new List<ISessionInfo>(); 
        [SerializeField] private ISession hostSession;
        [SerializeField] private ISession joinedSession;
        [SerializeField] private float heartbeatTimer;
        [SerializeField] private float lobbyUpdateTimer;
        [SerializeField] private string playerName;
        public string currentSessionCode { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this; 
        }

        #region SIGN IN
        private async void Start()
        {
            var options = new InitializationOptions();
            playerName = "Player" + UnityEngine.Random.Range(10, 99);

            options.SetProfile(playerName);

            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Profile {playerName} Signed in with ID: {AuthenticationService.Instance.PlayerId}");
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
        }

        private string GetVirtualPlayerName()
        {
            // Default for the main editor instance
            string name = "PrimaryPlayer";

            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                // MPPM uses "-name" for the instance identifier
                if (args[i] == "-myProfile" && i + 1 < args.Length)
                {
                    name = args[i + 1];
                    break;
                }
            }
            return name;
        }
        #endregion SIGN IN

        private void Update()
        {
            //HandleLobbyHeartbeat();
            //HandleLobbyPollForUpdates();
        }

        private async void HandleLobbyHeartbeat()
        {
            if (hostSession != null)
            {
                heartbeatTimer -= Time.deltaTime;
                if (heartbeatTimer <= 0f)
                {
                    float heartbeatTimerMax = 3f;
                    heartbeatTimer = heartbeatTimerMax;

                    await LobbyService.Instance.SendHeartbeatPingAsync(hostSession.Id);
                }
            }
        }

        private async void HandleLobbyPollForUpdates()
        {
            //if (joinedSession == null) return;
            //if (IsSessionHost()) return;

            //lobbyUpdateTimer -= Time.deltaTime;
            //if (lobbyUpdateTimer > 0f) return;
            //lobbyUpdateTimer = 1.5f;

            //Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedSession.Id);
            //SetSession(lobby);

            //if (joinedSession.Data.TryGetValue("GameState", out var gameState) && gameState.Value == "Starting")
            //{
            //    if (!hasStartedClient) // only clients
            //    {
            //        hasStartedClient = true;
            //        Debug.Log("Client detected start state. Joining Relay...");
            //        await JoinGameSession(joinedSession.Data["RelayJoinCode"].Value);
            //    }
            //}
        }

        private async void SetSession(ISession sesh)
        {
            joinedSession = sesh;
            await joinedSession.RefreshAsync();
            OnSessionUpdated?.Invoke();
        }

        public async void RefreshSessionList()
        {
            if (isRefreshing) return;

            Debug.Log("Refreshing lobby list...");

            isRefreshing = true;

            availableSessions = await ListSessionsAsync();

            isRefreshing = false;

            OnSessionsListUpdated?.Invoke();
        }

        //public async void CreateLobby(string name)
        //{
        //    try
        //    {
        //        string lobbyName = name;
        //        int maxPlayers = 4;

        //        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
        //        {
        //            IsPrivate = false,
        //            Player = GetPlayer(),
        //            Data = new Dictionary<string, DataObject>
        //            {
        //                { "Map", new DataObject(DataObject.VisibilityOptions.Public, "Pillow Fort") },
        //                { "GameState", new DataObject(DataObject.VisibilityOptions.Public, "Waiting") },
        //            }
        //        };

        //        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);

        //        hostSession = lobby;
        //        SetSession(lobby);

        //        OnSessionsListUpdated?.Invoke();

        //        //Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
        //        PrintPlayers(hostSession);

        //    }
        //    catch (RelayServiceException e)
        //    {
        //        Debug.Log(e.Message);
        //    }

        //}

        public async Task<IList<ISessionInfo>> ListSessionsAsync()
        {
            #region OLD LOBBY CRAP
            //try
            //{
            //    QueryLobbiesOptions lobbyOptions = new QueryLobbiesOptions
            //    {
            //        Count = 10,
            //        Filters = new List<QueryFilter>{
            //        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            //    },
            //        Order = new List<QueryOrder>{
            //        new QueryOrder(false, QueryOrder.FieldOptions.Created)
            //    }
            //    };


            //    QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(lobbyOptions);

            //    Debug.Log("Lobbies found: " + response.Results.Count);
            //    foreach (Lobby lobby in response.Results)
            //    {
            //        Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            //    }

            //    return response.Results;
            //}
            //catch (LobbyServiceException e)
            //{
            //    Debug.Log(e.Message);
            //    return null;
            //}
            #endregion OLD LOBBY CRAP

            try
            {
                QuerySessionsOptions options = new QuerySessionsOptions
                {
                    Count = 4,
                };
                
                QuerySessionsResults response = await MultiplayerService.Instance.QuerySessionsAsync(options);
                OnSessionsListUpdated?.Invoke();

                return response.Sessions;

            } catch (Exception e)
            {
                Debug.Log(e.Message);
                return null;
            }
        }

        public async void JoinSessionByCode(string lobbyCode)
        {
            #region OLD LOBBY CRAP
            //try
            //{
            //    JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            //    {
            //        Player = GetPlayer()
            //    };

            //    Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            //    SetSession(lobby);

            //    Debug.Log($"Joined {joinedSession.Name} with code: {lobbyCode}");

            //    PrintPlayers(lobby);
            //}
            //catch (LobbyServiceException e)
            //{
            //    Debug.Log(e.Message);
            //} 
            #endregion OLD LOBBY CRAP
        }

        public async void QuickJoinLobby()
        {
            try
            {
                await LobbyService.Instance.QuickJoinLobbyAsync();
                OnSessionUpdated?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        public ISession GetJoinedSession()
        {
            return joinedSession;
        }

        public bool IsSessionHost()
        {
            return joinedSession != null && joinedSession.IsHost;
        }

        private Player GetPlayer()
        {
            return new Player
            {
                Data = new Dictionary<string, PlayerDataObject> {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
             }
            };
        }

        public async void CreateSession(string name)
        {
            if (IsSessionHost()) return;

            #region OLD RELAY CODE

            #region SET RELAY ALLOCATION & CONFIGURE
            //Allocation allocation = await RelayService.Instance.CreateAllocationAsync(hostSession.MaxPlayers);
            //string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            //var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            //transport.SetRelayServerData(
            //    allocation.RelayServer.IpV4,
            //    (ushort)allocation.RelayServer.Port,
            //    allocation.AllocationIdBytes,
            //    allocation.Key,
            //    allocation.ConnectionData
            //);

            ////NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "udp"));
            //NetworkManager.Singleton.StartHost();

            #endregion SET RELAY ALLOCATION & CONFIGURE

            #region THE LOBBY IS STARTING

            //var lobbyOptions = new UpdateLobbyOptions
            //{
            //    Data = new Dictionary<string, DataObject>
            //    {
            //        { "GameState", new DataObject(
            //                visibility: DataObject.VisibilityOptions.Public,
            //                value: "Starting"
            //            )
            //        },

            //        { "RelayJoinCode", new DataObject(
            //                visibility: DataObject.VisibilityOptions.Public,
            //                value:relayJoinCode)
            //            }
            //    }
            //};

            //await LobbyService.Instance.UpdateLobbyAsync(hostSession.Id, lobbyOptions);

            #endregion THE LOBBY IS STARTING
            #endregion OLD RELAY CODE

            //handles Auth, Lobby creation, and Relay allocation
            var options = new SessionOptions
            {
                Name = name,
                MaxPlayers = 4,
                IsPrivate = false,
                SessionProperties = new Dictionary<string, SessionProperty>
                {
                    { "Map", new SessionProperty("Pillow Fort", VisibilityPropertyOptions.Public) },
                    { "SessionJoinCode", new SessionProperty("", VisibilityPropertyOptions.Public) }
                }
            }
            .WithPlayerName(VisibilityPropertyOptions.Public);                             

            var session = await MultiplayerService.Instance.CreateSessionAsync(options);

            session.AsHost().SetProperty("SessionJoinCode", new SessionProperty(session.Code));
            await session.AsHost().SavePropertiesAsync();
            hostSession = session;
            SetSession(session);

            PrintPlayers();
        }


        public async void StartGame()
        {
            try
            {
                if (!IsSessionHost()) return;

                var hostInterface = hostSession.AsHost();
                if (hostInterface == null) { return; }

                Debug.Log("Starting Relay Network...");

                await hostInterface.Network.StartRelayNetworkAsync(new RelayNetworkOptions { });

                if (NetworkManager.Singleton.IsServer)
                {

                    while (NetworkManager.Singleton.ConnectedClients.Count < hostSession.PlayerCount - 1)
                    {
                        Debug.Log($"{NetworkManager.Singleton.ConnectedClients.Count} / {hostSession.PlayerCount} clients connected...");
                        await Task.Delay(100);
                    }

                    bool allClientsReady = false;
                    while (!allClientsReady)
                    {
                        allClientsReady = true;
                        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClients.Values)
                        {
                            if (!client.PlayerObject.IsSpawned)
                            {
                                allClientsReady = false;
                                break;
                            }
                        }

                        if (!allClientsReady)
                        {
                            await Task.Delay(1000);
                        }
                    }

                    Debug.Log("Clients ready! Moving to Game Scene...");
                    NetworkManager.Singleton.SceneManager.LoadScene(SceneID.CharacterSelect.ToString(), LoadSceneMode.Single); // not going to scene...?
                }
            }
            catch (Exception e)
            {

                Debug.Log(e.Message);
            }
        }


        public async void JoinGameSession(string sessionCode)
        {
            #region OLD RELAY CODE
            //try
            //{
            //    var nm = NetworkManager.Singleton;

            //    if (nm.IsListening || nm.IsConnectedClient || nm.IsHost)
            //    {
            //        nm.Shutdown();
            //        // Wait until it's actually not listening anymore
            //        while (nm.ShutdownInProgress) await Task.Yield();
            //    }

            //    #region SET RELAY ALLOCATION & CONFIGURE
            //    JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

            //    var transport = nm.GetComponent<UnityTransport>();

            //    transport.SetRelayServerData(
            //        joinAllocation.RelayServer.IpV4,
            //        0,
            //        joinAllocation.AllocationIdBytes,
            //        joinAllocation.Key,
            //        joinAllocation.ConnectionData,
            //        joinAllocation.HostConnectionData,
            //        false
            //    );

            //    //transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "udp")); // causing error...
            //    #endregion SET RELAY ALLOCATION & CONFIGURE

            //    if (!nm.StartClient())
            //    {
            //        Debug.LogError("Failed to start NetworkManager Client.");
            //    }
            //}
            //catch (Exception e)
            //{
            //    Debug.Log($"Join error: {e.Message}");
            //}
            #endregion OLD RELAY CODE

            // handles finding the lobby, joining the relay, and starting the client
            var session = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
            //await session.SaveCurrentPlayerDataAsync();

            SetSession(session);
            PrintPlayers();
        }

        private void PrintPlayers()
        {
            PrintPlayers(joinedSession);
        }

        private void PrintPlayers(ISession session)
        {
            Debug.Log($"Players in lobby {session.Name}:");

            foreach (IPlayer player in session.Players)
            {
                Debug.Log($"{player.Id} | {player.GetPlayerName()} | {session.Properties["Map"].Value}");
            }
        }

        // write update lobby method later

        private async void LeaveSession()
        {
            try
            {
                if (joinedSession == null) return;

                await MultiplayerService.Instance.GetJoinedSessionIdsAsync();

                if (IsSessionHost()) // if the host leaves
                {
                    if (joinedSession.Players.Count > 1) // if there's still players in the lobby
                    {
                        //MigrateLobbyHost();
                        joinedSession = null;
                        OnSessionUpdated?.Invoke();
                        return;
                    }

                    //DeleteSession(); // delete the empty lobby
                    joinedSession = null;
                    return;
                }

                string playerId = AuthenticationService.Instance.PlayerId;
                await LobbyService.Instance.RemovePlayerAsync(joinedSession.Id, playerId);
                OnSessionUpdated?.Invoke();
                joinedSession = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        public async void Leave()
        {
            LeaveSession();
        }

        private async void KickPlayer()
        {
            try
            {
                // kicks second player as test
                await LobbyService.Instance.RemovePlayerAsync(joinedSession.Id, joinedSession.Players[1].Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        //private async void MigrateLobbyHost()
        //{
        //    try
        //    {
        //        hostSession = await MultiplayerService.Instance.(hostSession.Id, new UpdateLobbyOptions
        //        {
        //            HostId = joinedSession.Players[1].Id
        //        });
        //        joinedSession = hostSession;
        //    }
        //    catch (LobbyServiceException e)
        //    {
        //        Debug.Log(e.Message);
        //    }
        //}

        private async void DeleteSession()
        {
            try
            {
                //await MultiplayerService.Instance.
                //OnSessionsListUpdated?.Invoke();
                //joinedSession = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}

