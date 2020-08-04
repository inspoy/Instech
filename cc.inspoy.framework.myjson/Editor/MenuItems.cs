/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.MyJson.Editor
 * FileName: MenuItems.cs
 * Created on 2019/12/06 by inspoy
 * All rights reserved.
 */

using UnityEditor;

namespace Instech.Framework.MyJson.Editor
{
    public static class MenuItems
    {
        [MenuItem("Instech/MyJson/GenResolverCode", false, 4101)]
        private static void GenResolverCode()
        {
            GenUtf8JsonCode.GenCode();
        }
    }
}