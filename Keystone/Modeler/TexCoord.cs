using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.Modeler
{
    public struct TexCoord
    {
        float tu, tv;

        public TexCoord(float _tu, float _tv)
        {
            tu = _tu;
            tv = _tv;
        }
    };
}
