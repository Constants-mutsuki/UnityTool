namespace Darkness
{
    public abstract class PreviewLogic<T> : PreviewLogic where T : ClipAsset
    {
        public T Clip => (T)Directable;
    }

    public abstract class PreviewLogic
    {
        public DirectableAsset Directable;

        public void SetTarget(DirectableAsset t)
        {
            Directable = t;
        }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void ReverseEnter()
        {
        }

        public virtual void Reverse()
        {
        }


        public abstract void Update(float time, float previousTime);
    }
}
