﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resto2025
{
    public interface IBluetoothService
    {
        IList<string> GetDeviceList();
        Task Print(string deviceName, string text);
    }


}
