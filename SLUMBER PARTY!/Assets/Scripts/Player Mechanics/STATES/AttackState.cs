using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AttackState : PlayerState
{
    // player's character data stores moves (MoveData scriptables) which contain everything from the move type down to the hitboxes. access that?
    private MoveData m_MoveData;
    private float start;
    private float active;
    private float recovery;

    private float timer;
    private float endTime;

    private List<HitboxController> activeHitboxes = new List<HitboxController>();

    public AttackState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void SetMove(MoveData m)
    {
        m_MoveData = m;
    }

    // Called ONCE when entering the state
    public override void Enter() {
        // start animation by startup
        player._animator.SetBool("isAttacking", true);

        timer = 0;
        start = FrameToSeconds(m_MoveData.startup);
        active = FrameToSeconds(m_MoveData.active);
        recovery = FrameToSeconds(m_MoveData.recovery);
        endTime = start + active + recovery;

        Debug.Log($"Attacking: {m_MoveData.moveName}, duration ~= {endTime} seconds, active hitboxes = {m_MoveData.active}");
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        // exit once animation / recovery frames finish
        DestroyHitboxes(); // juuuuust in case
        player._animator.SetBool("isAttacking", false);
    }

    // Called every frame
    public override void UpdateLogic() {
        // see which other attacks are chainable?
        // apply damage..? oh man i'd have to keep track of hitboxes too
        timer += Time.deltaTime;

        // SpawnHitboxes(); // testicles

        // startup -> active
        if (timer >= start && timer < start + active)
        {
            if (activeHitboxes.Count == 0) // doesn't pass this part..?
            {
                SpawnHitboxes();
            }
        }

        // active -> recovery frames

        else if (timer >= start + active && activeHitboxes.Count > 0)
        {
            DestroyHitboxes();
        }

        if (timer >= endTime)
        {
            player.isAttacking = false;
            if (!player.isGrounded())
            {
                stateMachine.ChangeState(player.falling);
                return;
            } else
            {
                Debug.Log("Okay I'm done");
                stateMachine.ChangeState(player.idle);
                return;
            }
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        // check if current frame is an active frame
        //   apply appropriate knockback + angle
    }

    float FrameToSeconds(int frames)
    {
        return frames / 24f;
    }

    private void SpawnHitboxes()
    {
        foreach (var h in m_MoveData.hitboxes)
        {
            GameObject obj = GameObject.Instantiate(player.hitboxPrefab);
            obj.transform.SetParent(player.hitboxParent); // <-- key difference
            obj.transform.position = player.transform.position + (Vector3)h.offset;

            var controller = obj.GetComponent<HitboxController>();
            controller.Setup(h, player);

            activeHitboxes.Add(controller);
        }
    }

    private void DestroyHitboxes()
    {
        foreach (var h in activeHitboxes)
            GameObject.Destroy(h.gameObject);

        activeHitboxes.Clear();
    }
}
