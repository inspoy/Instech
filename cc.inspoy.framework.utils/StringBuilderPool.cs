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
    public class StringBuilderPool : AutoCreateSingleton<StringBuilderPool>
    {
        private Stack<StringBuilder> _pool;

        protected override void Init()
        {
            _pool = new Stack<StringBuilder>();
        }

        public static StringBuilder Acquire()
        {
            if (Instance._pool.Count > 0)
            {
                return Instance._pool.Pop();
            }

            return new StringBuilder();
        }

        public static void Release(StringBuilder sb)
        {
            if (sb == null)
            {
                throw new ArgumentNullException(nameof(sb));
            }

            sb.Clear();
            Instance._pool.Push(sb);
        }
    }

    public static class StringBuilderPoolExtension
    {
        public static void ReleaseToPool(this StringBuilder sb)
        {
            StringBuilderPool.Release(sb);
        }
    }
}
