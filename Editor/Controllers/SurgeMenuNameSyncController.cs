using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Surge.Editor.Controllers
{
    /// <summary>
    /// Synchronizes the GameObject names with the menu item names
    /// for SurgeMenu and SurgeControl properties.
    /// </summary>
    internal class SurgeMenuNameSyncController<T> : ISurgeModuleHandler<T> where T : SurgeModule
    {
        private readonly Dictionary<GameObject, T> _modules = new();

        public void Add(T module) => _modules[module.gameObject] = module;

        public void Remove(T module) => _modules.Remove(module.gameObject);
        
        protected SurgeMenuNameSyncController()
        {
            Undo.postprocessModifications += OnModification;
        }

        public string Id => typeof(T).Name;

        protected virtual void Process(T module) { }

        private UndoPropertyModification[] OnModification(UndoPropertyModification[] modifications)
        {
            foreach (var modification in modifications)
            {
                var value = modification.currentValue;

                // We only care for when GameObjects get renamed
                if (value.target is not GameObject gameObject)
                    continue;
                
                // Specifically check the m_Name property.
                if (modification.currentValue.propertyPath != "m_Name")
                    continue;
                
                // And finally, ensure that we're not in the prefab view.
                if (PrefabStageUtility.GetPrefabStage(gameObject))
                    continue;

                if (!_modules.TryGetValue(gameObject, out var module))
                    continue;
                
                if (!module)
                {
                    Debug.LogWarning($"Null {Id} in sync controller");
                    continue;
                }
                
                Process(module);
            }

            return modifications;
        }
    }
    
    internal class SurgeMenuSync : SurgeMenuNameSyncController<SurgeMenu>
    {
        protected override void Process(SurgeMenu module) => module.SetName(module.gameObject.name);
    }
        
    internal class SurgeControlSync : SurgeMenuNameSyncController<SurgeControl>
    {
        protected override void Process(SurgeControl module) => module.SetName(module.gameObject.name);
    }
}