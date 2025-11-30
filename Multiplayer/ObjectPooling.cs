using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// A robust Object Pool for Fusion.
/// Reuses NetworkObjects by disabling them instead of Destroying them.
/// </summary>
public class FusionObjectPool : MonoBehaviour, INetworkObjectProvider
{
    public NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
    {
        throw new System.NotImplementedException();
    }

    public NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid)
    {
        throw new System.NotImplementedException();
    }

    public void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
    {
        throw new System.NotImplementedException();
    }
}