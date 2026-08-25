using UnityEngine;
using UnityEngine.InputSystem;
using ApexArena.Gameplay;
using ApexArena.Core;

namespace ApexArena.Gameplay
{
    /// <summary>
    /// متحكم اللاعب - يدير الحركة، القتال، والتفاعل
    /// Player Controller - manages movement, combat, and interaction
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(ResourceManager))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Identity")]
        public string PlayerName = "Operative";
        public string PlayerId;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = -20f;

        [Header("Stats")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private bool isAlive = true;

        [Header("Components")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private WeaponSystem weaponSystem;
        [SerializeField] private TechSystem techSystem;

        private CharacterController characterController;
        private ResourceManager resourceManager;
        private Vector3 velocity;
        private Vector2 moveInput;
        private bool isRunning;
        private bool isGrounded;

        public bool IsAlive => isAlive;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public event System.Action<int, int> OnHealthChanged;
        public event System.Action OnPlayerDeath;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            resourceManager = GetComponent<ResourceManager>();

            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;
        }

        private void Start()
        {
            GameManager.Instance?.RegisterPlayer(this);
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (!isAlive) return;

            HandleMovement();
            HandleGravity();
            HandleRotation();
        }

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void SetMoveInput(Vector2 input)
        {
            moveInput = input;
        }

        public void OnRun(InputValue value)
        {
            isRunning = value.isPressed;
        }

        public void SetRunInput(bool running)
        {
            isRunning = running;
        }

        public void OnJump()
        {
            if (isGrounded)
            {
                velocity.y = jumpForce;
                PlayJumpVFX();
            }
        }

        public void OnFire()
        {
            if (!isAlive) return;
            weaponSystem?.Fire();
        }

        public void OnReload()
        {
            weaponSystem?.Reload();
        }

        public void OnActivateTech()
        {
            if (!isAlive) return;
            techSystem?.ActivateCurrentTech();
        }

        public void OnInteract()
        {
            // جمع الموارد، فتح الأبواب، إلخ
            TryCollectResource();
        }

        public void TakeDamage(int damage, DamageType damageType)
        {
            if (!isAlive) return;

            // تطبيق الدروع التكيفية إذا كانت مفعلة
            if (techSystem?.IsTechActive(TechType.AdaptiveArmor) == true)
            {
                damage = Mathf.RoundToInt(damage * 0.6f); // 40% تخفيض
            }

            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (!isAlive) return;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            isAlive = false;
            currentHealth = 0;
            OnPlayerDeath?.Invoke();
            GameManager.Instance?.OnPlayerDeath(this);

            // تأثيرات الموت
            PlayDeathVFX();

            Debug.Log($"[PlayerController] {PlayerName} has died.");
        }

        private void HandleMovement()
        {
            float speed = isRunning ? runSpeed : walkSpeed;
            Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

            // تحويل الحركة حسب اتجاه الكاميرا
            move = cameraTransform.TransformDirection(move);
            move.y = 0;
            move.Normalize();

            characterController.Move(move * speed * Time.deltaTime);
        }

        private void HandleGravity()
        {
            isGrounded = characterController.isGrounded;

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        private void HandleRotation()
        {
            if (moveInput.sqrMagnitude > 0.01f)
            {
                Vector3 lookDirection = new Vector3(moveInput.x, 0, moveInput.y);
                lookDirection = cameraTransform.TransformDirection(lookDirection);
                lookDirection.y = 0;

                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                }
            }
        }

        private void TryCollectResource()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 2f, LayerMask.GetMask("Resource"));
            foreach (var hit in hits)
            {
                var resource = hit.GetComponent<ResourceNode>();
                if (resource != null)
                {
                    resource.Collect(resourceManager);
                    break;
                }
            }
        }

        private void PlayJumpVFX()
        {
            // TODO: Particle effect
        }

        private void PlayDeathVFX()
        {
            // TODO: Death particle + sound
        }

        private void OnDestroy()
        {
            GameManager.Instance?.UnregisterPlayer(this);
        }
    }

    public enum DamageType
    {
        Physical,
        Energy,
        Thermal,
        Radiation,
        Quantum
    }
}
