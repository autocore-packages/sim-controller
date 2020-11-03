using Assets.Scripts.simai;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.simController
{
    public class PedestrainController : PedestrianObj
    {
        private Animator pedAnimator;
        Animator PedAnimator
        {
            get
            {
                if (pedAnimator == null) pedAnimator = GetComponent<Animator>();
                if (pedAnimator == null) Debug.LogError("Please set Animator");
                return pedAnimator;
            }
        }
        private NavMeshAgent agent;
        NavMeshAgent Agent
        {
            get
            {
                if (agent == null) agent = GetComponent<NavMeshAgent>();
                if (agent == null) Debug.LogError("Please set NavMeshAgent");
                return agent;
            }
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
            PedAnimator.SetFloat("Forward", Agent.speed);
        }

        public override void SetPedstrianAim()
        {
            isReachTarget = false;
            Agent.SetDestination(AimPos);
            Agent.speed = speedObjTarget;
        }

        public override void SetPedstrianStop()
        {
            base.SetPedstrianStop();
            Agent.speed = 0;
        }
    }
}