using System.Collections;
using UnityEngine;
using Autohand; // si usas AutoHand

namespace Autohand.Demo
{
    public class Pistol : MonoBehaviour
    {
        [Header("Componentes")]
        public Rigidbody body;
        public Transform barrelTip;
        public Grabbable grabbable; // opcional (AutoHand)

        [Header("Disparo")]
        public float hitPower = 1;
        public float recoilPower = 1;
        public float range = 100;
        public LayerMask layer;

        [Header("Audio")]
        public AudioClip shootSound;
        public float shootVolume = 1f;

        [Header("Animación")]
        public Animator gunAnimator;
        public string shootTriggerName = "Fire";

        [Header("Flotar al soltar")]
        public bool floatOnRelease = true;          // activar/desactivar funcionalidad
        public float hoverAmplitude = 0.15f;       // altura máxima de la oscilación
        public float hoverSpeed = 1.2f;            // velocidad de oscilación
        public float snapUpSpeed = 4f;            // velocidad para corregir a la altura objetivo al iniciar hover

        private bool isHovering = false;
        private Vector3 hoverBasePosition;
        private Coroutine hoverCoroutine;

        private void Start()
        {
            if (body == null)
                body = GetComponent<Rigidbody>();

            if (grabbable == null)
                grabbable = GetComponent<Grabbable>();

            // Si existe Grabbable (AutoHand), nos suscribimos a eventos
            if (grabbable != null)
            {
                grabbable.OnGrabEvent += OnGrabbed;
                grabbable.OnReleaseEvent += OnReleased;
            }
        }

        private void OnDestroy()
        {
            if (grabbable != null)
            {
                grabbable.OnGrabEvent -= OnGrabbed;
                grabbable.OnReleaseEvent -= OnReleased;
            }
        }

        // ---------------------- Eventos de agarre / soltar ----------------------
        private void OnGrabbed(Hand hand, Grabbable grab)
        {
            // Cuando alguien la agarra, detenemos el flotado y restauramos física normal
            StopHovering();

            if (body != null)
            {
                // permitimos que la física la controle mientras la mano la tiene (si corresponde)
                body.isKinematic = false;
                body.useGravity = true;
            }
        }

        private void OnReleased(Hand hand, Grabbable grab)
        {
            // Cuando la sueltan, la dejamos flotando (si está habilitado)
            if (!floatOnRelease) return;

            // Guardamos la posición base para el hover (posición actual)
            hoverBasePosition = transform.position;

            if (body != null)
            {
                // Desactivamos gravedad y hacemos kinematic para que no caiga
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.useGravity = false;
                body.isKinematic = true;
            }

            StartHovering();
        }

        // ---------------------- Hover logic ----------------------
        private void StartHovering()
        {
            if (isHovering) return;
            isHovering = true;
            hoverCoroutine = StartCoroutine(HoverRoutine());
        }

        private void StopHovering()
        {
            if (!isHovering) return;
            isHovering = false;
            if (hoverCoroutine != null)
                StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        private IEnumerator HoverRoutine()
        {
            // Nos aseguramos de que la pistola suba suavemente a la altura base + amplitude
            float t = 0f;
            float startY = transform.position.y;
            float targetY = hoverBasePosition.y + hoverAmplitude * 0.5f; // ligero snap hacia una altura agradable

            // fase de "snap up" para corregir bruscos
            while (t < 1f)
            {
                t += Time.deltaTime * snapUpSpeed;
                float y = Mathf.Lerp(startY, targetY, Mathf.SmoothStep(0f, 1f, t));
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
                yield return null;
            }

            // Oscilación continua
            float elapsed = 0f;
            while (isHovering)
            {
                elapsed += Time.deltaTime * hoverSpeed;
                float yOffset = Mathf.Sin(elapsed * Mathf.PI * 2f) * (hoverAmplitude * 0.5f);
                transform.position = new Vector3(hoverBasePosition.x, hoverBasePosition.y + yOffset, hoverBasePosition.z);
                yield return null;
            }
        }

        // ---------------------- Disparo ----------------------
        public void Shoot()
        {
            // Si la pistola está kinematic (flotando), permite disparar igualmente
            if (shootSound)
                AudioSource.PlayClipAtPoint(shootSound, transform.position, shootVolume);

            if (gunAnimator != null)
            {
                gunAnimator.SetTrigger(shootTriggerName);
                StartCoroutine(ResetTrigger());
            }

            RaycastHit hit;
            if (barrelTip == null)
            {
                Debug.LogWarning("[Pistol] barrelTip no asignado.");
                return;
            }

            if (Physics.Raycast(barrelTip.position, barrelTip.forward, out hit, range, layer))
            {
                Debug.DrawRay(barrelTip.position, (hit.point - barrelTip.position), Color.green, 2f);

                var hitBody = hit.transform.GetComponent<Rigidbody>();
                if (hitBody != null)
                {
                    hitBody.GetComponent<Smash>()?.DoSmash();
                    hitBody.AddForceAtPosition((hit.point - barrelTip.position).normalized * hitPower * 10, hit.point, ForceMode.Impulse);
                }

                if (hit.transform.name.Contains("GunCube"))
                {
                    Destroy(hit.transform.gameObject);
                    
                }
            }
            else
            {
                Debug.DrawRay(barrelTip.position, barrelTip.forward * range, Color.red, 1f);
            }

            // Recoil: si el rigidbody existe y no es kinematic, aplica fuerza real; de lo contrario puedes animar el retroceso
            if (body != null && !body.isKinematic)
                body.AddForce(barrelTip.transform.up * recoilPower * 5, ForceMode.Impulse);
        }

        private IEnumerator ResetTrigger()
        {
            yield return null;
            if (gunAnimator != null)
                gunAnimator.ResetTrigger(shootTriggerName);
        }
    }
}
