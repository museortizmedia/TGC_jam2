using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleModule : NetworkBehaviour
{
    public string puzzleName;
    [Tooltip("Exactamente 4 módulos hijos")]
    public GameObject[] moduleSlots;
    public NetworkVariable<FixedString32Bytes> ColorIdRute =
        new NetworkVariable<FixedString32Bytes>(
            "blanco",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    public ObjectColored[] RouteColorObjets;
    public ObjectColored[] NoRouteColorObjets;
    public UnityEvent<string> OnInitPuzzle;

    public void IniciarPuzzle()
    {
        OnInitPuzzle?.Invoke(puzzleName);
    }

    public override void OnNetworkSpawn()
    {
        ColorIdRute.OnValueChanged += OnColorChanged;
        OnColorChanged(default, ColorIdRute.Value);
    }

    private void OnColorChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        //ApplyColor(newValue.ToString());
        /*foreach (var routecolor in RouteColorObjets)
        {
            routecolor.ApplyColorInObject(newValue);
        }

        string otherColor = NoRouteColorObjets[0].GetOtherColorThan(newValue.ToString());
        foreach (var noroutecolor in NoRouteColorObjets)
        {
            noroutecolor.ApplyColorInObject(otherColor);
        }*/
    }
}
