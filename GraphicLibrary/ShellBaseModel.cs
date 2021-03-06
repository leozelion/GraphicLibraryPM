﻿using SlimDX.Direct3D9;
using SlimDX;
using System.Collections.Generic;

namespace Graphic
{
    public abstract class ShellBaseModel
    {
        public abstract void Draw(Device device, Matrix matrix);
        public abstract void SetAutoAnimation(bool value);
        public abstract void SetAnimationWeight(float value);

        public Dictionary<int, string> animateProgFunMass;
    }
}
