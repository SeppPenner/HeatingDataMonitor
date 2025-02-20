﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataHandler.Services
{
    public abstract class DataService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly DataStorage dataStorage;

        public DataService(DataStorage dataStorage) => this.dataStorage = dataStorage;

        private async Task LoopAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Data newData = null;
                try
                {
                    newData = await GetNewData(stoppingToken);
                }
                catch (Exceptions.NoDataReceivedException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.InnerException.Message);
                }
                catch (Exceptions.FaultyDataReceivedException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.FaultyData);
                }
                // maybe stops weird OperationCanceledException on shutdown (untested)
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled in LoopAsync of DataService");
                }

                // ignore this cycle if there was an error
                if (newData == null) continue;

                // update the current data
                dataStorage.CurrentData = newData;

                // signal that there is some new data
                dataStorage.OnNewDataReceived();
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await BeforeLoopStart();                    // some configuration work before the loop starts
            await base.StartAsync(cancellationToken);   // starts loop
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // blocks until loop ends or until the shutdown timer (default=5s) elapses
            await base.StopAsync(cancellationToken);    
            // cleanup after loop ends. You can't be sure if the thread has ended tho because maybe the application triggered the abort signal (timer elapsed)
            await CleanupOnApplicationShutdown();                               
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return LoopAsync(stoppingToken);
        }

        /// <summary>
        /// Retrieves <see cref="Data"/> from wherever. This method is expected to block until the next <see cref="Data"/> is ready - there is no artificial delay.
        /// </summary>
        /// <returns>The new <see cref="Data"/></returns>
        protected abstract Task<Data> GetNewData(CancellationToken cancellationToken);

        /// <summary>
        /// Override when there's configuration work to do before the service loop can start
        /// </summary>
        protected virtual Task BeforeLoopStart() => Task.CompletedTask;

        /// <summary>
        /// Override when there is cleanup to do after the loop ends. 
        /// There is no Guarantee that the loop already ended when this method is called.
        /// </summary>
        protected virtual Task CleanupOnApplicationShutdown() => Task.CompletedTask;
    }
}
