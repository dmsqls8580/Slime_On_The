public interface ISavable<T>
{
    public T GetSaveData();
    public void LoadFromData(T _data);
}
