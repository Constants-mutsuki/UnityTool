using System.Collections.Generic;
using UnityEngine;

namespace Darkness
{
    public interface IDirector : IData
    {
        float Length { get; }

        void SaveToAssets();
    }
}
