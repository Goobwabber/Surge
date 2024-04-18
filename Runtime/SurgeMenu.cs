using UnityEngine;

namespace Surge
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    internal class SurgeMenu : SurgeModule
    {   
        [field: SerializeField]
        public string Name { get; private set; } = string.Empty;

        [field: SerializeField]
        public Texture2D Icon { get; private set; } = null!;

        [field: SerializeField]
        public bool Synchronize { get; private set; } = true;
        
        public void SetName(string newName)
        {
            if (!string.IsNullOrEmpty(newName))
                name = newName;
            else
                name = " ";
            Name = newName;
        }
  
        private void OnValidate()
        {
            EditorControllers.Get<ISurgeModuleHandler<SurgeMenu>>(nameof(SurgeMenu)).Add(this);
            
            // Only update the name if synchronization is on
            // and if the names aren't matching.
            if (!Synchronize || name == Name)
                return;

            SetName(name);
        }

        private void OnDestroy()
        {
            EditorControllers.Get<ISurgeModuleHandler<SurgeMenu>>(nameof(SurgeMenu)).Remove(this);
        }
    }
}