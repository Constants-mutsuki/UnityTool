using System.Collections.Generic;
using CZToolKit;
using CFloat = System.Single;

namespace Darkness
{
    [ViewModel(typeof(Group))]
    public class GroupProcessor : IDirectable
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool Active { get; }
        public IDirector Root { get; }
        public IDirectable Parent { get; }
        public IEnumerable<IDirectable> Children { get; }
        public CFloat Length { get; }
        public CFloat StartTime { get; }
        public CFloat EndTime { get; }
        public void Enter(FrameData frameData)
        {
            throw new System.NotImplementedException();
        }

        public void Update(FrameData frameData)
        {
            throw new System.NotImplementedException();
        }

        public void Exit(FrameData frameData)
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }
    }
}
