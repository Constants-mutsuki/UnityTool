using System;

namespace Darkness
{
    /// <summary>
    /// 时间轴运行时逻辑
    /// </summary>
    public class TimelineProcessor
    {
        private TimelineGraph m_graph;
        private float startTime;
        private float endTime;
        private float currentTime;
        private Action m_onStop;
    }
}
