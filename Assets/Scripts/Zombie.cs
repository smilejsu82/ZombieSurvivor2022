using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    public float radius = 1.0f;
    public LayerMask layerMask;
    private bool isDead = false;
    private GameObject targetGo;
    private NavMeshAgent agent;
    private Animator anim;

    //test
    private void Start()
    {
        this.Init();    
    }

    public void Init()
    {
        this.agent = this.GetComponent<NavMeshAgent>();
        this.anim = this.GetComponent<Animator>();

        this.StartCoroutine(this.UpdatePathRoutine());
    }

    private IEnumerator UpdatePathRoutine()
    {
        while (true) {
            if (this.isDead) break;

            if (this.targetGo == null)
            {

                this.FindTarget();

                yield return new WaitForSeconds(1f);

                //this.targetGo = null;
                //this.agent.isStopped = true;
                //this.anim.SetBool("HasTarget", false);

            }
            else {
                if (this.isMoving == false) {
                    this.isMoving = true;
                    this.anim.SetInteger("State", 1);

                    //Ÿ���� �ִٸ� Ÿ������ �̵��Ѵ� 
                    this.agent.isStopped = false;
                    this.agent.SetDestination(this.targetPosition);

                    if (this.routine != null) this.StopCoroutine(this.routine);
                    this.routine = this.StartCoroutine(this.MoveRoutine());
                }
                
            }


            yield return null;  
        }
    }

    private Coroutine routine;
    private Vector3 targetPosition;
    private bool isMoving = false;

    private void FindTarget() {
        //���������� Ÿ���� ã�� 
        //Ÿ�� : �÷��̾� 
        var center = this.transform.position + new Vector3(0, 0.75f, 0);
        Collider[] colliders = Physics.OverlapSphere(center, this.radius, layerMask);

        Debug.Log(colliders.Length);

        for (int i = 0; i < colliders.Length; i++)
        {
            var playerControl = colliders[i].GetComponent<PlayerControl>();

            if (playerControl != null && !playerControl.IsDead())
            {
                this.targetGo = playerControl.gameObject;
                this.targetPosition = this.targetGo.transform.position;
                break;
            }
        }
    }

    private IEnumerator MoveRoutine()
    {
        while (true) {
            var dis = Vector3.Distance(this.targetPosition, this.transform.position);

            Debug.Log(dis);

            if (dis <= 1f)
            {
                this.agent.isStopped = true;
                this.anim.SetInteger("State", 0);
                break;
            }

            yield return null;
        }

        Debug.Log("�̵� �Ϸ�");

        //���� 
        Debug.Log("����");
        yield return new WaitForSeconds(0.5f);

        //������ ������ �ٽ� ������ �ѷ����� ã�´� 
        this.isMoving = false;
        this.targetGo = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(this.transform.position + new Vector3(0, 0.75f, 0), this.radius);
    }
}
