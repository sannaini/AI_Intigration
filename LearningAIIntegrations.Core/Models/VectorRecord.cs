using System;
using System.Collections.Generic;
using System.Text;

namespace LearningAIIntegrations.Core.Models
{
    public class VectorRecord<TPayload>  
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public float[] Vector { get; set; }
        public TPayload Payload { get; set; }
    }
}
