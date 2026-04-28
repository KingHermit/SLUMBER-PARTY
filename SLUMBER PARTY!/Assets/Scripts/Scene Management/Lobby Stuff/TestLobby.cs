using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SLUMBER_PARTY.LobbyUtils
{
    public class TestLobby : MonoBehaviour
    {
        public static TestLobby Instance { get; private set; }

        public event Action OnLobbyUpdated;
        public event Action OnLobbiesListUpdated;

        [SerializeField] private bool isRefreshing;
        public List<Lobby> availableLobbies { get; private set; } = new(); 
        [SerializeField] private Lobby hostLobby;
        [SerializeField] private Lobby joinedLobby;
        [SerializeField] private float heartbeatTimer;
        [SerializeField] private float lobbyUpdateTimer;
        [SerializeField] private string playerName;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this; 
        }

        private async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            playerName = "Player" + UnityEngine.Random.Range(10, 99);
            Debug.Log(playerName);
        }

        private void Update()
        {
            HandleLobbyHeartbeat();
            HandleLobbyPollForUpdates();
        }

        private async void HandleLobbyHeartbeat()
        {
            if (hostLobby != null)
            {
                heartbeatTimer -= Time.deltaTime;
                if (heartbeatTimer <= 0f)
                {
                    float heartbeatTimerMax = 15;
                    heartbeatTimer = heartbeatTimerMax;

                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                }
            }
        }

        private async void HandleLobbyPollForUpdates()
        {
            if (joinedLobby != null)
            {
                lobbyUpdateTimer -= Time.deltaTime;
                if (lobbyUpdateTimer > 0f) return;

                lobbyUpdateTimer = 1.1f;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                SetLobby(lobby);

                if (NetworkManager.Singleton.IsClient &&
                    joinedLobby.Data.TryGetValue("GameState", out var gameState))
                {
                    if (!IsLobbyHost() && gameState.Value == "Starting")
                    {
                        Debug.Log("Oh yeah we're starting by the way GET YOUR ASS IN HERE");
                        JoinGameSession(); // other client not joining...!?
                    }
                }
            }
        }

        private void SetLobby(Lobby lobby)
        {
            joinedLobby = lobby;
            OnLobbyUpdated?.Invoke();
        }

        public async void RefreshLobbyList()
        {
            if (isRefreshing) return;

            Debug.Log("Refreshing lobby list...");

            isRefreshing = true;

            availableLobbies = await ListLobbiesAsync();

            isRefreshing = false;

            OnLobbiesListUpdated?.Invoke();
        }

        public async void CreateLobby(string name)
        {
            try
            {
                string lobbyName = name;
                int maxPlayers = 4;

                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = GetPlayer(),
                    Data = new Dictionary<string, DataObject>
                {
                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, "Pillow Fort") }
                }
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

                hostLobby = lobby;
                SetLobby(lobby);

                OnLobbiesListUpdated?.Invoke();

                Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
                PrintPlayers(hostLobby);

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        public async Task<List<Lobby>> ListLobbiesAsync()
        {
            try
            {
                QueryLobbiesOptions lobbyOptions = new QueryLobbiesOptions
                {
                    Count = 10,
                    Filters = new List<QueryFilter>{
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                    Order = new List<QueryOrder>{
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
                };


                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(lobbyOptions);

                Debug.Log("Lobbies found: " + response.Results.Count);
                foreach (Lobby lobby in response.Results)
                {
                    Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
                }

                return response.Results;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
                return null;
            }
        }

        public async void JoinLobbyByCode(string lobbyCode)
        {
            try
            {
                JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
                {
                    Player = GetPlayer()
                };

                Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
                SetLobby(lobby);

                Debug.Log("Joined lobby with code: " + lobbyCode);

                PrintPlayers(lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        public async void QuickJoinLobby()
        {
            try
            {
                await LobbyService.Instance.QuickJoinLobbyAsync();
                OnLobbyUpdated?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        public Lobby GetJoinedLobby()
        {
            return joinedLobby;
        }

        private bool IsLobbyHost()
        {
            return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
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

        public async void StartGame()
        {
            if (!IsLobbyHost()) return;

            // Step 1: create session (or relay allocation if using Unity Relay)
            // Step 2: Start NGO
            // Step 3: now RPCs + network scene load WORK
            //NetworkManager.SceneManager.LoadScene("CharacterSelect", LoadSceneMode.Single);

            Debug.Log("The game is starting! Going to character select screen...");

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameState", new DataObject(DataObject.VisibilityOptions.Public, "Starting") }
                }
            });

            NetworkManager.Singleton.StartHost();
        }

        public void JoinGameSession()
        {
            if (NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Network session is already running.");
                return;
            }

            NetworkManager.Singleton.StartClient();
        }

        private void PrintPlayers()
        {
            PrintPlayers(joinedLobby);
        }

        private void PrintPlayers(Lobby lobby)
        {
            Debug.Log($"Players in lobby {lobby.Name}:");

            foreach (Player player in lobby.Players)
            {
                Debug.Log($"{player.Id} | {player.Data["PlayerName"].Value} | {lobby.Data["Map"].Value}");
            }
        }

        // write update lobby method later

        private async void LeaveLobby()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                OnLobbyUpdated?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        private async void KickPlayer()
        {
            try
            {
                // kicks second player as test
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        private async void MigrateLobbyHost()
        {
            try
            {
                hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
                {
                    HostId = joinedLobby.Players[1].Id
                });
                joinedLobby = hostLobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        private async void DeleteLobby()
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}

