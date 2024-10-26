using UnityEngine;

namespace Darkness
{
    [CustomPreview(typeof(AnimancerClipAsset))]
    public class AnimancerClipLogic : PreviewLogic<AnimancerClipAsset>
    {
        public override void Enter()
        {
            base.Enter();
            Debug.Log($"Enter");
        }

        public override void Update(float time, float previousTime)
        {
        }

        public override void Exit()
        {
            base.Exit();
            Debug.Log($"Exit");
        }
    }
}