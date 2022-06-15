using Entity;
using Singleton;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Singleton
{
    /// <summary>
    /// 存储所有实体
    /// </summary>
    public class EntityManager : Singleton<EntityManager>
    {
        ConcurrentDictionary<long, IEntity> mEtities = new ConcurrentDictionary<long, IEntity>();
        public void AddEntity(IEntity entity, long id)
        {
            bool res = mEtities.TryAdd(id, entity);
            LoggerHelper.Instance().Log(LogType.Console, $"EntityManager AddEntity id:{id} type:{entity} result:{res}");
        }
        public IEnumerator GetEnumerator()
        {
            // 防止迭代器失效, 不直接遍历 mEtities
            var ids = mEtities.Keys;
            foreach (long id in ids)
            {
                mEtities.TryGetValue(id, out IEntity entity);
                yield return entity;
            }
        }
    }
}
