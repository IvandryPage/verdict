using System;

namespace Verdict.Editor.CaseEditor.Authoring
{
    internal static class AuthoringId
    {
        public static string New()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
