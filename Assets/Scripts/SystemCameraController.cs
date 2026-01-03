using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

public class SystemCameraController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private CinemachineCamera gameplayCamera;
    public CinemachineCamera GameplayCamera { get => gameplayCamera; }
    [SerializeField] private CinemachineCamera cinematicCamera;

    [Header("Settings")]
    [SerializeField] private float cinematicDuration = 3f;
    [SerializeField] private float startDistance = 15f;
    [SerializeField] private float endDistance = 4f;

    [Header("Events")]
    public UnityEvent OnStartCinematic1;
    public UnityEvent OnEndCinematic1;
    public UnityEvent OnStartCinematic2;
    public UnityEvent OnEndCinematic2;

    private Coroutine currentRoutine;

    #region PUBLIC API

    public void PlayIntroCinematic(Vector3 center, Transform target)
    {
        StopCurrentRoutine();
        currentRoutine = StartCoroutine(IntroCinematicRoutine(center, target));
    }

    public void PlayEndCinematic(Vector3 center)
    {
        StopCurrentRoutine();
        currentRoutine = StartCoroutine(EndCinematicRoutine(center));
    }

    #endregion

    #region ROUTINES

    private IEnumerator IntroCinematicRoutine(Vector3 center, Transform target)
    {
        OnStartCinematic1?.Invoke();

        Debug.DrawRay(target.position, target.forward * 5f, Color.blue, 5f);
        Debug.DrawRay(target.position, -target.forward * 5f, Color.red, 5f);

        cinematicCamera.Priority = 20;
        gameplayCamera.Priority = 0;

        Transform cam = cinematicCamera.transform;

        Vector3 dir = (target.position - center).normalized;
        Vector3 startPos = center - dir * startDistance + Vector3.up * 5f;
        Vector3 endPos = target.position - dir * endDistance + Vector3.up * 2f;

        Quaternion startRot = Quaternion.LookRotation(target.position - startPos);
        Quaternion endRot = Quaternion.LookRotation(target.position - endPos);

        float elapsed = 0f;

        while (elapsed < cinematicDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cinematicDuration;

            cam.position = Vector3.Lerp(startPos, endPos, t);
            cam.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        cam.position = endPos;
        cam.rotation = endRot;

        OnEndCinematic1?.Invoke();

        cinematicCamera.Priority = 0;
        gameplayCamera.Priority = 20;

        currentRoutine = null;
    }

    private IEnumerator EndCinematicRoutine(Vector3 center)
    {
        OnStartCinematic2?.Invoke();

        cinematicCamera.Priority = 20;
        gameplayCamera.Priority = 0;

        Transform cam = cinematicCamera.transform;

        Vector3 topDownPos = center + Vector3.up * 25f;
        Quaternion topDownRot = Quaternion.Euler(90f, 0f, 0f);

        cam.position = topDownPos;
        cam.rotation = topDownRot;

        yield return new WaitForSeconds(2f);

        OnEndCinematic2?.Invoke();

        currentRoutine = null;
    }

    #endregion

    private void StopCurrentRoutine()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
    }
}