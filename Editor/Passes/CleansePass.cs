using System.Linq;
using nadena.dev.ndmf;
using UnityEngine;

namespace Surge.Editor.Passes
{
    internal class CleansePass : Pass<CleansePass>
    {
        public override string DisplayName => "Remove Surge Components";

        protected override void Execute(BuildContext context)
        {
            var modules = context.AvatarRootObject.GetComponentsInChildren<SurgeModule>(true);
            foreach (var module in modules)
            {
                if (!module || !module.gameObject)
                    continue;

                // Make sure we don't delete a GameObject with stuff on it
                var deleteSelf = module.gameObject.GetComponentsInChildren<Component>()
                    .Any(c => c is not Transform && c is not SurgeModule);
                
                Object.DestroyImmediate(deleteSelf ? module : module.gameObject);
            }
        }
    }
}