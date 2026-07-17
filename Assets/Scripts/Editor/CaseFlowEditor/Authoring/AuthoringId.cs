using System;

namespace Verdict.Editor.CaseFlow.Authoring
{
    internal static class AuthoringId
    {
        public static string New()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
