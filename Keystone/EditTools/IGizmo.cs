using System;
using System.Collections.Generic;
using System.Text;
using Keystone.Entities;
using Keystone.Types;

namespace Keystone.Controllers
{
    // TODO: Manipulator is obsolete and now we use a ManipulatorController so perhaps IGizmo should be an interface for IOController
    public interface IGizmo
    {
        Entity Target { get; set; }
        float TranslationRate { get; set; }
        Vector3d Translation { get; set; }
        Vector3d Scale { get; set; }
        Vector3d Rotation { get; set; }
        bool Enable { get; set; }

        void SetWindow(int w, int h, float r);
        void SetWindow(int w, int h);

        void Reset();
        void Update(int mouseX, int mouseY);
        void BeginTranslation(int mouseX, int mouseY);
        void BeginRotation(int mouseX, int mouseY);
        void EndTransformation();
    }
}
