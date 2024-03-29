﻿using System;
using System.Collections.ObjectModel;

namespace WebRPC
{
    public class ApplicationRoute
    {
        public Type InterfaceType { get; set; }
        public ReadOnlyDictionary<string, MethodCache> Methods { get; set; }
    }
}
