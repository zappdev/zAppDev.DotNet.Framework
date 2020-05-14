// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class ViewDTO
    {
        public bool IsOptimized { get; set; }
        public ViewModelDTOBase Model;
        public List<ResponseAssigment> ClientResponse;
        public RuleEvaluationInfo RuleEvaluations = new RuleEvaluationInfo();        
        
        public List<ClientCommandInfo> ClientCommands = new List<ClientCommandInfo>();

        [JsonIgnore]
        public bool DataValidationsHaveFailed
        {
            get
            {
                return RuleEvaluations.DataValidations.Any(d => d.Evaluations.Any(e => e.Status == true));
            }
        }

        [JsonIgnore]
        public bool DataValidationsHaveOnlyWarnings
        {
            get
            {
                return !RuleEvaluations.DataValidations.Any(
                    d => d.Evaluations.Any(e => e.Status && e.DataValidationMessageType != DataValidationMessageType.WARN)
                );
            }
        }

        public string Serialize(JsonSerializerSettings settings = null, bool indented = false)
        {
            return Utilities.Serialize(this, settings, indented);            
        }       
    }

    public class ResponseAssigment
    {
        public string Path { get; set; }
        public object Value { get; set; }
    }

    public class RuleEvaluationInfo
    {
        public List<RuleResult> DataValidations = new List<RuleResult>();
        public List<RuleResult> ConditionalFormattings = new List<RuleResult>();
        public List<RuleResult> CalculatedExpressions = new List<RuleResult>();
    }

    public class ClientCommandInfo
    {
        public ClientCommandInfo(ClientCommands cmd, params object[] args)
        {
            Command = cmd.ToString();
            Params = args;
        }

        public string Command;
        public object[] Params;
    }

    public enum ClientCommands
    {
        LIST_REFRESH,
        LIST_CLEAR_SELECTED_ITEMS,
        DATALIST_REFRESH,
        DATALIST_CLEAR_SELECTED_ITEMS,
        DATALIST_UPDATE_SIZE,
        GRID_GOTO_LAST_PAGE,
        GRID_GOTO_PREV_PAGE,
        GRID_GOTO_NEXT_PAGE,
        GRID_GOTO_FIRST_PAGE,
        GRID_GOTO_PAGE,        
		CLEAR_GLOBAL_RESOURCES_CACHE,
        SHOW_MESSAGE,
		HIDE_MODAL,
		SHOW_MODAL,
        CHART_REFRESH,
        PUSH_TO_NAVIGATION_HISTORY,
        CLOSE_FORM,
        REDIRECT,
        GRID_REFRESH,
        MAP_REFRESH,
	    MAP_DIRECTIONS,
        MAP_FITCONTENT,
        EXPORT_FORM_TO_PDF,
        EXPORT_CONTROL_TO_PDF,
        EXECUTE_JS,
        DROPDOWN_REFRESH,
        CALENDAR_REFRESH,
        SET_DIRTY,
        DOWNLOAD,
        ImageRefresh
    }
}