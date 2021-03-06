﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Data;
using log4net;

namespace zAppDev.DotNet.Framework.Threading
{
    public class ParallelTask
    {
        public static void ForEach<T>(IEnumerable<T> items, Action<T, long> iterationFunction)
        {
            var exceptions = new ConcurrentQueue<Exception>();
            var logger = LogManager.GetLogger(typeof(ParallelTask));
            var taskid = Guid.NewGuid();

            var outerWatch = new Stopwatch();
            outerWatch.Start();
            Parallel.ForEach(items, (item, state, idx) =>
            {
                var innerWatch = new Stopwatch();
                innerWatch.Start();
                MiniSessionManager.ExecuteInUoW(manager =>
                {
                    try
                    {
                        iterationFunction(item, idx);
                    }
                    catch (Exception e)
                    {
                        exceptions.Enqueue(e);
                    }
                });
                innerWatch.Stop();
                logger.Debug($"Parallel task item {idx}, took: {innerWatch.Elapsed}. ({taskid})");
            });
            outerWatch.Stop();
            logger.Debug($"OVERALL: Parallel ForEach Task took: {outerWatch.Elapsed}.");

            // Throw the exceptions here after the loop completes.
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }

    public class ParallelTask<T>
    {

    }
}