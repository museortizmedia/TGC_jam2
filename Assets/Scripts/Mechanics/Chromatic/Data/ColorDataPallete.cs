using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewColor", menuName = "BETWEEN/ColorDataPallete")]
public class ColorDataPallete : ScriptableObject
{
    public ColorData[] pallete;

    public ColorData Find(string colorName)
    {
        if(pallete.Length == 0 ) return null;
        return pallete.FirstOrDefault(c => c.colorId == colorName);
    }
}