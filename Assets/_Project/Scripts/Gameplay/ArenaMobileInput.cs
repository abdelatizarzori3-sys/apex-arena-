using UnityEngine;
using UnityEngine.InputSystem;

namespace ApexArena.Gameplay
{
    /// <summary>
    /// Lightweight touch adapter for Android builds. The left half of the
    /// display is a virtual movement pad; the right half exposes jump and run
    /// gestures without requiring a scene-authored Canvas prefab.
    /// </summary>
    public sealed class ArenaMobileInput : MonoBehaviour
    {
        [SerializeField] private float movementRadius = 180f;
        [SerializeField] private float jumpButtonRadius = 150f;

        private PlayerController player;
        private int movementFinger = -1;
        private Vector2 movementOrigin;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (player == null || Application.isEditor || Input.touchCount == 0)
            {
                return;
            }

            var run = false;
            var movement = Vector2.zero;
            for (var index = 0; index < Input.touchCount; index++)
            {
                var touch = Input.GetTouch(index);
                var isLeftSide = touch.position.x < Screen.width * 0.52f;
                if (isLeftSide)
                {
                    if (movementFinger < 0 && touch.phase == TouchPhase.Began)
                    {
                        movementFinger = touch.fingerId;
                        movementOrigin = touch.position;
                    }

                    if (touch.fingerId == movementFinger)
                    {
                        var delta = touch.position - movementOrigin;
                        movement = Vector2.ClampMagnitude(delta / movementRadius, 1f);
                        if (touch.phase is TouchPhase.Ended or TouchPhase.Canceled)
                        {
                            movementFinger = -1;
                        }
                    }
                }
                else
                {
                    var rightBottom = new Vector2(Screen.width - jumpButtonRadius, jumpButtonRadius);
                    if (touch.phase == TouchPhase.Began && Vector2.Distance(touch.position, rightBottom) <= jumpButtonRadius)
                    {
                        player.OnJump();
                    }

                    run |= touch.position.y < Screen.height * 0.55f && touch.phase is TouchPhase.Stationary or TouchPhase.Moved;
                }
            }

            player.SetMoveInput(movement);
            player.SetRunInput(run);
        }
    }
}
