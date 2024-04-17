namespace Surge
{
    internal interface ISurgeModuleHandler<in T> : EditorControllers.IEditorController
    {
        void Add(T module);

        void Remove(T module);
    }
}