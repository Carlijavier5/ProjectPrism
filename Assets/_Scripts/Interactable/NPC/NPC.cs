using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

/// <summary>
/// Npc, dialogue, their character model
/// </summary>
public class NPC : Interactable
{
    [SerializeField] protected GameObject vfx;
    [SerializeField] private Animator animator;
    [SerializeField] protected NPCData _npcData;

    [SerializeField] private Animator screenspaceAnimator;
    [SerializeField] private CinemachineVirtualCamera vc;
    [SerializeField] private float orthoZoom;
    protected bool _dialogueStarted = false;
    protected int _dialogueIndex = 0;

    [SerializeField] private Transform quadTransform;
    private Vector3 quadAnchor;

    void Start() {
        quadAnchor = quadTransform.position;
    }

    public override void InteractAction(OrbThrownData data) {
        base.InteractAction(data);
        if (!_dialogueStarted) { StartCoroutine(IDialogue(data)); }
    }

    void Update() {
        if (_dialogueStarted) {
            quadTransform.position = Vector3.MoveTowards(quadTransform.position, quadAnchor
                                   + new Vector3(Mathf.Sin(Time.time) * 0.1f, Mathf.Cos(Time.time / 1.72f) * 0.1f, 0),
                                     Time.deltaTime / 10);
        }
    }

    protected void DialogueStart() {
        _dialogueIndex = 0;
        _dialogueStarted = true;
        vfx.SetActive(true);
        GameManager.Instance.EnterUIControls();
        GameManager.Instance.PlayerActionMap.UIControl.Continue.performed += ContinueDialogue;
        animator.SetTrigger("wahoo");
    }

    protected void ContinueDialogue(InputAction.CallbackContext context) {
        _dialogueIndex++;

        if (_dialogueIndex == _npcData.ImageSeq.Count) {  EndDialoge(); return;  }
        animator.speed = 2;
        animator.SetTrigger($"swap{_dialogueIndex}");
        //_ImageHolder.sprite = _npcData.ImageSeq[_dialogueIndex];
    }

    protected void EndDialoge() {
        //_ImageHolder.gameObject.SetActive(false);
        GameManager.Instance.EnterPlayerControls();
    }

    private IEnumerator IDialogue(OrbThrownData data) {
        screenspaceAnimator.SetTrigger("Talk");
        vc.Priority = 20;
        while (vc.m_Lens.OrthographicSize != orthoZoom) {
            vc.m_Lens.OrthographicSize = Mathf.MoveTowards(vc.m_Lens.OrthographicSize, orthoZoom, Time.deltaTime);
            yield return null;
        } yield return new WaitForSeconds(0.5f);
        DialogueStart();
    }
}
