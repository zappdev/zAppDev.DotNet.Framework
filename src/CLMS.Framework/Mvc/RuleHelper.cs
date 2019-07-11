using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace CLMS.Framework.Mvc
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Rule : Attribute
    {
        public List<EvalTime> EvaluationTimes;
        public RuleType Type;

        public Rule(RuleType type, EvalTime eval1, EvalTime eval2 = EvalTime.None, EvalTime eval3 = EvalTime.None)
        {
            EvaluationTimes = new List<EvalTime>();
            EvaluationTimes.Add(eval1);

            if (eval2 != EvalTime.None)
            {
                EvaluationTimes.Add(eval2);
            }

            if (eval3 != EvalTime.None)
            {
                EvaluationTimes.Add(eval3);
            }

            Type = type;
        }
    }

    public class RuleToExecuteInfo
    {
        public string Name { get; set; }
        public List<string> PartialViewControls { get; set; }
        public RuleType Type { get; set; }
        public List<int[]> Indexes { get; set; }
    }

    public enum EvalTime
    {
        None = 0,
        OnLoad,
        OnChange,
        OnSubmit
    }

    public enum DataValidationMessageType
    {
        INFO = 1,
        WARN,
        ERROR,
        SUCCESS,
        CUSTOM
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleType
    {
        [EnumMember(Value = "cf")]
        ConditionalFormatting,
        [EnumMember(Value = "dv")]
        DataValidation,
        [EnumMember(Value = "ce")]
        CalculatedExpression,
        [EnumMember(Value = "lcf")]
        ListConditionalFormatting,
    }

    public class RuleEvaluation
    {
        public bool Status;
        public object Expression;
        public int[] Indexes;
        public DataValidationMessageType DataValidationMessageType;
    }

    public class RuleResult
    {
        public RuleResult()
        {
            Evaluations = new List<RuleEvaluation>();
        }

        public List<RuleEvaluation> Evaluations;

        public string Name;
        public string PartialControl { get; set; }
    }
}