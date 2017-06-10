using Microsoft.VisualStudio.TestTools.UnitTesting;
using DamageMeter.TeraDpsApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Newtonsoft.Json;

namespace DamageMeter.TeraDpsApi.Tests
{
    [TestClass()]
    public class DpsServerTests
    {
        [TestMethod()]
        public void FetchAllowedAreaIdTest()
        {
            List<AreaAllowed> areasAllowed = new List<AreaAllowed>();
            try {
                areasAllowed = JsonConvert.DeserializeObject<List<AreaAllowed>>("[{AreaId:953, BossIds:[]},{AreaId:950, BossIds:[1000,2000,3000,4000]}]");
            }
            catch
            {
                Assert.Fail();
            }

            Console.WriteLine("test: "+areasAllowed.ElementAt(0).AreaId);
            Assert.AreEqual(953, areasAllowed.ElementAt(0).AreaId);
            Assert.AreEqual(950, areasAllowed.ElementAt(1).AreaId);
            Assert.IsTrue(areasAllowed.ElementAt(1).BossIds.Contains(2000));
            Assert.IsTrue(areasAllowed.ElementAt(1).BossIds.Contains(1000));
            Assert.IsTrue(areasAllowed.ElementAt(1).BossIds.Contains(3000));
            Assert.IsTrue(areasAllowed.ElementAt(1).BossIds.Contains(4000));
            Assert.IsTrue(areasAllowed.ElementAt(0).BossIds.Count == 0);
        }
    }
}