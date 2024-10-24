﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.AspNet.RegisterServices;
using Microsoft.Extensions.Options;
using Moq;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Senparc.CO2NET.Tests.RegisterServices
{
    [TestClass]
    public class RegisterServiceTests
    {
        [TestMethod]
        public void RegisterServiceTest()
        {
            var isDebug = true;
            Senparc.CO2NET.Config.IsDebug = !isDebug;// Convert the opposite value

            var mockEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath );
            RegisterService.Start(mockEnv.Object, new SenparcSetting() { IsDebug = isDebug });

            Console.WriteLine(Senparc.CO2NET.Config.RootDirectoryPath);
            Assert.IsTrue(Senparc.CO2NET.Config.RootDirectoryPath.Length > 0);
            Assert.AreEqual(isDebug, Senparc.CO2NET.Config.IsDebug);

            // Verify the directory, cannot be a file
            Assert.IsTrue(File.Exists(Path.Combine(Senparc.CO2NET.Config.RootDirectoryPath, "TestEntities", "Logo.jpg")));
        }
    }
}
