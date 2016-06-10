using SlimDX.Direct3D11;
using SlimDX;
using System.Collections.Generic;

namespace Graphic
{
    public abstract class ShellBaseModel
    {
        public abstract void Draw(Device device, DeviceContext context, Matrix matrix);
        public abstract void SetAutoAnimation(bool value);
        public abstract void SetAnimationWeight(float value);

        public Dictionary<int, string> animateProgFunMass;
    }
}
