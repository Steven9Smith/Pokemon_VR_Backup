namespace UnityEditor.GameFoundation
{
    internal interface ICollectionEditor
    {
        string name { get; }
        bool isCreating { get; }

        void Update();
        void OnWillEnter();
        void OnWillExit();
        void ValidateSelection();
    }
}
