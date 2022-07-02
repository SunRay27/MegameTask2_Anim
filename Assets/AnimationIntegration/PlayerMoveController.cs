using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{


    [Header("Finishing")]
    [SerializeField] TextMeshProUGUI finishText;
    [SerializeField] float finishMoveSpeed = 20;
    [SerializeField] float finishStopDistance = 0.5f;
    [SerializeField] float finishDistance = 5;
    //persent of animation time to wait to ragdollify enemy
    [Range(0f,1f)]
    [SerializeField] float finishAnimationRagdollifyWaitPersent = 0.25f;
    private bool isFinishing = false;
    private bool canFinish = false;
    private Enemy lastFinishEnemy;

    [Header("Motion")]
    [SerializeField] float moveSpeed = 20;
    private Vector3 cameraRotationForward;
    Quaternion visualBaseRotation;

    [Header("Pointer follow")]
    [SerializeField] GameCamera gameCamera;
    [SerializeField] Transform torsoTransform;
    [Range(-180, 180)]
    [SerializeField] float torsoOffset = -90;
    [SerializeField] bool useRaycast = true;

    [Header("Visual")]
    [SerializeField] MeshRenderer rifle;
    [SerializeField] MeshRenderer sword;

    private Animator animator;
    private CharacterController characterController;
    private static readonly int isWalking = Animator.StringToHash("isWalking");
    private static readonly int animMoveSpeed = Animator.StringToHash("moveSpeed");
    private static readonly int animFinish = Animator.StringToHash("finish");

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        characterController = GetComponent<CharacterController>();
        cameraRotationForward = gameCamera.GetForwardMoveDirectionVector();
    }

    // Update is called once per frame
    private void Update()
    {
        //stop if we are in finishing state
        if (isFinishing)
            return;

        Vector2 moveVector = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            moveVector.x = -1;
        if (Input.GetKey(KeyCode.D))
            moveVector.x = 1;
        if (Input.GetKey(KeyCode.W))
            moveVector.y = 1;
        if (Input.GetKey(KeyCode.S))
            moveVector.y = -1;
        moveVector.Normalize();


        //needs axis sensivity/gravity calibration, but still works
        //moveVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

        //rotate moveVector so it matches camera target
        float angle = Vector3.SignedAngle(Vector3.forward, cameraRotationForward, Vector3.up) * Mathf.Deg2Rad;
        Vector3 moveDirection = new Vector3(
            moveVector.x * Mathf.Cos(angle) + moveVector.y * Mathf.Sin(angle),
            0,
            moveVector.y * Mathf.Cos(angle) - moveVector.x * Mathf.Sin(angle));

        Debug.DrawRay(transform.position, cameraRotationForward, Color.green);
        Debug.DrawRay(transform.position, moveDirection, Color.red);

        //move player according input
        Move(moveDirection, moveSpeed);

        //find closest enemy and determine if we can finish him
        Enemy closest = PseudoEnemyManager.Instance.GetClosestEnemy(transform.localPosition);
        float sqrDistanceToEnemy = (transform.localPosition - closest.transform.localPosition).sqrMagnitude;

        // 1.if there are enemies on map and we were not in range to closest enemy OR we have another enemy closer than previous
        // 2.AND if we are in range to closest
        if (((lastFinishEnemy == null && closest != null) || (lastFinishEnemy != null  && lastFinishEnemy != closest))
            && sqrDistanceToEnemy < finishDistance * finishDistance) 
        {
            finishText.enabled = true;
            lastFinishEnemy = closest;
            canFinish = true;
        }
        else if ((lastFinishEnemy != null) && (closest == null || sqrDistanceToEnemy > finishDistance * finishDistance)) // else if we were in range AND (there are no enemies OR we are not in range now)
        {
            finishText.enabled = false;
            lastFinishEnemy = null;
            canFinish = false;
        }

        //finish input
        if (canFinish)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(FinishEnemy(lastFinishEnemy));
                finishText.enabled = false;
            }
        }

    }
    private void LateUpdate()
    {
        //stop if we are in finishing state
        if (isFinishing)
            return;

        Vector3 mouseInput = Input.mousePosition;
        mouseInput.z = gameCamera.distance;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseInput);

        //raycast gives more accurate result
        if (useRaycast)
            if (Physics.Raycast(Camera.main.ScreenPointToRay(mouseInput), out RaycastHit hit))
                mouseWorldPos = hit.point;

        RotateTorso(mouseWorldPos);
    }
    
    void Move(Vector3 moveDirection, float moveSpeed)
    {
        if (Mathf.Approximately(moveDirection.sqrMagnitude, 0))
        {
            animator.SetBool(isWalking, false);
            transform.rotation = visualBaseRotation;
        }
        else
        {
            animator.SetBool(isWalking, true);
            visualBaseRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), Time.deltaTime * 15);
            transform.rotation = visualBaseRotation;
        }

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime - transform.up * Physics.gravity.magnitude * Time.deltaTime);
    }
    void RotateTorso(Vector3 target)
    {
        Vector3 directionToTarget = Vector3.ProjectOnPlane(new Vector3(target.x, transform.position.y, target.z) - transform.position, Vector3.up);
        Debug.DrawRay(transform.position, directionToTarget, Color.cyan);
        float rawTorsoAngle = Vector3.SignedAngle(Vector3.forward, directionToTarget, Vector3.up);

        //a bit of tweaking in Quaternion.Euler()
        torsoTransform.rotation = Quaternion.Euler(0, rawTorsoAngle + torsoOffset, -90);
    }

    private IEnumerator FinishEnemy(Enemy lastFinishEnemy)
    {
        isFinishing = true;

        //increase animation speed
        animator.SetFloat(animMoveSpeed, finishMoveSpeed / moveSpeed);

        //get direction to enemy
        Vector3 moveDirection = (lastFinishEnemy.transform.position - transform.position);

        //while we are not close enough
        while (moveDirection.sqrMagnitude > finishStopDistance * finishStopDistance)
        {
            Debug.DrawRay(transform.position, moveDirection);

            //move to enemy
            Move(moveDirection.normalized, finishMoveSpeed);
            yield return null;
            //update direction to enemy (enemy can be moving)
            moveDirection = (lastFinishEnemy.transform.position - transform.position);
        }
        //make sure player is facing enemy
        transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        //reset walk animation speed and trigger animator event
        animator.SetFloat(animMoveSpeed, 1);
        animator.SetBool(isWalking, false);
        animator.SetTrigger(animFinish);

        //switch weapon
        rifle.enabled = false;
        sword.enabled = true;

        //Debug.Log("Waiting for state switch");
        //wait for animator to apply state
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("SwordFinish"))
            yield return null;

        //Debug.Log($"Waiting for {ragdollifyAnimationPersent*100}% time");
        //wait N% of finish animation time
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < finishAnimationRagdollifyWaitPersent)
            yield return null;

        //ragdollify enemy
        lastFinishEnemy.Ragdollify();

        //Debug.Log("Waiting for animation end");
        //wait for finish animation end
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("SwordFinish"))
            yield return null;

        //Debug.Log("Animation finished");
        //switch weapon
        rifle.enabled = true;
        sword.enabled = false;

        isFinishing = false;
    }


}
