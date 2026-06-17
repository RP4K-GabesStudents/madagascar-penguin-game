using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace Multiplayer.GameObjects
{
    /// <summary>
    /// Server-authoritative record of which player picked which character, keyed by
    /// the stable UGS PlayerId (from NetcodeSigninSequence) so a selection survives a
    /// reconnect even though the runtime clientId changes.
    ///
    /// Spawn one of these as a persistent NetworkObject (e.g. via
    /// SpawnNetworkObjectSequence) before SelectCharacterSequence runs. It lives on a
    /// DontDestroyOnLoad object so it outlives the additive selection scene.
    ///
    /// The Taken list is replicated to every peer and drives the selector visuals
    /// (greying out / "Selected by NAME").
    /// </summary>
    public class CharacterSelectionStore : NetworkBehaviour
    {
        public static CharacterSelectionStore Instance { get; private set; }

        public struct Taken : INetworkSerializable, System.IEquatable<Taken>
        {
            public ulong PrefabId;
            public FixedString64Bytes PlayerName;

            public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
            {
                s.SerializeValue(ref PrefabId);
                s.SerializeValue(ref PlayerName);
            }

            public bool Equals(Taken other) =>
                PrefabId == other.PrefabId && PlayerName.Equals(other.PlayerName);
        }

        // Replicated to all peers. Read by PenguinSelector to update visuals.
        private readonly NetworkList<Taken> _taken = new();
        public NetworkList<Taken> TakenList => _taken;

        // Server-only authoritative state, keyed by stable playerId.
        private readonly Dictionary<string, ulong> _selectionByPlayer = new();  // playerId -> prefabId
        private readonly Dictionary<ulong, string> _clientToPlayer = new();      // live clientId -> playerId
        private readonly Dictionary<string, string> _playerNames = new();        // playerId -> display name

        /// <summary>Count of distinct players that currently have a live connection AND a selection.</summary>
        public int LiveSelectionCount
        {
            get
            {
                int n = 0;
                foreach (var kvp in _clientToPlayer)
                    if (_selectionByPlayer.ContainsKey(kvp.Value)) n++;
                return n;
            }
        }

        /// <summary>clientId -> prefabId for every currently-connected player that has a pick.</summary>
        public IEnumerable<KeyValuePair<ulong, ulong>> ActiveSelections()
        {
            foreach (var kvp in _clientToPlayer)
                if (_selectionByPlayer.TryGetValue(kvp.Value, out ulong prefabId))
                    yield return new KeyValuePair<ulong, ulong>(kvp.Key, prefabId);
        }

        /// <summary>Resolve a live clientId to its stable playerId. Server only;
        /// returns false if the client was never registered.</summary>
        public bool TryGetPlayerId(ulong clientId, out string playerId)
            => _clientToPlayer.TryGetValue(clientId, out playerId);

        /// <summary>
        /// Server fallback: make sure every currently-connected client has a
        /// clientId -> playerId mapping, so auto-assign can target players that never
        /// picked. Uses the clientId string as the playerId when nothing better is
        /// known. Prefer calling RegisterClient with the real UGS PlayerId at signin;
        /// this only backfills anything that slipped through.
        /// </summary>
        public void RegisterFromConnected()
        {
            if (!IsServer) return;
            foreach (var clientId in NetworkManager.ConnectedClientsIds)
                if (!_clientToPlayer.ContainsKey(clientId))
                    _clientToPlayer[clientId] = clientId.ToString();
        }

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager != null)
                NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;

            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Link a (re)connecting client to its stable identity. A prior selection,
        /// if any, is already in _selectionByPlayer + _taken, so it re-inherits it.
        /// Server only.
        /// </summary>
        public void RegisterClient(ulong clientId, string playerId, string playerName)
        {
            if (!IsServer) return;
            _clientToPlayer[clientId] = playerId;
            if (!string.IsNullOrEmpty(playerName)) _playerNames[playerId] = playerName;
        }

        /// <summary>
        /// Attempt to claim a character. Returns false if another player already holds
        /// it. Re-selecting releases the player's previous pick first. Server only.
        /// </summary>
        public bool TrySelect(ulong clientId, string playerId, string playerName, ulong prefabId)
        {
            if (!IsServer) return false;
            if (string.IsNullOrEmpty(playerId)) return false;

            _clientToPlayer[clientId] = playerId;
            if (!string.IsNullOrEmpty(playerName)) _playerNames[playerId] = playerName;

            // Reject if a DIFFERENT player already holds this prefab.
            foreach (var kvp in _selectionByPlayer)
                if (kvp.Value == prefabId && kvp.Key != playerId)
                    return false;

            // Release this player's previous pick, if it changed.
            if (_selectionByPlayer.TryGetValue(playerId, out ulong prev) && prev != prefabId)
                RemoveTaken(prev);

            _selectionByPlayer[playerId] = prefabId;
            UpsertTaken(prefabId, _playerNames.TryGetValue(playerId, out var nm) ? nm : playerName);
            return true;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            // Keep the selection (keyed by playerId) so a reconnect re-inherits it.
            // Drop only the live clientId mapping.
            _clientToPlayer.Remove(clientId);
        }

        private void UpsertTaken(ulong prefabId, string name)
        {
            var entry = new Taken { PrefabId = prefabId, PlayerName = name ?? "Player" };
            for (int i = 0; i < _taken.Count; i++)
                if (_taken[i].PrefabId == prefabId) { _taken[i] = entry; return; }
            _taken.Add(entry);
        }

        private void RemoveTaken(ulong prefabId)
        {
            for (int i = 0; i < _taken.Count; i++)
                if (_taken[i].PrefabId == prefabId) { _taken.RemoveAt(i); return; }
        }
    }
}