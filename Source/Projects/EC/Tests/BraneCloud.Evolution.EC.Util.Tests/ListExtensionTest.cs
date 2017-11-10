using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BraneCloud.Evolution.EC.Util.Tests
{
    [TestClass]
    public class ListExtensionTest
    {
        [TestMethod]
        public void RemoveAndReturn()
        {
            var list = new List<object>();
            for (var i = 0; i < 10; i++)
            {
                list.Add(i);
            }

            var count = 5;
            var newList = list.TakeTop(count);

            Assert.AreEqual(newList.Count, count);


        }


    }

    public static class ListExtensions
    {
        public static IList<T> TakeTop<T>(this List<T> list, int count)
            where T : class
        {
            var offset = list.Count - count;
            if (offset <= 0 && count > 0) return list.ToList();

            var newList = list.Skip(offset).Take(count).ToList();

            for (var i = list.Count - 1; i >= offset; i--)
                list.RemoveAt(i);

            return newList;
        }
    }
}
