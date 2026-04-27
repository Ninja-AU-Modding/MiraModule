using MiraAPI.GameOptions;
using MiraOverloaded.Options.Roles.Neutral;
using TownOfUs.Assets;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraOverloaded.Modules;

public sealed class Explode
{
    public Transform Transform { get; }

    private Explode(Transform transform)
    {
        Transform = transform;
    }

    public void Clear()
    {
        Object.Destroy(Transform.gameObject);
    }

    public static Explode CreateExplode(Vector3 location)
    {
        var igniteRadius = OptionGroupSingleton<SentinelOptions>.Instance.ExplosionRadius.Value;

        var gameObject = MiscUtils.CreateSpherePrimitive(location, igniteRadius);
        gameObject.GetComponent<MeshRenderer>().material = AuAvengersAnims.IgniteMaterial.LoadAsset();

        return new Explode(gameObject.transform);
    }
}
