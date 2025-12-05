
/// <summary>
/// Interface 
/// </summary>
namespace ObjectPool
{
    public interface IPooledObjects
    {
        /// <summary>
        /// Called when the object is spawned by an object pooler.
        /// </summary>
        void OnObjectSpawn();
    }
}