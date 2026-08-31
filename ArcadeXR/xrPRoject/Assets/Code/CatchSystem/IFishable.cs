namespace Core.Catching
{
    public interface IFishable
    {
        void Catch();
        CatchData GetData();
        void MarkAttachedToBuoy(bool flag);
    }  
}

