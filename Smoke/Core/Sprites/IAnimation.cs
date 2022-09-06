using System;

namespace Smoke.Core.Sprites
{
    internal interface IAnimation
    {
        public void Start();
        public void Stop();
        public void Reset();

        long CurrentMilliseconds { get; set; }
        TimeSpan Duration { get; set; }
        bool IsPlaying { get; set; }
        int Frame { get; }
        int Frames { get; }
    }
}
