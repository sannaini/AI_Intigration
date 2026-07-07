using System;
using System.Collections.Generic;
using System.Text;
using LearningAIIntegrations.Core.Models;

namespace LearningAIIntegrations.Core.Interfaces
{
    public interface IVectorMapper<TDomain, TPayload>
    {
        VectorRecord<TPayload> ToVectorRecord(TDomain domain);

        TDomain FromVectorRecord(VectorRecord<TPayload> record);
    }
}
