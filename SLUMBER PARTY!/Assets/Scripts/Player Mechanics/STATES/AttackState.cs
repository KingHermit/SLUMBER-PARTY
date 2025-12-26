using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using Combat;

public class AttackState : CharacterState
{
    // player's character data stores moves (MoveData scriptables) which contain everything from the move type down to the hitboxes. access that?
    private MoveData m_MoveData;
    private float start;
    private float active;
    private float recovery;

    private float timer;
    private float endTime;

    private List<HitboxController> activeHitboxes = new List<HitboxController>();
    private Dictionary<HitboxData, bool> spawned = new Dictionary<HitboxData, bool>();

    public AttackState(CharacterController controller, CharacterStateMachine stateMachine)
        : base(controller, stateMachine) { }

    public override void SetMove(MoveData m)
    {
        m_MoveData = m;
    }

    // Called ONCE when entering the state
    public override void Enter()
    {
        controller.setAttackTrue();

        // start animation by startup
        controller.animator.SetBool("isAttacking", true);

        foreach (var h in m_MoveData.hitboxes)
            spawned[h] = false;

        start = FrameToSeconds(m_MoveData.startup);
        active = FrameToSeconds(m_MoveData.active);
        recovery = FrameToSeconds(m_MoveData.recovery);
        endTime = start + active + recovery;

        Debug.Log($"{controller.name} attacking: {m_MoveData.moveName}, duration ~= {endTime} seconds, active hitboxes = {m_MoveData.hitboxes.Length}");

        timer = 0;
    }

    // Called ONCE when exiting the state
    public override void Exit()
    {
        controller.setAttackFalse();
        timer = 0;
        // exit once animation / recovery frames finish
        controller.animator.SetBool("isAttacking", false);
    }

    // Called every frame
    public override void UpdateLogic()
    {
        // see which other attacks are chainable?
        // apply damage..? oh man i'd have to keep track of hitboxes too
        timer += Time.deltaTime;

        // startup -> active (Enter Active Frame Duration)
        if (timer >= start && timer < start + active)
        {
            if (activeHitboxes.Count == 0)
            {
                SpawnHitboxes();
                return;
            }
            DespawnHitboxes(); // self-explanatory
        }

        if (timer >= endTime)
        {
            if (!controller.isGrounded())
            {
                controller.RequestFall();
                return;
            }
            else
            {
                controller.RequestIdle();
                return;
            }
        }
    }

    // Called every physics frame
    public override void UpdatePhysics()
    {
        // check if current frame is an active frame
        //   apply appropriate knockback + angle

        // slightly halt movement while attacking
        controller.rb.linearVelocity = new Vector2(
            controller.moveDirection.x * (controller.playerSpeed * 0.4f), // 0.4f change per attack (light, heavy, medium maybe?)
            controller.rb.linearVelocity.y
        );
    }

    public float GetActiveFrames()
    {
        return active;
    }

    float FrameToSeconds(int frames) { return frames / 24f; }

    private void SpawnHitboxes()
    {
        // for each hitbox in MoveData.hitboxes, add hitbox to activeHitboxes when timer (float) = HitboxData.startFrame
        for (int i = 0; i < m_MoveData.hitboxes.Length; i++)
        {
            var hb = m_MoveData.hitboxes[i];
            float startTime = start + FrameToSeconds(hb.startFrame);

            if (spawned[hb]) { continue; } // avoid re-adding hitbox

            if (!spawned[hb] && timer >= startTime) // if current frame is hitbox's startFrame, instantiate
            {
                spawned[hb] = true;

                // Debug.Log($"[SPAWN]: {hb.name} at {timer}");

                GameObject hitbox = GameObject.Instantiate(hb.HitboxPrefab); // create hitbox item
                hitbox.transform.SetParent(controller.hitboxParent);
                var hb_controller = hitbox.GetComponent<HitboxController>();

                hb_controller.Setup(hb, controller); // apply HitboxData to hitbox item
                activeHitboxes.Add(hb_controller);
            }
        }
    }

    private void DespawnHitboxes()
    {
        for (int i = activeHitboxes.Count - 1; i >= 0; i--)
        {
            var hb = activeHitboxes[i];
            float endTime = start + FrameToSeconds(hb.data.endFrame);

            if (timer >= endTime)
            {
                // Debug.Log($"[DESPAWN] {hb.data.name} at {timer}");

                hb.Disable();
                GameObject.Destroy(hb.gameObject);

                activeHitboxes.RemoveAt(i);
            }
        }
    }
}
