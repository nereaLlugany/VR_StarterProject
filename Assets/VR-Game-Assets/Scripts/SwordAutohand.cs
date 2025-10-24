using System.Collections;
using UnityEngine;
using Autohand;

namespace Autohand.Demo {
    [AddComponentMenu("Autohand/Demo/SwordAutohand")]
    public class SwordAutohand : MonoBehaviour {
        [Header("Components")]
        [SerializeField] private Rigidbody body;
        [SerializeField] private Grabbable grabbable;

        [Header("Hover (suspension) on release")]
        [SerializeField] private bool floatOnRelease = true;
        [SerializeField] private float hoverAmplitude = 0.15f;
        [SerializeField] private float hoverSpeed = 1.2f;
        [SerializeField] private float snapUpSpeed = 4f;
        [SerializeField] private float minHeightAboveGround = 0.12f; 
        [SerializeField] private LayerMask groundMask = ~0; 
        [SerializeField] private float groundCheckDistance = 5f; 
        [Header("Optional")]
        [SerializeField] private bool autoFindComponents = true;

        private bool isHovering = false;
        private Vector3 hoverBasePosition;
        private Coroutine hoverCoroutine;

        private void Reset() {
            if (autoFindComponents) {
                if (body == null) body = GetComponent<Rigidbody>();
                if (grabbable == null) grabbable = GetComponent<Grabbable>();
            }
        }

        private void Start() {
            if (body == null) body = GetComponent<Rigidbody>();
            if (grabbable == null) grabbable = GetComponent<Grabbable>();

            if (grabbable != null) {
                grabbable.OnGrabEvent += OnGrabbed;
                grabbable.OnReleaseEvent += OnReleased;
            }
        }

        private void OnDestroy() {
            if (grabbable != null) {
                grabbable.OnGrabEvent -= OnGrabbed;
                grabbable.OnReleaseEvent -= OnReleased;
            }
        }

        private void OnGrabbed(Hand hand, Grabbable grabbed) {
            StopHovering();

            if (body != null) {
                body.isKinematic = false;
                body.useGravity = true;
            }
        }

        private void OnReleased(Hand hand, Grabbable grabbed) {
            if (!floatOnRelease) return;

            hoverBasePosition = transform.position;

            AdjustHoverBaseAboveGround();

            if (body != null) {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.useGravity = false;
                body.isKinematic = true;
            }

            StartHovering();
        }

        private void AdjustHoverBaseAboveGround() {
            RaycastHit hit;
            Vector3 origin = hoverBasePosition + Vector3.up * 0.01f; // small offset to avoid self-hit
            if (Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore)) {
                float desiredY = hit.point.y + minHeightAboveGround;
                if (hoverBasePosition.y < desiredY) hoverBasePosition.y = desiredY;
            }
        }

        public void StartHovering() {
            if (isHovering) return;
            isHovering = true;
            hoverCoroutine = StartCoroutine(HoverRoutine());
        }

        public void StopHovering() {
            if (!isHovering) return;
            isHovering = false;
            if (hoverCoroutine != null) {
                StopCoroutine(hoverCoroutine);
                hoverCoroutine = null;
            }
        }

        private IEnumerator HoverRoutine() {
            float t = 0f;
            float startY = transform.position.y;
            float targetY = hoverBasePosition.y + hoverAmplitude * 0.5f;

            while (t < 1f && isHovering) {
                t += Time.deltaTime * snapUpSpeed;
                float y = Mathf.Lerp(startY, targetY, Mathf.SmoothStep(0f, 1f, t));
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
                yield return null;
            }

            float elapsed = 0f;
            while (isHovering) {
                elapsed += Time.deltaTime * hoverSpeed;
                float yOffset = Mathf.Sin(elapsed * Mathf.PI * 2f) * (hoverAmplitude * 0.5f);
                transform.position = new Vector3(hoverBasePosition.x, hoverBasePosition.y + yOffset, hoverBasePosition.z);
                yield return null;
            }
        }

        public void ForceReleaseHover() {
            OnReleased(null, grabbable);
        }

        public void ForceGrabRestorePhysics() {
            OnGrabbed(null, grabbable);
        }

        private void OnValidate() {
            if (hoverAmplitude < 0f) hoverAmplitude = 0f;
            if (hoverSpeed <= 0f) hoverSpeed = 0.1f;
            if (snapUpSpeed <= 0f) snapUpSpeed = 0.1f;
            if (minHeightAboveGround < 0f) minHeightAboveGround = 0f;
            if (groundCheckDistance <= 0f) groundCheckDistance = 0.1f;
        }
    }
}
