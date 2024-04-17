using Surge.Editor.Animation;
using Surge.Editor.Extensions;
using Surge.Editor.Passes;
using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(Surge.Editor.SurgePlugin))]

namespace Surge.Editor
{
    public class SurgePlugin : Plugin<SurgePlugin>
    {
        public override string DisplayName => nameof(Surge);

        protected override void Configure()
        {
            InPhase(BuildPhase.Resolving).Run<ResolvePass>();
            
            // Clone animators (using the Modular Avatar implementation... ty bd_ <3)
            InPhase(BuildPhase.Generating).Run("Clone Animators (MA Impl)", AnimationUtilities.CloneAllControllers);
            //InPhase(BuildPhase.Generating).Run<ContainerizationPass>();
            InPhase(BuildPhase.Generating).Run<ParameterGenerationPass>();
            
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.modular-avatar")
                //.Run<ContainerizationPass>()
                //.Then
                .Run<MenuGenerationPass>()
                //.Then.Run<ControlPass>()
                //.Then.Run<ParametrizationPass>()
                //.Then.Run<MenuizationPass>()
                //.Then.Run<TagizationPass>()
                .Then.Run<CleansePass>();
        }
    }
}
