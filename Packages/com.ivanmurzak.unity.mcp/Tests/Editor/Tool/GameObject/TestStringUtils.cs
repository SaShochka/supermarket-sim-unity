using System.Collections;
using com.IvanMurzak.ReflectorNet.Utils;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestStringUtils : BaseTest
    {
        [UnityTest]
        public IEnumerator Path_ParseParent()
        {
            {
                Assert.IsTrue(StringUtils.Path_ParseParent("root/nestedGo", out var parentPath, out var name), "Path should be valid");
                Assert.AreEqual("root", parentPath, "Parent path should be 'root'");
                Assert.AreEqual("nestedGo", name, "Name should be 'nestedGo'");
            }
            {
                Assert.IsFalse(StringUtils.Path_ParseParent("root", out var parentPath, out var name), "Path should be invalid");
                Assert.AreEqual(null, parentPath, "Parent path should be 'null'");
                Assert.AreEqual("root", name, "Name should be 'root'");
            }
            {
                Assert.IsTrue(StringUtils.Path_ParseParent("root/obj/child", out var parentPath, out var name), "Path should be invalid");
                Assert.AreEqual("root/obj", parentPath, "Parent path should be 'root/obj'");
                Assert.AreEqual("child", name, "Name should be 'child'");
            }
            {
                Assert.IsFalse(StringUtils.Path_ParseParent("", out var parentPath, out var name), "Path should be invalid");
                Assert.AreEqual(null, parentPath, "Parent path should be 'null'");
                Assert.AreEqual(null, name, "Name should be 'null'");
            }
            {
                Assert.IsFalse(StringUtils.Path_ParseParent(null, out var parentPath, out var name), "Path should be invalid");
                Assert.AreEqual(null, parentPath, "Parent path should be 'null'");
                Assert.AreEqual(null, name, "Name should be 'null'");
            }

            yield return null;
        }
    }
}