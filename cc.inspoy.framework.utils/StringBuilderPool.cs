/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Utils
 * FileName: StringBuilderPool.cs
 * Created on 2020/07/13 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Instech.Framework.Core;

namespace Instech.Framework.Utils
{
    public static class StringBuilderPool
    {
        public static StringBuilder Acquire()
        {
            return ObjectPool<StringBuilder>.GetNew();
        }

        public static void ReleaseToPool(this StringBuilder sb)
        {
            ObjectPool<StringBuilder>.Instance.Recycle(sb);
        }
        
        static StringBuilderPool()
        {
            ObjectPool<StringBuilder>.Instance.RecycleCallback = OnRecycle;
            ObjectPool<StringBuilder>.Instance.ActivateCallback = ObjectPool<StringBuilder>.Instance.EmptyCallback;
            ObjectPool<StringBuilder>.Instance.DestroyCallback = ObjectPool<StringBuilder>.Instance.EmptyCallback;
        }

        private static void OnRecycle(StringBuilder sb)
        {
            if (sb == null)
            {
                throw new ArgumentNullException(nameof(sb));
            }

            sb.Clear();
        }
    }
}
