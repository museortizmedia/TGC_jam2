using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class AutoParticle : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem targetParticleSystem;

    [Tooltip("Color principal del Particle System")]
    [SerializeField] private Color particleColor = Color.white;

    /// <summary>
    /// Propiedad pública para asignar color dinámicamente.
    /// Al asignarla, actualiza el ParticleSystem inmediatamente.
    /// </summary>
    public Color ParticleColor
    {
        get => particleColor;
        set
        {
            particleColor = value;
            if (targetParticleSystem != null)
            {
                var main = targetParticleSystem.main; // Obtener siempre el MainModule al vuelo
                main.startColor = particleColor;
            }
        }
    }

    private void Awake()
    {
        // Obtener el ParticleSystem si no se asignó en inspector
        if (targetParticleSystem == null)
            targetParticleSystem = GetComponent<ParticleSystem>();

        // Aplicar color inicial
        if (targetParticleSystem != null)
        {
            var main = targetParticleSystem.main;
            main.startColor = particleColor;
        }
    }

    private void OnEnable()
    {
        if (targetParticleSystem == null) return;

        // Aplicar color antes de reproducir
        var main = targetParticleSystem.main;
        main.startColor = particleColor;

        // Reproducir automáticamente al habilitar
        targetParticleSystem.Play();

        // Iniciar coroutine para apagar GO cuando termine
        StartCoroutine(DisableWhenFinished());
    }

    private IEnumerator DisableWhenFinished()
    {
        if (targetParticleSystem == null)
            yield break;

        // Esperar mientras el ParticleSystem esté vivo
        while (targetParticleSystem.IsAlive(true))
        {
            yield return null;
        }

        // Desactivar el GameObject cuando termine
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Función opcional para reproducir el ParticleSystem manualmente
    /// </summary>
    public void Play()
    {
        if (targetParticleSystem == null) return;

        gameObject.SetActive(true); // Asegurarse de estar activo

        // Aplicar color actual antes de reproducir
        var main = targetParticleSystem.main;
        main.startColor = particleColor;

        targetParticleSystem.Play();
        StartCoroutine(DisableWhenFinished());
    }
}