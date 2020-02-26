// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class ExceptionHandler
    {
        public static List<Map> Maps;
        public static bool MapsParsed = false;

        public ExceptionHandler()
        {
            if (MapsParsed) return;

            Maps = new List<Map>();
            ParseMapFiles(Web.MapPath("~/App_Data/CodeMap.js"));
        }

        public string GetFriendlyMessageEntriesHTML(Exception e)
        {
			if(e == null) return "";
            var result = "";

            var friendlyMessageDTO = HandleException(e);
            foreach (var entry in friendlyMessageDTO.Entries)
                result += $"{entry.AppdevSemantic}: {entry.AppdevIdentifier}\r\n";

            return result;
        }

        public FriendlyMessageDTO HandleException(Exception e)
        {
            var exceptionHelpers = new List<ExceptionHelper>();
            var stackTrace = new StackTrace(e, true);
            var clientHelpers = new List<FriendlyMessageDTO>();
            var originalMaps = new List<Map>();
            var friendlyMessageDTO = new FriendlyMessageDTO
            {
                Entries = new List<FriendlyMessageEntryDTO>(),
                OriginalExceptionMessage = e.Message,
                OriginalStackTrace = e.StackTrace,
                ExceptionType = e.GetType().Name
            };


            if (e is System.Data.ConstraintException)
            {
                friendlyMessageDTO.Title = e.Message;
                /*if (e.Data != null && e.Data.Count > 0)
                {
                    foreach (System.Collections.DictionaryEntry dictionaryEntry in e.Data)
                    {
                        var friendlyMessageEntry = new FriendlyMessageEntryDTO($"{dictionaryEntry.Key} : {dictionaryEntry.Value}", AppDevSemantic.None.ToString());
                        friendlyMessageDTO.Entries.Add(friendlyMessageEntry);
                    }
                }*/
                return friendlyMessageDTO;
            }

            if(e is NHibernate.StaleObjectStateException)
            {
                friendlyMessageDTO.Title = e.Data["ZAPPDEV_TITLE"].ToString();
                friendlyMessageDTO.OriginalExceptionMessage = e.Data["ZAPPDEV_MESSAGE"].ToString();
            }

            if ((e.GetType().Name.ToLower() == "businessexception" || e.GetType().Name.ToLower() == "validationexception") && !string.IsNullOrWhiteSpace(e.Message))
            {
                string[] stringSeparators = new string[] { "\\r\\n" };
                string[] lines = e.Message.Split(stringSeparators, StringSplitOptions.None);

                foreach(var line in lines)
                {
                    if (line.Trim() == "") continue;
                    if(line == lines.First())
                    {
                        friendlyMessageDTO.Title = line;
                        continue;
                    }
                    var friendlyMessageEntry = new FriendlyMessageEntryDTO($"{line}", AppDevSemantic.None.ToString());
                    friendlyMessageDTO.Entries.Add(friendlyMessageEntry);
                }
                
                return friendlyMessageDTO;
            }

            foreach (var frame in stackTrace.GetFrames().Where(f => !string.IsNullOrEmpty(f.GetFileName())).Reverse())
            {
                var exceptionHelper = new ExceptionHelper(frame.GetFileName(), frame.GetFileLineNumber());
                exceptionHelpers.Add(exceptionHelper);
                originalMaps.AddRange(Maps.AsParallel().Where(m => m.SourceFilePath.Contains(exceptionHelper.filePath) &&
                    (m.Border.Start.Line <= exceptionHelper.lineNo && m.Border.End.Line >= exceptionHelper.lineNo)).ToList());
            }

            foreach (var map in originalMaps.Distinct())
            {
                var FriendlyMessageEntry = new FriendlyMessageEntryDTO(map.AppDevIdentifier, map.AppDevSemantic.ToString());

                friendlyMessageDTO.Entries.Add(FriendlyMessageEntry);
            }
           
            return friendlyMessageDTO;
        }

        public void ParseMapFiles(string filePath)
        {
            try
            {
                if (MapsParsed) return;

                var json = File.ReadAllText(filePath);

                Maps = JsonConvert.DeserializeObject<List<Map>>(json);
                MapsParsed = true;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(FriendlyMessageDTO)).Error("Could not parse Code Map Files!", e);
            }
        }

    }

    public class ExceptionHelper
    {
        public string filePath;
        public int lineNo;

        public ExceptionHelper(string path, int line)
        {
            filePath = path;
            lineNo = line;
        }
    }

    public class FriendlyMessageDTO
    {
        public string Title;
        public string OriginalStackTrace;
        public string OriginalExceptionMessage;
        public string ExceptionType;
        public List<FriendlyMessageEntryDTO> Entries;
    }

    public class FriendlyMessageEntryDTO
    {
        public string AppdevIdentifier;
        public string AppdevSemantic;

        public FriendlyMessageEntryDTO(string id, string semantic)
        {
            AppdevIdentifier = id;
            AppdevSemantic = semantic;
        }
    }

    public class Map
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AppDevSemantic AppDevSemantic;
        public string SourceFilePath;
        public string AppDevIdentifier;
        public int Nesting;
        public Location Border;
        public Map Parent;

        public Map(string absolutePath, string appDevSemanticString, string id, int nesting)
        {
            SourceFilePath = absolutePath;
            AppDevIdentifier = id;
            Nesting = nesting;

            if (Enum.TryParse(appDevSemanticString, out AppDevSemantic appDevSemanticEnum))
            {
                AppDevSemantic = appDevSemanticEnum;
            }
            else
            {
                throw new ApplicationException($"Could not parse value '{appDevSemanticString}' to AppDevSemantic Enum!");
            }
        }

        public Map() { }
    }

    public class Location
    {
        public FilePosition Start;
        public FilePosition End;

        public Location()
        {

        }
    }

    public class FilePosition
    {
        public int Line;
        public int Column = 0;

        public FilePosition()
        {

        }
    }

    public enum AppDevSemantic
    {
        None = 0,
        CalculatedExpression,
        CondionalFormating,
        ControllerAction,
        Form,
        Logic,
        ControllerActionEntry,
        ControllerActionImplementation,
        Datasource,
        DataSourceDataBinding,
        DataSourceGetFullRecordset,
        DataSourceEntryPoint,
        DataSourceFilter,
        DataSourceGroupBy,
        DataSourceDataAccess,
        DataSourceGrid,
        DataSourceGridEntry,
        DataSourceAggregators,
        DataValidation,
        DataValidationCondition,
        DataValidationMesage,
        ConditionalFormatting,
        ConditionalFormattingEvaluation,
        ControlOnChangeAction,
        ControllerEntryPoint,
        CalculatedExpressionValueMethod
    }
}