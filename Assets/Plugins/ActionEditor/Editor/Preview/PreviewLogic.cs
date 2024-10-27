namespace Darkness
{
    public abstract class PreviewLogic<T> : PreviewLogic where T : ClipAsset
    {
        public T clip => (T)directable;
    }

    public abstract class PreviewLogic
    {
        public DirectableAsset directable;

        public void SetTarget(DirectableAsset t)
        {
            directable = t;
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