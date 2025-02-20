﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler
{
    public class DataStorage
    {
        public Data CurrentData { get; internal set; }
        public event Action NewDataReceived;

        public void OnNewDataReceived()
        {
            var temp = NewDataReceived;
            temp?.Invoke();
        }
    }
}
