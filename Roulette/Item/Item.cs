using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type
    {
        Ammo,
        Coin,
        Grenade,
        Heart,
        Weapon,
        Roulette,
        //Armor,
        //Accessory,
        //Consumable,
        //Material
    }

    public Type type;
    public int value;

    private void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }

}
