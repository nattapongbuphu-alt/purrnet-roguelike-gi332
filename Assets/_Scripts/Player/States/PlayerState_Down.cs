using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerState_Down : StateNode
{
    [SerializeField] private StateNode aliveState;
    [SerializeField] private float reviveDistance = 2f;
    [SerializeField] private float reviveTime = 3f;
    [SerializeField] private GameObject graphics;
    [SerializeField] private Image reviveUI;
    [SerializeField] private List<MonoBehaviour> components;
    private SyncVar<float> reviveProgress = new SyncVar<float>(0f, ownerAuth: true);

    override public void Enter(bool asServer)
    {
        base.Enter(asServer);
        graphics.SetActive(true);
        ToggleComponents(false);
    }

    override public void Exit(bool asServer)
    {
        if (isOwner) { reviveProgress.value = 0; }
        base.Exit(asServer);
        graphics.SetActive(false);
        ToggleComponents(true);
    }
    private void ToggleComponents(bool enabled)
    {
        if (!isOwner) { return; }

        foreach (var component in components)
        {
            component.enabled = enabled;
        }
    }
    private void Awake()
    {
        reviveProgress.onChanged += OnReviveProgressChanged;
    }
    override protected void OnDestroy()
    {
        base.OnDestroy();
        reviveProgress.onChanged -= OnReviveProgressChanged;
    }
    private void OnReviveProgressChanged(float newProgress)
    {
        reviveUI.fillAmount = newProgress / reviveTime;
    }
    public override void StateUpdate(bool asServer)
    {
        base.StateUpdate(asServer);

        if (!isOwner || asServer) { return; }

        bool beingRevived = false;
        foreach (var player in PlayerHealth.AllPlayers.Values)
        {
            if (player.isOwner) { continue; }
            if (Vector3.Distance(player.transform.position, transform.position) > reviveDistance)
            {
                continue;
            }
            beingRevived = true;
            reviveProgress.value += Time.deltaTime;
        }

        if (!beingRevived)
        {
            reviveProgress.value = 0f;
        }

        if (reviveProgress >= reviveTime)
        {
            machine.SetState(aliveState);
        }
    }
}
