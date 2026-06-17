using System;
using Cysharp.Threading.Tasks;
using Sequencing.Core;
using InventorySystem.Core;
using UnityEngine;

namespace InventorySystem.Loading
{
    /// <summary>
    /// Boot step that populates ItemRegistry, slotted into a SequenceEntryPoint
    /// chain so item loading is ordered, reported, and cancellable alongside
    /// every other load step. The loading screen covers the window before the
    /// registry is Ready, which is exactly the readiness gap this introduces.
    ///
    /// Note: Resources.LoadAll is synchronous and runs on the main thread, so
    /// this step still blocks the frame it executes on. For the small definition
    /// SOs that cost is negligible. The heavy world prefabs are what actually
    /// want async loading (Addressables at spawn), not these.
    /// </summary>
    public class ItemRegistryLoadStep : MonoBehaviour, IEntrySequence
    {
        [SerializeField] private string resourcesPath = "";   // "" scans every Resources folder
        [SerializeField] private Behaviour next;              // next IEntrySequence in the chain

        public event Action<string> DisplayMessage;
        public IEntrySequence Default => next as IEntrySequence;
        public bool IsCompleted { get; private set; }

        public async UniTask<IEntrySequence> ExecuteSequence()
        {
            DisplayMessage?.Invoke("Loading items...");

            // Let the loading UI paint the message before the blocking load.
            await UniTask.Yield();

            var defs = Resources.LoadAll<ItemStats>(resourcesPath);
            ItemRegistry.BuildFrom(defs);

            IsCompleted = true;
            return Default;
        }
    }
}