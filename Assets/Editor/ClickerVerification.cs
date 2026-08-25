using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Clicker.Editor
{
    public static class ClickerVerification
    {
        [MenuItem("Tools/Clicker/Run EditMode Tests")]
        public static void RunEditModeTests()
        {
            if (EditorApplication.isCompiling)
            {
                return;
            }

            TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
            VerificationCallbacks callbacks = new(api);
            api.RegisterCallbacks(callbacks);
            ExecutionSettings settings = new(new Filter
            {
                assemblyNames = new[] { "Clicker.EditModeTests" },
                testMode = TestMode.EditMode
            })
            {
                runSynchronously = true
            };

            api.Execute(settings);
        }

        private sealed class VerificationCallbacks : ICallbacks
        {
            private readonly TestRunnerApi api;

            public VerificationCallbacks(TestRunnerApi api)
            {
                this.api = api;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                if (result.FailCount == 0 && result.PassCount > 0)
                {
                    Debug.Log($"CLICKER_TESTS_PASSED: {result.PassCount} passed, {result.SkipCount} skipped.");
                }
                else
                {
                    Debug.LogError(
                        $"CLICKER_TESTS_FAILED: {result.FailCount} failed, {result.PassCount} passed. {result.Message}");
                }

                api.UnregisterCallbacks(this);
                Object.DestroyImmediate(api);
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.HasChildren || result.TestStatus != TestStatus.Failed)
                {
                    return;
                }

                Debug.LogError($"Test '{result.FullName}' failed: {result.Message}\n{result.StackTrace}");
            }
        }
    }
}
