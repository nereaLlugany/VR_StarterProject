
using UnityEngine;

public class CubKiller : MonoBehaviour
{
    [SerializeField] private GameSceneController sceneController;

    private void OnTriggerEnter(Collider other)
    {
        // Comprova que el nom de l'objecte que entra és "GunCube" o "SwordCube"
      
            
            print("Detectat");
            // Crida la funció Substract del GameSceneController
            if (sceneController != null)
            {
                sceneController.Substract();
            }

            // Destrueix el cub quan entra al trigger
            Destroy(other.gameObject);
        
    }
}