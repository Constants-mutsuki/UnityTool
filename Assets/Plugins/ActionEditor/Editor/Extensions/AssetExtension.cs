﻿using UnityEngine;

namespace Darkness
{
    public static class AssetExtension
    {
        public static float TimeToPos(this TimelineGraphAsset timelineGraphAsset, float time)
        {
            return (time - timelineGraphAsset.ViewTimeMin) / timelineGraphAsset.ViewTime * G.CenterRect.width;
        }

        public static float PosToTime(this TimelineGraphAsset timelineGraphAsset, float pos)
        {
            return (pos - Styles.LeftMargin) / G.CenterRect.width * timelineGraphAsset.ViewTime + timelineGraphAsset.ViewTimeMin;
        }
    }
}
