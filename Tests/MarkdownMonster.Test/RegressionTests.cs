using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    /// <summary>
    /// Regression tests covering bugs discovered during the code review on
    /// branch claude/code-review-deaRx. Each test is named after the class
    /// it guards; when they fail the fix in the matching file has regressed.
    /// </summary>
    [TestClass]
    public class RegressionTests
    {
        [TestMethod]
        public void ApplicationUpdater_VersionOverload_InitializesVersionInfo()
        {
            // Guards: ApplicationUpdater(Version) previously left VersionInfo
            // null, which caused NullReferenceException in MainWindow when
            // accessing updater.VersionInfo.Detail after an update check.
            var updater = new ApplicationUpdater(new Version(1, 0));
            Assert.IsNotNull(updater.VersionInfo,
                "ApplicationUpdater(Version) must initialize VersionInfo.");
        }

        [TestMethod]
        public void MarkdownDocument_Close_DoesNotThrow_WhenRenderFileMissing()
        {
            // Guards: MarkdownDocument.Close() is invoked from the finalizer
            // and must not throw if the .htm file is already gone.
            var doc = new MarkdownDocument { Filename = "untitled" };

            // Ensure the render file does NOT exist
            if (File.Exists(doc.HtmlRenderFilename))
                File.Delete(doc.HtmlRenderFilename);

            doc.Close(); // should be a no-op, not throw
        }

        [TestMethod]
        public void MarkdownDocument_WriteFile_WritesContent()
        {
            // Guards: WriteFile retry loop was previously convoluted and
            // could throw on the 4th attempt even when earlier attempts
            // would have succeeded. It must now write successfully on the
            // first try and leave the file on disk.
            var doc = new MarkdownDocument();
            var filename = Path.Combine(Path.GetTempPath(),
                "mm_writefile_test_" + Guid.NewGuid().ToString("N") + ".htm");

            try
            {
                doc.WriteFile(filename, "<html>hello</html>");
                Assert.IsTrue(File.Exists(filename),
                    "WriteFile should create the output file.");
                Assert.AreEqual("<html>hello</html>", File.ReadAllText(filename));
            }
            finally
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void MarkdownDocument_WriteFile_ThrowsAfterRetries_WhenPathInvalid()
        {
            // Guards: WriteFile must still raise ApplicationException when
            // all retries fail (e.g. path that cannot be created).
            var doc = new MarkdownDocument();
            // Null character is illegal in file names on both Windows and
            // Linux, so WriteAllText reliably throws on every attempt.
            var bad = Path.Combine(Path.GetTempPath(), "mm_invalid_\0_name.htm");
            doc.WriteFile(bad, "<html/>");
        }

        [TestMethod]
        public void Editor_Js_SetLanguage_TranslatesCppToAceMode()
        {
            // Guards: editor.js contained `lang == "c_cpp"` (a stray
            // comparison) instead of an assignment, so the "c++" alias was
            // never rewritten to Ace's "c_cpp" language id.
            var jsPath = FindEditorJs();
            var js = File.ReadAllText(jsPath);
            StringAssert.Contains(js, "lang = \"c_cpp\"",
                "editor.js must assign c_cpp, not compare.");
            Assert.IsFalse(js.Contains("lang == \"c_cpp\""),
                "editor.js should no longer contain the buggy comparison.");
        }

        [TestMethod]
        public void Editor_Js_KeydownHandler_UsesLocalKeycode()
        {
            // Guards: the printable-key detection compared against a
            // non-existent `e.keycode` property; every non-digit first
            // keystroke bypassed the dirty flag. The fix compares the
            // local `keycode` variable throughout.
            var js = File.ReadAllText(FindEditorJs());
            Assert.IsFalse(js.Contains("e.keycode"),
                "editor.js must not reference e.keycode (lowercase).");
        }

        private static string FindEditorJs()
        {
            // The tests run from Tests/MarkdownMonster.Test/bin/... - walk up
            // until we find the repo root so we can locate Editor/editor.js.
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            while (dir != null)
            {
                var candidate = Path.Combine(dir, "MarkdownMonster", "Editor", "editor.js");
                if (File.Exists(candidate))
                    return candidate;
                dir = Path.GetDirectoryName(dir);
            }
            Assert.Fail("Could not locate MarkdownMonster/Editor/editor.js relative to test assembly.");
            return null;
        }
    }
}
