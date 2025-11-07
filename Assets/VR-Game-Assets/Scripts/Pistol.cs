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
        public Grabbable grabbable; 

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
        public bool floatOnRelease = true;          
        public float hoverAmplitude = 0.15f;       
        public float hoverSpeed = 1.2f;            
        public float snapUpSpeed = 4f;            

        private bool isHovering = false;
        private Vector3 hoverBasePosition;
        private Coroutine hoverCoroutine;

        [SerializeField] private GameSceneController sceneController;

        private Hand currentHand;


        private void Start()
        {
            if (body == null)
                body = GetComponent<Rigidbody>();

            if (grabbable == null)
                grabbable = GetComponent<Grabbable>();

            
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

        
        private void OnGrabbed(Hand hand, Grabbable grab)
        {

            currentHand = hand;
            
            StopHovering();

            if (body != null)
            {
                
                body.isKinematic = false;
                body.useGravity = true;
            }
        }

        private void OnReleased(Hand hand, Grabbable grab)
        {

            if (currentHand == hand)
                currentHand = null; 
            
            if (!floatOnRelease) return;
            hoverBasePosition = transform.position;

            if (body != null)
            {
                
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
            
            float t = 0f;
            float startY = transform.position.y;
            float targetY = hoverBasePosition.y + hoverAmplitude * 0.5f; 

            
            while (t < 1f)
            {
                t += Time.deltaTime * snapUpSpeed;
                float y = Mathf.Lerp(startY, targetY, Mathf.SmoothStep(0f, 1f, t));
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
                yield return null;
            }

            
            float elapsed = 0f;
            while (isHovering)
            {
                elapsed += Time.deltaTime * hoverSpeed;
                float yOffset = Mathf.Sin(elapsed * Mathf.PI * 2f) * (hoverAmplitude * 0.5f);
                transform.position = new Vector3(hoverBasePosition.x, hoverBasePosition.y + yOffset, hoverBasePosition.z);
                yield return null;
            }
        }

        // ---------------------- Disparar ----------------------
        public void Shoot()
        {


            if (grabbable != null)
            {

                
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
                        //Destroy(hit.transform.gameObject);
                        CubeExplode cubeExplode = hit.transform.GetComponent<CubeExplode>();

                        if (cubeExplode != null)
                        {
                            // Cridem la funció Explode() del cub
                            cubeExplode.Explode();
                            sceneController.Sum();
                            
                        }

                    }
                }
                else
                {
                    Debug.DrawRay(barrelTip.position, barrelTip.forward * range, Color.red, 1f);
                }

                
                if (body != null && !body.isKinematic)
                    body.AddForce(barrelTip.transform.up * recoilPower * 5, ForceMode.Impulse);
            }
        }

        private IEnumerator ResetTrigger()
        {
            yield return null;
            if (gunAnimator != null)
                gunAnimator.ResetTrigger(shootTriggerName);
        }

        public void DestroyCube()
        {
            Destroy(gameObject);
        }
    }
}