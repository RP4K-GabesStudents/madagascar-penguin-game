using System;
using System.Collections.Generic;
using System.Linq;
using AI.GOAP.Sensor;
using Game.Environment;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace AI.GOAP.Agent
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AnimationController))]
    public class GoapAgent : MonoBehaviour
    {
        [Header("Sensors")] 
        [SerializeField] private Sensors chaseSensor;
        [SerializeField] private Sensors attackSensor;

        [Header("Known Locations")] 
        //random positions on map that agent knows about
        [SerializeField] private Transform restingPosition;
        [SerializeField] private Transform target1;
        [SerializeField] private Transform target2;
        [SerializeField] private Transform target3;
        
        private NavMeshAgent _agent;
        private AnimationController _animController;
        private Rigidbody _rigidbody;
        private LocationStats _locationstats;
        private IGoapPlanner _goapPlanner;
        private ActionPlan _actionPlan;
        
        
        [Header("Stats")]
        [SerializeField] private GoapAgentStats stats;
        private float _curHealth;
        private float _curStamina;
        
        private CountdownTimer _statTimer;

        private GameObject _objective;
        private Vector3 _destination;

        private Goals _lastGoal;
        private Goals _curGoal;
        private Actions _curAction;

        private Dictionary<string, AgentBelief> _beliefs;
        public HashSet<Actions> Actions {get; private set; }
        private HashSet<Goals> _goals;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animController = GetComponent<AnimationController>();
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.freezeRotation = true;
            
            _goapPlanner = new GoapPlanner();
        }

        private void Start()
        {
            _curHealth = stats.MaxHealth;
            _curStamina = stats.MaxStamina;
            SetupTimers();
            SetupBeliefs();
            SetupActions();
            SetupGoals();
        }

        private void Update()
        {
            _statTimer.Tick(Time.deltaTime);
            _animController.SetSpeed(_agent.velocity.magnitude);

            if (_curAction == null)
            {
                Debug.Log("Calculating New Plan");
                CalculatePlan();
            }

            if (_actionPlan != null && _actionPlan.Actions.Count > 0)
            {
                _agent.ResetPath();

                _curGoal = _actionPlan.Goal;
                Debug.Log($"Goal: {_curGoal.name} with {_actionPlan.Actions.Count} actions in plan");
                _curAction = _actionPlan.Actions.Pop();
                Debug.Log($"Popped action: {_curAction.Name}");
                _curAction.Start();
            }

            if (_actionPlan != null && _curAction != null)
            {
                _curAction.Update(Time.deltaTime);
                if (_curAction.Complete)
                {
                    Debug.Log($"{_curAction.Name} complete");
                    _curAction.Stop();

                    if (_actionPlan.Actions.Count == 0)
                    {
                        Debug.Log("Plan complete");
                        _lastGoal = _curGoal;
                        _curGoal = null;
                        _curAction = null;
                    }
                }
            }
        }

        private void CalculatePlan()
        {
            var priorityLevel = _curGoal?.priority ?? 0;

            HashSet<Goals> goalsToCheck = _goals;

            if (_curGoal != null)
            {
                Debug.Log("Current goal exists, checking goal with higher priority");
                goalsToCheck = new HashSet<Goals>(_goals.Where(g => g.priority > priorityLevel));
            }
            var potentialPlan = _goapPlanner.Plan(this, goalsToCheck, _lastGoal);
            if (potentialPlan != null)
            {
                _actionPlan = potentialPlan;
            }
        }

        private void SetupBeliefs()
        {
            _beliefs = new Dictionary<string, AgentBelief>();
            BeliefFactory factory = new BeliefFactory(this, _beliefs);
            
            factory.AddBelief("Nothing", () => false);
            factory.AddBelief("AgentIdle", () => !_agent.hasPath);
            factory.AddBelief("AgentMoving", () => _agent.hasPath);
            factory.AddBelief("AgentLowHealth", () => _curHealth < 30);
            factory.AddBelief("AgentHealthy", () => _curHealth >= 30);
            factory.AddBelief("AgentLowStamina", () => _curStamina < 10);
            factory.AddBelief("AgentIsRested", () => _curStamina >= 10);
            
            factory.AddLocationBelief("AtLocation1", _locationstats.Radius, restingPosition);
            factory.AddLocationBelief("AtLocation2", _locationstats.Radius, target1);
            factory.AddLocationBelief("AtLocation3", _locationstats.Radius, target2);
            factory.AddLocationBelief("AtLocation4", _locationstats.Radius, target2);
            
            factory.AddSensorBelief("PlayerInChaseRange", chaseSensor);
            factory.AddSensorBelief("PlayerInAttackRange", attackSensor);
            factory.AddBelief("AttackingPlayer", () => false); //player can always be attacked, will never be true
        }

        private void SetupActions()
        {
            Actions = new HashSet<Actions>();

            Actions.Add(new Actions.Builder("Relax").WithStrategy(new IdleStrategy(stats.IdleTime)).AddEffect(_beliefs["Nothing"]).Build());
            Actions.Add(new Actions.Builder("Wandering").WithStrategy(new WanderStrategy(_agent, stats.WanderRadius)).AddEffect(_beliefs["AgentMoving"]).Build());
            Actions.Add(new Actions.Builder("MoveToRestingPosition").WithStrategy(new MoveStrategy(_agent, () => restingPosition.position)).AddEffect(_beliefs["AtLocation1"]).Build());
            Actions.Add(new Actions.Builder("Rest").WithStrategy(new IdleStrategy(stats.IdleTime)).AddPrecondition(_beliefs["AtLocation1"]).AddEffect(_beliefs["AgentHealthy"]).Build());
            Actions.Add(new Actions.Builder("MoveFromTarget2ToTarget1").WithStrategy(new MoveStrategy(_agent, () => target2.position)).AddPrecondition(_beliefs["AtLocation2"]).AddEffect(_beliefs["AtLocation1"]).Build());
            Actions.Add(new Actions.Builder("MoveFromTarget3ToTarget1").WithStrategy(new MoveStrategy(_agent, () => target3.position)).WithCost(2).AddPrecondition(_beliefs["AtLocation3"]).AddEffect(_beliefs["AtLocation1"]).Build());
        } 

        private void SetupGoals()
        {
            _goals = new HashSet<Goals>();

            _goals.Add(new Goals.Builder("Chill Out").WithPriority(1).WithDesiredEffects(_beliefs["Nothing"]).Build());
            _goals.Add(new Goals.Builder("Wandering").WithPriority(1).WithDesiredEffects(_beliefs["AgentMoving"]).Build());
            _goals.Add(new Goals.Builder("KeepHealthUp").WithPriority(2).WithDesiredEffects(_beliefs["AgentHealthy"]).Build());
        }

        private void SetupTimers()
        {
            _statTimer = new CountdownTimer(stats.Time);
            _statTimer.OnTimerStop += () =>
            {
                UpdateStats();
                _statTimer.Start();
            };
            _statTimer.Start();
        }

        private void UpdateStats()
        {
            _curHealth += InRangeOf(restingPosition.position, _locationstats.Radius) ? _locationstats.HealAmount : -10;
            _curStamina += InRangeOf(restingPosition.position, _locationstats.Radius) ? _locationstats.RechargeAmount : -10;
            _curStamina = Mathf.Clamp(_curStamina, 0, stats.MaxStamina);
        }

        private bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) <= range;
        
        private void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;
        private void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;
        
        private void HandleTargetChanged()
        {
            Debug.Log("Target Changed, clearing current action and goals.");
            _curAction = null;
            _curGoal = null;
        }
    }
}
