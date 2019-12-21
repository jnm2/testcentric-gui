// ***********************************************************************
// Copyright (c) 2019 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace NUnit.Engine.Runners
{
    internal class TestPackageValidator
    {
        private IRuntimeFrameworkService _runtimeService;

        public TestPackageValidator(IRuntimeFrameworkService runtimeService)
        {
            _runtimeService = runtimeService;
        }

        // Any Errors thrown from this method indicate that the client
        // runner is putting invalid values into the package.
        public void Validate(TestPackage package)
        {
#if !NETSTANDARD1_6 && !NETSTANDARD2_0  // TODO: How do we validate runtime framework for .NET Standard 2.0?
            var processModel = package.GetSetting(EnginePackageSettings.ProcessModel, "Default").ToLower();
            var runningInProcess = processModel == "inprocess";
            var frameworkSetting = package.GetSetting(EnginePackageSettings.RuntimeFramework, "");
            var runAsX86 = package.GetSetting(EnginePackageSettings.RunAsX86, false);

            if (frameworkSetting.Length > 0)
            {
                // Check requested framework is actually available
                if (!_runtimeService.IsAvailable(frameworkSetting))
                    throw new NUnitEngineException($"The requested framework {frameworkSetting} is unknown or not available.");

                // If running in process, check requested framework is compatible
                if (runningInProcess)
                {
                    var currentFramework = RuntimeFramework.CurrentFramework;

                    RuntimeFramework requestedFramework;
                    if (!RuntimeFramework.TryParse(frameworkSetting, out requestedFramework))
                        throw new NUnitEngineException("Invalid or unknown framework requested: " + frameworkSetting);

                    if (!currentFramework.Supports(requestedFramework))
                        throw new NUnitEngineException(string.Format(
                            "Cannot run {0} framework in process already running {1}.", frameworkSetting, currentFramework));
                }
            }

            if (runningInProcess && runAsX86 && IntPtr.Size == 8)
                throw new NUnitEngineException("Cannot run tests in process - a 32 bit process is required.");
#endif
        }
    }
}