using System;
using System.Collections.Generic;
using System.Xml;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Resource;
using Keystone.Shaders;

namespace Keystone.Appearance
{
    public class AppearanceLOD : Appearance
    {
        public AppearanceLOD(string id)
            : base(id)
        {
        }

        public static AppearanceLOD Create(string id)
        {
            AppearanceLOD app;
            app = (AppearanceLOD)Repository.Get(id);
            if (app != null) return app;
            app = new AppearanceLOD(id);
            return app;
        }

        public void AddChild(Appearance child)
        {
            base.AddChild(child);
            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        }

        public override int Apply(Actor3d actor, int appearanceFlags)
        {
            if (_children != null)
            {
                return Select(appearanceFlags).Apply(actor,  appearanceFlags);
            }
            return 0;
        }

        public override int Apply(Mesh3d mesh, int appearanceFlags)
        {
            if (_children != null)
            {
                return Select(appearanceFlags).Apply(mesh, appearanceFlags);
            }
            return 0;
        }

        public override int Apply(Minimesh2 mini, int appearanceFlags)
        {
            if (_children != null)
            {
                return Select(appearanceFlags).Apply(mini, appearanceFlags);
            }
            return 0;
        }

        public override int Apply(Terrain land, int appearanceFlags)
        {
            if (_children != null)
            {
                return Select(appearanceFlags).Apply(land, appearanceFlags);
            }
            return 0;
        }

        private Appearance Select ( int appearanceFlags)
        {
            return (Appearance)_children[appearanceFlags];
        }
    }
}
