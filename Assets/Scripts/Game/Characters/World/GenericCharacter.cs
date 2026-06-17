using System;
using System.Collections.Generic;
using Game.Characters.Stats;
using Game.Objects;
using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Characters.World
{
    /// <summary>
    /// A Generic Character is like a husk; it describes how a character might
    /// work, but not who controls it, or how. The Generic Character should not
    /// query outside scripts.
    ///
    /// NETWORKED-DAMAGE WORKED EXAMPLE. This is the template to propagate to the
    /// other IDamageable implementers that need others to see their damage:
    ///   1. Health is a NetworkVariable&lt;float&gt; (server-write), so a hit
    ///      applied on the server replicates to everyone.
    ///   2. The IDamageable.Health property reads/writes that NetworkVariable.
    ///   3. TakeDamageRpc is the one required per-class stub (see IDamageable):
    ///      [Rpc(SendTo.Server)] -> runs the shared TakeDamageLocal on the server.
    ///   4. OnHurt fans its visible reaction (the knockback shove) out to all
    ///      peers via an Rpc, since physics applied only on the server wouldn't
    ///      be felt by the owner's local rigidbody otherwise.
    /// A lightweight damageable (e.g. Potion) that just dies + despawns needs
    /// only step 3; despawn already replicates on its own.
    /// </summary>
    [SelectionBase, RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class GenericCharacter : NetworkBehaviour, IDamageable, IInputSubscriber
    {
        [SerializeField] private Transform head;
        [SerializeField] protected CharacterStats stats;
        private GameControls _gameControls;

        private readonly Dictionary<uint, int> _data = new();

        protected float _currentResistance = 0;

        // Server-authoritative health. Initialised on spawn (server) from stats.
        private readonly NetworkVariable<float> _health = new();

        public Rigidbody rigidbody { get; private set; }
        public Animator animator { get; private set; }
        public Transform Head => head;

        // IDamageable.Health now reads/writes the replicated value. The setter
        // keeps your original semantics: 'value' is a delta (heal positive,
        // damage negative arrives via Health -= damage), clamped to stats.Hp,
        // fires OnHealthUpdated with the actual change, and dies at <= 0.
        public float Health
        {
            get => _health.Value;
            set
            {
                if (!IsServer) return; // only the server mutates authoritative health

                float t = Mathf.Min(value, stats.Hp); // 'value' is the new absolute health
                if (!Mathf.Approximately(t, _health.Value))
                {
                    float diff = t - _health.Value;
                    _health.Value = t;
                    OnHealthUpdated_Rpc(diff);
                }

                if (_health.Value <= 0) Die(Vector3.zero);
            }
        }

        public float DamageRes => Mathf.Min(0.9f, stats.BaseResistance + _currentResistance);
        public float HealthPercent => Health / stats.Hp;

        public int GetDataDictionaryValue(uint key) => _data[key];
        public void SetDataDictionaryValue(uint key, int value) => _data[key] = value;
        public bool TryAddDataKey(uint key, int initialValue) => _data.TryAdd(key, initialValue);

        #region Actions

        public event Action<float> OnHealthUpdated;
        public event Action OnDeath;
        public event Action OnAttack;

        #endregion

        private void Awake()
        {
            InitializeComponents();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer) _health.Value = stats.Hp;
            _health.OnValueChanged += (_, _) => { }; // hook here if UI wants raw value changes
        }

        protected virtual void InitializeComponents()
        {
            rigidbody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
        }

        #region Damageable

        // The one required per-class stub. Verbatim across implementers; lives
        // here (not the interface) because [Rpc] needs a concrete NetworkBehaviour.
        [Rpc(SendTo.Server)]
        public void TakeDamageRpc(float damage, Vector3 force) =>
            ((IDamageable)this).TakeDamageLocal(damage, force);

        // Runs on the server (via TakeDamageLocal). The visible reaction has to
        // reach everyone, so fan the knockback out rather than shoving only the
        // server's copy of the rigidbody.
        public virtual void OnHurt(float amount, Vector3 force)
        {
            OnHurt_Rpc(amount, force);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void OnHurt_Rpc(float amount, Vector3 force)
        {
            OnHealthUpdated?.Invoke(amount);
            rigidbody.AddForce(force, ForceMode.Impulse);
            Debug.LogWarning($"{name} was hit for {amount}, play on-hit animation", gameObject);
        }

        public virtual void Die(Vector3 force)
        {
            Die_Rpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Die_Rpc()
        {
            Debug.Log($"{name} Died", gameObject);
            OnDeath?.Invoke();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void OnHealthUpdated_Rpc(float diff)
        {
            OnHealthUpdated?.Invoke(diff);
        }

        #endregion

        public void BindControls(GameControls controls)
        {
            _gameControls = controls;
            _gameControls.Player.Attack.performed += Attack;
        }

        private void Attack(InputAction.CallbackContext obj)
        {
            animator.SetBool(StaticUtilities.AttackAnimID, obj.ReadValueAsButton());
        }

        public override void OnDestroy()
        {
            if (_gameControls != null) _gameControls.Player.Attack.performed -= Attack;
            base.OnDestroy();
        }

        public void ExecuteAttack()
        {
            OnAttack?.Invoke();
        }
    }
}