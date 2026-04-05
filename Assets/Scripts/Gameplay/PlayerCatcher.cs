using KoKoKrunch.Managers;
using Spine.Unity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace KoKoKrunch.Gameplay
{
    public class PlayerCatcher : MonoBehaviour
    {
        [SerializeField] private float boundaryLeft = -2.8f;
        [SerializeField] private float boundaryRight = 2.8f;

        private Camera mainCamera;

        private void Awake()
        {
            EnhancedTouchSupport.Enable();
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGameActive) return;
            HandleInput();
        }

        private void HandleInput()
        {
            Vector3 pos = transform.position;

            // Touch input (works on touch panels and mobile)
            if (Touch.activeTouches.Count > 0)
            {
                Vector2 touchPos = Touch.activeTouches[0].screenPosition;
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 0));
                pos.x = worldPos.x;
            }
            // Mouse input (for editor testing)
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
                pos.x = worldPos.x;
            }
            // Keyboard input (for editor testing)
            else if (Keyboard.current != null)
            {
                float horizontal = 0f;
                if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                    horizontal = -1f;
                else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                    horizontal = 1f;

                if (Mathf.Abs(horizontal) > 0.01f)
                {
                    float speed = GameManager.Instance.Config.playerMoveSpeed;
                    pos.x += horizontal * speed * Time.deltaTime;
                }
            }

            pos.x = Mathf.Clamp(pos.x, boundaryLeft, boundaryRight);
            transform.position = pos;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var fallingItem = other.GetComponent<FallingItem>();
            if (fallingItem != null)
            {
                fallingItem.OnCaught();
            }
        }
    }
}
