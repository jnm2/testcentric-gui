// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using NUnit.Engine;
using NUnit.Framework;

namespace TestCentric.Engine.Services
{
    public class ProjectServiceTests
    {
        private ProjectService _projectService;

        [SetUp]
        public void CreateServiceContext()
        {
            var services = new ServiceContext();
            services.Add(new ExtensionService());
            _projectService = new ProjectService();
            services.Add(_projectService);
            services.ServiceManager.StartServices();
        }

        [Test]
        public void ServiceIsStarted()
        {
            Assert.That(_projectService.Status, Is.EqualTo(ServiceStatus.Started));
        }
    }
}
