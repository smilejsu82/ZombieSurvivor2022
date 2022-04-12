using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunControl : MonoBehaviour
{
    LineRenderer lineRenderer;
    public Transform firePoint;
    public float attackRange = 5f;
    Coroutine routine;
    int ammo = 8;

    [SerializeField]
    private ParticleSystem muzzleVfx;
    [SerializeField]
    private ParticleSystem shellEjectVfx;
    [SerializeField]
    private AudioSource gunAudioPlayer;
    [SerializeField]
    private AudioClip shotClip;
    [SerializeField]
    private AudioClip reloadClip;

    public UnityEvent<GameObject> lookatEvent = new UnityEvent<GameObject>();
    public UnityEvent shotCompleteEvent = new UnityEvent();


    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    public void Reload()
    {
        if(routine != null)
        {
            return;
        }
        routine = StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        this.gunAudioPlayer.PlayOneShot(this.reloadClip);

        yield return new WaitForSeconds(0.85f);
        ammo = 8;
        routine = null;
    }

    public void Fire()
    {
        if(routine != null || ammo <= 0)
        {
            return;
        }
        routine = StartCoroutine(ShotRoutine());
        ammo -= 1;
    }

    IEnumerator ShotRoutine()
    {

        var playerCenter = this.transform.root.GetComponent<CapsuleCollider>().bounds.center;
        var layerMask = LayerMask.NameToLayer("Zombie");

        //find target 
        var colliders =  Physics.OverlapSphere(playerCenter, this.attackRange, 1 << layerMask);

        //find nearest target 
        var max = 100000.0f;
        GameObject targetGo = null;
        for(int i = 0; i<colliders.Length; i++){

            var targetCenter = colliders[i].bounds.center;
            var distance = Vector3.Distance(playerCenter, targetCenter);
            if( max > distance){
                max = distance;
                targetGo = colliders[i].transform.gameObject;
            }
        }
        //found targetGo 
        if( targetGo != null){
            //send event -> look at 
            Debug.DrawLine(playerCenter, targetGo.GetComponent<CapsuleCollider>().bounds.center, Color.red, 2.0f);

            lookatEvent.Invoke(targetGo);

        }
        else{
            // not thing happen 
        }

        

        this.muzzleVfx.Play();
        this.shellEjectVfx.Play();
        this.gunAudioPlayer.PlayOneShot(this.shotClip);

        Ray ray = new Ray(firePoint.position, transform.forward);
        float lineLength = attackRange;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, attackRange))
        {
            lineLength = (firePoint.position - hit.point).magnitude;
        }
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, firePoint.position + transform.forward * lineLength);
        lineRenderer.enabled = true;
        yield return new WaitForSeconds(0.03f);
        lineRenderer.enabled = false;
        yield return new WaitForSeconds(0.4f);
        routine = null;


        //end of shot event 
        this.shotCompleteEvent.Invoke();
    }
}
