using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Multiplayer.Editor
{
    public static class NetworkObjectFixer
    {
        [MenuItem("Tools/Fix NetworkObjects in Scene")]
        public static void FixNetworkObjectsInScene()
        {
            var networkObjects = Object.FindObjectsByType<NetworkObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            foreach (var networkObject in networkObjects)
            {
                if (!networkObject.gameObject.scene.isLoaded) continue;

                var serializedObject = new SerializedObject(networkObject);
                var hashField = serializedObject.FindProperty("GlobalObjectIdHash");
    
                // Ugly hack. Reset the hash and apply it.
                // This implicitly marks the field as dirty, allowing it to be saved as an override.
                hashField.uintValue = 0;
                serializedObject.ApplyModifiedProperties();
                // Afterwards, OnValidate will kick in and return the hash to it's real value, which will be saved now.
            }
        }
    }
}