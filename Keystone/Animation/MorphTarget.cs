using System;
using System.Collections.Generic;
using System.Text;
using MTV3D65;

namespace Keystone.Animation
{
    public class MorphTarget
    {
        private TVActor _actor;

        public MorphTarget(TVActor actor)
        {
            if (actor == null) throw new ArgumentNullException();
            _actor = actor;
        }

        public void MorphTargetCompiler_Clear()
        {
            _actor.MorphTargetCompiler_Clear();
        }

        public void MorphTargetCompiler_AddMorphTargetMesh(string sName, TVMesh pMesh)
        {
            _actor.MorphTargetCompiler_AddMorphTargetMesh(sName, pMesh);
        }

        public void MorphTargetCompiler_AddMorphTargetMesh(string sName, TVMesh pMesh, bool bReference)
        {
            _actor.MorphTargetCompiler_AddMorphTargetMesh(sName, pMesh, bReference);
        }

        public void MorphTargetCompiler_DeleteMorphTargetMesh(string sName)
        {
            _actor.MorphTargetCompiler_DeleteMorphTargetMesh(sName);
        }

        public void MorphTargetCompiler_Compile()
        {
            _actor.MorphTargetCompiler_Compile();
        }

        public void MorphTarget_Enable(bool bEnable)
        {
            _actor.MorphTarget_Enable(bEnable);
        }

        public void MorphTarget_Enable(bool bEnable, bool bDynamic)
        {
            _actor.MorphTarget_Enable(bEnable, bDynamic);
        }

        public void MorphTarget_SetWeight(int iPose, float fWeight)
        {
            _actor.MorphTarget_SetWeight(iPose, fWeight);
        }

        public void MorphTarget_SetWeightByName(string sMorphTarget, float fWeight)
        {
            _actor.MorphTarget_SetWeightByName(sMorphTarget, fWeight);
        }

        public string MorphTarget_GetName(int iPose)
        {
            return _actor.MorphTarget_GetName(iPose);
        }

        public float MorphTarget_GetWeight(int iPose)
        {
            return _actor.MorphTarget_GetWeight(iPose);
        }

        public int MorphTarget_GetCount()
        {
            return _actor.MorphTarget_GetCount();
        }
    }
}