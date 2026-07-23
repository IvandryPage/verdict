using System;

namespace Verdict.Data.Cases
{
    /// <summary>
    /// A single reusable condition that gates whether an ArgumentRule can
    /// succeed. Pure data - it does not know how to evaluate itself.
    /// ResolverEngine/ResolverUtilities (Systems.Evaluation) interpret
    /// each concrete condition type against a ResolverContext. This keeps
    /// the Data layer free of any dependency on Runtime/Systems.
    /// </summary>
    [Serializable]
    public abstract class ArgumentConditionData
    {
    }
}
