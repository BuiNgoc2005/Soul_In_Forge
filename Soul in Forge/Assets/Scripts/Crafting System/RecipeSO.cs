using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "SO/Recipe")]
public class RecipeSO : ScriptableObject
{
    public ItemSO[] inputs;
    public ItemSO output;
    public int outputQuantity = 1;

    public bool Matches(ItemSO[] provided)
    {
        Debug.Log($"[Recipe Check] {name} cần: {string.Join(", ", inputs.Select(i => i.itemName + " (ref:" + i.GetInstanceID() + ")"))}");
        Debug.Log($"[Recipe Check] người chơi đưa: {string.Join(", ", provided.Select(i => i.itemName + " (ref:" + i.GetInstanceID() + ")"))}");


        if (provided.Length != inputs.Length) return false;

        var listA = new List<ItemSO>(inputs);
        var listB = new List<ItemSO>(provided);

        foreach (var item in listA)
        {
            if (listB.Contains(item)) listB.Remove(item);
            else return false;
        }
        return listB.Count == 0;
    }
}
