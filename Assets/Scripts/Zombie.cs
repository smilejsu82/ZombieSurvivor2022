using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    public float sight = 5.0f;
    public float attackRange = 2.0f;
    public float moveSpeed = 2.0f;

    public LayerMask layerMask;
    private NavMeshAgent agent;
    private Animator anim;

    private GameObject targetGo;
    private NavMeshPath navMeshPath;

    private UnityEvent moveCompleteEvent = new UnityEvent();
    private UnityEvent attackCompleteEvent = new UnityEvent();
    public UnityEvent dieEvent = new UnityEvent();

    public enum eState{
        Idle, Move, Attack 
    }

    public void Init(Vector3 initPosition)
    {
        this.GetComponent<CapsuleCollider>().enabled = true;
        this.GetComponent<BoxCollider>().enabled = true;

        this.transform.parent = null;
        this.transform.position = initPosition;
        this.gameObject.SetActive(true);

        this.agent = this.GetComponent<NavMeshAgent>();
        this.anim = this.GetComponent<Animator>();
        this.navMeshPath = new NavMeshPath();
        this.moveCompleteEvent.AddListener(()=>{
            
            this.targetGo = null;

            this.Idle();
        });

        this.attackCompleteEvent.AddListener(()=>{
            
            this.targetGo = null;

            this.Idle();
        });

        this.Idle();
    }

    private void Idle()
    {
        this.anim.SetInteger("State", 0);

        this.StartCoroutine(this.IdleRoutine(()=>{

            var dis = Vector3.Distance(this.transform.position,  this.targetGo.transform.position);

            Debug.DrawLine(this.transform.position, this.targetGo.transform.position, Color.yellow, 2.0f);

            Debug.LogFormat("found target: {0}, dis: {1}", this.targetGo, dis);

            if(dis <= this.attackRange){
                Debug.Log("사거리 안에 있음");

                this.Attack();
            }else{
                Debug.Log("사거리 밖에 있음 ---> 이동 시작");

                var tpos = this.targetGo.transform.position;

                this.Move(tpos);
            }

        }));
    }

    private IEnumerator IdleRoutine(Action callback)
    {
        while(true)
        {
            yield return new WaitForSeconds(1.0f);  // idle anim clip length : 5.5

            this.FindTarget();

            if(this.targetGo != null)       //has target 
            {
                break;
            }

            Debug.Log("타겟을 찾지 못했습니다.");
        }

        callback();

    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result) {
		Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) {
            result = hit.position;
            return true;
        }
		result = Vector3.zero;
		return false;
	}

    private void Move(Vector3 tpos)
    {
        this.anim.SetInteger("State", 1);

        Debug.LogFormat("목표 지점 : {0}", tpos);

        if(this.agent.CalculatePath(tpos, this.navMeshPath) == false)
        {
            Debug.Log("갈수 없는 곳입니다.");

            Vector3 result;
            while(true)
            {
                var isFound = this.RandomPoint(tpos, 2.0f, out result);
                if(isFound)
                {
                    break;
                }
            }
            Debug.LogFormat("다시 찾은 갈수 있는 곳입니다. : {0}", result);
            
            this.StartCoroutine(this.MoveRoutine(result));

        }
        else{
            this.StartCoroutine(this.MoveRoutine(tpos));
        }

        //NavMeshHit hit;

        // if (NavMesh.SamplePosition(tpos, out hit, 1.0f, NavMesh.AllAreas)) {
        //     return true;
        // }
    }

    private IEnumerator MoveRoutine(Vector3 tpos)
    {
        //look at 
        this.transform.LookAt(tpos);

        while(true){

            //이동 
            this.transform.Translate(Vector3.forward * this.moveSpeed * Time.deltaTime);

            var dis = Vector3.Distance(tpos, this.transform.position);
            if(dis <= this.agent.stoppingDistance)
            {
                //이동완료 
                break;
            }

            yield return null;
        }

        Debug.Log("이동완료");

        this.moveCompleteEvent.Invoke();
    }

    private void Attack()
    {
        Debug.LogFormat("{0}을 공격 합니다.", this.targetGo);

        this.anim.SetInteger("State", 2);

        this.StartCoroutine(this.AttackRoutine());
    }

    public void Die()
    {
        this.GetComponent<CapsuleCollider>().enabled = false;
        this.GetComponent<BoxCollider>().enabled = false;

        this.StopAllCoroutines();

        this.StartCoroutine(this.DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        this.anim.SetTrigger("Die");

        yield return new WaitForSeconds(2.5f);

        this.dieEvent.Invoke();
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(2.5f);

        this.attackCompleteEvent.Invoke();
    }

    private void FindTarget()
    {
        var colliders = Physics.OverlapSphere(this.transform.position, this.sight, this.layerMask);
        if(colliders.Length > 0){
            this.targetGo = colliders[0].gameObject;
        }
    }

    public eState GetState(){
        return (eState)this.anim.GetInteger("State");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(this.transform.position, this.sight);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, this.attackRange);
    }
}
