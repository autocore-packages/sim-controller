using Assets.Scripts.simai;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.simController
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class PedestrainController : PedestrianObj
    {
        private Animator pedAnimator;
        private NavMeshAgent agent;
        private void OnEnable()
        {
            agent = GetComponent<NavMeshAgent>();
            pedAnimator = GetComponent<Animator>();
        }
        protected override void Start()
        {
            base.Start(); 
            PedInit();
        }
        public void PedInit()
        {
            SetPedstrianAim();
        }
        private void Update()
        {
            if (!isReachTarget && RemainDistance < 0.2)
            {
                OnReachTarget();
            }
            pedAnimator.SetFloat("Forward", agent.speed);
        }

        public override void SetPedstrianAim()
        {
            isReachTarget = false;
            agent.SetDestination(AimPos);
            agent.speed = speedObjTarget;
        }

        public override void SetPedstrianStop()
        {
            base.SetPedstrianStop();
            agent.speed = 0;
        }
    }
}