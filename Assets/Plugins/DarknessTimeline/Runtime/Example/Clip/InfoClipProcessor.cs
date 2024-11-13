﻿using UnityEngine;

namespace Darkness
{
    [ViewModel(typeof(InfoClip))]
    public class InfoClipProcessor : ClipProcessor<InfoClip>
    {
        protected override void OnEnter(FrameData frameData, FrameData innerFrameData)
        {
            Debug.Log(TData.Info);
        }
    }
}