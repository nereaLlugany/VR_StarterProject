using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand.Demo {
    public class Pistol : MonoBehaviour {
        public Rigidbody body;
        public Transform barrelTip;
        public float hitPower = 1;
        public float recoilPower = 1;
        public float range = 100;
        public LayerMask layer;
        public AudioClip shootSound;
        public float shootVolume = 1f;

        private void Start() {
            if (body == null && GetComponent<Rigidbody>() != null)
                body = GetComponent<Rigidbody>();
        }

        public void Shoot() {
            // Reproduce el sonido del disparo
            if (shootSound)
                AudioSource.PlayClipAtPoint(shootSound, transform.position, shootVolume);

            RaycastHit hit;

            // Lanza el rayo desde el cañón del arma
            if (Physics.Raycast(barrelTip.position, barrelTip.forward, out hit, range, layer)) {
                Debug.DrawRay(barrelTip.position, (hit.point - barrelTip.position), Color.green, 2f);

                // Si el objeto golpeado tiene un Rigidbody, aplica fuerza
                var hitBody = hit.transform.GetComponent<Rigidbody>();
                if (hitBody != null) {
                    hitBody.GetComponent<Smash>()?.DoSmash();
                    hitBody.AddForceAtPosition((hit.point - barrelTip.position).normalized * hitPower * 10, hit.point, ForceMode.Impulse);
                }

                // 🔥 Si el objeto golpeado se llama "GunCube", lo destruimos
                if (hit.transform.name.Contains("GunCube")) {
                    Destroy(hit.transform.gameObject);
                    Debug.Log("GunCube destruido 🔫");
                }
            }
            else {
                Debug.DrawRay(barrelTip.position, barrelTip.forward * range, Color.red, 1f);
            }

            // Recoil del arma
            body.AddForce(barrelTip.transform.up * recoilPower * 5, ForceMode.Impulse);
        }
    }
}
