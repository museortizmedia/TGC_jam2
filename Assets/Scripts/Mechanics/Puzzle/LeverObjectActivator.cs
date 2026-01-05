using UnityEngine;
using System.Collections.Generic;

public class LeverObjectActivator : MonoBehaviour
{
    [System.Serializable]
    public class LeverObjectPair
    {
        [Tooltip("La palanca que activa el objeto")]
        public InteractionLever lever;

        [Tooltip("El objeto que se activará (puede ser uno o varios)")]
        public GameObject[] objectsToActivate;

        [Tooltip("¿Desactivar el objeto cuando se desactiva la palanca?")]
        public bool canDeactivate = false;

        [Header("Opciones Avanzadas")]
        [Tooltip("Retardo antes de activar el objeto (en segundos)")]
        public float activationDelay = 0f;

        [Tooltip("¿Activar solo una vez y luego ignorar interacciones?")]
        public bool oneTimeUse = false;

        [HideInInspector]
        public bool hasBeenUsed = false;
    }

    [Header("Configuración de Palancas y Objetos")]
    [SerializeField] private List<LeverObjectPair> leverPairs = new List<LeverObjectPair>();

    [Header("Opciones Generales")]
    [SerializeField] private bool debugMode = true;

    [Header("Modo Cooperativo (Opcional)")]
    [Tooltip("¿Requiere que TODAS las palancas estén activas para activar objetos?")]
    [SerializeField] private bool requireAllLeversActive = false;

    [Tooltip("Objetos que se activan solo cuando todas las palancas están activas")]
    [SerializeField] private GameObject[] cooperativeObjects;

    private void Start()
    {
        // Suscribirse a los eventos de todas las palancas
        foreach (var pair in leverPairs)
        {
            if (pair.lever != null)
            {
                pair.lever.onInteracted.AddListener(OnLeverInteracted);

                // Asegurar que los objetos estén desactivados al inicio
                foreach (var obj in pair.objectsToActivate)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                    }
                }
            }
            else
            {
                Debug.LogWarning("[LeverObjectActivator] Una palanca no está asignada en el inspector.", this);
            }
        }

        // Desactivar objetos cooperativos al inicio
        if (cooperativeObjects != null)
        {
            foreach (var obj in cooperativeObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse de los eventos para evitar errores
        foreach (var pair in leverPairs)
        {
            if (pair.lever != null)
            {
                pair.lever.onInteracted.RemoveListener(OnLeverInteracted);
            }
        }
    }

    private void OnLeverInteracted(InteractionLever lever)
    {
        // Buscar el par correspondiente a esta palanca
        LeverObjectPair targetPair = leverPairs.Find(p => p.lever == lever);

        if (targetPair != null)
        {
            // Verificar si es de un solo uso y ya fue usada
            if (targetPair.oneTimeUse && targetPair.hasBeenUsed)
            {
                if (debugMode)
                {
                    Debug.Log($"[LeverObjectActivator] La palanca '{lever.gameObject.name}' ya fue usada (one-time use).", this);
                }
                return;
            }

            // Activar objetos con o sin retardo
            if (targetPair.activationDelay > 0)
            {
                StartCoroutine(ActivateObjectsWithDelay(targetPair));
            }
            else
            {
                ActivateObjects(targetPair);
            }

            // Marcar como usada si es de un solo uso
            if (targetPair.oneTimeUse)
            {
                targetPair.hasBeenUsed = true;
            }
        }

        // Verificar modo cooperativo
        if (requireAllLeversActive)
        {
            CheckCooperativeActivation();
        }
    }

    private void ActivateObjects(LeverObjectPair pair)
    {
        if (pair.lever.IsActive)
        {
            // Activar objetos
            foreach (var obj in pair.objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);

                    if (debugMode)
                    {
                        Debug.Log($"[LeverObjectActivator] Objeto '{obj.name}' activado por palanca '{pair.lever.gameObject.name}'.", this);
                    }
                }
            }
        }
        else if (pair.canDeactivate)
        {
            // Desactivar objetos si está permitido
            foreach (var obj in pair.objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);

                    if (debugMode)
                    {
                        Debug.Log($"[LeverObjectActivator] Objeto '{obj.name}' desactivado por palanca '{pair.lever.gameObject.name}'.", this);
                    }
                }
            }
        }
    }

    private System.Collections.IEnumerator ActivateObjectsWithDelay(LeverObjectPair pair)
    {
        if (debugMode)
        {
            Debug.Log($"[LeverObjectActivator] Esperando {pair.activationDelay} segundos antes de activar objetos...", this);
        }

        yield return new WaitForSeconds(pair.activationDelay);

        ActivateObjects(pair);
    }

    private void CheckCooperativeActivation()
    {
        bool allActive = true;

        // Verificar si todas las palancas están activas
        foreach (var pair in leverPairs)
        {
            if (pair.lever != null && !pair.lever.IsActive)
            {
                allActive = false;
                break;
            }
        }

        // Activar o desactivar objetos cooperativos
        if (cooperativeObjects != null)
        {
            foreach (var obj in cooperativeObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(allActive);

                    if (debugMode)
                    {
                        Debug.Log($"[LeverObjectActivator] Objeto cooperativo '{obj.name}' {(allActive ? "activado" : "desactivado")}.", this);
                    }
                }
            }
        }
    }

    // =========================
    // MÉTODOS PÚBLICOS (API)
    // =========================

    /// <summary>
    /// Activa manualmente una palanca específica por índice
    /// </summary>
    public void ActivateLeverByIndex(int index)
    {
        if (index >= 0 && index < leverPairs.Count)
        {
            var pair = leverPairs[index];
            if (pair.lever != null)
            {
                pair.lever.Activate();
                OnLeverInteracted(pair.lever);
            }
        }
    }

    /// <summary>
    /// Desactiva manualmente una palanca específica por índice
    /// </summary>
    public void DeactivateLeverByIndex(int index)
    {
        if (index >= 0 && index < leverPairs.Count)
        {
            var pair = leverPairs[index];
            if (pair.lever != null && pair.canDeactivate)
            {
                pair.lever.Deactivate();
                ActivateObjects(pair);
            }
        }
    }

    /// <summary>
    /// Resetea todas las palancas y objetos
    /// </summary>
    public void ResetAll()
    {
        foreach (var pair in leverPairs)
        {
            if (pair.lever != null)
            {
                pair.lever.Deactivate();
                pair.hasBeenUsed = false;
            }

            foreach (var obj in pair.objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        if (cooperativeObjects != null)
        {
            foreach (var obj in cooperativeObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        if (debugMode)
        {
            Debug.Log("[LeverObjectActivator] Todas las palancas y objetos han sido reseteados.", this);
        }
    }

    /// <summary>
    /// Verifica si todas las palancas están activas
    /// </summary>
    public bool AreAllLeversActive()
    {
        foreach (var pair in leverPairs)
        {
            if (pair.lever != null && !pair.lever.IsActive)
            {
                return false;
            }
        }
        return true;
    }
}