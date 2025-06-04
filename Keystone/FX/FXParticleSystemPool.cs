using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Keystone.FX
{
    public class FXParticleSystemPool : FXBase
    {
        // todo: we need an array of struct PoolItem that hosts the time created, lifespan and the ModeledEntity 
        // todo: should we have seperate pools for each ModeledEntity ParticleSystem?
        // todo: const int MAX_COUNT
        // todo: maybe this hosts <T>ObjectPool
        private Dictionary<string, Entities.ModeledEntity> mPool;

        public FXParticleSystemPool()
        {
            mPool = new Dictionary<string, Entities.ModeledEntity>();


        }
    }
}