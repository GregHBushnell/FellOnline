using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.AI;

namespace FellOnline.Shared
{
public class MobController : MobBehaviour
{

    public NavMeshAgent agent;
    public Animator animator;

    public const float StableMoveSpeedConstant = 4.0f;
    public const float StableSprintSpeedConstant = 6.0f;
    public MobAttributeTemplate MoveSpeedTemplate;
	public MobAttributeTemplate RunSpeedTemplate;
	
    public float MoveRange = 10f;

    public int MoveEveryXSeconds = 5;
    public int RandomiseByXSeconds = 15;
    
    int TicksPerMove;

    public Vector3 SpawnPosition;

    private MobState _MobState;

    public Transform Target;

    int tickCounter = 0;
        private uint _moveStartTick;

        public override void OnAwake()
        {
            base.OnAwake();
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            agent.speed = StableMoveSpeedConstant;
        }
       
        public override void OnSpawnServer(NetworkConnection connection)
        {
            base.OnSpawnServer(connection);
            if (base.TimeManager != null)
            {
                base.TimeManager.OnTick += TimeManager_OnTick;
                int random = Random.Range(0, RandomiseByXSeconds);
                TicksPerMove = (MoveEveryXSeconds * base.TimeManager.TickRate) +random;
                if(agent == null|| animator == null){
                     agent = GetComponent<NavMeshAgent>();
                     animator = GetComponentInChildren<Animator>();
                     agent.speed = StableMoveSpeedConstant;
                }
               // _MobState.Position = transform.position;
                //_MobState.Rotation = transform.rotation;
                
            }
        }

        public override void OnStartServer()
	    {   
         base.OnStartServer();

		    //Quang: Subscribe to tick event, this will replace FixedUpdate
         
	    }

		public override void OnStopNetwork()
		{
			if (base.TimeManager != null)
			{
				base.TimeManager.OnTick -= TimeManager_OnTick;
			}
		}

        private void TimeManager_OnTick()
		{
			
			if (!base.IsServerStarted)
			{return;}
                 if(IsServerInitialized){
                    //ServerUpdateNPCState();
                    tickCounter++;
                    if (tickCounter >= TicksPerMove)
                    {
                        // Move the mob
                    
                        MoveMob();

                            // Set animator parameter based on rotation
                            float rotationValue = Mathf.Sign(transform.forward.x);
                            animator.SetFloat("Rotation", rotationValue);

                            // Set animator parameter based on speed
                            animator.SetFloat("Speed", agent.velocity.magnitude);
                        
                        tickCounter = 0; // Reset the counter
                    }
                 }
               
                // if(IsClientInitialized){
                      //  ApplyStateToNPC();
                   // }
            
			

		}
        private void ServerUpdateNPCState()
        {
                _MobState.Position =  transform.position;
                _MobState.Rotation = transform.rotation;
         }
        private void ApplyStateToNPC()
        {
        // Client-side state application (e.g., for visual smoothing)
        transform.position = Vector3.Lerp(transform.position, _MobState.Position, Time.deltaTime * 2);
        }
        void MoveMob()
        {
            // Get a random position within the move range
            Vector3 randomPosition = RandomNavSphere(SpawnPosition, MoveRange, -1);
            Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
            {
            Vector3 randDirection = Random.insideUnitSphere * dist;

            randDirection += origin;

            NavMeshHit navHit;

            NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

            return navHit.position;
            }

            agent.SetDestination(randomPosition);
        }

       
    }
}
